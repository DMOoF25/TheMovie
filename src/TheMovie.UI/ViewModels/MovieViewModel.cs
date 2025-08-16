using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class MovieViewModel : INotifyPropertyChanged
{
    private readonly IMovieRepository _repository;

    private string _title = string.Empty;
    private string _durationText = string.Empty; // keep raw input for validation display
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



    public MovieViewModel(IMovieRepository repository)
    {
        _repository = repository;

        LoadGenreOptions();

        SaveCommand = new RelayCommand(Save, CanSave);
        ResetCommand = new RelayCommand(Reset, CanReset);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void LoadGenreOptions()
    {
        // Exclude 0 (None)
        foreach (Genre g in Enum.GetValues(typeof(Genre)))
        {
            if (g == Genre.None) continue;
            GenreOptions.Add(new GenreOptionViewModel(g.ToString(), g));
        }

        foreach (var opt in GenreOptions)
            opt.PropertyChanged += (_, _) => RefreshCommandStates();
    }

    private bool CanSave()
    {
        if (IsSaving) return false;
        if (string.IsNullOrWhiteSpace(Title)) return false;
        if (!TryParseDuration(out _)) return false;
        if (GetSelectedGenres() == Genre.None) return false;
        return true;
    }

    private bool CanReset()
        => !string.IsNullOrEmpty(Title) ||
           !string.IsNullOrEmpty(DurationText) ||
           GenreOptions.Any(o => o.IsSelected);

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
            var movie = new Movie(Title.Trim(), duration)
            {
                Genres = selectedGenres
            };

            // In-memory add
            _repository.AddAsync(movie).GetAwaiter().GetResult();

            MessageBox.Show("Movie saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Reset(); // Clear form for next entry
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
        foreach (var g in GenreOptions)
            g.IsSelected = false;
        Error = null;
    }

    private void Cancel()
    {
        // For now just clear; navigation/back handled by caller (MainWindow frame history).
        Reset();
        MessageBox.Show("Input cleared.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
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
