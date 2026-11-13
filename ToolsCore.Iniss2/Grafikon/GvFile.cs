using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Vlaky grafikonu so spolocnou trasou pre vsetky stanice - subor GV.XML v priecinku grafikonu.
/// </summary>
/// <remarks>
/// <see cref="Load" /> cita subor tak, ako je; <see cref="Read" /> vrati vlaky tak, ako ich server drzi v pamati (pozri
/// <see cref="ForServer(IReadOnlySet{int}, IReadOnlyDictionary{int, List{MosTrain}})" />). Zapisuje sa vzdy obsah suboru, nie pohlad servera.
/// </remarks>
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

    /// <summary>Cislo atributu <c>f</c> korena; server ho len prenasa, vyznam nie je znamy.</summary>
    public int Flags { get; init; }

    /// <summary>Jazyk (atribut <c>l</c> korena, v datach <c>C</c>); server ho len prenasa.</summary>
    public string Language { get; init; } = "";

    /// <summary>Dalsi zoznam cisel stanic (prvky <c>sl</c>); server ho len prenasa, vyznam nie je znamy.</summary>
    public IReadOnlyList<int> LimitedStations { get; init; } = [];

    /// <summary>
    /// Zapise subor GV.XML. Existujuci subor sa upravi na mieste - zmenia sa len zmenene vlaky. Varianty rozdelene podla
    /// MOS.xml dostanu poznamku z dni.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    /// <returns><see langword="false" />, ak subor uz mal tento obsah.</returns>
    public bool Write(string groupDir) => Iniss2Xml.Write(Path.Combine(groupDir, FileName), Build(), root => Parse(root).Build());

    /// <summary>
    /// Nacita subor GV.XML grafikonu tak, ako je - bez uprav, ktore server robi pri nacitani.
    /// </summary>
    /// <param name="groupDir">priecinok grafikonu</param>
    public static GvFile Load(string groupDir) => Parse(Iniss2Xml.Load(Path.Combine(groupDir, FileName)).Root!);

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
        var records = new Dictionary<int, List<MosTrain>>();
        foreach (var item in group.Items)
            records.TryAdd(item.Station, MosFile.Read(Path.Combine(groupDir, item.Directory)));

        return Load(groupDir).ForServer(group.Items.Select(item => item.Station).ToHashSet(), records);
    }

    /// <summary>
    /// Vlaky tak, ako ich server drzi v pamati: zo zoznamu stanic vypadnu stanice mimo grafikonu a ich body stratia kluc Z,
    /// prazdne cislo bodu sa doplni zo zobrazovaneho, druh MOs sa zmeni na Os a podla MOS.xml stanic sa znovu urci, pricom
    /// sa varianta moze rozdelit.
    /// </summary>
    /// <param name="known">cisla stanic grafikonu z DirList.xml</param>
    /// <param name="mos">zaznamy MOS.xml podla cisla stanice</param>
    public GvFile ForServer(IReadOnlySet<int> known, IReadOnlyDictionary<int, List<MosTrain>> mos)
    {
        ArgumentNullException.ThrowIfNull(known);
        ArgumentNullException.ThrowIfNull(mos);

        var trains = Trains.Select(train => train with
        {
            Variants = train.Variants.Select(variant => variant with { Stops = variant.Stops.Select(stop => ForServer(stop, known)).ToList() }).ToList()
        }).ToList();

        return this with { Stations = Stations.Where(known.Contains).ToList(), Trains = MosFile.Apply(trains, mos) };
    }

    private static GvStop ForServer(GvStop stop, IReadOnlySet<int> known) => stop with
    {
        // stanicu mimo grafikonu server riadit nevie
        Flags = known.Contains(stop.Station) ? stop.Flags : stop.Flags & ~GvStopFlags.Interest,
        // server doplni cislo z atributu cv, ak dcv chyba
        Number = stop.Number.Length > 0 ? stop.Number : stop.DisplayNumber,
        // druh MOs urcuje len MOS.xml
        TrainType = stop.TrainType == MosFile.MotorType ? MosFile.PassengerType : stop.TrainType
    };

    /// <summary>
    /// Priznaky bodu trasy klucmi ako v subore (napr. <c>P DB</c>).
    /// </summary>
    public static string FlagText(GvStopFlags flags) => Iniss2Xml.FlagText((int)flags, StopFlagKeys);

    /// <summary>
    /// Priznaky varianty klucmi ako v subore (napr. <c>D X</c>).
    /// </summary>
    public static string FlagText(GvVariantFlags flags) => Iniss2Xml.FlagText((int)flags, VariantFlagKeys);

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

    // poradie atributov a prvkov ako v XmlSerializer servera
    private XElement Build() => new("GV",
        Iniss2Xml.Optional("f", Flags),
        Iniss2Xml.Optional("l", Language),
        Stations.Select(station => new XElement("s", station)),
        LimitedStations.Select(station => new XElement("sl", station)),
        Trains.Select(BuildTrain));

    private static XElement BuildTrain(GvTrain train) => new("v",
        train.From == DateOnly.MinValue ? null : new XAttribute("o", train.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
        train.To == DateOnly.MaxValue ? null : new XAttribute("d", train.To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
        train.Variants.Select(variant => BuildVariant(train, variant)));

    private static XElement BuildVariant(GvTrain train, GvVariant variant) => new("v",
        Iniss2Xml.Optional("jd", variant.Days is { } days ? GvValidity.Text(train.From, train.To, days) : variant.Validity),
        Iniss2Xml.Optional("d", variant.Operator),
        Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)variant.Flags, VariantFlagKeys)),
        Iniss2Xml.Optional("p", variant.Note),
        variant.Stops.Select(BuildStop));

    private static XElement BuildStop(GvStop stop) => new("z",
        new XAttribute("c", stop.Station),
        stop.Arrival is { } arrival ? new XAttribute("p", arrival) : null,
        Iniss2Xml.Optional("cp", stop.ArrivalDay),
        stop.Departure is { } departure ? new XAttribute("o", departure) : null,
        Iniss2Xml.Optional("co", stop.DepartureDay),
        Iniss2Xml.Optional("f", Iniss2Xml.FlagText((int)stop.Flags, StopFlagKeys)),
        Iniss2Xml.Optional("dcv", stop.Number),
        Iniss2Xml.Optional("cv", stop.DisplayNumber),
        Iniss2Xml.Optional("l", stop.ReplacementLine),
        Iniss2Xml.Optional("dv", stop.TrainType),
        Iniss2Xml.Optional("nv", stop.TrainName),
        Iniss2Xml.Optional("ids", stop.Line));

    private static GvFile Parse(XElement root) =>
        new(root.Elements("s").Select(Number).ToList(), root.Elements("v").Select(ParseTrain).ToList())
        {
            Flags = Iniss2Xml.Number(root, "f"),
            Language = Iniss2Xml.Text(root, "l"),
            LimitedStations = root.Elements("sl").Select(Number).ToList()
        };

    private static int Number(XElement element) => XmlConvert.ToInt32(element.Value.Trim());

    private static GvTrain ParseTrain(XElement element) => new(
        ParseDate(element, "o", DateOnly.MinValue),
        ParseDate(element, "d", DateOnly.MaxValue),
        element.Elements("v").Select(ParseVariant).ToList());

    // vlak bez atributu d nema koniec platnosti
    private static DateOnly ParseDate(XElement element, string attribute, DateOnly missing) => element.Attribute(attribute) is { } value
        ? DateOnly.FromDateTime(XmlConvert.ToDateTime(value.Value, XmlDateTimeSerializationMode.Unspecified))
        : missing;

    private static GvVariant ParseVariant(XElement element) => new()
    {
        Validity = Iniss2Xml.Text(element, "jd"),
        Operator = Iniss2Xml.Text(element, "d"),
        Flags = (GvVariantFlags)Iniss2Xml.Flags(element, "f", VariantFlagKeys),
        Note = Iniss2Xml.Text(element, "p"),
        Stops = element.Elements("z").Select(ParseStop).ToList()
    };

    private static GvStop ParseStop(XElement element)
    {
        var arrival = Iniss2Xml.Number(element, "p", -1);
        var departure = Iniss2Xml.Number(element, "o", -1);

        return new GvStop
        {
            Station = Iniss2Xml.Number(element, "c"),
            Arrival = arrival >= 0 ? arrival : null,
            ArrivalDay = Iniss2Xml.Number(element, "cp"),
            Departure = departure >= 0 ? departure : null,
            DepartureDay = Iniss2Xml.Number(element, "co"),
            Flags = (GvStopFlags)Iniss2Xml.Flags(element, "f", StopFlagKeys),
            Number = Iniss2Xml.Text(element, "dcv"),
            DisplayNumber = Iniss2Xml.Text(element, "cv"),
            ReplacementLine = Iniss2Xml.Text(element, "l"),
            TrainType = Iniss2Xml.Text(element, "dv"),
            TrainName = Iniss2Xml.Text(element, "nv"),
            Line = Iniss2Xml.Text(element, "ids")
        };
    }
}
