using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class EditCinemaView : Page
{
    private CinemasListViewModel? _listVm;
    private CinemaViewModel? _vm;

    public EditCinemaView()
    {
        InitializeComponent();

        var vm = App.HostInstance.Services.GetRequiredService<CinemaViewModel>();
        DataContext = vm;
        _vm = vm;

        _listVm = App.HostInstance.Services.GetRequiredService<CinemasListViewModel>();
        CinemasListControl.DataContext = _listVm;

        if (_listVm is not null)
            _listVm.PropertyChanged += ListVm_PropertyChanged;

        vm.CinemaSaved += (_, __) =>
        {
            if (_listVm is not null)
                _ = _listVm.RefreshAsync();
        };

        Loaded += async (_, __) =>
        {
            if (_listVm is not null)
                await _listVm.RefreshAsync();
        };
    }

    private async void ListVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CinemasListViewModel.SelectedCinema)
            && _listVm?.SelectedCinema is { } item
            && _vm is not null)
        {
            await _vm.LoadAsync(item.Id);
        }
    }
}