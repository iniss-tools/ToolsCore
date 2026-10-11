using ToolsCore.Iniss.Grafikon;
using ToolsCore.Iniss.Tables;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Tests.Tables;

/// <summary>
/// Data pre celo tabule: kolaje z Pozice_A.TXT a z katalogu druh tabule, stlpce zobrazene pri vlaku, rozmer a kolaje
/// pod tabulou cez logicke tabule.
/// </summary>
[TestClass]
public sealed class TableFacesTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "TableFacesTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    [TestMethod]
    public void Kolaje_TextNastupisteATabule()
    {
        File.WriteAllLines(Path.Combine(_dir, GvdFileConsts.FilePoziceA),
        [
            "; kolaje stanice",
            "/5512345",
            "\"N\",\"N\",\"Neznáma\",\"\",\"\",\"N\",\"\",\"\",0",
            "\"1\",\"1\",\"Koľaj 1\",\"Nástupište 1\",\"1A\",\"1\",\"0100\",\"01\",2,\"Nast1\",\"Podch1\",0,0",
            "\"2\",\"2\",\"Koľaj 2\",\"Nástupište 1\",\"\",\"1\",\"0200\",\"01\",1,\"Nast2\",0",
            "\"3\",\"3\",\"Koľaj 3\",\"Nástupište 2\",\"3\",\"2\",\"0300\"",
            "\"4\",\"4\",\"Koľaj 4\",\"Nástupište 2\",\"4\",\"2\",\"0400\",\"02\",x"
        ], Encodings.Win1250);

        var tracks = StationTracks.Read(_dir);

        // neznama kolaj, kratky riadok a riadok s chybnym poctom tabul sa preskocia; bez textu = kluc
        Assert.HasCount(2, tracks);
        Assert.AreEqual(new StationTrack("1", "1A", "1", tracks[0].Tables), tracks[0]);
        CollectionAssert.AreEqual(new[] { "Nast1", "Podch1" }, tracks[0].Tables.ToArray());
        Assert.AreEqual("2", tracks[1].Text);
        CollectionAssert.AreEqual(new[] { "Nast2" }, tracks[1].Tables.ToArray());
        Assert.IsEmpty(StationTracks.Read(Path.Combine(_dir, "chyba")));
    }

    [TestMethod]
    public void Celo_DruhStlpceAKolajeZLogickychTabul()
    {
        var catalog = new TableCatalog
        {
            Key = "Nast", Name = "Nástupištná", Comment = "", Manufacturer = TableManufacturer.Elen10, MaxRecCount = 1,
            Segments = [new TableSegment { Width = 270, Height = 10, Size = 15 }],
            Items =
            [
                Item("Cas", 0, 0, 26, TableFillSection.CasOdchodu, TableAlign.Right),
                Item("Typ", 0, 29, 45, TableFillSection.TypVlaku, TableAlign.Left),
                Item("Ciel", 0, 48, 136, TableFillSection.CielovaStanicaNastupiste, TableAlign.Left),
                Item("Text", 0, 0, 280, TableFillSection.TextLine1, TableAlign.Left),
                Item("Mesk", 0, 264, 280, TableFillSection.MeskanieOdchod, TableAlign.Right)
            ]
        };
        var tab = new TableViewTypeTab { ViewType = TableViewType.Nastupistna, CountLinesRecord = "1" };
        tab.TypeModeItems.Add(new TableTypeModeItem { ViewMode = TableViewMode.Vlak, ItemsKeys = ["Cas", "Typ", "Ciel", "Mesk"] });
        catalog.ViewTypeTabs.Add(tab);
        var table = new TablePhysical { Key = "Nastupistna 1", Name = "Nastupistna 1", Comment = "", ID = 1, TableCatalog = catalog };
        var logical = new TableLogical { Key = "Nast1", Name = "Nast1", Comment = "", ViewType = TableViewType.Nastupistna };
        logical.Records.Add(new TableRecord { Positions = [new TablePosition { Table = table, TypeView = TableViewType.Nastupistna }] });
        StationTrack[] tracks =
        [
            new("1", "1A", "1", ["Nast1"]), new("2", "1V", "1", ["Nast1"]), new("3", "3", "2", ["Nast3"]), new("5", "5", "N", ["Nast1"])
        ];

        var face = TableFaces.Build(table, [logical], tracks, "Stanica")!;

        Assert.AreEqual("Tabule_Nastupistni", face.ViewType);
        Assert.AreEqual(1, face.LinesPerRecord);
        Assert.AreEqual(1, face.Records);
        // stlpec siaha za sirku segmentu - sirka podla neho
        Assert.AreEqual(280, face.Width);
        Assert.AreEqual(10, face.RowHeight);
        CollectionAssert.AreEqual(new[] { "Cas", "Typ", "Ciel", "Mesk" }, face.Columns.Select(c => c.Name).ToArray());
        Assert.AreEqual(new FaceColumnInfo(0, 264, 280, 1, TableFillSection.MeskanieOdchod.Id, "Mesk"), face.Columns[^1]);
        CollectionAssert.AreEqual(new[] { "1A", "1V", "5" }, face.Tracks.ToArray());
        // nastupiste N (nezname) sa nepocita
        CollectionAssert.AreEqual(new[] { "1" }, face.Platforms.ToArray());
        Assert.AreEqual("Stanica", face.Station);

        Assert.IsNull(TableFaces.Build(new TablePhysical { Key = "Bez", Name = "Bez", Comment = "" }, [logical], tracks, "Stanica"));
    }

    private static TableItem Item(string key, int line, int start, int end, TableFillSection content, TableAlign align) => new()
    {
        Key = key, Name = key, FillSection = content, Line = line, Start = start, End = end, Align = align, DivType = TableDivType.Free,
        Tab1 = TableTabTab.Empty, Tab2 = TableTabTab.Empty
    };
}
