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
    private void MenuItemEditMovie_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new EditMovieView());
    }
    private void MenuItemEditCinema_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new EditCinemaView());
    }

    private void MenuItemEditInstructor_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new EditInstructorView());
    }

    private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("The Movie App" +
                        "\n\nA simple WPF application to manage movies." +
                        "\n\nDeveloped by Team 4" +
                        "\n\n",
                        "Om The Movie App",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
    }
    #endregion
}