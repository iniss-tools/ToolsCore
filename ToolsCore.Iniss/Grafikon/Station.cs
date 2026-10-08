using ToolsCore.Iniss.Entities;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Trieda reprezentujuca stanicu/zastávku, v ktorej može zastaviť vlak.
/// </summary>
/// <param name="ID">Identifikátor stanice.</param>
/// <param name="Name">Názov stanice.</param>
/// <param name="IsInShortReport">Ci sa bude hlásiť v krátkom hlásení.</param>
/// <param name="IsInLongReport">Ci sa bude hlásiť v dlhom hlásení.</param>
/// <param name="IsCustom">Ci stanica nepochadza zo zvukovej banky ale zo suboru Stanice.txt.</param>
/// <remarks>
/// Hodnotovy objekt - kazdy vlak ma v trase vlastne kopie (<see cref="CopyRoute" />) a trasy sa porovnavaju
/// hodnotou (<see cref="SequencesEqual" />). Priznaky hlasenia sa upravuju na mieste v tabulke trasy, preto sa
/// stanica nesmie pouzivat ako kluc v <see cref="HashSet{T}" /> alebo <see cref="Dictionary{TKey,TValue}" />.
/// </remarks>
public sealed record Station(string ID, string Name, bool IsInShortReport = false, bool IsInLongReport = false, bool IsCustom = false) : IComparable
{
    /// <summary>
    /// Predvolena (nedefinovana) stanica. Zakazdym nova instancia - stanica v trase sa upravuje na mieste
    /// (priznaky hlasenia), zdielana instancia by sa tak zmenila vsetkym.
    /// </summary>
    public static Station None => new("0000000", "None");

    /// <summary>
    /// Identifikátor stanice.
    /// </summary>
    public string ID { get; set; } = ID;

    /// <summary>
    /// Názov stanice.
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// Stanica sa bude hlásiť v krátkom hlásení.
    /// </summary>
    public bool IsInShortReport { get; set; } = IsInShortReport;

    /// <summary>
    /// Stanica sa bude hlásiť v dlhom hlásení.
    /// </summary>
    public bool IsInLongReport { get; set; } = IsInLongReport;

    /// <summary>
    /// Je použiváteľom definovaná stanica (zo súboru STANICE.TXT).
    /// </summary>
    public bool IsCustom { get; set; } = IsCustom;

    /// <inheritdoc />
    public int CompareTo(object? obj) => string.Compare(Name, obj?.ToString(), StringComparison.Ordinal);

    /// <summary>
    /// Vrati stanicu zo zvukovej banky alebo zo stanic grafikonu podla identifikatora stanice.
    /// </summary>
    /// <param name="id">Identifikator stanice.</param>
    /// <param name="stations">stanice zo zvukovej banky</param>
    /// <param name="customStations">stanice definovane v grafikone (Stanice.txt)</param>
    /// <returns>Nova instancia stanice. Ak nenaslo ziadnu zhodu, vrati stanicu s nazvom zadaneho ID.</returns>
    public static Station GetFromID(string? id, IEnumerable<Station> stations, IEnumerable<Station> customStations)
    {
        if (string.IsNullOrEmpty(id))
            return None;

        var stationWithSameId = stations.FirstOrDefault(station => station.ID == id)
                                ?? customStations.FirstOrDefault(customStation => customStation.ID == id);

        return stationWithSameId is not null ? new Station(stationWithSameId.ID, stationWithSameId.Name) : new Station(id, id);
    }

    /// <summary>
    /// Vráti stanice dostupné zo zvukovej banky (prehľadáva sa skupina s kľúčom R1).
    /// </summary>
    /// <remarks>Číslo stanice je kľúč zvuku - INISS hľadá zvuky v skupine podľa kľúča, nie podľa názvu.</remarks>
    /// <param name="sounds">zvuky zakladneho jazyka zvukovej banky</param>
    /// <returns>list staníc.</returns>
    public static List<Station> GetStations(IEnumerable<FyzSound> sounds)
    {
        return sounds
            .Where(soundE => soundE.Group.Key.EqualsIgnoreCase("R1"))
            .Select(soundE => new Station(soundE.Key, soundE.Text.Replace(",", "")))
            .ToList();
    }

    /// <summary>
    /// Skopíruje trasu vlaku.
    /// </summary>
    /// <param name="stations">list staníc.</param>
    /// <returns>skopírovaná trasa vlaku.</returns>
    public static List<Station> CopyRoute(IEnumerable<Station> stations) 
        => stations.Select(station => new Station(station)).ToList();

    /// <summary>
    /// Porovná stanice podľa názvu staníc.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool EqualsName(string name) => !string.IsNullOrEmpty(name) && name == Name;

    /// <summary>
    /// Porovná zoznamy staníc vo všetkých vlastnostiach.
    /// </summary>
    /// <param name="st1">list staníc 1</param>
    /// <param name="st2">list staníc 2</param>
    /// <returns></returns>
    public static bool SequencesEqual(List<Station> st1, List<Station> st2)
    {
        if (st1.Count == st2.Count)
            return !st1.Where((station, i) => station != st2[i]).Any();

        return false;
    }

    /// <summary>
    /// Zistí, sa v liste staníc nachádza stanica so zadaným názvom stanice.
    /// </summary>
    /// <param name="stations"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool ContainsName(IEnumerable<Station> stations, string name) 
        => !string.IsNullOrEmpty(name) && stations.Any(station => station.Name == name);

    /// <inheritdoc />
    public override string ToString() => Name;

    public static bool operator <(Station left, Station right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Station left, Station right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Station left, Station right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Station left, Station right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}