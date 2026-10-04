namespace ToolsCore.Iniss.Registry;

/// <summary>Druh polozky na vycistenie konfiguracie.</summary>
public enum RegCleanupKind
{
    /// <summary>Pozostatok - nazov, ktory necita ziadna verzia INISSu.</summary>
    Leftover,

    /// <summary>Nazov, ktory INISS vobec nepozna (preklep, ina aplikacia).</summary>
    Unknown,

    /// <summary>Nazov farby inej jazykovej verzie INISSu.</summary>
    OtherLanguageColor,

    /// <summary>Sekcia, ktoru INISS nepozna.</summary>
    UnknownSection,

    /// <summary>Stary nazov bez jednotky vedla noveho - INISS ho uz necita.</summary>
    LegacyGhost,

    /// <summary>Hodnota len pod starym nazvom - INISS ju cita a pri zapise premenuje; da sa premenovat hned.</summary>
    LegacyName,

    /// <summary>Hodnota zleho typu - INISS ju ignoruje a pouzije predvolenu.</summary>
    WrongType,

    /// <summary>Kopia vo VirtualStore, ktora INISSu bez prav spravcu prebija odlisnu hodnotu v HKLM.</summary>
    VirtualStoreCopy
}

/// <summary>
/// Polozka na vycistenie konfiguracie - jedna hodnota (alebo cela sekcia) na jednom mieste registra.
/// </summary>
/// <param name="Kind">druh</param>
/// <param name="Location">miesto registra</param>
/// <param name="Section">sekcia</param>
/// <param name="Name">hodnota; null pri celej sekcii</param>
/// <param name="Raw">sucasna hodnota</param>
public sealed record RegCleanupItem(RegCleanupKind Kind, RegLocation Location, string Section, string? Name, RegRawValue? Raw)
{
    /// <summary>Nova hodnota (prevedeny typ, premenovana hodnota); null = hodnota sa zmaze.</summary>
    public RegRawValue? Replacement { get; init; }

    /// <summary>Novy nazov pri <see cref="RegCleanupKind.LegacyName" />.</summary>
    public string? NewName { get; init; }

    /// <summary>Hodnota v HKLM pri <see cref="RegCleanupKind.VirtualStoreCopy" />.</summary>
    public RegRawValue? Reference { get; init; }

    /// <summary>Odporucane vycistit (predvolene oznacene).</summary>
    public bool Recommended { get; init; }
}

/// <summary>
/// Rozdiel nastavenia medzi dvoma konfiguraciami (aktualnou a druhou - ina vetva, subor .reg alebo .INI).
/// </summary>
/// <param name="Section">sekcia</param>
/// <param name="Name">nazov hodnoty</param>
/// <param name="Type">typ</param>
/// <param name="Current">nastavenie v aktualnej konfiguracii (null = v nej nie je)</param>
/// <param name="Other">nastavenie v druhej konfiguracii (null = v nej nie je)</param>
public sealed record RegDifference(string Section, string Name, RegValueType Type, ResolvedSetting? Current, ResolvedSetting? Other)
{
    /// <summary>Hodnota je v druhej konfiguracii zapisana (nie predvolena).</summary>
    public bool OtherExplicit => Other is not null && RegTools.IsExplicit(Other.Source);

    /// <summary>Hodnota je v aktualnej konfiguracii zapisana (nie predvolena).</summary>
    public bool CurrentExplicit => Current is not null && RegTools.IsExplicit(Current.Source);

    /// <summary>Ucinne hodnoty su rovnake.</summary>
    public bool IsEqual => RegValues.AreEqual(Current?.Value, Other?.Value, Type);
}

/// <summary>
/// Nastroje nad konfiguraciou INISSu bez pristupu k registru: klon vetvy, export do .INI, porovnanie, prevzatie
/// hodnot, vycistenie a zalozenie/zrusenie vetvy HKCU. Vysledkom su plany zapisu, ktore vykona vrstva Windows.
/// </summary>
public static class RegTools
{
    private static readonly RegLocation[] RegistryLocations = [RegLocation.Machine, RegLocation.User, RegLocation.VirtualStore];

    /// <summary>Hodnota je zapisana v registri alebo .INI (nie predvolena, nie necitana).</summary>
    public static bool IsExplicit(RegSource source) => source is RegSource.Ini or RegSource.User or RegSource.VirtualStore or RegSource.Machine;

