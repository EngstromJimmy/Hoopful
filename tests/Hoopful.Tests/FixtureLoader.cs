using System.Text.Json;

namespace Hoopful.Tests;

/// <summary>One golden vector: compressed input plus the reference decompressed output.</summary>
public sealed record Fixture(
    string Name,
    int Level,
    byte[] Compressed,
    byte[] Expected,
    string Description,
    string Source)
{
    public override string ToString() => Name;
}

/// <summary>
/// Loads the golden vectors generated from the reference C++ ArchiveLib implementation.
/// See Fixtures/FIXTURES.md for provenance.
/// </summary>
public static class FixtureLoader
{
    public static string FixtureDirectory { get; } =
        Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private static readonly Lazy<IReadOnlyList<Fixture>> LazyFixtures = new(LoadAll);

    public static IReadOnlyList<Fixture> All => LazyFixtures.Value;

    public static Fixture Get(string name) =>
        All.FirstOrDefault(f => f.Name == name)
        ?? throw new InvalidOperationException($"Fixture '{name}' not found.");

    public static byte[] ReadFile(string relativePath) =>
        File.ReadAllBytes(Path.Combine(FixtureDirectory, relativePath));

    private static IReadOnlyList<Fixture> LoadAll()
    {
        string manifestPath = Path.Combine(FixtureDirectory, "manifest.json");
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllBytes(manifestPath));

        var fixtures = new List<Fixture>();
        foreach (JsonElement entry in manifest.RootElement.EnumerateArray())
        {
            var fixture = new Fixture(
                entry.GetProperty("name").GetString()!,
                entry.GetProperty("level").GetInt32(),
                ReadFile(entry.GetProperty("compressedFile").GetString()!),
                ReadFile(entry.GetProperty("expectedFile").GetString()!),
                entry.GetProperty("description").GetString()!,
                entry.GetProperty("source").GetString()!);

            if (fixture.Compressed.Length != entry.GetProperty("compressedLength").GetInt32()
                || fixture.Expected.Length != entry.GetProperty("expectedLength").GetInt32())
            {
                throw new InvalidOperationException($"Fixture '{fixture.Name}' does not match its manifest entry.");
            }

            fixtures.Add(fixture);
        }

        return fixtures;
    }
}
