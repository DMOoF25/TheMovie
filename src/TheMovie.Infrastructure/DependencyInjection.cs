using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Infrastructure.Persistents;

namespace TheMovie.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMovieInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository, MovieRepository>();
        services.AddSingleton<ICinemaRepository, CinemaRepository>();
        services.AddSingleton<IInstructorRepository, InstructorRepository>();
        services.AddSingleton<IHallRepository, HallRepository>();
        services.AddSingleton<IScreeningRepository, ScreeningRepository>();
        services.AddSingleton<IBookingRepository, BookingRepository>();
        return services;
    }
}