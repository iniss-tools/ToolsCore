using System.Collections;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Zastavenie vlaku v stanici - bod trasy jednej varianty.
/// </summary>
/// <param name="Train">vlak</param>
/// <param name="Variant">varianta vlaku</param>
/// <param name="Index">index bodu v trase varianty</param>
public sealed record StationCall(GvTrain Train, GvVariant Variant, int Index)
{
    /// <summary>
    /// Bod trasy.
    /// </summary>
    public GvStop Stop => Variant.Stops[Index];

    /// <summary>
    /// Vlak v stanici zacina.
    /// </summary>
    public bool IsFirst => Index == 0;

    /// <summary>
    /// Vlak v stanici konci.
    /// </summary>
    public bool IsLast => Index == Variant.Stops.Count - 1;

    /// <summary>
    /// Cislo vlaku pri prichode - moze sa lisit od cisla pri odchode.
    /// </summary>
    public string ArrivalNumber => Variant.NumberSource(Index, true)?.Number ?? "";

    /// <summary>
    /// Cislo vlaku pri odchode. Podla neho server hlada aj kolaj vlaku.
    /// </summary>
    public string DepartureNumber => Variant.NumberSource(Index, false)?.Number ?? "";

    /// <summary>
    /// Cislo, ktorym sa vlak v stanici oznacuje: cislo pri odchode, v cielovej stanici cislo pri prichode.
    /// </summary>
    public string Number => Source?.Number ?? "";

    /// <summary>
    /// Druh vlaku k <see cref="Number" />.
    /// </summary>
    public string TrainType => Source?.TrainType ?? "";

    /// <summary>
    /// Nazov vlaku k <see cref="Number" />.
    /// </summary>
    public string TrainName => Source?.TrainName ?? "";

    private GvStop? Source => Variant.NumberSource(Index, IsLast);

    /// <summary>
    /// Den, ktorym server vlak v stanici oznacuje: vo vychodzej stanici den odchodu, inde den prichodu.
    /// </summary>
    /// <param name="run">den jazdy vlaku</param>
    public DateOnly DateAtStation(DateOnly run) => IsFirst ? Stop.DepartureDate(run) : Stop.ArrivalDate(run);
}

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

        var stationDir = Path.Combine(dataDir, group.Name, station.Directory);
        return new StationTimetable(group, station, GvFile.Read(dataDir, group), PositionsDefFile.Read(stationDir), PositionsFile.Read(stationDir));
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

    private BitArray DaysOf(TrainPosition position, (DateOnly From, DateOnly To) range)
    {
        if (!_positionDays.TryGetValue(position, out var days))
            _positionDays[position] = days = GvValidity.Days(range.From, range.To, position.Validity);
        return days;
    }
}
