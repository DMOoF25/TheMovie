namespace TheMovie.Application.Abstractions;

/// <summary>
/// Generic CRUD repository abstraction (non-persistent / in-memory implementation expected).
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepository<TEntity>
    where TEntity : class
{
    #region Create operations
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    #endregion

    #region Read operations
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Update operations
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    #endregion

    #region Delete operations
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    #endregion

}
