using System.Collections.ObjectModel;

namespace TheMovie.UI.ViewModels;

public sealed class HallScreeningsViewModel
{
    public string CinemaName { get; }
    public string HallName { get; }
    public string Header => $"{CinemaName} — {HallName}";
    public ObservableCollection<ScreeningsListItemViewModel> Items { get; } = [];

    public HallScreeningsViewModel(string cinemaName, string hallName, IEnumerable<ScreeningsListItemViewModel> items)
    {
        CinemaName = cinemaName;
        HallName = hallName;
        foreach (var item in items) Items.Add(item);
    }
}