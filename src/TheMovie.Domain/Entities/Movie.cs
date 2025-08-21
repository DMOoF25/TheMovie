using TheMovie.Domain.ValueObjects;

namespace TheMovie.Domain.Entities;

/// <summary>
/// Represents a movie within the domain, including identity, descriptive metadata,
/// runtime characteristics, release information, instructor (e.g. director) association,
/// and genre flags.
/// </summary>
/// <remarks>
/// Genres are stored using a bitwise flags enum (<see cref="Genre"/>) allowing efficient
/// combination, querying, and manipulation of multiple genre classifications.
/// </remarks>
public class Movie
{
    /// <summary>
    /// Unique identifier for the movie.
    /// </summary>
    /// <remarks>
    /// Automatically generated in the parameterized constructor. May be set by persistence mechanisms when materializing.
    /// </remarks>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the movie.
    /// </summary>
    /// <remarks>
    /// Should be a human-readable, localized title. Empty string by default to avoid null handling.
    /// </remarks>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the movie in whole minutes.
    /// </summary>
    /// <remarks>
    /// Business validation (e.g., ensuring a positive value) should be enforced externally.
    /// </remarks>
    public int Duration { get; set; }

    /// <summary>
    /// Bitwise combination of one or more <see cref="Genre"/> values describing the movie.
    /// </summary>
    /// <example>
    /// <code>
    /// movie.Genres = Genre.Action | Genre.SciFi | Genre.Thriller;
    /// </code>
    /// </example>
    public Genre Genres { get; set; } = Genre.None;

    /// <summary>
    /// Identifier referencing the instructor (e.g., director) entity associated with the movie.
    /// </summary>
    /// <remarks>
    /// Optional; may remain <see cref="Guid.Empty"/> if no instructor is assigned yet.
    /// </remarks>
    public Guid InstructorId { get; set; } = Guid.Empty;

    /// <summary>
    /// The premiere (initial public release) date of the movie.
    /// </summary>
    /// <remarks>
    /// Stored as a <see cref="DateOnly"/> to avoid time zone ambiguity.
    /// Defaults to the current local date at instantiation.
    /// </remarks>
    public DateOnly Premiere { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    /// Initializes a new instance of <see cref="Movie"/>.
    /// </summary>
    /// <remarks>
    /// Intended for serialization / ORM usage. Prefer the parameterized constructor for new entities.
    /// </remarks>
    public Movie() { }

    /// <summary>
    /// Initializes a new instance of <see cref="Movie"/> with required descriptive data.
    /// </summary>
    /// <param name="title">Human-readable title.</param>
    /// <param name="duration">Runtime length in minutes.</param>
    /// <param name="premiere">Premiere (release) date.</param>
    /// <param name="instructorId">Optional instructor (director) identifier; defaults to empty.</param>
    /// <example>
    /// <code>
    /// var movie = new Movie("Inception", 148, new DateOnly(2010, 7, 16));
    /// movie.AddGenre(Genre.Action | Genre.Thriller);
    /// </code>
    /// </example>
    public Movie(string title, int duration, DateOnly premiere, Guid instructorId = default)
    {
        Id = Guid.NewGuid();
        Title = title;
        Duration = duration;
        InstructorId = instructorId;
        Premiere = premiere;
    }

    /// <summary>
    /// Determines whether the movie includes the specified genre flag(s).
    /// </summary>
    /// <param name="genre">One or more combined <see cref="Genre"/> flags to test.</param>
    /// <returns>
    /// True if all provided genre flags are present; otherwise, false.
    /// </returns>
    /// <example>
    /// <code>
    /// if (movie.HasGenre(Genre.Comedy | Genre.Romance)) { /* handle rom-com */ }
    /// </code>
    /// </example>
    public bool HasGenre(Genre genre) => (Genres & genre) == genre;

    /// <summary>
    /// Adds (sets) one or more genre flags on the movie.
    /// </summary>
    /// <param name="genre">The genre flag(s) to add.</param>
    /// <remarks>
    /// Existing genre flags are preserved; only the specified bits are turned on.
    /// </remarks>
    public void AddGenre(Genre genre) => Genres |= genre;

    /// <summary>
    /// Removes (clears) one or more genre flags from the movie.
    /// </summary>
    /// <param name="genre">The genre flag(s) to remove.</param>
    /// <remarks>
    /// Only the specified bits are turned off; other flags remain unchanged.
    /// </remarks>
    public void RemoveGenre(Genre genre) => Genres &= ~genre;
}

/*
Example usage:

var movie = new Movie("Inception", 148, new DateOnly(2010, 7, 16));
movie.AddGenre(Genre.Action | Genre.Thriller | Genre.Mystery);

if (movie.HasGenre(Genre.Action))
{
    Console.WriteLine($"{movie.Title} is an action movie.");
}

movie.RemoveGenre(Genre.Action);

if (!movie.HasGenre(Genre.Action))
{
    Console.WriteLine($"{movie.Title} is no longer marked as action.");
}
*/

