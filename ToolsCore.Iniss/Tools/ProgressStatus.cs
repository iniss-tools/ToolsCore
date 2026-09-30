namespace ToolsCore.Iniss.Tools;

/// <summary>
/// Priebeh dlhej operacie na pozadi (napr. citanie banky zvukov).
/// </summary>
/// <param name="ProgressPartName">Nazov prave vykonavanej casti.</param>
/// <param name="TotalProgress">Pocet krokov casti; 0 = neurcity priebeh.</param>
/// <param name="Value">Pocet hotovych krokov.</param>
public sealed record ProgressStatus(string ProgressPartName, int TotalProgress, int Value = 0);
