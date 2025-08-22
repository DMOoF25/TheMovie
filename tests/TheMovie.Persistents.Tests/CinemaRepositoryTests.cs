using System.Text;
using TheMovie.Domain.Entities;
using TheMovie.Infrastructure.Persistents;

namespace TheMovie.Persistents.Tests;

[TestClass]
public class CinemaRepositoryTests
{
    private string _tempDir = null!;
    private CinemaRepository _repo = null!;
    private const string _fileName = "cinemas.csv";

    private CinemaRepository CreateRepositoryWithPath(string folder)
    {
        var repo = (CinemaRepository)Activator.CreateInstance(typeof(CinemaRepository), nonPublic: true)!;
        typeof(CinemaRepository)
            .GetField("_filePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy)!
            .SetValue(repo, Path.Combine(folder, _fileName));
        Directory.CreateDirectory(folder);
        return repo;
    }

    [TestInitialize]
    public async Task Init()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TheCinemaTests_" + Guid.NewGuid().ToString("N"));
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
    #endregion

    #region Read Tests
    #endregion

    #region Update Tests
    #endregion

    #region Delete Tests
    #endregion


    [TestMethod]
    public async Task AddAsync_PersistsSingleInstructor()
    {
        var cinema = new Cinema("Cineplex", "123 Main St") { };
        await _repo.AddAsync(cinema);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Cineplex", all[0].Name);
        Assert.AreEqual("123 Main St", all[0].Location);
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "Cineplex");
    }

    [TestMethod]
    public async Task AddRangeAsync_AddsAll()
    {
        List<Cinema> cinemas =
        [
            new("Cineplex", "123 Main St"),
            new("Cineworld", "456 Elm St"),
            new("Cinemax", "789 Oak Ave")
        ];
        await _repo.AddRangeAsync(cinemas);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(3, all.Count);
        Assert.IsTrue(all.Any(c => c.Name == "Cineplex"));
        Assert.IsTrue(all.Any(c => c.Name == "Cineworld"));
        Assert.IsTrue(all.Any(c => c.Name == "Cinemax"));
    }


    [TestMethod]
    public async Task UpdateAsync_ModifiesAndPersists()
    {
        var cinema = new Cinema("Cineplex", "123 Main St") { };
        await _repo.AddAsync(cinema);
        cinema.Name = "Cineworld";
        cinema.Location = "456 Elm St";
        await _repo.UpdateAsync(cinema);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Cineworld", all[0].Name);
        Assert.AreEqual("456 Elm St", all[0].Location);
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "Cineworld");
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesAndPersists()
    {
        var cinema = new Cinema("Cineplex", "123 Main St") { };
        await _repo.AddAsync(cinema);
        await _repo.DeleteAsync(cinema.Id);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(0, all.Count);
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(0, lines.Length);
    }

    [TestMethod]
    public async Task InitializeAsync_LoadsExistingFile()
    {
        var filePath = Path.Combine(_tempDir, _fileName);
        var mId = Guid.NewGuid();
        var line = $"{mId},Cinemax,Strøget 33 8700 Horsens";
        await File.WriteAllTextAsync(filePath, line, Encoding.UTF8);

        // New repository instance
        var repo2 = CreateRepositoryWithPath(_tempDir);
        await repo2.InitializeAsync();

        var all = (await repo2.GetAllAsync()).ToList();


        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Cinemax", all[0].Name);
        Assert.AreEqual("Strøget 33 8700 Horsens", all[0].Location);
    }

}
