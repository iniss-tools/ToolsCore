using System.Globalization;

namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Priznaky grafikonu - 4. stlpec DirList.TXT, rozlozeny tak, ako ho cita INISS: text sa prevedie na velke pismena
/// a hladaju sa v nom jednotlive znaky bez ohladu na poradie a oddelovace. Ine znaky INISS ignoruje.
/// </summary>
/// <param name="Spread"><c>Z</c> - tratova kolaj zo spravy GTN sa siri na nasledujuce stanice trasy</param>
/// <param name="DepartureSetsBothTracks"><c>O</c> - odchod a odchodova cesta zo sprav GTN menia okrem kolaje odchodu aj
/// kolaj prichodu</param>
/// <param name="MissingTrain"><c>K</c>/<c>M</c> - co urobi sprava pre vlak, ktory v stanici nie je</param>
/// <param name="Switch">cislica <c>1</c>-<c>9</c> - grafikon je aktivny len pri spusteni INISSu s prepinacom <c>/N</c></param>
public readonly record struct DirListFlags(bool Spread, bool DepartureSetsBothTracks, DirListMissingTrain MissingTrain, int? Switch)
{
    /// <summary>
    /// Ziadny priznak - grafikon je aktivny vzdy a spravy GTN pre vlak, ktory v stanici nie je, zahadzuje.
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
        var missingTrain = upper.Contains('K', StringComparison.Ordinal) ? DirListMissingTrain.ApplyToRoute
            : upper.Contains('M', StringComparison.Ordinal) ? DirListMissingTrain.ApplyToRouteWithoutCategori
            : DirListMissingTrain.Ignore;

        int? @switch = null;
        foreach (var c in upper)
            if (c is >= '1' and <= '9')
                @switch = c - '0';

        return new DirListFlags(upper.Contains('Z', StringComparison.Ordinal), upper.Contains('O', StringComparison.Ordinal), missingTrain, @switch);
    }

    /// <summary>
    /// Zapis do DirList.TXT v poradi <c>Z</c>, <c>O</c>, <c>K</c>/<c>M</c>, cislica - napr. <c>ZK3</c>; bez priznakov prazdny text.
    /// </summary>
    public override string ToString()
    {
        var text = new StringBuilder();
        if (Spread)
            text.Append('Z');
        if (DepartureSetsBothTracks)
            text.Append('O');
        if (MissingTrain == DirListMissingTrain.ApplyToRoute)
            text.Append('K');
        else if (MissingTrain == DirListMissingTrain.ApplyToRouteWithoutCategori)
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
