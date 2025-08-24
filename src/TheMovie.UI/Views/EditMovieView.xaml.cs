using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class EditMovieView : Page
{
    private static readonly Regex _digits = new(@"^[0-9]+$", RegexOptions.Compiled);
    private MoviesListViewModel? _listVm;
    private MovieViewModel? _vm;

    public EditMovieView()
    {
        InitializeComponent();

        var vm = App.HostInstance.Services.GetRequiredService<MovieViewModel>();
        _vm = vm;                 // FIX: assign to field instead of shadowing
        DataContext = vm;

        _listVm = App.HostInstance.Services.GetRequiredService<MoviesListViewModel>();
        MoviesListControl.DataContext = _listVm;

        // Populate form when a movie is selected in the list
        if (_listVm is not null)
            _listVm.PropertyChanged += ListVm_PropertyChanged;

        // Auto-refresh list after save
        _vm.MovieSaved += (_, __) =>
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
        if (e.PropertyName == nameof(MoviesListViewModel.SelectedMovie)
            && _listVm?.SelectedMovie is { } item
            && _vm is not null)
        {
            await _vm.LoadAsync(item.Id);
        }
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