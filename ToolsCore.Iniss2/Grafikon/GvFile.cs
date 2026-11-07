using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Serilog;
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

/// <summary>
/// Varianta vlaku - trasa a casy pre dni z datumovej poznamky.
/// </summary>
public sealed record GvVariant
{
    /// <summary>Datumova poznamka (atribut <c>jd</c>); prazdna = kazdy den platnosti vlaku.</summary>
    public string Validity { get; init; } = "";

    /// <summary>
    /// Dni jazdy varianty rozdelenej podla MOS.xml; bit <c>i</c> plati pre zaciatok platnosti vlaku plus <c>i</c> dni.
    /// <see langword="null" /> = dni urcuje <see cref="Validity" />.
    /// </summary>
    public BitArray? Days { get; init; }

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
        return variant.Days is { } days ? new BitArray(days) : GvValidity.Days(From, To, variant.Validity);
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
    /// <summary>Najdlhsie obdobie v dnoch, do ktoreho server poznamku rozvinie.</summary>
    public const int MaxDays = 400;

    /// <summary>
    /// Prevedie poznamku na dni rovnako ako server: prazdna plati kazdy den, <c>-</c> ziadny, poznamka moze byt aj
    /// retazec nul a jednotiek s dlzkou obdobia. Chybna poznamka a obdobie nad 400 dni znamenaju ziadny den.
    /// </summary>
    /// <param name="from">prvy den obdobia</param>
    /// <param name="to">posledny den obdobia</param>
    /// <param name="text">poznamka</param>
    /// <returns>bit <c>i</c> plati pre den <paramref name="from" /> plus <c>i</c> dni.</returns>
    public static BitArray Days(DateOnly from, DateOnly to, string? text)
    {
        var count = Math.Max(0, to.DayNumber - from.DayNumber + 1);

        if (string.IsNullOrEmpty(text))
            return new BitArray(count, true);
        if (text is "-" or "--")
            return new BitArray(count, false);

        if (count is 0 or > MaxDays)
            return Failed(from, to, text, count);

        if (text.Length == count && text != "1" && text.All(c => c is '0' or '1'))
        {
            var bits = new BitArray(count);
            for (var i = 0; i < count; i++)
                bits[i] = text[i] == '1';
            return bits;
        }

        try
        {
            // server pocita sviatky vzdy podla ceskeho kalendara, aj na slovenskych staniciach
            var days = new DateLimit(from, to, insertMarks: false) { HolidayLocale = DateLimit.Locale.Cz }.TextToBitArray(text);
            return days.Length == count ? days : Failed(from, to, text, count);
        }
        catch (DateLimit.ParseException)
        {
            return Failed(from, to, text, count);
        }
    }

    /// <summary>
    /// Poznamka pre server: text po cesky, inym jazykom server nerozumie. Ked by ho server rozvinul na ine dni, retazec nul
    /// a jednotiek.
    /// </summary>
    /// <param name="from">prvy den obdobia</param>
    /// <param name="to">posledny den obdobia</param>
    /// <param name="days">dni obdobia; pri obdobi nad <see cref="MaxDays" /> dni server poznamku nerozvinie</param>
    public static string Text(DateOnly from, DateOnly to, BitArray days)
    {
        ArgumentNullException.ThrowIfNull(days);

        if (days.HasAllSet())
            return "";
        if (!days.HasAnySet())
            return "-";

        var bits = string.Concat(days.Cast<bool>().Select(day => day ? '1' : '0'));
        if (days.Length > MaxDays)
            return bits;

        // texty poznamok sa beru z jazyka nastaveneho pre cely program
        var locale = DateLimit.Loc;
        DateLimit.Loc = DateLimit.Locale.Cz;
        try
        {
            var text = new DateLimit(from, to, insertMarks: false) { HolidayLocale = DateLimit.Locale.Cz }.BitArrayToText(days);
            return new BitArray(Days(from, to, text)).Xor(days).HasAnySet() ? bits : text;
        }
        finally
        {
            DateLimit.Loc = locale;
        }
    }

