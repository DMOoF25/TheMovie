using System.Text;
using TheMovie.Domain.Entities;
using TheMovie.Infrastructure.Persistents;

namespace TheMovie.Persistents.Tests;

[TestClass]
public class InstructorRepositoryTests
{
    private string _tempDir = null!;
    private InstructorRepository _repo = null!;
    private const string _fileName = "instructors.csv";

    private InstructorRepository CreateRepositoryWithPath(string folder)
    {
        var repo = (InstructorRepository)Activator.CreateInstance(typeof(InstructorRepository), nonPublic: true)!;
        typeof(InstructorRepository)
            .GetField("_filePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy)!
            .SetValue(repo, Path.Combine(folder, _fileName));
        Directory.CreateDirectory(folder);
        return repo;
    }

    [TestInitialize]
    public async Task Init()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TheInstructorTests_" + Guid.NewGuid().ToString("N"));
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
    public async Task AddAsync_PersistsSingleInstructor()
    {
        var instructor = new Instructor("Sergei Dvortsevoy") { };
        await _repo.AddAsync(instructor);

        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Sergei Dvortsevoy", all[0].Name);

        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));

        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "Sergei Dvortsevoy");
    }

    [TestMethod]
    public async Task UpdateAsync_ModifiesAndPersists()
    {
        var instructor = new Instructor("Sergei Dvortsevoy") { };
        await _repo.AddAsync(instructor);
        instructor.Name = "Sergei Dvortsevoy Updated";
        await _repo.UpdateAsync(instructor);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Sergei Dvortsevoy Updated", all[0].Name);
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(1, lines.Length);
        StringAssert.Contains(lines[0], "Sergei Dvortsevoy Updated");
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesAndPersists()
    {
        var instructor = new Instructor("Sergei Dvortsevoy") { };
        await _repo.AddAsync(instructor);
        await _repo.DeleteAsync(instructor.Id);
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
        // Prepare a CSV file manually
        var filePath = Path.Combine(_tempDir, _fileName);
        var mId = Guid.NewGuid();
        var line = $"{mId},Sergei Dvortsevoy";
        await File.WriteAllTextAsync(filePath, line, Encoding.UTF8);

        // New repository instance
        var repo2 = CreateRepositoryWithPath(_tempDir);
        await repo2.InitializeAsync();

        var all = (await repo2.GetAllAsync()).ToList();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Sergei Dvortsevoy", all[0].Name);
    }

    [TestMethod]
    public async Task AddRangeAsync_AddsAll()
    {
        var instructors = new List<Instructor>
        {
            new Instructor("Instructor One"),
            new Instructor("Instructor Two"),
            new Instructor("Instructor Three")
        };
        await _repo.AddRangeAsync(instructors);
        var all = (await _repo.GetAllAsync()).ToList();
        Assert.AreEqual(3, all.Count);
        Assert.IsTrue(all.Any(i => i.Name == "Instructor One"));
        Assert.IsTrue(all.Any(i => i.Name == "Instructor Two"));
        Assert.IsTrue(all.Any(i => i.Name == "Instructor Three"));
        var filePath = Path.Combine(_tempDir, _fileName);
        Assert.IsTrue(File.Exists(filePath));
        var lines = File.ReadAllLines(filePath);
        Assert.AreEqual(3, lines.Length);
    }

}