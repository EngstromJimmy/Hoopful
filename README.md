# Hoopful

*A hoopful of embroideries, right in your browser.*

Point Hoopful at a folder of embroidery designs and — hoop, there it is — every
design inside gets parsed, decompressed and drawn, entirely on your machine.
No uploads, no installs, no cloud. Just open the page, open a folder, and
browse your collection like fabric swatches pinned to a board.

## What it does

- **Opens whole folders.** Pick a folder (or individual files) and Hoopful
  indexes every supported design into a swatchboard of live thumbnails.
- **Reads twelve formats.** `.hus`, `.vip`, `.vp3`, `.dst`, `.exp`, `.jef`,
  `.sew`, `.ksm`, `.pcs`, `.pec`, `.pes`, `.xxx` — including the compressed
  Husqvarna/Viking and Pfaff formats, decoded by a fully managed decompressor.
- **Draws them two ways.** A crisp flat vector view, and a bump-mapped
  **3D thread view** that shades every stitch by its angle — dark at the ends,
  catching the light in the middle — so designs look sewn, not plotted.
- **Shows the real colours.** Thread palettes come straight from the files
  (including Pfaff's obfuscated VIP colour table), shown as little spools on
  every swatch.
- **Stays private by design.** Everything runs in WebAssembly inside your
  browser tab. No file ever leaves your computer.
- **Comes in linen and midnight.** A warm light theme and a moody dark theme;
  follows your system or toggles with one click.

And yes — it's a little fun on purpose. Thread carefully.

## Run it

```bash
dotnet run --project samples/Hoopful.Browser
```

Then open the printed URL, click the big hoop, and pick a folder full of
designs. To publish a static site you can host anywhere:

```bash
dotnet publish samples/Hoopful.Browser -c Release
```

Requirements: the .NET 10 SDK. No JavaScript frameworks, no npm, no native
dependencies — the whole thing is C# and CSS.

## Use it as a library

All parsing and rendering lives in one dependency-free library,
`Hoopful.Formats`:

```csharp
using Hoopful.Formats;
using Hoopful.Formats.Rendering;

EmbroideryDesign design = EmbroideryFormatFactory.Load(
    File.ReadAllBytes("design.hus"), "design.hus");

string svg = EmbroiderySvgRenderer.Render(design);          // flat vector view
byte[] png = EmbroideryBumpMapRenderer.RenderPng(design);   // 3D shaded view
```

`EmbroideryDesign` exposes the stitch records (relative movements plus
stitch/jump/colour-change types), thread colours, and extents. The library
targets `net10.0`, uses only the .NET base class libraries, contains no unsafe
code and no P/Invoke, and runs on Windows, Linux, macOS and `browser-wasm`.

## Repository layout

```text
src/Hoopful.Formats/              format handlers, the HUS/VIP decompressor and both renderers
samples/Hoopful.Browser/          the Hoopful app (Blazor WebAssembly)
tests/Hoopful.Tests/              xunit tests + golden fixtures
benchmarks/Hoopful.Benchmarks/    BenchmarkDotNet benchmarks
tools/generate-fixtures/          developer scripts to rebuild the golden vectors
docs/                             deep-dive documentation
```

## Development

```bash
dotnet test                                                        # full test-suite
dotnet run --project benchmarks/Hoopful.Benchmarks -c Release -- --filter '*'
```

The test-suite verifies the HUS/VIP decompressor byte-for-byte against golden
vectors produced by the original 1994 C++ implementation (see
[docs/DECOMPRESSION.md](docs/DECOMPRESSION.md) for the format deep-dive and
[tests/Hoopful.Tests/Fixtures/FIXTURES.md](tests/Hoopful.Tests/Fixtures/FIXTURES.md)
for fixture provenance), exercises real sample files, and feeds the parsers
truncated, bit-flipped and hand-crafted hostile input. CI builds and tests on
Windows, Linux and macOS and publishes the browser app.

## Credits & sources

Hoopful stands on the shoulders of people who reverse-engineered these formats
long before us:

- [software-opal/archivelib-rs](https://github.com/software-opal/archivelib-rs) —
  preserves the original Greenleaf ArchiveLib 1.0 C++ source, a cleaned-up
  rewrite and a fuzz-verified Rust port. This was the primary reference for the
  HUS/VIP decompressor, and its sample files back several test fixtures
  (GPL-2.0; see FIXTURES.md for details and how to strip them).
- [EmbroidePy/pyembroidery](https://github.com/EmbroidePy/pyembroidery) —
  independent implementation used to cross-check every golden fixture (MIT).
- [Embroidermodder/libembroidery](https://github.com/Embroidermodder/libembroidery) —
  source of the VIP colour XOR table constant and general container-format
  knowledge.
- The original *hexreader* WinForms application — Hoopful's format handlers and
  both renderers are direct ports of its parsing and drawing code, quirks
  lovingly preserved.

I will keep you in the hoop.
