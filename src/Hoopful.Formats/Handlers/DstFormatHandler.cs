namespace Hoopful.Formats.Handlers;

/// <summary>
/// Tajima <c>.dst</c> handler, ported from the original <c>DSTHandler</c>: an ASCII
/// header (stitch/colour counts and extents as text fields) followed by 3-byte ternary
/// movement records starting at offset 512. DST carries no colour values, so every
/// thread renders black, as in the original.
/// </summary>
public sealed class DstFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name };

        design.NumberOfStitches = int.Parse(new string(reader.ReadChars(23, 7)).Trim(), System.Globalization.CultureInfo.InvariantCulture);
        design.NumberOfColors = int.Parse(new string(reader.ReadChars(34, 3)).Trim(), System.Globalization.CultureInfo.InvariantCulture) + 1;
        design.PositiveX = int.Parse(new string(reader.ReadChars(41, 5)).Trim(), System.Globalization.CultureInfo.InvariantCulture);
        design.NegativeX = int.Parse(new string(reader.ReadChars(50, 5)).Trim(), System.Globalization.CultureInfo.InvariantCulture);
        design.PositiveY = int.Parse(new string(reader.ReadChars(59, 5)).Trim(), System.Globalization.CultureInfo.InvariantCulture);
        design.NegativeY = int.Parse(new string(reader.ReadChars(68, 5)).Trim(), System.Globalization.CultureInfo.InvariantCulture);

        design.Width = design.NegativeX + design.PositiveX;
        design.Height = design.NegativeY + design.PositiveY;
        design.StartXOffset = design.NegativeX;
        design.StartYOffset = design.NegativeY;
        design.Colors = ThreadPalettes.AllBlack(design.NumberOfColors);

        reader.SetPosition(512);
        long end = reader.Length - 1;

        for (long recordStart = 512; recordStart <= end; recordStart += 3)
        {
            if (reader.Position + 3 > reader.Length)
            {
                break; // the original swallowed the out-of-range read and stopped producing stitches
            }

            byte[] b = reader.ReadNextBytes(3);

            // 0xF3 anywhere in the record marked end-of-data in the original.
            if (b[0] == 243 || b[1] == 243 || b[2] == 243)
            {
                break;
            }

            // Ternary movement encoding: each bit adds/subtracts 1, 3, 9, 27 or 81.
            int x = 0, y = 0;
            if ((b[0] & 0x80) != 0) { y += 1; }
            if ((b[0] & 0x40) != 0) { y -= 1; }
            if ((b[0] & 0x20) != 0) { y += 9; }
            if ((b[0] & 0x10) != 0) { y -= 9; }
            if ((b[0] & 0x08) != 0) { x -= 9; }
            if ((b[0] & 0x04) != 0) { x += 9; }
            if ((b[0] & 0x02) != 0) { x -= 1; }
            if ((b[0] & 0x01) != 0) { x += 1; }

            if ((b[1] & 0x80) != 0) { y += 3; }
            if ((b[1] & 0x40) != 0) { y -= 3; }
            if ((b[1] & 0x20) != 0) { y += 27; }
            if ((b[1] & 0x10) != 0) { y -= 27; }
            if ((b[1] & 0x08) != 0) { x -= 27; }
            if ((b[1] & 0x04) != 0) { x += 27; }
            if ((b[1] & 0x02) != 0) { x -= 3; }
            if ((b[1] & 0x01) != 0) { x += 3; }

            if ((b[2] & 0x20) != 0) { y += 81; }
            if ((b[2] & 0x10) != 0) { y -= 81; }
            if ((b[2] & 0x08) != 0) { x -= 81; }
            if ((b[2] & 0x04) != 0) { x += 81; }

            // The top two bits of byte 3 classify the record.
            if ((b[2] & 0xC0) == 0xC0 && (b[2] & 0x03) == 0x03)
            {
                design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
            }

            if ((b[2] & 0xC0) == 0x00 && (x != 0 || y != 0))
            {
                design.Stitches.Add(new Stitch { X = x, Y = y, Type = StitchType.Normal });
            }

            if ((b[2] & 0x80) == 0x80 && (b[2] & 0x03) == 0x03)
            {
                design.Stitches.Add(new Stitch { X = x, Y = y, Type = StitchType.Jump });
            }
        }

        return design;
    }
}
