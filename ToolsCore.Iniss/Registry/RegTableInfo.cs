namespace ToolsCore.Iniss.Registry;

/// <summary>Fyzicka tabula, na ktoru sa vztahuju hodnoty Tables\…&lt;N&gt;.</summary>
/// <param name="Name">nazov tabule (KEY z TPhysic)</param>
/// <param name="Grafikon">grafikon (priecinok), z ktoreho tabula pochadza</param>
/// <param name="Manufacturer">kod vyrobcu z katalogovej predlohy alebo null</param>
/// <param name="Line">cislo komunikacnej linky (COMUNICATION_PORT)</param>
public sealed record RegTableInfo(string Name, string Grafikon, int? Manufacturer, int Line);
