namespace ToolsCore.XML;

/// <summary>
/// Casti polozky farebnej schemy, ktore moze pouzivatel menit.
/// </summary>
[Flags]
public enum ColorParts
{
    /// <summary>Nic.</summary>
    None = 0,

    /// <summary>Farba popredia (textu).</summary>
    Fore = 1,

    /// <summary>Farba pozadia.</summary>
    Back = 2,

    /// <summary>Tucne pismo.</summary>
    Bold = 4,

    /// <summary>Popredie a pozadie.</summary>
    ForeBack = Fore | Back
}

/// <summary>
/// Zaklad farebnej schemy - predvolene farby svetlej alebo tmavej schemy. Urcuje aj tmave zahlavie okien, tmave
/// posuvniky a vlastne kreslenie prvkov.
/// </summary>
public enum SchemeBase
{
    /// <summary>Svetla schema (systemove farby a vzhlad prvkov).</summary>
    Light,

    /// <summary>Tmava schema.</summary>
    Dark
}

/// <summary>
/// Hodnota polozky farebnej schemy.
/// </summary>
/// <param name="Fore">farba popredia</param>
/// <param name="Back">farba pozadia (<see cref="Color.Empty" /> = polozka pozadie nema)</param>
/// <param name="Bold">tucne pismo</param>
public readonly record struct ColorValue(Color Fore, Color Back, bool Bold = false);

/// <summary>
/// Zmena polozky schemy voci jej zakladu; <see langword="null" /> = hodnota zo zakladu.
/// </summary>
public sealed record ColorOverride(Color? Fore = null, Color? Back = null, bool? Bold = null)
{
    /// <summary>
    /// Ziadna zmena.
    /// </summary>
    public bool IsEmpty => Fore is null && Back is null && Bold is null;
}

/// <summary>
/// Kategoria poloziek farebnej schemy (vetva stromu na stranke schemy) s volitelnym nahladom.
/// </summary>
/// <param name="Id">identifikator (zaciatok identifikatorov jej poloziek)</param>
/// <param name="Name">nazov v jazyku programu</param>
/// <param name="CreatePreview">nahlad kategorie; <see langword="null" /> = bez nahladu</param>
public sealed record ColorCategory(string Id, Func<string> Name, Func<IColorPreview>? CreatePreview = null);

/// <summary>
/// Nahlad kategorie farebnej schemy na stranke schemy: kresli farby schemy a klik vyberie polozku.
/// </summary>
public interface IColorPreview
{
    /// <summary>
    /// Prvok nahladu.
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Pouzivatel klikol na cast nahladu s danou polozkou (identifikator <see cref="ColorKey.Id" />).
    /// </summary>
    event EventHandler<string>? KeyClicked;

    /// <summary>
    /// Zobrazi farby schemy.
    /// </summary>
    void ShowScheme(Style style);
}

/// <summary>
/// Polozka farebnej schemy: identifikator v subore schem, nazov, predvolene hodnoty svetlej a tmavej schemy, co sa
/// da menit a kde ju pouziva program.
/// </summary>
/// <param name="id">identifikator v subore schem (nemenit)</param>
/// <param name="category">kategoria</param>
/// <param name="name">nazov v jazyku programu</param>
/// <param name="light">predvolena hodnota svetlej schemy</param>
/// <param name="dark">predvolena hodnota tmavej schemy</param>
/// <param name="editable">co moze pouzivatel menit</param>
/// <param name="setting">farby polozky v styl, ktore cita program</param>
/// <param name="legacyPath">cesta k prvku v starom subore stylov (napr. <c>ControlsColorScheme/Button</c>)</param>
/// <param name="contrastWith">polozka, ktorej pozadie je pod textom tejto polozky (pre kontrolu kontrastu), ak
/// polozka nema vlastne pozadie; <see langword="null" /> = kontrast sa nekontroluje</param>
public sealed class ColorKey(
    string id,
    ColorCategory category,
    Func<string> name,
    ColorValue light,
    ColorValue dark,
    ColorParts editable,
    Func<Style, ColorSetting> setting,
    string? legacyPath = null,
    string? contrastWith = null)
{
    /// <summary>Identifikator v subore schem.</summary>
    public string Id { get; } = id;

    /// <summary>Kategoria.</summary>
    public ColorCategory Category { get; } = category;

    /// <summary>Nazov v jazyku programu.</summary>
    public string Name => name();

    /// <summary>Predvolena hodnota svetlej schemy.</summary>
    public ColorValue Light { get; } = light;

    /// <summary>Predvolena hodnota tmavej schemy.</summary>
    public ColorValue Dark { get; } = dark;

    /// <summary>Co moze pouzivatel menit.</summary>
    public ColorParts Editable { get; } = editable;

    /// <summary>Farby polozky v style, ktore cita program.</summary>
    public Func<Style, ColorSetting> Setting { get; } = setting;

    /// <summary>Cesta k prvku v starom subore stylov.</summary>
    public string? LegacyPath { get; } = legacyPath;

    /// <summary>Polozka s pozadim pod textom (kontrola kontrastu).</summary>
    public string? ContrastWith { get; } = contrastWith;

    /// <summary>
    /// Predvolena hodnota pre zaklad schemy.
    /// </summary>
    public ColorValue DefaultFor(SchemeBase schemeBase) => schemeBase == SchemeBase.Dark ? Dark : Light;

    /// <inheritdoc />
    public override string ToString() => Name;
}

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
