namespace Hoopful.Formats.Handlers;

/// <summary>
/// Brother <c>.pes</c> handler, ported from the original <c>PESHandler</c>: locates the
/// embedded PEC block via the offset at byte 8, reads the hoop size at PEC+520 and the
/// 2-byte stitch records from PEC+529. The original had no PES colour support and used
/// ten black threads; preserved.
/// </summary>
public sealed class PesFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name, NumberOfColors = 10 };
        design.Colors = ThreadPalettes.AllBlack(design.NumberOfColors);

        design.Stitches.Add(new Stitch { Type = StitchType.Jump });

        // The original shifted the fourth byte by 32 bits, which .NET masks to a shift of
        // zero — so it is OR-ed in unshifted. Real files have 0 there. Preserved.
        byte[] offsetBytes = reader.ReadBytes(8, 4);
        int pecStart = offsetBytes[0] | (offsetBytes[1] << 8) | (offsetBytes[2] << 16) | offsetBytes[3];

        reader.SetPosition(pecStart + 520);
        byte[] widthBytes = reader.ReadNextBytes(2);
        design.PositiveX = widthBytes[0] | (widthBytes[1] << 8);
        byte[] heightBytes = reader.ReadNextBytes(2);
        design.PositiveY = heightBytes[0] | (heightBytes[1] << 8);
        design.Width = design.PositiveX;
        design.Height = design.PositiveY;

        reader.SetPosition(pecStart + 529);
        long size = reader.Length - 1;

        for (long recordIndex = 0; recordIndex <= size; recordIndex += 2)
        {
            if (reader.Position + 2 > reader.Length)
            {
                break; // the original swallowed the out-of-range read
            }

            byte[] b = reader.ReadNextBytes(2);

            // Colour change: 0xFE 0xB0, followed by 3 bytes to skip.
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
            }

            // Normal short-form stitch: both bytes below 128.
            if (b[0] < 128 && b[1] < 128)
            {
                int x = b[0] >= 63 ? b[0] - 128 : b[0];
                int y = b[1] >= 63 ? 128 - b[1] : b[1];

                if (x != 0 || y != 0)
                {
                    design.Stitches.Add(new Stitch { X = x, Y = y, Type = StitchType.Normal });
                    design.NumberOfStitches++;
                }
            }

            // Long-form jump: first byte in 129..254. The original decoded the offset and
            // then zeroed it, so jumps do not move the needle; preserved.
            if (b[0] > 128 && b[0] <= 254)
            {
                if (reader.Position + 2 > reader.Length)
                {
                    break;
                }

                reader.ReadNextBytes(2);
                design.Stitches.Add(new Stitch { X = 0, Y = 0, Type = StitchType.Jump });
            }

            // End of stitch data: 0xFF 0x00.
            if (b[0] == 255 && b[1] == 0)
            {
                break;
            }
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
