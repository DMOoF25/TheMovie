namespace TheMovie.Domain.Entities;

/// <summary>
/// Represents a scheduled movie screening within a specific hall.
/// </summary>
/// <remarks>
/// A screening defines the time interval during which a movie is shown in a hall.
/// Overlapping screenings in the same hall should be avoided at a higher validation layer.
/// </remarks>
public class Screening
{
    /// <summary>
    /// Unique identifier for the screening.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The UTC date and time at which the screening begins.
    /// </summary>
    /// <remarks>
    /// Consumers should treat this as UTC (or ensure consistent time zone handling)
    /// to avoid scheduling conflicts caused by daylight saving transitions.
    /// </remarks>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Identifier of the movie being shown.
    /// </summary>
    public Guid MovieId { get; set; }

    /// <summary>
    /// Identifier of the hall in which the screening takes place.
    /// </summary>
    public Guid HallId { get; set; }

    /// <summary>
    /// Initializes a new, empty instance of <see cref="Screening"/>.
    /// </summary>
    /// <remarks>
    /// Intended for serialization / ORM usage. Prefer the parameterized constructor for new entities.
    /// </remarks>
    public Screening() { }

    /// <summary>
    /// Initializes a new instance of <see cref="Screening"/> with the specified parameters.
    /// </summary>
    /// <param name="startTime">The UTC start time of the screening.</param>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <param name="hallId">The identifier of the hall.</param>
    public Screening(DateTime startTime, Guid movieId, Guid hallId)
    {
        Id = Guid.NewGuid();
        StartTime = startTime;
        MovieId = movieId;
        HallId = hallId;
    }
}
