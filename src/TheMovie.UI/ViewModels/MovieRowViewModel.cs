using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheMovie.Domain.Entities;

namespace TheMovie.UI.ViewModels;

public sealed class MovieRowViewModel : INotifyPropertyChanged
{
    private string _title;
    private int _duration;
    private DateOnly _premiereDate;
    private string _genresDisplay;

    public Guid Id { get; }
    public Movie Source { get; }

    public string Title
    {
        get => _title;
        private set { if (_title == value) return; _title = value; OnPropertyChanged(); }
    }

    public int Duration
    {
        get => _duration;
        private set { if (_duration == value) return; _duration = value; OnPropertyChanged(); }
    }

    public DateOnly PremiereDate
    {
        get => _premiereDate;
        private set { if (_premiereDate == value) return; _premiereDate = value; OnPropertyChanged(); }
    }

    public string GenresDisplay
    {
        get => _genresDisplay;
        private set { if (_genresDisplay == value) return; _genresDisplay = value; OnPropertyChanged(); }
    }

    public MovieRowViewModel(Movie movie)
    {
        Source = movie;
        Id = movie.Id;
        _title = movie.Title;
        _duration = movie.Duration;
        _premiereDate = movie.PremiereDate;
        _genresDisplay = movie.Genres == Domain.ValueObjects.Genre.None
            ? "-"
            : movie.Genres.ToString().Replace(", ", " | ");
    }

    public void RefreshFromSource()
    {
        Title = Source.Title;
        Duration = Source.Duration;
        PremiereDate = Source.PremiereDate;
        GenresDisplay = Source.Genres == Domain.ValueObjects.Genre.None
            ? "-"
            : Source.Genres.ToString().Replace(", ", " | ");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}