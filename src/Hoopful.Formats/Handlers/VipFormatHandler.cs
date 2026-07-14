using System.Buffers.Binary;
using System.Drawing;
using Hoopful.Formats.Compression;

namespace Hoopful.Formats.Handlers;

/// <summary>
/// Pfaff <c>.vip</c> handler: parses the container, undoes the XOR-obfuscated colour
/// table, decompresses the three stitch streams with the managed ArchiveLib decompressor
/// and converts them into an <see cref="EmbroideryDesign"/> exactly like the original
/// <c>VIPHandler</c> (which P/Invoked the native <c>al21mfc.dll</c>).
/// </summary>
/// <remarks>
/// <para>File layout (all integers little endian):</para>
/// <code>
/// offset  size  field
///      0     4  magic code 0x0190FC5D
///      4     4  number of stitches
///      8     4  number of colours
///     12     2  extent +X    (0.1 mm)
///     14     2  extent +Y
///     16     2  extent -X
///     18     2  extent -Y
///     20     4  offset of the compressed attribute stream
///     24     4  offset of the compressed X stream
///     28     4  offset of the compressed Y stream
///     32     8  name string (zero padded)
///     40     2  unknown
///     42     4  colour table length in bytes
///     46   4*n  obfuscated colour table, four bytes per colour
/// </code>
/// <para>
/// The colour table is obfuscated with a byte-chained XOR: each stored byte is XORed with
/// a fixed 400-byte table and with the <em>previous stored byte</em> to recover the value.
/// Writers may place additional unknown data between the colour table and the attribute
/// offset; the streams are located purely by the header offsets. The streams themselves
/// are identical to HUS: ArchiveLib level-4, one decompressed byte per stitch.
/// </para>
/// </remarks>
public sealed class VipFormatHandler : IEmbroideryFormatHandler
{
    private const uint Magic = 0x0190FC5D;
    private const int FixedHeaderLength = 46;

