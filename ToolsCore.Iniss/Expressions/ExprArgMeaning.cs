// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Vyznam argumentu funkcie - pouziva sa pri semantickej kontrole a napovede.
/// </summary>
public enum ExprArgMeaning
{
    None,
    TrainNumber,
    Position,
    TrainType,
    Flags,
    State,
    StationId,
    TrackName,
    OperatorName,
    LineName,
    TrainName
}
