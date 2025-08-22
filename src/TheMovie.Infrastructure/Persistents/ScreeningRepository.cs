using System.Text;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.Infrastructure.Persistents;

public class ScreeningRepository : RepositoryBase<Screening>, IScreeningRepository
{
    private int _numberOfColumns = 4; // Id, StartTime, MovieId, HallId

    public ScreeningRepository() : base()
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
                    if (parts.Length < _numberOfColumns) continue;
                    if (!Guid.TryParse(parts[0], out var id)) continue;
                    if (!DateTime.TryParse(parts[1], out var startTime)) continue;
                    if (!Guid.TryParse(parts[2], out var movieId)) continue;
                    if (!Guid.TryParse(parts[3], out var hallId)) continue;
                    var screening = new Screening
                    {
                        Id = id,
                        StartTime = startTime,
                        MovieId = movieId,
                        HallId = hallId
                    };
                    UpsertInMemory(screening);
                    loaded++;
                }
                Console.WriteLine($"Loaded {loaded} screenings.");
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
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var tempPath = filePath + ".tmp";
        await WithRetriesAsync(async () =>
        {
            try
            {
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var screening in Items)
                    {
                        var line = $"{screening.Id},{screening.StartTime},{screening.MovieId},{screening.HallId}";
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
            }
            finally
            {
                _ioLock.Release();
            }
        }).ConfigureAwait(false);
    }
}
