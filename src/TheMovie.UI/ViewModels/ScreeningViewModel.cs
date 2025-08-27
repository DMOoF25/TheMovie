using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class ScreeningViewModel : ViewModelBase<IScreeningRepository, Screening>
{
    private readonly IMovieRepository _movieRepository;
    private readonly ICinemaRepository _cinemaRepository;
    private readonly IHallRepository _hallRepository;

    private Guid? _currentId;

    private Guid? _selectedCinemaId;
    public Guid? SelectedCinemaId
    {
        get => _selectedCinemaId;
        set
        {
            if (_selectedCinemaId != value)
            {
                _selectedCinemaId = value;
                OnPropertyChanged();
                FilterHalls();
                SelectedHallId = null;
                RefreshCommandStates();
            }
        }
    }

    private Guid? _selectedMovieId;
    public Guid? SelectedMovieId
    {
        get => _selectedMovieId;
        set
        {
            if (_selectedMovieId != value)
            {
                _selectedMovieId = value;
                OnPropertyChanged();
                RefreshCommandStates();
            }
        }
    }

    private Guid? _selectedHallId;
    public Guid? SelectedHallId
    {
        get => _selectedHallId;
        set
        {
            if (_selectedHallId != value)
            {
                _selectedHallId = value;
                OnPropertyChanged();
                RefreshCommandStates();
            }
        }
    }

    private DateTime? _selectedDate;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate != value)
            {
                _selectedDate = value;
                OnPropertyChanged();
                RefreshCommandStates();
            }
        }
    }

    private string _startTimeString = "12:00";
    public string StartTimeString
    {
        get => _startTimeString;
        set
        {
            if (_startTimeString != value)
            {
                _startTimeString = value;
                OnPropertyChanged();
                RefreshCommandStates();
            }
        }
    }

    public ObservableCollection<CinemaListItemViewModel> Cinemas { get; } = new();
    public ObservableCollection<HallListItemViewModel> Halls { get; } = new();
    public ObservableCollection<MovieListItemViewModel> Movies { get; } = new();

    private readonly List<HallListItemViewModel> _allHalls = new();

    public event EventHandler<Screening>? ScreeningSaved;

    public ScreeningViewModel(ScreeningListItemViewModel? selected = default)
        : base(App.HostInstance.Services.GetRequiredService<IScreeningRepository>())
    {
        _movieRepository = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        _cinemaRepository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();
        _hallRepository = App.HostInstance.Services.GetRequiredService<IHallRepository>();

        _ = LoadMoviesAsync();
        _ = LoadCinemasAsync();
        _ = LoadHallsAsync();
    }

    #region Load method
    public override async Task LoadAsync(Guid screeningId)
    {
        Error = null;
        var entity = await _repository.GetByIdAsync(screeningId);
        if (entity is null)
        {
            Error = "Kunne ikke indlæse forestillingen.";
            return;
        }
        _currentId = entity.Id;

        // Resolve hall to get cinema for filtering
        var hall = await _hallRepository.GetByIdAsync(entity.HallId);
        SelectedCinemaId = hall?.CinemaId;
        // FilterHalls() runs on SelectedCinemaId setter

        SelectedHallId = entity.HallId;
        SelectedMovieId = entity.MovieId;
        SelectedDate = entity.StartTime.Date;
        StartTimeString = entity.StartTime.ToString("HH:mm");

        IsEditMode = true;
        Error = null;
    }

    private async Task LoadCinemasAsync()
    {
        try
        {
            var all = (await _cinemaRepository.GetAllAsync()).ToList();
            Cinemas.Clear();
            foreach (var item in all)
                Cinemas.Add(new CinemaListItemViewModel(item));
            OnPropertyChanged(nameof(Cinemas));
        }
        catch { }
    }

    private async Task LoadHallsAsync()
    {
        try
        {
            var all = (await _hallRepository.GetAllAsync()).ToList();
            _allHalls.Clear();
            foreach (var item in all)
                _allHalls.Add(new HallListItemViewModel(item));
            FilterHalls();
        }
        catch { }
    }

    private void FilterHalls()
    {
        Halls.Clear();
        foreach (var h in _allHalls.Where(h => !SelectedCinemaId.HasValue || h.CinemaId == SelectedCinemaId.Value))
            Halls.Add(h);
        OnPropertyChanged(nameof(Halls));
    }

    private async Task LoadMoviesAsync()
    {
        try
        {
            var all = (await _movieRepository.GetAllAsync()).ToList();
            Movies.Clear();
            foreach (var item in all)
                Movies.Add(new MovieListItemViewModel(item));
            OnPropertyChanged(nameof(Movies));
        }
        catch { }
    }
    #endregion

    #region CanXXX methods
    protected override bool CanSubmitCore() =>
        !IsSaving &&
        SelectedCinemaId.HasValue &&
        SelectedMovieId.HasValue &&
        SelectedHallId.HasValue &&
        SelectedDate.HasValue &&
        TryParseTime(StartTimeString, out _);

    #endregion

    #region Helpers
    private static bool TryParseTime(string input, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(input)) return false;
        return TimeSpan.TryParse(input, out time) &&
               time >= TimeSpan.Zero && time < TimeSpan.FromDays(1);
    }

    private async Task<(bool Ok, string? Message)> ValidateScheduleAsync(Guid? excludeId, Guid hallId, Guid movieId, DateTime proposedStart)
    {
        // Compute proposed end based on movie duration
        var movie = await _movieRepository.GetByIdAsync(movieId);
        if (movie is null) return (false, "Ugyldig film.");
        var proposedEnd = proposedStart.AddMinutes(movie.Duration);

        // Get all screenings in the same hall
        var all = await _repository.GetAllAsync();
        var sameHall = all.Where(s => s.HallId == hallId && (!excludeId.HasValue || s.Id != excludeId.Value));

        foreach (var s in sameHall)
        {
            var sMovie = await _movieRepository.GetByIdAsync(s.MovieId);
            var sEnd = s.StartTime.AddMinutes(sMovie?.Duration ?? 0);

            // Enforce 30-minute gap
            var gap = TimeSpan.FromMinutes(30);
            var sStartWithPadding = s.StartTime - gap;
            var sEndWithPadding = sEnd + gap;

            // If proposed [start,end] intersects [sStart-gap, sEnd+gap], it's invalid
            bool overlap = proposedStart < sEndWithPadding && proposedEnd > sStartWithPadding;
            if (overlap)
            {
                var msg = $"Konflikt i sal: eksisterende visning {s.StartTime:dd-MM-yyyy HH:mm} - {sEnd:HH:mm} (kræver 30 min. pause).";
                return (false, msg);
            }
        }

        return (true, null);
    }

    private DateTime BuildStartDateTime()
    {
        _ = TryParseTime(StartTimeString, out var t);
        var date = SelectedDate!.Value.Date;
        return new DateTime(date.Year, date.Month, date.Day, t.Hours, t.Minutes, 0, DateTimeKind.Local);
    }
    #endregion

    #region Command Handlers
    protected override async Task OnAddAsync()
    {
        if (!CanAdd()) return;
        IsSaving = true;
        Error = null;

        var start = BuildStartDateTime();
        var hallId = SelectedHallId!.Value;
        var movieId = SelectedMovieId!.Value;

        var (ok, msg) = await ValidateScheduleAsync(null, hallId, movieId, start);
        if (!ok)
        {
            Error = msg;
            IsSaving = false;
            return;
        }

        var screening = new Screening
        {
            MovieId = movieId,
            HallId = hallId,
            StartTime = start
        };

        try
        {
            await _repository.AddAsync(screening);
            ScreeningSaved?.Invoke(this, screening);
            // OnResetAsync back to add mode
            IsEditMode = false;
            SelectedHallId = null;
            SelectedMovieId = null;
            StartTimeString = "12:00";
            Error = null;
        }
        catch
        {
            Error = "Kunne ikke tilføje forestilling.";
        }
        finally
        {
            IsSaving = false;
        }
    }



    protected override async Task OnSaveAsync()
    {
        if (_currentId is null)
        {
            Error = "Ingen forestilling valgt.";
            return;
        }
        if (!CanSave()) return;

        IsSaving = true;
        Error = null;

        try
        {
            var start = BuildStartDateTime();
            var hallId = SelectedHallId!.Value;
            var movieId = SelectedMovieId!.Value;

            var (ok, msg) = await ValidateScheduleAsync(_currentId, hallId, movieId, start);
            if (!ok)
            {
                Error = msg;
                return;
            }

            var current = await _repository.GetByIdAsync(_currentId.Value);
            if (current is null)
            {
                Error = "Forestilling findes ikke længere.";
                return;
            }

            current.HallId = hallId;
            current.MovieId = movieId;
            current.StartTime = start;

            await _repository.UpdateAsync(current);
            ScreeningSaved?.Invoke(this, current);
            IsEditMode = false;
            await OnResetAsync();
        }
        catch
        {
            Error = "Kunne ikke gemme forestilling.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected override async Task OnResetAsync()
    {
        _currentId = null;
        SelectedCinemaId = null;
        SelectedMovieId = null;
        SelectedHallId = null;
        SelectedDate = null;
        StartTimeString = "12:00";
        IsEditMode = false;
        Error = null;
        await Task.CompletedTask;
    }

    protected override async Task OnDeleteAsync()
    {
        if (!CanDelete() || _currentId is null) return;
        IsSaving = true;
        Error = null;
        try
        {
            await _repository.DeleteAsync(_currentId.Value);
            ScreeningSaved?.Invoke(this, null!);
            await OnResetAsync();
        }
        catch
        {
            Error = "Kunne ikke slette forestilling.";
        }
        finally
        {
            IsSaving = false;
        }
    }
    #endregion

}