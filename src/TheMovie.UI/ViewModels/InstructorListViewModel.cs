using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class InstructorListViewModel : ListViewModelBase<IInstructorRepository, InstructorListItemViewModel>
{
    public InstructorListViewModel(IInstructorRepository repository) : base(repository)
    {
    }

    public override async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var instructors = await _repository.GetAllAsync().ConfigureAwait(true);
            Items.Clear();
            foreach (var i in instructors.OrderBy(i => i.Name))
                Items.Add(new InstructorListItemViewModel(i));
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
            (RefreshCommandState as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

}