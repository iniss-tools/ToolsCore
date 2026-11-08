namespace ToolsCore.Iniss.Elen;

/// <summary>
/// Pismo ELEN - jedno z najviac 16 pisiem suboru .bin, vzdy 256 znakov.
/// </summary>
public sealed class ElenFont
{
    /// <summary>Pocet znakov v pisme.</summary>
    public const int GlyphCount = 256;

    /// <summary>
    /// Najvacsia sirka pisma, s ktorou pocita INISS2 - bitova mapu cita len s 1 alebo 2 bajtmi na riadok.
    /// </summary>
    public const int MaxSupportedWidth = 16;

    /// <param name="name">nazov pisma (najviac 20 znakov ASCII)</param>
    /// <param name="maxWidth">najvacsia sirka znaku v bodoch</param>
    /// <param name="height">vyska v bodoch</param>
    /// <param name="isProportional">proporcionalne pismo</param>
    /// <param name="glyphs">256 znakov</param>
    public ElenFont(string name, int maxWidth, int height, bool isProportional, IReadOnlyList<ElenGlyph> glyphs)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(glyphs);
        if (glyphs.Count != GlyphCount)
            throw new ArgumentException($"Font must have {GlyphCount} glyphs, not {glyphs.Count}.", nameof(glyphs));

        Name = name;
        MaxWidth = maxWidth;
        Height = height;
        IsProportional = isProportional;
        Glyphs = glyphs;
    }

    /// <summary>Nazov pisma.</summary>
    public string Name { get; }

    /// <summary>Najvacsia sirka znaku v bodoch.</summary>
    public int MaxWidth { get; }

    /// <summary>Vyska v bodoch.</summary>
    public int Height { get; }

    /// <summary>Proporcionalne pismo.</summary>
    public bool IsProportional { get; }

    /// <summary>Bajtov na riadok bitovej mapy znaku.</summary>
    public int BytesPerRow => BytesPerRowFor(MaxWidth);

    /// <summary>Znaky podla kodu bajtu, ktory prisiel tabuli.</summary>
    public IReadOnlyList<ElenGlyph> Glyphs { get; }

    /// <summary>Bajtov na riadok bitovej mapy pre sirku <paramref name="maxWidth" />.</summary>
    public static int BytesPerRowFor(int maxWidth) => (maxWidth + 7) / 8;

    /// <inheritdoc />
    public override string ToString() => Name;
}
