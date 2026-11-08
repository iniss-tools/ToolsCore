namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Umiestnenie hlasenia v diagrame (indexy do zoznamov modelu), aby sa dalo v editore preskocit na prvok.
/// </summary>
/// <param name="Kind">Druh prvku.</param>
/// <param name="Category">Index kategorie alebo -1.</param>
/// <param name="State">Index stavu v kategorii alebo -1.</param>
/// <param name="Index">Index prvku v jeho zozname (vzhlad, casovy bod, akcia, ovladac, starter) alebo -1.</param>
public readonly record struct StateDgmLocation(StateDgmElementKind Kind, int Category = -1, int State = -1, int Index = -1)
{
    /// <summary>Cely diagram.</summary>
    public static readonly StateDgmLocation Root = new(StateDgmElementKind.Diagram);
}
