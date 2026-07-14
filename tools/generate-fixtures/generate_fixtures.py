#!/usr/bin/env python3
"""Golden fixture generator.

Uses the reference C++ ArchiveLib implementation (built from
software-opal/archivelib-rs, archivelib-sys-refactored/c-lib) to produce
compressed/decompressed golden vectors for the C# test-suite.
"""
import json, os, struct, subprocess, hashlib, random

REFCLI = "/tmp/refcli"
OUT = "/tmp/fixtures"
os.makedirs(OUT, exist_ok=True)
manifest = []

def ref(mode, level, data: bytes) -> bytes:
    open("/tmp/fixgen/in.bin", "wb").write(data)
    r = subprocess.run([REFCLI, mode, str(level), "/tmp/fixgen/in.bin", "/tmp/fixgen/out.bin"],
                       capture_output=True)
    if r.returncode != 0:
        raise RuntimeError(f"refcli {mode} failed: {r.stderr.decode()}")
    return open("/tmp/fixgen/out.bin", "rb").read()

def add(name, raw: bytes, level, desc, source):
    comp = ref("c", level, raw)
    back = ref("d", level, comp)
    assert back == raw, f"{name}: reference roundtrip mismatch"
    open(f"{OUT}/{name}.cmp", "wb").write(comp)
    open(f"{OUT}/{name}.out", "wb").write(raw)
    manifest.append({
        "name": name, "level": level,
        "compressedFile": f"{name}.cmp", "expectedFile": f"{name}.out",
        "compressedLength": len(comp), "expectedLength": len(raw),
        "expectedSha256": hashlib.sha256(raw).hexdigest(),
        "description": desc, "source": source,
    })
    print(f"{name}: {len(raw)} -> {len(comp)}")

def add_precompressed(name, comp: bytes, level, desc, source):
    raw = ref("d", level, comp)
    open(f"{OUT}/{name}.cmp", "wb").write(comp)
    open(f"{OUT}/{name}.out", "wb").write(raw)
    manifest.append({
        "name": name, "level": level,
        "compressedFile": f"{name}.cmp", "expectedFile": f"{name}.out",
        "compressedLength": len(comp), "expectedLength": len(raw),
        "expectedSha256": hashlib.sha256(raw).hexdigest(),
        "description": desc, "source": source,
    })
    print(f"{name}: {len(comp)} -> {len(raw)} (pre-compressed)")
    return raw

GEN = "generated with the reference C++ implementation (software-opal/archivelib-rs, archivelib-sys-refactored/c-lib) via tools/generate-fixtures"
rng = random.Random(0x48555356)  # deterministic

# --- synthetic vectors, level 4 (the HUS/VIP level) ---
add_precompressed("eof_only", bytes([0x00,0x01,0x00,0x00,0x1F,0xE0,0x00]), 4,
    "Hand-crafted single-block stream containing only the EOF symbol; decompresses to empty output.",
    "eof_semantics.rs 'semivalid' test vector in software-opal/archivelib-rs")
add_precompressed("hus_trims", bytes([0x00,0x18,0x40,0x68,0x61,0xB5,0xFF,0x0D,0x9F,0x43,0xD1,0x10,0xBC,0xA0,0xCB,0x89,0xDA,0x80,0x16,0x77,0x00,0x01,0xB6,0x7B,0x39,0xF0]), 4,
    "Real HUS attribute stream with trims/jumps; long offset-0 style runs.",
    "eof_semantics.rs 'compressed_hus_data' test vector in software-opal/archivelib-rs")
add("single_byte", b"A", 4, "Single literal byte.", GEN)
add("two_bytes", b"AB", 4, "Two distinct literal bytes (below minimum run length).", GEN)
add("literal_only", bytes(range(0x20, 0x60)), 4, "64 distinct bytes; no back-references possible.", GEN)
add("all_byte_values", bytes(range(256)), 4, "Every byte value once; dense literal Huffman tree.", GEN)
add("repeated_byte_tiny", b"AAAA", 4, "Four identical bytes; minimal run/limits of tiny tables.", GEN)
add("repeated_byte", b"A" * 1000, 4, "1000 identical bytes; overlapping distance-1 runs and offset code 0.", GEN)
add("overlapping_backref", b"AB" * 500, 4, "ABAB... pattern; overlapping back-reference with distance 2.", GEN)
add("short_backrefs", b"Hello, World! " * 3, 4, "Short repeated phrase; short back-references.", GEN)
block = bytes(rng.randrange(256) for _ in range(256))
add("long_backrefs", block * 40, 4, "256-byte random block repeated 40x; maximum run length (256) back-references.", GEN)
# max distance: window at level 4 is 16384
prefix = bytes(rng.randrange(256) for _ in range(16384))
add("max_distance", prefix + prefix[:64], 4, "16 KiB of random data followed by a copy of its first 64 bytes; exercises distances near the level-4 window limit (16384).", GEN)
# deep huffman: exponentially skewed distribution -> wide range of code lengths
skew = bytes(min(255, int(rng.expovariate(0.06))) for _ in range(8192))
add("deep_huffman", skew, 4, "Exponentially skewed byte distribution; deep/uneven Huffman code lengths.", GEN)
# text-like
words = ["needle", "thread", "stitch", "bobbin", "hoop", "satin", "fill", "jump", "trim", "color"]
text = " ".join(rng.choice(words) for _ in range(6000)).encode()
add("text_like", text, 4, "Pseudo-English text; mixed literals and back-references, typical Huffman shapes.", GEN)
# multiple blocks: > 65,535 symbols forces a second block header (16-bit item count)
incompressible = bytes(rng.randrange(256) for _ in range(60000))
add("multi_block_random", incompressible, 4, "60,000 random bytes; the compressor flushes its ~8 KiB item buffer repeatedly, producing many blocks each with fresh Huffman tables.", GEN)
compressible_big = (text * 40)[:64000]
add("multi_block_text", compressible_big, 4, "64,000 bytes of repetitive text; multiple blocks with dense back-references.", GEN)
# same input at other levels to prove level handling (window sizes 1024 / 16384)
add("level0_text", text[:4096], 0, "Level-0 (1 KiB window) compression of text data.", GEN)
add("level2_text", text[:8192], 2, "Level-2 (4 KiB window) compression of text data.", GEN)

# --- real HUS file streams ---
HUS_SRC = "streams extracted from {f} in software-opal/archivelib-rs test_data (GPL-2.0); original design sample from the Embroidermodder project"
for husname in ["small_heart", "star", "paris1"]:
    d = open(f"/tmp/archivelib-rs/test_data/{husname}.hus", "rb").read()
    cmd_off, x_off, y_off = struct.unpack_from("<iii", d, 20)
    for key, sl in [("attr", d[cmd_off:x_off]), ("x", d[x_off:y_off]), ("y", d[y_off:])]:
        add_precompressed(f"hus_{husname}_{key}", bytes(sl), 4,
            f"Real HUS {key} stream from {husname}.hus.", HUS_SRC.format(f=husname + ".hus"))

json.dump(manifest, open(f"{OUT}/manifest.json", "w"), indent=2)
print(f"\n{len(manifest)} fixtures written to {OUT}")
