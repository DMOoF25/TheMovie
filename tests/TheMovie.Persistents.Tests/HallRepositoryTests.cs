using TheMovie.Domain.Entities;
using TheMovie.Infrastructure.Persistents;

namespace TheMovie.Persistents.Tests;

[TestClass]
public class HallRepositoryTests
{
    private string _tempDir = null!;
    private HallRepository _repo = null!;
    private const string _fileName = "Halls.csv";

    private HallRepository CreateRepositoryWithPath(string folder)
    {
        var repo = (HallRepository)Activator.CreateInstance(typeof(HallRepository), nonPublic: true)!;
        typeof(HallRepository)
            .GetField("_filePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy)!
            .SetValue(repo, Path.Combine(folder, _fileName));
        Directory.CreateDirectory(folder);
        return repo;
    }

    [TestInitialize]
    public async Task Init()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TheHallTests_" + Guid.NewGuid().ToString("N"));
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

    #region Create Test
    [TestMethod]
    public async Task AddAsync_PersistsSingleHall()
    {
        var hall = new Hall("Main Hall", Guid.NewGuid()) { };
        await _repo.AddAsync(hall);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Main Hall", all[0].Name);
        Assert.IsTrue(all[0].CinemaId != Guid.Empty);
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "Main Hall");
    }

    [TestMethod]
    public async Task AddRangeAsync_AddsAll()
    {
        var halls = new List<Hall>
        {
            new("Hall 1", Guid.NewGuid()) { },
            new("Hall 2", Guid.NewGuid()) { },
            new("Hall 3", Guid.NewGuid()) { }
        };
        await _repo.AddRangeAsync(halls);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(3, all.Count);
        Assert.IsTrue(all.Any(h => h.Name == "Hall 1"));
        Assert.IsTrue(all.Any(h => h.Name == "Hall 2"));
        Assert.IsTrue(all.Any(h => h.Name == "Hall 3"));
    }
    #endregion

    #region Read Tests
    [TestMethod]
    public async Task GetAllAsync_ReturnsAllHalls()
    {
        var hall1 = new Hall("Hall 1", Guid.NewGuid()) { };
        var hall2 = new Hall("Hall 2", Guid.NewGuid()) { };
        await _repo.AddAsync(hall1);
        await _repo.AddAsync(hall2);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(2, all.Count);
        Assert.IsTrue(all.Any(h => h.Name == "Hall 1"));
        Assert.IsTrue(all.Any(h => h.Name == "Hall 2"));
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsCorrectHall()
    {
        var hall = new Hall("Specific Hall", Guid.NewGuid()) { };
        await _repo.AddAsync(hall);
        var retrieved = await _repo.GetByIdAsync(hall.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Specific Hall", retrieved.Name);
        Assert.AreEqual(hall.Id, retrieved.Id);
    }
    #endregion

    #region Update Tests
    [TestMethod]
    public async Task UpdateAsync_ModifiesAndPersists()
    {
        var hall = new Hall("Main Hall", Guid.NewGuid()) { };
        await _repo.AddAsync(hall);
        hall.Name = "Updated Hall";
        await _repo.UpdateAsync(hall);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Updated Hall", all[0].Name);
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "Updated Hall");
    }
    #endregion

    #region Delete Tests
    [TestMethod]
    public async Task DeleteAsync_RemovesHall()
    {
        var hall = new Hall("Hall to Delete", Guid.NewGuid()) { };
        await _repo.AddAsync(hall);
        await _repo.DeleteAsync(hall.Id);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(0, all.Count);
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(0, lines.Length);
    }
    #endregion
}
