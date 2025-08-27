using System.ComponentModel;
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
        private BookingListViewModel? _listVm;
        private BookingViewModel? _vm;

        public EditBookingWindow()
        {
            InitializeComponent();

            var vm = App.HostInstance.Services.GetService<BookingViewModel>() ?? new BookingViewModel();
            _vm = vm;
            DataContext = vm;

            _listVm = App.HostInstance.Services.GetRequiredService<BookingListViewModel>();

            // Populate form when a movie is selected in the list
            if (_listVm is not null)
                _listVm.PropertyChanged += ListVm_PropertyChanged;

            _vm.CloseRequested += (_, __) => Close();

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

        private async void ListVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BookingListViewModel.SelectedItem)
                && _listVm?.SelectedItem is { } item
                && _vm is not null)
            {
                await _vm.LoadAsync(item.Id);
            }
        }

        private static readonly Regex Digits = new("^[0-9]+$");
        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Digits.IsMatch(e.Text);
        }
    }
}
