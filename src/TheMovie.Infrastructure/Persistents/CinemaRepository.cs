using System.Text;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.Infrastructure.Persistents;

public class CinemaRepository : RepositoryBase<Cinema>, ICinemaRepository
{
    public CinemaRepository() : base()
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
                    if (parts.Length < 2) continue;
                    if (!Guid.TryParse(parts[0], out var id)) continue;
                    if (string.IsNullOrWhiteSpace(parts[1])) continue;
                    if (string.IsNullOrWhiteSpace(parts[2]) || parts[2] == string.Empty) continue;
                    var cinema = new Cinema { Id = id, Name = parts[1], Location = parts[2] };
                    UpsertInMemory(cinema);
                    loaded++;
                }
                Console.WriteLine($"Loaded {loaded} cinemas.");
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
                    foreach (var cinema in Items)
                    {
                        var line = $"{cinema.Id},{cinema.Name},{cinema.Location}";
                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }

                // Replace atomically
                if (File.Exists(filePath))
                {
                    File.Replace(tempPath, filePath, null);
                }
                else
                {
                    File.Move(tempPath, filePath);
                }
                await Task.CompletedTask;
            }).ConfigureAwait(false);

        }
        finally
        {
            _ioLock.Release();
        }
    }
}
