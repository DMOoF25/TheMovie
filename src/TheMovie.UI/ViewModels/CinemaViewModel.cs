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

public sealed class CinemaViewModel : INotifyPropertyChanged
{
    private readonly ICinemaRepository _repository;

    private Guid? _currentId;
    private string _name = string.Empty;
    private string _location = string.Empty;
    private bool _isSaving;
    private string? _error;
    private bool _isEditMode;

    public string Name
    {
        get => _name;
        set { if (_name == value) return; _name = value; OnPropertyChanged(); RefreshCommands(); }
    }

    public string Location
    {
        get => _location;
        set { if (_location == value) return; _location = value; OnPropertyChanged(); RefreshCommands(); }
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

    public event EventHandler<Cinema>? CinemaSaved;

    public CinemaViewModel()
    {
        _repository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();
        AddCommand = new RelayCommand(OnAdd, CanSubmitAdd);
        SaveCommand = new RelayCommand(OnSave, CanSubmitSave);
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

    private bool CanSubmitCore() =>
        !IsSaving
        && !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Location);

    private bool CanSubmitAdd() => IsAddMode && CanSubmitCore();
    private bool CanSubmitSave() => IsEditMode && CanSubmitCore();
    private bool CanSubmitDelete() => true;
    private bool CanReset() => !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Location) || IsEditMode;

    private void OnAdd()
    {
        Error = null;
        if (!CanSubmitCore()) return;

        IsSaving = true;
        try
        {
            var cinema = new Cinema(Name.Trim(), Location.Trim());
            _repository.AddAsync(cinema).GetAwaiter().GetResult();
            CinemaSaved?.Invoke(this, cinema);
            MessageBox.Show("Biograf tilføjet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Reset();
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

    private void OnSave()
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
    }

    private void Delete()
    {
        if (_currentId is null) return;
        if (MessageBox.Show("Er du sikker på, at du vil slette denne biograf?", "Bekræft sletning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            try
            {
                _repository.DeleteAsync(_currentId.Value).GetAwaiter().GetResult();
                CinemaSaved?.Invoke(this, null);
                MessageBox.Show("Biograf slettet.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Reset();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                MessageBox.Show($"Kunne ikke slette biograf.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Reset()
    {
        _currentId = null;
        Name = string.Empty;
        Location = string.Empty;
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