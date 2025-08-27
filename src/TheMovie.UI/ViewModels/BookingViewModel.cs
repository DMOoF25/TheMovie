using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.Commands;

namespace TheMovie.UI.ViewModels;

public sealed class BookingViewModel : INotifyPropertyChanged
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly ICinemaRepository _cinemaRepository;

    public event EventHandler? CloseRequested;

    // Screening context
    public Guid ScreeningId { get; private set; }
    private Screening? _screening;

    public string CinemaName { get; private set; } = string.Empty;
    public string HallName { get; private set; } = string.Empty;
    public string MovieTitle { get; private set; } = string.Empty;
    public string StartDisplay { get; private set; } = string.Empty;
    public uint HallCapacity { get; private set; } = 0;
    public uint AvailableSeats { get; private set; } = 0;

    // Booking inputs
    private string _numberOfSeatsText = "1";
    public string NumberOfSeatsText
    {
        get => _numberOfSeatsText;
        set { if (_numberOfSeatsText == value) return; _numberOfSeatsText = value; OnPropertyChanged(); RefreshCommands(); }
    }

    private string? _email;
    public string? Email
    {
        get => _email;
        set { if (_email == value) return; _email = value; OnPropertyChanged(); RefreshCommands(); }
    }

    private string? _phoneNumber;
    public string? PhoneNumber
    {
        get => _phoneNumber;
        set { if (_phoneNumber == value) return; _phoneNumber = value; OnPropertyChanged(); RefreshCommands(); }
    }

    private string? _error;
    public string? Error
    {
        get => _error;
        private set { if (_error == value) return; _error = value; OnPropertyChanged(); }
    }

    public ICommand IncreaseSeatsCommand { get; }
    public ICommand DecreaseSeatsCommand { get; }
    public ICommand BookCommand { get; }
    public ICommand CancelCommand { get; }

    public BookingViewModel(ScreeningsListItemViewModel? selected = default)
    {
        _bookingRepository = App.HostInstance.Services.GetRequiredService<IBookingRepository>();
        _screeningRepository = App.HostInstance.Services.GetRequiredService<IScreeningRepository>();
        _movieRepository = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        _hallRepository = App.HostInstance.Services.GetRequiredService<IHallRepository>();
        _cinemaRepository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();

        IncreaseSeatsCommand = new RelayCommand(() => AdjustSeats(1));
        DecreaseSeatsCommand = new RelayCommand(() => AdjustSeats(-1));
        BookCommand = new RelayCommand(async () => await OnBookAsync(), CanBook);
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(this, EventArgs.Empty));

        HallCapacity = selected?.GetCapacity() ?? 0;
    }

    private void AdjustSeats(int delta)
    {
        if (!uint.TryParse(NumberOfSeatsText, out var seats)) seats = 1;
        var next = (int)seats + delta;
        if (next < 1) next = 1;
        if (next > 999) next = 999;
        NumberOfSeatsText = next.ToString();
    }

    public async Task LoadForScreeningAsync(Guid screeningId)
    {
        Error = null;
        ScreeningId = screeningId;

        _screening = await _screeningRepository.GetByIdAsync(ScreeningId).ConfigureAwait(true);
        if (_screening is null)
        {
            Error = "Screening not found.";
            return;
        }

        var movie = await _movieRepository.GetByIdAsync(_screening.MovieId).ConfigureAwait(true);
        var hall = await _hallRepository.GetByIdAsync(_screening.HallId).ConfigureAwait(true);
        Cinema? cinema = null;
        if (hall is not null)
            cinema = await _cinemaRepository.GetByIdAsync(hall.CinemaId).ConfigureAwait(true);

        MovieTitle = movie?.Title ?? "(unknown)";
        HallName = hall?.Name ?? "(unknown)";
        CinemaName = cinema?.Name ?? "(unknown)";
        StartDisplay = _screening.StartTime.ToString("dd-MM-yyyy HH:mm");

        OnPropertyChanged(nameof(MovieTitle));
        OnPropertyChanged(nameof(HallName));
        OnPropertyChanged(nameof(CinemaName));
        OnPropertyChanged(nameof(StartDisplay));
        OnPropertyChanged(nameof(HallCapacity));
        OnPropertyChanged(nameof(AvailableSeats));

        RefreshCommands();
    }

    private bool CanBook()
    {
        if (ScreeningId == Guid.Empty) return false;
        if (!uint.TryParse(NumberOfSeatsText, out var seats) || seats == 0) return false;
        // Require at least one contact method
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber)) return false;
        return true;
    }

    private async Task OnBookAsync()
    {
        if (!uint.TryParse(NumberOfSeatsText, out var seats) || seats == 0)
        {
            Error = "Enter a valid number of seats.";
            return;
        }

        try
        {
            Error = null;
            var booking = new Booking(ScreeningId, seats, Email?.Trim(), PhoneNumber?.Trim());
            await _bookingRepository.AddAsync(booking).ConfigureAwait(true);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    private void RefreshCommands()
    {
        (BookCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
