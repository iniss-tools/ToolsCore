using System.Globalization;
using System.Text.RegularExpressions;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Vyhodnoti konfiguraciu INISSu tak, ako ju INISS 3.39 nacita pri starte: pre kazde nastavenie ucinnu hodnotu
/// a jej zdroj. Poradie: subor .INI -> register (HKCU, ak vetva existuje a sekcia je per-user; inak HKLM, pri
/// spusteni bez prav spravcu s prednostou VirtualStore) -> predvolena hodnota (pri Driver podla triedy).
/// Cislovana sekcia (Driver*, Volume*) pritomna v .INI sa cita len zo suboru.
/// </summary>
public static partial class RegResolver
{
    /// <summary>Vyhodnoti konfiguraciu.</summary>
    public static ResolvedConfig Resolve(InissConfigSource source)
    {
        var userActive = source.User.Exists;
        var sections = new List<ResolvedSection>();
        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var def in RegCatalog.Sections)
        {
            foreach (var name in def.InstanceNames())
            {
                var fromIni = def.IsNumbered && source.Ini?.HasSection(name) == true;
                if (def.IsNumbered && !fromIni && !RegistryHasSection(source, def.Hive, userActive, name)) continue;
                known.Add(name);
                sections.Add(ResolveSection(source, def, name, fromIni, userActive));
            }
        }

