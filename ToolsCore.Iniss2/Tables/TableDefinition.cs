namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Tabula stanice.
/// </summary>
/// <param name="Key">kluc</param>
/// <param name="Name">nazov</param>
/// <param name="Line">kluc komunikacnej linky; prazdny = tabula nema zariadenie</param>
/// <param name="Address">adresa na linke</param>
/// <param name="SaveFile">subor XML v priecinku logov; prazdny = tabula sa do XML neuklada</param>
/// <param name="Parts">casti tabule</param>
public sealed record TableDefinition(string Key, string Name, string Line, int Address, string SaveFile, IReadOnlyList<TablePart> Parts)
{
    /// <summary>Bez adresy - tabula so zariadenim nekomunikuje.</summary>
    public const int NoAddress = -1;

    /// <summary>Cisla zaznamov s otocenymi sipkami.</summary>
    public string ReverseArrows { get; init; } = "";

    /// <summary>Nazov podriadenej linky pre ovladac zariadenia (<c>lin_podr</c>).</summary>
    public string SubLine { get; init; } = "";

    /// <summary>Zariadenie nema rolovat zaznamy (priznak <c>NOSCROLL</c>).</summary>
    public bool NoScroll { get; init; }

    /// <summary>Zaciatok denneho obdobia jasu (<c>lighttime0</c>); prazdny = z linky.</summary>
    public string LightTime0 { get; init; } = "";

    /// <summary>Koniec denneho obdobia jasu (<c>lighttime1</c>).</summary>
    public string LightTime1 { get; init; } = "";

    /// <summary>Jas v dennom obdobi (<c>light0</c>).</summary>
    public string Light0 { get; init; } = "";

    /// <summary>Jas mimo denneho obdobia (<c>light1</c>).</summary>
    public string Light1 { get; init; } = "";

    /// <summary>Jas prazdnej tabule v dennom obdobi (<c>lightoff0</c>).</summary>
    public string LightOff0 { get; init; } = "";

    /// <summary>Jas prazdnej tabule mimo denneho obdobia (<c>lightoff1</c>).</summary>
    public string LightOff1 { get; init; } = "";

    /// <summary>Prazdna tabula svieti v dennom obdobi (<c>lightIfEmpty0</c>, text ako v subore).</summary>
    public string LightIfEmpty0 { get; init; } = "";

    /// <summary>Prazdna tabula svieti mimo denneho obdobia (<c>lightIfEmpty1</c>).</summary>
    public string LightIfEmpty1 { get; init; } = "";

    /// <summary>Najkratsi cas v ms po zmene jasu (<c>lightOnTime</c>).</summary>
    public string LightOnTime { get; init; } = "";
}
