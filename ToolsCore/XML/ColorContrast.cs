namespace ToolsCore.XML;

/// <summary>
/// Kontrast farieb podla WCAG 2 (pomer jasov 1:1 az 21:1).
/// </summary>
public static class ColorContrast
{
    /// <summary>
    /// Odporucane minimum pre text.
    /// </summary>
    public const double MinimumText = 4.5;

    /// <summary>
    /// Pomer kontrastu dvoch farieb.
    /// </summary>
    public static double Ratio(Color a, Color b)
    {
        double la = Luminance(a), lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    /// <summary>
    /// Relativny jas farby (0 = cierna, 1 = biela).
    /// </summary>
    public static double Luminance(Color c) => 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);

    private static double Channel(byte value)
    {
        var v = value / 255.0;
        return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
    }
}
