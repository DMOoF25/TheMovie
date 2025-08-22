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

    private string _title = string.Empty;
    private string _durationText = string.Empty;
    private DateOnly _premiereDate = DateOnly.FromDateTime(DateTime.Now);
    private bool _isSaving;
    private string? _error;

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    public string DurationText
    {
        get => _durationText;
        set
        {
            if (_durationText == value) return;
            _durationText = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    public DateOnly PremiereDate
    {
        get => _premiereDate;
        set
        {
            _premiereDate = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    public ObservableCollection<GenreOptionViewModel> GenreOptions { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }

    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (_isSaving == value) return;
            _isSaving = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    public string? Error
    {
        get => _error;
        private set
        {
            if (_error == value) return;
            _error = value;
            OnPropertyChanged();
        }
    }

    public event EventHandler<Movie>? MovieSaved;

    public MovieViewModel()
    {
        _repository = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        LoadGenreOptions();

        SaveCommand = new RelayCommand(Save, CanSave);
        ResetCommand = new RelayCommand(Reset, CanReset);
        CancelCommand = new RelayCommand(Cancel);
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
                // Force re-evaluation of ComboBox.Text binding (uses converter on GenreOptions)
                OnPropertyChanged(nameof(GenreOptions));
            };
        }
    }

    private bool CanSave() =>
        !IsSaving
        && !string.IsNullOrWhiteSpace(Title)
        && PremiereDate >= DateOnly.FromDateTime(DateTime.Now)
        && TryParseDuration(out _)
        && GetSelectedGenres() != Genre.None;

    private bool CanReset() =>
        !string.IsNullOrEmpty(Title)
        || !string.IsNullOrEmpty(DurationText)
        || PremiereDate != DateOnly.FromDateTime(DateTime.Now)
        || GenreOptions.Any(o => o.IsSelected);

    private void Save()
    {
        Error = null;
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

        IsSaving = true;
        try
        {
            var movie = new Movie(Title.Trim(), duration, PremiereDate)
            {
                Genres = selectedGenres
            };

            _repository.AddAsync(movie).GetAwaiter().GetResult();

            MovieSaved?.Invoke(this, movie);

            MessageBox.Show("Movie saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Reset();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Failed to save movie.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void Reset()
    {
        Title = string.Empty;
        DurationText = string.Empty;
        PremiereDate = DateOnly.FromDateTime(DateTime.Now);
        foreach (var g in GenreOptions)
            g.IsSelected = false;
        Error = null;
    }

    private void Cancel()
    {
        Reset();

        var mainFrame = (System.Windows.Application.Current.MainWindow as MainWindow)?.MainFrame;
        mainFrame?.Navigate(new MainPage());
    }

    private bool TryParseDuration(out int value)
    {
        if (int.TryParse(DurationText, out value))
        {
            if (value > 0) return true;
        }
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
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
