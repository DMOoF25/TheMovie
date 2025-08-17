using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Infrastructure.Data;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class AddMovieView : Page
{
    private static readonly Regex _digits = new(@"^[0-9]+$", RegexOptions.Compiled);
    private MoviesListViewModel? _moviesListVm;

    public AddMovieView() : this(ResolveViewModel())
    {
    }

    public AddMovieView(MovieViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;

        // Resolve list VM (from embedded control once loaded)
        Loaded += (_, _) =>
        {
            _moviesListVm = (MoviesListControl.DataContext as MoviesListViewModel) ?? ResolveMoviesListViewModel();
            vm.MovieSaved += (_, movie) => _moviesListVm?.AddOrUpdate(movie);
        };
    }

    private static MovieViewModel ResolveViewModel()
    {
        if (App.HostInstance is not null)
            return App.HostInstance.Services.GetRequiredService<MovieViewModel>();

        return new MovieViewModel(new MovieRepository());
    }

    private static MoviesListViewModel ResolveMoviesListViewModel()
    {
        if (App.HostInstance is not null)
            return App.HostInstance.Services.GetRequiredService<MoviesListViewModel>();

        return new MoviesListViewModel(new MovieRepository());
    }

    private void Duration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        => e.Handled = !_digits.IsMatch(e.Text);

    private void GenresComboItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ComboBoxItem cbi && cbi.DataContext is { } dataContext)
        {
            // Find the CheckBox inside and toggle it manually without closing the dropdown
            if (cbi.ContentTemplate?.LoadContent() is FrameworkElement fe)
            {
                // Not used (template load creates new instance), so instead toggle via reflection of bound property if possible.
            }

            // Expect bound object has an IsSelected boolean property
            var prop = dataContext.GetType().GetProperty("IsSelected");
            if (prop is not null && prop.PropertyType == typeof(bool))
            {
                bool current = (bool)prop.GetValue(dataContext)!;
                prop.SetValue(dataContext, !current);
            }

            // Prevent ComboBox selection logic (keeps dropdown open)
            e.Handled = true;
        }
    }
}