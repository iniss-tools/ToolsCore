namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Udaj predlohy, v ktorom sa texty sady prejavia.
/// </summary>
/// <param name="Catalog">kluc predlohy z TCatalog.xml</param>
/// <param name="Column">kluc udaja predlohy</param>
public sealed record TextTarget(string Catalog, string Column);
