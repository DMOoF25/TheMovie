using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class EditInstructorView : Page
{
    private InstructorListViewModel? _listVm;
    private InstructorViewModel? _vm;

    public EditInstructorView()
    {
        InitializeComponent();

        var vm = App.HostInstance.Services.GetRequiredService<InstructorViewModel>();
        DataContext = vm;
        _vm = vm;

        _listVm = App.HostInstance.Services.GetRequiredService<InstructorListViewModel>();
        InstructorsListControl.DataContext = _listVm;

        if (_listVm is not null)
            _listVm.PropertyChanged += ListVm_PropertyChanged;

        vm.InstructorSaved += (_, __) =>
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
        if (e.PropertyName == nameof(InstructorListViewModel.SelectedItem)
            && _listVm?.SelectedItem is { } item
            && _vm is not null)
        {
            await _vm.LoadAsync(item.Id);
        }
    }
}