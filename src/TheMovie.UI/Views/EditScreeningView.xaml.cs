using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class EditScreeningView : Page
{
    private ScreeningsListViewModel? _screeningsListVm;
    private ScreeningViewModel? _vm;

    public EditScreeningView()
    {
        InitializeComponent();

        _vm = App.HostInstance.Services.GetRequiredService<ScreeningViewModel>();
        DataContext = _vm;

        _screeningsListVm = App.HostInstance.Services.GetRequiredService<ScreeningsListViewModel>();

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