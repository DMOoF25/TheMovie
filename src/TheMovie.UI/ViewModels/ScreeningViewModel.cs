using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public class ScreeningViewModel : INotifyPropertyChanged
{
    private readonly IScreeningRepository _repository;
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

    public ObservableCollection<CinemaListItemViewModel> Cinemas { get; } = new();
    public ObservableCollection<HallListItemViewModel> Halls { get; } = new();
    public ObservableCollection<MovieListItemViewModel> Movies { get; } = new();

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }

    private string? _error;
    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); }
    }
    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set { if (_isSaving == value) return; _isSaving = value; OnPropertyChanged(); RefreshCommandStates(); }
    }
    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            if (_isEditMode == value) return;
            _isEditMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAddMode));
            RefreshCommandStates();
        }
    }
    public bool IsAddMode => !IsEditMode;

    public event EventHandler<Screening>? ScreeningSaved;

    public ScreeningViewModel(IScreeningRepository repository)
    {
        _repository = repository ?? App.HostInstance.Services.GetRequiredService<IScreeningRepository>();
        _movieRepository = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        _cinemaRepository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();
        _hallRepository = App.HostInstance.Services.GetRequiredService<IHallRepository>();

        _ = LoadMoviesAsync();
        _ = LoadCinemasAsync();
        _ = LoadHallsAsync();

        AddCommand = new RelayCommand(OnAdd, CanAdd);
        SaveCommand = new RelayCommand(OnSave, CanSave);
        ResetCommand = new RelayCommand(OnReset, CanReset);
        CancelCommand = new RelayCommand(Cancel);
        DeleteCommand = new RelayCommand(Delete, CanDelete);

        IsEditMode = false;
    }

    private async Task LoadCinemasAsync()
    {
        try
        {
            var all = (await _cinemaRepository.GetAllAsync()).ToList();
            Cinemas.Clear();
            foreach (var item in all)
            {
                Cinemas.Add(new CinemaListItemViewModel(item));
            }
            OnPropertyChanged(nameof(Cinemas));
        }
        catch
        {
            // Best-effort; keep UI responsive even if loading fails
        }
    }

    private async Task LoadHallsAsync()
    {
        try
        {
            var all = (await _hallRepository.GetAllAsync()).ToList();
            Halls.Clear();
            foreach (var item in all)
            {
                Halls.Add(new HallListItemViewModel(item));
            }
            OnPropertyChanged(nameof(Halls));
        }
        catch
        {
            // Best-effort; keep UI responsive even if loading fails
        }
    }

    private async Task LoadMoviesAsync()
    {
        try
        {
            var all = (await _movieRepository.GetAllAsync()).ToList();
            Movies.Clear();
            foreach (var item in all)
            {
                Movies.Add(new MovieListItemViewModel(item));
            }
            OnPropertyChanged(nameof(Movies));
        }
        catch
        {
            // Best-effort; keep UI responsive even if loading fails
        }
    }


    private bool CanSubmit() =>
        !IsSaving &&
        SelectedCinemaId.HasValue &&
        SelectedMovieId.HasValue &&
        SelectedHallId.HasValue;

    private bool CanAdd() => CanSubmit() && IsAddMode;
    private bool CanSave() => CanSubmit() && IsEditMode;
    private bool CanReset() => IsEditMode && !IsSaving;
    private bool CanDelete() => IsEditMode && !IsSaving;

    private void OnAdd()
    {
        if (!CanAdd()) return;
        IsSaving = true;
        Error = null;
        var screening = new Screening
        {
            MovieId = SelectedMovieId!.Value,
            HallId = SelectedHallId!.Value,
            StartTime = DateTime.Now // Placeholder; in real app, would be user input
        };
        _repository.AddAsync(screening).ContinueWith(t =>
        {
            IsSaving = false;
            if (t.IsFaulted)
            {
                Error = "Failed to add screening.";
            }
            else
            {
                ScreeningSaved?.Invoke(this, screening);
                OnReset();
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void OnSave()
    {
        if (_currentId is null)
        {
            Error = "Ingen forestilling valgt.";
            return;
        }


    }

    private void OnReset()
    {
        if (!CanReset()) return;
        SelectedCinemaId = null;
        SelectedMovieId = null;
        SelectedHallId = null;
        IsEditMode = false;
        Error = null;
    }
    private void Cancel()
    {
        OnReset();
        Error = null;
    }

    private void Delete()
    {
        if (!CanDelete()) return;
        IsSaving = true;
        Error = null;
        // In a real app, would delete the existing screening entity
        IsSaving = false;
        ScreeningSaved?.Invoke(this, null!); // Placeholder
        OnReset();
    }

    private void RefreshCommandStates()
    {
        (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}