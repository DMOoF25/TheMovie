using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;

namespace TheMovie.UI.ViewModels;

public sealed class MovieListItemViewModel
{
    public Guid Id { get; }
    public string Title { get; }
    public int Duration { get; }
    public DateOnly PremiereDate { get; }
    public string GenresDisplay { get; }

    public MovieListItemViewModel(Movie movie)
    {
        Id = movie.Id;
        Title = movie.Title;
        Duration = movie.Duration;
        PremiereDate = movie.PremiereDate;
        GenresDisplay = movie.Genres == Genre.None
            ? nameof(Genre.None)
            : string.Join(", ", Enum.GetValues<Genre>()
                                    .Where(g => g != Genre.None && movie.Genres.HasFlag(g)));
    }
}