    /// <summary>Koren registra, v ktorom INISS nastavenie cita (per-user sekcie s vetvou HKCU).</summary>
    public static RegHive HiveOf(ResolvedSetting setting) => setting.Setting.HiveOverride ?? setting.Setting.Section.Hive;

    /// <summary>
    /// Plan, ktory skopiruje vsetky vetvy konfiguracie (HKLM, HKCU, VirtualStore - kazdu na svoje miesto) do novej
    /// vetvy; nazov novej vetvy sa urci pri vykonani planu. Subor .INI patri k programu, nie k vetve - nekopiruje sa.
    /// </summary>
    public static RegWritePlan ClonePlan(InissConfigSource source)
    {
        var ops = new List<RegWriteOp>();
        foreach (var location in RegistryLocations)
        {
            var branch = Branch(source, location);
            if (!branch.Exists) continue;
            ops.Add(RegWriteOp.CreateKey(location, ""));
            foreach (var section in branch.SectionNames)
            {
                ops.Add(RegWriteOp.CreateKey(location, section));
                foreach (var (name, value) in branch.Values(section))
                    if (value.Kind != RegRawKind.Other)
                        ops.Add(new RegWriteOp(location, section, name, value));
            }
        }

        return new RegWritePlan(ops);
    }

    /// <summary>
    /// Ucinne hodnoty konfiguracie ako subor .INI (cisla desiatkovo, bajty sestnastkovo).
    /// </summary>
    /// <param name="config">vyhodnotena konfiguracia</param>
    /// <param name="includeDefaults">aj predvolene hodnoty (inak len zapisane v registri alebo .INI)</param>
    public static InissIniFile ToIni(ResolvedConfig config, bool includeDefaults)
    {
        var ini = InissIniFile.Empty();
        foreach (var section in config.Sections)
        foreach (var setting in section.Settings)
        {
            if (setting.Source == RegSource.NotRead || setting.Value is null) continue;
            if (!includeDefaults && !IsExplicit(setting.Source)) continue;
            ini.Set(setting.Section, setting.Name, RegValues.ToIniText(setting.Value, setting.Setting.Type));
        }

        return ini;
    }

    /// <summary>
    /// Nastavenia oboch konfiguracii vo dvojiciach podla sekcie a nazvu (aj rovnake - filtruje volajuci). Hodnoty,
    /// ktore verzia INISSu necita, sa vynechaju.
    /// </summary>
    public static List<RegDifference> Compare(ResolvedConfig current, ResolvedConfig other)
    {
        static string Key(ResolvedSetting s) => s.Section + "\\" + s.Name;
        var others = new Dictionary<string, ResolvedSetting>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in other.Sections.SelectMany(s => s.Settings))
            others.TryAdd(Key(s), s);

