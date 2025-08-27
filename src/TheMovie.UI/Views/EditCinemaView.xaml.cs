using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class EditCinemaView : Page
{
    private CinemaListViewModel? _listVm;
    private CinemaViewModel? _vm;

    public EditCinemaView()
    {
        InitializeComponent();

        var vm = App.HostInstance.Services.GetRequiredService<CinemaViewModel>();
        DataContext = vm;
        _vm = vm;

        _listVm = App.HostInstance.Services.GetRequiredService<CinemaListViewModel>();
        CinemasListControl.DataContext = _listVm;

        if (_listVm is not null)
            _listVm.PropertyChanged += ListVm_PropertyChanged;

        vm.CinemaSaved += (_, __) =>
        {
            if (_listVm?.RefreshCommandState is { } cmd && cmd.CanExecute(null))
                cmd.Execute(null);
        };

        Loaded += async (_, __) =>
        {
            if (_listVm is not null)
                await _listVm.RefreshAsync();
        };
    }

    private async void ListVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CinemaListViewModel.SelectedItem)
            && _listVm?.SelectedItem is { } item
            && _vm is not null)
        {
            await _vm.LoadAsync(item.Id);
        }
    }
}