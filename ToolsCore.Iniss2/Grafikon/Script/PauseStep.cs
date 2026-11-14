namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Ticho: <c>Pauza(200)</c>. Plati v kazdom jazyku hlasenia.
/// </summary>
/// <param name="Milliseconds">dlzka v ms</param>
public sealed record PauseStep(int Milliseconds) : ScriptStep;
