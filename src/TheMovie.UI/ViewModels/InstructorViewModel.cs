using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class InstructorViewModel : ViewModelBase<IInstructorRepository, Instructor>
{
    private Guid? _currentId;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { if (_name == value) return; _name = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    //public event EventHandler<Instructor>? InstructorSaved;
    // Change the event declaration to allow null for the Instructor parameter
    public event EventHandler<Instructor?>? InstructorSaved;

    public InstructorViewModel(IInstructorRepository? repository = null) : base(App.HostInstance.Services.GetRequiredService<IInstructorRepository>())
    {
    }

    public override async Task LoadAsync(Guid id)
    {
        try
        {
            Error = null;
            var instructor = await _repository.GetByIdAsync(id).ConfigureAwait(true);
            if (instructor is null)
            {
                Error = "Filmintruktør ikke fundet.";
                return;
            }

            _currentId = instructor.Id;
            Name = instructor.Name;
            IsEditMode = true;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    protected override bool CanSubmitCore() =>
        !IsSaving
        && !string.IsNullOrWhiteSpace(Name);

    protected override async Task OnAddAsync()
    {
        Error = null;
        if (!CanSubmitCore()) return;

        IsSaving = true;
        try
        {
            var instructor = new Instructor(Name.Trim());
            await _repository.AddAsync(instructor);

            InstructorSaved?.Invoke(this, instructor);
            MessageBox.Show("Instructor added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await OnResetAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Failed to add instructor.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Error = "No instructor selected.";
            return;
        }

        Error = null;
        if (!CanSubmitCore()) return;

        IsSaving = true;
        try
        {
            var instructor = _repository.GetByIdAsync(_currentId.Value).GetAwaiter().GetResult();
            if (instructor is null)
            {
                Error = "Instructor not found.";
                return;
            }

            instructor.Name = Name.Trim();
            _repository.UpdateAsync(instructor).GetAwaiter().GetResult();

            InstructorSaved?.Invoke(this, instructor);
            MessageBox.Show("Instructor saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Failed to save instructor.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            await OnResetAsync(); // back to add mode with empty form
            IsSaving = false;
        }
        await Task.CompletedTask;
    }

    protected override async Task OnDeleteAsync()
    {
        if (_currentId is null) return;
        if (MessageBox.Show("Vil du slette filminstruktøren?", "Bekræft sletning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        IsSaving = true;
        try
        {
            _repository.DeleteAsync(_currentId.Value).GetAwaiter().GetResult();
            InstructorSaved?.Invoke(this, null);
            MessageBox.Show("Filminstruktøren slettet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await OnResetAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            MessageBox.Show($"Failed to delete instructor.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
        await Task.CompletedTask;
    }

    protected override async Task OnResetAsync()
    {
        _currentId = null;
        Name = string.Empty;
        Error = null;
        IsEditMode = false; // back to add mode
        await Task.CompletedTask;
    }
}