# Hoopful

*A hoopful of embroideries, right in your browser.*

Hoopful opens a folder of embroidery designs and draws them — parsing,
decompression and rendering all run locally in Blazor WebAssembly. Everything
lives in one library, `Hoopful.Formats`: a fully managed, cross-platform C#
implementation of the **Greenleaf ArchiveLib decompression algorithm** used
inside Husqvarna/Viking `.hus` and Pfaff `.vip` files
(`Hoopful.Formats.Compression`), handlers for twelve embroidery formats plus two
stitch renderers ported from the original `hexreader` WinForms application in
`reference/`. `Hoopful.Browser` is the app on top.

- Target framework `net10.0`, no third-party runtime dependencies, no `unsafe`
  code, no P/Invoke, no reflection, no file-system access in the decompressor.
- Runs on Windows, Linux, macOS and in the browser (Blazor WebAssembly /
  `browser-wasm`).
- Byte-for-byte identical output to the original 1994 C++ library for every
  committed golden vector, including real HUS/VIP data.

## Why this exists

HUS and VIP files store their stitch data in three streams compressed with a
proprietary LZSS + Huffman scheme from *Greenleaf ArchiveLib 1.0* (1994), usually
referred to as `AL_GREENLEAF_LEVEL_4`. There is no maintained managed
implementation; existing readers either bundle native code or port the algorithm
loosely. This library implements exactly the decompression variant those files
need — nothing else from ArchiveLib — and verifies it against the original
implementation.

