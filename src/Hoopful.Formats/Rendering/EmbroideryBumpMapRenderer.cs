using System.Drawing;

namespace Hoopful.Formats.Rendering;

/// <summary>
/// Raster renderer with the original application's "3D" bump-mapped thread shading,
/// ported from <c>Embroidery.GetImageByGradient</c> (with <c>threed = true</c> and
/// <c>Bumpmapping = true</c>).
/// </summary>
/// <remarks>
/// <para>The original shading model, reproduced here:</para>
/// <list type="bullet">
///   <item><description>Every stitch is split at its midpoint and drawn as two gradient
///   segments: end colour → middle colour → end colour.</description></item>
///   <item><description>The end colour is the thread colour darkened by 100 per channel
///   (clamped), so thread ends read as shadowed.</description></item>
///   <item><description>The middle colour is the thread colour lightened by
///   <c>angle × light / 4</c>, where <c>angle</c> is the stitch's inclination in degrees
///   (steeper stitches catch more light) and <c>light</c> is the light intensity
///   parameter (the original used 2).</description></item>
///   <item><description>When the gradient runs bottom-up the endpoints are swapped, the
///   midpoint is left as computed (an original quirk kept for identical output).</description></item>
///   <item><description>Black threads get the original's special case: end colours of
///   RGB(0, 0, 30) and a slightly wider pen.</description></item>
///   <item><description>Jumps move the needle without drawing (unlike the flat renderer,
///   which follows the original pixel renderer and draws them).</description></item>
/// </list>
/// <para>
/// Output is an in-memory PNG produced by a fully managed rasterizer + encoder, so it
/// runs unchanged in WebAssembly. The original drew with GDI+ pens; this port draws
/// anti-alias-free round-capped thick lines, which matches the original's chunky look.
/// </para>
/// </remarks>
public static class EmbroideryBumpMapRenderer
{
    /// <summary>Canvas pixel budget; the scale divisor grows until the canvas fits.</summary>
    private const long MaxCanvasPixels = 12_000_000;

    private readonly record struct Rgb(int R, int G, int B)
    {
        public static Rgb FromColor(Color color) => new(color.R, color.G, color.B);

        public Rgb Clamp() => new(Math.Clamp(R, 0, 255), Math.Clamp(G, 0, 255), Math.Clamp(B, 0, 255));
    }

    /// <summary>
    /// Renders the design with 3D thread shading and returns PNG file bytes.
    /// </summary>
    /// <param name="design">The design to draw.</param>
    /// <param name="threadWidth">Pen width in canvas pixels (the original default pens were 2–3 wide).</param>
    /// <param name="light">Light intensity for the angle-based highlight; the original used 2.</param>
    /// <param name="pixelScale">
    /// Coordinate divisor (the original's <c>quility</c> parameter): 1 renders at full
    /// resolution, larger values shrink the canvas. Automatically increased when the
    /// canvas would exceed an internal pixel budget.
    /// </param>
    public static byte[] RenderPng(EmbroideryDesign design, int threadWidth = 3, double light = 2, int pixelScale = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threadWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pixelScale);

        // The gradient renderer used the bare design size (no margin); keep a small
        // margin only as bounds slack via per-pixel clipping.
        int scale = pixelScale;
        while (((long)(design.Width / scale) + 1) * ((design.Height / scale) + 1) > MaxCanvasPixels)
        {
            scale++;
        }

        int canvasWidth = Math.Max(design.Width / scale, 1);
        int canvasHeight = Math.Max(design.Height / scale, 1);

        var canvas = new RasterCanvas(canvasWidth, canvasHeight);

        int colorIndex = 0;
        int xBefore = design.StartXOffset;
        int yBefore = design.StartYOffset;

        foreach (Stitch stitch in design.Stitches)
        {
            switch (stitch.Type)
            {
                case StitchType.Normal when stitch.X != 0 || stitch.Y != 0:
                {
                    var start = (X: xBefore / scale, Y: yBefore / scale);
                    var middle = (X: (xBefore + (stitch.X / 2)) / scale, Y: (yBefore + (stitch.Y / 2)) / scale);
                    var stop = (X: (xBefore + stitch.X) / scale, Y: (yBefore + stitch.Y) / scale);

                    Color threadColor = CurrentColor(design, colorIndex);
                    var endColor = new Rgb(threadColor.R - 100, threadColor.G - 100, threadColor.B - 100).Clamp();

                    // Angle-based highlight: steeper stitches catch more light.
                    double angleRadians = Math.Asin(stitch.Y / Math.Sqrt(((double)stitch.Y * stitch.Y) + ((double)stitch.X * stitch.X)));
                    int angleDegrees = Math.Abs((int)Math.Round(angleRadians * (180 / Math.PI), MidpointRounding.ToEven));
                    double highlight = angleDegrees * light / 4;

                    // Original quirk: bottom-up gradients swap the endpoints only.
                    if (start.Y > stop.Y)
                    {
                        (start, stop) = (stop, start);
                    }

                    var middleColor = new Rgb(
                        threadColor.R + (int)highlight,
                        threadColor.G + (int)highlight,
                        threadColor.B + (int)highlight).Clamp();

                    int penWidth = threadWidth;
                    if (start != middle && stop != middle)
                    {
                        if (threadColor.R == 0 && threadColor.G == 0 && threadColor.B == 0)
                        {
                            // Black threads: dark blue sheen and a wider pen, as in the original.
                            endColor = new Rgb(0, 0, 30);
                            penWidth = threadWidth + 1;
                        }

                        canvas.DrawGradientLine(start, middle, endColor, middleColor, penWidth);
                        canvas.DrawGradientLine(middle, stop, middleColor, endColor, penWidth);
                    }
                    else
                    {
                        // Degenerate (very short) stitch: solid thread colour.
                        canvas.DrawGradientLine(start, stop, Rgb.FromColor(threadColor), Rgb.FromColor(threadColor), penWidth);
                    }

                    xBefore += stitch.X;
                    yBefore += stitch.Y;
                    break;
                }

                case StitchType.Jump:
                    // The gradient renderer moved without drawing jumps.
                    xBefore += stitch.X;
                    yBefore += stitch.Y;
                    break;

                case StitchType.ColorChange:
                    colorIndex++;
                    xBefore += stitch.X;
                    yBefore += stitch.Y;
                    break;

                default:
                    break; // Ignored/Stop: no draw, no movement.
            }
        }

