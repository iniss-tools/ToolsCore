namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Zastavenie vlaku v stanici v konkretny den.
/// </summary>
/// <param name="Call">zastavenie</param>
/// <param name="Run">den jazdy vlaku (referencny den, ku ktoremu sa pripocitavaju dni bodov trasy)</param>
/// <param name="Position">kolaj vlaku v ten den; <see langword="null" /> = vlak kolaj nema</param>
public sealed record StationDayCall(StationCall Call, DateOnly Run, TrainPosition? Position)
{
    /// <summary>
    /// Cas v stanici v minutach od polnoci dna v stanici: prichod, vo vychodzej stanici odchod.
    /// </summary>
    public int Minute => (Call.IsFirst ? Call.Stop.Departure : Call.Stop.Arrival ?? Call.Stop.Departure) ?? 0;
}