    private static BitArray Failed(DateOnly from, DateOnly to, string text, int count)
    {
        Log.Warning("Datumovu poznamku {Text} sa nepodarilo rozvinut do obdobia {From} - {To}", text, from, to);
        return new BitArray(count);
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
    /// Zapise subor GV.XML. Zapisuje to, co vlak v pamati ma, varianty rozdelene podla MOS.xml dostanu poznamku z dni.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    public void Write(string groupDir) =>
        Iniss2Xml.Save(Path.Combine(groupDir, FileName), new XElement("GV", Stations.Select(station => new XElement("s", station)), Trains.Select(WriteTrain)));

    /// <summary>
    /// Nacita subor GV.XML grafikonu tak, ako ho server drzi v pamati: stanice mimo <paramref name="group" /> nie su zaujmove
    /// a druh MOs urcuje len subor MOS.xml stanic, varianty sa podla neho mozu rozdelit.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="group">grafikon</param>
    public static GvFile Read(string dataDir, DirListGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        var groupDir = Path.Combine(dataDir, group.Name);
        var root = Iniss2Xml.Load(Path.Combine(groupDir, FileName)).Root!;
        var known = group.Items.Select(item => item.Station).ToHashSet();

        var stations = root.Elements("s").Select(s => XmlConvert.ToInt32(s.Value)).Where(known.Contains).ToList();
        var trains = root.Elements("v").Select(element => ReadTrain(element, known)).ToList();

        var records = new Dictionary<int, List<MosTrain>>();
        foreach (var item in group.Items)
            records.TryAdd(item.Station, MosFile.Read(Path.Combine(groupDir, item.Directory)));

        return new GvFile(stations, MosFile.Apply(trains, records));
    }

    /// <summary>
    /// Obdobia vlakov v stanici podla cisla: zjednotenie obdobi vlakov, ktore s tym cislom stanicou idu. Do nich server
    /// rozvija poznamky suborov stanice.
    /// </summary>
    /// <param name="station">cislo stanice</param>
    public Dictionary<string, (DateOnly From, DateOnly To)> TrainRanges(int station) => TrainRanges(Trains, station);

    /// <summary>
    /// Obdobia vlakov v stanici podla cisla - pozri <see cref="TrainRanges(int)" />.
    /// </summary>
    /// <param name="trains">vlaky</param>
    /// <param name="station">cislo stanice</param>
    internal static Dictionary<string, (DateOnly From, DateOnly To)> TrainRanges(IReadOnlyList<GvTrain> trains, int station)
    {
        var ranges = new Dictionary<string, (DateOnly From, DateOnly To)>(StringComparer.Ordinal);

        foreach (var train in trains)
        {
            foreach (var variant in train.Variants)
            {
                var number = "";
                foreach (var stop in variant.Stops)
                {
                    if (stop.Number.Length > 0)
                        number = stop.Number;

                    if (stop.Station != station || !stop.Flags.HasFlag(GvStopFlags.Interest) || !stop.HasTime || number.Length == 0)
                        continue;

                    ranges[number] = ranges.TryGetValue(number, out var range)
                        ? (train.From < range.From ? train.From : range.From, train.To > range.To ? train.To : range.To)
                        : (train.From, train.To);
                }
            }
        }

        return ranges;
    }

    private static XElement WriteTrain(GvTrain train) => new("v",
        train.From == DateOnly.MinValue ? null : new XAttribute("o", train.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
        train.To == DateOnly.MaxValue ? null : new XAttribute("d", train.To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
        train.Variants.Select(variant => WriteVariant(train, variant)));

    private static XElement WriteVariant(GvTrain train, GvVariant variant) => new("v",
        Iniss2Xml.Optional("jd", variant.Days is { } days ? GvValidity.Text(train.From, train.To, days) : variant.Validity),
        Iniss2Xml.Optional("d", variant.Operator),
        Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)variant.Flags, VariantFlagKeys)),
        Iniss2Xml.Optional("p", variant.Note),
        variant.Stops.Select(WriteStop));

    private static XElement WriteStop(GvStop stop) => new("z",
        new XAttribute("c", stop.Station),
        stop.Arrival is { } arrival ? new XAttribute("p", arrival) : null,
        Iniss2Xml.Optional("cp", stop.ArrivalDay),
        stop.Departure is { } departure ? new XAttribute("o", departure) : null,
        Iniss2Xml.Optional("co", stop.DepartureDay),
        Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)stop.Flags, StopFlagKeys)),
        Iniss2Xml.Optional("dcv", stop.Number),
        Iniss2Xml.Optional("dv", stop.TrainType),
        Iniss2Xml.Optional("nv", stop.TrainName),
        Iniss2Xml.Optional("ids", stop.Line));

    private static GvTrain ReadTrain(XElement element, HashSet<int> known) => new(
        ReadDate(element, "o", DateOnly.MinValue),
        ReadDate(element, "d", DateOnly.MaxValue),
        element.Elements("v").Select(variant => ReadVariant(variant, known)).ToList());

    // vlak bez atributu d nema koniec platnosti
    private static DateOnly ReadDate(XElement element, string attribute, DateOnly missing) => element.Attribute(attribute) is { } value
        ? DateOnly.FromDateTime(XmlConvert.ToDateTime(value.Value, XmlDateTimeSerializationMode.Unspecified))
        : missing;

    private static GvVariant ReadVariant(XElement element, HashSet<int> known) => new()
    {
        Validity = Iniss2Xml.Text(element, "jd"),
        Operator = Iniss2Xml.Text(element, "d"),
        Flags = (GvVariantFlags)Iniss2Xml.Flags(element, "f", VariantFlagKeys),
        Note = Iniss2Xml.Text(element, "p"),
        Stops = element.Elements("z").Select(stop => ReadStop(stop, known)).ToList()
    };

    private static GvStop ReadStop(XElement element, HashSet<int> known)
    {
        var arrival = Iniss2Xml.Number(element, "p", -1);
        var departure = Iniss2Xml.Number(element, "o", -1);
        var number = Iniss2Xml.Text(element, "dcv");
        var station = Iniss2Xml.Number(element, "c");
        var flags = (GvStopFlags)Iniss2Xml.Flags(element, "f", StopFlagKeys);
        var type = Iniss2Xml.Text(element, "dv");

        return new GvStop
        {
            Station = station,
            Arrival = arrival >= 0 ? arrival : null,
            ArrivalDay = Iniss2Xml.Number(element, "cp"),
            Departure = departure >= 0 ? departure : null,
            DepartureDay = Iniss2Xml.Number(element, "co"),
            // stanicu mimo grafikonu server riadit nevie
            Flags = known.Contains(station) ? flags : flags & ~GvStopFlags.Interest,
            // server doplni cislo z atributu cv, ak dcv chyba
            Number = number.Length > 0 ? number : Iniss2Xml.Text(element, "cv"),
            // druh MOs urcuje len MOS.xml
            TrainType = type == MosFile.MotorType ? MosFile.PassengerType : type,
            TrainName = Iniss2Xml.Text(element, "nv"),
            Line = Iniss2Xml.Text(element, "ids")
        };
    }
}
