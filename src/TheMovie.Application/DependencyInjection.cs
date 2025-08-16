using Microsoft.Extensions.DependencyInjection;

namespace TheMovie.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMovieApplication(this IServiceCollection services)
    {
        // Register application services / use cases here when you add them.
        return services;
    }
}