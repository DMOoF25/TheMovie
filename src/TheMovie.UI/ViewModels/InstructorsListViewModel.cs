using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class InstructorsListViewModel : INotifyPropertyChanged
{
    private readonly IInstructorRepository _repository;
    private bool _isLoading;
    private string? _error;
    private InstructorListItemViewModel? _selectedInstructor;

    public ObservableCollection<InstructorListItemViewModel> Instructors { get; } = new();
    public ICommand RefreshCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set { if (_isLoading == value) return; _isLoading = value; OnPropertyChanged(); }
    }

    public string? Error
    {
        get => _error;
        private set
        {
            if (_error == value) return;
            _error = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(Error);

    public InstructorListItemViewModel? SelectedInstructor
    {
        get => _selectedInstructor;
        set
        {
            if (_selectedInstructor == value) return;
            _selectedInstructor = value;
            OnPropertyChanged();
        }
    }

    public InstructorsListViewModel(IInstructorRepository repository)
    {
        _repository = repository;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsLoading);
        _ = RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var instructors = await _repository.GetAllAsync().ConfigureAwait(true);
            Instructors.Clear();
            foreach (var i in instructors.OrderBy(i => i.Name))
                Instructors.Add(new InstructorListItemViewModel(i));
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