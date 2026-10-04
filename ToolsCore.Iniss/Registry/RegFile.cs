using System.Globalization;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Vetva konfiguracie INISSu nacitana zo suboru <c>.reg</c>.
/// </summary>
/// <param name="Location">miesto registra, pod ktorym v subore je</param>
/// <param name="AppName">nazov aplikacie pod CHAPS</param>
/// <param name="Branch">sekcie a hodnoty</param>
public sealed record RegFileBranch(RegLocation Location, string AppName, RegBranch Branch);

/// <summary>
/// Subor <c>.reg</c> (Windows Registry Editor Version 5.00) - na import zmien do HKLM so zvysenymi pravami
/// (<c>reg import subor /reg:32</c>), na export a zalohu konfiguracie. Cesty HKLM su v 32-bitovom pohlade registra,
/// preto bez WOW6432Node - import s <c>/reg:32</c> ich presmeruje sam.
/// </summary>
public sealed class RegFile
{
    private readonly List<(string Key, List<string> Lines, bool DeleteKey)> _keys = [];

    /// <summary>Koren HKLM s konfiguraciami INISSu (32-bitovy pohlad).</summary>
    public const string MachineRoot = @"HKEY_LOCAL_MACHINE\SOFTWARE\CHAPS";

    /// <summary>Koren HKCU s konfiguraciami INISSu.</summary>
    public const string UserRoot = @"HKEY_CURRENT_USER\Software\CHAPS";

    /// <summary>Koren kopii HKLM pre pouzivatela (VirtualStore, 64-bitove Windows).</summary>
    public const string VirtualStoreRoot = @"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\CHAPS";

    /// <summary>Koren pre miesto registra.</summary>
    public static string Root(RegLocation location) => location switch
    {
        RegLocation.User => UserRoot,
        RegLocation.VirtualStore => VirtualStoreRoot,
        RegLocation.Machine => MachineRoot,
        _ => throw new ArgumentOutOfRangeException(nameof(location), location, null)
    };

    /// <summary>Cesta k sekcii aplikacie pod korenom (prazdna sekcia = kluc aplikacie).</summary>
    public static string SectionPath(string root, string appName, string section) =>
        section.Length == 0 ? $@"{root}\{appName}" : $@"{root}\{appName}\{section}";

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

    /// <summary>Operacie planu na danom mieste registra ako subor .reg.</summary>
    public static RegFile FromPlan(RegWritePlan plan, string appName, RegLocation location)
    {
        var root = Root(location);
        var file = new RegFile();
        foreach (var op in plan.Ops.Where(o => o.Location == location))
        {
            var path = SectionPath(root, appName, op.Section);
            if (op.IsKeyCreate) file.AddKey(path);
            else if (op.IsSectionDelete) file.DeleteKey(path);
            else if (op.Value is null) file.Delete(path, op.Name);
            else file.Set(path, op.Name, op.Value);
        }

        return file;
    }

    /// <summary>Cela vetva ako subor .reg (export).</summary>
    public static RegFile FromBranch(RegBranch branch, string root, string appName)
    {
        var file = new RegFile();
        file.AddBranch(branch, root, appName);
        return file;
    }

