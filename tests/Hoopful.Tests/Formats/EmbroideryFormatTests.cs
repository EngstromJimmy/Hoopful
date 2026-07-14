using System.Drawing;
using Hoopful.Formats;
using Hoopful.Formats.Handlers;
using Xunit;

namespace Hoopful.Tests.Formats;

/// <summary>
/// Tests for the ported format handlers (Hoopful.Formats). HUS/VIP are checked against
/// the golden reference data; DST/EXP are checked against hand-built byte streams whose
/// expected decoding follows the original handler's rules.
/// </summary>
public sealed class EmbroideryFormatTests
{
    [Fact]
    public void SupportedExtensions_AllHaveHandlers()
    {
        foreach (string extension in EmbroideryFormatFactory.SupportedExtensions)
        {
            Assert.NotNull(EmbroideryFormatFactory.GetHandler("design" + extension));
        }

        Assert.Null(EmbroideryFormatFactory.GetHandler("design.png"));
        Assert.False(EmbroideryFormatFactory.IsSupported("design.txt"));
    }

    [Fact]
    public void Hus_SmallHeart_ParsesLikeOriginalHandler()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));

        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "small_heart.hus");

        Assert.Equal(889, design.NumberOfStitches);
        Assert.Equal(1, design.NumberOfColors);
        // Thread index 0 -> black in the Husqvarna palette.
        Assert.Equal(Color.FromArgb(0, 0, 0), Assert.Single(design.Colors));
        // Original extent quirk: negative extents come out one less than the magnitude.
        Assert.Equal(179, design.PositiveX);
        Assert.Equal(178, design.NegativeX);
        Assert.Equal(178, design.StartXOffset);
        Assert.Equal(889, design.Stitches.Count);
        // Deltas must match the golden decompressed streams byte for byte.
        byte[] xGolden = FixtureLoader.Get("hus_small_heart_x").Expected;
        for (int i = 0; i < design.Stitches.Count; i++)
        {
            int expected = xGolden[i] > 128 ? xGolden[i] - 256 : xGolden[i];
            Assert.Equal(expected, design.Stitches[i].X);
        }

        // 881 normal + 7 jumps + final 0x90 end byte (kept as Ignored).
        Assert.Equal(881, design.Stitches.Count(s => s.Type == StitchType.Normal));
        Assert.Equal(7, design.Stitches.Count(s => s.Type == StitchType.Jump));
        Assert.Equal(1, design.Stitches.Count(s => s.Type == StitchType.Ignored));
    }

    [Fact]
    public void Vip_Rose_ParsesWithDecodedColors()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "rose.vip"));

        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "rose.vip");

        Assert.Equal(842, design.NumberOfStitches);
        Assert.Equal(2, design.NumberOfColors);
        Assert.Equal(Color.FromArgb(226, 32, 60), design.Colors[0]);
        Assert.Equal(Color.FromArgb(255, 200, 40), design.Colors[1]);
        Assert.Equal(1, design.Stitches.Count(s => s.Type == StitchType.ColorChange));
    }

    [Fact]
    public void Dst_HandBuiltRecords_DecodeLikeOriginal()
    {
        // Header: text fields at fixed offsets; stitch data from 512.
        byte[] file = new byte[512 + 9];
        TestFileHelper.WriteAscii(file, 23, "     12");  // stitches
        TestFileHelper.WriteAscii(file, 34, "  1");      // colours - 1
        TestFileHelper.WriteAscii(file, 41, "  100");    // +X
        TestFileHelper.WriteAscii(file, 50, "   50");    // -X
        TestFileHelper.WriteAscii(file, 59, "   80");    // +Y
        TestFileHelper.WriteAscii(file, 68, "   40");    // -Y

        // Record 1: x+1 (bit 0 of byte 0), normal (b2 top bits 00; low bits per spec).
        file[512] = 0x01; file[513] = 0x00; file[514] = 0x00;
        // Record 2: colour change (b2 == 0xC3). Also matches the original's jump test,
        // which therefore adds a jump record too — behaviour preserved from the original.
        file[515] = 0x00; file[516] = 0x00; file[517] = 0xC3;
        // Record 3: end marker 0xF3.
        file[518] = 0xF3; file[519] = 0x00; file[520] = 0x00;

        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "test.dst");

        Assert.Equal(12, design.NumberOfStitches);
        Assert.Equal(2, design.NumberOfColors);
        Assert.Equal(150, design.Width);
        Assert.Equal(120, design.Height);
        Assert.Equal(50, design.StartXOffset);

        Assert.Equal(3, design.Stitches.Count);
        Assert.Equal(StitchType.Normal, design.Stitches[0].Type);
        Assert.Equal(1, design.Stitches[0].X);
        Assert.Equal(StitchType.ColorChange, design.Stitches[1].Type);
        Assert.Equal(StitchType.Jump, design.Stitches[2].Type);
    }

    [Fact]
    public void Exp_HandBuiltRecords_DecodeLikeOriginal()
    {
        // normal +3/+5; colour change; jump prefix 0x80 0x02 then -2/+5; normal -1/-1 (0xFF).
        byte[] file =
        [
            0x03, 0x05,
            0x80, 0x01,
            0x80, 0x02, 0xFE, 0x05,
            0xFF, 0xFF,
        ];

        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "test.exp");

        Assert.Equal(4, design.Stitches.Count);
        Assert.Equal(StitchType.Normal, design.Stitches[0].Type);
        Assert.Equal((3, 5), (design.Stitches[0].X, design.Stitches[0].Y));
        Assert.Equal(StitchType.ColorChange, design.Stitches[1].Type);
        Assert.Equal(StitchType.Jump, design.Stitches[2].Type);
        Assert.Equal((-2, 5), (design.Stitches[2].X, design.Stitches[2].Y));
        Assert.Equal((-1, -1), (design.Stitches[3].X, design.Stitches[3].Y));
        // 2 colours: the implicit first plus one change.
        Assert.Equal(2, design.NumberOfColors);
        // Extents tracked from the running position (3,5) -> (1,10) -> (0,9).
        Assert.Equal(3, design.PositiveX);
        Assert.Equal(10, design.PositiveY);
        Assert.Equal(0, design.NegativeX);
        Assert.Equal(3, design.Width);
        Assert.Equal(10, design.Height);
        // The original assigned start offsets before parsing, so they are always 0.
        Assert.Equal(0, design.StartXOffset);
    }

    [Fact]
    public void Exp_JumpPrecedenceQuirk_IsPreserved()
    {
        // The original's jump condition `(b0 == 0x80 && b1 == 0x02) || b1 == 0x04` fires
        // for ANY record whose second byte is 4: the record decodes as a normal stitch
        // AND consumes the following two bytes as a jump. Locked in by this test.
        byte[] file = [0x01, 0x04, 0x02, 0x03];

        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "test.exp");

        Assert.Equal(2, design.Stitches.Count);
        Assert.Equal(StitchType.Normal, design.Stitches[0].Type);
        Assert.Equal((1, 4), (design.Stitches[0].X, design.Stitches[0].Y));
        Assert.Equal(StitchType.Jump, design.Stitches[1].Type);
        Assert.Equal((2, 3), (design.Stitches[1].X, design.Stitches[1].Y));
    }

    [Fact]
    public void Ytlc_ColorOverride_ReplacesThreadColors()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));
        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "small_heart.hus");

        byte[] ytlc = System.Text.Encoding.UTF8.GetBytes(
            "<NewDataSet><Table><red>10</red><green>20</green><blue>30</blue></Table></NewDataSet>");

        Assert.True(EmbroideryFormatFactory.TryApplyColorOverride(design, ytlc));
        Assert.Equal(Color.FromArgb(10, 20, 30), Assert.Single(design.Colors));

        Assert.False(EmbroideryFormatFactory.TryApplyColorOverride(design, [0x01, 0x02]));
    }
}

internal static class TestFileHelper
{
    public static void WriteAscii(byte[] destination, int offset, string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            destination[offset + i] = (byte)text[i];
        }
    }
}
