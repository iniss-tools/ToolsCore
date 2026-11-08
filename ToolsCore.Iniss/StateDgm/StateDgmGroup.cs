using System.Globalization;

namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Skupina v strome StateDgm (<c>G:"meno" { … }</c> alebo blok <c>P:"cesta" { … }</c>).
/// </summary>
public sealed class StateDgmGroup : StateDgmItem
{
    /// <summary>Vytvori prazdnu skupinu.</summary>
    public StateDgmGroup(string name)
    {
        Name = name;
    }

    /// <summary>Meno skupiny tak, ako je v subore (napr. <c>State1</c>, <c>Event</c>, <c>Categorie3</c>).</summary>
    public string Name { get; set; }

    /// <summary>Polozky v poradi zapisu.</summary>
    public List<StateDgmItem> Items { get; } = [];

    /// <summary>Vsetky hodnoty skupiny.</summary>
    public IEnumerable<StateDgmValue> Values => Items.OfType<StateDgmValue>();

    /// <summary>Vsetky vnorene skupiny.</summary>
    public IEnumerable<StateDgmGroup> Groups => Items.OfType<StateDgmGroup>();

    /// <summary>
    /// Meno skupiny bez poradoveho cisla (<c>State12</c> → <c>State</c>). INISS vyhladava skupiny podla
    /// zakladneho mena; cislo za nim je nepovinne.
    /// </summary>
    public static string BaseName(string name)
    {
        var end = name.Length;
        while (end > 0 && char.IsDigit(name[end - 1])) end--;
        return end == 0 ? name : name[..end];
    }

    /// <summary>Vnorene skupiny so zakladnym menom <paramref name="baseName" /> (bez ohladu na cislo), v poradi zapisu.</summary>
    public IEnumerable<StateDgmGroup> GroupsNamed(string baseName) =>
        Groups.Where(g => string.Equals(BaseName(g.Name), baseName, StringComparison.Ordinal));

    /// <summary>Prva vnorena skupina s presnym menom (posledna vyhrava ako pri INISSe, ked je ich viac).</summary>
    public StateDgmGroup? Group(string name) => Groups.LastOrDefault(g => string.Equals(g.Name, name, StringComparison.Ordinal));

    /// <summary>Hodnota s danym klucom (posledna vyhrava), null ak chyba.</summary>
    public StateDgmValue? Value(string key) =>
        Values.LastOrDefault(v => !v.IsRemoval && string.Equals(v.Key, key, StringComparison.Ordinal));

    /// <summary>Textova hodnota kluca; ak je kluc ciselny/pravdivostny, vrati jeho textovu podobu.</summary>
    public string? GetString(string key)
    {
        var v = Value(key);
        return v?.Kind switch
        {
            StateDgmValueKind.String => v.Text,
            StateDgmValueKind.Int => v.Raw ?? v.Number.ToString(CultureInfo.InvariantCulture),
            StateDgmValueKind.Bool => v.Flag ? "Ano" : "Ne",
            _ => null
        };
    }

    /// <summary>Ciselna hodnota kluca; textovy kluc sa skusi previest ako INISS (strtol so zakladom 0).</summary>
    public int? GetInt(string key)
    {
        var v = Value(key);
        return v?.Kind switch
        {
            StateDgmValueKind.Int => v.Number,
            StateDgmValueKind.Bool => v.Flag ? 1 : 0,
            StateDgmValueKind.String => StateDgmReader.TryStrtol(v.Text, out var n) ? n : null,
            _ => null
        };
    }

    /// <summary>Pravdivostna hodnota kluca (cislo: nenulove = true).</summary>
    public bool? GetBool(string key)
    {
        var v = Value(key);
        return v?.Kind switch
        {
            StateDgmValueKind.Bool => v.Flag,
            StateDgmValueKind.Int => v.Number != 0,
            StateDgmValueKind.String => v.Text is "Ano" or "Yes" or "1",
            _ => null
        };
    }

    /// <summary>Odstrani vsetky hodnoty s danym klucom.</summary>
    public void Remove(string key) => Items.RemoveAll(i => i is StateDgmValue v && string.Equals(v.Key, key, StringComparison.Ordinal));

    /// <summary>Prida vnorenu skupinu a vrati ju.</summary>
    public StateDgmGroup AddGroup(string name)
    {
        var g = new StateDgmGroup(name);
        Items.Add(g);
        return g;
    }

    /// <summary>Prida textovu hodnotu.</summary>
    public StateDgmGroup Add(string key, string text)
    {
        Items.Add(new StateDgmValue(key, text));
        return this;
    }

    /// <summary>Prida ciselnu hodnotu.</summary>
    public StateDgmGroup Add(string key, int number, string? raw = null)
    {
        Items.Add(new StateDgmValue(key, number, raw));
        return this;
    }

    /// <summary>Prida pravdivostnu hodnotu.</summary>
    public StateDgmGroup Add(string key, bool flag)
    {
        Items.Add(new StateDgmValue(key, flag));
        return this;
    }

    /// <inheritdoc />
    public override StateDgmItem Clone()
    {
        var g = new StateDgmGroup(Name) { Line = Line };
        foreach (var i in Items) g.Items.Add(i.Clone());
        return g;
    }

    /// <inheritdoc />
    public override string ToString() => $"G:\"{Name}\" ({Items.Count})";
}
