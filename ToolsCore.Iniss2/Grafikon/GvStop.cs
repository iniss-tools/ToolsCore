namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Bod trasy vlaku.
/// </summary>
public sealed record GvStop
{
    /// <summary>Cislo stanice SR70.</summary>
    public required int Station { get; init; }

    /// <summary>Prichod v minutach od polnoci; <see langword="null" />, ak ho vlak nema.</summary>
    public int? Arrival { get; init; }

    /// <summary>O kolko dni po dni jazdy vlaku je prichod.</summary>
    public int ArrivalDay { get; init; }

    /// <summary>Odchod v minutach od polnoci; <see langword="null" />, ak ho vlak nema.</summary>
    public int? Departure { get; init; }

    /// <summary>O kolko dni po dni jazdy vlaku je odchod.</summary>
    public int DepartureDay { get; init; }

    /// <summary>Priznaky zastavenia.</summary>
    public GvStopFlags Flags { get; init; }

    /// <summary>Cislo vlaku od tohto bodu (atribut <c>dcv</c>), podla neho sa vlak paruje s inymi subormi; prazdne = bez zmeny.</summary>
    public string Number { get; init; } = "";

    /// <summary>
    /// Cislo, ktore sa hlasi a zobrazuje (atribut <c>cv</c>); prazdne = <see cref="Number" />. Plati len v bode s cislom vlaku,
    /// server ho do prazdneho <see cref="Number" /> pri nacitani skopiruje.
    /// </summary>
    public string DisplayNumber { get; init; } = "";

    /// <summary>Linka nahradnej dopravy od tohto bodu (atribut <c>l</c>); plati len v bode s cislom vlaku.</summary>
    public string ReplacementLine { get; init; } = "";

    /// <summary>Druh vlaku od tohto bodu; prazdny = bez zmeny.</summary>
    public string TrainType { get; init; } = "";

    /// <summary>Nazov vlaku od tohto bodu; prazdny = bez zmeny.</summary>
    public string TrainName { get; init; } = "";

    /// <summary>Linka integrovaneho dopravneho systemu od tohto bodu; plati len v bode s cislom vlaku.</summary>
    public string Line { get; init; } = "";

    /// <summary>
    /// Vlak v tomto bode riadi INISS: bod ma priznak zaujmovej stanice a vlak v nom zastavuje.
    /// </summary>
    public bool IsInterest => (Flags & (GvStopFlags.Passing | GvStopFlags.Interest)) == GvStopFlags.Interest;

    /// <summary>
    /// Bod ma aspon jeden cas.
    /// </summary>
    public bool HasTime => Arrival.HasValue || Departure.HasValue;

    /// <summary>
    /// Den prichodu; bez prichodu den odchodu.
    /// </summary>
    /// <param name="run">den jazdy vlaku</param>
    public DateOnly ArrivalDate(DateOnly run) => run.AddDays(Arrival.HasValue ? ArrivalDay : DepartureDay);

    /// <summary>
    /// Den odchodu; bez odchodu den prichodu.
    /// </summary>
    /// <param name="run">den jazdy vlaku</param>
    public DateOnly DepartureDate(DateOnly run) => run.AddDays(Departure.HasValue ? DepartureDay : ArrivalDay);
}
