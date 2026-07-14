using BenchmarkDotNet.Attributes;
using Hoopful.Formats;
using Hoopful.Formats.Compression;
using Hoopful.Formats.Handlers;

namespace Hoopful.Benchmarks;

/// <summary>
/// Decompression throughput and allocation benchmarks over the golden fixtures.
/// </summary>
/// <remarks>
/// <see cref="MemoryDiagnoserAttribute"/> reports allocated bytes and Gen 0 collections.
/// Mean time per operation gives operations per second; each benchmark's
/// <c>OutputBytes</c> column value divided by the mean time gives decompressed bytes per
/// second. <c>DecompressReusingBuffer</c> variants show the allocation-free path
/// (no allocation beyond the fixed decoder state).
/// </remarks>
[MemoryDiagnoser]
[HideColumns("StdDev", "Median")]
public class DecompressionBenchmarks
{
    private static string FixtureDirectory => Path.Combine(AppContext.BaseDirectory, "Fixtures");

    /// <summary>fixture name -> (compressed, expected length)</summary>
    private readonly Dictionary<string, (byte[] Compressed, int OutputLength)> _fixtures = [];

    private byte[] _reusableBuffer = [];

    [ParamsSource(nameof(FixtureNames))]
    public string Fixture { get; set; } = "";

    public static IEnumerable<string> FixtureNames =>
    [
        "short_backrefs",      // small block, 42 bytes output
        "hus_small_heart_x",   // small real HUS block
        "hus_paris1_x",        // medium real HUS block, 18 KB output
        "multi_block_text",    // large repetitive stream, 64 KB output
        "multi_block_random",  // large mostly-literal stream, 60 KB output
        "repeated_byte",       // highly repetitive, offset-0 runs
    ];

    [GlobalSetup]
    public void Setup()
    {
        foreach (string name in FixtureNames)
        {
            byte[] compressed = File.ReadAllBytes(Path.Combine(FixtureDirectory, name + ".cmp"));
            int outputLength = (int)new FileInfo(Path.Combine(FixtureDirectory, name + ".out")).Length;
            _fixtures[name] = (compressed, outputLength);
        }

        _reusableBuffer = new byte[_fixtures.Values.Max(f => f.OutputLength)];
    }

    /// <summary>Decompressed output size, for computing bytes/second from the mean time.</summary>
    public int OutputBytes => _fixtures[Fixture].OutputLength;

    [Benchmark]
    public byte[] DecompressAllocating()
    {
        (byte[] compressed, int outputLength) = _fixtures[Fixture];
        return ArchiveLibDecompressor.Decompress(compressed, outputLength);
    }

    [Benchmark]
    public int DecompressReusingBuffer()
    {
        (byte[] compressed, _) = _fixtures[Fixture];
        if (!ArchiveLibDecompressor.TryDecompress(compressed, _reusableBuffer, out int bytesWritten))
        {
            throw new InvalidOperationException("Benchmark fixture failed to decompress.");
        }

        return bytesWritten;
    }
}

/// <summary>End-to-end parse of a full HUS file (header + three streams + conversion).</summary>
[MemoryDiagnoser]
public class HusFileBenchmarks
{
    private byte[] _husFile = [];

    [GlobalSetup]
    public void Setup()
    {
        _husFile = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Files", "paris1.hus"));
    }

    [Benchmark]
    public EmbroideryDesign ReadParis1Hus() => new HusFormatHandler().Read(_husFile, "paris1.hus");
}
