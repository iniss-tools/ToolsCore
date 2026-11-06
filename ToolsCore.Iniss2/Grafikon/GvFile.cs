using System.Collections;
using System.Xml;
using System.Xml.Linq;
using ToolsCore.Iniss.Calendar;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Priznaky zastavenia vlaku v stanici.
/// </summary>
[Flags]
public enum GvStopFlags
{
    /// <summary>Vlak v stanici zastavuje.</summary>
    None = 0,

    /// <summary>Vlak stanicou prechadza bez zastavenia (kluc P).</summary>
    Passing = 1,

    /// <summary>Stanica je zaujmova - vlak v nej riadi INISS (kluc Z).</summary>
    Interest = 2,

    /// <summary>Nutny prestup (kluc PR) - kolaj zo spravy GTN sa v takom bode neprenasa medzi prichodom a odchodom.</summary>
    Pr = 4,

    /// <summary>Dopravny bod (kluc DB).</summary>
    OperatingPoint = 8,

    /// <summary>Zastavka na znamenie (kluc X).</summary>
    OnRequest = 0x10
}

/// <summary>
/// Priznaky varianty vlaku. Nazvy zodpovedaju klucom v subore.
/// </summary>
[Flags]
public enum GvVariantFlags
{
    /// <summary>Bez priznakov.</summary>
    None = 0,

    /// <summary>Kluc O.</summary>
    O = 1,

    /// <summary>Kluc M.</summary>
    M = 2,

    /// <summary>Kluc R.</summary>
    R = 4,

    /// <summary>Kluc L.</summary>
    L = 8,

    /// <summary>Kluc D - s takou variantou sa spravy GTN neparuju.</summary>
    D = 0x10,

    /// <summary>Kluc X.</summary>
    X = 0x20,

    /// <summary>Kluc N.</summary>
    N = 0x40,

    /// <summary>Kluc NP.</summary>
    NP = 0x80
}

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

    /// <summary>Cislo vlaku od tohto bodu; prazdne = bez zmeny.</summary>
    public string Number { get; init; } = "";

    /// <summary>Druh vlaku od tohto bodu; prazdny = bez zmeny.</summary>
    public string TrainType { get; init; } = "";

    /// <summary>Nazov vlaku od tohto bodu; prazdny = bez zmeny.</summary>
    public string TrainName { get; init; } = "";

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

/// <summary>
/// Varianta vlaku - trasa a casy pre dni z datumovej poznamky.
/// </summary>
public sealed record GvVariant
{
    /// <summary>Datumova poznamka (atribut <c>jd</c>); prazdna = kazdy den platnosti vlaku.</summary>
    public string Validity { get; init; } = "";

    /// <summary>Dopravca.</summary>
    public string Operator { get; init; } = "";

    /// <summary>Priznaky varianty.</summary>
    public GvVariantFlags Flags { get; init; }

    /// <summary>Poznamka.</summary>
    public string Note { get; init; } = "";

    /// <summary>Body trasy v poradi jazdy.</summary>
    public required IReadOnlyList<GvStop> Stops { get; init; }

    /// <summary>
    /// Bod trasy, od ktoreho plati cislo a druh vlaku v bode <paramref name="index" />. Prichod plati este z predchadzajuceho
    /// useku, vlak moze v stanici zmenit cislo.
    /// </summary>
    /// <param name="index">index bodu trasy</param>
    /// <param name="arrival">pri prichode; inak pri odchode</param>
    /// <returns>bod so zmenou alebo <see langword="null" />, ak trasa cislo nema.</returns>
    public GvStop? NumberSource(int index, bool arrival)
    {
        for (var i = arrival && index > 0 ? index - 1 : index; i >= 0; i--)
        {
            if (Stops[i].Number.Length > 0)
                return Stops[i];
        }

        return null;
    }
}

/// <summary>
/// Vlak grafikonu s obdobim platnosti a variantmi. Prva varianta, ktora v dany den ide, je ta platna.
/// </summary>
/// <param name="From">prvy den platnosti</param>
/// <param name="To">posledny den platnosti</param>
/// <param name="Variants">varianty vlaku</param>
public sealed record GvTrain(DateOnly From, DateOnly To, IReadOnlyList<GvVariant> Variants)
{
    /// <summary>
    /// Dni jazdy varianty v obdobi vlaku. Bit <c>i</c> plati pre den <see cref="From" /> plus <c>i</c> dni.
    /// </summary>
    public BitArray DaysOf(GvVariant variant)
    {
        ArgumentNullException.ThrowIfNull(variant);
        return GvValidity.Days(From, To, variant.Validity);
    }

    /// <summary>
    /// Dni, ked plati prave tato varianta - bez dni, ktore patria niektorej variante pred nou.
    /// </summary>
    public BitArray ActiveDaysOf(GvVariant variant)
    {
        var days = DaysOf(variant);

        foreach (var earlier in Variants.TakeWhile(candidate => !ReferenceEquals(candidate, variant)))
            days.And(DaysOf(earlier).Not());

        return days;
    }
}

