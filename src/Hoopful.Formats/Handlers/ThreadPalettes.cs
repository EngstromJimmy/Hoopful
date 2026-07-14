using System.Drawing;

namespace Hoopful.Formats.Handlers;

/// <summary>Thread colour palettes copied verbatim from the original handlers.</summary>
internal static class ThreadPalettes
{
    /// <summary>
    /// Husqvarna/Viking standard palette used by HUS files; indexed by the palette number
    /// stored in the file header (0..28).
    /// </summary>
    public static readonly Color[] Husqvarna =
    [
        Color.FromArgb(0, 0, 0),        //  0 black
        Color.FromArgb(0, 0, 255),      //  1 blue
        Color.FromArgb(0, 255, 0),      //  2 green
        Color.FromArgb(255, 0, 0),      //  3 red
        Color.FromArgb(255, 0, 255),    //  4 purple
        Color.FromArgb(255, 255, 0),    //  5 yellow
        Color.FromArgb(132, 130, 132),  //  6 grey
        Color.FromArgb(0, 130, 255),    //  7 light blue
        Color.FromArgb(0, 255, 132),    //  8 light green
        Color.FromArgb(255, 130, 0),    //  9 orange
        Color.FromArgb(255, 162, 181),  // 10 pink
        Color.FromArgb(198, 65, 0),     // 11 brown
        Color.FromArgb(255, 255, 255),  // 12 white
        Color.FromArgb(0, 0, 132),      // 13 dark blue
        Color.FromArgb(0, 130, 0),      // 14 dark green
        Color.FromArgb(165, 0, 0),      // 15 dark red
        Color.FromArgb(255, 121, 123),  // 16 light red
        Color.FromArgb(132, 0, 132),    // 17 dark purple
        Color.FromArgb(255, 130, 255),  // 18 light purple
        Color.FromArgb(198, 195, 0),    // 19 dark yellow
        Color.FromArgb(255, 255, 165),  // 20 light yellow
        Color.FromArgb(66, 65, 66),     // 21 dark grey
        Color.FromArgb(198, 195, 198),  // 22 light grey
        Color.FromArgb(231, 65, 0),     // 23 dark orange
        Color.FromArgb(255, 174, 66),   // 24 light orange
        Color.FromArgb(255, 89, 123),   // 25 dark pink
        Color.FromArgb(255, 211, 214),  // 26 light pink
        Color.FromArgb(132, 32, 0),     // 27 dark brown
        Color.FromArgb(231, 97, 33),    // 28 light brown
    ];

    /// <summary>Janome palette used by JEF and SEW files (80 entries).</summary>
    public static readonly Color[] Janome =
    [
        Color.FromArgb(192, 192, 192),  //  0 not in use
        Color.FromArgb(0, 0, 0),
        Color.FromArgb(240, 240, 240),
        Color.FromArgb(255, 255, 23),
        Color.FromArgb(255, 102, 0),
        Color.FromArgb(47, 89, 51),
        Color.FromArgb(35, 115, 54),
        Color.FromArgb(101, 194, 200),
        Color.FromArgb(171, 90, 50),
        Color.FromArgb(246, 105, 160),
        Color.FromArgb(255, 0, 0),      // 10
        Color.FromArgb(156, 100, 69),
        Color.FromArgb(11, 47, 132),
        Color.FromArgb(228, 195, 93),
        Color.FromArgb(72, 26, 5),
        Color.FromArgb(171, 156, 199),
        Color.FromArgb(253, 145, 181),
        Color.FromArgb(249, 153, 183),
        Color.FromArgb(250, 179, 129),
        Color.FromArgb(215, 189, 164),
        Color.FromArgb(151, 5, 51),     // 20
        Color.FromArgb(160, 184, 204),
        Color.FromArgb(127, 194, 28),
        Color.FromArgb(229, 229, 229),
        Color.FromArgb(136, 155, 155),
        Color.FromArgb(152, 214, 189),
        Color.FromArgb(178, 225, 227),
        Color.FromArgb(152, 243, 254),
        Color.FromArgb(112, 169, 226),
        Color.FromArgb(29, 84, 120),
        Color.FromArgb(7, 22, 80),      // 30
        Color.FromArgb(255, 187, 187),
        Color.FromArgb(255, 96, 72),
        Color.FromArgb(255, 90, 39),
        Color.FromArgb(226, 161, 136),
        Color.FromArgb(181, 148, 116),
        Color.FromArgb(245, 219, 139),
        Color.FromArgb(255, 204, 0),
        Color.FromArgb(255, 189, 227),
        Color.FromArgb(195, 0, 126),
        Color.FromArgb(168, 0, 67),     // 40
        Color.FromArgb(84, 5, 113),
        Color.FromArgb(255, 9, 39),
        Color.FromArgb(198, 238, 203),
        Color.FromArgb(96, 133, 65),
        Color.FromArgb(96, 148, 24),
        Color.FromArgb(6, 72, 13),
        Color.FromArgb(91, 210, 181),
        Color.FromArgb(76, 181, 143),
        Color.FromArgb(4, 125, 123),
        Color.FromArgb(89, 91, 97),     // 50
        Color.FromArgb(255, 255, 220),
        Color.FromArgb(230, 101, 30),
        Color.FromArgb(230, 150, 90),
        Color.FromArgb(240, 156, 150),
        Color.FromArgb(167, 108, 61),
        Color.FromArgb(180, 90, 48),
        Color.FromArgb(110, 57, 55),
        Color.FromArgb(92, 38, 37),
        Color.FromArgb(98, 49, 189),
        Color.FromArgb(20, 50, 156),    // 60
        Color.FromArgb(22, 95, 167),
        Color.FromArgb(196, 227, 157),
        Color.FromArgb(253, 51, 163),
        Color.FromArgb(238, 113, 175),
        Color.FromArgb(132, 49, 84),
        Color.FromArgb(163, 145, 102),
        Color.FromArgb(12, 137, 24),
        Color.FromArgb(247, 242, 151),
        Color.FromArgb(204, 153, 0),
        Color.FromArgb(199, 151, 60),   // 70
        Color.FromArgb(255, 157, 0),
        Color.FromArgb(255, 186, 94),
        Color.FromArgb(252, 241, 33),
        Color.FromArgb(255, 71, 32),
        Color.FromArgb(0, 181, 82),
        Color.FromArgb(2, 87, 181),
        Color.FromArgb(208, 186, 176),
        Color.FromArgb(227, 190, 129),
        Color.FromArgb(192, 192, 192),
    ];

    /// <summary>Fills an array with <paramref name="count"/> black entries (the original default).</summary>
    public static Color[] AllBlack(int count)
    {
        var colors = new Color[Math.Max(count, 0)];
        Array.Fill(colors, Color.FromArgb(0, 0, 0));
        return colors;
    }
}
