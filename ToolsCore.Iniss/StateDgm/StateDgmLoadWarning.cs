namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Upozornenie z nacitania (subor je platny, ale INISS by sa zachoval inak, nez autor cakal).
/// </summary>
/// <param name="Message">Text upozornenia.</param>
/// <param name="Line">Riadok (0-based), -1 ak sa neda urcit.</param>
public sealed record StateDgmLoadWarning(string Message, int Line);
