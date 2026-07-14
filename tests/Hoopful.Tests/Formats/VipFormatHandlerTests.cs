using System.Drawing;
using Hoopful.Formats;
using Hoopful.Formats.Compression;
using Hoopful.Formats.Handlers;
using Xunit;

namespace Hoopful.Tests.Formats;

/// <summary>
/// Container-level tests for the VIP handler: header parsing, the XOR-obfuscated colour
/// table, validation of corrupt files, and byte-for-byte agreement of the decompressed
/// streams with the golden outputs produced by the reference C++ implementation.
/// </summary>
public sealed class VipFormatHandlerTests
{
    private static byte[] RoseVip() => FixtureLoader.ReadFile(Path.Combine("Files", "rose.vip"));

    private static EmbroideryDesign LoadRose() => EmbroideryFormatFactory.Load(RoseVip(), "rose.vip");

    [Fact]
    public void Read_ParsesHeaderAndMetadata()
    {
        EmbroideryDesign design = LoadRose();

        Assert.Equal(842, design.NumberOfStitches);
        Assert.Equal(2, design.NumberOfColors);
        Assert.Equal(106, design.PositiveX);
        Assert.Equal(67, design.PositiveY);
        // Legacy negative extents: magnitude - 1 (stored -106 / -120).
        Assert.Equal(105, design.NegativeX);
        Assert.Equal(119, design.NegativeY);
    }

    [Fact]
    public void Read_DecodesXorObfuscatedColorTable()
    {
        EmbroideryDesign design = LoadRose();

        Assert.Equal(Color.FromArgb(226, 32, 60), design.Colors[0]);
        Assert.Equal(Color.FromArgb(255, 200, 40), design.Colors[1]);
    }

    [Fact]
    public void Read_StitchesMatchReferenceGoldenStreams()
    {
        EmbroideryDesign design = LoadRose();

        byte[] attrGolden = FixtureLoader.Get("vip_rose_attr").Expected;
        byte[] xGolden = FixtureLoader.Get("vip_rose_x").Expected;
        byte[] yGolden = FixtureLoader.Get("vip_rose_y").Expected;

        Assert.Equal(attrGolden.Length, design.Stitches.Count);
        for (int i = 0; i < design.Stitches.Count; i++)
        {
            Stitch stitch = design.Stitches[i];
            Assert.Equal(xGolden[i] > 128 ? xGolden[i] - 256 : xGolden[i], stitch.X);
            Assert.Equal(yGolden[i] > 128 ? yGolden[i] - 256 : yGolden[i], stitch.Y);
        }

        Assert.Equal(design.NumberOfColors - 1, design.Stitches.Count(s => s.Type == StitchType.ColorChange));
        Assert.Equal(StitchType.Ignored, design.Stitches[^1].Type); // final 0x90 end marker
    }

    [Fact]
    public void Read_TooShortFile_Fails()
    {
        Assert.Throws<ArchiveLibException>(() => new VipFormatHandler().Read(new byte[10], "x.vip"));
    }

    [Fact]
    public void Read_WrongMagic_Fails()
    {
        // A HUS file is not a VIP file.
        byte[] husFile = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));

        Assert.Throws<ArchiveLibException>(() => new VipFormatHandler().Read(husFile, "x.vip"));
    }

    [Theory]
    [InlineData(20)] // attribute offset
    [InlineData(24)] // x offset
    [InlineData(28)] // y offset
    public void Read_CorruptStreamOffsets_FailSafely(int headerOffset)
    {
        byte[] original = RoseVip();

        foreach (int bogus in new[] { -1, 0, 10, 1_000_000, int.MaxValue })
        {
            byte[] file = (byte[])original.Clone();
            BitConverter.TryWriteBytes(file.AsSpan(headerOffset), bogus);

            Assert.Throws<ArchiveLibException>(() => new VipFormatHandler().Read(file, "x.vip"));
        }
    }

    [Fact]
    public void Read_UnreasonableStitchCount_Fails()
    {
        byte[] file = RoseVip();
        BitConverter.TryWriteBytes(file.AsSpan(4), int.MaxValue);

        Assert.Throws<ArchiveLibException>(() => new VipFormatHandler().Read(file, "x.vip"));
    }

    [Fact]
    public void Read_UnreasonableColorCount_Fails()
    {
        byte[] file = RoseVip();
        BitConverter.TryWriteBytes(file.AsSpan(8), 101);

        Assert.Throws<ArchiveLibException>(() => new VipFormatHandler().Read(file, "x.vip"));
    }

    [Fact]
    public void Read_TruncatedStreams_FailSafely()
    {
        byte[] file = RoseVip();

        for (int length = 46; length < file.Length; length += 17)
        {
            byte[] truncated = file.AsSpan(0, length).ToArray();
            try
            {
                new VipFormatHandler().Read(truncated, "x.vip");
            }
            catch (ArchiveLibException)
            {
                // Expected for most cut points; never any other exception type.
            }
        }
    }

    [Fact]
    public void HasVipMagic_DetectsFormat()
    {
        Assert.True(VipFormatHandler.HasVipMagic(RoseVip()));
        Assert.False(VipFormatHandler.HasVipMagic(FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"))));
        Assert.False(VipFormatHandler.HasVipMagic([1, 2]));
    }
}
