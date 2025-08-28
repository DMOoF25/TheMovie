using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.Commands;
using TheMovie.UI.Views;

namespace TheMovie.UI.ViewModels.Abstractions;

public abstract class ViewModelBase<TRepos, TEntity> : ModelBase
    where TRepos : notnull
{
    protected readonly TRepos _repository;

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand NavigateHomeCommand { get; }

    protected string? _error;
    public string? Error
    {
        get => _error;
        protected set { if (_error == value) return; _error = value; OnPropertyChanged(); }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);
    protected bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        protected set { if (_isSaving == value) return; _isSaving = value; OnPropertyChanged(); RefreshCommandStates(); }
    }
    protected bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        protected set
        {
            if (_isEditMode == value) return;
            _isEditMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAddMode));
            RefreshCommandStates();
        }
    }
    public bool IsAddMode => !IsEditMode;

    protected ViewModelBase(TRepos repository)
    {
        _repository = repository ?? App.HostInstance.Services.GetRequiredService<TRepos>();

        // Change RelayCommand instantiations to use async-compatible command
        AddCommand = new RelayCommand(async () => await OnAddAsync(), CanAdd);
        SaveCommand = new RelayCommand(async () => await OnSaveAsync(), CanSave);
        DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), CanDelete);
        ResetCommand = new RelayCommand(async () => await OnResetAsync(), CanReset);
        CancelCommand = new RelayCommand(async () => await OnCancelAsync(), CanCancel);
        NavigateHomeCommand = new RelayCommand(async () => await OnNavigateHomeAsync());

        IsEditMode = false;
    }

    #region Load method
    public abstract Task LoadAsync(Guid id);
    #endregion
    #region CanXXX methods
    protected abstract bool CanSubmitCore();

    protected bool CanAdd() => CanSubmitCore() && IsAddMode;
    protected bool CanSave() => CanSubmitCore() && IsEditMode;
    protected bool CanReset() => IsEditMode && !IsSaving;
    protected bool CanDelete() => IsEditMode && !IsSaving;
    protected bool CanCancel() => IsEditMode && !IsSaving;
    #endregion
    #region Command Handlers
    protected abstract Task OnAddAsync();

    protected abstract Task OnSaveAsync();

    protected async Task OnCancelAsync()
    {
        await OnResetAsync();
    }

    protected abstract Task OnDeleteAsync();

    protected abstract Task OnResetAsync();

    protected async Task OnNavigateHomeAsync()
    {
        // Get the main window
        var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        // Navigate the MainFrame to MainPageView
        mainWindow?.MainFrame.Navigate(new MainPageView());

        await Task.CompletedTask;
    }

    #endregion

    protected virtual void RefreshCommandStates()
    {
        (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

}
