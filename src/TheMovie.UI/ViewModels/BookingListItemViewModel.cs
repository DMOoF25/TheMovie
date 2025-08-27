using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;

namespace TheMovie.UI.ViewModels;

public class BookingListItemViewModel : INotifyPropertyChanged
{
    public Guid Id { get; }

    public string ScreeningIdDisplay { get; }

    public string Email { get; }
    public string PhoneNumber { get; }
    public uint NumberOfSeats { get; }
    public BookingListItemViewModel(Guid id, Guid screeningId, string email, string phoneNumber, uint numberOfSeats)
    {
        var repoScreening = App.HostInstance.Services.GetRequiredService<IScreeningRepository>();
        var repoMovie = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        var repoHall = App.HostInstance.Services.GetRequiredService<IHallRepository>();
        var screening = repoScreening.GetByIdAsync(screeningId).Result;

        Id = id;
        Email = email;
        PhoneNumber = phoneNumber;
        NumberOfSeats = numberOfSeats;

        var movie = repoMovie.GetByIdAsync(screening?.MovieId ?? Guid.Empty).Result;
        var hall = repoHall.GetByIdAsync(screening?.HallId ?? Guid.Empty).Result;
        var cinema = repoHall.GetByIdAsync(hall?.CinemaId ?? Guid.Empty).Result;

        ScreeningIdDisplay = $"{cinema?.Name ?? "Unknown Cinema"} - {hall?.Name ?? "Unknown Hall"} - {movie?.Title ?? "Unknown Movie"} - {screening?.StartTime.ToString("g") ?? "Unknown Time"}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
