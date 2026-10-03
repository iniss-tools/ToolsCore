namespace ToolsCore.Iniss.Registry;

/// <summary>Kam sa ma zmena zapisat.</summary>
public enum RegWriteTarget
{
    /// <summary>Do registra - koren (HKCU alebo HKLM) urci editor podla toho, odkial INISS hodnotu cita.</summary>
    Registry,

    /// <summary>Do suboru .INI vedla programu.</summary>
    Ini
}

/// <summary>
/// Zmena jedneho nastavenia.
/// </summary>
/// <param name="Section">sekcia (konkretna, napr. <c>Driver3</c>)</param>
/// <param name="Name">nazov hodnoty</param>
/// <param name="Type">typ hodnoty</param>
/// <param name="Value">nova hodnota (int, string, byte[]); null = odstranit zo vsetkych vrstiev (obnovit predvolenu)</param>
/// <param name="Target">kam zapisat</param>
public sealed record RegChange(string Section, string Name, RegValueType Type, object? Value, RegWriteTarget Target);

/// <summary>Jedna operacia na konkretnom mieste.</summary>
/// <param name="Location">miesto</param>
/// <param name="Section">sekcia</param>
/// <param name="Name">nazov hodnoty</param>
/// <param name="Value">nova hodnota; null = zmazat</param>
public sealed record RegWriteOp(RegLocation Location, string Section, string Name, RegRawValue? Value)
{
    /// <summary>Zmazanie hodnoty.</summary>
    public bool IsDelete => Value is null;

    /// <summary>Zmazanie celej sekcie (prazdny nazov hodnoty).</summary>
    public bool IsSectionDelete => Name.Length == 0 && Value is null;

    /// <summary>Operacia, ktora zmaze celu sekciu na danom mieste.</summary>
    public static RegWriteOp DeleteSection(RegLocation location, string section) => new(location, section, "", null);
}

/// <summary>
/// Operacie, ktore zmeny zapisu tak, aby mali u INISSu ucinok v oboch rezimoch spustenia.
/// </summary>
public sealed class RegWritePlan
{
    internal RegWritePlan(IReadOnlyList<RegWriteOp> ops, IReadOnlyList<RegChange> iniStillOverrides, IReadOnlyList<RegChange> registryNotRead)
    {
        Ops = ops;
        IniStillOverrides = iniStillOverrides;
        RegistryNotRead = registryNotRead;
    }

    /// <summary>Operacie v poradi vykonania.</summary>
    public IReadOnlyList<RegWriteOp> Ops { get; }

    /// <summary>Zmeny do registra, ktore aj po zapise prebije .INI (odstranenie zo suboru nebolo povolene).</summary>
    public IReadOnlyList<RegChange> IniStillOverrides { get; }

    /// <summary>Zmeny do registra v cislovanej sekcii zo suboru .INI - INISS ich necita.</summary>
    public IReadOnlyList<RegChange> RegistryNotRead { get; }

    /// <summary>Plan zapisuje do HKLM (potrebuje prava spravcu).</summary>
    public bool WritesMachine => Ops.Any(o => o.Location == RegLocation.Machine);

    /// <summary>Plan meni subor .INI.</summary>
    public bool WritesIni => Ops.Any(o => o.Location == RegLocation.Ini);
}

