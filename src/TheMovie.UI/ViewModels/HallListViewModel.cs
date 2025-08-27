using System.Collections.ObjectModel;
using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public class HallListViewModel : ListViewModelBase<IHallRepository, HallListItemViewModel>
{
    public ObservableCollection<HallListItemViewModel> Irems { get; } = [];

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
            Irems.Clear();
            foreach (var m in halls.OrderBy(m => m.Name))
                Irems.Add(new HallListItemViewModel(m));
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
