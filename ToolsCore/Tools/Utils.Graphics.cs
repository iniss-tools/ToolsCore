using System.Drawing.Text;
using System.Globalization;

namespace ToolsCore.Tools;

/// <summary>
/// Farby a pisma.
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// Vrati farbu zadanu v hexadecimalnom tvare (BGR) z objektu <see cref="Color"/>.
    /// </summary>
    /// <param name="c">Farbu <see cref="Color"/>.</param>
    /// <returns>farba v hexadecimalnom tvare.</returns>
    public static string ToHex(this Color c) => "0x" + c.B.ToString("X2", CultureInfo.InvariantCulture) + c.G.ToString("X2", CultureInfo.InvariantCulture) + c.R.ToString("X2", CultureInfo.InvariantCulture);

    /// <summary>
    /// Vrati objekt Color z farby zadanej hexadecimalnou hodnotou (BGR).
    /// </summary>
    /// <param name="hex">Farba v hexadecimalnom tvare.</param>
    /// <returns>farbu <see cref="Color"/>.</returns>
    public static Color ParseHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);

        var replaced = hex.Replace("0x", "#");
        var color = ColorTranslator.FromHtml(replaced);
        var newColor = Color.FromArgb(color.B, color.G, color.R);
        return newColor;
    }

    /// <summary>
    /// Vrati objekt Color z farby zadanej hexadecimalnou hodnotou (BGR) alebo <see langword="null"/>.
    /// </summary>
    /// <param name="hex">Farba v hexadecimalnom tvare.</param>
    /// <returns>farbu <see cref="Color"/> alebo <see langword="null"/>, ak konvertovanie neprebehlo uspesne.</returns>
    public static Color? TryParseHex(string? hex)
    {
        if (hex == null) return null;
        try { return ParseHex(hex); } catch { return null; }
    }

    /// <summary>
    /// Vrati zoznam vsetkych systemovych fontov.
    /// </summary>
    /// <returns>zoznam systemovych fontov.</returns>
    public static List<string> GetSystemFontNames()
    {
        var fontnames = new List<string>();

        using var col = new InstalledFontCollection();
        fontnames.AddRange(col.Families.Select(fa => fa.Name));

        return fontnames;
    }

    /// <summary>
    /// Zisti, ci je <see cref="Font"/> <paramref name="ft"/> neproporcionalny.
    /// </summary>
    /// <param name="g">Grafika, na ktorej sa bude testovat proporcialnost.</param>
    /// <param name="ft">Pismo na otestovanie.</param>
    /// <returns><see langword="true" /> ak je font neproporcionalny, inak <see langword="false"/>.</returns>
    public static bool IsFontMonospaced(Graphics g, Font ft)
    {
        ArgumentNullException.ThrowIfNull(g);

        ArgumentNullException.ThrowIfNull(ft);

        char[] charSizes = ['i', 'a', 'Z', '%', '#', 'a', 'B', 'l', 'm', ',', '.'];
        var charWidth = g.MeasureString("I", ft).Width;

        return charSizes.All(c => Math.Abs(g.MeasureString(c.ToString(), ft).Width - charWidth) <= 0.0001f);
    }
}
