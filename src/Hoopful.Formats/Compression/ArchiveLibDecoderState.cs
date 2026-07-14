namespace Hoopful.Formats.Compression;

/// <summary>
/// Reusable decoder state: the three Huffman tables of a compressed block plus the
/// scratch buffer used while reading code lengths.
/// </summary>
/// <remarks>
/// Every compressed block carries three tables, transmitted in this order:
/// <list type="number">
///   <item><description>
///   A 19-symbol <em>code-length code</em>: a small Huffman table used only to decode the
///   code lengths of the literal/length table that follows.
///   </description></item>
///   <item><description>
///   The 511-symbol <em>literal/length table</em>: symbols 0..255 are literal bytes,
///   256..509 encode run lengths 3..256 and 510 is the end-of-stream marker.
///   </description></item>
///   <item><description>
///   The 15-symbol <em>offset table</em>: each symbol is the bit width of a back-reference
///   offset (see <see cref="ArchiveLibDecompressor"/> for the offset encoding).
///   </description></item>
/// </list>
/// The arrays are allocated once per decompression call and reused for every block, so
/// the decoder performs no per-symbol and no per-block allocations.
/// </remarks>
internal sealed class ArchiveLibDecoderState
{
    /// <summary>Alphabet size of the literal/length table: 256 literals + 254 lengths + EOF.</summary>
    public const int LiteralSymbolCount = 511;

    /// <summary>Symbol marking the end of the compressed stream.</summary>
    public const int EndOfStreamSymbol = 510;

    /// <summary>Alphabet size of the code-length table (gaps 0..2 + lengths 3..18).</summary>
    public const int CodeLengthSymbolCount = 19;

    /// <summary>Alphabet size of the offset table (offset bit widths 0..14).</summary>
    public const int OffsetSymbolCount = 15;

    public readonly HuffmanDecoder CodeLengthTable = new(CodeLengthSymbolCount, 8);
    public readonly HuffmanDecoder LiteralTable = new(LiteralSymbolCount, 12);
    public readonly HuffmanDecoder OffsetTable = new(OffsetSymbolCount, 8);

    /// <summary>Scratch space for code lengths; sized for the largest table.</summary>
    public readonly byte[] LengthScratch = new byte[LiteralSymbolCount];
}
