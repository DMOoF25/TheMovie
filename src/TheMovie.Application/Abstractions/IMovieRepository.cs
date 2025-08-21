using TheMovie.Domain.Entities;

namespace TheMovie.Application.Abstractions;

public interface IMovieRepository : IRepositoryBase<Movie>
{
    /*
    Task<IEnumerable<Movie>> ListByGenreAsync(Genre genre, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> ListByAnyGenresAsync(Genre genres, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> ListByAllGenresAsync(Genre genres, CancellationToken cancellationToken = default);
    */
}
