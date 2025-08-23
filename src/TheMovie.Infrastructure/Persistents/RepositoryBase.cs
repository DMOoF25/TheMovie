using System.Collections.Concurrent;
using System.Reflection;
using TheMovie.Application.Abstractions;

namespace TheMovie.Infrastructure.Persistents;

public abstract class RepositoryBase<TEntity> : IRepositoryBase<TEntity>
    where TEntity : class
{
    protected readonly SemaphoreSlim _ioLock = new(1, 1);

    /// <summary>
    /// Fully-qualified path to the CSV persistence file for this entity type.
    /// Format: <c>%LOCALAPPDATA%\TheMovie\{entityname}s.csv</c>.
    /// </summary>
    protected string _filePath;

    /// <summary>
    /// In-memory concurrent store keyed by entity <see cref="Guid"/>.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, TEntity> _store = new();

    /// <summary>
    /// Delegate used to retrieve the <see cref="Guid"/> identifier from an entity instance.
    /// </summary>
    private readonly Func<TEntity, Guid> _getId;

    /// <summary>
    /// Delegate used to assign a new <see cref="Guid"/> identifier to an entity instance (optional).
    /// </summary>
    private readonly Action<TEntity, Guid>? _setId;

    /// <summary>
    /// Initializes the repository, configuring identifier accessors and the persistence file path.
    /// If <paramref name="idSelector"/> or <paramref name="idSetter"/> is omitted, the constructor
    /// reflects over <typeparamref name="TEntity"/> to locate a public mutable <c>Guid Id</c> property.
    /// Also ensures the target directory exists and immediately attempts to load existing CSV data.
    /// </summary>
    /// <param name="idSelector">Function that extracts the <see cref="Guid"/> identifier from an entity.</param>
    /// <param name="idSetter">Action that can assign a new <see cref="Guid"/> to an entity.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <paramref name="idSelector"/> is provided and no suitable <c>Id</c> property is found.
    /// </exception>
    protected RepositoryBase(
        Func<TEntity, Guid>? idSelector = null,
        Action<TEntity, Guid>? idSetter = null
    )
    {
        _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TheMovie",
            $"{typeof(TEntity).Name.ToLowerInvariant()}s.csv");

        if (idSelector is null || idSetter is null)
        {
            var prop = typeof(TEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
            if (prop is not null &&
                prop.CanRead &&
                prop.CanWrite &&
                prop.PropertyType == typeof(Guid))
            {
                idSelector ??= e => (Guid)(prop.GetValue(e) ?? Guid.Empty);
                idSetter ??= (e, v) => prop.SetValue(e, v);
            }
        }

        _getId = idSelector ?? throw new InvalidOperationException(
            $"No id selector provided and no public Guid Id property found on {typeof(TEntity).Name}.");
        _setId = idSetter;

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        // Comment this line out when testing to avoid loading data
        LoadFromCvsAsync(_filePath).ConfigureAwait(false);
    }

    #region CRUD Operations

    /// <summary>
    /// Adds a new entity to the repository. If the entity's identifier is empty and
    /// a setter is available, a new <see cref="Guid"/> is generated and assigned.
    /// Persists the repository state to the CSV file after insertion.
    /// </summary>
    /// <param name="entity">The entity instance to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the entity has an empty Id and no setter delegate is available.
    /// </exception>
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var id = _getId(entity);
        if (id == Guid.Empty)
        {
            if (_setId is null)
                throw new InvalidOperationException("Entity Id is empty and no Id setter available.");
            id = Guid.NewGuid();
            _setId(entity, id);
        }

        _store[id] = entity;
        await SaveToCvsAsync(_filePath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds multiple entities to the repository sequentially, invoking <see cref="AddAsync(TEntity, CancellationToken)"/> for each.
    /// </summary>
    /// <param name="entities">Collection of entities to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var e in entities)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddAsync(e, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">Entity identifier to locate.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The entity if present; otherwise <c>null</c>.</returns>
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.TryGetValue(id, out var entity);
        return await Task.FromResult(entity);
    }

    /// <summary>
    /// Returns a snapshot enumeration of all entities currently stored.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>Snapshot list of all entities.</returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<TEntity> snapshot = _store.Values.ToList();
        return await Task.FromResult(snapshot);
    }

    /// <summary>
    /// Updates an existing entity. The entity's identifier must be non-empty and already exist in the store.
    /// Persists the updated state to the CSV file afterward.
    /// </summary>
    /// <param name="entity">Entity instance containing updated values.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="InvalidOperationException">Thrown if the entity's Id is empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no entity with the given Id exists.</exception>
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var id = _getId(entity);
        if (id == Guid.Empty)
            throw new InvalidOperationException("Cannot update entity with empty Id.");

        if (!_store.ContainsKey(id))
            throw new KeyNotFoundException($"Entity {typeof(TEntity).Name} with Id '{id}' not found.");

        _store[id] = entity;
        await SaveToCvsAsync(_filePath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes an entity by its identifier if present and persists the modified state.
    /// </summary>
    /// <param name="id">Identifier of the entity to remove.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.TryRemove(id, out _);
        await SaveToCvsAsync(_filePath, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region File Persistence

    /// <summary>
    /// Persists repository contents to the specified CSV file.
    /// Implementations must be thread-safe relative to other persistence operations.
    /// </summary>
    /// <param name="filePath">Destination CSV file path.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    protected abstract Task SaveToCvsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads repository contents from the specified CSV file into memory.
    /// Implementations may overwrite or merge existing entries as appropriate.
    /// </summary>
    /// <param name="filePath">Source CSV file path.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    protected abstract Task LoadFromCvsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with retry semantics and returns a result.
    /// Retries only on <see cref="IOException"/> (commonly for transient file sharing violations).
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="action">Operation to execute.</param>
    /// <param name="attempts">Maximum number of attempts (default 5).</param>
    /// <param name="initialDelayMs">Initial backoff delay in milliseconds (exponential backoff applied).</param>
    /// <returns>Result of the successful operation.</returns>
    /// <exception cref="IOException">Propagated if all retry attempts fail.</exception>
    protected static async Task<T> WithRetriesAsync<T>(Func<Task<T>> action, int attempts = 5, int initialDelayMs = 40)
    {
        var delay = initialDelayMs;
        for (int i = 0; ; i++)
        {
            try { return await action().ConfigureAwait(false); }
            catch (IOException) when (i < attempts - 1)
            {
                await Task.Delay(delay).ConfigureAwait(false);
                delay *= 2;
            }
        }
    }

    /// <summary>
    /// Executes an asynchronous operation with retry semantics (non-result variant).
    /// Retries only on <see cref="IOException"/>.
    /// </summary>
    /// <param name="action">Operation to execute.</param>
    /// <param name="attempts">Maximum number of attempts.</param>
    /// <param name="initialDelayMs">Initial backoff delay in milliseconds.</param>
    /// <returns>A task that completes when the operation succeeds or final attempt fails.</returns>
    protected static async Task WithRetriesAsync(Func<Task> action, int attempts = 5, int initialDelayMs = 40)
    {
        await WithRetriesAsync(async () => { await action().ConfigureAwait(false); return 0; }, attempts, initialDelayMs).ConfigureAwait(false);
    }
    #endregion

    /// <summary>
    /// Provides derived classes with read-only access to the current entity collection.
    /// </summary>
    protected IReadOnlyCollection<TEntity> Items => (IReadOnlyCollection<TEntity>)_store.Values;

    /// <summary>
    /// Inserts or replaces an entity in the in-memory store without triggering persistence.
    /// Generates a new identifier if the current one is empty and a setter is available.
    /// </summary>
    /// <param name="entity">Entity to upsert.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the entity identifier is empty and no setter delegate is provided.
    /// </exception>
    protected void UpsertInMemory(TEntity entity)
    {
        var id = _getId(entity);
        if (id == Guid.Empty)
        {
            if (_setId is null)
                throw new InvalidOperationException("Entity Id is empty and no Id setter available.");
            id = Guid.NewGuid();
            _setId(entity, id);
        }
        _store[id] = entity;
    }

    /// <summary>
    /// Asynchronously (re)loads repository contents from the configured persistence file path.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await LoadFromCvsAsync(_filePath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Escapes a string for rudimentary CSV output by quoting values containing a comma or double quote
    /// and doubling embedded quotes per RFC 4180 guidelines.
    /// </summary>
    /// <param name="value">Raw field value.</param>
    /// <returns>Escaped CSV field value.</returns>
    protected static string Escape(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}