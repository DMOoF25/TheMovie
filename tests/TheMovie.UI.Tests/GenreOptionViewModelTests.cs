using TheMovie.Domain.ValueObjects;
using TheMovie.UI.ViewModels;

namespace TheMovie.UI.Tests;

[TestClass]
public class GenreOptionViewModelTests
{
    [TestMethod]
    public void Constructor_SetsNameAndValue()
    {
        // Arrange
        var name = "Action";
        var value = Genre.Action;

        // Act
        var vm = new GenreOptionViewModel(name, value);

        // Assert
        Assert.AreEqual(name, vm.Name);
        Assert.AreEqual(value, vm.Value);
        Assert.IsFalse(vm.IsSelected);
    }

    [TestMethod]
    public void IsSelected_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var vm = new GenreOptionViewModel("Comedy", Genre.Comedy);
        bool eventRaised = false;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.IsSelected))
                eventRaised = true;
        };

        // Act
        vm.IsSelected = true;

        // Assert
        Assert.IsTrue(vm.IsSelected);
        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    public void IsSelected_SetSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        var vm = new GenreOptionViewModel("Drama", Genre.Drama);
        bool eventRaised = false;
        vm.IsSelected = false;
        vm.PropertyChanged += (s, e) => eventRaised = true;

        // Act
        vm.IsSelected = false;

        // Assert
        Assert.IsFalse(eventRaised);
    }
}