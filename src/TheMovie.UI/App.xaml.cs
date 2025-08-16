using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheMovie.UI.ViewModels;
using TheMovie.UI.Views;

namespace TheMovie.UI;

public partial class App : System.Windows.Application
{
    public static IHost HostInstance { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddMovieApplication()
                    .AddMovieInfrastructure();

                services.AddTransient<AddMovieViewModel>();
                services.AddTransient<AddMovieView>();
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