using ToolsCore.Iniss.Grafikon;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Stlpec katalogovej tabule, ktory sa ukazuje pri vlaku - nad nim byva na cele tabule natlaceny popis.
/// </summary>
/// <param name="Line">riadok zaznamu (TYPE_ITEMS_LINE, od 0)</param>
/// <param name="Start">zaciatok stlpca v bodoch</param>
/// <param name="End">koniec stlpca v bodoch (prvy bod za stlpcom)</param>
/// <param name="Align">zarovnanie (TYPE_ITEMS_ALIGN: 0 vlavo, 1 vpravo, 2 na stred)</param>
/// <param name="Content">druh obsahu (TYPE_ITEMS_IDX - cislo FILL_SECTION)</param>
/// <param name="Name">nazov stlpca pre obsluhu</param>
public sealed record FaceColumnInfo(int Line, int Start, int End, int Align, int Content, string Name);

/// <summary>
/// Co o cele fyzickej tabule vedia data stanice: druh tabule, riadky zaznamu a sirka riadku, stlpce zobrazene pri
/// vlaku, kolaje a nastupistia, nad ktorymi tabula stoji, a nazov stanice.
/// </summary>
/// <param name="ViewType">druh tabule (kluc VIEW_TYPE, napr. <c>Tabule_Nastupistni</c>); prazdny = neznamy</param>
/// <param name="LinesPerRecord">riadkov tabule na jeden zaznam</param>
/// <param name="Records">zaznamov na tabuli (MAX_REC_COUNT)</param>
/// <param name="Width">sirka riadku tabule v bodoch - z katalogu, alebo koniec najvzdialenejsieho stlpca; 0 = neznama</param>
/// <param name="RowHeight">vyska riadku (segmentu) v bodoch z katalogu; 0 = neznama</param>
/// <param name="Columns">stlpce zobrazene pri vlaku</param>
/// <param name="Tracks">texty kolaji, nad ktorymi tabula stoji (cez logicke tabule v Pozice_A)</param>
/// <param name="Platforms">nastupistia tychto kolaji</param>
/// <param name="Station">nazov stanice grafikonu</param>
public sealed record TableFaceInfo(
    string ViewType,
    int LinesPerRecord,
    int Records,
    int Width,
    int RowHeight,
    IReadOnlyList<FaceColumnInfo> Columns,
    IReadOnlyList<string> Tracks,
    IReadOnlyList<string> Platforms,
    string Station);

/// <summary>
/// Fakty o cele tabule z dat stanice. Druh tabule je z logickych tabul, ktore ju pouzivaju (najcastejsi), inak
/// z katalogu; stlpce su tie, ktore katalog vypisuje v rezime <c>LVM_Vlak</c> (inak vsetky s obsahom); kolaje su
/// tie, nad ktorymi stoji niektora z tychto logickych tabul.
/// </summary>
public static class TableFaces
{
    /// <summary>Fakty o cele tabule; null = tabula nema katalogovu predlohu.</summary>
    public static TableFaceInfo? Build(TablePhysical table, IReadOnlyList<TableLogical> logicals, IReadOnlyList<StationTrack> tracks, string station)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(logicals);
        ArgumentNullException.ThrowIfNull(tracks);
        if (table.TableCatalog is not { } catalog)
            return null;

        bool IsThis(TablePosition position) => ReferenceEquals(position.Table, table) || string.Equals(position.Table?.Key, table.Key, StringComparison.Ordinal);

        var used = logicals.Where(l => l.Records.Any(r => r.Positions.Any(IsThis))).ToList();
        var viewType = used.SelectMany(l => l.Records).SelectMany(r => r.Positions).Where(IsThis)
                           .Select(p => p.TypeView?.Key).OfType<string>().Where(k => k.Length > 0)
                           .GroupBy(k => k, StringComparer.Ordinal).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault()
                       ?? catalog.ViewTypeTabs.Select(t => t.ViewType?.Key).OfType<string>().FirstOrDefault() ?? "";
        var tab = catalog.ViewTypeTabs.FirstOrDefault(t => string.Equals(t.ViewType?.Key, viewType, StringComparison.Ordinal)) ?? catalog.ViewTypeTabs.FirstOrDefault();
        var lines = int.TryParse(tab?.CountLinesRecord?.Trim(), out var n) && n > 0 ? n : 1;

        var shown = tab?.TypeModeItems.FirstOrDefault(m => m.ViewMode == TableViewMode.Vlak)?.ItemsKeys.ToHashSet(StringComparer.Ordinal);
        var columns = catalog.Items
            .Where(i => shown is null ? i.FillSection != TableFillSection.NotDefined : shown.Contains(i.Key))
            .Select(i => new FaceColumnInfo(i.Line, i.Start, i.End, i.Align?.Id ?? 0, i.FillSection?.Id ?? 0, i.Name ?? ""))
            .ToList();

        var keys = used.Select(l => l.Key).ToHashSet(StringComparer.Ordinal);
        var under = tracks.Where(t => t.Tables.Any(keys.Contains)).ToList();
        // stlpce mozu siahat za sirku segmentu (tabula je sirsia, nez katalog uvadza)
        var segment = catalog.Segments.FirstOrDefault();
        var width = Math.Max(segment?.Width ?? 0, columns.Count > 0 ? columns.Max(c => c.End) : 0);
        return new TableFaceInfo(viewType, lines, Math.Max(1, catalog.MaxRecCount), width, segment?.Height ?? 0, columns,
            under.Select(t => t.Text).Distinct(StringComparer.Ordinal).ToList(),
            under.Select(t => t.Platform).Where(p => p.Length > 0 && p != "N").Distinct(StringComparer.Ordinal).ToList(),
            station);
    }
}
