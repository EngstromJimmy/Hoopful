using System.Drawing;

namespace Hoopful.Formats;

/// <summary>The role of one stitch record, as classified by the original application.</summary>
public enum StitchType
{
    /// <summary>
    /// Unrecognised command byte. The original application left these at the enum default;
    /// its renderer neither drew them nor advanced the needle position, and this port
    /// preserves that behaviour.
    /// </summary>
    Ignored = 0,

    /// <summary>A normal stitch: move the needle and draw thread.</summary>
    Normal = 1,

    /// <summary>A jump/trim movement: the needle moves without stitching.</summary>
    Jump = 2,

    /// <summary>Switch to the next thread colour.</summary>
    ColorChange = 3,

    /// <summary>Machine stop.</summary>
    Stop = 4,
}

/// <summary>One stitch record: a relative movement plus its type.</summary>
public sealed class Stitch
{
    /// <summary>Relative X movement in 0.1 mm units.</summary>
    public int X { get; set; }

    /// <summary>Relative Y movement in 0.1 mm units.</summary>
    public int Y { get; set; }

    /// <summary>What this record does.</summary>
    public StitchType Type { get; set; } = StitchType.Ignored;
}

/// <summary>
/// The in-memory representation of an embroidery design, ported from the original
/// application's <c>Embroidery</c> class.
/// </summary>
/// <remarks>
/// Coordinates are relative movements in 0.1 mm units. <see cref="StartXOffset"/> /
/// <see cref="StartYOffset"/> position the first stitch inside the
/// <see cref="Width"/> × <see cref="Height"/> canvas, exactly as the original renderer
/// expected. <see cref="RotateForDisplay"/> mirrors the original <c>rotate</c> flag
/// (used by KSM) that selected a different final image flip.
/// </remarks>
public sealed class EmbroideryDesign
{
    /// <summary>Design name (usually the file name).</summary>
    public string Name { get; set; } = "";

    /// <summary>Number of stitches as reported by the file header (when available).</summary>
    public long NumberOfStitches { get; set; }

    /// <summary>Number of thread colours.</summary>
    public int NumberOfColors { get; set; }

    /// <summary>Canvas width in 0.1 mm units.</summary>
    public int Width { get; set; }

    /// <summary>Canvas height in 0.1 mm units.</summary>
    public int Height { get; set; }

    /// <summary>Thread colours, indexed by colour-change order.</summary>
    public Color[] Colors { get; set; } = [];

    /// <summary>The stitch records in file order.</summary>
    public List<Stitch> Stitches { get; } = [];

    /// <summary>Largest X extent (0.1 mm).</summary>
    public int PositiveX { get; set; }

    /// <summary>Largest Y extent (0.1 mm).</summary>
    public int PositiveY { get; set; }

    /// <summary>Magnitude of the smallest X extent (0.1 mm).</summary>
    public int NegativeX { get; set; }

    /// <summary>Magnitude of the smallest Y extent (0.1 mm).</summary>
    public int NegativeY { get; set; }

    /// <summary>X position of the first stitch inside the canvas.</summary>
    public int StartXOffset { get; set; }

    /// <summary>Y position of the first stitch inside the canvas.</summary>
    public int StartYOffset { get; set; }

    /// <summary>
    /// When true the design is displayed rotated 270° + flipped instead of the default
    /// 180° + flip (the original app set this for KSM files).
    /// </summary>
    public bool RotateForDisplay { get; set; }
}
