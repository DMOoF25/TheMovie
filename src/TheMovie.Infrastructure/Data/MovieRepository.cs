using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.Infrastructure.Data;

/// <summary>
/// In-memory Movie repository with simple genre filtering helpers.
/// </summary>
public sealed class MovieRepository : RepositoryBase<Movie>, IMovieRepository
{
    public MovieRepository() : base() { }

    /*
    // Movies that include (at least) the specified single genre flag.
    public Task<IEnumerable<Movie>> ListByGenreAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        var result = Items.Where(m => (m.Genres & genre) == genre).ToList();
        return Task.FromResult<IEnumerable<Movie>>(result);
    }

    // Movies that contain ANY of the provided genre flags.
    public Task<IEnumerable<Movie>> ListByAnyGenresAsync(Genre genres, CancellationToken cancellationToken = default)
    {
        var result = Items.Where(m => (m.Genres & genres) != Genre.None).ToList();
        return Task.FromResult<IEnumerable<Movie>>(result);
    }

    // Movies that contain ALL of the provided genre flags.
    public Task<IEnumerable<Movie>> ListByAllGenresAsync(Genre genres, CancellationToken cancellationToken = default)
    {
        var result = Items.Where(m => (m.Genres & genres) == genres).ToList();
        return Task.FromResult<IEnumerable<Movie>>(result);
    }
    */
}
