using TheMovie.Domain.Entities;
using TheMovie.Infrastructure.Persistents;

namespace TheMovie.Persistents.Tests;

[TestClass]
public class ScreeningRepositoryTests
{
    private string _tempDir = null!;
    private ScreeningRepository _repo = null!;
    private const string _fileName = "screenings.csv";

    private ScreeningRepository CreateRepositoryWithPath(string folder)
    {
        var repo = (ScreeningRepository)Activator.CreateInstance(typeof(ScreeningRepository), nonPublic: true)!;
        typeof(ScreeningRepository)
            .GetField("_filePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy)!
            .SetValue(repo, Path.Combine(folder, _fileName));
        Directory.CreateDirectory(folder);
        return repo;
    }
    [TestInitialize]
    public async Task Init()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TheScreeningTests_" + Guid.NewGuid().ToString("N"));
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

    #region Create Tests
    [TestMethod]
    public async Task AddAsync_PersistsSingleScreening()
    {
        var movie1Id = Guid.NewGuid();
        var screening = new Screening(DateTime.Now, movie1Id, Guid.NewGuid());
        await _repo.AddAsync(screening);
        var screenings = await _repo.GetAllAsync();
        Assert.IsTrue(screenings.Any(s => s.MovieId == movie1Id));
    }

    [TestMethod]
    public async Task AddRangeAsync_AddsAll()
    {
        var movie1Id = Guid.NewGuid();
        var movie2Id = Guid.NewGuid();
        var screenings = new List<Screening>
        {
            new(DateTime.Now, movie1Id, Guid.NewGuid()),
            new(DateTime.Now, movie2Id, Guid.NewGuid())
        };
        await _repo.AddRangeAsync(screenings);
        var all = await _repo.GetAllAsync();
        Assert.AreEqual(2, all.Count());
        Assert.IsTrue(all.Any(s => s.MovieId == movie1Id));
        Assert.IsTrue(all.Any(s => s.MovieId == movie2Id));
    }
    #endregion
    #region Read Tests
    [TestMethod]
    public async Task GetAllAsync_ReturnsAllScreenings()
    {
        var movie1Id = Guid.NewGuid();
        var movie2Id = Guid.NewGuid();
        var screenings = new List<Screening>
        {
            new(DateTime.Now, movie1Id, Guid.NewGuid()),
            new(DateTime.Now.AddHours(1), movie2Id, Guid.NewGuid())
        };
        await _repo.AddRangeAsync(screenings);
        var all = await _repo.GetAllAsync();
        Assert.AreEqual(2, all.Count());
        Assert.IsTrue(all.Any(s => s.MovieId == movie1Id));
        Assert.IsTrue(all.Any(s => s.MovieId == movie2Id));
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsCorrectScreening()
    {
        var movieId = Guid.NewGuid();
        var screening = new Screening(DateTime.Now, movieId, Guid.NewGuid());
        await _repo.AddAsync(screening);
        var retrieved = await _repo.GetByIdAsync(screening.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(screening.Id, retrieved.Id);
        Assert.AreEqual(movieId, retrieved.MovieId);
    }

    #endregion
    #region Update Tests
    [TestMethod]
    public async Task UpdateAsync_UpdatesScreening()
    {
        var movieId = Guid.NewGuid();
        var screening = new Screening(DateTime.Now, movieId, Guid.NewGuid());
        await _repo.AddAsync(screening);

        // Modify the screening
        screening.StartTime = DateTime.Now.AddHours(2);
        await _repo.UpdateAsync(screening);

        var updated = await _repo.GetByIdAsync(screening.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual(screening.StartTime, updated.StartTime);
    }
    #endregion
    #region Delete Tests
    [TestMethod]
    public async Task DeleteAsync_RemovesScreening()
    {
        var movieId = Guid.NewGuid();
        var screening = new Screening(DateTime.Now, movieId, Guid.NewGuid());
        await _repo.AddAsync(screening);
        await _repo.DeleteAsync(screening.Id);
        var deleted = await _repo.GetByIdAsync(screening.Id);
        Assert.IsNull(deleted);
    }
    #endregion
}
