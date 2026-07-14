namespace Hoopful.Formats.Compression;

/// <summary>
/// Decompressor for the Greenleaf ArchiveLib format used inside Husqvarna/Viking
/// <c>.hus</c> and Pfaff <c>.vip</c> embroidery files (<c>AL_GREENLEAF_LEVEL_4</c>).
/// </summary>
/// <remarks>
/// <para><b>Stream layout.</b> A compressed stream is a sequence of blocks. Each block is:</para>
/// <list type="number">
///   <item><description>a 16-bit item count (the number of Huffman symbols in the block,
///   including the end-of-stream symbol when present);</description></item>
///   <item><description>three Huffman tables (see <see cref="ArchiveLibDecoderState"/>);</description></item>
///   <item><description>the items: literal bytes and LZ back-references.</description></item>
/// </list>
/// <para>
/// All bits are packed MSB first. A literal/length symbol &lt; 256 is a literal byte.
/// A symbol <c>s</c> in 256..509 is a back-reference of length <c>s - 256 + 3</c>
/// (3..256 bytes). Symbol 510 ends the stream; any remaining bits are padding.
/// When a block's item count is exhausted without an end-of-stream symbol, a new block
/// header (with fresh Huffman tables) follows immediately.
/// </para>
/// <para>
/// A back-reference offset is decoded in two steps: the offset table yields a bit width
/// <c>w</c>; the offset is then 0 for <c>w = 0</c>, 1 for <c>w = 1</c>, and otherwise
/// <c>(1 &lt;&lt; (w-1)) | next(w-1 bits)</c>. The copy source starts <c>offset + 1</c> bytes
/// before the current output position, and copies may overlap themselves (an offset of 0
/// repeats the previous byte). Offsets must stay inside both the bytes already produced
/// and the sliding window of the compression level (16 KiB for level 4).
/// </para>
/// <para><b>Compatibility notes.</b> Behaviour is matched to the reference implementation
/// (software-opal/archivelib-rs, which is fuzz-verified against the original 1994 C++
/// library) including its quirks:
/// </para>
/// <list type="bullet">
///   <item><description>Bits read past the end of the input are zeros; a block header of
///   zeros therefore decodes as an item count of 0, which terminates the stream.</description></item>
///   <item><description>A block whose item count is 0 still carries (trivial) Huffman
///   tables, which are read before the stream ends.</description></item>
///   <item><description>A Huffman table sent with a count field of 0 consists of a single
///   symbol whose codes consume zero bits.</description></item>
///   <item><description>Huffman tables must be exactly complete; incomplete or
///   oversubscribed tables are rejected (the original C++ has undefined behaviour here,
///   the Rust reference errors — this implementation errors too).</description></item>
/// </list>
/// </remarks>
public static class ArchiveLibDecompressor
{
    /// <summary>Minimum back-reference length; length symbols start at this value.</summary>
    private const int MinRunLength = 3;

    /// <summary>
    /// Upper bound for <c>expectedDecompressedLength</c>, guarding against corrupt
    /// headers requesting absurd allocations. The original ArchiveLib memory streams
    /// cannot even exceed 64 KiB, so 64 MiB is far beyond any real HUS/VIP stream.
    /// </summary>
    public const int MaxExpectedDecompressedLength = 64 * 1024 * 1024;

    /// <summary>
    /// Decompresses <paramref name="compressedData"/>, which must expand to exactly
    /// <paramref name="expectedDecompressedLength"/> bytes.
    /// </summary>
    /// <exception cref="ArchiveLibException">
    /// The data is malformed, truncated, or does not expand to the expected length.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="expectedDecompressedLength"/> is negative or exceeds
    /// <see cref="MaxExpectedDecompressedLength"/>.
    /// </exception>
    public static byte[] Decompress(
        ReadOnlySpan<byte> compressedData,
        int expectedDecompressedLength)
    {
        return Decompress(compressedData, expectedDecompressedLength, ArchiveLibCompressionLevel.Level4);
    }

