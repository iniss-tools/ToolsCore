using ToolsCore.Iniss.Tools;
using static ToolsCore.Iniss.Tools.ParseUtils;

namespace ToolsCore.Iniss.Grafikon;

/// <summary>
/// Kolaj stanice z <c>Pozice_A.TXT</c> - co z nej potrebuju tabule: text kolaje, nastupiste a logicke tabule nad nou.
/// </summary>
/// <param name="Key">kluc kolaje (1. pole)</param>
/// <param name="Text">text kolaje pre tabule (5. pole); prazdny = kluc</param>
/// <param name="Platform">kluc nastupista (6. pole)</param>
/// <param name="Tables">kluce logickych tabul nad kolajou</param>
public sealed record StationTrack(string Key, string Text, string Platform, IReadOnlyList<string> Tables);

/// <summary>Kolaje stanice z <c>Pozice_A.TXT</c> grafikonu (bez nastupist a zvukov, ktore cita GVDEditor).</summary>
public static class StationTracks
{
    /// <summary>Kluc neznamej kolaje - na tabuli nema cislo.</summary>
    private const string Unknown = "N";

    /// <summary>Najmenej poli riadku - kratsi riadok INISS zahodi.</summary>
    private const int MinFields = 9;

    /// <summary>Kolaje grafikonu v poradi suboru; subor chyba = ziadne, chybny riadok sa preskoci (ako v INISSe).</summary>
    public static IReadOnlyList<StationTrack> Read(string grafikonDir)
    {
        var file = Path.Combine(grafikonDir, GvdFileConsts.FilePoziceA);
        if (!File.Exists(file))
            return [];

        var tracks = new List<StationTrack>();
        using var reader = new CsvFileReader(file);
        var row = new CsvRow();
        while (true)
        {
            var status = reader.ReadRow(row);
            if (LineIsEof(status))
                break;
            if (LineIsEmpty(status) || row.Count < MinFields || !int.TryParse(row[8].Trim(), out var count) || count < 0)
                continue;
            var key = row[0].Trim();
            if (key.Length == 0 || key == Unknown)
                continue;
            var tables = Enumerable.Range(0, count).Select(i => row.ElementAtOrDefault(9 + i)?.Trim() ?? "").Where(t => t.Length > 0).ToList();
            var text = row[4].Trim();
            tracks.Add(new StationTrack(key, text.Length > 0 ? text : key, row[5].Trim(), tables));
        }

        return tracks;
    }
}
