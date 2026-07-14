using System.Drawing;

namespace Hoopful.Formats.Handlers;

/// <summary>
/// Janome/Elna <c>.sew</c> handler, ported from the original <c>SEWHandler</c>: colour
/// count in the first byte, Janome palette indices every 2 bytes from offset 2, and the
/// EXP-style 2-byte movement stream at the fixed offset 7544.
/// </summary>
public sealed class SewFormatHandler : IEmbroideryFormatHandler
{
    private const int StitchDataStart = 7544;

    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name };

        design.NumberOfColors = reader.ReadBytes(0, 1)[0];

        design.Colors = new Color[design.NumberOfColors];
        reader.SetPosition(2);
        for (int i = 0; i < design.NumberOfColors; i++)
        {
            byte paletteIndex = reader.ReadNextBytes(1)[0];
            design.Colors[i] = paletteIndex < 78
                ? ThreadPalettes.Janome[paletteIndex]
                : ThreadPalettes.Janome[77];
            reader.ReadNextBytes(1);
        }

        reader.SetPosition(StitchDataStart);
        TwoByteRecordParser.ParseMovements(reader, design, StitchDataStart, trackExtents: true);

        design.NegativeX = -design.NegativeX;
        design.NegativeY = -design.NegativeY;
        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;
        design.StartXOffset = design.NegativeX;
        design.StartYOffset = design.NegativeY;
        return design;
    }
}