    /// <summary>
    /// Decompresses <paramref name="compressedData"/> at the given compression level.
    /// HUS and VIP files always use <see cref="ArchiveLibCompressionLevel.Level4"/>.
    /// </summary>
    public static byte[] Decompress(
        ReadOnlySpan<byte> compressedData,
        int expectedDecompressedLength,
        ArchiveLibCompressionLevel level)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expectedDecompressedLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(expectedDecompressedLength, MaxExpectedDecompressedLength);
        ValidateLevel(level);

        byte[] output = new byte[expectedDecompressedLength];
        DecodeCore(compressedData, output, level, exactLength: true, out int bytesWritten);

        if (bytesWritten != expectedDecompressedLength)
        {
            throw new ArchiveLibException(
                $"Decompressed data is shorter than expected: got {bytesWritten} bytes, expected {expectedDecompressedLength}.");
        }

        return output;
    }

    /// <summary>
    /// Attempts to decompress <paramref name="compressedData"/> into
    /// <paramref name="destination"/>. Returns false when the data is malformed or the
    /// decompressed output does not fit in <paramref name="destination"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Decompress(ReadOnlySpan{byte}, int)"/> this overload does not know
    /// the expected output length, so it cannot detect a stream that ends early; callers
    /// should check <paramref name="bytesWritten"/> against their own expectations.
    /// </remarks>
    public static bool TryDecompress(
        ReadOnlySpan<byte> compressedData,
        Span<byte> destination,
        out int bytesWritten)
    {
        return TryDecompress(compressedData, destination, ArchiveLibCompressionLevel.Level4, out bytesWritten);
    }

    /// <summary>
    /// Attempts to decompress <paramref name="compressedData"/> at the given compression
    /// level into <paramref name="destination"/>.
    /// </summary>
    public static bool TryDecompress(
        ReadOnlySpan<byte> compressedData,
        Span<byte> destination,
        ArchiveLibCompressionLevel level,
        out int bytesWritten)
    {
        ValidateLevel(level);
        try
        {
            DecodeCore(compressedData, destination, level, exactLength: false, out bytesWritten);
            return true;
        }
        catch (ArchiveLibException)
        {
            bytesWritten = 0;
            return false;
        }
    }

    private static void ValidateLevel(ArchiveLibCompressionLevel level)
    {
        if ((uint)level > (uint)ArchiveLibCompressionLevel.Level4)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }
    }

    /// <summary>
    /// Decodes the full stream into <paramref name="output"/>.
    /// </summary>
    /// <param name="input">The compressed bytes.</param>
    /// <param name="output">Destination; decoding fails if the stream produces more bytes.</param>
    /// <param name="level">Compression level; determines the sliding-window size.</param>
    /// <param name="exactLength">
    /// True when <paramref name="output"/> is exactly the expected length, which allows a
    /// more precise error message when the stream overflows it.
    /// </param>
    /// <param name="bytesWritten">Number of bytes produced.</param>
    private static void DecodeCore(
        ReadOnlySpan<byte> input,
        Span<byte> output,
        ArchiveLibCompressionLevel level,
        bool exactLength,
        out int bytesWritten)
    {
        if (input.IsEmpty)
        {
            throw new ArchiveLibException("Compressed data is empty.");
        }

        int windowSize = 1 << (10 + (int)level);
        var state = new ArchiveLibDecoderState();
        var reader = new BitReader(input);
        int written = 0;

        while (true)
        {
            // Block header: 16-bit item count followed by the three Huffman tables.
            // An item count of 0 ends the stream — this is also how a stream that ran
            // past its real input terminates, because overrun bits read as zeros.
            int itemCount = reader.ReadBits(16);

            ReadCodeLengthTable(ref reader, state);
            ReadLiteralTable(ref reader, state);
            ReadOffsetTable(ref reader, state);

            if (itemCount == 0)
            {
                break;
            }

            for (int item = 0; item < itemCount; item++)
            {
                int symbol = state.LiteralTable.DecodeSymbol(ref reader);

                if (symbol < 256)
                {
                    if (written >= output.Length)
                    {
                        ThrowOutputOverflow(exactLength);
                    }

                    output[written++] = (byte)symbol;
                    continue;
                }

                if (symbol == ArchiveLibDecoderState.EndOfStreamSymbol)
                {
                    if (reader.HasOverrun)
                    {
                        throw new ArchiveLibException("Compressed data is truncated: the stream ran past the end of the input.");
                    }

                    bytesWritten = written;
                    return;
                }

                CopyBackReference(ref reader, state, output, ref written, symbol, windowSize, exactLength);
            }
        }

        if (reader.HasOverrun && written == 0)
        {
            throw new ArchiveLibException("Compressed data is truncated or not an ArchiveLib stream.");
        }

        bytesWritten = written;
    }

    /// <summary>
    /// Decodes one back-reference (length symbol 256..509) and copies it into the output.
    /// </summary>
    private static void CopyBackReference(
        ref BitReader reader,
        ArchiveLibDecoderState state,
        Span<byte> output,
        ref int written,
        int lengthSymbol,
        int windowSize,
        bool exactLength)
    {
        int runLength = lengthSymbol - 256 + MinRunLength;

        // Offset: the offset table yields a bit width; widths >= 2 are followed by
        // (width - 1) literal bits completing the value (1 << (width-1)) | extraBits.
        int offsetBitWidth = state.OffsetTable.DecodeSymbol(ref reader);
        int offset = offsetBitWidth switch
        {
            0 => 0,
            1 => 1,
            _ => (1 << (offsetBitWidth - 1)) | reader.ReadBits(offsetBitWidth - 1),
        };

        // The copy source starts offset + 1 bytes back. It must lie inside the bytes
        // already produced and inside the sliding window of the compression level.
        int historyLength = Math.Min(written, windowSize);
        if (offset >= historyLength)
        {
            throw new ArchiveLibException(
                written == 0
                    ? "Corrupt compressed data: back-reference before the start of the output."
                    : $"Corrupt compressed data: back-reference offset {offset} exceeds the available history ({historyLength} bytes).");
        }

        if (written + runLength > output.Length)
        {
            ThrowOutputOverflow(exactLength);
        }

        int source = written - offset - 1;
        if (offset + 1 >= runLength)
        {
            // Source and destination do not overlap; copy as one span.
            output.Slice(source, runLength).CopyTo(output.Slice(written));
            written += runLength;
        }
        else
        {
            // Overlapping copy (e.g. offset 0 repeats the previous byte); must run
            // byte by byte in forward order, exactly like the original.
            for (int i = 0; i < runLength; i++)
            {
                output[written + i] = output[source + i];
            }

            written += runLength;
        }
    }

    private static void ThrowOutputOverflow(bool exactLength)
    {
        throw new ArchiveLibException(exactLength
            ? "Decompressed data is larger than the expected length."
            : "Decompressed data does not fit in the destination buffer.");
    }

    /// <summary>
    /// Reads the 19-symbol code-length table (the table used to decode the literal
    /// table's code lengths).
    /// </summary>
    /// <remarks>
    /// Layout: a 5-bit entry count. Count 0 means a single symbol follows in 5 bits.
    /// Otherwise each entry is a code length in the 3-bit-plus-unary encoding of
    /// <see cref="ReadCodeLengthValue"/>, and — a quirk specific to this table — after the
    /// third entry a 2-bit gap count skips 0..3 symbols (they get length 0, i.e. unused).
    /// Entries beyond the count are unused. The reference caps the count at 19 rather than
    /// rejecting larger values; this implementation does the same.
    /// </remarks>
    private static void ReadCodeLengthTable(ref BitReader reader, ArchiveLibDecoderState state)
    {
        const int SymbolCount = ArchiveLibDecoderState.CodeLengthSymbolCount;
        int entryCount = Math.Min(reader.ReadBits(5), SymbolCount);

        if (entryCount == 0)
        {
            int symbol = reader.ReadBits(5);
            if (symbol >= SymbolCount)
            {
                throw new ArchiveLibException("Invalid Huffman table: single-symbol value out of range.");
            }

            state.CodeLengthTable.BuildSingleSymbol(symbol);
            return;
        }

        Span<byte> lengths = state.LengthScratch.AsSpan(0, SymbolCount);
        lengths.Clear();

        int index = 0;
        while (index < entryCount)
        {
            lengths[index++] = ReadCodeLengthValue(ref reader);
            if (index == 3)
            {
                // Symbols 0..2 are the gap symbols; a 2-bit field right after them can
                // skip up to 3 of the (rarely used) small length symbols.
                index += reader.ReadBits(2);
            }
        }

        state.CodeLengthTable.Build(lengths);
    }

    /// <summary>
    /// Reads the 511-symbol literal/length table using the code-length table.
    /// </summary>
    /// <remarks>
    /// Layout: a 9-bit entry count. Count 0 means a single symbol follows in 9 bits.
    /// Otherwise entries are decoded with the code-length table: symbol 0 marks one unused
    /// symbol, symbol 1 marks <c>3 + next(4 bits)</c> unused symbols, symbol 2 marks
    /// <c>20 + next(9 bits)</c> unused symbols, and symbols 3..18 assign the code length
    /// <c>symbol - 2</c> (1..16) to the current position.
    /// </remarks>
    private static void ReadLiteralTable(ref BitReader reader, ArchiveLibDecoderState state)
    {
        const int SymbolCount = ArchiveLibDecoderState.LiteralSymbolCount;
        int entryCount = Math.Min(reader.ReadBits(9), SymbolCount);

        if (entryCount == 0)
        {
            int symbol = reader.ReadBits(9);
            if (symbol >= SymbolCount)
            {
                throw new ArchiveLibException("Invalid Huffman table: single-symbol value out of range.");
            }

            state.LiteralTable.BuildSingleSymbol(symbol);
            return;
        }

        Span<byte> lengths = state.LengthScratch.AsSpan(0, SymbolCount);
        lengths.Clear();

        int index = 0;
        while (index < entryCount)
        {
            int value = state.CodeLengthTable.DecodeSymbol(ref reader);
            switch (value)
            {
                case 0:
                    index++;
                    break;
                case 1:
                    index += 3 + reader.ReadBits(4);
                    break;
                case 2:
                    index += 20 + reader.ReadBits(9);
                    break;
                default:
                    lengths[index++] = (byte)(value - 2);
                    break;
            }
        }

        state.LiteralTable.Build(lengths);
    }

    /// <summary>
    /// Reads the 15-symbol offset table. Same layout as the code-length table but without
    /// the 2-bit gap field after the third entry.
    /// </summary>
    private static void ReadOffsetTable(ref BitReader reader, ArchiveLibDecoderState state)
    {
        const int SymbolCount = ArchiveLibDecoderState.OffsetSymbolCount;
        int entryCount = Math.Min(reader.ReadBits(5), SymbolCount);

        if (entryCount == 0)
        {
            int symbol = reader.ReadBits(5);
            if (symbol >= SymbolCount)
            {
                throw new ArchiveLibException("Invalid Huffman table: single-symbol value out of range.");
            }

            state.OffsetTable.BuildSingleSymbol(symbol);
            return;
        }

        Span<byte> lengths = state.LengthScratch.AsSpan(0, SymbolCount);
        lengths.Clear();

        int index = 0;
        while (index < entryCount)
        {
            lengths[index++] = ReadCodeLengthValue(ref reader);
        }

        state.OffsetTable.Build(lengths);
    }

    /// <summary>
    /// Reads one code length: a 3-bit value, where 7 is extended by unary — each following
    /// 1-bit adds one, terminated by a 0-bit (so 7 is <c>111 0</c>, 8 is <c>111 10</c>, ...).
    /// </summary>
    /// <remarks>
    /// Values above 16 cannot occur in a valid table; they are clamped to 17 here and then
    /// rejected by <see cref="HuffmanDecoder.Build"/>. The loop always terminates because
    /// bits past the end of the input read as zero.
    /// </remarks>
    private static byte ReadCodeLengthValue(ref BitReader reader)
    {
        int value = reader.ReadBits(3);
        if (value == 7)
        {
            while (reader.ReadBit())
            {
                value++;
                if (value > 16)
                {
                    // Already invalid; stop counting so pathological inputs (e.g. megabytes
                    // of 0xFF) do not waste time. Consume the terminating convention lazily.
                    return 17;
                }
            }
        }

        return (byte)value;
    }
}
