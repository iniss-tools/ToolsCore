using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Jedno nastavenie v katalogu (hodnota v sekcii registra).
/// </summary>
/// <param name="name">nazov hodnoty v registri (pri farbach slovensky nazov z dokumentacie)</param>
/// <param name="type">typ</param>
/// <param name="defaultValue">predvolena hodnota</param>
/// <param name="write">ako hodnota v registri vznika</param>
/// <param name="descriptionKey">kluc popisu v <see cref="RegTexts" /></param>
public sealed class RegSetting(string name, RegValueType type, RegDefault defaultValue, RegWriteMode write, string descriptionKey)
{
    /// <summary>Nazov hodnoty (sablona <c>Enabled&lt;N&gt;</c> pri <see cref="RegNameKind.Indexed" />).</summary>
    public string Name { get; } = name;

    /// <summary>Typ.</summary>
    public RegValueType Type { get; } = type;

    /// <summary>Predvolena hodnota.</summary>
    public RegDefault Default { get; } = defaultValue;

    /// <summary>Ako hodnota v registri vznika.</summary>
    public RegWriteMode Write { get; } = write;

    /// <summary>Kluc popisu.</summary>
    public string DescriptionKey { get; } = descriptionKey;

    /// <summary>Druh nazvu.</summary>
    public RegNameKind Kind { get; init; }

    /// <summary>Poradie farby (0-41) pri <see cref="RegNameKind.Color" />.</summary>
    public int ColorIndex { get; init; } = -1;

    /// <summary>Kluc slovneho popisu dynamickej predvolby (<c>podľa triedy</c>).</summary>
    public string? DynamicDefaultKey { get; init; }

    /// <summary>Prva overena verzia, ktora hodnotu cita; null = vsetky.</summary>
    public RegVersion? Since { get; init; }

    /// <summary>Posledna overena verzia, ktora hodnotu este citala; null = aj najnovsia.</summary>
    public RegVersion? Until { get; init; }

    /// <summary>Vetva, ak sa lisi od sekcie (Tables: hodnoty tabul su per-user, BlackOut spolocny).</summary>
    public RegHive? HiveOverride { get; init; }

    /// <summary>Program hodnotu len zapisuje, necita ju (HlasitKolej).</summary>
    public bool WriteOnly { get; init; }

    /// <summary>Stary nazov bez jednotky, ktory INISS precita a premenuje na novy.</summary>
    public string? LegacyName { get; init; }

    /// <summary>Hodnoty, ktore program rozlisuje.</summary>
    public IReadOnlyList<RegChoice> Choices { get; init; } = [];

    /// <summary>Sekcia, do ktorej nastavenie patri (doplni katalog).</summary>
    public RegSection Section { get; internal set; } = null!;

    /// <summary>Spatny zapis - INISS chybajucu hodnotu zalozi s predvolbou.</summary>
    public bool WritesBack => Write is RegWriteMode.Auto or RegWriteMode.AutoAndApp;

    /// <summary>Popis v jazyku UI.</summary>
    public string Description => RegTexts.Get(DescriptionKey);

    /// <summary>Slovny popis dynamickej predvolby v jazyku UI.</summary>
    public string? DynamicDefaultText => DynamicDefaultKey is null ? null : RegTexts.Get(DynamicDefaultKey);

    /// <summary>Ci verzia INISSu hodnotu cita (null = verzia neznama, berie sa ako najnovsia).</summary>
    public bool IsAvailableIn(RegVersion? version)
    {
        if (WriteOnly) return false;
        if (version is not { } v) return Until is null;
        return (Since is not { } s || v >= s) && (Until is not { } u || v <= u);
    }

    /// <summary>Nazov hodnoty pre index tabule (len <see cref="RegNameKind.Indexed" />).</summary>
    public string NameFor(int index) => Name.Replace("<N>", index.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => $"{Section?.Name}\\{Name}";
}
