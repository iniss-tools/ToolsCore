using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;
using ToolsCore.Iniss.Registry;
using Vanara.PInvoke;

namespace ToolsCore.Tools;

/// <summary>Vysledok zapisu nastaveni INISSu.</summary>
public enum RegApplyResult
{
    /// <summary>Vsetko zapisane.</summary>
    Ok,

    /// <summary>Pouzivatel odmietol zvysenie prav - nic sa nezapisalo.</summary>
    Cancelled,

    /// <summary>Zapis do HKLM zlyhal - nic sa nezapisalo.</summary>
    Failed
}

/// <summary>
/// Pristup k nastaveniam INISSu v registri Windows a v subore .INI: nacitanie vetiev (HKCU, HKLM v 32-bitovom pohlade,
/// VirtualStore), udaje z exe (verzia, nazvy farieb z RCIniss.dll) a vykonanie planu zapisu. Zapis do HKLM bez prav
/// spravcu ide cez docasny subor .reg a <c>reg import /reg:32</c> so zvysenymi pravami (jedna vyzva UAC).
/// </summary>
public static class InissRegistry
{
    private const string MachineChaps = @"SOFTWARE\CHAPS";
    private const string UserChaps = @"Software\CHAPS";

    private static string VirtualStoreChaps => Environment.Is64BitOperatingSystem
        ? @"Software\Classes\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\CHAPS"
        : @"Software\Classes\VirtualStore\MACHINE\SOFTWARE\CHAPS";

    /// <summary>Retazce s nazvami farieb v jazykovej kniznici INISSu.</summary>
    private const int ColorStringBase = 10000;

    /// <summary>Nazvy vsetkych konfiguracii pod CHAPS (HKLM, HKCU aj VirtualStore), zoradene.</summary>
    public static IReadOnlyList<string> AppNames()
    {
        var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
        using (var key = hklm.OpenSubKey(MachineChaps))
            names.UnionWith(key?.GetSubKeyNames() ?? []);
        using (var key = Registry.CurrentUser.OpenSubKey(UserChaps)) names.UnionWith(key?.GetSubKeyNames() ?? []);
        using (var key = Registry.CurrentUser.OpenSubKey(VirtualStoreChaps)) names.UnionWith(key?.GetSubKeyNames() ?? []);
        return names.ToList();
    }

    /// <summary>Meno vetvy pre exe INISSu: hodnota /Reg: z argumentov, inak meno suboru bez pripony.</summary>
    public static string AppNameFor(string exePath, string? regArgument) =>
        string.IsNullOrWhiteSpace(regArgument) ? Path.GetFileNameWithoutExtension(exePath) : regArgument;

    /// <summary>Subor .INI vedla exe (rovnake meno, pripona .INI).</summary>
    public static string IniPathFor(string exePath) => Path.ChangeExtension(exePath, ".INI");

    /// <summary>Nacita vetvu konfiguracie z jedneho miesta registra.</summary>
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
    /// Nacita vsetky vrstvy konfiguracie: tri miesta registra, .INI vedla exe, verziu exe a nazvy farieb.
    /// </summary>
    /// <param name="appName">nazov vetvy pod CHAPS</param>
    /// <param name="exePath">exe INISSu (pre .INI, verziu a jazykovu kniznicu) alebo null</param>
    /// <param name="mode">ako sa INISS spusta</param>
    /// <param name="tables">fyzicke tabule podla indexu v sekcii Tables</param>
    public static InissConfigSource LoadSource(string appName, string? exePath, InissRunMode mode, IReadOnlyDictionary<int, RegTableInfo>? tables = null) => new()
    {
        AppName = appName,
        RunMode = mode,
        User = Load(RegLocation.User, appName),
        Machine = Load(RegLocation.Machine, appName),
        VirtualStore = Load(RegLocation.VirtualStore, appName),
        Ini = exePath is null ? null : InissIniFile.Load(IniPathFor(exePath)),
        Version = exePath is null ? null : ExeVersion(exePath),
        ColorNames = exePath is null ? null : ColorNames(Path.GetDirectoryName(exePath)!),
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

    /// <summary>Nazvy 42 farieb z jazykovej kniznice RCIniss.dll vedla exe; null, ak kniznica nie je.</summary>
    public static IReadOnlyList<string>? ColorNames(string exeDir)
    {
        var dll = Path.Combine(exeDir, "RCIniss.dll");
        if (!File.Exists(dll)) return null;
        using var module = Kernel32.LoadLibraryEx(dll, default, Kernel32.LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE | Kernel32.LoadLibraryExFlags.LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        if (module.IsInvalid) return null;
        var names = new List<string>();
        var buffer = new StringBuilder(256);
        for (var i = 0; i < RegCatalog.ColorCount; i++)
        {
            buffer.Clear();
            var length = User32.LoadString(module, ColorStringBase + i, buffer, buffer.Capacity);
            if (length <= 0) return null;
            names.Add(buffer.ToString());
        }

        return names;
    }

    /// <summary>Seriove porty pocitaca (COM1, COM2…) podla HKLM\HARDWARE\DEVICEMAP\SERIALCOMM, zoradene podla cisla.</summary>
    public static IReadOnlyList<string> SerialPorts()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
        if (key is null) return [];
        return key.GetValueNames().Select(n => key.GetValue(n) as string).OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => DriverClasses.LineNumber(p) ?? int.MaxValue).ThenBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Ci tento proces moze zapisovat do HKLM konfiguracie (bez zvysenia prav).</summary>
    public static bool CanWriteMachine(string appName)
    {
        try
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var key = hklm.OpenSubKey($@"{MachineChaps}\{appName}", true) ?? hklm.OpenSubKey(MachineChaps, true);
            return key is not null;
        }
        catch (Exception e) when (e is UnauthorizedAccessException or System.Security.SecurityException)
        {
            return false;
        }
    }

