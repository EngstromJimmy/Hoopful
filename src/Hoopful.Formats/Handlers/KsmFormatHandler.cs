namespace Hoopful.Formats.Handlers;

/// <summary>
/// Pfaff <c>.ksm</c> handler, ported from the original <c>KSMHandler</c>: 3-byte records
/// from offset 512 with magnitude bytes and sign bits in the third byte. A record whose
/// third byte matches <c>(b &amp; 0x19) == 0x19</c> is a colour change followed by a jump
/// movement. KSM designs are displayed with the alternative rotation
/// (<see cref="EmbroideryDesign.RotateForDisplay"/>), as in the original.
/// </summary>
public sealed class KsmFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name };

        int xPosition = 0;
        int yPosition = 0;
        reader.SetPosition(512);
        long size = reader.Length - 1;

        for (long recordStart = 512; recordStart <= size; recordStart += 3)
        {
            if (reader.Position + 3 > reader.Length)
            {
                break; // the original swallowed the out-of-range read
            }

            byte[] b = reader.ReadNextBytes(3);

            bool isColorChange = (b[2] & 0x19) == 0x19;
            if (isColorChange)
            {
                design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
                design.NumberOfColors++;
            }

            int x = b[0];
            int y = b[1];
            if ((b[2] & 0x40) != 0)
            {
                y = -y;
            }

            if ((b[2] & 0x20) != 0)
            {
                x = -x;
            }

            if (x != 0 || y != 0)
            {
                design.Stitches.Add(new Stitch
                {
                    X = x,
                    Y = y,
                    // After a colour change the movement is a jump; otherwise a stitch.
                    Type = isColorChange ? StitchType.Jump : StitchType.Normal,
                });
                if (!isColorChange)
                {
                    design.NumberOfStitches++;
                }
            }

            xPosition += x;
            yPosition += y;
            if (xPosition < design.NegativeX) { design.NegativeX = xPosition; }
            if (xPosition > design.PositiveX) { design.PositiveX = xPosition; }
            if (yPosition < design.NegativeY) { design.NegativeY = yPosition; }
            if (yPosition > design.PositiveY) { design.PositiveY = yPosition; }
        }

        design.RotateForDisplay = true;
        design.NegativeX = -design.NegativeX;
        design.NegativeY = -design.NegativeY;
        design.Colors = ThreadPalettes.AllBlack(design.NumberOfColors);
        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;
        return design;
    }
}
