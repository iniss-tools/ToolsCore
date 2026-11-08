namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Udaje o vlaku, ktore evaluator potrebuje. Casy a meskania su v sekundach; funkcie jazyka
/// ich prevadzaju na minuty rovnako ako INISS (celociselne delenie 60).
/// </summary>
public interface IExprTrainContext
{
    /// <summary>Text cisla vlaku (napr. <c>"1234"</c>); <c>CVLAKU</c> z neho cita vedúce cislice.</summary>
    string TrainNumber { get; }

    /// <summary>Pozicia vlaku: 1 vychozi, 2 prechadzajuci, 3 konciaci.</summary>
    int Position { get; }

    /// <summary>Index druhu vlaku v zabudovanej tabulke (0-94).</summary>
    int TrainTypeIndex { get; }

    /// <summary>Slovo priznakov vlaku (bity <c>Prizn_*</c>; najnizsi bajt su ine priznaky).</summary>
    uint Flags { get; }

    /// <summary>Stavove bity vlaku (<c>Stav_*</c>).</summary>
    uint State { get; }

    /// <summary>Ci ma vlak odklon.</summary>
    bool IsDeflected { get; }

    /// <summary>Meskanie prichodu v sekundach.</summary>
    int ArrivalDelaySeconds { get; }

    /// <summary>Meskanie odchodu v sekundach.</summary>
    int DepartureDelaySeconds { get; }

    /// <summary>Doba pobytu v sekundach (s meskaniami).</summary>
    int StayTimeSeconds { get; }

    /// <summary>Planovana doba pobytu v sekundach.</summary>
    int PlannedStayTimeSeconds { get; }

    /// <summary>ID stanice zaujmu.</summary>
    int HomeStationId { get; }

    /// <summary>ID vychodiskovej stanice.</summary>
    int BaseStationId { get; }

    /// <summary>ID cielovej stanice.</summary>
    int EndStationId { get; }

    /// <summary>Ci je vlak miestny.</summary>
    bool IsLocal { get; }

    /// <summary>Ci vlak prichadza zo smeru danej stanice.</summary>
    bool IsFromStation(int stationId);

    /// <summary>Ci vlak odchadza smerom na danu stanicu.</summary>
    bool IsToStation(int stationId);

    /// <summary>Nazov kolaje prichodu; <c>""</c>, ak nie je.</summary>
    string ArrivalTrack { get; }

    /// <summary>Nazov kolaje odchodu; <c>""</c>, ak nie je.</summary>
    string DepartureTrack { get; }

    /// <summary>Datum prichodu.</summary>
    DateOnly ArrivalDate { get; }

    /// <summary>Datum odchodu.</summary>
    DateOnly DepartureDate { get; }

    /// <summary>Cas prichodu.</summary>
    TimeOnly ArrivalTime { get; }

    /// <summary>Cas odchodu.</summary>
    TimeOnly DepartureTime { get; }

    /// <summary>Nazov dopravcu.</summary>
    string OperatorName { get; }

    /// <summary>Linka na prichode.</summary>
    string ArrivalLine { get; }

    /// <summary>Linka na odchode.</summary>
    string DepartureLine { get; }

    /// <summary>Nazov vlaku.</summary>
    string TrainName { get; }
}
