using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>Typ hodnoty v registri INISSu.</summary>
public enum RegValueType
{
    /// <summary>REG_DWORD - cislo, program rozlisuje viac urovni.</summary>
    Dword,

    /// <summary>REG_DWORD redukovany na nula / nenula.</summary>
    Bool,

    /// <summary>REG_SZ v CP1250.</summary>
    String,

    /// <summary>REG_BINARY - ulozene rozlozenie okna, rucne needitovatelne.</summary>
    Binary,

    /// <summary>REG_DWORD s farbou 0x00BBGGRR; 0xFE000000 = systemova farba.</summary>
    Color
}

/// <summary>Vetva registra, z ktorej INISS hodnotu cita.</summary>
public enum RegHive
{
    /// <summary>HKCU, ak tam existuje kluc aplikacie, inak HKLM.</summary>
    User,

    /// <summary>Vzdy HKLM.</summary>
    Machine
}

/// <summary>Ako hodnota v registri vznika.</summary>
public enum RegWriteMode
{
    /// <summary>Zalozi sa sama pri prvom spusteni s predvolenou hodnotou (spatny zapis).</summary>
    Auto,

    /// <summary>Zalozi sa sama a program ju pocas behu prepisuje.</summary>
    AutoAndApp,

    /// <summary>Nezalozi sa sama, zapisuje ju program pri zmene nastavenia.</summary>
    App,

    /// <summary>Nezalozi sa sama; kym ju niekto nezalozi rucne, plati predvolena hodnota.</summary>
    None
}

/// <summary>Skupina sekcii v okne nastaveni (podla ucelu, nie podla nazvu sekcie).</summary>
public enum RegGroup
{
    /// <summary>Tabule - linky (Driver*) a prevadzka tabul (Tables).</summary>
    Boards,

    /// <summary>Zvuk a hlasenia.</summary>
    Sound,

    /// <summary>Grafikon.</summary>
    Timetable,

    /// <summary>Prostredie obsluhy (Environment).</summary>
    Operator,

    /// <summary>Logy.</summary>
    Logs,

    /// <summary>Rozhrania na ine systemy.</summary>
    Interfaces,

    /// <summary>Nazvy suborov.</summary>
    Files,

    /// <summary>Vlastnosti nasadenia.</summary>
    Installation,

    /// <summary>Vzhlad - farby, pismo, rozlozenie okna.</summary>
    Appearance,

    /// <summary>Ladenie a servisne hodnoty.</summary>
    Debug
}

/// <summary>Ako sa tvori nazov hodnoty.</summary>
public enum RegNameKind
{
    /// <summary>Pevny nazov.</summary>
    Fixed,

    /// <summary>Sablona s indexom tabule (<c>Enabled&lt;N&gt;</c>).</summary>
    Indexed,

    /// <summary>Nazov urcuje hardver alebo program za behu (prvky mixera).</summary>
    Dynamic,

    /// <summary>Farba - nazov je z jazykovej kniznice RCIniss.dll (retazce 10000-10041).</summary>
    Color
}

/// <summary>
/// Verzia INISSu (hlavna.vedlajsia) - porovnavacie body katalogu su 3.0, 3.10, 3.34 a 3.39.
/// </summary>
/// <param name="Major">hlavna verzia</param>
/// <param name="Minor">vedlajsia verzia ako cislo (3.10 = 10, nie 1)</param>
public readonly record struct RegVersion(int Major, int Minor) : IComparable<RegVersion>
{
    /// <inheritdoc />
    public int CompareTo(RegVersion other) => Major != other.Major ? Major.CompareTo(other.Major) : Minor.CompareTo(other.Minor);

    /// <summary>Mensia verzia.</summary>
    public static bool operator <(RegVersion a, RegVersion b) => a.CompareTo(b) < 0;

    /// <summary>Vacsia verzia.</summary>
    public static bool operator >(RegVersion a, RegVersion b) => a.CompareTo(b) > 0;

    /// <summary>Mensia alebo rovnaka verzia.</summary>
    public static bool operator <=(RegVersion a, RegVersion b) => a.CompareTo(b) <= 0;

    /// <summary>Vacsia alebo rovnaka verzia.</summary>
    public static bool operator >=(RegVersion a, RegVersion b) => a.CompareTo(b) >= 0;

    /// <summary>
    /// Verzia z textu suboru exe (<c>3.39</c>, <c>3.34.6</c>, <c>3.00</c>); null, ak text nie je verzia.
    /// </summary>
    public static RegVersion? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var parts = text.Trim().Split('.');
        if (parts.Length < 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minor))
            return null;
        return new RegVersion(major, minor);
    }

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor:00}").Replace(".00", ".0", StringComparison.Ordinal);
}

