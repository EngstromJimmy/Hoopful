using Hoopful.Formats;
using Hoopful.Formats.Rendering;
using Xunit;

namespace Hoopful.Tests.Formats;

/// <summary>
/// The SVG renderer must reproduce the original bitmap renderer's drawing rules; these
/// tests pin the geometry, colour handling and the legacy display flips.
/// </summary>
public sealed class EmbroiderySvgRendererTests
{
    private static EmbroideryDesign TwoColorDesign()
    {
        var design = new EmbroideryDesign
        {
            Width = 100,
            Height = 100,
            StartXOffset = 50,
            StartYOffset = 50,
            NumberOfColors = 2,
            Colors = [System.Drawing.Color.FromArgb(255, 0, 0), System.Drawing.Color.FromArgb(0, 0, 255)],
        };
        design.Stitches.Add(new Stitch { X = 10, Y = 0, Type = StitchType.Normal });
        design.Stitches.Add(new Stitch { X = 0, Y = 10, Type = StitchType.Normal });
        design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
        design.Stitches.Add(new Stitch { X = -10, Y = 0, Type = StitchType.Jump });
        design.Stitches.Add(new Stitch { X = 5, Y = 5, Type = StitchType.Normal });
        return design;
    }

    [Fact]
    public void Render_EmitsOnePathPerColorRun()
    {
        string svg = EmbroiderySvgRenderer.Render(TwoColorDesign());

        // Canvas = design size + 10 margin per side; default flip = vertical mirror.
        Assert.Contains("viewBox=\"0 0 120 120\"", svg, StringComparison.Ordinal);
        Assert.Contains("translate(0,120) scale(1,-1)", svg, StringComparison.Ordinal);

        // First colour: two chained segments starting at (60, 60).
        Assert.Contains("<path stroke=\"#ff0000\" d=\"M60 60L70 60L70 70\"/>", svg, StringComparison.Ordinal);
        // Second colour: jump then stitch, drawn as one run.
        Assert.Contains("<path stroke=\"#0000ff\" d=\"M70 70L60 70L65 75\"/>", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_SkipsSegmentsWithNegativeCoordinates_ButMovesTheNeedle()
    {
        var design = new EmbroideryDesign
        {
            Width = 100,
            Height = 100,
            Colors = [System.Drawing.Color.FromArgb(0, 0, 0)],
            NumberOfColors = 1,
        };
        // StartOffsets are 0, so the needle starts at (10, 10): moving -20 goes negative.
        design.Stitches.Add(new Stitch { X = -20, Y = 0, Type = StitchType.Normal });
        design.Stitches.Add(new Stitch { X = 30, Y = 0, Type = StitchType.Normal });
        design.Stitches.Add(new Stitch { X = 5, Y = 0, Type = StitchType.Normal });

        string svg = EmbroiderySvgRenderer.Render(design);

        // Segment 1 ends negative and segment 2 starts negative: both are clipped away
        // (the original's bounds check), but the needle still moves, so segment 3 draws
        // from (20, 10).
        Assert.DoesNotContain("M10 10", svg, StringComparison.Ordinal);
        Assert.DoesNotContain("M-10", svg, StringComparison.Ordinal);
        Assert.Contains("M20 10L25 10", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_IgnoredStitches_DoNotDrawOrMove()
    {
        var design = new EmbroideryDesign
        {
            Width = 50,
            Height = 50,
            Colors = [System.Drawing.Color.FromArgb(0, 0, 0)],
            NumberOfColors = 1,
        };
        design.Stitches.Add(new Stitch { X = 5, Y = 5, Type = StitchType.Ignored });
        design.Stitches.Add(new Stitch { X = 5, Y = 0, Type = StitchType.Normal });

        string svg = EmbroiderySvgRenderer.Render(design);

        // The ignored record must not have advanced the needle from (10, 10).
        Assert.Contains("M10 10L15 10", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_RotateForDisplay_UsesTransposedViewport()
    {
        EmbroideryDesign design = TwoColorDesign();
        design.RotateForDisplay = true;

        string svg = EmbroiderySvgRenderer.Render(design);

        Assert.Contains("matrix(0,-1,-1,0,120,120)", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_MoreColorChangesThanColors_ClampsInsteadOfCrashing()
    {
        var design = new EmbroideryDesign
        {
            Width = 50,
            Height = 50,
            Colors = [System.Drawing.Color.FromArgb(1, 2, 3)],
            NumberOfColors = 1,
        };
        design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
        design.Stitches.Add(new Stitch { Type = StitchType.ColorChange });
        design.Stitches.Add(new Stitch { X = 5, Y = 5, Type = StitchType.Normal });

        string svg = EmbroiderySvgRenderer.Render(design);

        Assert.Contains("#010203", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_RealHusDesign_ProducesPathsForAllThreads()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "star.hus"));
        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "star.hus");

        string svg = EmbroiderySvgRenderer.Render(design);

        // star.hus has 2 threads; the drawing must contain runs in both colours.
        Assert.Equal(2, design.Colors.Length);
        Assert.Contains($"stroke=\"#{design.Colors[0].R:x2}{design.Colors[0].G:x2}{design.Colors[0].B:x2}\"", svg, StringComparison.Ordinal);
        Assert.Contains($"stroke=\"#{design.Colors[1].R:x2}{design.Colors[1].G:x2}{design.Colors[1].B:x2}\"", svg, StringComparison.Ordinal);
    }
}
