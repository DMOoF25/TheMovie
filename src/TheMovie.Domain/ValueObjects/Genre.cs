namespace TheMovie.Domain.ValueObjects;
/// <summary>
/// Represents the genre of a movie using bit flags.
/// </summary>
/// <remarks>
/// This enum allows for multiple genres to be assigned to a movie using bitwise operations.
/// Each genre is represented by a unique bit, allowing for efficient storage and manipulation.
/// For example, a movie can be both Action and Comedy by combining the flags:
/// `Genre.Action | Genre.Comedy`.
/// The `None` value represents no genres assigned.
/// Usage:
/// - To check if a movie has a specific genre: `(Genres & Genre.Action) == Genre.Action`
/// See also:
/// https://www.youtube.com/watch?v=wiqJW64I1Rc
/// </remarks>
[System.Flags]
public enum Genre
{
    None = 0,           // Default value, no genres assigned
    Action = 1 << 0,    // 1 (0001 in binary)
    Adventure = 1 << 1, // 2 (0010 in binary)
    Biography = 1 << 2, // 4 (0100 in binary)
    Comedy = 1 << 3,    // 8 (1000 in binary)
    Crime = 1 << 4,     // 16 (0001 0000 in binary)
    Drama = 1 << 5,     // 32 (0010 0000 in binary)
    History = 1 << 6,   // 64 (0100 0000 in binary)
    Horror = 1 << 7,    // 128 (1000 0000 in binary)
    Music = 1 << 8,     // 256 (0001 0000 0000 in binary)
    Mystery = 1 << 9,   // 512 (0010 0000 0000 in binary)
    Romance = 1 << 10,  // 1024 (0100 0000 0000 in binary)
    Thriller = 1 << 11, // 2048 (1000 0000 0000 in binary)
    War = 1 << 12       // 4096 (0001 0000 0000 0000 in binary)
}
