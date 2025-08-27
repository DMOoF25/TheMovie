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
    private readonly HallListViewModel _listVm = null!;
    private readonly HallViewModel _vm = null!;

    public EditHallView()
    {
        InitializeComponent();

        var vm = App.HostInstance.Services.GetRequiredService<HallViewModel>();
        _vm = vm;                 // FIX: assign to field instead of shadowing
        DataContext = vm;

        _listVm = App.HostInstance.Services.GetRequiredService<HallListViewModel>();
        HallListControl.DataContext = _listVm;

        // Populate form when a hall is selected in the list
        if (_listVm is not null)
            _listVm.PropertyChanged += ListVm_PropertyChanged;

        // Auto-refresh list after save
        _vm.HallSaved += (_, __) =>
        {
            if (_listVm?.RefreshCommandState is { } cmd && cmd.CanExecute(null))
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
        if (e.PropertyName == nameof(HallListViewModel.SelectedItem)
            && _listVm?.SelectedItem is { } item
            && _vm is not null)
        {
            await _vm.LoadAsync(item.Id);
        }
    }
}
