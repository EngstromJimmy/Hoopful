namespace Hoopful.Formats.Compression;

/// <summary>
/// Canonical Huffman decoder matching the code assignment used by Greenleaf ArchiveLib.
/// </summary>
/// <remarks>
/// <para>
/// ArchiveLib transmits Huffman tables as a list of per-symbol code lengths (0 = symbol
/// unused, 1..16 = code length in bits). Codes are assigned canonically: shorter codes
/// first, and among codes of equal length, lower symbol values get lower codes. The set
/// of lengths must describe an <em>exactly complete</em> code (the Kraft sum must equal
/// 2^16 when each length <c>l</c> contributes <c>2^(16-l)</c>); both the original C++
/// implementation and the Rust reference reject anything else.
/// </para>
/// <para>
/// Decoding uses the same two-level scheme as the original implementation: a direct
/// lookup table indexed by the top <see cref="_tableBits"/> bits of the 16-bit look-ahead
/// window resolves short codes in one step, and codes longer than <see cref="_tableBits"/>
/// fall back to walking a small binary trie one bit at a time. The original uses 12 index
/// bits for the 511-symbol literal/length table and 8 for the two small tables; the same
/// values are used here.
/// </para>
/// <para>
/// A table can also consist of a single symbol (transmitted with a count field of 0).
/// Decoding such a table yields that symbol and consumes <em>zero</em> bits — this odd
/// but important behaviour is inherited from ArchiveLib and real HUS/VIP streams use it
/// (for example an attribute stream that is a single run needs almost no table bits).
/// </para>
/// </remarks>
internal sealed class HuffmanDecoder
{
    private const ushort EmptySlot = ushort.MaxValue;

    /// <summary>Number of symbols in the alphabet (511, 19 or 15).</summary>
    private readonly int _symbolCount;

    /// <summary>Number of look-ahead bits resolved by the direct lookup table.</summary>
    private readonly int _tableBits;

    /// <summary>
    /// Direct lookup table (1 &lt;&lt; <see cref="_tableBits"/> entries). An entry is either a
    /// symbol (&lt; <see cref="_symbolCount"/>) or <see cref="_symbolCount"/> + trie node index.
    /// </summary>
    private readonly ushort[] _primaryTable;

    /// <summary>Trie children for codes longer than <see cref="_tableBits"/> (0-bit branch).</summary>
    private readonly ushort[] _trieZero;

    /// <summary>Trie children for codes longer than <see cref="_tableBits"/> (1-bit branch).</summary>
    private readonly ushort[] _trieOne;

    /// <summary>Code length in bits per symbol; used to advance the bit reader after a lookup.</summary>
    private readonly byte[] _codeLengths;

    private int _trieNodeCount;

    /// <summary>When ≥ 0 the table contains this single symbol and decoding consumes no bits.</summary>
    private int _singleSymbol = -1;

    public HuffmanDecoder(int symbolCount, int tableBits)
    {
        _symbolCount = symbolCount;
        _tableBits = tableBits;
        _primaryTable = new ushort[1 << tableBits];
        _codeLengths = new byte[symbolCount];

        // Each code longer than tableBits creates at most (16 - tableBits) trie nodes.
        int maxTrieNodes = symbolCount * (16 - tableBits);
        _trieZero = new ushort[maxTrieNodes];
        _trieOne = new ushort[maxTrieNodes];
    }

    /// <summary>Configures the table to always produce <paramref name="symbol"/> using zero bits.</summary>
    public void BuildSingleSymbol(int symbol)
    {
        _singleSymbol = symbol;
    }

