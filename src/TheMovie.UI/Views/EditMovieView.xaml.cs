using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class EditMovieView : Page
{
    private static readonly Regex _digits = new(@"^[0-9]+$", RegexOptions.Compiled);
    private MoviesListViewModel? _moviesListVm;

    public EditMovieView()
    {
        InitializeComponent();

        var vm = App.HostInstance.Services.GetRequiredService<MovieViewModel>();
        DataContext = vm;

        _moviesListVm = App.HostInstance.Services.GetRequiredService<MoviesListViewModel>();
        MoviesListControl.DataContext = _moviesListVm;

        // Auto-refresh list after save
        vm.MovieSaved += (_, __) =>
        {
            if (_moviesListVm?.RefreshCommand is { } cmd && cmd.CanExecute(null))
                cmd.Execute(null);
        };

        // Ensure initial refresh after the page is loaded
        Loaded += async (_, __) =>
        {
            if (_moviesListVm is not null)
                await _moviesListVm.RefreshAsync();
        };
    }

    private void Duration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        => e.Handled = !_digits.IsMatch(e.Text);

    private void GenresComboItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ComboBoxItem cbi && cbi.DataContext is { } dataContext)
        {
            var prop = dataContext.GetType().GetProperty("IsSelected");
            if (prop is not null && prop.PropertyType == typeof(bool))
            {
                bool current = (bool)prop.GetValue(dataContext)!;
                prop.SetValue(dataContext, !current);
            }
            e.Handled = true; // keep dropdown open
        }
    }

    private void GenresCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.Items.Count > 0)
        {
            // Ensure the dropdown remains open after selection
            combo.IsDropDownOpen = true;
        }
    }
}