        var result = new List<RegDifference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in current.Sections.SelectMany(s => s.Settings))
        {
            if (!seen.Add(Key(s))) continue;
            others.TryGetValue(Key(s), out var o);
            if (s.Source == RegSource.NotRead && (o is null || o.Source == RegSource.NotRead)) continue;
            result.Add(new RegDifference(s.Section, s.Name, s.Setting.Type, s.Source == RegSource.NotRead ? null : s,
                o is null || o.Source == RegSource.NotRead ? null : o));
        }

        foreach (var o in other.Sections.SelectMany(s => s.Settings))
            if (o.Source != RegSource.NotRead && seen.Add(Key(o)))
                result.Add(new RegDifference(o.Section, o.Name, o.Setting.Type, null, o));

        return result;
    }

    /// <summary>
    /// Zmeny, ktore prevezmu hodnoty druhej konfiguracie: zapisanu hodnotu zapisu, predvolenu (alebo chybajucu)
    /// obnovia na predvolenu - zmazu ju z aktualnej konfiguracie.
    /// </summary>
    public static List<RegChange> TakeOver(IEnumerable<RegDifference> differences, RegWriteTarget target) =>
        differences.Select(d => new RegChange(d.Section, d.Name, d.Type, d.OtherExplicit ? d.Other!.Value : null, target)).ToList();

    /// <summary>
    /// Co sa da v registri vycistit: pozostatky, nezname nazvy a sekcie, stare nazvy, hodnoty zleho typu a kopie vo
    /// VirtualStore, ktore prebijaju odlisnu hodnotu v HKLM. Subor .INI sa necisti (patri pouzivatelovi).
    /// </summary>
    public static List<RegCleanupItem> FindCleanup(ResolvedConfig config)
    {
        var src = config.Source;
        var items = new List<RegCleanupItem>();
        foreach (var section in config.Sections)
        {
            foreach (var extra in section.Extra.Where(e => e.Location != RegLocation.Ini))
            {
                var code = section.Diagnostics.FirstOrDefault(d => string.Equals(d.Name, extra.Name, StringComparison.OrdinalIgnoreCase))?.Code;
                var kind = code switch
                {
                    RegDiagnosticCode.Leftover => RegCleanupKind.Leftover,
                    RegDiagnosticCode.ColorOtherLanguage => RegCleanupKind.OtherLanguageColor,
                    RegDiagnosticCode.Unknown => RegCleanupKind.Unknown,
                    _ => (RegCleanupKind?)null
                };
                if (kind is { } k)
                    items.Add(new RegCleanupItem(k, extra.Location, section.Name, extra.Name, extra.Raw) { Recommended = k == RegCleanupKind.Leftover });
            }

            foreach (var setting in section.Settings)
            foreach (var layer in setting.Layers.Where(l => l.Location != RegLocation.Ini))
            {
                var type = setting.Setting.Type;
                var legacy = !string.Equals(layer.Name, setting.Name, StringComparison.OrdinalIgnoreCase);
                if (legacy)
                {
                    if (Branch(src, layer.Location).Get(section.Name, setting.Name) is not null)
                        items.Add(new RegCleanupItem(RegCleanupKind.LegacyGhost, layer.Location, section.Name, layer.Name, layer.Raw) { Recommended = true });
                    else
                        items.Add(new RegCleanupItem(RegCleanupKind.LegacyName, layer.Location, section.Name, layer.Name, layer.Raw)
                        {
                            NewName = setting.Name, Replacement = RegValues.Convert(type, layer.Raw), Recommended = true
                        });
                    continue;
                }

                if (!RegValues.Matches(type, layer.Raw))
                {
                    items.Add(new RegCleanupItem(RegCleanupKind.WrongType, layer.Location, section.Name, layer.Name, layer.Raw)
                    {
                        Replacement = RegValues.Convert(type, layer.Raw), Recommended = true
                    });
                    continue;
                }

                // kopia vo VirtualStore odlisna od HKLM (okrem hodnot, ktore si INISS prepisuje sam)
                if (layer.Location == RegLocation.VirtualStore && setting.Setting.Write is not (RegWriteMode.AutoAndApp or RegWriteMode.App)
                    && src.Machine.Get(section.Name, layer.Name) is { } machine && RegValues.Matches(type, machine) && !machine.Equals(layer.Raw))
                    items.Add(new RegCleanupItem(RegCleanupKind.VirtualStoreCopy, layer.Location, section.Name, layer.Name, layer.Raw) { Reference = machine });
            }
        }

        foreach (var diag in config.UnknownSections)
        foreach (var location in RegistryLocations)
            if (Branch(src, location).HasSection(diag.Section))
                items.Add(new RegCleanupItem(RegCleanupKind.UnknownSection, location, diag.Section, null, null));

        return items;
    }

    /// <summary>Plan, ktory vycisti vybrane polozky.</summary>
    public static RegWritePlan CleanupPlan(IEnumerable<RegCleanupItem> items)
    {
        var ops = new List<RegWriteOp>();
        foreach (var item in items)
        {
            switch (item.Kind)
            {
                case RegCleanupKind.UnknownSection:
                    ops.Add(RegWriteOp.DeleteSection(item.Location, item.Section));
                    break;
                case RegCleanupKind.LegacyName when item.NewName is not null && item.Replacement is not null:
                    ops.Add(new RegWriteOp(item.Location, item.Section, item.NewName, item.Replacement));
                    ops.Add(new RegWriteOp(item.Location, item.Section, item.Name!, null));
                    break;
                case RegCleanupKind.WrongType when item.Replacement is not null:
                    ops.Add(new RegWriteOp(item.Location, item.Section, item.Name!, item.Replacement));
                    break;
                default:
                    ops.Add(new RegWriteOp(item.Location, item.Section, item.Name!, null));
                    break;
            }
        }

        return new RegWritePlan(ops);
    }

    /// <summary>
    /// Plan, ktory zalozi vetvu HKCU. INISS potom cita per-user sekcie len z HKCU - pri <paramref name="copyValues" />
    /// sa do nej skopiruju ich sucasne zapisane hodnoty, aby sa spravanie INISSu nezmenilo.
    /// </summary>
    public static RegWritePlan CreateUserBranchPlan(ResolvedConfig config, bool copyValues)
    {
        var ops = new List<RegWriteOp> { RegWriteOp.CreateKey(RegLocation.User, "") };
        if (copyValues)
            foreach (var setting in config.Sections.SelectMany(s => s.Settings))
                if (HiveOf(setting) == RegHive.User && setting.Source is RegSource.Machine or RegSource.VirtualStore && setting.Value is not null)
                    ops.Add(new RegWriteOp(RegLocation.User, setting.Section, setting.Name, RegValues.ToRaw(setting.Value, setting.Setting.Type)));
        return new RegWritePlan(ops);
    }

    /// <summary>
    /// Plan, ktory zrusi vetvu HKCU (INISS potom cita vsetko z HKLM). Pri <paramref name="moveToMachine" /> sa
    /// hodnoty per-user sekcii, ktore INISS cital z HKCU, presunu do HKLM.
    /// </summary>
    public static RegWritePlan RemoveUserBranchPlan(ResolvedConfig config, bool moveToMachine)
    {
        var ops = new List<RegWriteOp>();
        if (moveToMachine)
        {
            foreach (var setting in config.Sections.SelectMany(s => s.Settings))
            {
                if (setting.Source != RegSource.User || setting.Value is null) continue;
                ops.Add(new RegWriteOp(RegLocation.Machine, setting.Section, setting.Name, RegValues.ToRaw(setting.Value, setting.Setting.Type)));
                if (config.Source.VirtualStore.Get(setting.Section, setting.Name) is not null)
                    ops.Add(new RegWriteOp(RegLocation.VirtualStore, setting.Section, setting.Name, null));
            }
        }

        ops.Add(RegWriteOp.DeleteBranch(RegLocation.User));
        return new RegWritePlan(ops);
    }

    /// <summary>
    /// Zdroj konfiguracie zo suboru .reg pre porovnanie: vetvy aplikacie <paramref name="appName" /> (ak v subore nie
    /// je, prva aplikacia zo suboru) na svojich miestach; verzia, rezim, .INI a tabule z <paramref name="template" />
    /// sa prevezmu, .INI nie.
    /// </summary>
    public static InissConfigSource SourceFromRegFile(IReadOnlyList<RegFileBranch> branches, InissConfigSource template)
    {
        var app = branches.Any(b => string.Equals(b.AppName, template.AppName, StringComparison.OrdinalIgnoreCase))
            ? template.AppName
            : branches.Count > 0 ? branches[0].AppName : template.AppName;
        RegBranch Find(RegLocation location) =>
            branches.FirstOrDefault(b => b.Location == location && string.Equals(b.AppName, app, StringComparison.OrdinalIgnoreCase))?.Branch ?? RegBranch.Missing;
        return new InissConfigSource
        {
            AppName = app, Version = template.Version, RunMode = template.RunMode, ColorNames = template.ColorNames, Tables = template.Tables,
            Machine = Find(RegLocation.Machine), User = Find(RegLocation.User), VirtualStore = Find(RegLocation.VirtualStore)
        };
    }

    /// <summary>Zdroj konfiguracie zo suboru .INI pre porovnanie (len subor, bez registra).</summary>
    public static InissConfigSource SourceFromIni(InissIniFile ini, InissConfigSource template) => new()
    {
        AppName = template.AppName, Version = template.Version, RunMode = template.RunMode, ColorNames = template.ColorNames, Tables = template.Tables,
        Ini = ini
    };

    private static RegBranch Branch(InissConfigSource src, RegLocation loc) => loc switch
    {
        RegLocation.User => src.User,
        RegLocation.VirtualStore => src.VirtualStore,
        _ => src.Machine
    };
}
