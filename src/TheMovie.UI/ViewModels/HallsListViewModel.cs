using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public class HallsListViewModel : INotifyPropertyChanged
{
    private readonly IHallRepository _repository;

    private HallListItemViewModel? _selectedHall;
    public HallListItemViewModel? SelectedHall
    {
        get => _selectedHall;
        set
        {
            if (_selectedHall == value) return;
            _selectedHall = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<HallListItemViewModel> Halls { get; } = [];

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set { if (_isLoading == value) return; _isLoading = value; OnPropertyChanged(); }
    }

    private string? _error;
    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }

    public HallsListViewModel(IHallRepository repository)
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
            var halls = await _repository.GetAllAsync().ConfigureAwait(true);
            Halls.Clear();
            foreach (var m in halls.OrderBy(m => m.Name))
                Halls.Add(new HallListItemViewModel(m));
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