/// <summary>
/// Predvolena hodnota nastavenia: cislo, text, alebo dynamicka (urci ju INISS podla okolnosti - trieda linky,
/// vyrobca tabule, nazov sekcie).
/// </summary>
public sealed record RegDefault
{
    private RegDefault(int? number, string? text, bool dynamic)
    {
        Number = number;
        Text = text;
        IsDynamic = dynamic;
    }

    /// <summary>Ciselna predvolba.</summary>
    public int? Number { get; }

    /// <summary>Textova predvolba.</summary>
    public string? Text { get; }

    /// <summary>Predvolbu urcuje program za behu (napr. podla triedy linky).</summary>
    public bool IsDynamic { get; }

    /// <summary>Bez predvolby (binarne hodnoty).</summary>
    public static RegDefault None { get; } = new(null, null, false);

    /// <summary>Dynamicka predvolba.</summary>
    public static RegDefault Dynamic { get; } = new(null, null, true);

    /// <summary>Ciselna predvolba.</summary>
    public static RegDefault FromNumber(int value) => new(value, null, false);

    /// <summary>Textova predvolba.</summary>
    public static RegDefault FromText(string value) => new(null, value, false);

    /// <summary>Hodnota predvolby (int, string alebo null).</summary>
    public object? Value => Number is { } n ? n : Text;
}

/// <summary>Hodnota, ktoru program rozlisuje, s popisom (text v <see cref="RegTexts" />).</summary>
/// <param name="Value">hodnota tak, ako je v dokumentacii (<c>0</c>, <c>1–65535</c>, <c>*</c>…)</param>
/// <param name="TextKey">kluc popisu</param>
public sealed record RegChoice(string Value, string TextKey)
{
    /// <summary>Presne cislo, ak hodnota je jedno cislo (inak rozsah alebo slovny popis).</summary>
    public int? Number => int.TryParse(Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n) ? n : null;

    /// <summary>Popis hodnoty v jazyku UI.</summary>
    public string Text => RegTexts.Get(TextKey);
}

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

/// <summary>
/// Sekcia registra INISSu (podkluc pod <c>CHAPS\&lt;aplikacia&gt;</c>).
/// </summary>
/// <param name="name">nazov sekcie; pri cislovanych zaklad (<c>Driver</c>)</param>
/// <param name="hive">vetva</param>
/// <param name="group">skupina v okne</param>
/// <param name="maxIndex">najvyssi index cislovanej sekcie (Driver 98, Volume 8); -1 = jedina sekcia</param>
/// <param name="twoDigit">cislovana sekcia ma pre indexy 0-9 aj tvar 00-09</param>
/// <param name="descriptionKey">kluc popisu</param>
/// <param name="settings">nastavenia sekcie</param>
public sealed class RegSection(string name, RegHive hive, RegGroup group, int maxIndex, bool twoDigit, string descriptionKey, IReadOnlyList<RegSetting> settings)
{
    /// <summary>Nazov sekcie.</summary>
    public string Name { get; } = name;

    /// <summary>Vetva.</summary>
    public RegHive Hive { get; } = hive;

    /// <summary>Skupina v okne.</summary>
    public RegGroup Group { get; } = group;

    /// <summary>Najvyssi index cislovanej sekcie; -1 = jedina sekcia.</summary>
    public int MaxIndex { get; } = maxIndex;

    /// <summary>Pre indexy 0-9 aj dvojciferny tvar.</summary>
    public bool TwoDigit { get; } = twoDigit;

    /// <summary>Kluc popisu.</summary>
    public string DescriptionKey { get; } = descriptionKey;

    /// <summary>Nastavenia.</summary>
    public IReadOnlyList<RegSetting> Settings { get; } = settings;

    /// <summary>Cislovana sekcia (Driver, Volume).</summary>
    public bool IsNumbered => MaxIndex >= 0;

    /// <summary>Popis v jazyku UI.</summary>
    public string Description => RegTexts.Get(DescriptionKey);

    /// <summary>
    /// Nazvy podklucov v poradi, v akom ich INISS hlada: zaklad, potom 0..max a pri dvojcifernom tvare 00..09.
    /// </summary>
    public IEnumerable<string> InstanceNames()
    {
        if (!IsNumbered)
        {
            yield return Name;
            yield break;
        }

        yield return Name;
        for (var i = 0; i <= MaxIndex; i++)
        {
            yield return Name + i.ToString(CultureInfo.InvariantCulture);
            if (TwoDigit && i <= 9) yield return Name + i.ToString("00", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Nastavenie podla nazvu (bez rozlisenia velkosti pismen, ako register).</summary>
    public RegSetting? Find(string name) => Settings.FirstOrDefault(s => s.Kind == RegNameKind.Fixed && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string ToString() => Name;
}
