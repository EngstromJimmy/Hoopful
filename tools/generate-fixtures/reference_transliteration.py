#!/usr/bin/env python3
"""Line-by-line transliteration of the C# ArchiveLibDecompressor for algorithm verification."""

class ArchiveLibError(Exception): pass
EMPTY = 0xFFFF
EOFSYM = 510

class BitReader:
    def __init__(self, data):
        self.data = data; self.total = len(data)*8
        self.next = 0; self.acc = 0; self.bits = 0; self.consumed = 0
    @property
    def overrun(self): return self.consumed > self.total
    def fill(self):
        while self.bits < 16:
            b = self.data[self.next] if self.next < len(self.data) else 0
            self.next += 1 if self.next < len(self.data) else 0  # careful: C# increments only when in range
            # C# uses: nextByte = idx < len ? data[idx++] : 0  -> idx increments only in range
            self.acc = ((self.acc << 8) | b) & 0xFFFFFFFF
            self.bits += 8
    def peek16(self):
        if self.bits < 16: self.fill()
        return (self.acc >> (self.bits - 16)) & 0xFFFF
    def consume(self, n):
        if self.bits < n: self.fill()
        self.bits -= n; self.consumed += n
    def read(self, n):
        if n == 0: return 0
        v = self.peek16() >> (16 - n)
        self.consume(n)
        return v
    def readbit(self): return self.read(1) != 0

class Huff:
    def __init__(self, nsym, tbits):
        self.n = nsym; self.tbits = tbits
        self.table = [EMPTY]*(1 << tbits)
        self.zero = [0]*(nsym*(16-tbits)); self.one = [0]*(nsym*(16-tbits))
        self.lens = [0]*nsym; self.single = -1; self.nodes = 0
    def build_single(self, s): self.single = s
    def build(self, lens):
        assert len(lens) == self.n
        self.single = -1; self.nodes = 0
        for i in range(len(self.table)): self.table[i] = EMPTY
        self.lens = list(lens)
        cnt = [0]*17
        for l in lens:
            if l > 16: raise ArchiveLibError("len>16")
            cnt[l] += 1
        kraft = sum(cnt[l] << (16-l) for l in range(1,17))
        if kraft != 1 << 16: raise ArchiveLibError("incomplete/oversubscribed")
        nxt = [0]*17; run = 0
        for l in range(1,17):
            nxt[l] = run; run += cnt[l] << (16-l)
        for sym in range(len(lens)):
            l = lens[sym]
            if l == 0: continue
            code16 = nxt[l]; nxt[l] += 1 << (16-l)
            if l <= self.tbits:
                first = code16 >> (16-self.tbits)
                for k in range(1 << (self.tbits-l)): self.table[first+k] = sym
            else:
                slot = code16 >> (16-self.tbits)
                mask = 1 << (15-self.tbits); rem = l - self.tbits
                arr = self.table; idx = slot
                while rem > 0:
                    e = arr[idx]
                    if e == EMPTY:
                        ni = self.nodes; self.nodes += 1
                        self.zero[ni] = EMPTY; self.one[ni] = EMPTY
                        arr[idx] = self.n + ni
                    else:
                        ni = e - self.n
                    arr = self.one if (code16 & mask) else self.zero
                    idx = ni; mask >>= 1; rem -= 1
                arr[idx] = sym
    def decode(self, r):
        if self.single >= 0: return self.single
        w = r.peek16()
        e = self.table[w >> (16-self.tbits)]
        if e >= self.n:
            mask = 1 << (15-self.tbits)
            while True:
                if e == EMPTY or mask == 0: raise ArchiveLibError("no code")
                ni = e - self.n
                e = self.one[ni] if (w & mask) else self.zero[ni]
                mask >>= 1
                if e < self.n: break
        if e == EMPTY: raise ArchiveLibError("no code")
        r.consume(self.lens[e])
        return e

def read_code_length_value(r):
    v = r.read(3)
    if v == 7:
        while r.readbit():
            v += 1
            if v > 16: return 17
    return v

def read_code_length_table(r, t):
    n = 19
    count = min(r.read(5), n)
    if count == 0:
        s = r.read(5)
        if s >= n: raise ArchiveLibError("single oob")
        t.build_single(s); return
    lens = [0]*n; idx = 0
    while idx < count:
        lens[idx] = read_code_length_value(r); idx += 1
        if idx == 3: idx += r.read(2)
    t.build(lens)

def read_literal_table(r, clt, t):
    n = 511
    count = min(r.read(9), n)
    if count == 0:
        s = r.read(9)
        if s >= n: raise ArchiveLibError("single oob")
        t.build_single(s); return
    lens = [0]*n; idx = 0
    while idx < count:
        v = clt.decode(r)
        if v == 0: idx += 1
        elif v == 1: idx += 3 + r.read(4)
        elif v == 2: idx += 20 + r.read(9)
        else: lens[idx] = v - 2; idx += 1
    t.build(lens)

def read_offset_table(r, t):
    n = 15
    count = min(r.read(5), n)
    if count == 0:
        s = r.read(5)
        if s >= n: raise ArchiveLibError("single oob")
        t.build_single(s); return
    lens = [0]*n; idx = 0
    while idx < count:
        lens[idx] = read_code_length_value(r); idx += 1
    t.build(lens)

def decompress(data, max_out, level=4):
    if len(data) == 0: raise ArchiveLibError("empty")
    window = 1 << (10 + level)
    r = BitReader(data)
    clt = Huff(19, 8); lit = Huff(511, 12); off = Huff(15, 8)
    out = bytearray()
    blocks = 0
    while True:
        items = r.read(16)
        read_code_length_table(r, clt)
        read_literal_table(r, clt, lit)
        read_offset_table(r, off)
        if items == 0: break
        blocks += 1
        for _ in range(items):
            sym = lit.decode(r)
            if sym < 256:
                if len(out) >= max_out: raise ArchiveLibError("overflow")
                out.append(sym)
                continue
            if sym == EOFSYM:
                if r.overrun: raise ArchiveLibError("truncated")
                return bytes(out), blocks
            runlen = sym - 256 + 3
            w = off.decode(r)
            o = 0 if w == 0 else (1 if w == 1 else ((1 << (w-1)) | r.read(w-1)))
            hist = min(len(out), window)
            if o >= hist: raise ArchiveLibError("bad offset")
            if len(out) + runlen > max_out: raise ArchiveLibError("overflow")
            src = len(out) - o - 1
            for i in range(runlen): out.append(out[src+i])
    if r.overrun and len(out) == 0: raise ArchiveLibError("truncated")
    return bytes(out), blocks

if __name__ == "__main__":
    import json, sys
    man = json.load(open("/tmp/fixtures/manifest.json"))
    fail = 0
    for m in man:
        comp = open(f"/tmp/fixtures/{m['compressedFile']}","rb").read()
        exp = open(f"/tmp/fixtures/{m['expectedFile']}","rb").read()
        lvl = m["level"]
        try:
            got, blocks = decompress(comp, len(exp), lvl)
        except ArchiveLibError as e:
            print(f"FAIL {m['name']}: error {e}"); fail += 1; continue
        ok = got == exp
        if not ok: fail += 1
        print(f"{'ok  ' if ok else 'FAIL'} {m['name']}: {len(got)}/{len(exp)} bytes, {blocks} block(s)")
    print("FAILURES:", fail)
    sys.exit(1 if fail else 0)
