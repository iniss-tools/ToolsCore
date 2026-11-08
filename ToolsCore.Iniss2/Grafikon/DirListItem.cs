namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Stanica grafikonu.
/// </summary>
/// <param name="Directory">priecinok stanice v priecinku grafikonu</param>
/// <param name="Station">cislo stanice SR70</param>
/// <param name="TableLine">linka tabul; prazdna = urcuje ju tabula</param>
/// <param name="AudioLine">linka zvuku</param>
/// <param name="Flags">druh stanice</param>
public sealed record DirListItem(string Directory, int Station, string TableLine, string AudioLine, DirListItemFlags Flags);
