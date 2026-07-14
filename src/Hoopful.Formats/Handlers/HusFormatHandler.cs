using System.Buffers.Binary;
using System.Drawing;
using Hoopful.Formats.Compression;

namespace Hoopful.Formats.Handlers;

/// <summary>
/// Husqvarna/Viking <c>.hus</c> handler: parses the container, decompresses the three
/// stitch streams with the managed ArchiveLib decompressor and converts them into an
/// <see cref="EmbroideryDesign"/> exactly like the original <c>HUSHandler</c> (which
/// P/Invoked the native <c>al21mfc.dll</c> for the decompression).
/// </summary>
/// <remarks>
/// <para>File layout (all integers little endian):</para>
/// <code>
/// offset  size  field
///      0     4  magic code (observed values 0x00C8AF5B and 0x00C8FC5D)
///      4     4  number of stitches
///      8     4  number of colours
///     12     2  extent +X    (0.1 mm)
///     14     2  extent +Y
///     16     2  extent -X
///     18     2  extent -Y
///     20     4  offset of the compressed attribute (command) stream
///     24     4  offset of the compressed X stream
///     28     4  offset of the compressed Y stream
///     32     8  name string (zero padded)
///     40     2  unknown
///     42   2*n  thread palette indices, one 16-bit value per colour
/// </code>
/// <para>
/// The attribute stream runs from its offset to the X offset, the X stream to the
/// Y offset and the Y stream to the end of the file. Each stream is an ArchiveLib
/// level-4 stream that decompresses to exactly one byte per stitch.
/// </para>
/// </remarks>
public sealed class HusFormatHandler : IEmbroideryFormatHandler
{
    /// <summary>Fixed part of the header, before the per-colour palette indices.</summary>
    private const int FixedHeaderLength = 42;

    /// <summary>The two magic values observed in real files share this upper word.</summary>
    private const ushort MagicUpperWord = 0x00C8;

    /// <summary>Sanity cap; the original ArchiveLib cannot even address streams past 64 K stitches.</summary>
    public const int MaxStitchCount = 1_000_000;

    /// <summary>Sanity cap for the colour count.</summary>
    public const int MaxColorCount = 1_000;

    /// <summary>Returns true when <paramref name="fileData"/> starts with a HUS magic code.</summary>
    public static bool HasHusMagic(ReadOnlySpan<byte> fileData)
    {
        return fileData.Length >= 4
            && (BinaryPrimitives.ReadUInt32LittleEndian(fileData) >> 16) == MagicUpperWord;
    }

    /// <inheritdoc />
    /// <exception cref="ArchiveLibException">The file is not a valid HUS file.</exception>
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        ReadOnlySpan<byte> file = fileData;
        if (file.Length < FixedHeaderLength)
        {
            throw new ArchiveLibException(
                $"Not a HUS file: {file.Length} bytes is shorter than the {FixedHeaderLength}-byte header.");
        }

        uint magic = BinaryPrimitives.ReadUInt32LittleEndian(file);
        if (magic >> 16 != MagicUpperWord)
        {
            throw new ArchiveLibException($"Not a HUS file: unexpected magic code 0x{magic:X8}.");
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

        if (stitchCount is <= 0 or > MaxStitchCount)
        {
            throw new ArchiveLibException($"HUS header is corrupt: implausible stitch count {stitchCount}.");
        }

        if (colorCount is < 0 or > MaxColorCount)
        {
            throw new ArchiveLibException($"HUS header is corrupt: implausible colour count {colorCount}.");
        }

        // The palette table sits between the fixed header and the first stream.
        int paletteEnd = checked(FixedHeaderLength + (2 * colorCount));
        if (attributeOffset < paletteEnd || attributeOffset > xOffset
            || xOffset > yOffset || yOffset > file.Length)
        {
            throw new ArchiveLibException(
                $"HUS header is corrupt: stream offsets {attributeOffset}/{xOffset}/{yOffset} do not fit a {file.Length}-byte file.");
        }

        byte[] attributeData = DecompressStream(file[attributeOffset..xOffset], stitchCount, "attribute");
        byte[] xData = DecompressStream(file[xOffset..yOffset], stitchCount, "X");
        byte[] yData = DecompressStream(file[yOffset..], stitchCount, "Y");

        var design = new EmbroideryDesign
        {
            Name = name,
            NumberOfStitches = stitchCount,
            NumberOfColors = colorCount,
            PositiveX = Math.Max(extendPositiveX, (short)0),
            PositiveY = Math.Max(extendPositiveY, (short)0),
            // The original computed the negative extents as `0xFFFF - raw16`, which for a
            // stored two's-complement value is the magnitude minus one. Kept as-is so the
            // canvas matches the old renderer pixel for pixel.
            NegativeX = LegacyNegativeExtent(extendNegativeX),
            NegativeY = LegacyNegativeExtent(extendNegativeY),
        };
        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;
        design.StartXOffset = design.NegativeX;
        design.StartYOffset = design.NegativeY;

        // The original read the low byte of each 16-bit palette entry and looked it up in
        // the Husqvarna palette; unknown values were left at the default (rendered black).
        design.Colors = new Color[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            int paletteIndex = file[FixedHeaderLength + (2 * i)];
            design.Colors[i] = paletteIndex < ThreadPalettes.Husqvarna.Length
                ? ThreadPalettes.Husqvarna[paletteIndex]
                : Color.FromArgb(0, 0, 0);
        }

        AppendStitches(design, attributeData, xData, yData);
        return design;
    }

    /// <summary>Original formula: <c>0xFFFF - raw</c> (one less than the true magnitude).</summary>
    internal static int LegacyNegativeExtent(short storedExtent) =>
        storedExtent < 0 ? ushort.MaxValue - (ushort)storedExtent : 0;

    /// <summary>
    /// Decompresses one ArchiveLib level-4 stream that must expand to exactly one byte
    /// per stitch. Shared by the HUS and VIP handlers.
    /// </summary>
    internal static byte[] DecompressStream(ReadOnlySpan<byte> compressed, int stitchCount, string streamName)
    {
        try
        {
            return ArchiveLibDecompressor.Decompress(compressed, stitchCount);
        }
        catch (ArchiveLibException inner)
        {
            throw new ArchiveLibException($"The {streamName} stream is corrupt: {inner.Message}", inner);
        }
    }

    /// <summary>
    /// Converts the decompressed attribute/X/Y streams into stitches exactly like the
    /// original loop: 0x80 normal, 0x81 jump, 0x84 colour change; anything else —
    /// including 0x88 trim and the final 0x90 end marker — is kept but ignored by the
    /// renderer, matching the old behaviour. Shared by the HUS and VIP handlers.
    /// </summary>
    internal static void AppendStitches(EmbroideryDesign design, byte[] attributeData, byte[] xData, byte[] yData)
    {
        for (int i = 0; i < xData.Length; i++)
        {
            var stitch = new Stitch
            {
                Type = attributeData[i] switch
                {
                    0x80 => StitchType.Normal,
                    0x81 => StitchType.Jump,
                    0x84 => StitchType.ColorChange,
                    _ => StitchType.Ignored,
                },
                // The original converted with "> 128" (not ">= 128"), leaving a delta of
                // exactly 128 positive. Preserved.
                X = xData[i] > 128 ? xData[i] - 256 : xData[i],
                Y = yData[i] > 128 ? yData[i] - 256 : yData[i],
            };
            design.Stitches.Add(stitch);
        }
    }
}
