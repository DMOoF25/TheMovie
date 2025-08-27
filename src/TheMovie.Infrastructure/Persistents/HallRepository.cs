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
                string name;
                uint capacity;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var parts = line.Split(',');
                    var i = 0;
                    //if (parts.Length < 4) continue;
                    if (!Guid.TryParse(parts[0], out var id)) continue;
                    if (string.IsNullOrWhiteSpace(parts[++i])) continue;
                    else name = parts[i];
                    if (!uint.TryParse(parts[++i], out capacity)) continue;
                    if (!Guid.TryParse(parts[++i], out var cinemaId)) continue;
                    var hall = new Hall { Id = id, Name = name, CinemaId = cinemaId, Capacity = capacity };
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
                        var line = $"{hall.Id},{hall.Name}, {hall.Capacity},{hall.CinemaId}";
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
