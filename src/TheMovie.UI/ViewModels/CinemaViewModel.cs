using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class CinemaViewModel : ViewModelBase<ICinemaRepository, Cinema>
{
    // To track current entity in edit mode
    private Guid? _currentId;

    // Form fields
    private string _name = string.Empty;
    private string _location = string.Empty;


    public string Name
    {
        get => _name;
        set { if (_name == value) return; _name = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    public string Location
    {
        get => _location;
        set { if (_location == value) return; _location = value; OnPropertyChanged(); RefreshCommandStates(); }
    }



    public event EventHandler<Cinema?>? CinemaSaved;

    public CinemaViewModel(CinemaListItemViewModel? selected = default) :
        base(App.HostInstance.Services.GetRequiredService<ICinemaRepository>())
    {

    }

    public override async Task LoadAsync(Guid id)
    {
        try
        {
            Error = null;
            var cinema = await _repository.GetByIdAsync(id).ConfigureAwait(true);
            if (cinema is null)
            {
                Error = "Biograf blev ikke fundet.";
                return;
            }

            _currentId = cinema.Id;
            Name = cinema.Name;
            Location = cinema.Location;
            IsEditMode = true;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    protected override bool CanSubmitCore() =>
        !IsSaving
        && !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Location);

    protected override async Task OnAddAsync()
    {
        Error = null;
        if (!CanSubmitCore()) return;

        IsSaving = true;
        try
        {
            var cinema = new Cinema(Name.Trim(), Location.Trim());
            await _repository.AddAsync(cinema);
            CinemaSaved?.Invoke(this, cinema);
            MessageBox.Show("Biograf tilføjet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await OnResetAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Kunne ikke tilføje biograf.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected override async Task OnSaveAsync()
    {
        if (_currentId is null)
        {
            Error = "Ingen biograf valgt.";
            return;
        }

        Error = null;
        if (!CanSubmitCore()) return;

        IsSaving = true;
        try
        {
            var cinema = _repository.GetByIdAsync(_currentId.Value).GetAwaiter().GetResult();
            if (cinema is null)
            {
                Error = "Biograf blev ikke fundet.";
                return;
            }

            cinema.Name = Name.Trim();
            cinema.Location = Location.Trim();
            _repository.UpdateAsync(cinema).GetAwaiter().GetResult();

            CinemaSaved?.Invoke(this, cinema);
            MessageBox.Show("Biograf gemt.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Kunne ikke gemme biograf.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
        await OnResetAsync();
    }

    protected override async Task OnDeleteAsync()
    {
        if (_currentId is null) return;
        if (MessageBox.Show("Er du sikker på, at du vil slette denne biograf?", "Bekræft sletning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            try
            {
                await _repository.DeleteAsync(_currentId.Value);
                CinemaSaved?.Invoke(this, null);
                MessageBox.Show("Biograf slettet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await OnResetAsync();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                MessageBox.Show($"Kunne ikke slette biograf.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    protected override async Task OnResetAsync()
    {
        _currentId = null;
        Name = string.Empty;
        Location = string.Empty;
        Error = null;
        IsEditMode = false; // back to add mode
        await Task.CompletedTask;
    }

}