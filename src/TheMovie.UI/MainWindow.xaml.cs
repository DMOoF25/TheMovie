using System.Windows;
using TheMovie.UI.Views;

namespace TheMovie.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    #region Menu bar
    private void MenuItemExit_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }
    private void MenuItemAddMovie_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new AddMovieView());
    }
    private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("The Movie App\nVersion 1.0\n© 2025 Team 4", "Om The Movie App", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    #endregion
}