using System.Drawing;
using System.Xml.Linq;
using Hoopful.Formats.Handlers;

namespace Hoopful.Formats;

/// <summary>
/// Maps file extensions to format handlers, ported from the original
/// <c>EmbroideryHandlerFactrory</c>.
/// </summary>
public static class EmbroideryFormatFactory
{
    /// <summary>All supported file extensions (lower case, with leading dot).</summary>
    public static readonly IReadOnlyList<string> SupportedExtensions =
    [
        ".hus", ".vip", ".vp3", ".dst", ".exp", ".jef", ".sew", ".ksm", ".pcs", ".pec", ".pes", ".xxx",
    ];

    /// <summary>
    /// Returns the handler for a file name/extension, or null when the format is not
    /// supported.
    /// </summary>
    public static IEmbroideryFormatHandler? GetHandler(string fileNameOrExtension)
    {
        return Path.GetExtension(fileNameOrExtension).ToLowerInvariant() switch
        {
            ".pcs" => new PcsFormatHandler(),
            ".hus" => new HusFormatHandler(),
            ".exp" => new ExpFormatHandler(),
            ".jef" => new JefFormatHandler(),
            ".sew" => new SewFormatHandler(),
            ".ksm" => new KsmFormatHandler(),
            ".dst" => new DstFormatHandler(),
            ".vip" => new VipFormatHandler(),
            ".vp3" => new Vp3FormatHandler(),
            ".pes" => new PesFormatHandler(),
            ".pec" => new PecFormatHandler(),
            ".xxx" => new XxxFormatHandler(),
            _ => null,
        };
    }

    /// <summary>Returns true when the extension of <paramref name="fileName"/> is supported.</summary>
    public static bool IsSupported(string fileName) => GetHandler(fileName) is not null;

    /// <summary>
    /// Parses an embroidery file. Throws <see cref="NotSupportedException"/> for unknown
    /// extensions; parse errors surface as the handler's own exceptions.
    /// </summary>
    public static EmbroideryDesign Load(byte[] fileData, string fileName)
    {
        IEmbroideryFormatHandler handler = GetHandler(fileName)
            ?? throw new NotSupportedException($"Unsupported embroidery format: {Path.GetExtension(fileName)}");
        return handler.Read(fileData, Path.GetFileName(fileName));
    }

    /// <summary>
    /// Applies a <c>.ytlc</c> colour side-car file to a design. The original application
    /// looked for <c>&lt;file&gt;.ytlc</c> next to each design and, when present, replaced
    /// the thread colours with the red/green/blue rows from that XML document.
    /// </summary>
    /// <returns>True when the override contained at least one colour and was applied.</returns>
    public static bool TryApplyColorOverride(EmbroideryDesign design, byte[] ytlcXml)
    {
        try
        {
            using var stream = new MemoryStream(ytlcXml);
            XDocument document = XDocument.Load(stream);

            var colors = new List<Color>();
            foreach (XElement row in document.Descendants())
            {
                XElement? red = row.Element("red");
                XElement? green = row.Element("green");
                XElement? blue = row.Element("blue");
                if (red is not null && green is not null && blue is not null)
                {
                    colors.Add(Color.FromArgb(
                        Math.Clamp((int)red, 0, 255),
                        Math.Clamp((int)green, 0, 255),
                        Math.Clamp((int)blue, 0, 255)));
                }
            }

            if (colors.Count == 0)
            {
                return false;
            }

            design.Colors = [.. colors];
            return true;
        }
        catch (Exception exception) when (exception is System.Xml.XmlException or FormatException)
        {
            return false;
        }
    }
}
