using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Subor <c>.reg</c> (Windows Registry Editor Version 5.00) - na import zmien do HKLM so zvysenymi pravami
/// (<c>reg import subor /reg:32</c>) a na export konfiguracie. Cesty su v 32-bitovom pohlade registra, preto bez
/// WOW6432Node - import s <c>/reg:32</c> ich presmeruje sam.
/// </summary>
public sealed class RegFile
{
    private readonly List<(string Key, List<string> Lines, bool DeleteKey)> _keys = [];

    /// <summary>Koren HKLM s konfiguraciami INISSu (32-bitovy pohlad).</summary>
    public const string MachineRoot = @"HKEY_LOCAL_MACHINE\SOFTWARE\CHAPS";

    /// <summary>Koren HKCU s konfiguraciami INISSu.</summary>
    public const string UserRoot = @"HKEY_CURRENT_USER\Software\CHAPS";

    /// <summary>Cesta k sekcii aplikacie pod korenom.</summary>
    public static string SectionPath(string root, string appName, string section) => $@"{root}\{appName}\{section}";

    /// <summary>Nastavi hodnotu.</summary>
    public void Set(string keyPath, string name, RegRawValue value) => Lines(keyPath).Add($"{Quote(name)}={Format(value)}");

    /// <summary>Zmaze hodnotu.</summary>
    public void Delete(string keyPath, string name) => Lines(keyPath).Add($"{Quote(name)}=-");

    /// <summary>Zalozi kluc (aj bez hodnot).</summary>
    public void AddKey(string keyPath) => Lines(keyPath);

    /// <summary>Zmaze cely kluc aj s podklucmi.</summary>
    public void DeleteKey(string keyPath) => _keys.Add((keyPath, [], true));

    /// <summary>Ci subor nic neobsahuje.</summary>
    public bool IsEmpty => _keys.Count == 0;

    /// <summary>Operacie planu na danom mieste (Machine alebo User) ako subor .reg.</summary>
    public static RegFile FromPlan(RegWritePlan plan, string appName, RegLocation location)
    {
        var root = location == RegLocation.User ? UserRoot : MachineRoot;
        var file = new RegFile();
        foreach (var op in plan.Ops.Where(o => o.Location == location))
        {
            var path = SectionPath(root, appName, op.Section);
            if (op.IsSectionDelete) file.DeleteKey(path);
            else if (op.Value is null) file.Delete(path, op.Name);
            else file.Set(path, op.Name, op.Value);
        }

        return file;
    }

    /// <summary>Cela vetva ako subor .reg (export).</summary>
    public static RegFile FromBranch(RegBranch branch, string root, string appName)
    {
        var file = new RegFile();
        foreach (var section in branch.SectionNames.Order(StringComparer.OrdinalIgnoreCase))
        {
            var path = SectionPath(root, appName, section);
            file.AddKey(path);
            foreach (var (name, value) in branch.Values(section).OrderBy(v => v.Key, StringComparer.OrdinalIgnoreCase))
                if (value.Kind != RegRawKind.Other)
                    file.Set(path, name, value);
        }

        return file;
    }

    /// <summary>Text suboru (CRLF).</summary>
    public override string ToString()
    {
        var sb = new StringBuilder("Windows Registry Editor Version 5.00\r\n");
        foreach (var (key, lines, delete) in _keys)
        {
            sb.Append("\r\n[").Append(delete ? "-" : "").Append(key).Append("]\r\n");
            foreach (var line in lines) sb.Append(line).Append("\r\n");
        }

        return sb.ToString();
    }

    /// <summary>Zapise subor v UTF-16 LE s BOM, ako ho zapisuje regedit.</summary>
    public void Save(string path) => File.WriteAllText(path, ToString(), Encoding.Unicode);

    private List<string> Lines(string keyPath)
    {
        foreach (var k in _keys)
            if (!k.DeleteKey && string.Equals(k.Key, keyPath, StringComparison.OrdinalIgnoreCase))
                return k.Lines;
        var lines = new List<string>();
        _keys.Add((keyPath, lines, false));
        return lines;
    }

    private static string Quote(string text) => "\"" + text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    private static string Format(RegRawValue value) => value.Kind switch
    {
        RegRawKind.Dword => "dword:" + unchecked((uint)value.Number).ToString("x8", CultureInfo.InvariantCulture),
        RegRawKind.String => Quote(value.Text ?? ""),
        RegRawKind.Binary => "hex:" + string.Join(",", (value.Bytes ?? []).Select(b => b.ToString("x2", CultureInfo.InvariantCulture))),
        _ => throw new NotSupportedException(value.OtherKind)
    };
}
