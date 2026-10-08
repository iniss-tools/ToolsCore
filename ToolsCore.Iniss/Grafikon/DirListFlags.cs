using System.Globalization;

namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Ci sa do grafikonu smu zakladat vlaky z externych sprav (pismena <c>K</c> a <c>M</c> v priznakoch DirList.TXT).
/// </summary>
public enum DirListTrainCreation
{
    /// <summary>
    /// Sprava o vlaku, ktory INISS nepozna, sa len zaloguje a zahodi.
    /// </summary>
    None,

    /// <summary>
    /// <c>K</c> - vlak z externej spravy sa do grafikonu vlozi.
    /// </summary>
    Create,

    /// <summary>
    /// <c>M</c> - ako <see cref="Create" />, navyse INISS z priecinka grafikonu nenacita Categori.TXT.
    /// </summary>
    CreateWithoutCategori
}

/// <summary>
/// Priznaky grafikonu - 4. stlpec DirList.TXT, rozlozeny tak, ako ho cita INISS: text sa prevedie na velke pismena
/// a hladaju sa v nom jednotlive znaky bez ohladu na poradie a oddelovace. Ine znaky INISS ignoruje.
/// </summary>
/// <param name="Spread"><c>Z</c> - externa sprava sa siri na nasledujuce stanice trasy</param>
/// <param name="DepartureTrack"><c>O</c> - sprava prepisuje druhy kolajovy udaj (s vlastnym zamkom)</param>
/// <param name="TrainCreation"><c>K</c>/<c>M</c> - zakladanie vlakov z externych sprav</param>
/// <param name="Switch">cislica <c>1</c>-<c>9</c> - grafikon je aktivny len pri spusteni INISSu s prepinacom <c>/N</c></param>
public readonly record struct DirListFlags(bool Spread, bool DepartureTrack, DirListTrainCreation TrainCreation, int? Switch)
{
    /// <summary>
    /// Ziadny priznak - grafikon je aktivny vzdy a externe spravy o neznamych vlakoch zahadzuje.
    /// </summary>
    public static DirListFlags None => default;

    /// <summary>
    /// Ziadny priznak nie je nastaveny.
    /// </summary>
    public bool IsEmpty => this == None;

    /// <summary>
    /// Rozlozi text priznakov ako INISS: pri <c>K</c> aj <c>M</c> plati <c>K</c>, z viacerych cislic posledna.
    /// </summary>
    public static DirListFlags Parse(string? text)
    {
        var upper = (text ?? "").ToUpperInvariant();
        var creation = upper.Contains('K', StringComparison.Ordinal) ? DirListTrainCreation.Create
            : upper.Contains('M', StringComparison.Ordinal) ? DirListTrainCreation.CreateWithoutCategori
            : DirListTrainCreation.None;

        int? @switch = null;
        foreach (var c in upper)
            if (c is >= '1' and <= '9')
                @switch = c - '0';

        return new DirListFlags(upper.Contains('Z', StringComparison.Ordinal), upper.Contains('O', StringComparison.Ordinal), creation, @switch);
    }

    /// <summary>
    /// Zapis do DirList.TXT v poradi <c>Z</c>, <c>O</c>, <c>K</c>/<c>M</c>, cislica - napr. <c>ZK3</c>; bez priznakov prazdny text.
    /// </summary>
    public override string ToString()
    {
        var text = new StringBuilder();
        if (Spread)
            text.Append('Z');
        if (DepartureTrack)
            text.Append('O');
        if (TrainCreation == DirListTrainCreation.Create)
            text.Append('K');
        else if (TrainCreation == DirListTrainCreation.CreateWithoutCategori)
            text.Append('M');
        if (Switch is { } number)
            text.Append(number.ToString(CultureInfo.InvariantCulture));
        return text.ToString();
    }

    /// <summary>
    /// Text v subore zodpoveda zapisu <see cref="ToString" /> - INISS z neho nic neignoruje ani neprepise inym znakom.
    /// </summary>
    public static bool IsCanonical(string? text) => string.Equals(Parse(text).ToString(), (text ?? "").Trim(), StringComparison.Ordinal);
}
