using System.Globalization;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Subor <c>.INI</c> vedla programu INISS (<c>INISS - stanica.INI</c>), ktory prebija register. Cita sa ako
/// funkciami Windows GetPrivateProfileInt/String: nazvy sekcii a hodnot bez rozlisenia velkosti pismen, plati
/// prva sekcia a prvy vyskyt nazvu, okrajove medzery sa orezu a hodnota v uvodzovkach sa z nich vyberie.
/// Uprava zachova komentare, poradie aj ostatne riadky; kodovanie CP1250, riadky CRLF.
/// </summary>
public sealed class InissIniFile
{
    /// <summary>Hodnota, ktoru INISS pri cislach berie ako "nie je v subore" (0x8C8C8C8C).</summary>
    public const int MissingNumberSentinel = unchecked((int)0x8C8C8C8C);

    /// <summary>Text, ktory INISS pri retazcoch berie ako "nie je v subore".</summary>
    public const string MissingTextSentinel = "#deflt#";

    private readonly List<string> _lines;

    private InissIniFile(List<string> lines) => _lines = lines;

    /// <summary>Prazdny subor.</summary>
    public static InissIniFile Empty() => new([]);

    /// <summary>Precita obsah (text uz dekodovany).</summary>
    public static InissIniFile Parse(string text)
    {
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
        // koncovy novy riadok nevytvara dalsi prazdny riadok
        if (lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
        return new InissIniFile(lines);
    }

    /// <summary>Precita subor v CP1250; neexistujuci subor vrati null.</summary>
    public static InissIniFile? Load(string path) => File.Exists(path) ? Parse(File.ReadAllText(path, Encodings.Win1250)) : null;

    /// <summary>Text suboru (CRLF).</summary>
    public override string ToString() => _lines.Count == 0 ? "" : string.Join("\r\n", _lines) + "\r\n";

    /// <summary>Zapise subor v CP1250.</summary>
    public void Save(string path) => File.WriteAllText(path, ToString(), Encodings.Win1250);

    /// <summary>Nazvy sekcii v poradi suboru (bez duplicit).</summary>
    public IReadOnlyList<string> SectionNames
    {
        get
        {
            var names = new List<string>();
            foreach (var line in _lines)
                if (TryHeader(line, out var name) && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
                    names.Add(name);
            return names;
        }
    }

    /// <summary>Ci sekcia v subore je.</summary>
    public bool HasSection(string section) => FindSection(section) >= 0;

    /// <summary>Nazvy a hodnoty sekcie v poradi (prvy vyskyt nazvu).</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Values(string section)
    {
        var result = new List<KeyValuePair<string, string>>();
        var start = FindSection(section);
        if (start < 0) return result;
        for (var i = start + 1; i < _lines.Count && !TryHeader(_lines[i], out _); i++)
            if (TryEntry(_lines[i], out var key, out var value) && result.All(r => !string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase)))
                result.Add(new KeyValuePair<string, string>(key, value));
        return result;
    }

    /// <summary>Text hodnoty alebo null, ak nazov v sekcii nie je (ako GetPrivateProfileString).</summary>
    public string? GetString(string section, string name)
    {
        var index = FindEntry(section, name);
        if (index < 0) return null;
        TryEntry(_lines[index], out _, out var value);
        return value == MissingTextSentinel ? null : value;
    }

    /// <summary>
    /// Cislo alebo null, ak nazov v sekcii nie je (ako GetPrivateProfileInt). Uzna sa desiatkovy zapis so znamienkom
    /// na zaciatku hodnoty; text bez cislic dava 0, sestnastkovy <c>0x…</c> tiez 0.
    /// </summary>
    public int? GetNumber(string section, string name)
    {
        var text = GetString(section, name);
        if (text is null) return null;
        var value = ParseNumber(text);
        return value == MissingNumberSentinel ? null : value;
    }

    /// <summary>Cislo zo zaciatku textu ako GetPrivateProfileInt.</summary>
    public static int ParseNumber(string text)
    {
        var s = text.TrimStart();
        var i = 0;
        var negative = false;
        if (i < s.Length && s[i] is '-' or '+')
        {
            negative = s[i] == '-';
            i++;
        }

        long value = 0;
        while (i < s.Length && char.IsAsciiDigit(s[i]))
        {
            value = value * 10 + (s[i] - '0');
            if (value > uint.MaxValue) value = uint.MaxValue;
            i++;
        }

        return unchecked((int)(negative ? -value : value));
    }

    /// <summary>Nastavi hodnotu - prepise existujuci riadok, inak ju doplni na koniec sekcie (alebo zalozi sekciu).</summary>
    public void Set(string section, string name, string value)
    {
        var text = value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])) ? $"\"{value}\"" : value;
        var index = FindEntry(section, name);
        if (index >= 0)
        {
            TryEntryKey(_lines[index], out var key);
            _lines[index] = $"{key}={text}";
            return;
        }

        var start = FindSection(section);
        if (start < 0)
        {
            if (_lines.Count > 0 && _lines[^1].Trim().Length > 0) _lines.Add("");
            _lines.Add($"[{section}]");
            _lines.Add($"{name}={text}");
            return;
        }

        var insert = start + 1;
        for (var i = start + 1; i < _lines.Count && !TryHeader(_lines[i], out _); i++)
            if (_lines[i].Trim().Length > 0)
                insert = i + 1;
        _lines.Insert(insert, $"{name}={text}");
    }

    /// <summary>Nastavi cislo (desiatkovo, ako ho INISS cita).</summary>
    public void Set(string section, string name, int value) => Set(section, name, value.ToString(CultureInfo.InvariantCulture));

    /// <summary>Odstrani hodnotu; vrati, ci bola v subore.</summary>
    public bool Remove(string section, string name)
    {
        var index = FindEntry(section, name);
        if (index < 0) return false;
        _lines.RemoveAt(index);
        return true;
    }

    /// <summary>Odstrani celu sekciu (prvy vyskyt) vratane jej riadkov a prazdnych riadkov na konci suboru; vrati, ci bola v subore.</summary>
    public bool RemoveSection(string section)
    {
        var start = FindSection(section);
        if (start < 0) return false;
        var end = start + 1;
        while (end < _lines.Count && !TryHeader(_lines[end], out _)) end++;
        _lines.RemoveRange(start, end - start);
        // oddelovac pred sekciou na konci suboru by ostal visiet
        while (_lines.Count > 0 && _lines[^1].Trim().Length == 0) _lines.RemoveAt(_lines.Count - 1);
        return true;
    }

    /// <summary>Kopia suboru (zmeny kopie sa povodneho nedotknu).</summary>
    public InissIniFile Clone() => new([.. _lines]);

    /// <summary>Subor nema ziadny riadok okrem prazdnych (prazdna sekcia Driver* by uz INISS ovplyvnila).</summary>
    public bool IsEmpty => _lines.All(l => l.Trim().Length == 0);

    /// <summary>Komentare sekcie (text za <c>;</c> bez okrajovych medzier) v poradi suboru.</summary>
    public IReadOnlyList<string> Comments(string section)
    {
        var result = new List<string>();
        var start = FindSection(section);
        if (start < 0) return result;
        for (var i = start + 1; i < _lines.Count && !TryHeader(_lines[i], out _); i++)
            if (TryComment(_lines[i], out var text))
                result.Add(text);
        return result;
    }

    /// <summary>Prida komentar hned za hlavicku sekcie; chybajucu sekciu zalozi na konci suboru.</summary>
    public void AddComment(string section, string text)
    {
        var start = FindSection(section);
        if (start >= 0)
        {
            _lines.Insert(start + 1, $"; {text}");
            return;
        }

        if (_lines.Count > 0 && _lines[^1].Trim().Length > 0) _lines.Add("");
        _lines.Add($"[{section}]");
        _lines.Add($"; {text}");
    }

    /// <summary>Odstrani komentare sekcie, ktore splnaju podmienku; vrati ich pocet.</summary>
    public int RemoveComments(string section, Func<string, bool> match)
    {
        var start = FindSection(section);
        if (start < 0) return 0;
        var removed = 0;
        for (var i = start + 1; i < _lines.Count && !TryHeader(_lines[i], out _);)
        {
            if (TryComment(_lines[i], out var text) && match(text))
            {
                _lines.RemoveAt(i);
                removed++;
            }
            else
            {
                i++;
            }
        }

        return removed;
    }

    private static bool TryComment(string line, out string text)
    {
        var t = line.TrimStart();
        text = t.StartsWith(';') ? t[1..].Trim() : "";
        return t.StartsWith(';');
    }

    private int FindSection(string section)
    {
        for (var i = 0; i < _lines.Count; i++)
            if (TryHeader(_lines[i], out var name) && string.Equals(name, section, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private int FindEntry(string section, string name)
    {
        var start = FindSection(section);
        if (start < 0) return -1;
        for (var i = start + 1; i < _lines.Count && !TryHeader(_lines[i], out _); i++)
            if (TryEntryKey(_lines[i], out var key) && string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static bool TryHeader(string line, out string name)
    {
        var t = line.Trim();
        if (t.StartsWith('['))
        {
            var end = t.IndexOf(']', StringComparison.Ordinal);
            name = (end < 0 ? t[1..] : t[1..end]).Trim();
            return true;
        }

        name = "";
        return false;
    }

    private static bool TryEntryKey(string line, out string key)
    {
        var t = line.TrimStart();
        var eq = t.IndexOf('=', StringComparison.Ordinal);
        if (t.StartsWith(';') || eq <= 0)
        {
            key = "";
            return false;
        }

        key = t[..eq].Trim();
        return key.Length > 0;
    }

    private static bool TryEntry(string line, out string key, out string value)
    {
        value = "";
        if (!TryEntryKey(line, out key)) return false;
        var v = line[(line.IndexOf('=', StringComparison.Ordinal) + 1)..].Trim();
        if (v.Length >= 2 && (v[0] == '"' && v[^1] == '"' || v[0] == '\'' && v[^1] == '\'')) v = v[1..^1];
        value = v;
        return true;
    }
}
