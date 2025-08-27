using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views
{
    /// <summary>
    /// Interaction logic for EditBookingWindow.xaml
    /// </summary>
    public partial class EditBookingWindow : Window
    {
        public EditBookingWindow()
        {
            InitializeComponent();

            var vm = App.HostInstance.Services.GetService<BookingViewModel>() ?? new BookingViewModel();
            DataContext = vm;

            vm.CloseRequested += (_, __) => Close();

            Loaded += async (_, __) =>
            {
                if (DataContext is not BookingViewModel bvm) return;

                // If invoked from MainPage, ScreeningId is passed via Tag
                if (Tag is Guid g)
                    await bvm.LoadForScreeningAsync(g);
                else if (Tag is string s && Guid.TryParse(s, out var g2))
                    await bvm.LoadForScreeningAsync(g2);
            };
        }

        private static readonly Regex Digits = new("^[0-9]+$");
        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Digits.IsMatch(e.Text);
        }
    }
}
