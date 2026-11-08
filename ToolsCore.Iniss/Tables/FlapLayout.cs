namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Moduly listovej tabule a zoznamy listov (kod → text INISSu) odvodene z katalogovej tabule a jej prekodovacich tabuliek.
/// </summary>
/// <param name="LinesPerRecord">riadkov tabule na jeden zaznam</param>
/// <param name="Records">pocet zaznamov (MAX_REC_COUNT)</param>
/// <param name="Positions">pocet pozicii v riadku (koniec najvzdialenejsieho stlpca)</param>
/// <param name="Modules">moduly v poradi riadku a pozicie</param>
/// <param name="Lists">zoznamy listov podla mena sekcie TabTab: kod listu → text</param>
public sealed record FlapLayout(
    int LinesPerRecord,
    int Records,
    int Positions,
    IReadOnlyList<FlapModuleInfo> Modules,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Lists);
