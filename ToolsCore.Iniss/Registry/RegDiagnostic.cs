namespace ToolsCore.Iniss.Registry;

/// <summary>Zistenie o konfiguracii (text v jazyku UI).</summary>
/// <param name="Code">druh</param>
/// <param name="Severity">zavaznost</param>
/// <param name="Section">sekcia</param>
/// <param name="Name">hodnota alebo null pri celej sekcii</param>
/// <param name="Message">text</param>
public sealed record RegDiagnostic(RegDiagnosticCode Code, RegSeverity Severity, string Section, string? Name, string Message);
