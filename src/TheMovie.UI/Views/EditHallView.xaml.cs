using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;


/// <summary>
/// Interaction logic for EditHallView.xaml
/// </summary>
public partial class EditHallView : Page
{
    private readonly HallsListViewModel _listVm;
    private readonly HallViewModel _vm;

    public EditHallView()
    {
        InitializeComponent();

        _vm = App.HostInstance.Services.GetRequiredService<HallViewModel>();
        DataContext = _vm;

        _listVm = App.HostInstance.Services.GetRequiredService<HallsListViewModel>();
        HallsListControl.DataContext = _listVm;

        if (_listVm is not null)
            _listVm.PropertyChanged += ListVm_PropertyChanged;

        // Auto-refresh list after save
        _vm.HallSaved += (_, __) =>
        {
            if (_listVm?.RefreshCommand is { } cmd && cmd.CanExecute(null))
                cmd.Execute(null);
        };

        // Ensure initial refresh after the page is loaded
        Loaded += async (_, __) =>
        {
            if (_listVm is not null)
                await _listVm.RefreshAsync();
        };
    }

    private async void ListVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HallsListViewModel.SelectedHall)
            && _listVm?.SelectedHall is { } item
            && _vm is not null)
        {
            await _vm.LoadAsync(item.Id);
        }
    }
}
