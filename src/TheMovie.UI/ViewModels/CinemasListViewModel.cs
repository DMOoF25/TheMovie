using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class CinemasListViewModel : INotifyPropertyChanged
{
    private readonly ICinemaRepository _repository;
    private bool _isLoading;
    private string? _error;
    private CinemaListItemViewModel? _selectedCinema;

    public ObservableCollection<CinemaListItemViewModel> Cinemas { get; } = new();
    public ICommand RefreshCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set { if (_isLoading == value) return; _isLoading = value; OnPropertyChanged(); }
    }

    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(Error);

    public CinemaListItemViewModel? SelectedCinema
    {
        get => _selectedCinema;
        set
        {
            if (_selectedCinema == value) return;
            _selectedCinema = value;
            OnPropertyChanged();
        }
    }

    public CinemasListViewModel(ICinemaRepository repository)
    {
        _repository = repository;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsLoading);
        _ = RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var cinemas = await _repository.GetAllAsync().ConfigureAwait(true);
            Cinemas.Clear();
            foreach (var c in cinemas.OrderBy(c => c.Name))
                Cinemas.Add(new CinemaListItemViewModel(c));
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