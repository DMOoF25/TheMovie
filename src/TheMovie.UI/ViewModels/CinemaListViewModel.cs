using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class CinemaListViewModel : ListViewModelBase<ICinemaRepository, CinemaListItemViewModel>
{
    public CinemaListViewModel(ICinemaRepository repository) : base(repository)
    {
    }

    public override async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var cinemas = await _repository.GetAllAsync().ConfigureAwait(true);
            Items.Clear();
            foreach (var c in cinemas.OrderBy(c => c.Name))
                Items.Add(new CinemaListItemViewModel(c));
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
