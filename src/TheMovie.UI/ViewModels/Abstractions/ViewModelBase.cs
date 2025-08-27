using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.Commands;

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

        AddCommand = new RelayCommand(OnAdd, CanAdd);
        SaveCommand = new RelayCommand(OnSave, CanSave);
        DeleteCommand = new RelayCommand(OnDelete, CanDelete);
        ResetCommand = new RelayCommand(OnReset, CanReset);
        CancelCommand = new RelayCommand(OnCancel, CanCancel);

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
    protected abstract void OnAdd();

    protected abstract void OnSave();

    protected void OnCancel()
    {
        OnReset();
    }

    protected abstract void OnDelete();

    protected abstract void OnReset();

    #endregion

    protected void RefreshCommandStates()
    {
        (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

}
