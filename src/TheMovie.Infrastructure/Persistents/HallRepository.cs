using System.Text;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.Infrastructure.Persistents;

public class HallRepository : RepositoryBase<Hall>, IHallRepository
{
    public HallRepository() : base()
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
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var parts = line.Split(',');
                    if (parts.Length < 3) continue;
                    if (!Guid.TryParse(parts[0], out var id)) continue;
                    if (string.IsNullOrWhiteSpace(parts[1])) continue;
                    if (!Guid.TryParse(parts[2], out var cinemaId)) continue;
                    var hall = new Hall { Id = id, Name = parts[1], CinemaId = cinemaId };
                    UpsertInMemory(hall);
                    loaded++;
                }
                Console.WriteLine($"Loaded {loaded} halls.");
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
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            var tempPath = filePath + ".tmp";
            await WithRetriesAsync(async () =>
            {
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var hall in Items)
                    {
                        var line = $"{hall.Id},{hall.Name},{hall.CinemaId}";
                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }
                File.Move(tempPath, filePath, true);
            }).ConfigureAwait(false);
        }
        finally
        {
            _ioLock.Release();
        }
    }
}
