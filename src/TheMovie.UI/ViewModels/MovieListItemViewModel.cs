using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;

namespace TheMovie.UI.ViewModels;

public sealed class MovieListItemViewModel
{
    private static readonly ConcurrentDictionary<Guid, string> _instructorNameCache = new();

    public Guid Id { get; }
    public string Title { get; }
    public int Duration { get; }
    public DateOnly PremiereDate { get; }
    public string GenresDisplay { get; }
    public string InstructorNameDisplay { get; }

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

        // Resolve instructor name (cached lookups to avoid repeated repository calls)
        if (movie.InstructorId != Guid.Empty)
        {
            if (!_instructorNameCache.TryGetValue(movie.InstructorId, out var name))
            {
                try
                {
                    var repo = App.HostInstance.Services.GetRequiredService<IInstructorRepository>();
                    var instructor = repo.GetByIdAsync(movie.InstructorId).GetAwaiter().GetResult();
                    name = instructor?.Name ?? string.Empty;
                }
                catch
                {
                    name = string.Empty;
                }
                if (!string.IsNullOrEmpty(name))
                    _instructorNameCache[movie.InstructorId] = name;
            }
            InstructorNameDisplay = name ?? string.Empty;
        }
        else
        {
            InstructorNameDisplay = string.Empty;
        }
    }
}