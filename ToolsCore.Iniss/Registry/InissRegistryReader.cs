using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Citanie nastaveni INISSu z registra Windows a zo suboru .INI: vetvy pod CHAPS (HKCU, HKLM v 32-bitovom pohlade,
/// VirtualStore), .INI vedla exe a verzia exe. Zapis a nazvy farieb z jazykovej kniznice ma ToolsCore (InissRegistry).
/// Register sa da citat len vo Windows - inde sa nastavenie sklada len zo suboru .INI.
/// </summary>
public static class InissRegistryReader
{
    /// <summary>Cesta konfiguracii INISSu v HKLM.</summary>
    public const string MachineChaps = @"SOFTWARE\CHAPS";

    /// <summary>Cesta konfiguracii INISSu v HKCU.</summary>
    public const string UserChaps = @"Software\CHAPS";

    /// <summary>Cesta kopie HKLM, ktoru Windows vytvori pri zapise bez prav spravcu (HKCU).</summary>
    public static string VirtualStoreChaps => Environment.Is64BitOperatingSystem
        ? @"Software\Classes\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\CHAPS"
        : @"Software\Classes\VirtualStore\MACHINE\SOFTWARE\CHAPS";

    /// <summary>Nazvy vsetkych konfiguracii pod CHAPS (HKLM, HKCU aj VirtualStore), zoradene.</summary>
    [SupportedOSPlatform("windows")]
    public static IReadOnlyList<string> AppNames()
    {
        var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
        using (var key = hklm.OpenSubKey(MachineChaps))
            names.UnionWith(key?.GetSubKeyNames() ?? []);
        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(UserChaps)) names.UnionWith(key?.GetSubKeyNames() ?? []);
        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(VirtualStoreChaps)) names.UnionWith(key?.GetSubKeyNames() ?? []);
        return names.ToList();
    }

    /// <summary>Meno vetvy pre exe INISSu: hodnota /Reg: z argumentov, inak meno suboru bez pripony.</summary>
    public static string AppNameFor(string exePath, string? regArgument) =>
        string.IsNullOrWhiteSpace(regArgument) ? Path.GetFileNameWithoutExtension(exePath) : regArgument;

    /// <summary>Subor .INI vedla exe (rovnake meno, pripona .INI).</summary>
    public static string IniPathFor(string exePath) => Path.ChangeExtension(exePath, ".INI");

    /// <summary>Cesta vetvy konfiguracie v danom mieste registra.</summary>
    public static string BranchPath(RegLocation location, string appName) => location switch
    {
        RegLocation.Machine => $@"{MachineChaps}\{appName}",
        RegLocation.VirtualStore => $@"{VirtualStoreChaps}\{appName}",
        _ => $@"{UserChaps}\{appName}"
    };

    /// <summary>Nacita vetvu konfiguracie z jedneho miesta registra.</summary>
    [SupportedOSPlatform("windows")]
    public static RegBranch Load(RegLocation location, string appName)
    {
        using var root = location == RegLocation.Machine
            ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            : RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        using var app = root.OpenSubKey(BranchPath(location, appName));
        if (app is null) return RegBranch.Missing;
        var branch = new RegBranch();
        foreach (var section in app.GetSubKeyNames())
        {
            using var key = app.OpenSubKey(section);
            if (key is null) continue;
            branch.AddSection(section);
            foreach (var name in key.GetValueNames())
                branch.Set(section, name, Read(key, name));
        }

        return branch;
    }

    /// <summary>
    /// Nacita vrstvy konfiguracie: tri miesta registra, .INI vedla exe a verziu exe (bez nazvov farieb).
    /// </summary>
    /// <param name="appName">nazov vetvy pod CHAPS</param>
    /// <param name="exePath">exe INISSu (pre .INI a verziu) alebo null</param>
    /// <param name="mode">ako sa INISS spusta</param>
    /// <param name="tables">fyzicke tabule podla indexu v sekcii Tables</param>
    [SupportedOSPlatform("windows")]
    public static InissConfigSource LoadSource(string appName, string? exePath, InissRunMode mode, IReadOnlyDictionary<int, RegTableInfo>? tables = null) => new()
    {
        AppName = appName,
        RunMode = mode,
        User = Load(RegLocation.User, appName),
        Machine = Load(RegLocation.Machine, appName),
        VirtualStore = Load(RegLocation.VirtualStore, appName),
        Ini = exePath is null ? null : InissIniFile.Load(IniPathFor(exePath)),
        Version = exePath is null ? null : ExeVersion(exePath),
        Tables = tables
    };

    /// <summary>Ci je exe program INISS (popis alebo nazov produktu v informaciach o subore je INISS).</summary>
    public static bool IsInissExe(string exePath)
    {
        if (!File.Exists(exePath)) return false;
        var info = FileVersionInfo.GetVersionInfo(exePath);
        return string.Equals(info.FileDescription?.Trim(), "INISS", StringComparison.OrdinalIgnoreCase)
               || string.Equals(info.ProductName?.Trim(), "INISS", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Verzia INISSu z informacii o subore exe (3.00, 3.10, 3.34.6, 3.39).</summary>
    public static RegVersion? ExeVersion(string exePath)
    {
        if (!File.Exists(exePath)) return null;
        var info = FileVersionInfo.GetVersionInfo(exePath);
        return RegVersion.Parse(info.FileVersion) ?? (info.FileMajorPart > 0 ? new RegVersion(info.FileMajorPart, info.FileMinorPart) : null);
    }

    [SupportedOSPlatform("windows")]
    private static RegRawValue Read(RegistryKey key, string name)
    {
        var kind = key.GetValueKind(name);
        var value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        return kind switch
        {
            RegistryValueKind.DWord when value is int n => RegRawValue.Dword(n),
            RegistryValueKind.String when value is string s => RegRawValue.String(s),
            RegistryValueKind.Binary when value is byte[] b => RegRawValue.Binary(b),
            _ => RegRawValue.Other(KindName(kind), value is byte[] bytes ? Convert.ToHexString(bytes) : Convert.ToString(value, CultureInfo.InvariantCulture))
        };
    }

    [SupportedOSPlatform("windows")]
    private static string KindName(RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.ExpandString => "REG_EXPAND_SZ",
        RegistryValueKind.MultiString => "REG_MULTI_SZ",
        RegistryValueKind.QWord => "REG_QWORD",
        RegistryValueKind.None => "REG_NONE",
        _ => "REG_" + kind.ToString().ToUpperInvariant()
    };
}
