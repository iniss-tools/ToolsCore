namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Zakomentovany riadok tela: vypnuty krok (napr. <c>'Zvuk("D/Vety/bezpec")</c>) alebo poznamka. Server ho nevykona.
/// </summary>
/// <param name="Text">text za apostrofom</param>
public sealed record CommentStep(string Text) : ScriptStep;
