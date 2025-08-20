using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class MoviesListViewModel : INotifyPropertyChanged
{
    private readonly IMovieRepository _repository;
    private bool _isLoading;
    private string? _error;
    private readonly object _sync = new();

    public ObservableCollection<MovieRowViewModel> Movies { get; } = new();
    public ICollectionView MoviesView { get; }

    public ICommand RefreshCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set { if (_isLoading == value) return; _isLoading = value; OnPropertyChanged(); }
    }

    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); }
    }

    public MoviesListViewModel(IMovieRepository repository)
    {
        _repository = repository;
        _repository.InitializeAsync(); // Ensure to have loaded data in view
        MoviesView = CollectionViewSource.GetDefaultView(Movies);
        MoviesView.SortDescriptions.Add(new SortDescription(nameof(MovieRowViewModel.Title), ListSortDirection.Ascending));

        RefreshCommand = new RelayCommand(async () => await LoadAsync(), () => !IsLoading);

        // Initial asynchronous load (fire & forget)
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var all = await _repository.GetAllAsync();
            lock (_sync)
            {
                Movies.Clear();
                foreach (var m in all.OrderBy(m => m.Title))
                    Movies.Add(new MovieRowViewModel(m));
            }
            MoviesView.Refresh();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public void AddOrUpdate(Movie movie)
    {
        lock (_sync)
        {
            var existing = Movies.FirstOrDefault(r => r.Id == movie.Id);
            if (existing is null)
            {
                Movies.Add(new MovieRowViewModel(movie));
            }
            else
            {
                existing.RefreshFromSource();
            }
        }
        MoviesView.Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}