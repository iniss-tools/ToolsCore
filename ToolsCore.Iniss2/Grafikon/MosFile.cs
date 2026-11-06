using System.Collections;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Zaznam motoroveho osobneho vlaku v stanici.
/// </summary>
/// <param name="Number">cislo vlaku</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
public sealed record MosTrain(string Number, string Validity);

/// <summary>
/// Motorove osobne vlaky stanice - subor MOS.xml v priecinku stanice.
/// </summary>
public static class MosFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "MOS.xml";

    /// <summary>Druh osobneho vlaku.</summary>
    internal const string PassengerType = "Os";

    /// <summary>Druh motoroveho osobneho vlaku.</summary>
    internal const string MotorType = "MOs";

    /// <summary>
    /// Nacita zaznamy v poradi suboru. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<MosTrain> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("v")
            .Select(element => new MosTrain(Iniss2Xml.Text(element, "cv"), Iniss2Xml.Text(element, "jd")))
            .ToList();
    }

    /// <summary>
    /// Uplatni zaznamy na vlaky ako server: usek vlaku s druhom Os, ktoreho cislo je v MOS.xml niektorej zaujmovej stanice
    /// useku, dostane druh MOs. Ak zaznam plati len v cast dni varianty, varianta sa rozdeli a nova ide hned za povodnu.
    /// </summary>
    /// <param name="trains">vlaky s druhom MOs uz prepisanym na Os</param>
    /// <param name="stations">zaznamy podla cisla stanice</param>
    internal static List<GvTrain> Apply(IReadOnlyList<GvTrain> trains, Dictionary<int, List<MosTrain>> stations)
    {
        if (stations.Values.All(records => records.Count == 0))
            return [.. trains];

        var ranges = new Dictionary<int, Dictionary<string, (DateOnly From, DateOnly To)>>();
        return trains.Select(train => Apply(train, trains, stations, ranges)).ToList();
    }

    private static GvTrain Apply(GvTrain train, IReadOnlyList<GvTrain> trains, Dictionary<int, List<MosTrain>> stations,
        Dictionary<int, Dictionary<string, (DateOnly From, DateOnly To)>> ranges)
    {
        var variants = train.Variants.ToList();

        // nova varianta ide za povodnu a prejde sa tiez, jej usek uz ma druh MOs a preskoci sa
        for (var i = 0; i < variants.Count; i++)
        {
            var start = -1;

            for (var index = 0; index < variants[i].Stops.Count; index++)
            {
                var stop = variants[i].Stops[index];
                if (stop.Number.Length > 0)
                    start = stop.TrainType == PassengerType ? index : -1;

                if (start < 0 || !stop.IsInterest || !stations.TryGetValue(stop.Station, out var records) || ReferenceDay(stop, index) is not { } day)
                    continue;

                var number = variants[i].Stops[start].Number;
                if (records.Find(record => record.Number == number) is not { } record)
                    continue;

                if (!ranges.TryGetValue(stop.Station, out var stationRanges))
                    ranges[stop.Station] = stationRanges = GvFile.TrainRanges(trains, stop.Station);
                if (!stationRanges.TryGetValue(number, out var range))
                    continue;

                var (inside, outside) = Split(train.DaysOf(variants[i]), train.From, GvValidity.Days(range.From, range.To, record.Validity), range.From, day);

                if (!outside.HasAnySet())
                {
                    variants[i] = Motor(variants[i], start);
                }
                else if (inside.HasAnySet())
                {
                    variants.Insert(i + 1, Motor(variants[i], start) with { Days = inside });
                    variants[i] = variants[i] with { Days = outside };
                }
            }
        }

        return train with { Variants = variants };
    }

    // den, od ktoreho sa rata den zaznamu: vo vychodzej stanici odchod, inde prichod; null = bod bez casu
    private static int? ReferenceDay(GvStop stop, int index)
    {
        if (index == 0 && stop.Departure.HasValue)
            return stop.DepartureDay;

        if (stop.Arrival.HasValue)
            return stop.ArrivalDay;

        return stop.Departure.HasValue ? stop.DepartureDay : null;
    }

    // zaznam plati v dni stanice, varianta v dni vlaku - lisia sa o pocet dni od zaciatku jazdy
    private static (BitArray Inside, BitArray Outside) Split(BitArray days, DateOnly from, BitArray recordDays, DateOnly recordFrom, int day)
    {
        var inside = new BitArray(days.Length);
        var outside = new BitArray(days.Length);
        var shift = from.DayNumber + day - recordFrom.DayNumber;

        for (var i = 0; i < days.Length; i++)
        {
            if (!days[i])
                continue;

            var index = i + shift;
            if (index >= 0 && index < recordDays.Length && recordDays[index])
                inside[i] = true;
            else
                outside[i] = true;
        }

        return (inside, outside);
    }

    private static GvVariant Motor(GvVariant variant, int start)
    {
        var stops = variant.Stops.ToArray();
        stops[start] = stops[start] with { TrainType = MotorType };
        return variant with { Stops = stops };
    }
}
