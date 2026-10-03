using System.Globalization;

namespace ToolsCore.Iniss.Registry;

/// <summary>Druh surovej hodnoty v registri.</summary>
public enum RegRawKind
{
    /// <summary>REG_DWORD.</summary>
    Dword,

    /// <summary>REG_SZ.</summary>
    String,

    /// <summary>REG_BINARY.</summary>
    Binary,

    /// <summary>Iny typ (REG_EXPAND_SZ, REG_MULTI_SZ, REG_QWORD…) - INISS ho necita.</summary>
    Other
}

/// <summary>
/// Surova hodnota z registra tak, ako tam je (pred vyhodnotenim podla typu nastavenia).
/// </summary>
public sealed record RegRawValue
{
    private RegRawValue(RegRawKind kind, int number, string? text, byte[]? bytes, string? otherKind)
    {
        Kind = kind;
        Number = number;
        Text = text;
        Bytes = bytes;
        OtherKind = otherKind;
    }

    /// <summary>Druh.</summary>
    public RegRawKind Kind { get; }

    /// <summary>Cislo pri REG_DWORD.</summary>
    public int Number { get; }

    /// <summary>Text pri REG_SZ.</summary>
    public string? Text { get; }

    /// <summary>Bajty pri REG_BINARY.</summary>
    public byte[]? Bytes { get; }

    /// <summary>Nazov typu pri <see cref="RegRawKind.Other" /> (napr. <c>REG_EXPAND_SZ</c>).</summary>
    public string? OtherKind { get; }

    /// <summary>REG_DWORD.</summary>
    public static RegRawValue Dword(int value) => new(RegRawKind.Dword, value, null, null, null);

    /// <summary>REG_SZ.</summary>
    public static RegRawValue String(string value) => new(RegRawKind.String, 0, value, null, null);

    /// <summary>REG_BINARY.</summary>
    public static RegRawValue Binary(byte[] value) => new(RegRawKind.Binary, 0, null, value, null);

    /// <summary>Iny typ.</summary>
    public static RegRawValue Other(string kindName, string? display = null) => new(RegRawKind.Other, 0, display, null, kindName);

    /// <summary>Hodnota ako objekt (int, string, byte[]).</summary>
    public object? Value => Kind switch
    {
        RegRawKind.Dword => Number,
        RegRawKind.String => Text,
        RegRawKind.Binary => Bytes,
        _ => Text
    };

    /// <inheritdoc />
    public bool Equals(RegRawValue? other) =>
        other is not null && Kind == other.Kind && Number == other.Number && Text == other.Text && OtherKind == other.OtherKind
        && (Bytes is null ? other.Bytes is null : other.Bytes is not null && Bytes.AsSpan().SequenceEqual(other.Bytes));

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Number, Text, OtherKind, Bytes?.Length);

    /// <inheritdoc />
    public override string ToString() => Kind switch
    {
        RegRawKind.Dword => Number.ToString(CultureInfo.InvariantCulture),
        RegRawKind.String => Text ?? "",
        RegRawKind.Binary => Convert.ToHexString(Bytes ?? []),
        _ => OtherKind ?? ""
    };
}

/// <summary>
/// Nacitana vetva konfiguracie INISSu (<c>CHAPS\&lt;aplikacia&gt;</c> v jednom koreni registra) - sekcie a ich
/// hodnoty. Nazvy sekcii aj hodnot sa porovnavaju bez rozlisenia velkosti pismen ako v registri.
/// </summary>
public sealed class RegBranch
{
    private readonly Dictionary<string, Dictionary<string, RegRawValue>> _sections = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Prazdna neexistujuca vetva.</summary>
    public static RegBranch Missing => new(false);

    /// <summary>Vytvori vetvu.</summary>
    /// <param name="exists">kluc aplikacie v registri existuje</param>
    public RegBranch(bool exists = true) => Exists = exists;

    /// <summary>Kluc aplikacie existuje (pri HKCU rozhoduje o vetve per-user nastaveni).</summary>
    public bool Exists { get; }

    /// <summary>Nazvy sekcii (podklucov).</summary>
    public IEnumerable<string> SectionNames => _sections.Keys;

    /// <summary>Ci sekcia existuje.</summary>
    public bool HasSection(string section) => _sections.ContainsKey(section);

    /// <summary>Hodnoty sekcie (prazdne, ak sekcia nie je).</summary>
    public IReadOnlyDictionary<string, RegRawValue> Values(string section) =>
        _sections.TryGetValue(section, out var values) ? values : EmptyValues;

    /// <summary>Hodnota alebo null.</summary>
    public RegRawValue? Get(string section, string name) =>
        _sections.TryGetValue(section, out var values) && values.TryGetValue(name, out var value) ? value : null;

    /// <summary>Zalozi sekciu (aj prazdnu).</summary>
    public RegBranch AddSection(string section)
    {
        if (!_sections.ContainsKey(section))
            _sections[section] = new Dictionary<string, RegRawValue>(StringComparer.OrdinalIgnoreCase);
        return this;
    }

    /// <summary>Nastavi hodnotu (zalozi aj sekciu).</summary>
    public RegBranch Set(string section, string name, RegRawValue value)
    {
        AddSection(section);
        _sections[section][name] = value;
        return this;
    }

    private static readonly Dictionary<string, RegRawValue> EmptyValues = new(StringComparer.OrdinalIgnoreCase);
}