    /// <summary>
    /// Builds the decoding tables from per-symbol code lengths.
    /// Throws <see cref="ArchiveLibException"/> when the lengths do not form a complete code.
    /// </summary>
    public void Build(ReadOnlySpan<byte> codeLengths)
    {
        if (codeLengths.Length != _symbolCount)
        {
            throw new ArgumentException("Code length array does not match the alphabet size.", nameof(codeLengths));
        }

        _singleSymbol = -1;
        _trieNodeCount = 0;
        _primaryTable.AsSpan().Fill(EmptySlot);
        codeLengths.CopyTo(_codeLengths);

        // Count codes per length and verify the code is exactly complete:
        // the Kraft sum of 2^(16-length) over all used symbols must equal 2^16.
        Span<int> lengthCount = stackalloc int[17];
        lengthCount.Clear();
        foreach (byte length in codeLengths)
        {
            if (length > 16)
            {
                throw new ArchiveLibException("Invalid Huffman table: code length exceeds 16 bits.");
            }

            lengthCount[length]++;
        }

        long kraftSum = 0;
        for (int length = 1; length <= 16; length++)
        {
            kraftSum += (long)lengthCount[length] << (16 - length);
        }

        if (kraftSum != 1L << 16)
        {
            throw new ArchiveLibException(
                kraftSum < 1L << 16
                    ? "Invalid Huffman table: code is incomplete (not all bit patterns are covered)."
                    : "Invalid Huffman table: code is oversubscribed.");
        }

        // First canonical code of each length, left-aligned in 16 bits.
        Span<int> nextCode16 = stackalloc int[17];
        int runningCode = 0;
        for (int length = 1; length <= 16; length++)
        {
            nextCode16[length] = runningCode;
            runningCode += lengthCount[length] << (16 - length);
        }

        for (int symbol = 0; symbol < codeLengths.Length; symbol++)
        {
            int length = codeLengths[symbol];
            if (length == 0)
            {
                continue;
            }

            int code16 = nextCode16[length];
            nextCode16[length] += 1 << (16 - length);

            if (length <= _tableBits)
            {
                // Short code: every table slot whose top `length` bits equal the code maps
                // directly to the symbol.
                int firstSlot = code16 >> (16 - _tableBits);
                int slotCount = 1 << (_tableBits - length);
                _primaryTable.AsSpan(firstSlot, slotCount).Fill((ushort)symbol);
            }
            else
            {
                InsertLongCode(symbol, code16, length);
            }
        }
    }

    /// <summary>
    /// Inserts a code longer than <see cref="_tableBits"/>: the primary table slot for its
    /// first <see cref="_tableBits"/> bits points at a trie that is walked with the
    /// remaining bits.
    /// </summary>
    private void InsertLongCode(int symbol, int code16, int length)
    {
        int slot = code16 >> (16 - _tableBits);

        // Bit just below the primary index bits, moving right as we descend.
        int bitMask = 1 << (15 - _tableBits);
        int remainingBits = length - _tableBits;

        // The primary slot acts as the root pointer of the trie.
        ushort[] currentArray = _primaryTable;
        int currentIndex = slot;

        while (remainingBits > 0)
        {
            ushort entry = currentArray[currentIndex];
            int nodeIndex;
            if (entry == EmptySlot)
            {
                nodeIndex = _trieNodeCount++;
                _trieZero[nodeIndex] = EmptySlot;
                _trieOne[nodeIndex] = EmptySlot;
                currentArray[currentIndex] = (ushort)(_symbolCount + nodeIndex);
            }
            else
            {
                // A complete prefix-free code never routes a long code through a leaf.
                nodeIndex = entry - _symbolCount;
            }

            bool bitIsOne = (code16 & bitMask) != 0;
            currentArray = bitIsOne ? _trieOne : _trieZero;
            currentIndex = nodeIndex;
            bitMask >>= 1;
            remainingBits--;
        }

        currentArray[currentIndex] = (ushort)symbol;
    }

    /// <summary>
    /// Decodes one symbol: peeks the 16-bit window, resolves the symbol via the lookup
    /// table (and trie for long codes), then consumes exactly the symbol's code length.
    /// </summary>
    public int DecodeSymbol(ref BitReader reader)
    {
        if (_singleSymbol >= 0)
        {
            return _singleSymbol;
        }

        int window = reader.Peek16();
        int entry = _primaryTable[window >> (16 - _tableBits)];

        if (entry >= _symbolCount)
        {
            // Long code: walk the trie with the bits below the primary index bits.
            int bitMask = 1 << (15 - _tableBits);
            do
            {
                if (entry == EmptySlot || bitMask == 0)
                {
                    throw new ArchiveLibException("Corrupt compressed data: bit pattern has no Huffman code assigned.");
                }

                int nodeIndex = entry - _symbolCount;
                entry = (window & bitMask) != 0 ? _trieOne[nodeIndex] : _trieZero[nodeIndex];
                bitMask >>= 1;
            }
            while (entry >= _symbolCount);
        }

        if (entry == EmptySlot)
        {
            throw new ArchiveLibException("Corrupt compressed data: bit pattern has no Huffman code assigned.");
        }

        reader.Consume(_codeLengths[entry]);
        return entry;
    }
}
