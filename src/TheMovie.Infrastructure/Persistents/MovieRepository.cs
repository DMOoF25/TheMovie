using System.Text;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;

namespace TheMovie.Infrastructure.Persistents;

/// <summary>
/// In-memory Movie repository with simple genre filtering helpers.
/// </summary>
public sealed class MovieRepository : RepositoryBase<Movie>, IMovieRepository
{
    private readonly SemaphoreSlim _ioLock = new(1, 1);

    public MovieRepository() : base()
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
                    if (parts.Length < 4) continue;
                    if (!Guid.TryParse(parts[0], out var id)) continue;
                    if (!int.TryParse(parts[2], out var duration)) continue;
                    if (!Enum.TryParse<Genre>(parts[3], true, out var genres)) continue;

                    var movie = new Movie { Id = id, Title = parts[1], Duration = duration, Genres = genres };
                    UpsertInMemory(movie);
                    loaded++;
                }
                Console.WriteLine($"Loaded {loaded} movies.");
                await Task.CompletedTask;
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
                // Write to temp file
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var movie in Items)
                    {
                        var line = $"{movie.Id},{Escape(movie.Title)},{movie.Duration},{movie.Genres}";
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



    /*
    // Movies that include (at least) the specified single genre flag.
    public Task<IEnumerable<Movie>> ListByGenreAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        var result = Items.Where(m => (m.Genres & genre) == genre).ToList();
        return Task.FromResult<IEnumerable<Movie>>(result);
    }

    // Movies that contain ANY of the provided genre flags.
    public Task<IEnumerable<Movie>> ListByAnyGenresAsync(Genre genres, CancellationToken cancellationToken = default)
    {
        var result = Items.Where(m => (m.Genres & genres) != Genre.None).ToList();
        return Task.FromResult<IEnumerable<Movie>>(result);
    }

    // Movies that contain ALL of the provided genre flags.
    public Task<IEnumerable<Movie>> ListByAllGenresAsync(Genre genres, CancellationToken cancellationToken = default)
    {
        var result = Items.Where(m => (m.Genres & genres) == genres).ToList();
        return Task.FromResult<IEnumerable<Movie>>(result);
    }
    */
}
