namespace ToolsCore.Iniss.Tables;

/// <summary>Tabula, ktorej INISS nic neposle, a dovod.</summary>
/// <param name="Table">tabula</param>
/// <param name="Problem">dovod</param>
public sealed record UnservedTable(InissTable Table, DriverLineProblem Problem)
{
    /// <summary>Dovod v jazyku UI.</summary>
    public string Reason => Problem.Text;
}
