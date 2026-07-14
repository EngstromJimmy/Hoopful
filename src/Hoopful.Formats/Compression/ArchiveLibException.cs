namespace Hoopful.Formats.Compression;

/// <summary>
/// Thrown when compressed data (or a HUS/VIP container) is malformed, truncated,
/// or violates a structural limit of the format.
/// </summary>
/// <remarks>
/// This exception indicates a problem with the <em>input data</em>. Programmer errors
/// (for example a negative expected length) throw the usual argument exceptions instead.
/// </remarks>
public sealed class ArchiveLibException : Exception
{
    /// <summary>Initializes a new instance with a message describing the problem.</summary>
    public ArchiveLibException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and an inner exception.</summary>
    public ArchiveLibException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
