namespace ToolsCore.StateDgm;

/// <summary>
///     Polozka stromu suboru StateDgm.txt - hodnota (<c>S:</c>, <c>I:</c>, <c>B:</c>) alebo skupina (<c>G:</c>, <c>P:</c>).
///     Strom je verny zapisu INISSu (okrem komentarov, ktore parser zahadzuje) a sluzi na zachovanie
///     klucov a skupin, ktorym typovany model nerozumie.
/// </summary>
public abstract class StateDgmItem
{
    /// <summary>Cislo riadka v povodnom texte (0-based), -1 pre polozky vytvorene v kode.</summary>
    public int Line { get; set; } = -1;

    /// <summary>Hlboka kopia polozky.</summary>
    public abstract StateDgmItem Clone();
}

/// <summary>
///     Druh hodnoty v subore StateDgm.txt.
/// </summary>
public enum StateDgmValueKind
{
    /// <summary><c>S:"kluc"="text"</c></summary>
    String,

    /// <summary><c>I:"kluc"=cislo</c> (strtol so zakladom 0 - <c>0x08</c> aj <c>010</c>)</summary>
    Int,

    /// <summary><c>B:"kluc"=Ano|Ne|Yes|No</c></summary>
    Bool
}

/// <summary>
///     Hodnota v strome StateDgm.
/// </summary>
public sealed class StateDgmValue : StateDgmItem
{
    /// <summary>Vytvori textovu hodnotu.</summary>
    public StateDgmValue(string key, string text)
    {
        Kind = StateDgmValueKind.String;
        Key = key;
        Text = text;
    }

    /// <summary>Vytvori celociselnu hodnotu; <paramref name="raw" /> je povodny zapis (napr. <c>0x08</c>), ak sa ma zachovat.</summary>
    public StateDgmValue(string key, int number, string? raw = null)
    {
        Kind = StateDgmValueKind.Int;
        Key = key;
        Number = number;
        Raw = raw;
    }

    /// <summary>Vytvori pravdivostnu hodnotu.</summary>
    public StateDgmValue(string key, bool flag)
    {
        Kind = StateDgmValueKind.Bool;
        Key = key;
        Flag = flag;
    }

    private StateDgmValue(StateDgmValueKind kind, string key)
    {
        Kind = kind;
        Key = key;
    }

    /// <summary>Druh hodnoty.</summary>
    public StateDgmValueKind Kind { get; }

    /// <summary>Kluc.</summary>
    public string Key { get; }

    /// <summary>Text (pre <see cref="StateDgmValueKind.String" />).</summary>
    public string Text { get; set; } = "";

    /// <summary>Cislo (pre <see cref="StateDgmValueKind.Int" />).</summary>
    public int Number { get; set; }

    /// <summary>Povodny zapis cisla (napr. <c>0x18</c>), aby sa pri zapise zachoval; null = zapisat desiatkovo.</summary>
    public string? Raw { get; set; }

    /// <summary>Priznak (pre <see cref="StateDgmValueKind.Bool" />).</summary>
    public bool Flag { get; set; }

    /// <summary>
    ///     Zapis <c>S:"kluc"=#</c> (bez hodnoty) - INISS nim kluc zo skupiny odstrani. V suboroch grafikonov sa nepouziva,
    ///     ale parser ho prijme a zapisovac zachova.
    /// </summary>
    public bool IsRemoval { get; private init; }

    /// <summary>Vytvori znacku odstranenia kluca (<c>X:"kluc"=#</c>).</summary>
    public static StateDgmValue Removal(StateDgmValueKind kind, string key) => new(kind, key) { IsRemoval = true };

    /// <inheritdoc />
    public override StateDgmItem Clone()
    {
        var c = new StateDgmValue(Kind, Key) { IsRemoval = IsRemoval, Text = Text, Number = Number, Raw = Raw, Flag = Flag, Line = Line };
        return c;
    }

    /// <inheritdoc />
    public override string ToString() => Kind switch
    {
        StateDgmValueKind.String => $"S:\"{Key}\"=\"{Text}\"",
        StateDgmValueKind.Int => $"I:\"{Key}\"={Raw ?? Number.ToString()}",
        _ => $"B:\"{Key}\"={(Flag ? "Ano" : "Ne")}"
    };
}

/// <summary>
///     Skupina v strome StateDgm (<c>G:"meno" { … }</c> alebo blok <c>P:"cesta" { … }</c>).
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
    ///     Meno skupiny bez poradoveho cisla (<c>State12</c> → <c>State</c>). INISS vyhladava skupiny podla
    ///     zakladneho mena; cislo za nim je nepovinne.
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
            StateDgmValueKind.Int => v.Raw ?? v.Number.ToString(),
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
