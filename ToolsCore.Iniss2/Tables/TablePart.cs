namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Cast tabule - jej obsah a katalogova predloha.
/// </summary>
/// <param name="Name">nazov</param>
/// <param name="CatalogKey">kluc predlohy v katalogu stanice</param>
/// <param name="Kind">druh casti, napriklad DEPARTURE</param>
/// <param name="Records">pocet zaznamov</param>
public sealed record TablePart(string Name, string CatalogKey, string Kind, int Records)
{
    /// <summary>Druh casti s informacnym textom.</summary>
    public const string Info = "INFO";

    /// <summary>Druh casti bez vlakov.</summary>
    public const string NotDefined = "NODEF";

    /// <summary>Kluce kolaji, ktorych vlaky cast ukazuje.</summary>
    public IReadOnlyList<string> Tracks { get; init; } = [];

    /// <summary>Priority kolaji podla kluca; chybajuca = 0.</summary>
    public IReadOnlyDictionary<string, int> TrackPriorities { get; init; } = new Dictionary<string, int>();

    /// <summary>Cisla stanic oddelene medzerou, ktorych vlaky cast ukazuje; prazdne = stanica tabule.</summary>
    public string Stations { get; init; } = "";

    /// <summary>Kluc tabule, na ktorej cast nadvazuje; prazdny = cast zacina.</summary>
    public string Previous { get; init; } = "";

    /// <summary>Kluc weboveho zdroja dat (len pri druhu INFO).</summary>
    public string WebSource { get; init; } = "";

    /// <summary>Podmienka zobrazenia vlaku, kod VB.NET typu Boolean; prazdna = vsetky vlaky.</summary>
    public string Condition { get; init; } = "";

    /// <summary>Rozdelenie zaznamov do multiokien (<c>windef</c>); prazdne = multiokno 0.</summary>
    public string WindowDefinition { get; init; } = "";
}
