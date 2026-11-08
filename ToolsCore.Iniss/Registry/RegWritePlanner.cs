namespace ToolsCore.Iniss.Registry;

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
