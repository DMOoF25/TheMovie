using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.UI.Tests.Fakes;

internal sealed class FakeMovieRepository : IMovieRepository
{
    private readonly List<Movie> _added = new();
    public IReadOnlyList<Movie> Added => _added;

    public Task AddAsync(Movie entity, CancellationToken cancellationToken = default)
    {
        _added.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<Movie> entities, CancellationToken cancellationToken = default)
    {
        _added.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<Movie?>(_added.FirstOrDefault(m => m.Id == id));

    public Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<Movie>>(_added.ToList());

    public Task UpdateAsync(Movie entity, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var idx = _added.FindIndex(m => m.Id == id);
        if (idx >= 0) _added.RemoveAt(idx);
        return Task.CompletedTask;
    }
}