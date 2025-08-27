using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheMovie.Application;
using TheMovie.Infrastructure;
using TheMovie.UI.Converters;
using TheMovie.UI.ViewModels;
using TheMovie.UI.Views;

namespace TheMovie.UI;

public partial class App : System.Windows.Application
{
    public static IHost HostInstance { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Register global resources early (before any windows/pages are created)
        if (Resources["NullToVisibilityConverter"] is null)
            Resources["NullToVisibilityConverter"] = new NullToVisibilityConverter();

        if (Resources["BoolToVis"] is null)
            Resources["BoolToVis"] = new BooleanToVisibilityConverter();

        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddMovieApplication()
                    .AddMovieInfrastructure();

                // Movies
                services.AddTransient<MovieListViewModel>();
                services.AddTransient<MovieViewModel>();
                services.AddTransient<EditMovieView>();

                // Cinemas
                services.AddTransient<CinemasListViewModel>();
                services.AddTransient<CinemaViewModel>();
                services.AddTransient<EditCinemaView>();

                // Instructors
                services.AddTransient<InstructorsListViewModel>();
                services.AddTransient<InstructorViewModel>();
                services.AddTransient<EditInstructorView>();

                // Halls
                services.AddTransient<HallsListViewModel>();
                services.AddTransient<HallViewModel>();
                services.AddTransient<EditHallView>();

                // Screenings
                services.AddTransient<ScreeningsListViewModel>();
                services.AddTransient<ScreeningViewModel>();
                services.AddTransient<EditScreeningView>();

                // Bookings
                services.AddTransient<BookingsListViewModel>();
                services.AddTransient<BookingViewModel>();
                services.AddTransient<EditBookingWindow>();

                services.AddSingleton<MainWindow>();
            })
            .Build();

        HostInstance.Services.GetRequiredService<MainWindow>().Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (HostInstance is IAsyncDisposable asyncDisp)
            await asyncDisp.DisposeAsync();
        else
            HostInstance.Dispose();
        base.OnExit(e);
    }
}