using Hoopful.Formats.Compression;
using Xunit;

namespace Hoopful.Tests.Compression;

/// <summary>
/// Behaviour at the edges of the format: empty output, tiny inputs, window-sized
/// distances, maximum run lengths and multi-block streams.
/// </summary>
public sealed class ArchiveLibBoundaryTests
{
    [Fact]
    public void EofOnlyStream_ProducesEmptyOutput()
    {
        // Hand-crafted single-block stream containing just the end-of-stream symbol
        // (taken from the reference test-suite, where it is called "semivalid").
        Fixture fixture = FixtureLoader.Get("eof_only");

        byte[] result = ArchiveLibDecompressor.Decompress(fixture.Compressed, 0);

        Assert.Empty(result);
        Assert.True(ArchiveLibDecompressor.TryDecompress(fixture.Compressed, [], out int written));
        Assert.Equal(0, written);
    }

    [Fact]
    public void SingleByte_RoundTrips()
    {
        Fixture fixture = FixtureLoader.Get("single_byte");

        Assert.Equal(fixture.Expected, ArchiveLibDecompressor.Decompress(fixture.Compressed, 1));
    }

    [Fact]
    public void MaxDistanceBackReferences_DecodeAtLevel4()
    {
        // 16 KiB of noise followed by a copy of its first bytes: distances reach the
        // edge of the level-4 window (16384 bytes).
        Fixture fixture = FixtureLoader.Get("max_distance");

        byte[] actual = ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length);

        Assert.Equal(fixture.Expected, actual);
    }

    [Fact]
    public void MaximumRunLength_Decodes()
    {
        // A 256-byte block repeated 40 times compresses to maximum-length (256 byte) runs.
        Fixture fixture = FixtureLoader.Get("long_backrefs");

        Assert.Equal(fixture.Expected, ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length));
    }

    [Fact]
    public void OverlappingBackReferences_Decode()
    {
        // "AB" x 500 uses distance-2 copies that overlap their own output; the copy must
        // run forward byte by byte.
        Fixture fixture = FixtureLoader.Get("overlapping_backref");

        Assert.Equal(fixture.Expected, ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length));
    }

    [Fact]
    public void RepeatedSingleByte_UsesOffsetZeroRuns()
    {
        // 1000 identical bytes compress to offset-0 runs (repeat the previous byte).
        Fixture fixture = FixtureLoader.Get("repeated_byte");

        Assert.Equal(fixture.Expected, ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length));
    }

    [Fact]
    public void MultiBlockStreams_RebuildTablesPerBlock()
    {
        // 60,000 random bytes force the compressor to flush its item buffer repeatedly;
        // the reference produces 9 blocks for this input, each with fresh Huffman tables.
        Fixture fixture = FixtureLoader.Get("multi_block_random");

        Assert.Equal(fixture.Expected, ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length));
    }

    [Fact]
    public void TrailingPaddingAfterEofSymbol_IsIgnored()
    {
        // Appending garbage after a complete stream must not change the result: decoding
        // stops at the end-of-stream symbol.
        Fixture fixture = FixtureLoader.Get("short_backrefs");
        byte[] padded = [.. fixture.Compressed, 0xDE, 0xAD, 0xBE, 0xEF];

        Assert.Equal(fixture.Expected, ArchiveLibDecompressor.Decompress(padded, fixture.Expected.Length));
    }

    [Fact]
    public void SingleSymbolTables_ConsumeZeroBitsPerSymbol()
    {
        // repeated_byte and hus_trims both carry a single-symbol offset table (count
        // field 0): every offset is symbol 0 and consumes no bits. eof_only additionally
        // uses single-symbol code-length and literal tables.
        foreach (string name in new[] { "repeated_byte", "hus_trims", "eof_only" })
        {
            Fixture fixture = FixtureLoader.Get(name);

            byte[] actual = ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length);

            Assert.Equal(fixture.Expected, actual);
        }
    }
}
