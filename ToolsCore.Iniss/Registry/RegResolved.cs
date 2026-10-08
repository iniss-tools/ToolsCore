namespace ToolsCore.Iniss.Registry;

/// <summary>Ako je INISS spusteny - rozhoduje o virtualizacii registra (VirtualStore).</summary>
public enum InissRunMode
{
    /// <summary>Bez prav spravcu: zapisy do HKLM idu do VirtualStore a pri citani ma VirtualStore prednost.</summary>
    Normal,

    /// <summary>Ako spravca: INISS cita a zapisuje priamo HKLM, VirtualStore nevidi.</summary>
    Elevated
}

/// <summary>Miesto, kde hodnota lezi.</summary>
public enum RegLocation
{
    /// <summary>Subor .INI vedla programu.</summary>
    Ini,

    /// <summary>HKEY_CURRENT_USER\Software\CHAPS\aplikacia.</summary>
    User,

    /// <summary>Kopia HKLM pre pouzivatela (HKCU\Software\Classes\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\CHAPS\…).</summary>
    VirtualStore,

    /// <summary>HKEY_LOCAL_MACHINE\SOFTWARE\(WOW6432Node\)CHAPS\aplikacia.</summary>
    Machine
}

/// <summary>Odkial pochadza ucinna hodnota.</summary>
public enum RegSource
{
    /// <summary>Subor .INI.</summary>
    Ini,

    /// <summary>HKCU.</summary>
    User,

    /// <summary>VirtualStore.</summary>
    VirtualStore,

    /// <summary>HKLM.</summary>
    Machine,

    /// <summary>Predvolena hodnota podla triedy linky.</summary>
    ClassDefault,

    /// <summary>Predvolena hodnota programu.</summary>
    Default,

    /// <summary>Predvolenu hodnotu urci program za behu a editor ju nepozna.</summary>
    UnknownDefault,

    /// <summary>Tato verzia INISSu hodnotu necita.</summary>
    NotRead
}

/// <summary>Stav hodnoty v jednej vrstve.</summary>
public enum RegLayerState
{
    /// <summary>Z tejto vrstvy INISS hodnotu berie.</summary>
    Used,

    /// <summary>Prebija ju vrstva s vyssou prioritou.</summary>
    Shadowed,

    /// <summary>Zly typ - INISS ju neuzna.</summary>
    WrongType,

    /// <summary>INISS tuto vrstvu pre nastavenie necita (iny koren registra, rezim spustenia).</summary>
    NotRead
}

/// <summary>Hodnota v jednej vrstve konfiguracie.</summary>
/// <param name="Location">miesto</param>
/// <param name="Name">nazov, pod ktorym tam lezi (aj stary nazov bez jednotky)</param>
/// <param name="Raw">surova hodnota</param>
/// <param name="State">stav</param>
public sealed record RegLayerValue(RegLocation Location, string Name, RegRawValue Raw, RegLayerState State);

/// <summary>Zavaznost diagnostiky.</summary>
public enum RegSeverity
{
    /// <summary>Informacia.</summary>
    Info,

    /// <summary>Upozornenie.</summary>
    Warning,

    /// <summary>Chyba - INISS bude pracovat inak, nez sa zrejme ocakava.</summary>
    Error
}

/// <summary>Druh diagnostiky konfiguracie.</summary>
public enum RegDiagnosticCode
{
    /// <summary>Hodnota ma zly typ.</summary>
    WrongType,

    /// <summary>Prazdny text so spatnym zapisom sa nahradi predvolbou.</summary>
    EmptyText,

    /// <summary>Hodnotu v registri prebija .INI.</summary>
    ShadowedByIni,

    /// <summary>Hodnota v HKLM sa nepouzije, sekcia sa cita z HKCU.</summary>
    MachineIgnored,

    /// <summary>Hodnotu v HKLM prebija VirtualStore.</summary>
    ShadowedByVirtualStore,

    /// <summary>Kopia vo VirtualStore sa pri spusteni ako spravca nepouzije.</summary>
    VirtualStoreIgnored,

    /// <summary>Hodnota je pod starym nazvom, INISS ju premenuje.</summary>
    LegacyName,

    /// <summary>Stary nazov vedla noveho sa uz necita.</summary>
    LegacyGhost,

    /// <summary>Verzia INISSu hodnotu necita.</summary>
    NotInVersion,

    /// <summary>Hodnota, ktoru INISS len zapisuje.</summary>
    WriteOnly,

