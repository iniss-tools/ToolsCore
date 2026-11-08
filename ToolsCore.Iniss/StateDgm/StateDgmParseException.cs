namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Chyba syntaxe suboru StateDgm.txt (INISS by nacitanie diagramu prerusil).
/// </summary>
public sealed class StateDgmParseException(string message, int line) : Exception(message)
{
    /// <summary>Cislo riadka (0-based), na ktorom sa chyba nasla.</summary>
    public int Line { get; } = line;
}
