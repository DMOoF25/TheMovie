using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.UI.ViewModels;

public class BookingListItemViewModel
{
    public Guid Id { get; }

    public string ScreeningIdDisplay { get; }

    public string? Email { get; }
    public string? PhoneNumber { get; }
    public uint NumberOfSeats { get; }
    public BookingListItemViewModel(Booking booking)
    {
        var repoScreening = App.HostInstance.Services.GetRequiredService<IScreeningRepository>();
        var repoMovie = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        var repoHall = App.HostInstance.Services.GetRequiredService<IHallRepository>();
        var screening = repoScreening.GetByIdAsync(booking.ScreeningId).Result;

        Id = booking.Id;
        Email = booking.Email;
        PhoneNumber = booking.PhoneNumber;
        NumberOfSeats = booking.NumberOfSeats;

        var movie = repoMovie.GetByIdAsync(screening?.MovieId ?? Guid.Empty).Result;
        var hall = repoHall.GetByIdAsync(screening?.HallId ?? Guid.Empty).Result;
        var cinema = repoHall.GetByIdAsync(hall?.CinemaId ?? Guid.Empty).Result;

        ScreeningIdDisplay = $"{cinema?.Name ?? "Unknown Cinema"} - {hall?.Name ?? "Unknown Hall"} - {movie?.Title ?? "Unknown Movie"} - {screening?.StartTime.ToString("g") ?? "Unknown Time"}";
    }

}
