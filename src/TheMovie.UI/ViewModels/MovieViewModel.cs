using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class MovieViewModel : ViewModelBase<IMovieRepository, Movie>
{
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

    public event EventHandler<Movie>? MovieSaved;

    public MovieViewModel(MovieListItemViewModel? selected = default) : base(App.HostInstance.Services.GetRequiredService<IMovieRepository>())
    {
        _instructorRepository = App.HostInstance.Services.GetRequiredService<IInstructorRepository>();

        LoadGenreOptions();
        _ = LoadInstructorOptionsAsync();

    }

    #region Load method
    // Populate form from repository by id (enter edit mode)
    public override async Task LoadAsync(Guid id)
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
    protected override bool CanSubmitCore() =>
        !IsSaving
        && !string.IsNullOrWhiteSpace(Title)
        && TryParseDuration(out _)
        && GetSelectedGenres() != Genre.None;

    #endregion

    #region Command Handlers
    protected override async Task OnAddAsync()
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

            await OnResetAsync(); // back to add mode with empty form
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

    protected override async Task OnSaveAsync()
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

            await _repository.UpdateAsync(movie);

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

    protected override async Task OnDeleteAsync()
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
            await _repository.DeleteAsync(_currentId.Value);
            MessageBox.Show("Film slettet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await OnResetAsync(); // back to add mode with empty form
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

    protected override async Task OnResetAsync()
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
        await Task.CompletedTask;
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

}
