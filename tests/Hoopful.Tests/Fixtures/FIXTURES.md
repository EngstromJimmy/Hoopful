# Golden fixture provenance

Every `<name>.cmp` / `<name>.out` pair is a golden vector: `.cmp` is the compressed
input, `.out` is the decompressed output **as produced by the reference C++
implementation** of Greenleaf ArchiveLib. `manifest.json` describes each pair
(level, lengths, SHA-256 of the output, description, source).

The C# tests compare every decompressed byte against these files. None of the
expected outputs were produced by the C# implementation itself.

## How the vectors were generated

The reference implementation is the cleaned-up 1994 Greenleaf ArchiveLib C++ code
maintained in <https://github.com/software-opal/archivelib-rs>
(`archivelib-sys-refactored/c-lib`, GPL-2.0). A small CLI harness
(`tools/generate-fixtures/refcli.cpp`) wraps its `compress`/`decompress` API, and
`tools/generate-fixtures/generate_fixtures.py` drives it. Compression level 4
(`AL_GREENLEAF_LEVEL_4`, the HUS/VIP level) is used unless the manifest says
otherwise. All synthetic inputs are deterministic (fixed RNG seed), so the vectors
are reproducible.

Every vector was additionally cross-checked against pyembroidery's independent
`EmbCompress` decompressor (<https://github.com/EmbroidePy/pyembroidery>, MIT),
which matched all fixtures byte for byte.

## Sources

| Fixture group | Source | Redistribution |
| --- | --- | --- |
| Synthetic vectors (`single_byte` … `multi_block_*`, `level*_text`) | Inputs generated mathematically by `generate_fixtures.py`; compressed with the reference C++ implementation. | Original work of this repository. |
| `eof_only`, `hus_trims` | Byte strings published as test vectors inside the archivelib-rs test-suite (`tests/eof_semantics.rs`). | Short test vectors from a GPL-2.0 test-suite, reproduced with attribution. |
| `hus_small_heart_*`, `hus_star_*`, `hus_paris1_*` and `Files/*.hus` (except `rose.hus`) | Sample HUS files committed as test data in archivelib-rs (`test_data/`), originally from the Embroidermodder project's sample designs. The `.cmp` files are the raw compressed streams cut out of those files; the `.out` files are the reference decompressions. | Test data redistributed from the GPL-2.0 archivelib-rs repository with attribution. If your use of this repository cannot accommodate that, delete `Files/small_heart.hus`, `Files/star.hus`, `Files/paris1.hus`, `Files/embroidermodder.hus` and the `hus_*` fixtures and rerun the generator — all remaining tests use original data. |
| `vip_rose_*`, `Files/rose.hus`, `Files/rose.vip` | Fully synthetic design (a parametric rose curve) created by `tools/generate-fixtures/generate_sample_files.py`; containers assembled per the format documentation, streams compressed with the reference implementation. | Original work of this repository. |

## Regenerating

See `tools/generate-fixtures/README.md`. Regeneration requires cloning
archivelib-rs and a C++ compiler; it is **not** required for normal development or
CI, which only consume the committed fixtures.
