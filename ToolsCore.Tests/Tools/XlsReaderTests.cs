using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Citanie .xlsx a .xls cez ExcelDataReader - subory vytvoril Excel (data od B2, cislo, cas, datum, text).
/// </summary>
[TestClass]
public class XlsReaderTests
{
    [TestMethod]
    [DataRow("Vlaky.xlsx")]
    [DataRow("Vlaky.xls")]
    public void Citanie_ObsahOdPrvejNeprazdnejBunky(string file)
    {
        using var reader = new XlsReader(Path.Combine(AppContext.BaseDirectory, "TestData", "Excel", file));

        Assert.AreEqual(3, reader.RowCount);
        Assert.AreEqual(5, reader.ColumnCount);
        Assert.AreEqual("Cislo", reader[0, 0]);
        Assert.AreEqual("1234", reader[1, 0]);
        // cas a datum v tvare, ktory citaju importy
        Assert.AreEqual("12:30", reader[1, 1]);
        Assert.AreEqual("09.12.2025", reader[1, 2]);
        Assert.AreEqual("Bratislava hl.st.", reader[1, 3]);
        // text, ktory len vyzera ako cas, ostane textom
        Assert.AreEqual("12:30", reader[2, 3]);
        Assert.AreEqual("", reader[2, 1]);
    }

    [TestMethod]
    public void UsedRange_OrezePrazdneOkraje()
    {
        var (data, rows, columns) = XlsReader.UsedRange([["", "", ""], ["", "a", ""], ["", "", "b"], ["", "", ""]]);

        Assert.AreEqual(2, rows);
        Assert.AreEqual(2, columns);
        Assert.AreEqual("a", data[0, 0]);
        Assert.AreEqual("b", data[1, 1]);
    }
}
