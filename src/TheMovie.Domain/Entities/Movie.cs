using TheMovie.Domain.ValueObjects;

namespace TheMovie.Domain.Entities;

/// <summary>
/// Represents a movie entity with properties for Id, Title, Duration, and Genres.
/// This class allows for the management of movie genres using bitwise operations,
/// enabling efficient storage and retrieval of genre information.
/// </summary>
public class Movie
{
    /// <summary>
    /// Unique identifier for the movie.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Title of the movie.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Duration of the movie in minutes.
    /// </summary>
    public int Duration { get; set; }
    /// <summary>
    /// Genres associated with the movie, represented as a bitwise combination of Genre flags.
    /// </summary>
    public Genre Genres { get; set; } = Genre.None;

    public Movie()
    {
        Id = Guid.NewGuid();
    }

    public Movie(string title, int duration)
    {
        Id = Guid.NewGuid();
        Title = title;
        Duration = duration;
    }


    /// <summary>
    /// Checks if the movie has a specific genre.
    /// </summary>
    public bool HasGenre(Genre genre) => (Genres & genre) == genre;

    /// <summary>
    /// Adds a genre to the movie.
    /// </summary>
    public void AddGenre(Genre genre) => Genres |= genre;

    /// <summary>
    /// Removes a genre from the movie.
    /// </summary>
    public void RemoveGenre(Genre genre) => Genres &= ~genre;
}

// Example usage:
// var movie = new Movie { Title = "Inception", Duration = 148 };
// movie.AddGenre(Genre.Action | Genre.SciFi);
// if (movie.HasGenre(Genre.Action))
// {
//     Console.WriteLine($"{movie.Title} is an action movie.");
// }
// movie.RemoveGenre(Genre.Action);
// if (!movie.HasGenre(Genre.Action))
// {
//     Console.WriteLine($"{movie.Title} is no longer an action movie.");
// }
// This code defines a Movie class with properties for Id, Title, Duration, and Genres.
// It uses a bitwise approach to manage multiple genres, allowing for efficient storage and retrieval of genre information.
// The HasGenre, AddGenre, and RemoveGenre methods provide a way to check, add, and remove genres from a movie instance.
// The example usage demonstrates how to create a movie, add genres, check for a specific genre, and remove a genre.
// This approach is efficient for scenarios where a movie can belong to multiple genres, and it allows for easy manipulation of genre data.