        var unknown = new List<RegDiagnostic>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in source.User.SectionNames.Concat(source.Machine.SectionNames).Concat(source.VirtualStore.SectionNames)
                     .Concat(source.Ini?.SectionNames ?? []))
        {
            if (known.Contains(name) || RegCatalog.FindSection(name) is not null || !seen.Add(name)) continue;
            unknown.Add(Diag(RegDiagnosticCode.UnknownSection, RegSeverity.Info, name, null, Resources.Reg_UnknownSection, name));
        }

        return new ResolvedConfig(source, userActive, sections, unknown);
    }

    private static bool RegistryHasSection(InissConfigSource src, RegHive hive, bool userActive, string section) =>
        hive == RegHive.User && userActive
            ? src.User.HasSection(section)
            : src.Machine.HasSection(section) || src.RunMode == InissRunMode.Normal && src.VirtualStore.HasSection(section);

    private static ResolvedSection ResolveSection(InissConfigSource src, RegSection def, string name, bool fromIni, bool userActive)
    {
        var result = new ResolvedSection(def, name, fromIni);
        var settings = new List<ResolvedSetting>();
        var consumed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sectionDiags = new List<RegDiagnostic>();
        if (fromIni)
            sectionDiags.Add(Diag(RegDiagnosticCode.IniSectionReplacesRegistry, RegSeverity.Warning, name, null, Resources.Reg_IniSectionReplacesRegistry, name));

        // Driver: predvolby zavisia od triedy a portu - tie sa citaju ako prve
        IReadOnlyDictionary<string, object>? classDefaults = null;
        if (def.Name == "Driver")
        {
            var cls = ResolveOne(src, def, def.Find("TableClass")!, name, "TableClass", fromIni, userActive, null);
            var port = ResolveOne(src, def, def.Find("TablePort")!, name, "TablePort", fromIni, userActive, null);
            classDefaults = DriverDefaults.For(cls.Value as int? ?? DriverDefaults.DefaultClass, port.Value as string ?? DriverDefaults.DefaultPort);
        }

        var names = AllNames(src, name);
        foreach (var setting in def.Settings)
        {
            switch (setting.Kind)
            {
                case RegNameKind.Fixed:
                    settings.Add(ResolveOne(src, def, setting, name, setting.Name, fromIni, userActive, classDefaults));
                    consumed.Add(setting.Name);
                    if (setting.LegacyName is { } legacy) consumed.Add(legacy);
                    break;
                case RegNameKind.Indexed:
                    foreach (var index in Indices(setting, names, src.Tables?.Keys))
                    {
                        var n = setting.NameFor(index);
                        var table = src.Tables?.GetValueOrDefault(index);
                        // vynuteny jas ma predvolbu podla vyrobcu tabule
                        var tableDefaults = table?.Manufacturer is { } manufacturer && setting.Name == "ForceLight<N>"
                            ? new Dictionary<string, object> { [setting.Name] = DriverClasses.ForceLightDefault(manufacturer) }
                            : null;
                        var resolved = ResolveOne(src, def, setting, name, n, fromIni, userActive, tableDefaults);
                        resolved.Table = table;
                        settings.Add(resolved);
                        consumed.Add(n);
                    }

                    break;
                case RegNameKind.Color:
                    var colorName = src.ColorNames is { Count: RegCatalog.ColorCount } colors ? colors[setting.ColorIndex] : setting.Name;
                    settings.Add(ResolveOne(src, def, setting, name, colorName, fromIni, userActive, null));
                    consumed.Add(colorName);
                    break;
                case RegNameKind.Dynamic:
                    // prvky mixera: kazda hodnota sekcie, ktoru nepokryli ostatne nastavenia
                    foreach (var n in names.Where(n => !IsCovered(def, n)))
                    {
                        settings.Add(ResolveOne(src, def, setting, name, n, fromIni, userActive, null));
                        consumed.Add(n);
                    }

                    break;
            }
        }

        result.Settings = settings;
        var extra = new List<RegLayerValue>();
        foreach (var n in names.Where(n => !consumed.Contains(n)))
        {
            var layers = Layers(src, name, n);
            extra.AddRange(layers);
            if (def.Name == "Colors")
                sectionDiags.Add(Diag(RegDiagnosticCode.ColorOtherLanguage, RegSeverity.Info, name, n, Resources.Reg_ColorOtherLanguage, n));
            else if (RegCatalog.IsLeftover(def.Name, n))
                sectionDiags.Add(Diag(RegDiagnosticCode.Leftover, RegSeverity.Info, name, n, Resources.Reg_Leftover, n));
            else if (def.Name != "ColumnWidth")
                sectionDiags.Add(Diag(RegDiagnosticCode.Unknown, RegSeverity.Warning, name, n, Resources.Reg_Unknown, n));
        }

        result.Extra = extra;
        result.Diagnostics = sectionDiags;
        return result;
    }

    private static bool IsCovered(RegSection def, string name) =>
        def.Settings.Any(s => s.Kind == RegNameKind.Fixed && (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)
                                                             || string.Equals(s.LegacyName, name, StringComparison.OrdinalIgnoreCase)))
        || RegCatalog.IsLeftover(def.Name, name);

    private static SortedSet<int> Indices(RegSetting setting, IEnumerable<string> names, IEnumerable<int>? wanted)
    {
        var prefix = setting.Name[..setting.Name.IndexOf("<N>", StringComparison.Ordinal)];
        var found = new SortedSet<int>(wanted ?? []);
        foreach (var n in names)
            if (n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && int.TryParse(n.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var i))
                found.Add(i);
        return found;
    }

    /// <summary>Vsetky nazvy hodnot sekcie vo vsetkych vrstvach.</summary>
    private static List<string> AllNames(InissConfigSource src, string section)
    {
        var names = new List<string>();
        void Add(IEnumerable<string> list)
        {
            foreach (var n in list)
                if (!names.Contains(n, StringComparer.OrdinalIgnoreCase))
                    names.Add(n);
        }

        if (src.Ini is { } ini) Add(ini.Values(section).Select(v => v.Key));
        Add(src.User.Values(section).Keys);
        Add(src.VirtualStore.Values(section).Keys);
        Add(src.Machine.Values(section).Keys);
        return names;
    }

    private static ResolvedSetting ResolveOne(InissConfigSource src, RegSection def, RegSetting setting, string section, string name,
        bool fromIni, bool userActive, IReadOnlyDictionary<string, object>? classDefaults)
    {
        var r = new ResolvedSetting(setting, section, name);
        var hive = setting.HiveOverride ?? def.Hive;
        var readUser = hive == RegHive.User && userActive;
        r.RegistryLocation = readUser ? RegLocation.User : RegLocation.Machine;
        var diags = new List<RegDiagnostic>();

        // predvolba
        RegSource defaultSource;
        if (classDefaults is not null && classDefaults.TryGetValue(setting.Name, out var classDefault))
        {
            r.DefaultValue = classDefault;
            defaultSource = setting.Kind == RegNameKind.Indexed ? RegSource.Default : RegSource.ClassDefault;
        }
        else if (DynamicDefault(def, setting, section) is { } dyn)
        {
            r.DefaultValue = dyn;
            defaultSource = RegSource.Default;
        }
        else
        {
            r.DefaultValue = setting.Default.IsDynamic ? null : setting.Default.Value;
            defaultSource = setting.Default.IsDynamic ? RegSource.UnknownDefault : RegSource.Default;
        }

        // vrstvy v poradi priority (aj stary nazov bez jednotky)
        var iniValue = IniValue(src.Ini, setting, section, name, out var iniName, diags);
        var order = readUser
            ? new[] { RegLocation.User }
            : src.RunMode == InissRunMode.Normal ? [RegLocation.VirtualStore, RegLocation.Machine] : [RegLocation.Machine];

        object? value = null;
        RegSource? source = null;
        var layers = new List<RegLayerValue>();
        if (iniValue is not null)
        {
            value = iniValue;
            source = RegSource.Ini;
        }

        RegLocation? decisive = null;
        string? decisiveName = null;
        if (source is null && !fromIni)
        {
            foreach (var loc in order)
                if (Branch(src, loc).Get(section, name) is not null)
                {
                    decisive = loc;
                    decisiveName = name;
                    break;
                }

            if (decisive is null && setting.LegacyName is { } legacy)
                foreach (var loc in order)
                    if (Branch(src, loc).Get(section, legacy) is not null)
                    {
                        decisive = loc;
                        decisiveName = legacy;
                        break;
                    }

            if (decisive is { } d)
            {
                var raw = Branch(src, d).Get(section, decisiveName!)!;
                if (Accept(setting, raw, r.DefaultValue, out var accepted))
                {
                    value = accepted;
                    source = d switch
                    {
                        RegLocation.User => RegSource.User,
                        RegLocation.VirtualStore => RegSource.VirtualStore,
                        _ => RegSource.Machine
                    };
                    if (decisiveName != name)
                        diags.Add(Diag(RegDiagnosticCode.LegacyName, RegSeverity.Info, section, name, Resources.Reg_LegacyName, name, decisiveName!));
                }
                else if (raw.Kind == RegRawKind.String && setting.Type == RegValueType.String)
                {
                    diags.Add(Diag(RegDiagnosticCode.EmptyText, RegSeverity.Info, section, name, Resources.Reg_EmptyText, name, r.DefaultValue));
                }
                else
                {
                    diags.Add(Diag(RegDiagnosticCode.WrongType, RegSeverity.Warning, section, name,
                        setting.WritesBack ? Resources.Reg_WrongTypeOverwritten : Resources.Reg_WrongType, name, KindName(raw)));
                }
            }
        }

        // vrstvy na zobrazenie
        if (src.Ini is { } ini && iniName is not null && ini.GetString(section, iniName) is { } iniText)
            layers.Add(new RegLayerValue(RegLocation.Ini, iniName, RegRawValue.String(iniText), source == RegSource.Ini ? RegLayerState.Used : RegLayerState.NotRead));
        foreach (var loc in new[] { RegLocation.User, RegLocation.VirtualStore, RegLocation.Machine })
        {
            foreach (var n in setting.LegacyName is { } lg ? new[] { name, lg } : [name])
            {
                if (Branch(src, loc).Get(section, n) is not { } raw) continue;
                RegLayerState state;
                if (!Array.Exists(order, o => o == loc) || fromIni) state = RegLayerState.NotRead;
                else if (loc == decisive && n == decisiveName) state = source is null ? RegLayerState.WrongType : RegLayerState.Used;
                else state = RegLayerState.Shadowed;
                layers.Add(new RegLayerValue(loc, n, raw, state));
                LayerDiagnostics(src, setting, section, name, n, loc, raw, state, source, source is null ? r.DefaultValue : value, readUser, diags);
            }
        }

        var available = setting.IsAvailableIn(src.Version);
        if (!available)
        {
            if (layers.Count > 0)
                diags.Add(setting.WriteOnly
                    ? Diag(RegDiagnosticCode.WriteOnly, RegSeverity.Info, section, name, Resources.Reg_WriteOnly, name)
                    : Diag(RegDiagnosticCode.NotInVersion, RegSeverity.Info, section, name, Resources.Reg_NotInVersion, name, src.Version?.ToString() ?? "?"));
            r.Value = null;
            r.Source = RegSource.NotRead;
        }
        else
        {
            r.Value = source is null ? r.DefaultValue : value;
            r.Source = source ?? defaultSource;
        }

        if (setting.Section.Name == "Driver" && setting.Name == "TablePort" && r.Value is string port && DriverDefaults.IsNetworkPortWithoutLine(port))
            diags.Add(Diag(RegDiagnosticCode.NetworkPortWithoutLine, RegSeverity.Error, section, name, Resources.Reg_NetworkPortWithoutLine, port));

        r.Layers = layers;
        r.Diagnostics = diags;
        return r;
    }

    private static void LayerDiagnostics(InissConfigSource src, RegSetting setting, string section, string name, string layerName, RegLocation loc,
        RegRawValue raw, RegLayerState state, RegSource? source, object? effective, bool readUser, List<RegDiagnostic> diags)
    {
        if (layerName != name && state is RegLayerState.Shadowed)
        {
            diags.Add(Diag(RegDiagnosticCode.LegacyGhost, RegSeverity.Warning, section, name, Resources.Reg_LegacyGhost, name, layerName));
            return;
        }

        // prebita hodnota rovnaka ako ucinna nicomu nevadi - upozorni sa len na rozdiel
        if (Accept(setting, raw, null, out var layerValue) && RegValues.AreEqual(layerValue, effective, setting.Type)) return;

        switch (state)
        {
            case RegLayerState.Shadowed when source == RegSource.Ini:
            case RegLayerState.NotRead when source == RegSource.Ini:
                diags.Add(Diag(RegDiagnosticCode.ShadowedByIni, RegSeverity.Info, section, name, Resources.Reg_ShadowedByIni, layerName, Where(loc)));
                break;
            case RegLayerState.Shadowed when loc == RegLocation.Machine && source == RegSource.VirtualStore:
                // hodnoty, ktore si INISS prepisuje sam (rozlozenie okna), sa v rezimoch prirodzene rozchadzaju
                diags.Add(Diag(RegDiagnosticCode.ShadowedByVirtualStore, setting.Write is RegWriteMode.AutoAndApp or RegWriteMode.App ? RegSeverity.Info : RegSeverity.Warning,
                    section, name, Resources.Reg_ShadowedByVirtualStore, layerName));
                break;
            case RegLayerState.NotRead when loc == RegLocation.Machine && readUser:
                diags.Add(Diag(RegDiagnosticCode.MachineIgnored, RegSeverity.Warning, section, name, Resources.Reg_MachineIgnored, layerName));
                break;
            case RegLayerState.NotRead when loc == RegLocation.VirtualStore && !readUser && src.RunMode == InissRunMode.Elevated:
                diags.Add(Diag(RegDiagnosticCode.VirtualStoreIgnored, RegSeverity.Info, section, name, Resources.Reg_VirtualStoreIgnored, layerName));
                break;
        }
    }

    private static string Where(RegLocation loc) => loc switch
    {
        RegLocation.User => "HKEY_CURRENT_USER",
        RegLocation.VirtualStore => "VirtualStore",
        RegLocation.Machine => "HKEY_LOCAL_MACHINE",
        _ => ".INI"
    };

    /// <summary>Predvolby, ktore zavisia od nazvu sekcie (Volume: Mikrofon je 1 len pre sekciu Volume).</summary>
    private static int? DynamicDefault(RegSection def, RegSetting setting, string section) =>
        def.Name == "Volume" && setting.Name == "Mikrofon" ? string.Equals(section, "Volume", StringComparison.OrdinalIgnoreCase) ? 1 : 0 : (int?)null;

    /// <summary>Hodnota zo suboru .INI v type nastavenia; null = nie je (alebo je neplatna).</summary>
    private static object? IniValue(InissIniFile? ini, RegSetting setting, string section, string name, out string? iniName, List<RegDiagnostic> diags)
    {
        iniName = null;
        if (ini is null) return null;
        foreach (var n in setting.LegacyName is { } lg ? new[] { name, lg } : [name])
        {
            var text = ini.GetString(section, n);
            if (text is null) continue;
            iniName = n;
            switch (setting.Type)
            {
                case RegValueType.String:
                    return text;
                case RegValueType.Binary:
                    return text.Length % 2 == 0 && HexText().IsMatch(text) ? Convert.FromHexString(text) : null;
                default:
                    if (!DecimalText().IsMatch(text.Trim()))
                        diags.Add(Diag(RegDiagnosticCode.IniInvalidNumber, RegSeverity.Warning, section, name, Resources.Reg_IniInvalidNumber, n, text,
                            InissIniFile.ParseNumber(text)));
                    return ini.GetNumber(section, n);
            }
        }

        return null;
    }

    /// <summary>Uzna hodnotu z registra, ak ma spravny typ (prazdny text so spatnym zapisom a neprazdnou predvolbou nie).</summary>
    private static bool Accept(RegSetting setting, RegRawValue raw, object? defaultValue, out object? value)
    {
        value = null;
        switch (setting.Type)
        {
            case RegValueType.String when raw.Kind == RegRawKind.String:
                if (raw.Text!.Length == 0 && setting.WritesBack && defaultValue is string { Length: > 0 }) return false;
                value = raw.Text;
                return true;
            case RegValueType.Binary when raw.Kind == RegRawKind.Binary:
                value = raw.Bytes;
                return true;
            case RegValueType.Dword or RegValueType.Bool or RegValueType.Color when raw.Kind == RegRawKind.Dword:
                value = raw.Number;
                return true;
            default:
                return false;
        }
    }

    private static string KindName(RegRawValue raw) => raw.Kind switch
    {
        RegRawKind.Dword => "REG_DWORD",
        RegRawKind.String => "REG_SZ",
        RegRawKind.Binary => "REG_BINARY",
        _ => raw.OtherKind ?? "?"
    };

    /// <summary>Hodnota mimo katalogu vo vsetkych vrstvach (bez vyhodnotenia).</summary>
    private static List<RegLayerValue> Layers(InissConfigSource src, string section, string name)
    {
        var list = new List<RegLayerValue>();
        if (src.Ini?.GetString(section, name) is { } t) list.Add(new RegLayerValue(RegLocation.Ini, name, RegRawValue.String(t), RegLayerState.Used));
        foreach (var loc in new[] { RegLocation.User, RegLocation.VirtualStore, RegLocation.Machine })
            if (Branch(src, loc).Get(section, name) is { } raw)
                list.Add(new RegLayerValue(loc, name, raw, RegLayerState.Used));
        return list;
    }

    private static RegBranch Branch(InissConfigSource src, RegLocation loc) => loc switch
    {
        RegLocation.User => src.User,
        RegLocation.VirtualStore => src.VirtualStore,
        _ => src.Machine
    };

    private static RegDiagnostic Diag(RegDiagnosticCode code, RegSeverity severity, string section, string? name, string format, params object?[] args) =>
        new(code, severity, section, name, string.Format(CultureInfo.CurrentCulture, format, args));

    [GeneratedRegex("^[0-9A-Fa-f]*$", RegexOptions.CultureInvariant)]
    private static partial Regex HexText();

    [GeneratedRegex(@"^[+-]?\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex DecimalText();
}
