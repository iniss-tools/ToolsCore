namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stanica vzoru trasy.
/// </summary>
/// <param name="Station">cislo stanice</param>
/// <param name="Reported">stanica sa povie</param>
public sealed record ReportPatternStop(int Station, bool Reported);
