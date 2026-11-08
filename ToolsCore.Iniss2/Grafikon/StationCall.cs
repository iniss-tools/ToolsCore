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
