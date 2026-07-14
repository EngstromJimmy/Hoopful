namespace Hoopful.Formats.Compression;

/// <summary>
/// Most-significant-bit-first bit reader over a byte span.
/// </summary>
/// <remarks>
/// <para>
/// ArchiveLib streams are written MSB first: the first bit of the stream is bit 7 of the
/// first byte. The original decompressor keeps a 16-bit look-ahead window that Huffman
/// decoding peeks into; this reader reproduces that model with <see cref="Peek16"/> /
/// <see cref="Consume"/>.
/// </para>
/// <para>
/// Reads past the end of the input return zero bits instead of failing. This mirrors the
/// reference implementation (the Rust port of ArchiveLib), which relies on trailing zero
/// bits to terminate cleanly: after the real data ends, a block header of zeros decodes as
/// an item count of 0, which ends the stream. <see cref="HasOverrun"/> reports whether any
/// synthesized padding bits were actually consumed, which callers use to distinguish a
/// truncated stream from ordinary end-of-stream padding.
/// </para>
/// </remarks>
internal ref struct BitReader
{
    private readonly ReadOnlySpan<byte> _data;
    private readonly long _totalBitsAvailable;

    /// <summary>Index of the next byte of <see cref="_data"/> to load into the accumulator.</summary>
    private int _nextByteIndex;

    /// <summary>Bit accumulator; the <see cref="_bitsInAccumulator"/> low bits are valid.</summary>
    private uint _accumulator;

    private int _bitsInAccumulator;

    /// <summary>Total number of bits handed out via <see cref="Consume"/>.</summary>
    private long _bitsConsumed;

    public BitReader(ReadOnlySpan<byte> data)
    {
        _data = data;
        _totalBitsAvailable = (long)data.Length * 8;
    }

    /// <summary>True once bits beyond the end of the input have been consumed.</summary>
    public readonly bool HasOverrun => _bitsConsumed > _totalBitsAvailable;

    /// <summary>The next 16 bits of the stream (zero-padded past the end), without consuming them.</summary>
    public ushort Peek16()
    {
        if (_bitsInAccumulator < 16)
        {
            FillAccumulator();
        }

        return (ushort)(_accumulator >> (_bitsInAccumulator - 16));
    }

    /// <summary>Consumes <paramref name="bitCount"/> bits (0..16).</summary>
    public void Consume(int bitCount)
    {
        if (_bitsInAccumulator < bitCount)
        {
            FillAccumulator();
        }

        _bitsInAccumulator -= bitCount;
        _bitsConsumed += bitCount;
    }

    /// <summary>Reads <paramref name="bitCount"/> bits (0..16) MSB first.</summary>
    public int ReadBits(int bitCount)
    {
        if (bitCount == 0)
        {
            return 0;
        }

        int value = Peek16() >> (16 - bitCount);
        Consume(bitCount);
        return value;
    }

    /// <summary>Reads a single bit; returns false past the end of the input.</summary>
    public bool ReadBit() => ReadBits(1) != 0;

    /// <summary>
    /// Loads bytes (or zero padding past the end of the input) until the accumulator
    /// holds at least 16 valid bits. The accumulator never exceeds 24 bits, so it fits
    /// comfortably in 32 bits.
    /// </summary>
    private void FillAccumulator()
    {
        while (_bitsInAccumulator < 16)
        {
            uint nextByte = _nextByteIndex < _data.Length ? _data[_nextByteIndex++] : 0u;
            _accumulator = (_accumulator << 8) | nextByte;
            _bitsInAccumulator += 8;
        }
    }
}
