namespace TheMovie.Domain.Entities;

/// <summary>
/// Represents an instructor (e.g., director) associated with one or more movies.
/// </summary>
/// <remarks>
/// This entity provides minimal identifying information. Additional metadata
/// (biography, birth date, etc.) can be added in future iterations or related aggregates.
/// </remarks>
public class Instructor
{
    /// <summary>
    /// Unique identifier for the instructor.
    /// </summary>
    /// <remarks>
    /// Intended for serialization / ORM usage. Prefer the parameterized constructor for new entities.
    /// </remarks>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the instructor.
    /// </summary>
    /// <remarks>
    /// Defaults to an empty string to avoid null reference issues. Domain validation for
    /// non-empty names should be enforced externally (e.g., in application services).
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="Instructor"/>.
    /// </summary>
    /// <remarks>
    /// Provided for serialization and ORM tools. Prefer the parameterized constructor for
    /// explicit creation with a name.
    /// </remarks>
    public Instructor() { }

    /// <summary>
    /// Initializes a new instance of <see cref="Instructor"/> with a specified name.
    /// </summary>
    /// <param name="name">The display name of the instructor.</param>
    /// <remarks>
    /// Generates a new <see cref="Guid"/> for <see cref="Id"/>. Validation of the <paramref name="name"/>
    /// (e.g., non-empty, length constraints) should be performed by the caller.
    /// </remarks>
    public Instructor(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
