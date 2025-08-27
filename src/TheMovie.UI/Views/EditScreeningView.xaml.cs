using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class EditScreeningView : Page
{
    private ScreeningListViewModel? _screeningsListVm;
    private ScreeningViewModel? _vm;

    public EditScreeningView()
    {
        InitializeComponent();

        _vm = App.HostInstance.Services.GetRequiredService<ScreeningViewModel>();
        DataContext = _vm;

        _screeningsListVm = App.HostInstance.Services.GetRequiredService<ScreeningListViewModel>();
        ScreeningsListControl.DataContext = _screeningsListVm;

        // Populate form when a screening is selected in the list
        if (_screeningsListVm is not null)
            _screeningsListVm.PropertyChanged += ListVm_PropertyChanged;

        // Auto-refresh list after save/delete
        if (_vm is not null)
        {
            _vm.ScreeningSaved += async (_, __) =>
            {
                if (_screeningsListVm is not null)
                    await _screeningsListVm.RefreshAsync();
            };
        }

        Loaded += async (_, __) =>
        {
            if (_screeningsListVm is not null)
                await _screeningsListVm.RefreshAsync();
        };
    }

    private async void ListVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScreeningListViewModel.SelectedItem)
            && _screeningsListVm?.SelectedItem is { } item
            && _vm is not null)
        {
            await _vm.LoadAsync(item.Id);
        }
    }

    // OnAdd this event handler for PreviewTextInput
    private void ScreeningTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow only digits and colon (for time input, e.g., "12:30")
        foreach (char c in e.Text)
        {
            if (!char.IsDigit(c) && c != ':')
            {
                e.Handled = true;
                return;
            }
        }
        e.Handled = false;
    }
}