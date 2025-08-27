using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class HallViewModel : ViewModelBase<IHallRepository, Hall>
{
    private readonly ICinemaRepository _cinemaRepository;

    // To track current entity in edit mode
    private Guid? _currentId;

    // Form fields
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

    // State fields
    public ObservableCollection<CinemaListItemViewModel> Cinemas { get; private set; } = [];

    public event EventHandler<Hall>? HallSaved;

    public HallViewModel(IHallRepository? repository = null) : base(repository ?? App.HostInstance.Services.GetRequiredService<IHallRepository>())
    {
        _cinemaRepository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();
    }


    #region Load method
    // Populate form from repository by id (enter edit mode)
    public override async Task LoadAsync(Guid id)
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
    protected override bool CanSubmitCore() =>
        !IsSaving &&
        !string.IsNullOrWhiteSpace(Name) &&
        SelectedCinemaId.HasValue &&
        Capacity > 0;

    //private bool CanAdd() => CanSubmitCore() && IsAddMode;
    //private bool CanSave() => CanSubmitCore() && IsEditMode;
    //private bool CanReset() => IsEditMode && !IsSaving;
    //private bool CanDelete() => IsEditMode && !IsSaving;
    protected override void OnAdd()
    {
        if (!CanAdd()) return;
        IsSaving = true;
        Error = null;
        var hall = new Hall(Name!, Capacity, SelectedCinemaId!.Value);
        try
        {
            _ = _repository.AddAsync(hall);
            // Raise the Saved event using the base class method or protected accessor
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

    protected override void OnSave()
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

    protected override void OnDelete()
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

    protected override void OnReset()
    {
        if (!CanReset()) return;
        Name = string.Empty;
        Capacity = 0;
        SelectedCinemaId = null;
        IsEditMode = false;
        Error = null;
    }

    #endregion

}