    /// <summary>
    /// Vykona plan zapisu. Najprv HKLM (ak treba, so zvysenymi pravami) - pri odmietnuti alebo chybe sa nic dalsie
    /// nezapise; potom HKCU, VirtualStore a subor .INI.
    /// </summary>
    /// <param name="plan">plan</param>
    /// <param name="appName">nazov vetvy pod CHAPS</param>
    /// <param name="iniPath">subor .INI (povinny, ak plan meni .INI)</param>
    public static RegApplyResult Apply(RegWritePlan plan, string appName, string? iniPath)
    {
        if (plan.WritesMachine)
        {
            if (CanWriteMachine(appName)) ApplyDirect(plan, appName, RegLocation.Machine);
            else
            {
                var result = ImportElevated(RegFile.FromPlan(plan, appName, RegLocation.Machine));
                if (result != RegApplyResult.Ok) return result;
            }
        }

        ApplyDirect(plan, appName, RegLocation.User);
        ApplyDirect(plan, appName, RegLocation.VirtualStore);
        if (plan.WritesIni)
        {
            ArgumentNullException.ThrowIfNull(iniPath);
            var ini = InissIniFile.Load(iniPath) ?? InissIniFile.Empty();
            foreach (var op in plan.Ops.Where(o => o.Location == RegLocation.Ini))
                if (op.IsSectionDelete) ini.RemoveSection(op.Section);
                else if (op.Value is null) ini.Remove(op.Section, op.Name);
                else ini.Set(op.Section, op.Name, op.Value.Kind == RegRawKind.Binary ? Convert.ToHexString(op.Value.Bytes!) : op.Value.ToString());
            ini.Save(iniPath);
        }

        return RegApplyResult.Ok;
    }

    private static void ApplyDirect(RegWritePlan plan, string appName, RegLocation location)
    {
        var ops = plan.Ops.Where(o => o.Location == location).ToList();
        if (ops.Count == 0) return;
        using var root = location == RegLocation.Machine
            ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            : RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        foreach (var op in ops)
        {
            var path = $@"{BranchPath(location, appName)}\{op.Section}";
            if (op.IsSectionDelete)
            {
                root.DeleteSubKeyTree(path, false);
                continue;
            }

            if (op.Value is null)
            {
                using var existing = root.OpenSubKey(path, true);
                existing?.DeleteValue(op.Name, false);
                continue;
            }

            using var key = root.CreateSubKey(path, true);
            switch (op.Value.Kind)
            {
                case RegRawKind.Dword:
                    key.SetValue(op.Name, op.Value.Number, RegistryValueKind.DWord);
                    break;
                case RegRawKind.String:
                    key.SetValue(op.Name, op.Value.Text ?? "", RegistryValueKind.String);
                    break;
                case RegRawKind.Binary:
                    key.SetValue(op.Name, op.Value.Bytes ?? [], RegistryValueKind.Binary);
                    break;
                default:
                    throw new NotSupportedException(op.Value.OtherKind);
            }
        }
    }

    /// <summary>Importuje subor .reg do 32-bitoveho pohladu registra cez reg.exe so zvysenymi pravami.</summary>
    private static RegApplyResult ImportElevated(RegFile file)
    {
        var temp = Path.Combine(Path.GetTempPath(), $"iniss-nastavenia-{Guid.NewGuid():N}.reg");
        file.Save(temp);
        try
        {
            using var process = Process.Start(new ProcessStartInfo("reg.exe", $"import \"{temp}\" /reg:32")
            {
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            });
            if (process is null) return RegApplyResult.Failed;
            process.WaitForExit();
            return process.ExitCode == 0 ? RegApplyResult.Ok : RegApplyResult.Failed;
        }
        catch (Win32Exception e) when (e.NativeErrorCode == ErrorCancelled)
        {
            return RegApplyResult.Cancelled;
        }
        finally
        {
            File.Delete(temp);
        }
    }

    private static string BranchPath(RegLocation location, string appName) => location switch
    {
        RegLocation.Machine => $@"{MachineChaps}\{appName}",
        RegLocation.VirtualStore => $@"{VirtualStoreChaps}\{appName}",
        _ => $@"{UserChaps}\{appName}"
    };

    private static RegRawValue Read(RegistryKey key, string name)
    {
        var kind = key.GetValueKind(name);
        var value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        return kind switch
        {
            RegistryValueKind.DWord when value is int n => RegRawValue.Dword(n),
            RegistryValueKind.String when value is string s => RegRawValue.String(s),
            RegistryValueKind.Binary when value is byte[] b => RegRawValue.Binary(b),
            _ => RegRawValue.Other(KindName(kind), value is byte[] bytes ? Convert.ToHexString(bytes) : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture))
        };
    }

    private static string KindName(RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.ExpandString => "REG_EXPAND_SZ",
        RegistryValueKind.MultiString => "REG_MULTI_SZ",
        RegistryValueKind.QWord => "REG_QWORD",
        RegistryValueKind.None => "REG_NONE",
        _ => "REG_" + kind.ToString().ToUpperInvariant()
    };

    private const int ErrorCancelled = 1223;
}
