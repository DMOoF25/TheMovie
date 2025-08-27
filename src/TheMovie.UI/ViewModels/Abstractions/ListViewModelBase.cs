using System.Collections.ObjectModel;
using System.Windows.Input;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels.Abstractions;

public abstract class ListViewModelBase<TEntity, ListItemVM> : ModelBase
    where TEntity : class
{
    protected readonly TEntity _repository;
    protected ListItemVM? _selectedItem;
    public ListItemVM? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Equals(_selectedItem, value)) return;
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ListItemVM> Items { get; } = new();

    protected bool _isLoading;
    // Ensure the IsLoading property has a protected or public setter
    public bool IsLoading { get; protected set; }
    protected string? _error;
    public string? Error
    {
        get => _error;
        protected set
        {
            if (_error == value) return;
            _error = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public ICommand RefreshCommandState { get; }

    public ListViewModelBase(TEntity repository)
    {
        _repository = repository;
        RefreshCommandState = new RelayCommand(async () => await RefreshAsync(), () => !IsLoading);
        _ = RefreshAsync(); // initial load
    }

    public abstract Task RefreshAsync();

}