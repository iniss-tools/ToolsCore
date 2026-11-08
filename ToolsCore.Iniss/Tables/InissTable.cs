using ToolsCore.Iniss.Registry;

namespace ToolsCore.Iniss.Tables;

/// <summary>Fyzicka tabula v zozname, ktory INISS nacita zo vsetkych grafikonov.</summary>
/// <param name="Index">poradie v spojenom zozname (index &lt;N&gt; v registri Tables\…&lt;N&gt;)</param>
/// <param name="Grafikon">priecinok grafikonu</param>
/// <param name="Table">fyzicka tabula</param>
public sealed record InissTable(int Index, string Grafikon, TablePhysical Table)
{
    /// <summary>Kod vyrobcu z katalogovej predlohy alebo null.</summary>
    public int? Manufacturer => Table.TableCatalog?.Manufacturer?.Id;

    /// <summary>Udaj pre vyhodnotenie registra.</summary>
    public RegTableInfo ToInfo() => new(Table.Key, Grafikon, Manufacturer, Table.CommunicationPort);

    /// <summary>Co o cele tabule vedia data stanice (druh, stlpce, kolaje); null = tabula nema katalog.</summary>
    public TableFaceInfo? Face { get; init; }
}
