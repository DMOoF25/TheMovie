using System.Text;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.Infrastructure.Persistents;

public class BookingRepository : RepositoryBase<Booking>, IBookingRepository
{
    private int _numberOfColumns = 5; // Id, ScreeningId, NumberOfSeats, Email, PhoneNumber

    public BookingRepository() : base()
    {
    }

    protected override async Task LoadFromCvsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await _ioLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;
            await WithRetriesAsync(async () =>
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, Encoding.UTF8);
                string? line;
                var loaded = 0;
                string email = string.Empty;
                string phoneNumber = string.Empty;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var parts = line.Split(',');
                    uint i = 0;
                    if (parts.Length < _numberOfColumns) continue;
                    if (!Guid.TryParse(parts[i], out var id)) continue;
                    if (!Guid.TryParse(parts[++i], out var screeningId)) continue;
                    if (!uint.TryParse(parts[++i], out var numberOfSeats)) continue;
                    if (string.IsNullOrWhiteSpace(parts[++i]) || parts[3] == string.Empty) continue;
                    else email = parts[i].Trim();
                    if (string.IsNullOrWhiteSpace(parts[++i]) || parts[4] == string.Empty) continue;
                    else phoneNumber = parts[i].Trim();
                    var booking = new Booking { Id = id, ScreeningId = screeningId, NumberOfSeats = numberOfSeats, Email = email, PhoneNumber = phoneNumber };
                    UpsertInMemory(booking);
                    loaded++;
                }
                Console.WriteLine($"Loaded {loaded} bookings.");
            }).ConfigureAwait(false);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    protected override async Task SaveToCvsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await _ioLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            var tempPath = filePath + ".tmp";
            await WithRetriesAsync(async () =>
            {
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var booking in Items)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var line = $"{booking.Id},{booking.ScreeningId},{booking.NumberOfSeats},{booking.Email}, {booking.PhoneNumber}";
                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }
                // Replace original file with the temp file
                if (File.Exists(filePath))
                    File.Delete(filePath);
                File.Move(tempPath, filePath);

            }).ConfigureAwait(false);
        }
        finally
        {
            _ioLock.Release();
        }

    }
}