/// <summary>
/// Datumove poznamky grafikonu.
/// </summary>
public static class GvValidity
{
    /// <summary>
    /// Prevedie poznamku na dni rovnako ako server: prazdna plati kazdy den, <c>-</c> ziadny, poznamka moze byt aj
    /// retazec nul a jednotiek s dlzkou obdobia.
    /// </summary>
    /// <param name="from">prvy den obdobia</param>
    /// <param name="to">posledny den obdobia</param>
    /// <param name="text">poznamka</param>
    /// <returns>bit <c>i</c> plati pre den <paramref name="from" /> plus <c>i</c> dni.</returns>
    public static BitArray Days(DateOnly from, DateOnly to, string? text)
    {
        var count = to.DayNumber - from.DayNumber + 1;

        if (string.IsNullOrEmpty(text))
            return new BitArray(count, true);
        if (text is "-" or "--")
            return new BitArray(count, false);

        if (text.Length == count && text != "1" && text.All(c => c is '0' or '1'))
        {
            var bits = new BitArray(count);
            for (var i = 0; i < count; i++)
                bits[i] = text[i] == '1';
            return bits;
        }

        // server pocita sviatky vzdy podla ceskeho kalendara, aj na slovenskych staniciach
        return new DateLimit(from, to, insertMarks: false) { HolidayLocale = DateLimit.Locale.Cz }.TextToBitArray(text);
    }
}

/// <summary>
/// Vlaky grafikonu so spolocnou trasou pre vsetky stanice - subor GV.XML v priecinku grafikonu.
/// </summary>
/// <param name="Stations">zaujmove stanice grafikonu</param>
/// <param name="Trains">vlaky</param>
public sealed record GvFile(IReadOnlyList<int> Stations, IReadOnlyList<GvTrain> Trains)
{
    /// <summary>
    /// Nazov suboru v priecinku grafikonu.
    /// </summary>
    public const string FileName = "GV.XML";

    private static readonly Dictionary<string, int> StopFlagKeys = new()
    {
        ["P"] = (int)GvStopFlags.Passing,
        ["Z"] = (int)GvStopFlags.Interest,
        ["PR"] = (int)GvStopFlags.Pr,
        ["DB"] = (int)GvStopFlags.OperatingPoint,
        ["X"] = (int)GvStopFlags.OnRequest
    };

    private static readonly Dictionary<string, int> VariantFlagKeys = new()
    {
        ["O"] = (int)GvVariantFlags.O,
        ["M"] = (int)GvVariantFlags.M,
        ["R"] = (int)GvVariantFlags.R,
        ["L"] = (int)GvVariantFlags.L,
        ["D"] = (int)GvVariantFlags.D,
        ["X"] = (int)GvVariantFlags.X,
        ["N"] = (int)GvVariantFlags.N,
        ["NP"] = (int)GvVariantFlags.NP
    };

    /// <summary>
    /// Nacita subor GV.XML.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    public static GvFile Read(string groupDir)
    {
        var root = Iniss2Xml.Load(Path.Combine(groupDir, FileName)).Root!;

        var stations = root.Elements("s").Select(s => XmlConvert.ToInt32(s.Value)).ToList();
        return new GvFile(stations, root.Elements("v").Select(ReadTrain).ToList());
    }

    private static GvTrain ReadTrain(XElement element) => new(
        DateOnly.FromDateTime(XmlConvert.ToDateTime(Iniss2Xml.Text(element, "o"), XmlDateTimeSerializationMode.Unspecified)),
        DateOnly.FromDateTime(XmlConvert.ToDateTime(Iniss2Xml.Text(element, "d"), XmlDateTimeSerializationMode.Unspecified)),
        element.Elements("v").Select(ReadVariant).ToList());

    private static GvVariant ReadVariant(XElement element) => new()
    {
        Validity = Iniss2Xml.Text(element, "jd"),
        Operator = Iniss2Xml.Text(element, "d"),
        Flags = (GvVariantFlags)Iniss2Xml.Flags(element, "f", VariantFlagKeys),
        Note = Iniss2Xml.Text(element, "p"),
        Stops = element.Elements("z").Select(ReadStop).ToList()
    };

    private static GvStop ReadStop(XElement element)
    {
        var arrival = Iniss2Xml.Number(element, "p", -1);
        var departure = Iniss2Xml.Number(element, "o", -1);
        var number = Iniss2Xml.Text(element, "dcv");

        return new GvStop
        {
            Station = Iniss2Xml.Number(element, "c"),
            Arrival = arrival >= 0 ? arrival : null,
            ArrivalDay = Iniss2Xml.Number(element, "cp"),
            Departure = departure >= 0 ? departure : null,
            DepartureDay = Iniss2Xml.Number(element, "co"),
            Flags = (GvStopFlags)Iniss2Xml.Flags(element, "f", StopFlagKeys),
            // server doplni cislo z atributu cv, ak dcv chyba
            Number = number.Length > 0 ? number : Iniss2Xml.Text(element, "cv"),
            TrainType = Iniss2Xml.Text(element, "dv"),
            TrainName = Iniss2Xml.Text(element, "nv")
        };
    }
}
