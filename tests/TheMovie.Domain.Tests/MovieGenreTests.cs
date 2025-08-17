using TheMovie.Domain.Entities;
using TheMovie.Domain.ValueObjects;

namespace TheMovie.Domain.Tests;

[TestClass]
public sealed class MovieGenreTests
{
    [TestMethod]
    public void NewMovie_HasNoGenres()
    {
        var movie = new Movie("Test Movie", 120);
        Assert.AreEqual(Genre.None, movie.Genres);
        Assert.IsFalse(movie.HasGenre(Genre.Action));
    }

    [TestMethod]
    public void AddGenre_SetsSingleFlag()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Action);
        Assert.IsTrue(movie.HasGenre(Genre.Action));
        Assert.AreEqual(Genre.Action, movie.Genres);
    }

    [TestMethod]
    public void AddGenre_Twice_IsIdempotent()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Action);
        movie.AddGenre(Genre.Action);
        Assert.AreEqual(Genre.Action, movie.Genres);
    }

    [TestMethod]
    public void AddMultipleGenres_CombinesFlags()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Action | Genre.Drama | Genre.Comedy);
        Assert.IsTrue(movie.HasGenre(Genre.Action));
        Assert.IsTrue(movie.HasGenre(Genre.Drama));
        Assert.IsTrue(movie.HasGenre(Genre.Comedy));
        Assert.IsFalse(movie.HasGenre(Genre.Horror));
        Assert.IsTrue(movie.HasGenre(Genre.Action | Genre.Drama));
    }

    [TestMethod]
    public void HasGenre_ForComposite_FailsIfAnyMissing()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Action | Genre.Drama);
        Assert.IsFalse(movie.HasGenre(Genre.Action | Genre.Drama | Genre.Comedy));
    }

    [TestMethod]
    public void RemoveGenre_ClearsFlag()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Action | Genre.Drama);
        movie.RemoveGenre(Genre.Action);
        Assert.IsFalse(movie.HasGenre(Genre.Action));
        Assert.IsTrue(movie.HasGenre(Genre.Drama));
    }

    [TestMethod]
    public void RemoveGenre_NotPresent_NoChange()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Drama);
        var before = movie.Genres;
        movie.RemoveGenre(Genre.Action);
        Assert.AreEqual(before, movie.Genres);
    }

    [TestMethod]
    public void AddThenRemoveAll_ResultsInNone()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Action | Genre.Comedy | Genre.Drama);
        movie.RemoveGenre(Genre.Action | Genre.Comedy | Genre.Drama);
        Assert.AreEqual(Genre.None, movie.Genres);
    }

    [TestMethod]
    public void ComplexSequence_EndsWithExpectedFlags()
    {
        var movie = new Movie("Test Movie", 120);
        movie.AddGenre(Genre.Action);
        movie.AddGenre(Genre.Comedy | Genre.Drama);
        movie.RemoveGenre(Genre.Action);
        movie.AddGenre(Genre.Horror);
        var expected = Genre.Comedy | Genre.Drama | Genre.Horror;
        Assert.AreEqual(expected, movie.Genres);
        Assert.IsTrue(movie.HasGenre(expected));
    }
}