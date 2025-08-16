using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Infrastructure.Data;
using TheMovie.UI.Converters;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

/// <summary>
/// Interaction logic for AddMovie.xaml
/// </summary>
public partial class AddMovieView : Page
{
    private static readonly Regex _digits = new(@"^[0-9]+$", RegexOptions.Compiled);

    // Parameterless ctor required by WPF (navigation/designer). 
    // Creates its own in-memory repository instance (non-persistent).
    public AddMovieView() : this(ResolveViewModel())
    {
    }

    public AddMovieView(MovieViewModel vm)
    {
        InitializeComponent();
        // NullToVisibilityConverter defined once in App resources if not already.
        if (System.Windows.Application.Current.Resources["NullToVisibilityConverter"] is null)
            System.Windows.Application.Current.Resources["NullToVisibilityConverter"] = new NullToVisibilityConverter();

        DataContext = vm;

    }

    private static MovieViewModel ResolveViewModel()
    {
        // Use DI (host built during normal runtime)
        if (App.HostInstance is not null)
            return App.HostInstance.Services.GetRequiredService<MovieViewModel>();

        // Designer / fallback (no host yet)
        return new MovieViewModel(new MovieRepository());
    }

    private void Duration_PreviewTextInput(object sender, TextCompositionEventArgs e)
    => e.Handled = !_digits.IsMatch(e.Text);
}