    /// <summary>Pozostatok - nazov necita ziadna verzia.</summary>
    Leftover,

    /// <summary>Neznamy nazov.</summary>
    Unknown,

    /// <summary>Neznama sekcia.</summary>
    UnknownSection,

    /// <summary>Farba inej jazykovej verzie.</summary>
    ColorOtherLanguage,

    /// <summary>Cislovana sekcia v .INI nahradza register.</summary>
    IniSectionReplacesRegistry,

    /// <summary>Sietovy kanal bez cisla linky.</summary>
    NetworkPortWithoutLine,

    /// <summary>Cislo v .INI nie je desiatkove.</summary>
    IniInvalidNumber
}

/// <summary>Zistenie o konfiguracii (text v jazyku UI).</summary>
/// <param name="Code">druh</param>
/// <param name="Severity">zavaznost</param>
/// <param name="Section">sekcia</param>
/// <param name="Name">hodnota alebo null pri celej sekcii</param>
/// <param name="Message">text</param>
public sealed record RegDiagnostic(RegDiagnosticCode Code, RegSeverity Severity, string Section, string? Name, string Message);

/// <summary>
/// Vyhodnotene nastavenie - ucinna hodnota, odkial pochadza a co lezi v jednotlivych vrstvach.
/// </summary>
public sealed class ResolvedSetting
{
    internal ResolvedSetting(RegSetting setting, string section, string name)
    {
        Setting = setting;
        Section = section;
        Name = name;
    }

    /// <summary>Nastavenie z katalogu.</summary>
    public RegSetting Setting { get; }

    /// <summary>Nazov sekcie (pri cislovanych konkretny, napr. <c>Driver3</c>).</summary>
    public string Section { get; }

    /// <summary>Nazov hodnoty (pri sablonach konkretny, napr. <c>Enabled3</c>; pri farbach nazov z jazykovej kniznice).</summary>
    public string Name { get; }

    /// <summary>Ucinna hodnota (int, string, byte[]) alebo null, ak ju editor nevie urcit.</summary>
    public object? Value { get; internal set; }

    /// <summary>Odkial ucinna hodnota pochadza.</summary>
    public RegSource Source { get; internal set; }

    /// <summary>Predvolena hodnota v tomto kontexte (pri Driver podla triedy) alebo null, ak je dynamicka.</summary>
    public object? DefaultValue { get; internal set; }

    /// <summary>Fyzicka tabula pri hodnotach Tables\…&lt;N&gt; (ak je znama).</summary>
    public RegTableInfo? Table { get; internal set; }

    /// <summary>Koren registra, z ktoreho INISS hodnotu cita a kam ju zapisuje (User alebo Machine).</summary>
    public RegLocation RegistryLocation { get; internal set; }

    /// <summary>Hodnoty vo vrstvach (od najvyssej priority).</summary>
    public IReadOnlyList<RegLayerValue> Layers { get; internal set; } = [];

    /// <summary>Zistenia k tomuto nastaveniu.</summary>
    public IReadOnlyList<RegDiagnostic> Diagnostics { get; internal set; } = [];

    /// <summary>Verzia INISSu hodnotu cita.</summary>
    public bool IsRead => Source != RegSource.NotRead;

    /// <summary>Ucinna hodnota sa rovna predvolenej (aj ked lezi v registri).</summary>
    public bool IsDefault => DefaultValue is not null && RegValues.AreEqual(Value, DefaultValue, Setting.Type);

    /// <inheritdoc />
    public override string ToString() => $"{Section}\\{Name} = {Value} ({Source})";
}

/// <summary>Vyhodnotena sekcia (pri cislovanych jedna konkretna, napr. <c>Driver3</c>).</summary>
public sealed class ResolvedSection
{
    internal ResolvedSection(RegSection definition, string name, bool fromIni)
    {
        Definition = definition;
        Name = name;
        FromIni = fromIni;
    }

    /// <summary>Sekcia z katalogu.</summary>
    public RegSection Definition { get; }

    /// <summary>Nazov podkluca.</summary>
    public string Name { get; }

    /// <summary>Cislovana sekcia je v .INI - register sa pre nu necita.</summary>
    public bool FromIni { get; }

    /// <summary>Nastavenia.</summary>
    public IReadOnlyList<ResolvedSetting> Settings { get; internal set; } = [];

