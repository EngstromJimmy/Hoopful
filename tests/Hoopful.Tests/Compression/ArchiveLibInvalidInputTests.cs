using Hoopful.Formats.Compression;
using Xunit;

namespace Hoopful.Tests.Compression;

/// <summary>
/// Malformed, truncated and hostile inputs must fail safely: <c>Decompress</c> throws
/// <see cref="ArchiveLibException"/>, <c>TryDecompress</c> returns false, and nothing
/// hangs, over-allocates or reads out of bounds.
/// </summary>
public sealed class ArchiveLibInvalidInputTests
{
    /// <summary>Builds a byte array from a string of '0'/'1' characters (spaces ignored).</summary>
    private static byte[] FromBits(string bits)
    {
        string clean = bits.Replace(" ", "", StringComparison.Ordinal);
        int byteCount = (clean.Length + 7) / 8;
        byte[] result = new byte[byteCount];
        for (int i = 0; i < clean.Length; i++)
        {
            if (clean[i] == '1')
            {
                result[i / 8] |= (byte)(0x80 >> (i % 8));
            }
        }

        return result;
    }

    private static void AssertFailsSafely(byte[] input, int expectedLength)
    {
        Assert.Throws<ArchiveLibException>(() => ArchiveLibDecompressor.Decompress(input, expectedLength));

        byte[] destination = new byte[expectedLength];
        // TryDecompress may legitimately succeed with a different length for inputs whose
        // corruption is only detectable via the expected length; it must never throw.
        bool success = ArchiveLibDecompressor.TryDecompress(input, destination, out int bytesWritten);
        if (success)
        {
            Assert.NotEqual(expectedLength, bytesWritten);
        }
    }

    [Fact]
    public void EmptyInput_Fails()
    {
        Assert.Throws<ArchiveLibException>(() => ArchiveLibDecompressor.Decompress([], 0));
        Assert.False(ArchiveLibDecompressor.TryDecompress([], new byte[16], out _));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void TruncatedHeader_Fails(int length)
    {
        byte[] full = FixtureLoader.Get("hus_small_heart_x").Compressed;
        AssertFailsSafely(full.AsSpan(0, length).ToArray(), 889);
    }

    [Theory]
    [InlineData(5.0)]   // inside the Huffman tables
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.95)]
    public void TruncatedStream_Fails(double cutAt)
    {
        byte[] full = FixtureLoader.Get("hus_small_heart_x").Compressed;
        int length = cutAt >= 1 ? (int)cutAt : (int)(full.Length * cutAt);
        AssertFailsSafely(full.AsSpan(0, length).ToArray(), 889);
    }

    [Fact]
    public void IncompleteHuffmanTable_Fails()
    {
        // 1 item; the 19-symbol code-length table declares two 2-bit codes, which covers
        // only half of the code space (Kraft sum 2^15 of 2^16).
        byte[] input = FromBits("0000000000000001 00010 010 010");
        AssertFailsSafely(input, 16);
    }

    [Fact]
    public void OversubscribedHuffmanTable_Fails()
    {
        // Three 1-bit codes: Kraft sum 3 * 2^15 > 2^16.
        byte[] input = FromBits("0000000000000001 00011 001 001 001");
        AssertFailsSafely(input, 16);
    }

    [Fact]
    public void SingleSymbolTable_ValueOutOfRange_Fails()
    {
        // Code-length table sent as "single symbol 20", but the alphabet has 19 symbols.
        byte[] input = FromBits("0000000000000001 00000 10100");
        AssertFailsSafely(input, 16);
    }

    [Fact]
    public void CodeLengthAbove16_Fails()
    {
        // 0xFF everywhere: the first code length is 7 extended by endless unary 1-bits.
        byte[] input = new byte[64];
        Array.Fill(input, (byte)0xFF);
        AssertFailsSafely(input, 1024);
    }

    [Fact]
    public void BackReferenceBeforeStartOfOutput_Fails()
    {
        // Single-symbol tables make every code zero bits wide: the first item is length
        // symbol 256 (a 3-byte run) with offset 0 — but there is no history yet.
        byte[] input = FromBits("0000000000000010 00000 00000 000000000 100000000 00000 00000");
        AssertFailsSafely(input, 16);
    }

    [Fact]
    public void BackReferenceBeyondWindow_FailsAtLowerLevel()
    {
        // max_distance contains back-references close to the level-4 window limit of
        // 16384 bytes; at level 0 the window is only 1024 bytes, so decoding must fail.
        Fixture fixture = FixtureLoader.Get("max_distance");

        Assert.Throws<ArchiveLibException>(() => ArchiveLibDecompressor.Decompress(
            fixture.Compressed, fixture.Expected.Length, ArchiveLibCompressionLevel.Level0));
    }

    [Fact]
    public void OutputLargerThanDestination_Fails()
    {
        Fixture fixture = FixtureLoader.Get("repeated_byte"); // expands to 1000 bytes

        Assert.Throws<ArchiveLibException>(
            () => ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length - 1));

        byte[] tooSmall = new byte[fixture.Expected.Length - 1];
        Assert.False(ArchiveLibDecompressor.TryDecompress(fixture.Compressed, tooSmall, out _));
    }

    [Fact]
    public void OutputSmallerThanExpectedLength_Fails()
    {
        Fixture fixture = FixtureLoader.Get("repeated_byte");

        Assert.Throws<ArchiveLibException>(
            () => ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length + 1));
    }

    [Fact]
    public void NegativeExpectedLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ArchiveLibDecompressor.Decompress(new byte[4], -1));
    }

    [Fact]
    public void HugeExpectedLength_IsRejectedWithoutAllocating()
    {
        // A corrupt header must not be able to request an absurd allocation.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ArchiveLibDecompressor.Decompress(new byte[4], int.MaxValue));
    }

    [Fact]
    public void InvalidLevel_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ArchiveLibDecompressor.Decompress(
            new byte[4], 4, (ArchiveLibCompressionLevel)9));
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x55)]
    [InlineData(0xAA)]
    public void ConstantGarbageInput_TerminatesQuickly(byte filler)
    {
        // Streams of repeated bytes must terminate (no endless loop) with either an error
        // or a bounded amount of output — never hang.
        byte[] input = new byte[4096];
        Array.Fill(input, filler);

        byte[] destination = new byte[1 << 16];
        try
        {
            ArchiveLibDecompressor.TryDecompress(input, destination, out _);
        }
        catch (ArchiveLibException)
        {
            // Also acceptable — TryDecompress swallows it anyway; keep for clarity.
        }
    }

    [Fact]
    public void RandomGarbageInput_NeverThrowsFromTryDecompress()
    {
        var random = new Random(20260714);
        byte[] destination = new byte[1 << 16];

        for (int round = 0; round < 200; round++)
        {
            byte[] input = new byte[random.Next(1, 512)];
            random.NextBytes(input);

            // Must not throw, hang, or corrupt memory; result value is irrelevant.
            ArchiveLibDecompressor.TryDecompress(input, destination, out _);
        }
    }

    [Fact]
    public void BitFlippedFixtures_FailSafely()
    {
        Fixture fixture = FixtureLoader.Get("hus_small_heart_x");
        byte[] destination = new byte[fixture.Expected.Length];

        for (int byteIndex = 0; byteIndex < fixture.Compressed.Length; byteIndex += 7)
        {
            byte[] corrupted = (byte[])fixture.Compressed.Clone();
            corrupted[byteIndex] ^= 0x40;

            // Any outcome except an unhandled exception or a hang is fine.
            ArchiveLibDecompressor.TryDecompress(corrupted, destination, out _);
        }
    }
}
