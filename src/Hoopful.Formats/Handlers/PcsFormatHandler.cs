namespace Hoopful.Formats.Handlers;

/// <summary>
/// Pfaff <c>.pcs</c> handler, ported from the original <c>PCSHandler</c>: 9-byte records
/// from offset 70 holding absolute coordinates, converted to relative movements against
/// the running position. The original decoded the coordinate bytes with a shift-combine
/// that effectively keeps only the low byte; that behaviour is preserved. PCS colours
/// were fixed at 16 black threads.
/// </summary>
/// <remarks>
/// One deliberate fix over the original: it never assigned a stitch type to movement
/// records, so its renderer silently skipped every PCS stitch and produced an empty
/// image. Movement records are typed <see cref="StitchType.Normal"/> here so the design
/// is actually visible.
/// </remarks>
public sealed class PcsFormatHandler : IEmbroideryFormatHandler
{
    /// <inheritdoc />
    public EmbroideryDesign Read(byte[] fileData, string name)
    {
        var reader = new ByteReader(fileData);
        var design = new EmbroideryDesign { Name = name, NumberOfColors = 16 };
        design.Colors = ThreadPalettes.AllBlack(design.NumberOfColors);

        reader.SetPosition(70);
        int xPosition = 0;
        int yPosition = 0;

        for (long recordStart = 70; recordStart < reader.Length - 9; recordStart += 9)
        {
            byte[] b = reader.ReadNextBytes(9);
            var stitch = new Stitch();
            switch (b[8])
            {
                case 0:
                case 2:
                    stitch.X = LegacyCombine(b, 1, 3) - xPosition;
                    stitch.Y = LegacyCombine(b, 5, 7) - yPosition;
                    stitch.Type = StitchType.Normal; // the original left this unset (see remarks)
                    xPosition += stitch.X;
                    yPosition += stitch.Y;
                    break;
                case 3:
                    stitch.Type = StitchType.ColorChange;
                    break;
                default:
                    break; // unknown records stay Ignored, as in the original
            }

            design.Stitches.Add(stitch);
        }

        return design;
    }

    /// <summary>
    /// The original's <c>GetIntegerFromBytes</c>: iterates high-to-low doing
    /// <c>result = (result &gt;&gt; 8) | b[i]</c>, which mostly reduces to the lowest byte.
    /// Preserved bit for bit.
    /// </summary>
    private static int LegacyCombine(byte[] bytes, int start, int stop)
    {
        int result = 0;
        for (int i = stop; i >= start; i--)
        {
            result = (result >> 8) | bytes[i];
        }

        return result;
    }
}
