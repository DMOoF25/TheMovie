namespace TheMovie.Application.Abstractions;

/// <summary>
/// Defines a generic asynchronous CRUD repository abstraction.
/// Implementations may be purely in-memory or provide persistence.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
public interface IRepositoryBase<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Performs any asynchronous initialization required before normal repository use
    /// (e.g., loading persisted data into memory). Safe to call multiple times (should be idempotent).
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    //Task InitializeAsync(CancellationToken cancellationToken = default);

    #region Create operations

    /// <summary>
    /// Adds a single entity to the repository. Generates a new identifier if necessary.
    /// </summary>
    /// <param name="entity">Entity instance to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the repository sequentially.
    /// </summary>
    /// <param name="entities">Entities to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    #endregion

    #region Read operations

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The matching entity, or <c>null</c> if not found.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities as a snapshot enumeration.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>All current entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Update operations

    /// <summary>
    /// Replaces the stored entity matching the identifier contained in <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">Entity containing updated state.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    #endregion

    #region Delete operations

    /// <summary>
    /// Removes an entity by identifier. No-op if the entity does not exist.
    /// </summary>
    /// <param name="id">Identifier of the entity to remove.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Filehandler

    /// <summary>
    /// Persists the current repository contents to the specified CSV file.
    /// NOTE: The name uses 'Cvs' instead of 'Csv' (typo retained for backward compatibility).
    /// </summary>
    /// <param name="filePath">Destination file path.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <remarks>
    /// Declared here for implementations that expose manual persistence control.
    /// </remarks>
    //protected Task SaveToCvsAsync(string filePath, CancellationToken cancellationToken = default);

    ///// <summary>
    ///// Loads repository contents from the specified CSV file, merging / overwriting existing entries per implementation rules.
    ///// NOTE: The name uses 'Cvs' instead of 'Csv' (typo retained for backward compatibility).
    ///// </summary>
    ///// <param name="filePath">Source file path.</param>
    ///// <param name="cancellationToken">Token to observe for cancellation.</param>
    //Task LoadFromCvsAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion
}
