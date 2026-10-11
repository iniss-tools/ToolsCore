using ToolsCore.Iniss.Grafikon;
using ToolsCore.Iniss.Registry;
using ToolsCore.Iniss.Tools;

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

/// <summary>
/// Fyzicke tabule instalacie v poradi, v akom ich INISS 3.39 sklada do jedneho zoznamu: riadky DirList.TXT
/// v poradi suboru, bez komentarov a bez grafikonov s priznakom M (bez Categori), z kazdej stanice len prvy grafikon;
/// tabule kazdeho grafikonu v poradi TPhysic.TXT. Index v tomto zozname je &lt;N&gt; v hodnotach Tables\…&lt;N&gt;.
/// </summary>
public static class InissTableMap
{
    /// <summary>Zostavi zoznam z grafikonov instalacie; grafikon, ktory sa neda precitat, sa preskoci.</summary>
    public static IReadOnlyList<InissTable> Build(IEnumerable<DirList> dirs)
    {
        var result = new List<InissTable>();
        var stations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in dirs)
        {
            if (dir.DirName.TrimStart().StartsWith(';')) continue;
            if (DirListFlags.Parse(dir.Flags).TrainCreation == DirListTrainCreation.CreateWithoutCategori) continue;
            try
            {
                var station = InfoGvdFile.Read(dir.FullPath).ThisStation;
                if (!stations.Add(station.ID)) continue;
                var (_, _, physicals, logicals) = TablesFile.Read(dir.FullPath);
                var tracks = ReadTracks(dir.FullPath);
                foreach (var table in physicals)
                    result.Add(new InissTable(result.Count, dir.IsDataRoot ? "DATA" : dir.DirName, table)
                    {
                        Face = TableFaces.Build(table, logicals, tracks, station.Name)
                    });
            }
            // chybny subor grafikonu (aj chybajuca povinna vlastnost - ArgumentException) sa preskoci, ostatne grafikony sa nacitaju
            catch (Exception e) when (e is IOException or FormatException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
            {
                Log.Exception(e);
            }
        }

        return result;
    }

    /// <summary>Kolaje grafikonu; bez nich (chybny subor) len tabule bez cisla kolaje.</summary>
    private static IReadOnlyList<StationTrack> ReadTracks(string dir)
    {
        try
        {
            return StationTracks.Read(dir);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Log.Exception(e);
            return [];
        }
    }
}
