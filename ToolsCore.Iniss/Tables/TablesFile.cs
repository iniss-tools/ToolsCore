using System.Globalization;
using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;
using static ToolsCore.Iniss.Grafikon.GvdFileConsts;
using static ToolsCore.Iniss.Tools.ParseUtils;
using static ToolsCore.Iniss.Tools.PathUtils;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Tabule (TabTab.txt, TKatalog.txt, TPhysic.txt, TLogical.txt).
/// </summary>
public static class TablesFile
{
    private static string CheckKey(this ITable table, string key, string name, IEnumerable<ITable> items)
    {
        if (string.IsNullOrEmpty(key))
            throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_NoKey, table.TypeName, name));

        if (items.Any(item => item.Key == key))
            throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_DuplicateKey, table.TypeName, name, key));

        return key;
    }


    /// <summary>
    /// Inicializuje definície a vlastnosti tabul.
    /// </summary>
    /// <param name="path">cesta do priecinka s dátami.</param>
    public static (List<TableTabTab>, List<TableCatalog>, List<TablePhysical>, List<TableLogical>) Read(string path)
    {
        var fileTabTab = CombinePath(path, FileTabtab)!;
        var fileTCatalog = CombinePath(path, FileTkatalog)!;
        var fileTPhysical = CombinePath(path, FileTphysic)!;
        var fileTLogical = CombinePath(path, FileTlogical)!;

        var tabtabs = new List<TableTabTab>();
        var tcatalogs = new List<TableCatalog>();
        var tphysicals = new List<TablePhysical>();
        var tlogicals = new List<TableLogical>();

        //TABTABS
        var tabtabF = new TxtPropsAreas(fileTabTab);
        var p = 1;
        foreach (var area in tabtabF.GetAreas())
        {
            if (string.IsNullOrWhiteSpace(area) || area == "")
            {
                throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_TabTabInvalidName, p));
            }

            var tabTab = new TableTabTab { Key = area, Text = tabtabF.Get(area) ?? "" };
            p++;

            // prazdna sekcia [Ziadny] je pozostatok starsich verzii FTableCatalog (vkladal polozku Ziadny do zoznamu TabTab)
            if (tabTab.Key == TableTabTab.Empty.Key && string.IsNullOrWhiteSpace(tabTab.Text)) continue;

            tabtabs.Add(tabTab);
        }

        //TCATALOGS
        var catalogF = new TxtPropsAreasFields(fileTCatalog);
        var count = int.Parse(catalogF.Get("MAIN", "COUNT"), CultureInfo.InvariantCulture);
        for (var i = 0; i < count; i++)
        {
            var tcatalog = new TableCatalog();
            var ti = i + 1;

            var area = $"TABLE_{ti.PadZeros()}";
            tcatalog.Comment = catalogF.GetComment(area);

            tcatalog.Name = catalogF.Get(area, "NAME").AnsiToUTF();
            tcatalog.Key = tcatalog.CheckKey(catalogF.Get(area, "KEY").AnsiToUTF(), tcatalog.Name, tcatalogs);
            var manufacturer = catalogF.Get(area, "MANUFACTURER_KEY");
            var parsedManufacturer = TableManufacturer.Parse(manufacturer);
            if (parsedManufacturer == null)
            {
                throw new FormatException(
                    string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadManufacturer, tcatalog.Key, manufacturer));
            }

            tcatalog.Manufacturer = parsedManufacturer;

            tcatalog.MaxRecCount = ParseIntOrDefault(catalogF.Get(area, "MAX_REC_COUNT", false));
            tcatalog.MinHeight = ParseIntOrDefault(catalogF.Get(area, "MIN_HEIGHT", false));
            tcatalog.NumSegments = ParseIntOrDefault(catalogF.Get(area, "NUM_SEGMENTS", false), 1);

            for (var j = 0; j < ParseIntOrDefault(catalogF.Get(area, "COUNT_PHYSICAL_LINE_INFO", false)); j++)
            {
                var tj = j + 1;
                var segment = new TableSegment
                {
                    Height = ParseIntOrDefault(catalogF.Get(area, $"HEIGHT_{tj.PadZeros()}", false)),
                    Width = ParseIntOrDefault(catalogF.Get(area, $"WIDTH_{tj.PadZeros()}", false)),
                    Size = ParseIntOrDefault(catalogF.Get(area, $"SIZE_{tj.PadZeros()}", false))
                };
                tcatalog.Segments.Add(segment);
            }

            for (var j = 0; j < ParseIntOrDefault(catalogF.Get(area, "COUNT_TYPE_ITEMS", false)); j++)
            {
                var tj = j + 1;
                var padded = tj.PadZeros();

                var name = catalogF.Get(area, $"TYPE_ITEMS_NAME_{padded}").AnsiToUTF();
                var item = new TableItem
                {
                    Name = name,
                    Key = tcatalog.CheckKey(catalogF.Get(area, $"TYPE_ITEMS_KEY_{padded}").AnsiToUTF(), name, tcatalog.Items),
                    Line = ParseIntOrDefault(catalogF.Get(area, $"TYPE_ITEMS_LINE_{padded}", false)),
                    Start = ParseIntOrDefault(catalogF.Get(area, $"TYPE_ITEMS_START_{padded}", false)),
                    End = ParseIntOrDefault(catalogF.Get(area, $"TYPE_ITEMS_END_{padded}", false)),
                    FontIdx = ParseIntOrDefault(catalogF.Get(area, $"TYPE_ITEMS_FONT_IDX_{padded}", false))
                };

                var fillSection = catalogF.Get(area, $"TYPE_ITEMS_IDX_{padded}", false);
                var parsedFillSection = TableFillSection.Parse(ParseIntOrDefault(fillSection));

                var align = catalogF.Get(area, $"TYPE_ITEMS_ALIGN_{padded}", false);
                var parsedAlign = TableAlign.Parse(ParseIntOrDefault(align));

                var divType = catalogF.Get(area, $"TYPE_ITEMS_DIVTYPE_{padded}", false);
                var parsedDivType = TableDivType.Parse(ParseIntOrDefault(divType));

                if (parsedFillSection == null)
                {
                    throw new FormatException(
                        string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadFillSection, tcatalog.Key, item.Key, fillSection));
                }

                if (parsedAlign == null)
                {
                    throw new FormatException(
                        string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadAlign, tcatalog.Key, item.Key, align));
                }

                if (parsedDivType == null)
                {
                    throw new FormatException(
                        string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadDivType, tcatalog.Key, item.Key, align));
                }

                item.FillSection = parsedFillSection;
                item.Align = parsedAlign;
                item.DivType = parsedDivType;

                var tab1 = catalogF.Get(area, $"TYPE_ITEMS_TAB1_{padded}").AnsiToUTF();
                var tab2 = catalogF.Get(area, $"TYPE_ITEMS_TAB2_{padded}").AnsiToUTF();

                CheckTabTab(tab1, item, tabtabs, true, tcatalog.Key);
                CheckTabTab(tab2, item, tabtabs, false, tcatalog.Key);

                tcatalog.Items.Add(item);
            }

            for (var j = 0; j < ParseIntOrDefault(catalogF.Get(area, "COUNT_TYPE_VIEW_TAB", false)); j++)
            {
                var tj = j + 1;
                var typetab = new TableViewTypeTab();

                var viewType = catalogF.Get(area, $"TYPE_VIEW_TAB_KEY_{tj.PadZeros()}").AnsiToUTF();
                var parsedViewType = TableViewType.Parse(viewType);

                if (parsedViewType == null)
                {
                    throw new FormatException(
                        string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadViewType, tcatalog.Key, viewType));
                }

                typetab.ViewType = parsedViewType;

                typetab.CountLinesRecord = catalogF.Get(area, $"TYPE_VIEW_TAB_COUNT_LINES_RECORD_{tj.PadZeros()}");
                if (!IsInt(typetab.CountLinesRecord))
                {
                    throw new FormatException(
                        string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadLineCount, tcatalog.Key, typetab.ViewType.Key, typetab.CountLinesRecord));
                }

                var ck = ParseIntOrDefault(catalogF.Get(area, $"TYPE_VIEW_TAB_COUNT_TYPE_MODE_{tj.PadZeros()}", false));
                for (var k = 0; k < ck; k++)
                {
                    var tk = k + 1;
                    var ttmi = new TableTypeModeItem();

                    var viewMode = catalogF.Get(area, $"TYPE_MODE_KEY_{tj.PadZeros()}_{tk.PadZeros()}").AnsiToUTF();
                    var parsedViewMode = TableViewMode.Parse(viewMode);

                    if (parsedViewMode == null)
                    {
                        throw new FormatException(
                            string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadViewType, tcatalog.Key, viewMode));
                    }

                    ttmi.ViewMode = parsedViewMode;

                    var cl = ParseIntOrDefault(catalogF.Get(area, $"TYPE_MODE_COUNT_TYPE_ITEM_{tj.PadZeros()}_{tk.PadZeros()}", false));
                    for (var l = 0; l < cl; l++)
                    {
                        var tl = l + 1;
                        var itemKey = catalogF.Get(area, $"TYPE_ITEM_{tj.PadZeros()}_{tk.PadZeros()}_{tl.PadZeros(2)}").AnsiToUTF();

                        string? tolist = null;
                        foreach (var item in tcatalog.Items)
                            if (item.Key == itemKey)
                                tolist = itemKey;

                        if (!string.IsNullOrEmpty(tolist))
                            ttmi.ItemsKeys.Add(itemKey);
                        else
                            throw new FormatException(
                                string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_BadColumn, tcatalog.Key, typetab.ViewType.Key, ttmi.ViewMode.Key, itemKey));
                    }

                    typetab.TypeModeItems.Add(ttmi);
                }

                tcatalog.ViewTypeTabs.Add(typetab);
            }

            tcatalogs.Add(tcatalog);
        }

        //TPHYSIC
        var tphysicF = new TxtPropsAreasFields(fileTPhysical);
        var countp = int.Parse(tphysicF.Get("MAIN", "COUNT"), CultureInfo.InvariantCulture);
        for (var i = 0; i < countp; i++)
        {
            var tphysical = new TablePhysical();
            var ti = i + 1;

            var area = $"TABLE_{ti.PadZeros()}";
            tphysical.Comment = tphysicF.GetComment(area);

            tphysical.Name = tphysicF.Get(area, "NAME").AnsiToUTF();
            tphysical.Key = tphysical.CheckKey(tphysicF.Get(area, "KEY").AnsiToUTF(), tphysical.Name, tphysicals);
            tphysical.ID = ParseIntOrDefault(tphysicF.Get(area, "ID", false));
            tphysical.CommunicationPort = ParseIntOrDefault(tphysicF.Get(area, "COMUNICATION_PORT", false));
            tphysical.RecCount = ParseIntOrDefault(tphysicF.Get(area, "REC_COUNT", false));
            tphysical.Rem = tphysicF.Get(area, "REM").AnsiToUTF();
            tphysical.ReverseArrows = ParseStringOrDefault(tphysicF.Get(area, "REVERSE_ARROWS", false)).AnsiToUTF();
            tphysical.SaveXML = tphysicF.Get(area, "SAVE_XML", "").AnsiToUTF();

            var catname = tphysicF.Get(area, "CATALOG_KEY").AnsiToUTF();
            AssignCatalogToPhysical(tcatalogs, tphysical, catname);
            if (tphysical.TableCatalog == null)
                throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_CatalogMissing, catname, tphysical.Key));

            tphysicals.Add(tphysical);
        }

        //TLOGICAL
        var tlogicF = new TxtPropsAreasFields(fileTLogical);
        var countl = int.Parse(tlogicF.Get("MAIN", "COUNT"), CultureInfo.InvariantCulture);
        for (var i = 0; i < countl; i++)
        {
            var tlLogical = new TableLogical();
            var ti = i + 1;

            var area = $"TABLE_{ti.PadZeros()}";
            tlLogical.Comment = tlogicF.GetComment(area);

            tlLogical.Name = tlogicF.Get(area, "NAME").AnsiToUTF();
            tlLogical.Key = tlLogical.CheckKey(tlogicF.Get(area, "KEY").AnsiToUTF(), tlLogical.Name, tlogicals);
            tlLogical.TypeViewFlags = tlogicF.Get(area, "TYPE_VIEW_FLAGS").AnsiToUTF();
            tlLogical.IdStation = ParseIntOrDefault(tlogicF.Get(area, "IDSTATION", false));
            var viewtype = tlogicF.Get(area, "TYPE_VIEW").AnsiToUTF();
            var parsedViewType = TableViewType.Parse(viewtype);
            if (parsedViewType == null)
            {
                throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_LogicalBadViewType, tlLogical.Key, viewtype));
            }

            tlLogical.ViewType = parsedViewType;

            for (var j = 0; j < ParseIntOrDefault(tlogicF.Get(area, "COUNT_REC", false)); j++)
            {
                var trecord = new TableRecord();
                var tj = j + 1;

                for (var k = 0; k < ParseIntOrDefault(tlogicF.Get(area, $"COUNT_POS_{tj.PadZeros()}", false)); k++)
                {
                    var tposition = new TablePosition();
                    var tk = k + 1;

                    tposition.Position = int.Parse(tlogicF.Get(area, $"POSITION_{tj.PadZeros()}_{tk.PadZeros()}"), CultureInfo.InvariantCulture);
                    var tv = tlogicF.Get(area, $"TYPE_VIEW_KEY_{tj.PadZeros()}_{tk.PadZeros()}").AnsiToUTF();
                    var parsedTypeView = TableViewType.Parse(tv);
                    if (parsedTypeView == null)
                    {
                        throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_LogicalBadViewKey, tlLogical.Key, tj.PadZeros(), tk.PadZeros(), tv));
                    }

                    tposition.TypeView = parsedTypeView;

                    var fyzname = tlogicF.Get(area, $"PHYSICAL_KEY_{tj.PadZeros()}_{tk.PadZeros()}").AnsiToUTF();
                    AssignPhysicalToPosition(tphysicals, tposition, fyzname);

                    if (tposition.Table == null)
                        throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_LogicalPhysicalMissing, tlLogical.Key, fyzname));

                    trecord.Positions.Add(tposition);
                }

                tlLogical.Records.Add(trecord);
            }

            tlogicals.Add(tlLogical);
        }

        return (tabtabs,tcatalogs,tphysicals,tlogicals);
    }

    private static void CheckTabTab(string tabkey, TableItem item, IEnumerable<TableTabTab> tabtabs, bool tab1, string catalogKey)
    {
        if (!string.IsNullOrEmpty(tabkey))
        {
            var tab = tabtabs.FirstOrDefault(tabtab => tabkey == tabtab.Key);

            if (tab == null)
                throw new FormatException(
                    string.Format(CultureInfo.CurrentCulture, Resources.TablesFile_TabTabMissing, catalogKey, item.Key, (tab1 ? "TABTAB1" : "TABTAB2"), tabkey));
                
            if (tab1) item.Tab1 = tab;
            else item.Tab2 = tab;
        }
        else
        {
            if (tab1) item.Tab1 = TableTabTab.Empty;
            else item.Tab2 = TableTabTab.Empty;
        }
    }

    private static void AssignPhysicalToPosition(IEnumerable<TablePhysical> tphysicals, TablePosition tposition, string fyzname)
    {
        foreach (var physical in tphysicals)
        {
            if (physical.Key != fyzname) continue;
            tposition.Table = physical;
            break;
        }
    }

    private static void AssignCatalogToPhysical(IEnumerable<TableCatalog> tcatalogs, TablePhysical tphysical, string catname)
    {
        foreach (var tableCatalog in tcatalogs)
        {
            if (tableCatalog.Key != catname) continue;
            tphysical.TableCatalog = tableCatalog;
            break;
        }
    }

    /// <summary>
    /// Zapise data o tabuliach do suborov.
    /// </summary>
    /// <param name="path">Cesta do priecinka s datami.</param>
    /// <param name="tabTabs">TabTabs.</param>
    /// <param name="catalogs">Katalogove tabule.</param>
    /// <param name="physicals">Fyzicke tabule.</param>
    /// <param name="logicals">Logicke tabule.</param>
    public static void Write(string path, IEnumerable<TableTabTab> tabTabs, IList<TableCatalog> catalogs, IList<TablePhysical> physicals, IList<TableLogical> logicals)
    {
        var fileTabTab = CombinePath(path, FileTabtab)!;
        var fileTCatalog = CombinePath(path, FileTkatalog)!;
        var fileTPhysical = CombinePath(path, FileTphysic)!;
        var fileTLogical = CombinePath(path, FileTlogical)!;

        //TABTABS - uvodne komentare suboru (pred prvou sekciou) sa zachovaju
        var tabtabF = new TxtPropsAreas(fileTabTab, true);
        if (File.Exists(fileTabTab))
            tabtabF.Preamble = new TxtPropsAreas(fileTabTab).Preamble;
        foreach (var tabTab in tabTabs)
        {
            // polozka Ziadny patri len do vyberu v okne, nie do suboru
            if (tabTab == TableTabTab.Empty) continue;
            tabtabF.Set(tabTab.Key, tabTab.Text);
        }
        tabtabF.Save();

        //TCATALOG
        var catalogF = new TxtPropsAreasFields(fileTCatalog, true);
        catalogF.Set("MAIN", "COUNT", catalogs.Count);
        for (var i = 0; i < catalogs.Count; i++)
        {
            var tcatalog = catalogs[i];
            var ti = i + 1;

            var area = $"TABLE_{ti.PadZeros()}";
            catalogF.SetComment(area, tcatalog.Comment);

            catalogF.Set(area, "KEY", tcatalog.Key, WriteType.WriteStringANSI);
            catalogF.Set(area, "NAME", tcatalog.Name, WriteType.WriteStringANSI);
            catalogF.Set(area, "MANUFACTURER_KEY", tcatalog.Manufacturer.Name, WriteType.WriteStringANSI);
            catalogF.Set(area, "MAX_REC_COUNT", tcatalog.MaxRecCount);
            catalogF.Set(area, "MIN_HEIGHT", tcatalog.MinHeight);
            catalogF.Set(area, "NUM_SEGMENTS", tcatalog.NumSegments);

            catalogF.Set(area, "COUNT_PHYSICAL_LINE_INFO", tcatalog.Segments.Count);
            for (var j = 0; j < tcatalog.Segments.Count; j++)
            {
                var segment = tcatalog.Segments[j];
                var tj = j + 1;

                catalogF.Set(area, $"HEIGHT_{tj.PadZeros()}", segment.Height);
                catalogF.Set(area, $"WIDTH_{tj.PadZeros()}", segment.Width);
                catalogF.Set(area, $"SIZE_{tj.PadZeros()}", segment.Size);
            }

            catalogF.Set(area, "COUNT_TYPE_ITEMS", tcatalog.Items.Count);
            for (var j = 0; j < tcatalog.Items.Count; j++)
            {
                var item = tcatalog.Items[j];
                var tj = j + 1;

                catalogF.Set(area, $"TYPE_ITEMS_KEY_{tj.PadZeros()}", item.Key, WriteType.WriteStringANSI);
                catalogF.Set(area, $"TYPE_ITEMS_NAME_{tj.PadZeros()}", item.Name, WriteType.WriteStringANSI);
                catalogF.Set(area, $"TYPE_ITEMS_IDX_{tj.PadZeros()}", item.FillSection.Id);
                catalogF.Set(area, $"TYPE_ITEMS_LINE_{tj.PadZeros()}", item.Line);
                catalogF.Set(area, $"TYPE_ITEMS_START_{tj.PadZeros()}", item.Start);
                catalogF.Set(area, $"TYPE_ITEMS_END_{tj.PadZeros()}", item.End);
                catalogF.Set(area, $"TYPE_ITEMS_FONT_IDX_{tj.PadZeros()}", item.FontIdx);
                catalogF.Set(area, $"TYPE_ITEMS_ALIGN_{tj.PadZeros()}", item.Align.Id);
                catalogF.Set(area, $"TYPE_ITEMS_DIVTYPE_{tj.PadZeros()}", item.DivType.Id);
                catalogF.Set(area, $"TYPE_ITEMS_TAB1_{tj.PadZeros()}", item.Tab1 == TableTabTab.Empty ? "" : item.Tab1.Key,
                    WriteType.WriteStringANSI);
                catalogF.Set(area, $"TYPE_ITEMS_TAB2_{tj.PadZeros()}", item.Tab2 == TableTabTab.Empty ? "" : item.Tab2.Key,
                    WriteType.WriteStringANSI);
            }

            catalogF.Set(area, "COUNT_TYPE_VIEW_TAB", tcatalog.ViewTypeTabs.Count);
            for (var j = 0; j < tcatalog.ViewTypeTabs.Count; j++)
            {
                var typetab = tcatalog.ViewTypeTabs[j];
                var tj = j + 1;

                catalogF.Set(area, $"TYPE_VIEW_TAB_KEY_{tj.PadZeros()}", typetab.ViewType.Key, WriteType.WriteStringANSI);
                catalogF.Set(area, $"TYPE_VIEW_TAB_COUNT_LINES_RECORD_{tj.PadZeros()}", typetab.CountLinesRecord, WriteType.WriteStringANSI);
                catalogF.Set(area, $"TYPE_VIEW_TAB_COUNT_TYPE_MODE_{tj.PadZeros()}", typetab.TypeModeItems.Count);
                for (var k = 0; k < typetab.TypeModeItems.Count; k++)
                {
                    var ttmi = typetab.TypeModeItems[k];
                    var tk = k + 1;

                    catalogF.Set(area, $"TYPE_MODE_KEY_{tj.PadZeros()}_{tk.PadZeros()}", ttmi.ViewMode.Key, WriteType.WriteStringANSI);
                    catalogF.Set(area, $"TYPE_MODE_COUNT_TYPE_ITEM_{tj.PadZeros()}_{tk.PadZeros()}", ttmi.ItemsKeys.Count);
                    for (var l = 0; l < ttmi.ItemsKeys.Count; l++)
                    {
                        var tl = l + 1;
                        catalogF.Set(area, $"TYPE_ITEM_{tj.PadZeros()}_{tk.PadZeros()}_{tl.PadZeros(2)}", ttmi.ItemsKeys[l],
                            WriteType.WriteStringANSI);
                    }
                }
            }
        }
        catalogF.Save();

        //TPHYSIC
        var tphysicF = new TxtPropsAreasFields(fileTPhysical, true);
        tphysicF.Set("MAIN", "COUNT", physicals.Count);
        for (var i = 0; i < physicals.Count; i++)
        {
            var tphysical = physicals[i];
            var ti = i + 1;

            var area = $"TABLE_{ti.PadZeros()}";
            tphysicF.SetComment(area, tphysical.Comment);

            tphysicF.Set(area, "KEY", tphysical.Key, WriteType.WriteStringANSI);
            tphysicF.Set(area, "NAME", tphysical.Name, WriteType.WriteStringANSI);
            tphysicF.Set(area, "ID", tphysical.ID);
            tphysicF.Set(area, "COMUNICATION_PORT", tphysical.CommunicationPort);
            tphysicF.Set(area, "REC_COUNT", tphysical.RecCount);
            tphysicF.Set(area, "REM", tphysical.Rem, WriteType.WriteStringANSINullable);
            tphysicF.Set(area, "REVERSE_ARROWS", tphysical.ReverseArrows, WriteType.WriteStringANSINullable);
            tphysicF.Set(area, "SAVE_XML", tphysical.SaveXML, WriteType.WriteStringANSI);
            tphysicF.Set(area, "CATALOG_KEY", tphysical.TableCatalog.Key, WriteType.WriteStringANSI);
        }
        tphysicF.Save();

        //TLOGICAL
        var tlogicF = new TxtPropsAreasFields(fileTLogical, true);
        tlogicF.Set("MAIN", "COUNT", logicals.Count);
        for (var i = 0; i < logicals.Count; i++)
        {
            var tlLogical = logicals[i];
            var ti = i + 1;

            var area = $"TABLE_{ti.PadZeros()}";
            tlogicF.SetComment(area, tlLogical.Comment);

            tlogicF.Set(area, "KEY", tlLogical.Key, WriteType.WriteStringANSI);
            tlogicF.Set(area, "NAME", tlLogical.Name, WriteType.WriteStringANSI);
            tlogicF.Set(area, "TYPE_VIEW", tlLogical.ViewType.Key, WriteType.WriteStringANSI);
            tlogicF.Set(area, "TYPE_VIEW_FLAGS", tlLogical.TypeViewFlags, WriteType.WriteStringANSINullable);
            if (tlLogical.IdStation != 0)
                tlogicF.Set(area, "IDSTATION", tlLogical.IdStation);
            tlogicF.Set(area, "COUNT_REC", tlLogical.Records.Count);

            for (var j = 0; j < tlLogical.Records.Count; j++)
            {
                var trecord = tlLogical.Records[j];
                var tj = j + 1;

                tlogicF.Set(area, $"COUNT_POS_{tj.PadZeros()}", trecord.Positions.Count);
                for (var k = 0; k < trecord.Positions.Count; k++)
                {
                    var tposition = trecord.Positions[k];
                    var tk = k + 1;

                    tlogicF.Set(area, $"POSITION_{tj.PadZeros()}_{tk.PadZeros()}", tposition.Position);
                    tlogicF.Set(area, $"TYPE_VIEW_KEY_{tj.PadZeros()}_{tk.PadZeros()}", tposition.TypeView.Key,
                        WriteType.WriteStringANSI);
                    tlogicF.Set(area, $"PHYSICAL_KEY_{tj.PadZeros()}_{tk.PadZeros()}", tposition.Table.Key,
                        WriteType.WriteStringANSI);
                }
            }
        }
        tlogicF.Save();
    }
}
