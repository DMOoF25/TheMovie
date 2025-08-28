using TheMovie.Application.Abstractions;
using TheMovie.UI.Commands;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class BookingListViewModel : ListViewModelBase<IBookingRepository, BookingListItemViewModel>
{
    public BookingListViewModel(IBookingRepository repository) : base(repository)
    {
    }

    public override async Task RefreshAsync()
    {
        Error = null;
        IsLoading = true;
        try
        {
            var bookings = await _repository.GetAllAsync().ConfigureAwait(true);
            Items.Clear();
            foreach (var b in bookings.OrderBy(b => b.ScreeningId))
                Items.Add(new BookingListItemViewModel(b));
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
