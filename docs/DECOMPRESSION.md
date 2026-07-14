# The HUS/VIP decompression format

Husqvarna/Viking `.hus` and Pfaff `.vip` files store their stitch data in three
streams compressed with a proprietary LZSS + Huffman scheme from *Greenleaf
ArchiveLib 1.0* (1994), usually referred to as `AL_GREENLEAF_LEVEL_4`. Hoopful
contains a fully managed C# implementation of the decompressor
(`Hoopful.Formats.Compression`) that produces byte-for-byte identical output to
the original library for every committed golden vector, including real HUS/VIP
data.

The reference used throughout is
[software-opal/archivelib-rs](https://github.com/software-opal/archivelib-rs),
which preserves the original C++ (`archivelib-sys-orig`), a cleaned-up C++
rewrite (`archivelib-sys-refactored/c-lib`) and a fuzz-verified Rust port.

## Scope

Implemented: ArchiveLib decompression for levels 0–4 (HUS/VIP use level 4; the
level only changes the sliding-window size, `1 << (10 + level)` bytes), dynamic
Huffman table reading (all three per-block tables), literals, length/offset
decoding, overlapping LZ back-references, multi-block streams, and strict
validation of malformed input. HUS and VIP container parsing lives inside the
format handlers: header validation, VIP colour-table decoding and decompression
of the attribute/X/Y streams.

Not implemented: compression, other ArchiveLib engines, archive/metadata
support, or encryption. Test data was generated with the original C++
compressor.

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

## Inherited quirks (kept for compatibility)

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

## Using the decompressor directly

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

## Verification

The golden fixtures under `tests/Hoopful.Tests/Fixtures/` were generated by the
reference C++ implementation and cross-checked against pyembroidery's
independent decompressor; the test-suite compares every decompressed byte. See
[`tests/Hoopful.Tests/Fixtures/FIXTURES.md`](../tests/Hoopful.Tests/Fixtures/FIXTURES.md)
for provenance and [`tools/generate-fixtures/`](../tools/generate-fixtures/) for
the regeneration scripts. Divergences from the original on *invalid* input are
listed above; on valid streams the output is byte-identical.