        byte[] pixels = design.RotateForDisplay
            ? canvas.ToRotated270FlippedRgb(out canvasWidth, out canvasHeight)
            : canvas.ToVerticallyMirroredRgb();

        return PngEncoder.Encode(pixels, canvasWidth, canvasHeight);
    }

    private static Color CurrentColor(EmbroideryDesign design, int colorIndex)
    {
        if (design.Colors.Length == 0)
        {
            return Color.FromArgb(0, 0, 0);
        }

        return design.Colors[Math.Min(colorIndex, design.Colors.Length - 1)];
    }

    /// <summary>RGB canvas with a gradient thick-line rasterizer.</summary>
    private sealed class RasterCanvas
    {
        private readonly byte[] _pixels; // 3 bytes per pixel, row-major, top-down
        private readonly int _width;
        private readonly int _height;

        public RasterCanvas(int width, int height)
        {
            _width = width;
            _height = height;
            _pixels = new byte[width * height * 3];
            _pixels.AsSpan().Fill(0xFF); // white background, like the original's whitener
        }

        /// <summary>
        /// Draws a straight line from <paramref name="from"/> to <paramref name="to"/>,
        /// interpolating the colour along its length. Thickness is applied as a round
        /// brush stamped at every Bresenham step; pixels outside the canvas are clipped,
        /// matching how GDI+ clipped the original's out-of-bounds stitches.
        /// </summary>
        public void DrawGradientLine((int X, int Y) from, (int X, int Y) to, Rgb fromColor, Rgb toColor, int penWidth)
        {
            int deltaX = Math.Abs(to.X - from.X);
            int deltaY = Math.Abs(to.Y - from.Y);
            int stepX = Math.Sign(to.X - from.X);
            int stepY = Math.Sign(to.Y - from.Y);
            int stepCount = Math.Max(deltaX, deltaY);

            int x = from.X;
            int y = from.Y;
            int error = deltaX - deltaY;

            for (int step = 0; step <= stepCount; step++)
            {
                double t = stepCount == 0 ? 0 : (double)step / stepCount;
                var color = new Rgb(
                    (int)(fromColor.R + ((toColor.R - fromColor.R) * t)),
                    (int)(fromColor.G + ((toColor.G - fromColor.G) * t)),
                    (int)(fromColor.B + ((toColor.B - fromColor.B) * t)));

                StampBrush(x, y, color, penWidth);

                int doubledError = 2 * error;
                if (doubledError > -deltaY)
                {
                    error -= deltaY;
                    x += stepX;
                }

                if (doubledError < deltaX)
                {
                    error += deltaX;
                    y += stepY;
                }
            }
        }

        /// <summary>Stamps a filled disc of diameter <paramref name="penWidth"/> at (x, y).</summary>
        private void StampBrush(int centerX, int centerY, Rgb color, int penWidth)
        {
            int radius = penWidth / 2;
            int radiusSquared = Math.Max(radius * radius, 1);
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                for (int offsetX = -radius; offsetX <= radius; offsetX++)
                {
                    if ((offsetX * offsetX) + (offsetY * offsetY) > radiusSquared)
                    {
                        continue;
                    }

                    SetPixel(centerX + offsetX, centerY + offsetY, color);
                }
            }
        }

        private void SetPixel(int x, int y, Rgb color)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return;
            }

            int index = ((y * _width) + x) * 3;
            _pixels[index] = (byte)color.R;
            _pixels[index + 1] = (byte)color.G;
            _pixels[index + 2] = (byte)color.B;
        }

        /// <summary>The original's final <c>Rotate180FlipX</c>: a vertical mirror.</summary>
        public byte[] ToVerticallyMirroredRgb()
        {
            var mirrored = new byte[_pixels.Length];
            int stride = _width * 3;
            for (int row = 0; row < _height; row++)
            {
                Array.Copy(_pixels, row * stride, mirrored, (_height - 1 - row) * stride, stride);
            }

            return mirrored;
        }

        /// <summary>
        /// The original's <c>Rotate270FlipX</c> (used for KSM): maps source (x, y) to
        /// destination (height - 1 - y, width - 1 - x) on a transposed canvas.
        /// </summary>
        public byte[] ToRotated270FlippedRgb(out int outWidth, out int outHeight)
        {
            outWidth = _height;
            outHeight = _width;
            var rotated = new byte[_pixels.Length];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int source = ((y * _width) + x) * 3;
                    int destinationX = _height - 1 - y;
                    int destinationY = _width - 1 - x;
                    int destination = ((destinationY * outWidth) + destinationX) * 3;
                    rotated[destination] = _pixels[source];
                    rotated[destination + 1] = _pixels[source + 1];
                    rotated[destination + 2] = _pixels[source + 2];
                }
            }

            return rotated;
        }
    }
}
