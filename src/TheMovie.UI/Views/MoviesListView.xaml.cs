using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Infrastructure.Data;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Views;

public partial class MoviesListView : UserControl
{
    public MoviesListView() : this(ResolveViewModel())
    {
    }

    public MoviesListView(MoviesListViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    private static MoviesListViewModel ResolveViewModel()
    {
        if (App.HostInstance is not null)
            return App.HostInstance.Services.GetRequiredService<MoviesListViewModel>();

        return new MoviesListViewModel(new MovieRepository());
    }

    public MoviesListViewModel ViewModel => (MoviesListViewModel)DataContext;
}