    /// <summary>
    /// XOR obfuscation table for the colour block, as used by the original Pfaff software
    /// (reproduced from the Embroidermodder project's libembroidery).
    /// Its 400 bytes bound the colour count at 100.
    /// </summary>
    private static ReadOnlySpan<byte> ColorDecodingTable =>
    [
        0x2E, 0x82, 0xE4, 0x6F, 0x38, 0xA9, 0xDC, 0xC6, 0x7B, 0xB6, 0x28, 0xAC,
        0xFD, 0xAA, 0x8A, 0x4E, 0x76, 0x2E, 0xF0, 0xE4, 0x25, 0x1B, 0x8A, 0x68,
        0x4E, 0x92, 0xB9, 0xB4, 0x95, 0xF0, 0x3E, 0xEF, 0xF7, 0x40, 0x24, 0x18,
        0x39, 0x31, 0xBB, 0xE1, 0x53, 0xA8, 0x1F, 0xB1, 0x3A, 0x07, 0xFB, 0xCB,
        0xE6, 0x00, 0x81, 0x50, 0x0E, 0x40, 0xE1, 0x2C, 0x73, 0x50, 0x0D, 0x91,
        0xD6, 0x0A, 0x5D, 0xD6, 0x8B, 0xB8, 0x62, 0xAE, 0x47, 0x00, 0x53, 0x5A,
        0xB7, 0x80, 0xAA, 0x28, 0xF7, 0x5D, 0x70, 0x5E, 0x2C, 0x0B, 0x98, 0xE3,
        0xA0, 0x98, 0x60, 0x47, 0x89, 0x9B, 0x82, 0xFB, 0x40, 0xC9, 0xB4, 0x00,
        0x0E, 0x68, 0x6A, 0x1E, 0x09, 0x85, 0xC0, 0x53, 0x81, 0xD1, 0x98, 0x89,
        0xAF, 0xE8, 0x85, 0x4F, 0xE3, 0x69, 0x89, 0x03, 0xA1, 0x2E, 0x8F, 0xCF,
        0xED, 0x91, 0x9F, 0x58, 0x1E, 0xD6, 0x84, 0x3C, 0x09, 0x27, 0xBD, 0xF4,
        0xC3, 0x90, 0xC0, 0x51, 0x1B, 0x2B, 0x63, 0xBC, 0xB9, 0x3D, 0x40, 0x4D,
        0x62, 0x6F, 0xE0, 0x8C, 0xF5, 0x5D, 0x08, 0xFD, 0x3D, 0x50, 0x36, 0xD7,
        0xC9, 0xC9, 0x43, 0xE4, 0x2D, 0xCB, 0x95, 0xB6, 0xF4, 0x0D, 0xEA, 0xC2,
        0xFD, 0x66, 0x3F, 0x5E, 0xBD, 0x69, 0x06, 0x2A, 0x03, 0x19, 0x47, 0x2B,
        0xDF, 0x38, 0xEA, 0x4F, 0x80, 0x49, 0x95, 0xB2, 0xD6, 0xF9, 0x9A, 0x75,
        0xF4, 0xD8, 0x9B, 0x1D, 0xB0, 0xA4, 0x69, 0xDB, 0xA9, 0x21, 0x79, 0x6F,
        0xD8, 0xDE, 0x33, 0xFE, 0x9F, 0x04, 0xE5, 0x9A, 0x6B, 0x9B, 0x73, 0x83,
        0x62, 0x7C, 0xB9, 0x66, 0x76, 0xF2, 0x5B, 0xC9, 0x5E, 0xFC, 0x74, 0xAA,
        0x6C, 0xF1, 0xCD, 0x93, 0xCE, 0xE9, 0x80, 0x53, 0x03, 0x3B, 0x97, 0x4B,
        0x39, 0x76, 0xC2, 0xC1, 0x56, 0xCB, 0x70, 0xFD, 0x3B, 0x3E, 0x52, 0x57,
        0x81, 0x5D, 0x56, 0x8D, 0x51, 0x90, 0xD4, 0x76, 0xD7, 0xD5, 0x16, 0x02,
        0x6D, 0xF2, 0x4D, 0xE1, 0x0E, 0x96, 0x4F, 0xA1, 0x3A, 0xA0, 0x60, 0x59,
        0x64, 0x04, 0x1A, 0xE4, 0x67, 0xB6, 0xED, 0x3F, 0x74, 0x20, 0x55, 0x1F,
        0xFB, 0x23, 0x92, 0x91, 0x53, 0xC8, 0x65, 0xAB, 0x9D, 0x51, 0xD6, 0x73,
        0xDE, 0x01, 0xB1, 0x80, 0xB7, 0xC0, 0xD6, 0x80, 0x1C, 0x2E, 0x3C, 0x83,
        0x63, 0xEE, 0xBC, 0x33, 0x25, 0xE2, 0x0E, 0x7A, 0x67, 0xDE, 0x3F, 0x71,
        0x14, 0x49, 0x9C, 0x92, 0x93, 0x0D, 0x26, 0x9A, 0x0E, 0xDA, 0xED, 0x6F,
        0xA4, 0x89, 0x0C, 0x1B, 0xF0, 0xA1, 0xDF, 0xE1, 0x9E, 0x3C, 0x04, 0x78,
        0xE4, 0xAB, 0x6D, 0xFF, 0x9C, 0xAF, 0xCA, 0xC7, 0x88, 0x17, 0x9C, 0xE5,
        0xB7, 0x33, 0x6D, 0xDC, 0xED, 0x8F, 0x6C, 0x18, 0x1D, 0x71, 0x06, 0xB1,
        0xC5, 0xE2, 0xCF, 0x13, 0x77, 0x81, 0xC5, 0xB7, 0x0A, 0x14, 0x0A, 0x6B,
        0x40, 0x26, 0xA0, 0x88, 0xD1, 0x62, 0x6A, 0xB3, 0x50, 0x12, 0xB9, 0x9B,
        0xB5, 0x83, 0x9B, 0x37,
    ];

    /// <summary>Largest representable colour count, bounded by the XOR table size.</summary>
    public const int MaxColorCount = 100;

    /// <summary>Sanity cap; see <see cref="HusFormatHandler.MaxStitchCount"/>.</summary>
    public const int MaxStitchCount = HusFormatHandler.MaxStitchCount;

    /// <summary>Returns true when <paramref name="fileData"/> starts with the VIP magic code.</summary>
    public static bool HasVipMagic(ReadOnlySpan<byte> fileData)
    {
        return fileData.Length >= 4 && BinaryPrimitives.ReadUInt32LittleEndian(fileData) == Magic;
    }

