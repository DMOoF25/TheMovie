using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.Commands;
using TheMovie.UI.Views;

namespace TheMovie.UI.ViewModels;

public sealed class MainPageViewModel : INotifyPropertyChanged
{
    // Lightweight option model to avoid coupling to other VMs
    public sealed record Option(Guid Id, string Label);

    private readonly IScreeningRepository _screeningRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ICinemaRepository _cinemaRepository;
    private readonly IHallRepository _hallRepository;

    // Dictionaries for quick lookups
    private readonly Dictionary<Guid, Cinema> _cinemas = new();
    private readonly Dictionary<Guid, Hall> _halls = new();
    private readonly Dictionary<Guid, Movie> _movies = new();
    private readonly List<Screening> _allScreenings = new();

    public ObservableCollection<Option> Cinemas { get; } = new();
    public ObservableCollection<Option> Halls { get; } = new();
    public ObservableCollection<Option> Movies { get; } = new();

    public ObservableCollection<ScreeningDisplayItem> Screenings { get; } = new();

    // Selected filters
    private Guid? _selectedCinemaId;
    public Guid? SelectedCinemaId
    {
        get => _selectedCinemaId;
        set
        {
            if (_selectedCinemaId == value) return;
            _selectedCinemaId = value;
            OnPropertyChanged();
            FilterHalls();
        }
    }

    private Guid? _selectedHallId;
    public Guid? SelectedHallId
    {
        get => _selectedHallId;
        set { if (_selectedHallId == value) return; _selectedHallId = value; OnPropertyChanged(); }
    }

    private Guid? _selectedMovieId;
    public Guid? SelectedMovieId
    {
        get => _selectedMovieId;
        set { if (_selectedMovieId == value) return; _selectedMovieId = value; OnPropertyChanged(); }
    }

    private DateTime? _selectedDate;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set { if (_selectedDate == value) return; _selectedDate = value; OnPropertyChanged(); }
    }

    // Selection
    private ScreeningDisplayItem? _selectedScreening;
    public ScreeningDisplayItem? SelectedScreening
    {
        get => _selectedScreening;
        set { if (_selectedScreening == value) return; _selectedScreening = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanOpenBooking)); }
    }

    public bool CanOpenBooking => SelectedScreening is not null;

    // Commands
    public ICommand SearchCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand OpenBookingCommand { get; }
    public ICommand RefreshCommand { get; }

    private string? _error;
    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); }
    }

    public MainPageViewModel()
    {
        // Resolve repositories from the host
        _screeningRepository = App.HostInstance.Services.GetRequiredService<IScreeningRepository>();
        _movieRepository = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        _cinemaRepository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();
        _hallRepository = App.HostInstance.Services.GetRequiredService<IHallRepository>();

        // Default date filter to today
        SelectedDate ??= DateTime.Today;

        SearchCommand = new RelayCommand(ApplyFilters);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        OpenBookingCommand = new RelayCommand(OpenBooking, () => CanOpenBooking);
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
    }

    public async Task RefreshAsync()
    {
        Error = null;

        try
        {
            // Load reference data
            _cinemas.Clear(); _halls.Clear(); _movies.Clear(); _allScreenings.Clear();

            foreach (var c in await _cinemaRepository.GetAllAsync()) _cinemas[c.Id] = c;
            foreach (var h in await _hallRepository.GetAllAsync()) _halls[h.Id] = h;
            foreach (var m in await _movieRepository.GetAllAsync()) _movies[m.Id] = m;
            _allScreenings.AddRange(await _screeningRepository.GetAllAsync());

            // Populate filter options
            Cinemas.Clear();
            foreach (var c in _cinemas.Values.OrderBy(c => c.Name))
                Cinemas.Add(new Option(c.Id, c.Name));

            Movies.Clear();
            foreach (var m in _movies.Values.OrderBy(m => m.Title))
                Movies.Add(new Option(m.Id, m.Title));

            FilterHalls();

            // Populate list
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            (OpenBookingCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    private void FilterHalls()
    {
        Halls.Clear();

        IEnumerable<Hall> halls = _halls.Values;
        if (SelectedCinemaId is Guid cid)
            halls = halls.Where(h => h.CinemaId == cid);

        foreach (var h in halls.OrderBy(h => h.Name))
            Halls.Add(new Option(h.Id, h.Name));
    }

    private void ClearFilters()
    {
        SelectedCinemaId = null;
        SelectedHallId = null;
        SelectedMovieId = null;
        // Keep default date filter on today when clearing
        SelectedDate = DateTime.Today;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allScreenings.AsEnumerable();

        if (SelectedMovieId is Guid mid)
            filtered = filtered.Where(s => s.MovieId == mid);
        if (SelectedCinemaId is Guid cid)
            filtered = filtered.Where(s => _halls.TryGetValue(s.HallId, out var hall) && hall.CinemaId == cid);

        if (SelectedDate is DateTime date)
        {
            var d = date.Date;
            filtered = filtered.Where(s => s.StartTime.Date >= d);
        }

        var items = filtered
            .Select(s => ToDisplayItem(s))
            .OrderBy(i => i.CinemaName)
            .ThenBy(i => i.HallName)
            .ThenBy(i => i.StartTimeRaw)
            .ToList();

        Screenings.Clear();
        foreach (var i in items) Screenings.Add(i);

        (OpenBookingCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private ScreeningDisplayItem ToDisplayItem(Screening s)
    {
        var movie = _movies.TryGetValue(s.MovieId, out var m) ? m : null;
        var hall = _halls.TryGetValue(s.HallId, out var h) ? h : null;
        var cinema = hall is not null && _cinemas.TryGetValue(hall.CinemaId, out var c) ? c : null;

        return new ScreeningDisplayItem(
            s.Id,
            cinema?.Name ?? "(Unknown)",
            hall?.Name ?? "(Unknown)",
            movie?.Title ?? "(Unknown)",
            s.StartTime,
            s.MovieId,
            s.HallId,
            cinema?.Id
        );
    }

    private void OpenBooking()
    {
        if (SelectedScreening is null) return;

        // Try to resolve BookingWindow from DI, fallback to direct creation
        var window = App.HostInstance.Services.GetService<EditBookingWindow>() ?? new EditBookingWindow();
        // Optional: pass screening Id via Tag (non-disruptive if window ignores it)
        window.Tag = SelectedScreening.Id;
        window.Owner = System.Windows.Application.Current?.MainWindow;
        window.Show();
    }

    public sealed class ScreeningDisplayItem
    {
        public Guid Id { get; }
        public string CinemaName { get; }
        public string HallName { get; }
        public string MovieTitle { get; }
        public string StartTime { get; }
        public DateTime StartTimeRaw { get; }
        public Guid MovieId { get; }
        public Guid HallId { get; }
        public Guid? CinemaId { get; }

        public ScreeningDisplayItem(Guid id, string cinemaName, string hallName, string movieTitle, DateTime start, Guid movieId, Guid hallId, Guid? cinemaId)
        {
            Id = id;
            CinemaName = cinemaName;
            HallName = hallName;
            MovieTitle = movieTitle;
            StartTimeRaw = start;
            StartTime = start.ToString("dd-MM-yyyy HH:mm");
            MovieId = movieId;
            HallId = hallId;
            CinemaId = cinemaId;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}