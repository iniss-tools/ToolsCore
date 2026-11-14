namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Parameter vstavanej metody skriptu hlaseni podla servera.
/// </summary>
/// <param name="Name">nazov parametra ako v servere (pre pomenovany argument <c>nazov:=hodnota</c>)</param>
/// <param name="Kind">co parameter ocakava</param>
/// <param name="Optional">parameter ma predvolenu hodnotu</param>
public sealed record ScriptParameter(string Name, ScriptParameterKind Kind, bool Optional = false);
