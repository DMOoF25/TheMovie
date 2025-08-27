using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class ScreeningListViewModel : ListViewModelBase<IScreeningRepository, ScreeningListItemViewModel>
{

    public ScreeningListViewModel(IScreeningRepository repository) : base(repository)
    {
    }

    public override async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var screenings = await _repository.GetAllAsync().ConfigureAwait(true);

            var ordered = screenings
                .Select(s => (Item: new ScreeningListItemViewModel(s), Start: s.StartTime))
                .OrderBy(t => t.Item.CinemaNameDisplay)
                .ThenBy(t => t.Item.HallNameDisplay)
                .ThenBy(t => t.Start);

            Items.Clear();
            foreach (var t in ordered)
                Items.Add(t.Item);
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