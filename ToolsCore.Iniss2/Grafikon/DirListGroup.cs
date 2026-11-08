namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Grafikon - priecinok v datach servera so spolocnym GV.XML a priecinkami stanic.
/// </summary>
/// <param name="Name">nazov priecinka grafikonu</param>
/// <param name="Items">stanice grafikonu</param>
public sealed record DirListGroup(string Name, IReadOnlyList<DirListItem> Items);
