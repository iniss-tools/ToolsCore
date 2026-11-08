namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Kategoria vlaku (<c>StateDgmCtrls\StateDgm\CategorieN</c>).
/// </summary>
public sealed class StateDgmCategory : StateDgmElement
{
    /// <summary>Kluc kategorie.</summary>
    public string Key { get; set; } = "";

    /// <summary>Nazov pre obsluhu.</summary>
    public string Name { get; set; } = "";

    /// <summary>Vysvetlenie, kedy kategoria plati.</summary>
    public string Comment { get; set; } = "";

    /// <summary>Cislo ikony 0-2 (cervene 3-5 si INISS odvodi sam).</summary>
    public int Icon { get; set; }

    /// <summary>Stavy; prvy ma byt <c>#Start</c>.</summary>
    public List<StateDgmState> States { get; } = [];

    /// <summary>Stav podla kluca.</summary>
    public StateDgmState? FindState(string key) => States.FirstOrDefault(s => s.Key == key);

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrEmpty(Name) ? Key : Name;
}