    /// <summary>Prida celu vetvu (kluc aplikacie, sekcie a hodnoty; hodnoty typu, ktory INISS necita, vynecha).</summary>
    public void AddBranch(RegBranch branch, string root, string appName)
    {
        if (!branch.Exists) return;
        AddKey(SectionPath(root, appName, ""));
        foreach (var section in branch.SectionNames.Order(StringComparer.OrdinalIgnoreCase))
        {
            var path = SectionPath(root, appName, section);
            AddKey(path);
            foreach (var (name, value) in branch.Values(section).OrderBy(v => v.Key, StringComparer.OrdinalIgnoreCase))
                if (value.Kind != RegRawKind.Other)
                    Set(path, name, value);
        }
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

    /// <summary>
    /// Nacita subor .reg (UTF-16 s BOM z regeditu verzie 5, alebo REGEDIT4 v kodovani Windows-1250).
    /// </summary>
    public static IReadOnlyList<RegFileBranch> Load(string path)
    {
        var bytes = File.ReadAllBytes(path);
        string text;
        if (bytes is [0xFF, 0xFE, ..]) text = Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
        else if (bytes is [0xEF, 0xBB, 0xBF, ..]) text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        else text = Encodings.Win1250.GetString(bytes);
        return Parse(text);
    }

    /// <summary>
    /// Vetvy konfiguracii INISSu v texte suboru .reg - kluce pod <c>CHAPS</c> v HKLM (aj s WOW6432Node), HKCU
    /// a VirtualStore. Ine kluce, mazanie klucov a hodnot a predvolene hodnoty (<c>@</c>) preskoci.
    /// </summary>
    /// <exception cref="FormatException">Text nie je subor .reg.</exception>
    public static IReadOnlyList<RegFileBranch> Parse(string text)
    {
        var lines = JoinContinuations(text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'));
        var first = lines.FirstOrDefault(l => l.Trim().Length > 0)?.Trim() ?? "";
        if (!first.StartsWith("Windows Registry Editor", StringComparison.OrdinalIgnoreCase) && !first.Equals("REGEDIT4", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Nie je subor .reg.");

        var branches = new List<RegFileBranch>();
        RegBranch? current = null;
        string? section = null;
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == ';') continue;
            if (line[0] == '[')
            {
                current = null;
                section = null;
                if (line.Length < 2 || line[1] == '-' || !line.EndsWith(']')) continue;
                if (KeyOf(line[1..^1]) is not { } key) continue;
                var existing = branches.Find(b => b.Location == key.Location && string.Equals(b.AppName, key.App, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    existing = new RegFileBranch(key.Location, key.App, new RegBranch());
                    branches.Add(existing);
                }

                current = existing.Branch;
                section = key.Section;
                if (section is not null) current.AddSection(section);
                continue;
            }

            if (current is null || section is null || line[0] != '"') continue;
            if (ParseValue(line) is { } value) current.Set(section, value.Name, value.Value);
        }

        return branches;
    }

    private static List<string> JoinContinuations(string[] lines)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var trimmed = line.TrimEnd();
            if (sb.Length > 0) trimmed = trimmed.TrimStart();
            // dlhy hex: zapis pokracuje na dalsom riadku (riadok konci spatnou lomkou; text v uvodzovkach konci uvodzovkou)
            if (trimmed.EndsWith('\\'))
            {
                sb.Append(trimmed[..^1]);
                continue;
            }

            sb.Append(trimmed);
            result.Add(sb.ToString());
            sb.Clear();
        }

        if (sb.Length > 0) result.Add(sb.ToString());
        return result;
    }

    private static readonly (string Prefix, RegLocation Location)[] Roots =
    [
        (@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\CHAPS\", RegLocation.Machine),
        (@"HKEY_LOCAL_MACHINE\SOFTWARE\CHAPS\", RegLocation.Machine),
        (@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\CHAPS\", RegLocation.VirtualStore),
        (@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\CHAPS\", RegLocation.VirtualStore),
        (@"HKEY_CURRENT_USER\Software\CHAPS\", RegLocation.User)
    ];

    private static (RegLocation Location, string App, string? Section)? KeyOf(string path)
    {
        path = path.Replace("HKLM\\", "HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase)
            .Replace("HKCU\\", "HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase);
        foreach (var (prefix, location) in Roots)
        {
            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            var parts = path[prefix.Length..].Split('\\');
            if (parts[0].Length == 0 || parts.Length > 2) return null;
            return (location, parts[0], parts.Length == 2 && parts[1].Length > 0 ? parts[1] : null);
        }

        return null;
    }

    private static (string Name, RegRawValue Value)? ParseValue(string line)
    {
        var end = EndOfQuoted(line, 0);
        if (end < 0 || end + 1 >= line.Length || line[end + 1] != '=') return null;
        var name = Unescape(line[1..end]);
        var data = line[(end + 2)..].Trim();
        if (data == "-") return null;
        if (data.StartsWith('"'))
        {
            var close = EndOfQuoted(data, 0);
            return close < 0 ? null : (name, RegRawValue.String(Unescape(data[1..close])));
        }

        if (data.StartsWith("dword:", StringComparison.OrdinalIgnoreCase))
            return uint.TryParse(data.AsSpan(6), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var number)
                ? (name, RegRawValue.Dword(unchecked((int)number)))
                : null;
        if (data.StartsWith("hex:", StringComparison.OrdinalIgnoreCase))
            return HexBytes(data[4..]) is { } bytes ? (name, RegRawValue.Binary(bytes)) : null;
        if (data.StartsWith("hex(", StringComparison.OrdinalIgnoreCase))
        {
            var kind = data[4..data.IndexOf(')', StringComparison.Ordinal)];
            return (name, RegRawValue.Other(kind switch
            {
                "2" => "REG_EXPAND_SZ",
                "7" => "REG_MULTI_SZ",
                "b" or "B" => "REG_QWORD",
                "0" => "REG_NONE",
                _ => "hex(" + kind + ")"
            }));
        }

        return null;
    }

    private static byte[]? HexBytes(string text)
    {
        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var bytes = new byte[parts.Length];
        for (var i = 0; i < parts.Length; i++)
            if (!byte.TryParse(parts[i], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out bytes[i]))
                return null;
        return bytes;
    }

    /// <summary>Index koncovej uvodzovky textu v uvodzovkach zacinajuceho na <paramref name="start" />.</summary>
    private static int EndOfQuoted(string text, int start)
    {
        for (var i = start + 1; i < text.Length; i++)
        {
            if (text[i] == '\\') i++;
            else if (text[i] == '"') return i;
        }

        return -1;
    }

    private static string Unescape(string text)
    {
        var sb = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\' && i + 1 < text.Length) i++;
            sb.Append(text[i]);
        }

        return sb.ToString();
    }

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
