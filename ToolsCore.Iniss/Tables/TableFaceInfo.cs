namespace ToolsCore.Iniss.Tables;

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
