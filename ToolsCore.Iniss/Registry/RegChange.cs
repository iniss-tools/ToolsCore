namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Zmena jedneho nastavenia.
/// </summary>
/// <param name="Section">sekcia (konkretna, napr. <c>Driver3</c>)</param>
/// <param name="Name">nazov hodnoty</param>
/// <param name="Type">typ hodnoty</param>
/// <param name="Value">nova hodnota (int, string, byte[]); null = odstranit zo vsetkych vrstiev (obnovit predvolenu)</param>
/// <param name="Target">kam zapisat</param>
public sealed record RegChange(string Section, string Name, RegValueType Type, object? Value, RegWriteTarget Target);
