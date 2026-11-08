using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace ToolsCore.Iniss2.Grafikon;

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
