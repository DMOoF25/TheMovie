namespace TheMovie.Domain.Entities;

/// <summary>
/// Represents a cinema hall (auditorium) where movie screenings take place.
/// </summary>
/// <remarks>
/// Stores identifying and capacity information. Business rules such as ensuring
/// non-negative capacity or uniqueness of hall names within a cinema should be enforced
/// by higher-level services or validators.
/// </remarks>
public class Hall
{
    /// <summary>
    /// Unique identifier for the hall.
    /// </summary>
    /// <remarks>
    /// Assigned automatically in the parameterized constructor; may be populated by persistence upon retrieval.
    /// </remarks>
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable name of the hall (e.g., "Hall 1", "IMAX", "Dolby Atmos").
    /// </summary>
    /// <remarks>
    /// Defaults to an empty string to avoid null handling. Validation for non-empty value should occur externally.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Maximum seating capacity of the hall.
    /// </summary>
    /// <remarks>
    /// Represents the total number of seats available for booking (excluding staff or blocked seats).
    /// Must be a non-negative integer; enforcement is external to this entity.
    /// </remarks>
    public int Capacity { get; set; }

    /// <summary>
    /// Identifier of the cinema (parent aggregate) to which this hall belongs.
    /// </summary>
    public Guid CinemaId { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Hall"/>.
    /// </summary>
    /// <remarks>
    /// Intended for serialization / ORM usage. Prefer the parameterized constructor for new entities.
    /// </remarks>
    public Hall() { }

    /// <summary>
    /// Initializes a new instance of <see cref="Hall"/> with the specified attributes.
    /// </summary>
    /// <param name="name">The display name of the hall.</param>
    /// <param name="capacity">The seating capacity (must be non-negative).</param>
    /// <param name="cinemaId">The identifier of the owning cinema.</param>
    /// <remarks>
    /// Generates a new <see cref="Guid"/> for <see cref="Id"/>. Does not perform validation;
    /// caller is responsible for ensuring correct values.
    /// </remarks>
    public Hall(string name, int capacity, Guid cinemaId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Capacity = capacity;
        CinemaId = cinemaId;
    }
}
