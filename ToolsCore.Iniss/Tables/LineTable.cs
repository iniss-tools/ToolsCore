namespace ToolsCore.Iniss.Tables;

/// <summary>Tabula priradena linke.</summary>
/// <param name="Table">tabula</param>
/// <param name="Automatic">priradena automaticky (COMUNICATION_PORT=0)</param>
public sealed record LineTable(InissTable Table, bool Automatic);
