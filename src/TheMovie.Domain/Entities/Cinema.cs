namespace TheMovie.Domain.Entities;

/// <summary>
/// Represents a cinema location containing one or more halls where movies are exhibited.
/// </summary>
/// <remarks>
/// This entity captures minimal identifying data. Additional attributes (contact info, timezone,
/// operational hours) can be introduced in future iterations or related aggregates.
/// </remarks>
public class Cinema
{
    /// <summary>
    /// Unique identifier for the cinema.
    /// </summary>
    /// <remarks>
    /// Automatically generated in the parameterized constructor; may be populated by persistence frameworks during materialization.
    /// </remarks>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the cinema (e.g., brand or branch name).
    /// </summary>
    /// <remarks>
    /// Defaults to an empty string to avoid null handling. Validation for non-empty value should occur externally.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable location or address descriptor of the cinema.
    /// </summary>
    /// <remarks>
    /// Can be a full address, city reference, or any meaningful location string. Structured address data can be added later if needed.
    /// </remarks>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="Cinema"/>.
    /// </summary>
    /// <remarks>
    /// Intended for serialization / ORM usage. Prefer the parameterized constructor for new entities.
    /// </remarks>
    public Cinema() { }

    /// <summary>
    /// Initializes a new instance of <see cref="Cinema"/> with the specified name and location.
    /// </summary>
    /// <param name="name">The display name of the cinema.</param>
    /// <param name="location">The location or address description.</param>
    /// <remarks>
    /// Generates a new <see cref="Guid"/> for <see cref="Id"/>. Input validation (non-empty, length constraints) should be enforced by the caller.
    /// </remarks>
    public Cinema(string name, string location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Location = location;
    }
}
