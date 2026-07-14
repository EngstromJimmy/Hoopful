using Hoopful.Formats.Compression;
using Xunit;

namespace Hoopful.Tests.Compression;

/// <summary>
/// Byte-for-byte comparison of the decompressor's output against golden vectors produced
/// by the reference C++ implementation (software-opal/archivelib-rs).
/// </summary>
public sealed class ArchiveLibGoldenVectorTests
{
    public static TheoryData<string> FixtureNames()
    {
        var data = new TheoryData<string>();
        foreach (Fixture fixture in FixtureLoader.All)
        {
            data.Add(fixture.Name);
        }

        return data;
    }

    private static ArchiveLibCompressionLevel LevelOf(Fixture fixture) =>
        (ArchiveLibCompressionLevel)fixture.Level;

    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void Decompress_MatchesReferenceOutput(string name)
    {
        Fixture fixture = FixtureLoader.Get(name);

        byte[] actual = ArchiveLibDecompressor.Decompress(
            fixture.Compressed, fixture.Expected.Length, LevelOf(fixture));

        Assert.Equal(fixture.Expected, actual);
    }

    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void TryDecompress_IntoExactSizedBuffer_MatchesReferenceOutput(string name)
    {
        Fixture fixture = FixtureLoader.Get(name);
        byte[] destination = new byte[fixture.Expected.Length];

        bool success = ArchiveLibDecompressor.TryDecompress(
            fixture.Compressed, destination, LevelOf(fixture), out int bytesWritten);

        Assert.True(success);
        Assert.Equal(fixture.Expected.Length, bytesWritten);
        Assert.Equal(fixture.Expected, destination);
    }

    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void TryDecompress_IntoOversizedBuffer_ReportsExactLength(string name)
    {
        Fixture fixture = FixtureLoader.Get(name);
        byte[] destination = new byte[fixture.Expected.Length + 64];

        bool success = ArchiveLibDecompressor.TryDecompress(
            fixture.Compressed, destination, LevelOf(fixture), out int bytesWritten);

        Assert.True(success);
        Assert.Equal(fixture.Expected.Length, bytesWritten);
        Assert.Equal(fixture.Expected, destination.AsSpan(0, bytesWritten).ToArray());
    }

    [Fact]
    public void Decompress_IsDeterministic()
    {
        Fixture fixture = FixtureLoader.Get("hus_paris1_x");

        byte[] first = ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length);
        byte[] second = ArchiveLibDecompressor.Decompress(fixture.Compressed, fixture.Expected.Length);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Level4_DecodesStreamsCompressedAtLowerLevels()
    {
        // The level only bounds back-reference distances, so a higher level always
        // decodes data compressed at a lower one.
        Fixture fixture = FixtureLoader.Get("level0_text");

        byte[] actual = ArchiveLibDecompressor.Decompress(
            fixture.Compressed, fixture.Expected.Length, ArchiveLibCompressionLevel.Level4);

        Assert.Equal(fixture.Expected, actual);
    }
}
