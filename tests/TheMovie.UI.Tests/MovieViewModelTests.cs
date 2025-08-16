using TheMovie.UI.Tests.Fakes;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Tests;

[TestClass]
public class MovieViewModelTests
{
    private static MovieViewModel Create(out FakeMovieRepository repo)
    {
        repo = new FakeMovieRepository();
        return new MovieViewModel(repo);
    }

    [TestMethod]
    public void Initial_state_commands_disabled()
    {
        var vm = Create(out _);

        Assert.AreEqual(string.Empty, vm.Title);
        Assert.AreEqual(string.Empty, vm.DurationText);
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));
        Assert.IsFalse(vm.ResetCommand.CanExecute(null));
    }

    [TestMethod]
    public void SaveCommand_enables_only_with_valid_inputs()
    {
        var vm = Create(out _);

        // Nothing set
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));

        // Title only
        vm.Title = "Inception";
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));

        // Invalid duration text
        vm.DurationText = "abc";
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));

        // Zero duration
        vm.DurationText = "0";
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));

        // Valid duration but no genre
        vm.DurationText = "148";
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));

        // Select one genre
        var firstGenre = vm.GenreOptions.First();
        firstGenre.IsSelected = true;

        Assert.IsTrue(vm.SaveCommand.CanExecute(null));
    }

    [TestMethod]
    public void ResetCommand_clears_fields_and_selection()
    {
        var vm = Create(out _);
        vm.Title = "Test";
        vm.DurationText = "10";
        vm.GenreOptions.First().IsSelected = true;

        Assert.IsTrue(vm.ResetCommand.CanExecute(null));

        vm.ResetCommand.Execute(null);

        Assert.AreEqual(string.Empty, vm.Title);
        Assert.AreEqual(string.Empty, vm.DurationText);
        Assert.IsTrue(vm.GenreOptions.All(g => !g.IsSelected));
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));
        Assert.IsFalse(vm.ResetCommand.CanExecute(null));
    }

    [TestMethod]
    public void Invalid_duration_prevents_save()
    {
        var vm = Create(out _);
        vm.Title = "Movie";
        vm.GenreOptions.First().IsSelected = true;

        vm.DurationText = "-5";
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));

        vm.DurationText = "notnumber";
        Assert.IsFalse(vm.SaveCommand.CanExecute(null));
    }
}
