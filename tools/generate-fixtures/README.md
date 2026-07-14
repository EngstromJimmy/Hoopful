# Golden fixture generator

Developer-only tooling that rebuilds the golden test vectors from the reference C++
implementation of Greenleaf ArchiveLib. **Normal development and CI never need this** —
the generated fixtures are committed under `tests/Hoopful.Tests/Fixtures/`.

## Contents

| File | Purpose |
| --- | --- |
| `refcli.cpp` | Tiny CLI wrapping the reference `compress`/`decompress` API. |
| `generate_fixtures.py` | Generates the synthetic golden vectors and extracts the compressed streams from the sample HUS files. |
| `generate_sample_files.py` | Builds the fully synthetic `rose.hus` / `rose.vip` sample files and their golden stream vectors. |
| `reference_transliteration.py` | A Python transliteration of the C# decoder, used to sanity-check algorithm changes against the fixtures without a .NET toolchain. |

## Rebuilding the fixtures

Requires git, a C++17 compiler and Python 3.

```bash
git clone --depth 1 https://github.com/software-opal/archivelib-rs.git /tmp/archivelib-rs

cd /tmp/archivelib-rs/archivelib-sys-refactored
g++ -O2 -std=c++17 -I c-lib -I c-lib/include -DNDEBUG \
    -o /tmp/refcli <path-to-this-dir>/refcli.cpp \
    c-lib/api.cpp c-lib/enum_rev.cpp $(find c-lib/src -name '*.cpp')

python3 generate_fixtures.py       # writes /tmp/fixtures
python3 generate_sample_files.py   # adds rose.hus / rose.vip

# then copy /tmp/fixtures/* into tests/Hoopful.Tests/Fixtures/
```

The scripts use fixed RNG seeds, so regenerated vectors are byte-identical.
Cross-check with an independent implementation before committing:

```bash
pip install pyembroidery
python3 - <<'EOF'
import json
from pyembroidery.EmbCompress import expand
for m in json.load(open("/tmp/fixtures/manifest.json")):
    comp = open(f"/tmp/fixtures/{m['compressedFile']}", "rb").read()
    exp = open(f"/tmp/fixtures/{m['expectedFile']}", "rb").read()
    assert bytes(expand(bytearray(comp), len(exp))) == exp, m["name"]
print("all fixtures match pyembroidery")
EOF
```