/// <summary>
/// Zostavi plan zapisu zmien nad vyhodnotenou konfiguraciou:
/// <list type="bullet">
/// <item>zapis do registra ide do korena, z ktoreho INISS hodnotu cita (HKCU pri per-user sekcii a existujucej vetve,
/// inak HKLM); kopia vo VirtualStore sa zmaze, aby ju INISS spusteny bez prav spravcu necital namiesto novej hodnoty;
/// stary nazov bez jednotky sa zmaze; hodnota v .INI sa (ak je to povolene) odstrani, inak by zmenu prebila,</item>
/// <item>zapis do .INI prepise alebo doplni hodnotu v subore, register ostane,</item>
/// <item>obnovenie predvolenej (null) zmaze hodnotu zo vsetkych vrstiev vratane stareho nazvu.</item>
/// </list>
/// </summary>
public static class RegWritePlanner
{
    /// <summary>Zostavi plan.</summary>
    /// <param name="config">vyhodnotena konfiguracia</param>
    /// <param name="changes">zmeny</param>
    /// <param name="removeIniOverride">pri zapise do registra odstranit tu istu hodnotu zo suboru .INI</param>
    public static RegWritePlan Plan(ResolvedConfig config, IEnumerable<RegChange> changes, bool removeIniOverride = true)
    {
        var ops = new List<RegWriteOp>();
        var iniStill = new List<RegChange>();
        var notRead = new List<RegChange>();
        var src = config.Source;
        foreach (var change in changes)
        {
            var resolved = config.Find(change.Section, change.Name);
            var section = config.FindSection(change.Section);
            var legacy = resolved?.Setting.LegacyName;
            var location = resolved?.RegistryLocation ?? DefaultLocation(config, change.Section);
            var inIni = src.Ini?.GetString(change.Section, change.Name) is not null;

            if (change.Value is null)
            {
                foreach (var name in legacy is null ? new[] { change.Name } : [change.Name, legacy])
                {
                    if (src.Ini?.GetString(change.Section, name) is not null) 
                        ops.Add(new RegWriteOp(RegLocation.Ini, change.Section, name, null));
                    foreach (var loc in new[] { RegLocation.User, RegLocation.VirtualStore, RegLocation.Machine })
                        if (Branch(src, loc).Get(change.Section, name) is not null)
                            ops.Add(new RegWriteOp(loc, change.Section, name, null));
                }

                continue;
            }

            var raw = RegValues.ToRaw(change.Value, change.Type);
            if (change.Target == RegWriteTarget.Ini)
            {
                ops.Add(new RegWriteOp(RegLocation.Ini, change.Section, change.Name, raw));
                continue;
            }

            if (section?.FromIni == true) notRead.Add(change);
            ops.Add(new RegWriteOp(location, change.Section, change.Name, raw));
            if (location == RegLocation.Machine && src.VirtualStore.Get(change.Section, change.Name) is not null)
                ops.Add(new RegWriteOp(RegLocation.VirtualStore, change.Section, change.Name, null));
            if (legacy is not null)
                foreach (var loc in location == RegLocation.Machine ? new[] { RegLocation.VirtualStore, RegLocation.Machine } : [location])
                    if (Branch(src, loc).Get(change.Section, legacy) is not null)
                        ops.Add(new RegWriteOp(loc, change.Section, legacy, null));
            if (inIni)
            {
                if (removeIniOverride) ops.Add(new RegWriteOp(RegLocation.Ini, change.Section, change.Name, null));
                else iniStill.Add(change);
            }
        }

        return new RegWritePlan(ops, iniStill, notRead);
    }

    /// <summary>
    /// Plan, ktory zmaze celu sekciu (napr. linku Driver3) zo vsetkych miest, kde lezi: HKCU, VirtualStore, HKLM
    /// aj subor .INI.
    /// </summary>
    public static RegWritePlan PlanRemoveSection(ResolvedConfig config, string section)
    {
        var src = config.Source;
        var ops = new List<RegWriteOp>();
        if (src.Ini?.HasSection(section) == true) ops.Add(RegWriteOp.DeleteSection(RegLocation.Ini, section));
        foreach (var loc in new[] { RegLocation.User, RegLocation.VirtualStore, RegLocation.Machine })
            if (Branch(src, loc).HasSection(section))
                ops.Add(RegWriteOp.DeleteSection(loc, section));
        return new RegWritePlan(ops, [], []);
    }

    private static RegLocation DefaultLocation(ResolvedConfig config, string section)
    {
        var def = RegCatalog.FindSection(section);
        return def?.Hive == RegHive.User && config.UserBranchActive ? RegLocation.User : RegLocation.Machine;
    }

    private static RegBranch Branch(InissConfigSource src, RegLocation loc) => loc switch
    {
        RegLocation.User => src.User,
        RegLocation.VirtualStore => src.VirtualStore,
        _ => src.Machine
    };
}
