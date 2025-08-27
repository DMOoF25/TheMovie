using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class MainPageView : Page
{
    public MainPageView()
    {
        InitializeComponent();

        // Prefer DI if registered, otherwise create directly
        var vm = App.HostInstance.Services.GetService<MainPageViewModel>() ?? new MainPageViewModel();
        DataContext = vm;

        Loaded += async (_, __) => await vm.RefreshAsync();
    }

    private void ScreeningsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainPageViewModel vm) return;
        if (vm.SelectedScreening is null) return;

        if (vm.OpenBookingCommand.CanExecute(null))
        {
            vm.OpenBookingCommand.Execute(null);
            // Clear selection so selecting the same row again reopens the window
            if (sender is ListView lv) lv.SelectedItem = null;
        }
    }
}