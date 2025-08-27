using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class HallViewModel : INotifyPropertyChanged
{
    private readonly IHallRepository _repository;
    private readonly ICinemaRepository _cinemaRepository;

    private Guid? _currentId;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    private uint _capacity;
    public uint Capacity
    {
        get => _capacity;
        set
        {
            if (_capacity == value) return;
            _capacity = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }
    private Guid? _selectedCinemaId;
    public Guid? SelectedCinemaId
    {
        get => _selectedCinemaId;
        set
        {
            if (_selectedCinemaId == value) return;
            _selectedCinemaId = value;
            OnPropertyChanged();
            RefreshCommandStates();
        }
    }

    public ObservableCollection<CinemaListItemViewModel> Cinemas { get; private set; } = [];

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }

    private string? _error;
    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); }
    }
    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set { if (_isSaving == value) return; _isSaving = value; OnPropertyChanged(); RefreshCommandStates(); }
    }
    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            if (_isEditMode == value) return;
            _isEditMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAddMode));
            RefreshCommandStates();
        }
    }
    public bool IsAddMode => !IsEditMode;

    public event EventHandler<Hall>? HallSaved;

    public HallViewModel(IHallRepository? repository = null)
    {
        _repository = repository ?? App.HostInstance.Services.GetRequiredService<IHallRepository>();
        _cinemaRepository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();

        _ = LoadCinemaOption();

        AddCommand = new RelayCommand(OnAdd, CanAdd);
        SaveCommand = new RelayCommand(OnSave, CanSave);
        ResetCommand = new RelayCommand(OnReset, CanReset);
        CancelCommand = new RelayCommand(OnCancel);
        DeleteCommand = new RelayCommand(OnDelete, CanDelete);

        IsEditMode = false;
    }


    #region Load method
    // Populate form from repository by id (enter edit mode)
    public async Task LoadAsync(Guid id)
    {
        try
        {
            Error = null;
            var hall = await _repository.GetByIdAsync(id).ConfigureAwait(true);
            if (hall is null)
            {
                Error = "Biografsalen blev ikke fundet.";
                OnReset();
                return;
            }
            _currentId = hall.Id;
            Name = hall.Name;
            Capacity = hall.Capacity;
            SelectedCinemaId = hall.CinemaId;
            IsEditMode = true;
            Error = null;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    private async Task LoadCinemaOption()
    {
        Error = null;
        try
        {
            var all = await _cinemaRepository.GetAllAsync().ConfigureAwait(true);
            Cinemas.Clear();
            foreach (var i in all.OrderBy(i => i.Name))
                Cinemas.Add(new CinemaListItemViewModel(i));
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
    #endregion

    #region CanXXX methods
    private bool CanSubmitCore() =>
        !IsSaving &&
        !string.IsNullOrWhiteSpace(Name) &&
        SelectedCinemaId.HasValue &&
        Capacity > 0;

    private bool CanAdd() => CanSubmitCore() && IsAddMode;
    private bool CanSave() => CanSubmitCore() && IsEditMode;
    private bool CanReset() => IsEditMode && !IsSaving;
    private bool CanDelete() => IsEditMode && !IsSaving;
    #endregion

    #region Command Handlers
    private void OnAdd()
    {
        if (!CanAdd()) return;
        IsSaving = true;
        Error = null;
        var hall = new Hall(Name!, Capacity, SelectedCinemaId!.Value);
        try
        {
            _ = _repository.AddAsync(hall);
            HallSaved?.Invoke(this, hall);
            MessageBox.Show("Biografsal tilføjet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OnReset();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Fejlet at tilføje biografsal.\n{ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnSave()
    {
        if (_currentId is null)
        {
            Error = "Ingen biografsal valgt.";
            return;
        }

        Error = null;
        if (!CanSubmitCore()) return;

        IsSaving = true;
        try
        {
            var hall = new Hall
            {
                Id = _currentId.Value,
                Name = Name!,
                Capacity = Capacity,
                CinemaId = SelectedCinemaId!.Value
            };
            _ = _repository.UpdateAsync(hall);
            HallSaved?.Invoke(this, hall);
            MessageBox.Show("Biografsal gemt.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OnReset();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Fejlet at gemme biografsal.\n{ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnCancel()
    {
        OnReset();
    }

    private void OnDelete()
    {
        if (_currentId is null)
        {
            Error = "Ingen biografsal valgt.";
            return;
        }
        if (!CanDelete()) return;
        if (MessageBox.Show("Vil du slette denne biografsal?", "Bekræft sletning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;
        IsSaving = true;
        Error = null;
        try
        {
            _ = _repository.DeleteAsync(_currentId.Value);
            MessageBox.Show("Movie deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OnReset();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Fejlet at slette biografsal.\n{ex.Message}", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void OnReset()
    {
        if (!CanReset()) return;
        Name = string.Empty;
        Capacity = 0;
        SelectedCinemaId = null;
        IsEditMode = false;
        Error = null;
    }

    #endregion

    private void RefreshCommandStates()
    {
        (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
