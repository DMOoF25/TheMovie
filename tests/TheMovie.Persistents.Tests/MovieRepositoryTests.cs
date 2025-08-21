using System.Text;
using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;
using TheMovie.Infrastructure.Persistents;

namespace TheMovie.Persistents.Tests;


[TestClass]
public class MovieRepositoryTests
{
    private string _tempDir = null!;
    private MovieRepository _repo = null!;

    private MovieRepository CreateRepositoryWithPath(string folder)
    {
        var repo = (MovieRepository)Activator.CreateInstance(typeof(MovieRepository), nonPublic: true)!;
        typeof(MovieRepository)
            .GetField("_filePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy)!
            .SetValue(repo, Path.Combine(folder, "movies.csv"));
        Directory.CreateDirectory(folder);
        return repo;
    }

    [TestInitialize]
    public async Task Init()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TheMovieTests_" + Guid.NewGuid().ToString("N"));
        _repo = CreateRepositoryWithPath(_tempDir);
        await _repo.InitializeAsync();
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { /* ignore */ }
    }

    [TestMethod]
    public async Task AddAsync_PersistsSingleMovie()
    {
        var movie = new Movie("Inception", 148, DateOnly.FromDateTime(DateTime.Now)) { Genres = Genre.Action | Genre.Mystery };
        await _repo.AddAsync(movie);

        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Inception", all[0].Title);

        var filePath = Path.Combine(_tempDir, "movies.csv");
        Assert.IsTrue(File.Exists(filePath));

        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "Inception");
    }

    [TestMethod]
    public async Task UpdateAsync_ModifiesAndPersists()
    {
        var movie = new Movie("Old", 90, DateOnly.FromDateTime(DateTime.Now)) { Genres = Genre.Drama };
        await _repo.AddAsync(movie);

        movie.Title = "New";
        movie.Duration = 95;
        await _repo.UpdateAsync(movie);

        var loaded = await _repo.GetByIdAsync(movie.Id);
        Assert.IsNotNull(loaded);
        Assert.AreEqual("New", loaded!.Title);

        var filePath = Path.Combine(_tempDir, "movies.csv");
        var text = File.ReadAllText(filePath);
        StringAssert.Contains(text, "New");
        Assert.IsFalse(text.Contains(",Old,", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesAndPersists()
    {
        var m1 = new Movie("A", 100, DateOnly.FromDateTime(DateTime.Now));
        var m2 = new Movie("B", 110, DateOnly.FromDateTime(DateTime.Now));
        await _repo.AddAsync(m1);
        await _repo.AddAsync(m2);

        await _repo.DeleteAsync(m1.Id);

        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("B", all[0].Title);

        var filePath = Path.Combine(_tempDir, "movies.csv");
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "B");
    }

    [TestMethod]
    public async Task InitializeAsync_LoadsExistingFile()
    {
        // Prepare a CSV file manually
        var filePath = Path.Combine(_tempDir, "movies.csv");
        var mId = Guid.NewGuid();
        var line = $"{mId},Interstellar,169,10-01-2010,{Genre.Music}";
        await File.WriteAllTextAsync(filePath, line, Encoding.UTF8);

        // New repository instance
        var repo2 = CreateRepositoryWithPath(_tempDir);
        await repo2.InitializeAsync();

        var all = (await repo2.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Interstellar", all[0].Title);
        Assert.AreEqual(169, all[0].Duration);
        Assert.IsTrue(all[0].HasGenre(Genre.Music));
    }

    [TestMethod]
    public async Task AddRangeAsync_AddsAll()
    {
        var movies = new[]
        {
            new Movie("M1", 90, DateOnly.FromDateTime(DateTime.Now)),
            new Movie("M2", 91, DateOnly.FromDateTime(DateTime.Now)),
            new Movie("M3", 92, DateOnly.FromDateTime(DateTime.Now))
        };

        await _repo.AddRangeAsync(movies);

        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(3, all.Count);
    }
}