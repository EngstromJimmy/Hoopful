using Hoopful.ArchiveLib.Compression;
using Hoopful.Formats;
using Hoopful.Formats.Rendering;

if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine("usage: hoopful <design file> [output-directory]");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Parses an embroidery file and prints its metadata. With an output");
    Console.Error.WriteLine("directory, also renders the design to an .svg file.");
    Console.Error.WriteLine($"Supported formats: {string.Join(", ", EmbroideryFormatFactory.SupportedExtensions)}");
    return 2;
}

byte[] fileData;
try
{
    fileData = File.ReadAllBytes(args[0]);
}
catch (IOException exception)
{
    Console.Error.WriteLine($"error: {exception.Message}");
    return 2;
}

EmbroideryDesign design;
try
{
    design = EmbroideryFormatFactory.Load(fileData, args[0]);
}
catch (Exception exception) when (exception is ArchiveLibException or NotSupportedException or FormatException or ArgumentOutOfRangeException or IndexOutOfRangeException)
{
    Console.Error.WriteLine($"error: {exception.Message}");
    return 1;
}

Console.WriteLine($"design         : {design.Name}");
Console.WriteLine($"stitches       : {design.NumberOfStitches} ({design.Stitches.Count} records)");
Console.WriteLine($"colours        : {design.NumberOfColors}");
Console.WriteLine($"size           : {design.Width / 10.0} x {design.Height / 10.0} mm");
Console.WriteLine($"extents (0.1mm): X -{design.NegativeX}..{design.PositiveX}, Y -{design.NegativeY}..{design.PositiveY}");
for (int i = 0; i < design.Colors.Length; i++)
{
    Console.WriteLine($"colour {i,-2}      : #{design.Colors[i].R:X2}{design.Colors[i].G:X2}{design.Colors[i].B:X2}");
}

if (args.Length == 2)
{
    Directory.CreateDirectory(args[1]);
    string baseName = Path.GetFileNameWithoutExtension(args[0]);
    string svgPath = Path.Combine(args[1], baseName + ".svg");
    File.WriteAllText(svgPath, EmbroiderySvgRenderer.Render(design));
    string pngPath = Path.Combine(args[1], baseName + ".3d.png");
    File.WriteAllBytes(pngPath, EmbroideryBumpMapRenderer.RenderPng(design));
    Console.WriteLine($"rendered to {svgPath} (flat) and {pngPath} (bump mapped)");
}

return 0;
