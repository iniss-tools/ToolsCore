namespace ToolsCore.XML;

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
