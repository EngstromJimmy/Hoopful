namespace Hoopful.Formats.Handlers;

/// <summary>
/// Melco <c>.exp</c> handler, ported from the original <c>EXPHandler</c>: a headerless
/// stream of 2-byte records. <c>0x80 0x01</c> is a colour change, <c>0x80 0x02/0x04</c>
/// prefixes a jump, anything else is a signed-byte movement. Extents are computed while
/// parsing because the format has no header. EXP carries no colours; threads render black.
/// </summary>
public sealed class ExpFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name, NumberOfColors = 1 };

        reader.SetPosition(0);
        TwoByteRecordParser.ParseMovements(reader, design, reader.Position, trackExtents: true);

        design.Colors = ThreadPalettes.AllBlack(design.NumberOfColors);

        design.NegativeX = -design.NegativeX;
        design.NegativeY = -design.NegativeY;
        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;

        // The original assigned the start offsets before parsing, when the extents were
        // still zero, so EXP designs start at the canvas origin (and segments that go
        // negative are clipped). Preserved for identical rendering.
        design.StartXOffset = 0;
        design.StartYOffset = 0;
        return design;
    }
}

/// <summary>
/// The 2-byte movement loop shared verbatim by the original EXP, JEF and SEW handlers.
/// </summary>
internal static class TwoByteRecordParser
{
    /// <summary>
    /// Parses 2-byte records until the end of the data, appending stitches to
    /// <paramref name="design"/>. When <paramref name="trackExtents"/> is set the running
    /// position updates Positive/Negative extents (stored signed; callers negate the
    /// negative side afterwards, as the original did).
    /// </summary>
    public static void ParseMovements(ByteReader reader, EmbroideryDesign design, int startPosition, bool trackExtents)
    {
        int xPosition = 0;
        int yPosition = 0;
        long size = reader.Length - 1 - startPosition;

        for (long recordIndex = 0; recordIndex <= size; recordIndex += 2)
        {
            if (reader.Position + 2 > reader.Length)
            {
                break; // the original swallowed the out-of-range read
            }

            byte[] b = reader.ReadNextBytes(2);

            // Colour change.
            if (b[0] == 0x80 && b[1] == 0x01)
            {
                design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
                design.NumberOfColors++;
            }

            // Normal movement: both bytes are signed deltas. Note this branch also runs
            // for records that the jump branch below picks up when b[1] == 0x04 — the
            // original's operator precedence made it so, and files rely on the net effect.
            if (b[0] != 0x80)
            {
                int x = b[0] > 128 ? -(256 - b[0]) : b[0];
                int y = b[1] > 128 ? -(256 - b[1]) : b[1];

                if (x != 0 || y != 0)
                {
                    design.Stitches.Add(new Stitch { X = x, Y = y, Type = StitchType.Normal });
                    design.NumberOfStitches++;
                }

                xPosition += x;
                yPosition += y;
                if (trackExtents)
                {
                    UpdateExtents(design, xPosition, yPosition);
                }
            }

            // Jump: the original condition was `(b0 == 0x80 && b1 == 0x02) || b1 == 0x04`
            // — the b1 == 0x04 arm fires regardless of b0. Preserved.
            if ((b[0] == 0x80 && b[1] == 0x02) || b[1] == 0x04)
            {
                if (reader.Position + 2 > reader.Length)
                {
                    break;
                }

                b = reader.ReadNextBytes(2);
                int x = b[0] > 128 ? -(256 - b[0]) : b[0];
                int y = b[1] > 128 ? -(256 - b[1]) : b[1];

                recordIndex += 2;
                if (x != 0 || y != 0)
                {
                    design.Stitches.Add(new Stitch { X = x, Y = y, Type = StitchType.Jump });
                }

                xPosition += x;
                yPosition += y;
                if (trackExtents)
                {
                    UpdateExtents(design, xPosition, yPosition);
                }
            }
        }
    }

    private static void UpdateExtents(EmbroideryDesign design, int xPosition, int yPosition)
    {
        if (xPosition < design.NegativeX)
        {
            design.NegativeX = xPosition;
        }

        if (xPosition > design.PositiveX)
        {
            design.PositiveX = xPosition;
        }

        if (yPosition < design.NegativeY)
        {
            design.NegativeY = yPosition;
        }

        if (yPosition > design.PositiveY)
        {
            design.PositiveY = yPosition;
        }
    }
}
