using ToolsCore.Iniss.TabTab;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Modul listovej tabule: na riadku zaznamu <paramref name="Line" /> zaberie pozicie od <paramref name="Position" />
/// (znaky spravy, jeden znak = jeden kod listu); kody <paramref name="Span" /> pozicii spolu urcuju list zo zoznamu
/// <paramref name="List" />.
/// </summary>
/// <param name="Line">riadok v ramci zaznamu (TYPE_ITEMS_LINE, od 0)</param>
/// <param name="Position">prva pozicia (START / 8)</param>
/// <param name="Span">pocet pozicii</param>
/// <param name="List">meno zoznamu listov (sekcia TabTab); null = kod sa ukaze, ako prisiel</param>
public sealed record FlapModuleInfo(int Line, int Position, int Span, string? List);

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

/// <summary>
/// Listove tabule (ERS, FERS) dostavaju od INISSu kody listov - text z grafikonu na ne prelozia prekodovacie tabulky
/// stlpcov (TabTab, jednoduche pravidla <c>kod = text</c>). Tu sa preklad obrati, aby sa dal kod listu ukazat ako text:
/// stlpec s <c>DIVTYPE</c> 1 alebo 3 je jeden modul cez vsetky svoje pozicie so zoznamom TAB1, cas (2) tri moduly
/// (hodiny TAB1, desiatky a jednotky minut TAB2), znak po znaku (4) modul na kazdu poziciu s TAB1 a text bez
/// prekodovania (0) modul na kazdu poziciu bez zoznamu.
/// </summary>
public static class FlapLayouts
{
    /// <summary>Sirka znaku stlpca v bodoch (START a END su v bodoch, pozicie v znakoch).</summary>
    public const int CharWidth = 8;

    /// <summary>Tabula je listova (vyrobca ERS alebo FERS).</summary>
    public static bool IsFlapBoard(TableManufacturer? manufacturer) => manufacturer == TableManufacturer.Ers || manufacturer == TableManufacturer.Fers;

    /// <summary>Moduly a zoznamy listov katalogovej tabule.</summary>
    public static FlapLayout Build(TableCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var modules = new List<FlapModuleInfo>();
        var lists = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);

        string? List(TableTabTab? tab)
        {
            if (tab is null || ReferenceEquals(tab, TableTabTab.Empty) || string.IsNullOrWhiteSpace(tab.Key))
                return null;
            if (!lists.ContainsKey(tab.Key))
                lists[tab.Key] = Reverse(tab.Text);
            return tab.Key;
        }

        foreach (var item in catalog.Items)
        {
            var position = Math.Max(0, item.Start / CharWidth);
            var span = Math.Max(1, (item.End - item.Start) / CharWidth);
            var divType = item.DivType?.Id ?? 0;
            switch (divType)
            {
                case 1 or 3:
                    modules.Add(new FlapModuleInfo(item.Line, position, span, List(item.Tab1)));
                    break;
                case 2 when span == 3:
                    modules.Add(new FlapModuleInfo(item.Line, position, 1, List(item.Tab1)));
                    modules.Add(new FlapModuleInfo(item.Line, position + 1, 1, List(item.Tab2)));
                    modules.Add(new FlapModuleInfo(item.Line, position + 2, 1, List(item.Tab2)));
                    break;
                case 4:
                {
                    var list = List(item.Tab1);
                    for (var i = 0; i < span; i++)
                        modules.Add(new FlapModuleInfo(item.Line, position + i, 1, list));
                    break;
                }

                case 2:
                    modules.Add(new FlapModuleInfo(item.Line, position, span, null));
                    break;
                default:
                    for (var i = 0; i < span; i++)
                        modules.Add(new FlapModuleInfo(item.Line, position + i, 1, null));
                    break;
            }
        }

        // prekryte stlpce (chyba predlohy) - plati prvy
        var distinct = new List<FlapModuleInfo>();
        foreach (var module in modules.OrderBy(m => m.Line).ThenBy(m => m.Position))
        {
            if (!distinct.Any(m => m.Line == module.Line && m.Position < module.Position + module.Span && module.Position < m.Position + m.Span))
                distinct.Add(module);
        }

        var lines = distinct.Count == 0 ? 1 : distinct.Max(m => m.Line) + 1;
        var positions = distinct.Count == 0 ? 0 : distinct.Max(m => m.Position + m.Span);
        return new FlapLayout(lines, Math.Max(1, catalog.MaxRecCount), positions, distinct, lists);
    }

    /// <summary>
    /// Obratena prekodovacia tabulka: jednoduche pravidla <c>kod = text</c> (bez udalosti a bez <c>@</c>); pri viacerych
    /// textoch pre jeden kod plati prvy.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Reverse(string sectionText)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rule in TabTabSection.Parse(sectionText).Rules)
        {
            if (rule.Event != TabTabEventKind.None)
                continue;
            var code = TabTabText.Decode(rule.Left).Text;
            var text = TabTabText.Decode(rule.Right).Text;
            if (code.Length == 0 || code.Contains('@') || text.Contains('@'))
                continue;
            map.TryAdd(code, text);
        }

        return map;
    }
}
