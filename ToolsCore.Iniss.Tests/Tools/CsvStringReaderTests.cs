using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Citanie CSV textu (import zo suboru alebo schranky).
/// </summary>
[TestClass]
public class CsvStringReaderTests
{
    [TestMethod]
    [DataRow("Číslo\tTyp\tPríchod\n521\tEx\t09:10", '\t')]
    [DataRow("Číslo;Typ;Trasy\n521;Ex;9900200,9900100", ';')]
    [DataRow("Číslo,Typ,Príchod\n521,Ex,09:10", ',')]
    [DataRow("521", ';')]
    public void DetectSeparator_PodlaPrvehoRiadku(string text, char expected)
    {
        Assert.AreEqual(expected, CsvStringReader.DetectSeparator(text));
    }

    [TestMethod]
    public void Citanie_UvodzovkyAChybajuceBunky()
    {
        var reader = new CsvStringReader("a;\"b;c\";\"d\"\"e\"\r\nf", rowsep: ';');

        Assert.AreEqual(2, reader.RowCount);
        Assert.AreEqual(3, reader.ColumnCount);
        Assert.AreEqual("b;c", reader[0, 1]);
        Assert.AreEqual("d\"e", reader[0, 2]);
        Assert.AreEqual("f", reader[1, 0]);
        Assert.AreEqual("", reader[1, 2]);
    }

    [TestMethod]
    public void Citanie_OddelovacNaKonciJePrazdnaBunka()
    {
        var reader = new CsvStringReader("a;b;\nc", rowsep: ';');

        Assert.AreEqual(3, reader.ColumnCount);
        Assert.AreEqual("", reader[0, 2]);
    }
}
