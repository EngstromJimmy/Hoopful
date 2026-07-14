namespace Hoopful.Formats.Handlers;

/// <summary>
/// Singer <c>.xxx</c> handler, ported from the original <c>XXXHandler</c>: 2-byte signed
/// movement records from offset 253; <c>0x7F 0x08</c> is a colour change (plus two bytes
/// to skip) and <c>0x7F 0x7F</c> ends the data. The original never decoded extents or
/// colours for XXX, so extents are computed from the movements here (the original would
/// otherwise produce a zero-size canvas) and threads render black.
/// </summary>
public sealed class XxxFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name };

        reader.SetPosition(253);
        long size = reader.Length - 1;
        int xPosition = 0;
        int yPosition = 0;

        for (long recordIndex = 0; recordIndex <= size; recordIndex += 2)
        {
            if (reader.Position + 2 > reader.Length)
            {
                break; // the original swallowed the out-of-range read
            }

            byte[] b = reader.ReadNextBytes(2);

            // Colour change: 0x7F 0x08 plus two bytes to skip.
            if (b[0] == 0x7F && b[1] == 0x08)
            {
                design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
                design.NumberOfColors++;
                if (reader.Position + 2 > reader.Length)
                {
                    break;
                }

                reader.ReadNextBytes(2);
            }

            // Normal signed movement.
            if (b[0] != 0x7F && b[1] != 0x08)
            {
                int x = b[0] >= 128 ? b[0] - 256 : b[0];
                int y = b[1] >= 128 ? b[1] - 256 : b[1];

                if (x != 0 || y != 0)
                {
                    design.Stitches.Add(new Stitch { X = x, Y = y, Type = StitchType.Normal });
                    design.NumberOfStitches++;
                    xPosition += x;
                    yPosition += y;
                    if (xPosition < -design.NegativeX) { design.NegativeX = -xPosition; }
                    if (xPosition > design.PositiveX) { design.PositiveX = xPosition; }
                    if (yPosition < -design.NegativeY) { design.NegativeY = -yPosition; }
                    if (yPosition > design.PositiveY) { design.PositiveY = yPosition; }
                }
            }

            // End of stitch data.
            if (b[0] == 0x7F && b[1] == 0x7F)
            {
                break;
            }
        }

        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;
        design.StartXOffset = design.NegativeX;
        design.StartYOffset = design.NegativeY;
        design.Colors = ThreadPalettes.AllBlack(Math.Max(design.NumberOfColors, 1));
        return design;
    }
}
