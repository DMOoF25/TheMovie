using System.Collections.Concurrent;
using System.Reflection;
using TheMovie.Application.Abstractions;

namespace TheMovie.Infrastructure.Data;

/// <summary>
/// Thread-safe in-memory repository base (no persistence).
/// Auto-detects public Guid Id property or allows custom id selector/setter injection.
/// </summary>
public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly ConcurrentDictionary<Guid, TEntity> _store = new();

    private readonly Func<TEntity, Guid> _getId;
    private readonly Action<TEntity, Guid>? _setId;

    protected RepositoryBase(
        Func<TEntity, Guid>? idSelector = null,
        Action<TEntity, Guid>? idSetter = null)
    {
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
    }

    // CREATE
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
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
        return Task.CompletedTask;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var e in entities)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddAsync(e, cancellationToken).ConfigureAwait(false);
        }
    }

    // READ
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<TEntity> snapshot = _store.Values.ToList();
        return Task.FromResult(snapshot);
    }

    // UPDATE
    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var id = _getId(entity);
        if (id == Guid.Empty)
            throw new InvalidOperationException("Cannot update entity with empty Id.");

        if (!_store.ContainsKey(id))
            throw new KeyNotFoundException($"Entity {typeof(TEntity).Name} with Id '{id}' not found.");

        _store[id] = entity;
        return Task.CompletedTask;
    }

    // DELETE
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    // Optional protected access for derived repositories (custom querying).
    protected IReadOnlyCollection<TEntity> Items => (IReadOnlyCollection<TEntity>)_store.Values;
}