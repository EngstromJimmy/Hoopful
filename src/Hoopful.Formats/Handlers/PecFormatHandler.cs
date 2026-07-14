namespace Hoopful.Formats.Handlers;

/// <summary>
/// Brother <c>.pec</c> handler, ported from the original <c>PECHandler</c>: stitch data
/// from offset 0x214, decoded one axis at a time — a byte below 128 is a short-form
/// delta (re-reading the next byte as the start of the other axis), a byte of 128 or
/// more starts a two-byte long-form jump offset. Colours were not decoded (ten black
/// threads) and the canvas size was fixed; preserved.
/// </summary>
public sealed class PecFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign
        {
            Name = name,
            NumberOfColors = 10,
            Height = 1000,
            Width = 4000,
        };
        design.Colors = ThreadPalettes.AllBlack(design.NumberOfColors);

        const int StitchDataStart = 0x214;
        long size = reader.Length - 1 - StitchDataStart;
        reader.SetPosition(StitchDataStart);

        for (long recordIndex = 0; recordIndex <= size; recordIndex++)
        {
            if (reader.Position + 2 > reader.Length)
            {
                break; // the original swallowed the out-of-range read
            }

            byte[] b = reader.ReadNextBytes(2);

            // Colour change: 0xFE 0xB0 plus 3 bytes to skip.
            if (b[0] == 254 && b[1] == 176)
            {
                design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
                design.NumberOfColors++;
                if (reader.Position + 3 > reader.Length)
                {
                    break;
                }

                reader.ReadNextBytes(1);
                reader.ReadNextBytes(2);
                recordIndex += 3;
                continue;
            }

            // End of stitch data: 0xFF 0x00.
            if (b[0] == 255 && b[1] == 0)
            {
                break;
            }

            var stitch = new Stitch();

            // X axis.
            if (b[0] < 128)
            {
                // Short form: one byte; rewind so the second byte starts the Y axis.
                stitch.X = b[0] < 64 ? b[0] : b[0] - 128;
                reader.SetPosition(reader.Position - 1);
                stitch.Type = StitchType.Normal;
                recordIndex++;
            }
            else
            {
                // Long form: 12-bit offset across both bytes.
                int high = b[0] & 0x0F;
                stitch.X = high < 8 ? b[1] + (high * 256) : (b[1] - 256) + ((high - 15) * 256);
                stitch.Type = StitchType.Jump;
                recordIndex += 2;
            }

            if (reader.Position + 2 > reader.Length)
            {
                break;
            }

            b = reader.ReadNextBytes(2);

            // Y axis (same scheme; short-form values are negated).
            if (b[0] < 128)
            {
                stitch.Y = b[0] < 64 ? -b[0] : 128 - b[0];
                reader.SetPosition(reader.Position - 1);
                stitch.Type = StitchType.Normal;
                recordIndex++;
            }
            else
            {
                int high = b[0] & 0x0F;
                stitch.Y = high < 8 ? -(b[1] + (high * 256)) : (b[1] - 256) + ((high - 15) * 256);
                stitch.Type = StitchType.Jump;
                recordIndex += 2;
            }

            design.Stitches.Add(stitch);
        }

        // Fixed values from the original, applied after parsing.
        design.NegativeX = 580;
        design.NegativeY = 400;
        design.StartXOffset = design.NegativeX;
        design.StartYOffset = design.NegativeY;
        design.NegativeX = -design.NegativeX;
        design.NegativeY = -design.NegativeY;
        return design;
    }
}
