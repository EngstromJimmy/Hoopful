namespace Hoopful.Formats.Handlers;

/// <summary>
/// A reader for one embroidery file format, ported from the original application's
/// <c>IEmbroideryHandler</c>. Handlers work on in-memory bytes so they run unchanged in
/// the browser (WebAssembly) where there is no file system.
/// </summary>
public interface IEmbroideryFormatHandler
{
    /// <summary>Parses <paramref name="fileData"/> into a design.</summary>
    /// <param name="fileData">The complete file contents.</param>
    /// <param name="name">Display name for the design (usually the file name).</param>
    EmbroideryDesign Read(byte[] fileData, string name);
}