    /// <summary>Hodnoty, ktore katalog nepozna (pozostatky, preklepy, farby inej jazykovej verzie).</summary>
    public IReadOnlyList<RegLayerValue> Extra { get; internal set; } = [];

    /// <summary>Zistenia k sekcii a k hodnotam mimo katalogu.</summary>
    public IReadOnlyList<RegDiagnostic> Diagnostics { get; internal set; } = [];

    /// <summary>Nastavenie podla nazvu.</summary>
    public ResolvedSetting? Find(string name) => Settings.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string ToString() => Name;
}

/// <summary>Vyhodnotena konfiguracia jednej vetvy INISSu.</summary>
public sealed class ResolvedConfig
{
    internal ResolvedConfig(InissConfigSource source, bool userBranchActive, IReadOnlyList<ResolvedSection> sections, IReadOnlyList<RegDiagnostic> unknownSections)
    {
        Source = source;
        UserBranchActive = userBranchActive;
        Sections = sections;
        UnknownSections = unknownSections;
    }

    /// <summary>Vstup.</summary>
    public InissConfigSource Source { get; }

    /// <summary>Vetva HKCU existuje - per-user sekcie sa citaju z HKCU.</summary>
    public bool UserBranchActive { get; }

    /// <summary>Sekcie v poradi katalogu.</summary>
    public IReadOnlyList<ResolvedSection> Sections { get; }

    /// <summary>Sekcie v registri alebo .INI, ktore katalog nepozna.</summary>
    public IReadOnlyList<RegDiagnostic> UnknownSections { get; }

    /// <summary>Vsetky zistenia.</summary>
    public IEnumerable<RegDiagnostic> Diagnostics =>
        UnknownSections.Concat(Sections.SelectMany(s => s.Diagnostics.Concat(s.Settings.SelectMany(v => v.Diagnostics))));

    /// <summary>Sekcia podla nazvu podkluca.</summary>
    public ResolvedSection? FindSection(string name) => Sections.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Nastavenie podla sekcie a nazvu.</summary>
    public ResolvedSetting? Find(string section, string name) => FindSection(section)?.Find(name);
}

/// <summary>Vstup vyhodnotenia - nacitane vrstvy jednej konfiguracie INISSu.</summary>
public sealed record InissConfigSource
{
    /// <summary>Nazov vetvy pod CHAPS (meno exe bez pripony alebo hodnota /Reg:).</summary>
    public required string AppName { get; init; }

    /// <summary>Verzia INISSu z exe; null = neznama (berie sa ako najnovsia).</summary>
    public RegVersion? Version { get; init; }

    /// <summary>Ako sa INISS spusta.</summary>
    public InissRunMode RunMode { get; init; }

    /// <summary>HKCU\Software\CHAPS\aplikacia.</summary>
    public RegBranch User { get; init; } = RegBranch.Missing;

    /// <summary>HKLM\SOFTWARE\(WOW6432Node\)CHAPS\aplikacia.</summary>
    public RegBranch Machine { get; init; } = RegBranch.Missing;

    /// <summary>VirtualStore kopia HKLM pre aktualneho pouzivatela.</summary>
    public RegBranch VirtualStore { get; init; } = RegBranch.Missing;

    /// <summary>Subor .INI vedla programu alebo null.</summary>
    public InissIniFile? Ini { get; init; }

    /// <summary>Nazvy 42 farieb z jazykovej kniznice RCIniss.dll; null = slovenske nazvy z katalogu.</summary>
    public IReadOnlyList<string>? ColorNames { get; init; }

    /// <summary>
    /// Fyzicke tabule podla indexu &lt;N&gt; v sekcii Tables (poradie v spojenom zozname tabul, ktory INISS nacita).
    /// Ich hodnoty sa ukazu aj ked v registri nie su; vyrobca urci predvoleny vynuteny jas.
    /// </summary>
    public IReadOnlyDictionary<int, RegTableInfo>? Tables { get; init; }
}

/// <summary>Fyzicka tabula, na ktoru sa vztahuju hodnoty Tables\…&lt;N&gt;.</summary>
/// <param name="Name">nazov tabule (KEY z TPhysic)</param>
/// <param name="Grafikon">grafikon (priecinok), z ktoreho tabula pochadza</param>
/// <param name="Manufacturer">kod vyrobcu z katalogovej predlohy alebo null</param>
/// <param name="Line">cislo komunikacnej linky (COMUNICATION_PORT)</param>
public sealed record RegTableInfo(string Name, string Grafikon, int? Manufacturer, int Line);
