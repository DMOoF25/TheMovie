using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.Commands;
using TheMovie.UI.Views;

namespace TheMovie.UI.ViewModels;

public sealed class InstructorViewModel : INotifyPropertyChanged
{
    private readonly IInstructorRepository _repository;

    private Guid? _currentId;
    private string _name = string.Empty;
    private bool _isSaving;
    private string? _error;
    private bool _isEditMode;

    public string Name
    {
        get => _name;
        set { if (_name == value) return; _name = value; OnPropertyChanged(); RefreshCommands(); }
    }

    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(Error);

    public bool IsSaving
    {
        get => _isSaving;
        private set { if (_isSaving == value) return; _isSaving = value; OnPropertyChanged(); RefreshCommands(); }
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set { if (_isEditMode == value) return; _isEditMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsAddMode)); RefreshCommands(); }
    }
    public bool IsAddMode => !IsEditMode;

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<Instructor>? InstructorSaved;

    public InstructorViewModel(IInstructorRepository? repository = null)
    {
        _repository = repository ?? App.HostInstance.Services.GetRequiredService<IInstructorRepository>();

        AddCommand = new RelayCommand(Add, CanSubmitAdd);
        SaveCommand = new RelayCommand(Save, CanSubmitSave);
        DeleteCommand = new RelayCommand(Delete, CanSubmitDelete);
        ResetCommand = new RelayCommand(Reset, CanReset);
        CancelCommand = new RelayCommand(Cancel);

        IsEditMode = false;
    }

    public async Task LoadAsync(Guid id)
    {
        try
        {
            Error = null;
            var instructor = await _repository.GetByIdAsync(id).ConfigureAwait(true);
            if (instructor is null)
            {
                Error = "Instructor not found.";
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

    private bool CanSubmitCore() =>
        !IsSaving
        && !string.IsNullOrWhiteSpace(Name);

    private bool CanSubmitAdd() => IsAddMode && CanSubmitCore();
    private bool CanSubmitSave() => IsEditMode && CanSubmitCore();
    private bool CanSubmitDelete() => true;
    private bool CanReset() => !string.IsNullOrWhiteSpace(Name) || IsEditMode;

    private void Add()
    {
        Error = null;
        if (!CanSubmitCore()) return;

        IsSaving = true;
        try
        {
            var instructor = new Instructor(Name.Trim());
            _repository.AddAsync(instructor).GetAwaiter().GetResult();

            InstructorSaved?.Invoke(this, instructor);
            MessageBox.Show("Instructor added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Reset();
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

    private void Save()
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
            IsSaving = false;
        }
    }

    private void Delete()
    {
        if (_currentId is null) return;
        if (MessageBox.Show("Are you sure you want to delete this instructor?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        IsSaving = true;
        try
        {
            _repository.DeleteAsync(_currentId.Value).GetAwaiter().GetResult();
            InstructorSaved?.Invoke(this, null);
            MessageBox.Show("Instructor deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Reset();
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
    }

    private void Reset()
    {
        _currentId = null;
        Name = string.Empty;
        Error = null;
        IsEditMode = false; // back to add mode
    }

    private void Cancel()
    {
        Reset();
        var mainFrame = (System.Windows.Application.Current.MainWindow as MainWindow)?.MainFrame;
        mainFrame?.Navigate(new MainPage());
    }

    private void RefreshCommands()
    {
        (AddCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}