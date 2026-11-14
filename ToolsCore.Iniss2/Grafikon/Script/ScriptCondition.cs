namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Podmienka bloku krokov: vyraz VB.NET typu Boolean tak, ako je v kode, alebo vetva <c>Else</c> tej istej podmienky.
/// Vyraz moze odkazovat len na clenov skriptu a servera (funkcia hlasenia nema premenne).
/// </summary>
/// <param name="Expression">vyraz za <c>If</c>, napr. <c>VlakS.JeProjizdejici</c></param>
/// <param name="Else">vetva <c>Else</c> podmienky <paramref name="Expression" /></param>
public sealed record ScriptCondition(string Expression, bool Else = false);
