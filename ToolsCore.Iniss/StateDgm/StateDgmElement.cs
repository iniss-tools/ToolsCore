namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Spolocny zaklad typovanych prvkov - drzi polozky, ktorym model nerozumie, aby sa pri zapise zachovali.
/// </summary>
public abstract class StateDgmElement
{
    /// <summary>Kluce a skupiny z povodneho suboru, ktore model nepozna (zapisu sa na koniec bloku).</summary>
    public List<StateDgmItem> Extras { get; } = [];

    /// <summary>Cislo riadka v povodnom subore (0-based), -1 pre nove prvky.</summary>
    public int Line { get; set; } = -1;
}
