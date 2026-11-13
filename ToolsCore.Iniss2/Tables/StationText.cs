namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Text stanice v TTStanice.xml (napr. nazov cielovej stanice na tabuli).
/// </summary>
/// <param name="Station">cislo stanice</param>
/// <param name="Text">text</param>
/// <param name="Font">cislo pisma; <see cref="TrainTextSet.NoFont" /> = pismo predlohy</param>
public sealed record StationText(int Station, string Text, int Font);
