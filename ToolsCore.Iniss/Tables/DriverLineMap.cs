namespace ToolsCore.Iniss.Tables;

/// <summary>Linky konfiguracie a priradenie fyzickych tabul k nim.</summary>
/// <param name="Lines">linky v poradi, v akom ich INISS zaklada</param>
/// <param name="Unserved">tabule s adresou, ktorym INISS nic neposle</param>
public sealed record DriverLineMap(IReadOnlyList<DriverLine> Lines, IReadOnlyList<UnservedTable> Unserved);
