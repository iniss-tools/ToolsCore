using ToolsCore.Iniss.Tables;

namespace ToolsCore.Iniss.Tests.Tables;

/// <summary>
/// Moduly a zoznamy listov listovej tabule z katalogu: stlpce podla DIVTYPE, obrateny preklad TabTab.
/// </summary>
[TestClass]
public sealed class FlapLayoutsTests
{
    private static TableTabTab Tab(string key, string text) => new() { Key = key, Text = text };

    private static TableItem Item(int line, int start, int end, TableDivType divType, TableTabTab? tab1 = null, TableTabTab? tab2 = null) => new()
    {
        Key = "S" + start, Name = "S" + start, FillSection = TableFillSection.Free, Line = line, Start = start, End = end, Align = TableAlign.Left,
        DivType = divType, Tab1 = tab1 ?? TableTabTab.Empty, Tab2 = tab2 ?? TableTabTab.Empty
    };

    [TestMethod]
    public void Moduly_PodlaSposobuPlneniaStlpca()
    {
        var ciel = Tab("CielFERS", "A=BRATISLAVA\r\nB=*BRATISLAVA\r\nA=Bratislava hl.st.\r\n(Typ(Typ_R)), \"*@\" = #SWITCH\r\n{@}=-{@}\r\n");
        var hodiny = Tab("Hodiny", "G=6\r\nH=7\r\n");
        var minuty = Tab("Minuty", "0=0\r\n1=1\r\n5=5\r\n");
        var catalog = new TableCatalog
        {
            Key = "Odch", Name = "Odchody", Comment = "", Manufacturer = TableManufacturer.Fers, MaxRecCount = 10, Segments = [], ViewTypeTabs = [],
            Items =
            [
                Item(0, 0, 8, TableDivType.Table, ciel),
                Item(0, 16, 24, TableDivType.Translate, Tab("Druh", "R=R\r\nO=Os\r\n")),
                Item(0, 24, 48, TableDivType.TableTime, hodiny, minuty),
                Item(0, 48, 64, TableDivType.Free),
                Item(1, 0, 16, TableDivType.Char, minuty)
            ]
        };

        var layout = FlapLayouts.Build(catalog);

        CollectionAssert.AreEqual(new[]
        {
            new FlapModuleInfo(0, 0, 1, "CielFERS"), new FlapModuleInfo(0, 2, 1, "Druh"), new FlapModuleInfo(0, 3, 1, "Hodiny"),
            new FlapModuleInfo(0, 4, 1, "Minuty"), new FlapModuleInfo(0, 5, 1, "Minuty"), new FlapModuleInfo(0, 6, 1, null),
            new FlapModuleInfo(0, 7, 1, null), new FlapModuleInfo(1, 0, 1, "Minuty"), new FlapModuleInfo(1, 1, 1, "Minuty")
        }, layout.Modules.ToArray());
        Assert.AreEqual(2, layout.LinesPerRecord);
        Assert.AreEqual(10, layout.Records);
        Assert.AreEqual(8, layout.Positions);

        // obrateny preklad: kod → text, pri dvoch textoch prvy; pravidla s udalostou a s @ sa vynechaju
        var list = layout.Lists["CielFERS"];
        Assert.HasCount(2, list);
        Assert.AreEqual("BRATISLAVA", list["A"]);
        Assert.AreEqual("*BRATISLAVA", list["B"]);
        Assert.AreEqual("6", layout.Lists["Hodiny"]["G"]);
        Assert.IsTrue(FlapLayouts.IsFlapBoard(TableManufacturer.Fers));
        Assert.IsFalse(FlapLayouts.IsFlapBoard(TableManufacturer.Lcd1));
    }

    [TestMethod]
    public void Preklad_KodVUvodzovkachAPrazdnyText()
    {
        var list = FlapLayouts.Reverse("\" A\"=PRAHA\r\n\"0\"=\r\n; komentar\r\nZ=ŽILINA\r\n");

        Assert.AreEqual("PRAHA", list[" A"]);
        Assert.AreEqual("", list["0"]);
        Assert.AreEqual("ŽILINA", list["Z"]);
    }
}
