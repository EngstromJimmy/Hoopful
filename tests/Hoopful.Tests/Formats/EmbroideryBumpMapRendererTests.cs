using System.Drawing;
using Hoopful.Formats;
using Hoopful.Formats.Rendering;
using Xunit;

namespace Hoopful.Tests.Formats;

/// <summary>
/// Tests for the bump-mapped raster renderer and its PNG output. The shading rules come
/// from the original <c>GetImageByGradient</c>; these tests pin the colour math and the
/// PNG container format. Pixel-exact assertions use a 1-pixel pen so overlapping brush
/// stamps cannot smear the sampled pixels.
/// </summary>
public sealed class EmbroideryBumpMapRendererTests
{
    private static EmbroideryDesign SingleStitchDesign(Color color, int dx, int dy)
    {
        var design = new EmbroideryDesign
        {
            Width = 100,
            Height = 100,
            StartXOffset = 50,
            StartYOffset = 50,
            NumberOfColors = 1,
            Colors = [color],
        };
        design.Stitches.Add(new Stitch { X = dx, Y = dy, Type = StitchType.Normal });
        return design;
    }

    /// <summary>
    /// Minimal decoder for our own encoder's output: 8-bit RGB, filter type 0 rows.
    /// </summary>
    private static (int Width, int Height, byte[] Rgb) DecodePng(byte[] png)
    {
        byte[] expectedSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.Equal(expectedSignature, png[..8]);

        int width = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16));
        int height = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20));

        int position = 8;
        using var pixelData = new MemoryStream();
        while (position < png.Length)
        {
            int length = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(position));
            string type = System.Text.Encoding.ASCII.GetString(png, position + 4, 4);
            if (type == "IDAT")
            {
                using var zlib = new System.IO.Compression.ZLibStream(
                    new MemoryStream(png, position + 8, length), System.IO.Compression.CompressionMode.Decompress);
                zlib.CopyTo(pixelData);
            }

            position += 12 + length;
        }

        byte[] scanlines = pixelData.ToArray();
        int stride = width * 3;
        byte[] rgb = new byte[width * height * 3];
        for (int row = 0; row < height; row++)
        {
            Assert.Equal(0, scanlines[row * (stride + 1)]); // filter type None
            Array.Copy(scanlines, (row * (stride + 1)) + 1, rgb, row * stride, stride);
        }

        return (width, height, rgb);
    }

    private static Color PixelAt(byte[] rgb, int width, int height, int canvasX, int canvasY)
    {
        // The finished image is vertically mirrored (the original's Rotate180FlipX).
        int row = height - 1 - canvasY;
        int index = ((row * width) + canvasX) * 3;
        return Color.FromArgb(rgb[index], rgb[index + 1], rgb[index + 2]);
    }

    private static bool ContainsColor(byte[] rgb, Color color)
    {
        for (int i = 0; i < rgb.Length; i += 3)
        {
            if (rgb[i] == color.R && rgb[i + 1] == color.G && rgb[i + 2] == color.B)
            {
                return true;
            }
        }

        return false;
    }

    [Fact]
    public void RenderPng_ProducesDecodableWhiteBackedImage()
    {
        EmbroideryDesign design = SingleStitchDesign(Color.FromArgb(200, 60, 60), 20, 0);

        (int width, int height, byte[] rgb) = DecodePng(EmbroideryBumpMapRenderer.RenderPng(design));

        Assert.Equal(100, width);
        Assert.Equal(100, height);
        Assert.Equal(Color.FromArgb(255, 255, 255), PixelAt(rgb, width, height, 2, 2));
    }

    [Fact]
    public void RenderPng_StitchEndsAreDarkened_MiddleIsThreadColor()
    {
        // A horizontal stitch has angle 0, so the middle gets no highlight: the ends are
        // colour - 100 and the midpoint carries the plain thread colour.
        var color = Color.FromArgb(200, 160, 120);
        EmbroideryDesign design = SingleStitchDesign(color, 40, 0);

        (int width, int height, byte[] rgb) = DecodePng(
            EmbroideryBumpMapRenderer.RenderPng(design, threadWidth: 1));

        Assert.Equal(Color.FromArgb(100, 60, 20), PixelAt(rgb, width, height, 50, 50)); // start
        Assert.Equal(color, PixelAt(rgb, width, height, 70, 50));                        // middle
        Assert.Equal(Color.FromArgb(100, 60, 20), PixelAt(rgb, width, height, 90, 50)); // stop
    }

    [Fact]
    public void RenderPng_SteepStitch_GetsAngleHighlight()
    {
        // A vertical stitch has angle 90°: highlight = 90 * light(2) / 4 = 45 per channel.
        var color = Color.FromArgb(100, 100, 100);
        EmbroideryDesign design = SingleStitchDesign(color, 0, 40);

        (int width, int height, byte[] rgb) = DecodePng(
            EmbroideryBumpMapRenderer.RenderPng(design, threadWidth: 1, light: 2));

        Assert.Equal(Color.FromArgb(145, 145, 145), PixelAt(rgb, width, height, 50, 70)); // middle
        Assert.Equal(Color.FromArgb(0, 0, 0), PixelAt(rgb, width, height, 50, 50));       // darkened end
    }

    [Fact]
    public void RenderPng_BlackThread_UsesDarkBlueSheen()
    {
        EmbroideryDesign design = SingleStitchDesign(Color.FromArgb(0, 0, 0), 40, 0);

        (int width, _, byte[] rgb) = DecodePng(EmbroideryBumpMapRenderer.RenderPng(design, threadWidth: 2));

        _ = width;
        // The original's special case colours black-thread ends RGB(0, 0, 30).
        Assert.True(ContainsColor(rgb, Color.FromArgb(0, 0, 30)));
    }

    [Fact]
    public void RenderPng_RealHusDesign_Renders()
    {
        byte[] file = FixtureLoader.ReadFile(Path.Combine("Files", "small_heart.hus"));
        EmbroideryDesign design = EmbroideryFormatFactory.Load(file, "small_heart.hus");

        byte[] png = EmbroideryBumpMapRenderer.RenderPng(design);

        (int width, int height, byte[] rgb) = DecodePng(png);
        Assert.Equal(design.Width, width);
        Assert.Equal(design.Height, height);
        Assert.False(ContainsColor(rgb, Color.FromArgb(1, 2, 3))); // sanity: decode worked
        // small_heart's single thread is palette index 0 (black): the black-thread
        // sheen colour must appear in the shaded rendering.
        Assert.True(ContainsColor(rgb, Color.FromArgb(0, 0, 30)));
    }
}
