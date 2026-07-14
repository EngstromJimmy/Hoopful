using System.Buffers.Binary;
using System.IO.Compression;

namespace Hoopful.Formats.Rendering;

/// <summary>
/// Minimal PNG encoder (8-bit RGB, no interlacing) so the raster renderer works without
/// System.Drawing, SkiaSharp or any native dependency — including in WebAssembly.
/// </summary>
internal static class PngEncoder
{
    /// <summary>Encodes an RGB pixel buffer (3 bytes per pixel, row-major) as a PNG file.</summary>
    public static byte[] Encode(byte[] rgbPixels, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (rgbPixels.Length != checked(width * height * 3))
        {
            throw new ArgumentException("Pixel buffer does not match the given dimensions.", nameof(rgbPixels));
        }

        using var output = new MemoryStream();

        // PNG signature.
        output.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        // IHDR: dimensions, 8-bit depth, colour type 2 (truecolour RGB).
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], height);
        header[8] = 8;  // bit depth
        header[9] = 2;  // colour type: RGB
        header[10] = 0; // compression
        header[11] = 0; // filter
        header[12] = 0; // interlace
        WriteChunk(output, "IHDR", header);

        // IDAT: zlib-compressed scanlines, each prefixed with filter type 0 (None).
        using (var idat = new MemoryStream())
        {
            using (var zlib = new ZLibStream(idat, CompressionLevel.Fastest, leaveOpen: true))
            {
                int stride = width * 3;
                for (int row = 0; row < height; row++)
                {
                    zlib.WriteByte(0);
                    zlib.Write(rgbPixels, row * stride, stride);
                }
            }

            WriteChunk(output, "IDAT", idat.GetBuffer().AsSpan(0, (int)idat.Length));
        }

        WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream output, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBytes, data.Length);
        output.Write(lengthBytes);

        Span<byte> typeBytes = stackalloc byte[4];
        for (int i = 0; i < 4; i++)
        {
            typeBytes[i] = (byte)type[i];
        }

        output.Write(typeBytes);
        output.Write(data);

        uint crc = Crc32(typeBytes, Crc32Seed);
        crc = Crc32(data, crc);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, ~crc);
        output.Write(crcBytes);
    }

    private const uint Crc32Seed = 0xFFFFFFFFu;

    private static readonly uint[] Crc32Table = BuildCrc32Table();

    private static uint[] BuildCrc32Table()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint value = n;
            for (int bit = 0; bit < 8; bit++)
            {
                value = (value & 1) != 0 ? 0xEDB88320u ^ (value >> 1) : value >> 1;
            }

            table[n] = value;
        }

        return table;
    }

    private static uint Crc32(ReadOnlySpan<byte> data, uint crc)
    {
        foreach (byte value in data)
        {
            crc = Crc32Table[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }
}
