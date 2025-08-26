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
        // Navigate to the front page on startup
        //MainFrame.Navigate(new MainPageView());
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
    private void MenuItemEditHall_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new EditHallView());
    }

    private void MenuItemEditInstructor_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new EditInstructorView());
    }

    private void MenuItemEditSchedule_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new EditScreeningView());
    }

    private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Biograf app" +
                        "\n\nRegistrering af film." +
                        "\nPlanlægning af filmvisninger." +
                        "\nBooking af billetter." +
                        "\n\nDeveloped by Team 4" +
                        "\n\n",
                        "Om Biograf app",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
    }
    #endregion
}