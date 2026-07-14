using System.Drawing;
using System.Text;

namespace Hoopful.Formats.Handlers;

/// <summary>
/// Pfaff <c>.vp3</c> handler, ported from the original <c>VP3Handler</c>: a chunked
/// big-endian format with length-prefixed UTF-16 strings, a hoop description, and one
/// section per colour containing the thread colour and its stitch bytes.
/// </summary>
public sealed class Vp3FormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name };

        // Length-prefixed identity string right after the magic.
        int stringLength = BigEndian(reader.ReadBytes(6, 2));
        reader.ReadBytes(8, stringLength);

        int position = 8 + 7 + stringLength;
        stringLength = BigEndian(reader.ReadBytes(position, 2));
        position += stringLength + 2;

        // Hoop extents, stored in 1/100 mm; converted to 0.1 mm like everything else.
        design.PositiveX = BigEndian(reader.ReadBytes(position, 4)) / 100;
        position += 4;
        design.PositiveY = BigEndian(reader.ReadBytes(position, 4)) / 100;
        position += 4;
        design.NegativeX = BigEndian(reader.ReadBytes(position, 4)) / 100;
        position += 4;
        design.NegativeY = BigEndian(reader.ReadBytes(position, 4)) / 100;
        position += 4;

        position += 16; // unknown dwords
        position += 4;  // unknown x offset
        position += 4;  // unknown y offset
        position += 3;  // unknown bytes
        position += 16; // second (centered?) hoop copy

        design.Width = BigEndian(reader.ReadBytes(position, 4)) / 100;
        position += 4;
        design.Height = BigEndian(reader.ReadBytes(position, 4)) / 100;
        position += 4;

        position += 20; // unknowns
        position += 6;  // magic number
        stringLength = BigEndian(reader.ReadBytes(position, 2));
        position += stringLength + 2; // identity string again

        design.NumberOfColors = BigEndian(reader.ReadBytes(position, 2));
        design.Colors = new Color[design.NumberOfColors];
        design.StartXOffset = design.Width - design.PositiveX;
        design.StartYOffset = design.Height - design.PositiveY;
        position += 2;

        for (int colorIndex = 0; colorIndex < design.NumberOfColors; colorIndex++)
        {
            position += 3;
            int nextColorOffset = BigEndian(reader.ReadBytes(position, 4));
            position += 4;
            nextColorOffset += position;

            position += 8; // unknown x/y offset
            int extraThreadSections = reader.ReadBytes(position, 1)[0];
            position++;
            int blue = reader.ReadBytes(position, 1)[0];
            position++;
            int green = reader.ReadBytes(position, 1)[0];
            position++;
            int red = reader.ReadBytes(position, 1)[0];
            position++;
            position += 6 * extraThreadSections;

            design.Colors[colorIndex] = Color.FromArgb(red, green, blue);

            // Three length-prefixed colour description strings.
            for (int i = 0; i < 3; i++)
            {
                stringLength = BigEndian(reader.ReadBytes(position, 2));
                position += 2;
                _ = Encoding.ASCII.GetString(reader.ReadBytes(position, stringLength));
                position += stringLength;
            }

            position += 8; // unknown x/y offset

            stringLength = BigEndian(reader.ReadBytes(position, 2));
            position += 2;
            position += stringLength;

            int stitchByteCount = BigEndian(reader.ReadBytes(position, 4));
            position += 4;
            position += 3; // unknown bytes

            int stitchesDecoded = 0;
            while (stitchesDecoded < (stitchByteCount - 3) / 2 && nextColorOffset > position)
            {
                int xByte = reader.ReadBytes(position, 1)[0];
                position++;
                int yByte = reader.ReadBytes(position, 1)[0];
                position++;
                stitchesDecoded++;

                var stitch = new Stitch();
                if (xByte == 0x80)
                {
                    if (yByte is 0x00 or 0x03)
                    {
                        // 0x80 0x00 and 0x80 0x03 are skipped.
                        stitchesDecoded++;
                        continue;
                    }

                    // Long-form jump: two 16-bit big-endian deltas plus 2 stop bytes.
                    stitch.Type = StitchType.Jump;
                    int x = BigEndian(reader.ReadBytes(position, 2));
                    if ((x & 0x8000) != 0)
                    {
                        x -= 0x10000;
                    }

                    position += 2;
                    int y = BigEndian(reader.ReadBytes(position, 2));
                    if ((y & 0x8000) != 0)
                    {
                        y -= 0x10000;
                    }

                    position += 2;
                    position += 2; // stop bytes
                    stitchesDecoded += 3;
                    stitch.X = x;
                    stitch.Y = y;
                }
                else
                {
                    stitch.Type = StitchType.Normal;
                    stitch.X = xByte > 128 ? xByte - 256 : xByte;
                    stitch.Y = yByte > 128 ? yByte - 256 : yByte;
                }

                design.Stitches.Add(stitch);
            }

            design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });

            if (colorIndex != design.NumberOfColors - 1)
            {
                position = nextColorOffset;
            }
        }

        return design;
    }

    /// <summary>Big-endian byte combine (the original's corrected <c>GetIntegerFromBytes</c>).</summary>
    private static int BigEndian(byte[] bytes)
    {
        int result = 0;
        foreach (byte value in bytes)
        {
            result = (result << 8) | value;
        }

        return result;
    }
}
