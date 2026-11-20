namespace ToolsCore.Iniss.Elen;

/// <summary>
/// Riadok textu po bodoch tabule: <see cref="Width" /> × <see cref="Height" /> bodov a poloha kazdeho znaku.
/// Vytvara ho <see cref="ElenText.Render" /> alebo program z vlastnej bitovej mapy (bez znakov).
/// </summary>
public sealed class ElenTextRaster
{
    private readonly bool[] _bits;

    /// <param name="width">sirka v bodoch</param>
    /// <param name="height">vyska v bodoch</param>
    /// <param name="bits">body po riadkoch zhora, <paramref name="width" /> × <paramref name="height" /></param>
    /// <param name="glyphs">znaky textu zlava doprava; prazdny zoznam = len body</param>
    public ElenTextRaster(int width, int height, bool[] bits, IReadOnlyList<ElenTextGlyph> glyphs)
    {
        ArgumentNullException.ThrowIfNull(bits);
        ArgumentNullException.ThrowIfNull(glyphs);
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        if (bits.Length != width * height)
            throw new ArgumentException($"Raster must have {width * height} dots, not {bits.Length}.", nameof(bits));

        Width = width;
        Height = height;
        _bits = bits;
        Glyphs = glyphs;
    }

    /// <summary>Sirka v bodoch.</summary>
    public int Width { get; }

    /// <summary>Vyska v bodoch.</summary>
    public int Height { get; }

    /// <summary>Znaky textu zlava doprava.</summary>
    public IReadOnlyList<ElenTextGlyph> Glyphs { get; }

    /// <summary>Ci svieti bod <paramref name="x" />, <paramref name="y" /> (0, 0 = lavy horny).</summary>
    public bool IsSet(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height && _bits[y * Width + x];

    /// <summary>
    /// Poradie znaku, na ktorom je stlpec <paramref name="x" />; -1 = ziadny (mimo textu).
    /// </summary>
    public int GlyphAt(int x)
    {
        foreach (var glyph in Glyphs)
        {
            if (x >= glyph.X && x < glyph.X + glyph.Width)
                return glyph.Index;
        }

        return -1;
    }
}
