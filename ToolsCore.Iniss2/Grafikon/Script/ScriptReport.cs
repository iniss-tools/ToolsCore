namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Hlasenie skriptu, ktore editor upravuje ako udaje: verejna funkcia s jednym oznacenim hlasenia a telom zo zvukov,
/// pauz a komentarov, pripadne v jazykovych blokoch. Funkcia, ktoru tento model neopise, ostava rucna.
/// </summary>
/// <param name="Name">nazov funkcie = kluc hlasenia (ReportKeys, statedgm, dodatky…)</param>
/// <param name="Kind">druh podla oznacenia</param>
public sealed record ScriptReport(string Name, ScriptReportKind Kind)
{
    /// <summary>Popis z oznacenia, ktory ukazuje klient.</summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Dalsie poradove argumenty oznacenia ako v kode, napr. typ vyluky <c>TYPVYLUKY.AUTO</c> a stanice oznacenia
    /// <c>Vyluka</c>.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>Obmedzenie <c>ProStanice</c> (cisla stanic oddelene ciarkou); <see langword="null" /> = bez obmedzenia.</summary>
    public string? ForStations { get; init; }

    /// <summary>Obmedzenie <c>MimoStanice</c>; <see langword="null" /> = bez obmedzenia.</summary>
    public string? NotStations { get; init; }

    /// <summary>
    /// Nazov parametra so zoznamom stanic (<c>ByVal aiStanice As Integer()</c>) pri vzore; <see langword="null" /> = funkcia
    /// nema parametre.
    /// </summary>
    public string? StationsParameter { get; init; }

    /// <summary>Hlasenie len pre jazyk (<c>If (Jazyk &lt;&gt; "CZ") Then Return</c> na zaciatku); <see langword="null" /> = vsetky.</summary>
    public string? OnlyLanguage { get; init; }

    /// <summary>
    /// Podmienky, pri ktorych sa hlasenie nevyhlasi (<c>If vyraz Then Return</c> na zaciatku, za jazykom), vyrazy ako v kode,
    /// napr. <c>Not Kontrola_koncici()</c>.
    /// </summary>
    public IReadOnlyList<string> Guards { get; init; } = [];

    /// <summary>
    /// Kroky su v sekcii <c>ZacatekSekce()</c> … <c>KonecSekce(bRollBack:=True)</c>: pri chybe zvuku sa vysledok vrati
    /// do stavu pred sekciou.
    /// </summary>
    public bool Section { get; init; }

    /// <summary>Bloky krokov v poradi kodu.</summary>
    public IReadOnlyList<ScriptBlock> Blocks { get; init; } = [];

    /// <inheritdoc />
    public bool Equals(ScriptReport? other) =>
        other is not null && Name == other.Name && Kind == other.Kind && Description == other.Description
        && Arguments.SequenceEqual(other.Arguments) && ForStations == other.ForStations && NotStations == other.NotStations
        && StationsParameter == other.StationsParameter && OnlyLanguage == other.OnlyLanguage && Guards.SequenceEqual(other.Guards)
        && Section == other.Section && Blocks.SequenceEqual(other.Blocks);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Name, Kind, Description, Blocks.Count);
}
