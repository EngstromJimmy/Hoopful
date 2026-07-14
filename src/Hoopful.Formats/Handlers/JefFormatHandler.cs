using System.Drawing;

namespace Hoopful.Formats.Handlers;

/// <summary>
/// Janome <c>.jef</c> handler, ported from the original <c>JEFHandler</c>: colour count
/// at offset 24, palette indices (Janome table) every 4 bytes from offset 116, and the
/// EXP-style 2-byte movement stream starting at the offset stored in the first byte of
/// the file. (The real format stores a 32-bit offset; the original only read its low
/// byte, which is preserved here.)
/// </summary>
public sealed class JefFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name };

        reader.SetPosition(0);
        int stitchDataStart = reader.ReadNextBytes(1)[0];
        design.NumberOfColors = reader.ReadBytes(24, 1)[0];

        design.Colors = new Color[design.NumberOfColors];
        reader.SetPosition(116);
        for (int i = 0; i < design.NumberOfColors; i++)
        {
            byte paletteIndex = reader.ReadNextBytes(1)[0];
            design.Colors[i] = paletteIndex < 78
                ? ThreadPalettes.Janome[paletteIndex]
                : ThreadPalettes.Janome[77];
            reader.ReadNextBytes(3);
        }

        reader.SetPosition(stitchDataStart);
        TwoByteRecordParser.ParseMovements(reader, design, stitchDataStart, trackExtents: true);

        design.NegativeX = -design.NegativeX;
        design.NegativeY = -design.NegativeY;
        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;
        design.StartXOffset = design.NegativeX;
        design.StartYOffset = design.NegativeY;
        return design;
    }
}
