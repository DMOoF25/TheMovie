using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Domain.Entities;

namespace TheMovie.UI.ViewModels;

public class ScreeningsListItemViewModel
{
    private static readonly ConcurrentDictionary<Guid, string> _cinameNameCache = new();
    private static readonly ConcurrentDictionary<Guid, string> _hallNameCache = new();
    private static readonly ConcurrentDictionary<Guid, Guid> _hallToCinemaCache = new();
    private static readonly ConcurrentDictionary<Guid, string> _movieTitleCache = new();
    private static readonly ConcurrentDictionary<Guid, int> _movieDurationCache = new();

    public Guid Id { get; }
    public string CinemaNameDisplay { get; }
    public string HallNameDisplay { get; }
    public string MovieTitleDisplay { get; }
    public string DateDisplay { get; }
    public TimeOnly StartTimeDisplay { get; }
    public TimeOnly EndTimeDisplay { get; }

    public ScreeningsListItemViewModel(Screening screening)
    {
        Id = screening.Id;
        DateDisplay = screening.StartTime.ToString("dd/MM/yyyy");
        StartTimeDisplay = TimeOnly.FromDateTime(screening.StartTime);

        // Resolve Hall name + CinemaId
        Guid cinemaId = Guid.Empty;
        if (screening.HallId != Guid.Empty)
        {
            if (!_hallNameCache.TryGetValue(screening.HallId, out var hallName)
                || !_hallToCinemaCache.TryGetValue(screening.HallId, out cinemaId))
            {
                try
                {
                    var hallRepo = App.HostInstance.Services.GetRequiredService<Application.Abstractions.IHallRepository>();
                    var hall = hallRepo.GetByIdAsync(screening.HallId).GetAwaiter().GetResult();
                    hallName = hall?.Name ?? string.Empty;
                    cinemaId = hall?.CinemaId ?? Guid.Empty;
                }
                catch
                {
                    hallName = string.Empty;
                    cinemaId = Guid.Empty;
                }
                if (!string.IsNullOrEmpty(hallName))
                    _hallNameCache[screening.HallId] = hallName;
                if (cinemaId != Guid.Empty)
                    _hallToCinemaCache[screening.HallId] = cinemaId;
            }
            HallNameDisplay = hallName ?? string.Empty;
        }
        else
        {
            HallNameDisplay = "Vælg en biograf";
        }

        // Resolve Movie title + duration (for EndTime)
        int duration = 0;
        if (screening.MovieId != Guid.Empty)
        {
            if (!_movieTitleCache.TryGetValue(screening.MovieId, out var title)
                || !_movieDurationCache.TryGetValue(screening.MovieId, out duration))
            {
                try
                {
                    var movieRepo = App.HostInstance.Services.GetRequiredService<Application.Abstractions.IMovieRepository>();
                    var movie = movieRepo.GetByIdAsync(screening.MovieId).GetAwaiter().GetResult();
                    title = movie?.Title ?? string.Empty;
                    duration = movie?.Duration ?? 0;
                }
                catch
                {
                    title = string.Empty;
                    duration = 0;
                }
                if (!string.IsNullOrEmpty(title))
                    _movieTitleCache[screening.MovieId] = title;
                _movieDurationCache[screening.MovieId] = duration;
            }
            MovieTitleDisplay = title ?? string.Empty;
        }
        else
        {
            MovieTitleDisplay = string.Empty;
        }

        EndTimeDisplay = StartTimeDisplay.AddMinutes(duration);

        // Resolve Cinema name using cinemaId from hall mapping
        if (cinemaId != Guid.Empty)
        {
            if (!_cinameNameCache.TryGetValue(cinemaId, out var cinemaName))
            {
                try
                {
                    var cinemaRepo = App.HostInstance.Services.GetRequiredService<Application.Abstractions.ICinemaRepository>();
                    var cinema = cinemaRepo.GetByIdAsync(cinemaId).GetAwaiter().GetResult();
                    cinemaName = cinema?.Name ?? string.Empty;
                }
                catch
                {
                    cinemaName = string.Empty;
                }
                if (!string.IsNullOrEmpty(cinemaName))
                    _cinameNameCache[cinemaId] = cinemaName;
            }
            CinemaNameDisplay = cinemaName ?? string.Empty;
        }
        else
        {
            CinemaNameDisplay = string.Empty;
        }
    }
}