namespace ToolsCore.Mcp;

/// <summary>
/// Stav servera MCP programu pre stranku nastaveni.
/// </summary>
public interface IMcpServerStatus
{
    /// <summary>Adresa beziaceho servera; null, ak nebezi.</summary>
    string? Address { get; }

    /// <summary>Preco sa server posledny raz nespustil (napr. obsadeny port); null = bez chyby.</summary>
    string? Error { get; }

    /// <summary>Nastavenie, s ktorym server bezi (alebo sa ho pokusil spustit).</summary>
    McpSettings? Settings { get; }
}
