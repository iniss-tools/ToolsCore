namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Polozka stromu suboru StateDgm.txt - hodnota (<c>S:</c>, <c>I:</c>, <c>B:</c>) alebo skupina (<c>G:</c>, <c>P:</c>).
/// Strom je verny zapisu INISSu (okrem komentarov, ktore parser zahadzuje) a sluzi na zachovanie
/// klucov a skupin, ktorym typovany model nerozumie.
/// </summary>
public abstract class StateDgmItem
{
    /// <summary>Cislo riadka v povodnom texte (0-based), -1 pre polozky vytvorene v kode.</summary>
    public int Line { get; set; } = -1;

    /// <summary>Hlboka kopia polozky.</summary>
    public abstract StateDgmItem Clone();
}
