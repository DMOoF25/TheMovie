using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EmailAddressAttribute = TheMovie.Domain.Validations.EmailAddressAttribute;

namespace TheMovie.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid ScreeningId { get; set; }
    public uint NumberOfSeats { get; set; } = 0;
    [AllowNull]
    [EmailAddress]
    public string? Email { get; set; } = null;
    [AllowNull]
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; } = null;

    public Booking() { }
    public Booking(Guid screeningId, uint numberOfSeats, string? email = default, string? phoneNumber = default)
    {
        Id = Guid.NewGuid();
        ScreeningId = screeningId;
        NumberOfSeats = numberOfSeats;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}
