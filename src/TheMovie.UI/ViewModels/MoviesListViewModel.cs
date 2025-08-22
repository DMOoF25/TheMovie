using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class MoviesListViewModel : INotifyPropertyChanged
{
    private readonly IMovieRepository _repository;
    private bool _isLoading;
    private string? _error;

    public ObservableCollection<MovieListItemViewModel> Movies { get; } = new();
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
        RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsLoading);
        _ = RefreshAsync(); // initial load
    }

    public async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var movies = await _repository.GetAllAsync().ConfigureAwait(true);
            Movies.Clear();
            foreach (var m in movies.OrderBy(m => m.Title))
                Movies.Add(new MovieListItemViewModel(m));
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}