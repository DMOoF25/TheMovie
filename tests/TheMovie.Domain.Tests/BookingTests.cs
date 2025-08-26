using System.ComponentModel.DataAnnotations;
using TheMovie.Domain.Entities;

namespace TheMovie.Domain.Tests;

[TestClass]
public class BookingTests
{
    [TestMethod]
    public void DefaultCtor_Initializes_Defaults()
    {
        var b = new Booking();

        Assert.AreEqual(Guid.Empty, b.Id, "Default ctor should not generate Id.");
        Assert.AreEqual(Guid.Empty, b.ScreeningId, "Default ctor should not set ScreeningId.");
        Assert.AreEqual<uint>(0, b.NumberOfSeats, "Default ctor should not set NumberOfSeats.");
        Assert.AreEqual(null, b.Email, "Default ctor should not set Email.");
        Assert.AreEqual(null, b.PhoneNumber, "Default ctor should not set PhoneNumber.");

        var results = Validate(b);
        Assert.AreEqual(0, results.Count, string.Join("; ", results.Select(r => r.ErrorMessage)));

    }

    [TestMethod]
    public void Ctor_Assigns_Properties_And_Generates_Id()
    {
        var screeningId = Guid.NewGuid();

        var b = new Booking(screeningId, numberOfSeats: 3, email: "user@example.com", phoneNumber: "+1 555-123-4567");

        Assert.AreNotEqual(Guid.Empty, b.Id);
        Assert.AreEqual(screeningId, b.ScreeningId);
        Assert.AreEqual<uint>(3, b.NumberOfSeats);
        Assert.AreEqual("user@example.com", b.Email);
        Assert.AreEqual("+1 555-123-4567", b.PhoneNumber);
    }

    [DataTestMethod]
    [DataRow("plainaddress")]
    [DataRow("user@")]
    [DataRow("@example.com")]
    [DataRow("user@example")]
    [DataRow("user@.com")]
    public void Validation_Fails_For_Invalid_Email(string badEmail)
    {
        var b = new Booking
        {
            ScreeningId = Guid.NewGuid(),
            NumberOfSeats = 1,
            Email = badEmail,
            PhoneNumber = "+1 555-123-4567"
        };

        var results = Validate(b);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(Booking.Email))),
            "Expected an Email validation error.");
    }

    [DataTestMethod]
    [DataRow("+4512131416")]
    [DataRow("(45)12131416")]
    [DataRow("004512131416")]
    [DataRow("+45 12-131416")]
    [DataRow("12131416")]
    public void Validation_Passes_For_Common_Phone_Format(string phoneNumber)
    {
        var b = new Booking
        {
            ScreeningId = Guid.NewGuid(),
            NumberOfSeats = 1,
            Email = "user@example.com",
            PhoneNumber = phoneNumber
        };

        var results = Validate(b);
        Assert.AreEqual(0, results.Count, string.Join("; ", results.Select(r => r.ErrorMessage)));
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var ctx = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, ctx, results, validateAllProperties: true);
        return results;
    }
}