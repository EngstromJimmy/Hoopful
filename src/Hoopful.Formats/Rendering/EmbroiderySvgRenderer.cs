using System.Drawing;
using System.Globalization;
using System.Text;

namespace Hoopful.Formats.Rendering;

/// <summary>
/// Renders an <see cref="EmbroideryDesign"/> to SVG using the same drawing rules as the
/// original application's bitmap renderer (<c>Embroidery.GetImage</c>):
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><description>The needle starts at <c>(StartXOffset + 10, StartYOffset + 10)</c>
///   on a <c>(Width + 20) × (Height + 20)</c> canvas.</description></item>
///   <item><description><see cref="StitchType.Normal"/> and <see cref="StitchType.Jump"/>
///   records draw a straight line in the current thread colour and advance the needle
///   (the original drew jump threads too).</description></item>
///   <item><description>A segment with any negative coordinate is skipped (not drawn) but
///   still advances the needle, exactly like the original's bounds check.</description></item>
///   <item><description><see cref="StitchType.ColorChange"/> advances to the next colour
///   and moves the needle by the record's movement.</description></item>
///   <item><description>Records with <see cref="StitchType.Ignored"/> (e.g. HUS trim/end
///   bytes) neither draw nor move — the original's <c>default:</c> case.</description></item>
///   <item><description>The finished image was flipped with <c>Rotate180FlipX</c>
///   (a vertical mirror), or <c>Rotate270FlipX</c> for designs with the rotate flag
///   (KSM); both are reproduced with an SVG transform.</description></item>
/// </list>
/// The output is vector rather than raster, so it scales cleanly; geometry, ordering and
/// colours match the original.
/// </remarks>
public static class EmbroiderySvgRenderer
{
    /// <summary>Margin the original renderer added around the design (10 units per side).</summary>
    private const int Margin = 10;

    /// <summary>
    /// Renders the design as an SVG document string.
    /// </summary>
    /// <param name="design">The design to draw.</param>
    /// <param name="threadWidth">Stroke width; the original browser used 2.</param>
    /// <param name="background">Optional background colour (the original used white).</param>
    public static string Render(EmbroideryDesign design, double threadWidth = 2, string background = "#ffffff")
    {
        int canvasWidth = design.Width + (2 * Margin);
        int canvasHeight = design.Height + (2 * Margin);

        // Guard against degenerate headers: fall back to bounds computed from the
        // stitches so a bad file still shows something instead of a zero-size image.
        if (canvasWidth <= 2 * Margin || canvasHeight <= 2 * Margin)
        {
            (canvasWidth, canvasHeight) = MeasureFallbackCanvas(design);
        }

        // Rotate180FlipX is a vertical mirror; Rotate270FlipX maps (x, y) -> (H - y, W - x).
        int viewportWidth = design.RotateForDisplay ? canvasHeight : canvasWidth;
        int viewportHeight = design.RotateForDisplay ? canvasWidth : canvasHeight;
        string transform = design.RotateForDisplay
            ? $"matrix(0,-1,-1,0,{canvasHeight},{canvasWidth})"
            : $"translate(0,{canvasHeight}) scale(1,-1)";

        var svg = new StringBuilder();
        svg.Append(CultureInfo.InvariantCulture,
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {viewportWidth} {viewportHeight}\" width=\"{viewportWidth}\" height=\"{viewportHeight}\">");
        if (!string.IsNullOrEmpty(background))
        {
            svg.Append(CultureInfo.InvariantCulture,
                $"<rect width=\"{viewportWidth}\" height=\"{viewportHeight}\" fill=\"{background}\"/>");
        }

        svg.Append(CultureInfo.InvariantCulture, $"<g transform=\"{transform}\" fill=\"none\" stroke-width=\"{threadWidth}\" stroke-linecap=\"round\">");

        AppendStitchPaths(svg, design);

        svg.Append("</g></svg>");
        return svg.ToString();
    }

    /// <summary>
    /// Walks the stitches exactly like the original render loop, emitting one SVG path
    /// per continuous run of same-coloured drawn segments.
    /// </summary>
    private static void AppendStitchPaths(StringBuilder svg, EmbroideryDesign design)
    {
        int colorNumber = 0;
        int x = design.StartXOffset + Margin;
        int y = design.StartYOffset + Margin;

        var path = new StringBuilder();
        bool pathHasSegments = false;
        bool needleAtPathEnd = false;
        string currentColor = ColorToSvg(CurrentColor(design, colorNumber));

        void FlushPath()
        {
            if (pathHasSegments)
            {
                svg.Append(CultureInfo.InvariantCulture, $"<path stroke=\"{currentColor}\" d=\"{path}\"/>");
            }

            path.Clear();
            pathHasSegments = false;
            needleAtPathEnd = false;
        }

        foreach (Stitch stitch in design.Stitches)
        {
            switch (stitch.Type)
            {
                case StitchType.Normal when stitch.X == 0 && stitch.Y == 0:
                    // The original skipped zero-length normal stitches entirely
                    // (no draw, no movement).
                    break;

                case StitchType.Normal:
                case StitchType.Jump:
                    int endX = x + stitch.X;
                    int endY = y + stitch.Y;

                    // Original bounds check: only draw when both endpoints are
                    // non-negative; the movement happens regardless.
                    if (x >= 0 && y >= 0 && endX >= 0 && endY >= 0)
                    {
                        if (!needleAtPathEnd)
                        {
                            path.Append(CultureInfo.InvariantCulture, $"M{x} {y}");
                        }

                        path.Append(CultureInfo.InvariantCulture, $"L{endX} {endY}");
                        pathHasSegments = true;
                        needleAtPathEnd = true;
                    }
                    else
                    {
                        needleAtPathEnd = false;
                    }

                    x = endX;
                    y = endY;
                    break;

                case StitchType.ColorChange:
                    FlushPath();
                    colorNumber++;
                    currentColor = ColorToSvg(CurrentColor(design, colorNumber));
                    x += stitch.X;
                    y += stitch.Y;
                    break;

                default:
                    // Ignored/Stop: the original's default case — no draw, no movement.
                    break;
            }
        }

        FlushPath();
    }

    /// <summary>
    /// Colour lookup with the safety the original lacked: it indexed past the array when
    /// a file contained more colour changes than declared colours (and crashed); this
    /// port clamps to the last colour, or black when none exist.
    /// </summary>
    private static Color CurrentColor(EmbroideryDesign design, int colorNumber)
    {
        if (design.Colors.Length == 0)
        {
            return Color.FromArgb(0, 0, 0);
        }

        return design.Colors[Math.Min(colorNumber, design.Colors.Length - 1)];
    }

    private static string ColorToSvg(Color color) => $"#{color.R:x2}{color.G:x2}{color.B:x2}";

    /// <summary>
    /// Canvas size for designs whose headers yield no usable extents: simulate the needle
    /// movements and size the canvas to the positive quadrant, as the drawn area is
    /// clipped to it anyway.
    /// </summary>
    private static (int Width, int Height) MeasureFallbackCanvas(EmbroideryDesign design)
    {
        int x = design.StartXOffset + Margin;
        int y = design.StartYOffset + Margin;
        int maxX = x;
        int maxY = y;

        foreach (Stitch stitch in design.Stitches)
        {
            if (stitch.Type is StitchType.Normal or StitchType.Jump or StitchType.ColorChange)
            {
                x += stitch.X;
                y += stitch.Y;
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        return (maxX + Margin, maxY + Margin);
    }
}
