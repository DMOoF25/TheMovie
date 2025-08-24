using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class ScreeningsListViewModel : INotifyPropertyChanged
{
    private readonly IScreeningRepository _repository;

    private ScreeningsListItemViewModel? _selectedScreening;
    public ScreeningsListItemViewModel? SelectedScreening
    {
        get => _selectedScreening;
        set
        {
            if (_selectedScreening != value)
            {
                _selectedScreening = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<ScreeningsListItemViewModel> Screenings { get; } = [];

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

    public ScreeningsListViewModel(IScreeningRepository repository)
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
            var screenings = await _repository.GetAllAsync().ConfigureAwait(true);

            var ordered = screenings
                .Select(s => (Item: new ScreeningsListItemViewModel(s), Start: s.StartTime))
                .OrderBy(t => t.Item.CinemaNameDisplay)
                .ThenBy(t => t.Item.HallNameDisplay)
                .ThenBy(t => t.Start);

            Screenings.Clear();
            foreach (var t in ordered)
                Screenings.Add(t.Item);
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