    /// <inheritdoc />
    /// <exception cref="ArchiveLibException">The file is not a valid VIP file.</exception>
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        ReadOnlySpan<byte> file = fileData;
        if (file.Length < FixedHeaderLength)
        {
            throw new ArchiveLibException(
                $"Not a VIP file: {file.Length} bytes is shorter than the {FixedHeaderLength}-byte header.");
        }

        uint magic = BinaryPrimitives.ReadUInt32LittleEndian(file);
        if (magic != Magic)
        {
            throw new ArchiveLibException($"Not a VIP file: unexpected magic code 0x{magic:X8} (expected 0x{Magic:X8}).");
        }

        int stitchCount = BinaryPrimitives.ReadInt32LittleEndian(file[4..]);
        int colorCount = BinaryPrimitives.ReadInt32LittleEndian(file[8..]);
        short extendPositiveX = BinaryPrimitives.ReadInt16LittleEndian(file[12..]);
        short extendPositiveY = BinaryPrimitives.ReadInt16LittleEndian(file[14..]);
        short extendNegativeX = BinaryPrimitives.ReadInt16LittleEndian(file[16..]);
        short extendNegativeY = BinaryPrimitives.ReadInt16LittleEndian(file[18..]);
        int attributeOffset = BinaryPrimitives.ReadInt32LittleEndian(file[20..]);
        int xOffset = BinaryPrimitives.ReadInt32LittleEndian(file[24..]);
        int yOffset = BinaryPrimitives.ReadInt32LittleEndian(file[28..]);
        int colorTableLength = BinaryPrimitives.ReadInt32LittleEndian(file[42..]);

        if (stitchCount is <= 0 or > MaxStitchCount)
        {
            throw new ArchiveLibException($"VIP header is corrupt: implausible stitch count {stitchCount}.");
        }

        if (colorCount is < 0 or > MaxColorCount)
        {
            throw new ArchiveLibException($"VIP header is corrupt: implausible colour count {colorCount}.");
        }

        int colorBytes = 4 * colorCount;
        if (colorTableLength < colorBytes)
        {
            throw new ArchiveLibException(
                $"VIP header is corrupt: colour table length {colorTableLength} is too small for {colorCount} colours.");
        }

        int colorTableEnd = checked(FixedHeaderLength + colorBytes);
        if (attributeOffset < colorTableEnd || attributeOffset > xOffset
            || xOffset > yOffset || yOffset > file.Length)
        {
            throw new ArchiveLibException(
                $"VIP header is corrupt: stream offsets {attributeOffset}/{xOffset}/{yOffset} do not fit a {file.Length}-byte file.");
        }

        // Undo the XOR chain: decoded[i] = stored[i] ^ table[i] ^ stored[i - 1].
        var colors = new Color[colorCount];
        byte previousStoredByte = 0;
        Span<byte> decodedColor = stackalloc byte[4];
        for (int i = 0; i < colorBytes; i++)
        {
            byte storedByte = file[FixedHeaderLength + i];
            decodedColor[i % 4] = (byte)(storedByte ^ ColorDecodingTable[i] ^ previousStoredByte);
            previousStoredByte = storedByte;
            if (i % 4 == 3)
            {
                // Four decoded bytes per colour: red, green, blue, unknown.
                colors[i / 4] = Color.FromArgb(decodedColor[0], decodedColor[1], decodedColor[2]);
            }
        }

        byte[] attributeData = HusFormatHandler.DecompressStream(file[attributeOffset..xOffset], stitchCount, "attribute");
        byte[] xData = HusFormatHandler.DecompressStream(file[xOffset..yOffset], stitchCount, "X");
        byte[] yData = HusFormatHandler.DecompressStream(file[yOffset..], stitchCount, "Y");

        var design = new EmbroideryDesign
        {
            Name = name,
            NumberOfStitches = stitchCount,
            NumberOfColors = colorCount,
            PositiveX = Math.Max(extendPositiveX, (short)0),
            PositiveY = Math.Max(extendPositiveY, (short)0),
            NegativeX = HusFormatHandler.LegacyNegativeExtent(extendNegativeX),
            NegativeY = HusFormatHandler.LegacyNegativeExtent(extendNegativeY),
            Colors = colors,
        };
        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;
        design.StartXOffset = design.NegativeX;
        design.StartYOffset = design.NegativeY;

        HusFormatHandler.AppendStitches(design, attributeData, xData, yData);
        return design;
    }
}
