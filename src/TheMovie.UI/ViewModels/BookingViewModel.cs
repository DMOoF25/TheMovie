using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.UI.Commands;
using TheMovie.UI.ViewModels.Abstractions;

namespace TheMovie.UI.ViewModels;

public sealed class BookingViewModel : ViewModelBase<IBookingRepository, Booking>
{
    // To track current entity in edit mode
    private readonly Guid? _currentId = null;

    //private readonly IBookingRepository _repository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly ICinemaRepository _cinemaRepository;

    // Screening context
    public Guid ScreeningId { get; private set; }
    private Screening? _screening;

    // Form fields
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
        set { if (_numberOfSeatsText == value) return; _numberOfSeatsText = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    private string? _email;
    public string? Email
    {
        get => _email;
        set { if (_email == value) return; _email = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    private string? _phoneNumber;
    public string? PhoneNumber
    {
        get => _phoneNumber;
        set { if (_phoneNumber == value) return; _phoneNumber = value; OnPropertyChanged(); RefreshCommandStates(); }
    }

    public ICommand IncreaseSeatsCommand { get; }
    public ICommand DecreaseSeatsCommand { get; }

    public event EventHandler? CloseRequested;

    public BookingViewModel(ScreeningListItemViewModel? selected = default)
        : base(App.HostInstance.Services.GetRequiredService<IBookingRepository>())
    {
        _screeningRepository = App.HostInstance.Services.GetRequiredService<IScreeningRepository>();
        _movieRepository = App.HostInstance.Services.GetRequiredService<IMovieRepository>();
        _hallRepository = App.HostInstance.Services.GetRequiredService<IHallRepository>();
        _cinemaRepository = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();

        IncreaseSeatsCommand = new RelayCommand(() => AdjustSeats(1));
        DecreaseSeatsCommand = new RelayCommand(() => AdjustSeats(-1));

        HallCapacity = (selected != null) ? selected.GetCapacity() : 0;

        if (selected != null)
        {
            _ = LoadForScreeningAsync(selected.Id);
        }
    }

    private void AdjustSeats(int delta)
    {
        if (!uint.TryParse(NumberOfSeatsText, out var seats)) seats = 1;
        var next = (int)seats + delta;
        if (next < 1) next = 1;
        if (next > 999) next = 999;
        NumberOfSeatsText = next.ToString();
    }

    #region Load methods

    // Treat id as ScreeningId for this ViewModel
    public override async Task LoadAsync(Guid id) => await LoadForScreeningAsync(id);

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
        HallCapacity = hall?.Capacity ?? 0;

        // Calculate available seats (hall capacity minus booked seats)
        try
        {
            var bookings = await _repository.GetAllAsync().ConfigureAwait(true);
            uint booked = (uint)bookings.Where(b => b.ScreeningId == ScreeningId).Sum(b => b.NumberOfSeats);
            AvailableSeats = (booked >= HallCapacity) ? 0 : (HallCapacity - booked);
        }
        catch
        {
            AvailableSeats = 0;
        }

        OnPropertyChanged(nameof(MovieTitle));
        OnPropertyChanged(nameof(HallName));
        OnPropertyChanged(nameof(CinemaName));
        OnPropertyChanged(nameof(StartDisplay));
        OnPropertyChanged(nameof(HallCapacity));
        OnPropertyChanged(nameof(AvailableSeats));

        RefreshCommandStates();
    }

    #endregion

    #region CanXXX methods
    protected override bool CanSubmitCore()
    {
        if (ScreeningId == Guid.Empty) return false;
        if (!uint.TryParse(NumberOfSeatsText, out var seats) || seats == 0) return false;
        // Require at least one contact method
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber)) return false;
        return true;
    }
    #endregion

    #region Commands
    // Base AddCommand will call this
    protected override async Task OnAddAsync()
    {
        if (!CanSubmitCore())
        {
            Error = "Please fill in seats and a contact (email or phone).";
            return;
        }
        if (!uint.TryParse(NumberOfSeatsText, out var seats) || seats == 0)
        {
            Error = "Enter a valid number of seats.";
            return;
        }

        try
        {
            Error = null;
            IsSaving = true;
            var booking = new Booking(ScreeningId, seats, Email?.Trim(), PhoneNumber?.Trim());
            await _repository.AddAsync(booking).ConfigureAwait(true);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsSaving = false;
            RefreshCommandStates();
        }
    }

    protected override async Task OnSaveAsync()
    {
        if (_currentId is null) return;


        await Task.CompletedTask;
    }

    protected override async Task OnDeleteAsync()
    {
        if (_currentId is null) return;
        await Task.CompletedTask;
    }

    protected override async Task OnResetAsync()
    {
        // OnResetAsync fields and request close so Cancel behaves as close
        NumberOfSeatsText = "1";
        Email = null;
        PhoneNumber = null;
        Error = null;
        CloseRequested?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }
    #endregion

}
