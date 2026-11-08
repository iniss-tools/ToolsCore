namespace ToolsCore.Iniss.Registry;

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
