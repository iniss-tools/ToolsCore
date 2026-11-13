using System.Collections;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Grafikon jednej stanice: zastavenia vlakov z GV.XML a ich kolaje.
/// </summary>
public sealed class StationTimetable
{
    private readonly IReadOnlyList<TrainPosition> _positions;
    private readonly Dictionary<string, (DateOnly From, DateOnly To)> _ranges;
    private readonly Dictionary<TrainPosition, BitArray> _positionDays = [];

    private StationTimetable(DirListGroup group, DirListItem station, GvFile gv, IReadOnlyList<TrackDefinition> tracks, IReadOnlyList<TrainPosition> positions)
    {
        Group = group;
        Station = station;
        Tracks = tracks;
        _positions = positions;
        _ranges = gv.TrainRanges(station.Station);
        Calls = gv.Trains
            .SelectMany(train => train.Variants.SelectMany(variant => variant.Stops
                .Select((stop, index) => (stop, index))
                .Where(item => item.stop.Station == station.Station && item.stop.IsInterest)
                .Select(item => new StationCall(train, variant, item.index))))
            .ToList();
    }

    /// <summary>
    /// Grafikon, do ktoreho stanica patri.
    /// </summary>
    public DirListGroup Group { get; }

    /// <summary>
    /// Stanica.
    /// </summary>
    public DirListItem Station { get; }

    /// <summary>
    /// Zastavenia vlakov v stanici v poradi suboru GV.XML.
    /// </summary>
    public IReadOnlyList<StationCall> Calls { get; }

    /// <summary>
    /// Kolaje stanice.
    /// </summary>
    public IReadOnlyList<TrackDefinition> Tracks { get; }

    /// <summary>
    /// Nacita grafikon stanice.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="group">grafikon</param>
    /// <param name="station">stanica grafikonu</param>
    public static StationTimetable Read(string dataDir, DirListGroup group, DirListItem station)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(station);

        return Read(dataDir, group, station, GvFile.Load(Path.Combine(dataDir, group.Name)));
    }

    /// <summary>
    /// Grafikon stanice z vlakov v pamati (napr. s neulozenymi zmenami): pohlad servera sa z nich zostavi rovnako ako pri
    /// nacitani suboru, kolaje a MOS.xml su zo suborov stanic.
    /// </summary>
    /// <param name="dataDir">priecinok dat servera</param>
    /// <param name="group">grafikon</param>
    /// <param name="station">stanica grafikonu</param>
    /// <param name="timetable">vlaky grafikonu tak, ako su v subore (<see cref="GvFile.Load" />)</param>
    public static StationTimetable Read(string dataDir, DirListGroup group, DirListItem station, GvFile timetable)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(station);
        ArgumentNullException.ThrowIfNull(timetable);

        var groupDir = Path.Combine(dataDir, group.Name);
        var mos = new Dictionary<int, List<MosTrain>>();
        foreach (var item in group.Items)
            mos.TryAdd(item.Station, MosFile.Read(Path.Combine(groupDir, item.Directory)));
        var server = timetable.ForServer(group.Items.Select(item => item.Station).ToHashSet(), mos);

        // PositionsDef.xml server v kazdej stanici nevyzaduje
        var stationDir = Path.Combine(groupDir, station.Directory);
        var tracks = File.Exists(Path.Combine(stationDir, PositionsDefFile.FileName)) ? PositionsDefFile.Read(stationDir) : [];
        return new StationTimetable(group, station, server, tracks, PositionsFile.Read(stationDir));
    }

    /// <summary>
    /// Najde kolaj vlaku v dany den ako server: prvy zaznam s cislom vlaku, ktory v ten den plati. Poznamka zaznamu sa
    /// rozvinie do obdobia, ked vlak s tym cislom stanicou ide.
    /// </summary>
    /// <param name="call">zastavenie vlaku</param>
    /// <param name="run">den jazdy vlaku</param>
    /// <returns>zaznam alebo <see langword="null" />, ak vlak v ten den kolaj nema.</returns>
    public TrainPosition? PositionFor(StationCall call, DateOnly run)
    {
        ArgumentNullException.ThrowIfNull(call);

        var number = call.DepartureNumber;
        if (!_ranges.TryGetValue(number, out var range))
            return null;

        var day = call.DateAtStation(run);
        if (day < range.From || day > range.To)
            return null;

        return _positions.FirstOrDefault(position => position.Train == number && DaysOf(position, range)[day.DayNumber - range.From.DayNumber]);
    }

    /// <summary>
    /// Zastavenia vlakov v stanici v dany den ako ich vidi server: v den jazdy plati prva varianta vlaku, ktora v ten den ide.
    /// Zoradene podla casu v stanici.
    /// </summary>
    /// <param name="day">den v stanici</param>
    public List<StationDayCall> On(DateOnly day)
    {
        var active = new Dictionary<(GvTrain, GvVariant), BitArray>();
        var calls = new List<StationDayCall>();

        foreach (var call in Calls)
        {
            var offset = call.DateAtStation(call.Train.From).DayNumber - call.Train.From.DayNumber;
            var run = day.AddDays(-offset);
            if (run < call.Train.From || run > call.Train.To)
                continue;

            if (!active.TryGetValue((call.Train, call.Variant), out var days))
                active[(call.Train, call.Variant)] = days = call.Train.ActiveDaysOf(call.Variant);

            var index = run.DayNumber - call.Train.From.DayNumber;
            if (index < days.Length && days[index])
                calls.Add(new StationDayCall(call, run, PositionFor(call, run)));
        }

        return [.. calls.OrderBy(call => call.Minute)];
    }

    private BitArray DaysOf(TrainPosition position, (DateOnly From, DateOnly To) range)
    {
        if (!_positionDays.TryGetValue(position, out var days))
            _positionDays[position] = days = GvValidity.Days(range.From, range.To, position.Validity);
        return days;
    }
}