The reference used throughout is
[software-opal/archivelib-rs](https://github.com/software-opal/archivelib-rs),
which preserves the original C++ (`archivelib-sys-orig`), a cleaned-up C++
rewrite (`archivelib-sys-refactored/c-lib`) and a fuzz-verified Rust port.

### Implemented

- ArchiveLib decompression for levels 0–4 (HUS/VIP use level 4; the level only
  changes the sliding-window size, `1 << (10 + level)` bytes).
- Dynamic Huffman table reading (all three per-block tables), literals,
  length/offset decoding, overlapping LZ back-references, multi-block streams,
  and strict validation of malformed input.
- HUS and VIP container parsing (inside the format handlers): header validation,
  VIP colour-table decoding and decompression of the attribute/X/Y streams.

### Not implemented

Compression, other ArchiveLib compression engines and levels' archive/metadata
support, encryption, thread palettes, rendering, or a full embroidery object
model. The readers stop at "decompressed streams + header metadata" by design.

## How the format works

A compressed stream is a sequence of **blocks**. All bits are packed MSB first.
Each block is:

```text
+-----------------+---------------------+---------------------+--------------+-----------------+
| item count (16) | code-length table   | literal/length table| offset table | items ...       |
+-----------------+---------------------+---------------------+--------------+-----------------+
```

1. **Item count** — 16 bits; the number of Huffman symbols in the block
   (including the end-of-stream symbol). `0` terminates the stream.
2. **Code-length table** (19 symbols) — a helper Huffman table used only to
   decode the literal table's code lengths. Sent as a 5-bit entry count followed
   by 3-bit code lengths, where the value 7 is extended by unary `1` bits
   (`1110` = 7, `11110` = 8, …). One quirk: immediately after the third entry a
   2-bit field skips 0–3 entries (they become unused).
3. **Literal/length table** (511 symbols) — a 9-bit entry count, then entries
   decoded with the code-length table: symbol 0 = one unused symbol, symbol 1 =
   `3 + 4 bits` unused symbols, symbol 2 = `20 + 9 bits` unused symbols, symbols
   3–18 = code length 1–16 for the current symbol.
4. **Offset table** (15 symbols) — like the code-length table but without the
   2-bit skip field.
5. **Items** — literal/length symbols: `0–255` are literal bytes; `s` in
   `256–509` is a back-reference of `s - 256 + 3` bytes (3–256); `510` ends the
   stream (remaining bits are padding). When the item count runs out first, a new
   block header with fresh tables follows immediately.

Any table can instead be sent with an entry count of **0**, followed by one
symbol id: the table then contains that single symbol and decoding it consumes
**zero bits** (heavily used by tiny embroidery attribute streams).

**Huffman codes are canonical**: shorter codes first; equal-length codes in
increasing symbol order. A table is only valid if the lengths form an *exactly
complete* code (Kraft sum `Σ 2^(16-len) = 2^16`). Decoding uses a direct lookup
table on the top 12 bits (literal table) or 8 bits (small tables) of a 16-bit
look-ahead window, with a per-bit trie for longer codes — the same structure as
the original.

**Back-reference offsets** are decoded in two steps: the offset table yields a
bit width `w`; the offset is `0` (for `w = 0`), `1` (for `w = 1`) or
`(1 << (w-1)) | next(w-1 bits)`. The copy starts `offset + 1` bytes back in the
output and may overlap itself (offset 0 repeats the previous byte — that is how
runs of a single byte are encoded). Offsets must fit inside both the produced
output and the level's sliding window (16 KiB at level 4).

### Inherited quirks (kept for compatibility)

- **Reads past the end of the input return zero bits.** The original reads
  whatever its stale buffer holds; the Rust reference reads zeros; we match the
  Rust behaviour. Well-formed streams end with symbol 510 and never notice.
- A stream may also terminate via a **zero item count**, whose (degenerate)
  Huffman tables are still read first.
- The entry counts of the two 5-bit tables are **clamped** to 19/15 rather than
  rejected (the original would overflow a stack array; the Rust port clamps).
- Compressing an **empty input** with the original library produces a stream
  that does not decompress to empty output; this library rejects empty *inputs*
  outright and represents "empty output" only via the hand-crafted EOF-only
  stream (see the `eof_only` fixture).
- Incomplete/oversubscribed Huffman tables are undefined behaviour in the 1994
  code; like the Rust reference, this implementation rejects them.

## Using the library

### Decompressing a raw stream

```csharp
using Hoopful.Formats.Compression;

// Returns a new array; throws ArchiveLibException unless the stream is valid
// and expands to exactly the expected number of bytes.
byte[] data = ArchiveLibDecompressor.Decompress(compressedSpan, expectedLength);

// Span-based, allocation-free variant (fills a caller-provided buffer):
Span<byte> destination = stackalloc byte[1024];
if (ArchiveLibDecompressor.TryDecompress(compressedSpan, destination, out int written))
{
    // destination[..written] is valid
}
```

Both methods accept an optional `ArchiveLibCompressionLevel` (default `Level4`).
`TryDecompress` returns `false` for invalid input instead of throwing; since it
has no expected length, a truncated-but-cleanly-terminating stream can succeed
with a shorter `bytesWritten` — check it against your expectation.
`Decompress` refuses expected lengths above
`ArchiveLibDecompressor.MaxExpectedDecompressedLength` (64 MiB) so corrupt
headers cannot trigger huge allocations.

### Reading an embroidery file

```csharp
using Hoopful.Formats;

EmbroideryDesign design = EmbroideryFormatFactory.Load(
    File.ReadAllBytes("design.hus"), "design.hus");
// design.NumberOfStitches, design.NumberOfColors, design.Colors,
// design.Stitches (relative movements + stitch/jump/colour-change type)
```

The factory picks the handler by extension; HUS and VIP go through the
container parsing + ArchiveLib decompression inside `HusFormatHandler` /
`VipFormatHandler` (use their static `HasHusMagic` / `HasVipMagic` to sniff
content instead of trusting the extension). Movements are signed 8-bit deltas
in 0.1 mm units (Y positive = up).

## The embroidery format library and browser

`src/Hoopful.Formats` is a faithful port of the original WinForms viewer's
parsing and drawing code:

- **Formats:** `.hus`, `.vip`, `.vp3`, `.dst`, `.exp`, `.jef`, `.sew`, `.ksm`,
  `.pcs`, `.pec`, `.pes`, `.xxx` — one handler per format via
  `EmbroideryFormatFactory.Load(bytes, fileName)`. HUS/VIP decompression uses the
  managed ArchiveLib decompressor instead of the original's native
  `al21mfc.dll`, which is what makes the whole thing work in WebAssembly.
  Optional `<file>.ytlc` colour side-car files are supported like the original.
- **Flat renderer** (`EmbroiderySvgRenderer`): reproduces the original pixel
  renderer — needle starts at `(StartXOffset+10, StartYOffset+10)`, stitches
  *and* jump threads draw as lines, segments with negative coordinates are
  clipped but still move the needle, colour changes advance the thread, and the
  finished image gets the original's `Rotate180FlipX` mirror
  (`Rotate270FlipX` for KSM). Output is SVG, so it scales cleanly.
- **Bump-mapped renderer** (`EmbroideryBumpMapRenderer`): the original
  `GetImageByGradient` "3D" shading — every stitch drawn dark at the ends
  (colour −100/channel), brightening toward the middle by `angle × light / 4`
  (steeper stitches catch more light), with the original's black-thread
  dark-blue sheen special case. Rendered by a fully managed rasterizer to an
  in-memory PNG (no System.Drawing, no SkiaSharp, no native code).
- **Preserved quirks** are documented in the code: the EXP jump-condition
  precedence bug, HUS/VIP negative extents computed one unit small, JEF reading
  only the low byte of the stitch-data offset, PES jumps decoding to zero
  movement, the DST colour-change record that also emits a jump, and EXP/KSM
  starting at the canvas origin. Two deliberate fixes: PCS stitches get a type
  (the original rendered PCS blank) and out-of-range colour indices clamp
  instead of crashing.

The Blazor sample (`samples/Hoopful.Browser`) is the old
"EmbroideryBrowser" rebuilt for the web: **Open folder** (uses the browser's
directory picker — Chromium and Firefox) or **Open files**, every supported
design is indexed with a thumbnail, and clicking one opens a viewer with
metadata and a **Flat / 3D shaded (bump mapped)** toggle. Everything runs
locally in WebAssembly; no file leaves the machine.

## Repository layout

```text
src/Hoopful.Formats/              everything: ArchiveLib decompressor (Compression/), 12 format
                                  handlers incl. HUS/VIP container parsing (Handlers/) and both
                                  renderers (Rendering/) — only .NET base libraries
tests/Hoopful.Tests/              xunit tests + golden fixtures (see Fixtures/FIXTURES.md)
benchmarks/Hoopful.Benchmarks/    BenchmarkDotNet benchmarks
samples/Hoopful.Browser/          the Hoopful app (Blazor WebAssembly)
tools/generate-fixtures/          developer scripts to rebuild the golden vectors
reference/                        the original hexreader WinForms application (source of the port)
```

## Running the tests

```bash
dotnet test
```

The suite decompresses every golden vector and compares each output byte with
the reference result, parses the real sample HUS files and the synthetic VIP
file, and feeds the decoder truncated, bit-flipped, and hand-crafted invalid
streams. Fixture provenance and licensing are documented in
[`tests/Hoopful.Tests/Fixtures/FIXTURES.md`](tests/Hoopful.Tests/Fixtures/FIXTURES.md);
the fixtures were generated by the reference C++ implementation and
cross-checked against pyembroidery (see
[`tools/generate-fixtures/README.md`](tools/generate-fixtures/README.md)).

## Running the benchmarks

```bash
dotnet run --project benchmarks/Hoopful.Benchmarks -c Release -- --filter '*'
```

`MemoryDiagnoser` reports allocated bytes and Gen 0 collections per operation;
mean time per operation gives ops/s, and dividing a fixture's output size by the
mean time gives decompressed bytes/s. The `DecompressReusingBuffer` benchmarks
demonstrate the allocation-free path: the only per-call allocation is the small
fixed decoder state (three Huffman tables, ~20 KB), and `DecompressAllocating`
additionally allocates the output array. CI runs the benchmarks with `--job Dry`
as a smoke test only.

## Running the Embroidery Browser (Blazor WebAssembly)

```bash
dotnet run --project samples/Hoopful.Browser
# or: dotnet publish samples/Hoopful.Browser -c Release
```

Open a folder (or files) of embroidery designs: everything supported is indexed
into a thumbnail grid; clicking a design opens the viewer with a Flat / 3D
shaded toggle. The page also runs a decompressor self-test on load: an embedded
golden fixture is decompressed and its SHA-256 compared with the reference C++
output (PASS/FAIL is shown). Folder selection uses the `webkitdirectory` input
attribute — supported in Chromium-based browsers and Firefox; in browsers
without it, use the file picker instead.

## Known limitations

- Decompression only. Writing HUS/VIP files or general ArchiveLib archives is
  out of scope (test data was generated with the original C++ compressor).
- The readers expose raw streams and header metadata, not a stitch object model.
- The original library cannot address streams over 64 KiB; this implementation
  allows larger outputs (up to 64 MiB) but real files never need it.
- Divergences from the original on *invalid* input are documented above; on
  valid streams the output is byte-identical.

## Licensing of third-party material

The source code in `src/` is original. Test data partially originates from the
GPL-2.0 archivelib-rs repository (see `FIXTURES.md` for details and how to strip
it); the VIP colour XOR table constant is reproduced from the Embroidermodder
project's libembroidery.
