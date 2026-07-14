namespace Hoopful.Formats.Compression;

/// <summary>
/// The Greenleaf ArchiveLib compression levels. The level only affects the size of the
/// LZ sliding window (<c>1 &lt;&lt; (10 + level)</c> bytes); the bit-stream layout is identical
/// for every level.
/// </summary>
/// <remarks>
/// HUS and VIP embroidery files use <see cref="Level4"/> (<c>AL_GREENLEAF_LEVEL_4</c>,
/// a 16 KiB window). Data compressed at a lower level always decodes correctly with a
/// higher level because the window only bounds the largest valid back-reference distance.
/// </remarks>
public enum ArchiveLibCompressionLevel
{
    /// <summary>1 KiB sliding window.</summary>
    Level0 = 0,

    /// <summary>2 KiB sliding window.</summary>
    Level1 = 1,

    /// <summary>4 KiB sliding window.</summary>
    Level2 = 2,

    /// <summary>8 KiB sliding window.</summary>
    Level3 = 3,

    /// <summary>16 KiB sliding window. Used by HUS and VIP embroidery files.</summary>
    Level4 = 4,
}
