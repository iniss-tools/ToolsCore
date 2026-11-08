namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Kolaj vlaku v stanici.
/// </summary>
/// <param name="Train">cislo vlaku</param>
/// <param name="ArrivalTrack">kluc kolaje pri prichode</param>
/// <param name="DepartureTrack">kluc kolaje pri odchode; bez nej plati kolaj prichodu</param>
/// <param name="Validity">datumova poznamka; prazdna = kazdy den, ked vlak stanicou ide</param>
public sealed record TrainPosition(string Train, string ArrivalTrack, string DepartureTrack, string Validity);
