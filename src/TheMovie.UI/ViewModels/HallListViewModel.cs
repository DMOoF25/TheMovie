using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public class HallListViewModel : ListViewModelBase<IHallRepository, HallListItemViewModel>
{
    public HallListViewModel(IHallRepository repository) : base(repository)
    {
    }

    public override async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var halls = await _repository.GetAllAsync().ConfigureAwait(true);
            Items.Clear();
            foreach (var m in halls.OrderBy(m => m.Name))
                Items.Add(new HallListItemViewModel(m));
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
