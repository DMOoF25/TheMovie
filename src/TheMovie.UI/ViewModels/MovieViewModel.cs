using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;
using TheMovie.UI.Commands;
using TheMovie.UI.Views;

namespace TheMovie.UI.ViewModels;

public sealed class MovieViewModel : INotifyPropertyChanged
{
    private readonly IMovieRepository _repository;
    private readonly IInstructorRepository _instructorRepository;

    private Guid? _currentId;

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set { if (_title == value) return; _title = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    private string _durationText = string.Empty;
    public string DurationText
    {
        get => _durationText;
        set { if (_durationText == value) return; _durationText = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    private DateOnly _premiereDate = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly PremiereDate
    {
        get => _premiereDate;
        set { _premiereDate = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    private Guid? _selectedInstructorId;
    public Guid? SelectedInstructorId
    {
        get => _selectedInstructorId;
        set { if (_selectedInstructorId == value) return; _selectedInstructorId = value; OnPropertyChanged(); }
    }

    public ObservableCollection<GenreOptionViewModel> GenreOptions { get; } = new();
    public ObservableCollection<InstructorListItemViewModel> Instructors { get; } = new();

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set { if (_isSaving == value) return; _isSaving = value; OnPropertyChanged(); RefreshCommandStates(); }
    }
    private string? _error;
    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); }
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

    public event EventHandler<Movie>? MovieSaved;

    public MovieViewModel(IMovieRepository? repository = null)
    {
        _repository = repository ?? App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        _instructorRepository = App.HostInstance.Services.GetRequiredService<IInstructorRepository>();

        LoadGenreOptions();
        _ = LoadInstructorOptionsAsync();

        AddCommand = new RelayCommand(OnAdd, CanAdd);
        SaveCommand = new RelayCommand(OnSave, CanSave);
        ResetCommand = new RelayCommand(OnReset, CanReset);
        CancelCommand = new RelayCommand(OnCancel);
        DeleteCommand = new RelayCommand(OnDelete, CanDelete);

        IsEditMode = false;
    }

    #region Load method
    // Populate form from repository by id (enter edit mode)
    public async Task LoadAsync(Guid id)
    {
        try
        {
            Error = null;
            var movie = await _repository.GetByIdAsync(id).ConfigureAwait(true);
            if (movie is null)
            {
                Error = "Movie not found.";
                return;
            }

            _currentId = movie.Id;
            Title = movie.Title;
            DurationText = movie.Duration.ToString();
            PremiereDate = movie.PremiereDate;
            foreach (var opt in GenreOptions)
                opt.IsSelected = movie.Genres.HasFlag(opt.Value);

            SelectedInstructorId = movie.InstructorId != Guid.Empty ? movie.InstructorId : null;

            IsEditMode = true;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    private void LoadGenreOptions()
    {
        foreach (Genre g in Enum.GetValues(typeof(Genre)))
        {
            if (g == Genre.None) continue;
            GenreOptions.Add(new GenreOptionViewModel(g.ToString(), g));
        }

        foreach (var opt in GenreOptions)
        {
            opt.PropertyChanged += (_, _) =>
            {
                RefreshCommandStates();
                OnPropertyChanged(nameof(GenreOptions)); // update ComboBox.Text via converter
            };
        }
    }

    private async Task LoadInstructorOptionsAsync()
    {
        try
        {
            var all = await _instructorRepository.GetAllAsync().ConfigureAwait(true);
            Instructors.Clear();
            foreach (var i in all.OrderBy(i => i.Name))
                Instructors.Add(new InstructorListItemViewModel(i));
        }
        catch
        {
            // Best-effort; keep UI responsive even if loading fails
        }
    }

    #endregion

    #region CanXXX methods
    private bool CanSubmit() =>
        !IsSaving
        && !string.IsNullOrWhiteSpace(Title)
        && TryParseDuration(out _)
        && GetSelectedGenres() != Genre.None;

    private bool CanAdd() => IsAddMode && CanSubmit() && PremiereDate >= DateOnly.FromDateTime(DateTime.Now);
    private bool CanSave() => IsEditMode && CanSubmit();

    private bool CanDelete() => true;
    private bool CanReset() => IsEditMode || !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(DurationText);
    #endregion

    #region Command Handlers
    private void OnAdd()
    {
        if (!TryParseDuration(out var duration))
        {
            Error = "Filmens varighed skal være et positv tal.";
            return;
        }

        var selectedGenres = GetSelectedGenres();
        if (selectedGenres == Genre.None)
        {
            Error = "Vælg mindst en genre.";
            return;
        }

        Error = null;
        IsSaving = true;
        try
        {
            var movie = new Movie(Title.Trim(), duration, PremiereDate, SelectedInstructorId ?? Guid.Empty)
            {
                Genres = selectedGenres
            };

            _repository.AddAsync(movie).GetAwaiter().GetResult();

            MovieSaved?.Invoke(this, movie);
            MessageBox.Show("Film tilføjet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            OnReset(); // back to add mode with empty form
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Fejlet at gemme film.\n{ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnSave()
    {
        if (_currentId is null)
        {
            Error = "Nothing to update. Please select a movie.";
            return;
        }

        if (!TryParseDuration(out var duration))
        {
            Error = "Duration must be a positive whole number.";
            return;
        }

        var selectedGenres = GetSelectedGenres();
        if (selectedGenres == Genre.None)
        {
            Error = "Select at least one genre.";
            return;
        }

        Error = null;
        IsSaving = true;
        try
        {
            var movie = _repository.GetByIdAsync(_currentId.Value).GetAwaiter().GetResult();
            if (movie is null)
            {
                Error = "Movie not found.";
                return;
            }

            movie.Title = Title.Trim();
            movie.Duration = duration;
            movie.PremiereDate = PremiereDate;
            movie.Genres = selectedGenres;
            movie.InstructorId = SelectedInstructorId ?? Guid.Empty;

            _repository.UpdateAsync(movie).GetAwaiter().GetResult();

            MovieSaved?.Invoke(this, movie); // reuse to trigger list refresh
            MessageBox.Show("Movie updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Failed to update movie.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnDelete()
    {
        if (_currentId is null)
        {
            Error = "Intet at slette!";
            return;
        }
        if (MessageBox.Show("Vil du slette denne film?", "Bekræft slet",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;
        IsSaving = true;
        try
        {
            _repository.DeleteAsync(_currentId.Value).GetAwaiter().GetResult();
            MessageBox.Show("Film slettet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OnReset(); // back to add mode with empty form
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Fejlet at slette film.\n{ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnReset()
    {
        _currentId = null;
        Title = string.Empty;
        DurationText = string.Empty;
        PremiereDate = DateOnly.FromDateTime(DateTime.Now);
        foreach (var g in GenreOptions)
            g.IsSelected = false;
        SelectedInstructorId = null;
        Error = null;

        IsEditMode = false; // back to add mode
    }

    private void OnCancel()
    {
        OnReset();

        var mainFrame = (System.Windows.Application.Current.MainWindow as MainWindow)?.MainFrame;
        mainFrame?.Navigate(new MainPage());
    }
    #endregion

    private bool TryParseDuration(out int value)
    {
        if (int.TryParse(DurationText, out value) && value > 0)
            return true;
        value = 0;
        return false;
    }

    private Genre GetSelectedGenres()
    {
        Genre combined = Genre.None;
        foreach (var opt in GenreOptions.Where(o => o.IsSelected))
            combined |= opt.Value;
        return combined;
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
