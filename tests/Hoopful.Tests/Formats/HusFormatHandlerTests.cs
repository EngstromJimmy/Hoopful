using Hoopful.Formats;
using Hoopful.Formats.Compression;
using Hoopful.Formats.Handlers;
using Xunit;

namespace Hoopful.Tests.Formats;

/// <summary>
/// Container-level tests for the HUS handler: header parsing, validation of corrupt
/// files, and byte-for-byte agreement of the decompressed streams with the golden
/// outputs produced by the reference C++ implementation.
/// </summary>
public sealed class HusFormatHandlerTests
{
    /// <summary>name, stitches, colours, +X extent, legacy -X extent (magnitude - 1).</summary>
    public static TheoryData<string, int, int, int, int> RealFiles => new()
    {
        { "small_heart", 889, 1, 179, 178 },
        { "star", 2557, 2, 296, 294 },
        { "paris1", 18395, 7, 736, 702 },
    };

    private static EmbroideryDesign Load(string fileName) =>
        EmbroideryFormatFactory.Load(FixtureLoader.ReadFile(Path.Combine("Files", fileName)), fileName);

    [Theory]
    [MemberData(nameof(RealFiles))]
    public void Read_ParsesHeaderAndMetadata(string name, int stitches, int colors, int positiveX, int legacyNegativeX)
    {
        EmbroideryDesign design = Load(name + ".hus");

        Assert.Equal(stitches, design.NumberOfStitches);
        Assert.Equal(colors, design.NumberOfColors);
        Assert.Equal(positiveX, design.PositiveX);
        Assert.Equal(legacyNegativeX, design.NegativeX);
        Assert.Equal(design.NegativeX, design.StartXOffset);
        Assert.Equal(design.NegativeX + design.PositiveX, design.Width);
    }

    [Theory]
    [MemberData(nameof(RealFiles))]
    public void Read_StitchesMatchReferenceGoldenStreams(string name, int stitches, int colors, int positiveX, int legacyNegativeX)
    {
        _ = colors;
        _ = positiveX;
        _ = legacyNegativeX;
        EmbroideryDesign design = Load(name + ".hus");

        // One stitch record per stream byte; deltas and types must match the golden
        // decompressed streams produced by the reference implementation.
        Assert.Equal(stitches, design.Stitches.Count);
        byte[] attrGolden = FixtureLoader.Get($"hus_{name}_attr").Expected;
        byte[] xGolden = FixtureLoader.Get($"hus_{name}_x").Expected;
        byte[] yGolden = FixtureLoader.Get($"hus_{name}_y").Expected;

        for (int i = 0; i < stitches; i++)
        {
            Stitch stitch = design.Stitches[i];
            Assert.Equal(xGolden[i] > 128 ? xGolden[i] - 256 : xGolden[i], stitch.X);
            Assert.Equal(yGolden[i] > 128 ? yGolden[i] - 256 : yGolden[i], stitch.Y);

            StitchType expectedType = attrGolden[i] switch
            {
                0x80 => StitchType.Normal,
                0x81 => StitchType.Jump,
                0x84 => StitchType.ColorChange,
                _ => StitchType.Ignored,
            };
            Assert.Equal(expectedType, stitch.Type);
        }
    }

    [Theory]
    [MemberData(nameof(RealFiles))]
    public void Read_StitchCommandsAreReasonable(string name, int stitches, int colors, int positiveX, int legacyNegativeX)
    {
        _ = stitches;
        _ = positiveX;
        _ = legacyNegativeX;
        EmbroideryDesign design = Load(name + ".hus");

        // A design with n colours has n - 1 colour changes, and always ends with the
        // 0x90 end marker (kept as an Ignored record).
        Assert.Equal(colors - 1, design.Stitches.Count(s => s.Type == StitchType.ColorChange));
        Assert.Equal(StitchType.Ignored, design.Stitches[^1].Type);
    }

    [Fact]
    public void Read_TooShortFile_Fails()
    {
        Assert.Throws<ArchiveLibException>(() => new HusFormatHandler().Read(new byte[10], "x.hus"));
    }

    [Fact]
    public void Read_WrongMagic_Fails()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));
        file[3] = 0x77;

        Assert.Throws<ArchiveLibException>(() => new HusFormatHandler().Read(file, "x.hus"));
    }

    [Theory]
    [InlineData(20)] // attribute offset
    [InlineData(24)] // x offset
    [InlineData(28)] // y offset
    public void Read_CorruptStreamOffsets_FailSafely(int headerOffset)
    {
        byte[] original = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));

        foreach (int bogus in new[] { -1, 0, 5, 1_000_000, int.MaxValue })
        {
            byte[] file = (byte[])original.Clone();
            BitConverter.TryWriteBytes(file.AsSpan(headerOffset), bogus);

            Assert.Throws<ArchiveLibException>(() => new HusFormatHandler().Read(file, "x.hus"));
        }
    }

    [Fact]
    public void Read_UnreasonableStitchCount_Fails()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));
        BitConverter.TryWriteBytes(file.AsSpan(4), int.MaxValue);

        Assert.Throws<ArchiveLibException>(() => new HusFormatHandler().Read(file, "x.hus"));
    }

    [Fact]
    public void Read_UnreasonableColorCount_Fails()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));
        BitConverter.TryWriteBytes(file.AsSpan(8), 500_000);

        Assert.Throws<ArchiveLibException>(() => new HusFormatHandler().Read(file, "x.hus"));
    }

    [Fact]
    public void Read_TruncatedStreams_FailSafely()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));

        for (int length = 44; length < file.Length; length += 101)
        {
            byte[] truncated = file.AsSpan(0, length).ToArray();
            try
            {
                new HusFormatHandler().Read(truncated, "x.hus");
            }
            catch (ArchiveLibException)
            {
                // Expected for most cut points; never any other exception type.
            }
        }
    }

    [Fact]
    public void HasHusMagic_DetectsFormat()
    {
        Assert.True(HusFormatHandler.HasHusMagic(FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"))));
        Assert.False(HusFormatHandler.HasHusMagic(FixtureLoader.ReadFile(Path.Combine("Files", "rose.vip"))));
        Assert.False(HusFormatHandler.HasHusMagic([1, 2]));
    }
}
