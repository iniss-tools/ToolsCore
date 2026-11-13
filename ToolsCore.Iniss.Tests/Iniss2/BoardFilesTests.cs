using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Tables;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Tabule stanice (Tables.xml) a katalogove predlohy (TCatalog.xml): uplny model a zapis na mieste s komentarmi.
/// </summary>
[TestClass]
public class BoardFilesTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "BoardFilesTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private void Write(string file, string text)
    {
        var path = Path.Combine(_dir, file);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text, Encodings.Win1250);
    }

    private string Text(string file) => File.ReadAllText(Path.Combine(_dir, file), Encodings.Win1250);

    [TestMethod]
    public void Tables_PodmienkaJasAMultiokna()
    {
        Write(TablesFile.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <TABLES>
              <!-- nastupistia -->
              <t k="N1" n="Nástupište 1" lin="Lipovec_Tabule" lin_podr="A" addr="2" f="NOSCROLL" lighttime0="06:00" light0="80" lightIfEmpty1="true">
                <part n="N1" ck="Nástupištní LCD" t="PLATFORM" rec="1" cond=".Druh &lt;&gt; &quot;Os&quot;" windef="0;1:2,4-6">
                  <pos k="1" />
                  <pos k="2" pri="1" />
                </part>
              </t>
              <t k="Info" n="Info" addr="-1">
                <part ck="Info" t="INFO" websource="pocasie" rec="1" />
              </t>
            </TABLES>
            """);

        var tables = TablesFile.Read(_dir);
        var table = tables[0];
        Assert.AreEqual(("A", true, "06:00", "80", "true"), (table.SubLine, table.NoScroll, table.LightTime0, table.Light0, table.LightIfEmpty1));
        Assert.AreEqual((".Druh <> \"Os\"", "0;1:2,4-6", 1), (table.Parts[0].Condition, table.Parts[0].WindowDefinition, table.Parts[0].TrackPriorities["2"]));
        Assert.AreEqual("pocasie", tables[1].Parts[0].WebSource);
        Assert.IsFalse(TablesFile.Write(_dir, tables));

        var part = table.Parts[0] with { Condition = ".Druh = \"R\"" };
        TablesFile.Write(_dir, [table with { Parts = [part], Light0 = "90" }, tables[1]]);

        var text = Text(TablesFile.FileName);
        StringAssert.Contains(text, "<!-- nastupistia -->");
        StringAssert.Contains(text, """<part ck="Info" t="INFO" websource="pocasie" rec="1" />""");
        StringAssert.Contains(text, "cond=\".Druh = &quot;R&quot;\"");
        Assert.AreEqual(("90", "A"), (TablesFile.Read(_dir)[0].Light0, TablesFile.Read(_dir)[0].SubLine));
    }

    [TestMethod]
    public void Katalog_ZapisNaMieste()
    {
        Write(TableCatalogFile.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <CATALOG>
              <!-- predlohy -->
              <t k="Nástupištní LCD" n="Nástupištní" cls="LCD1">
                <div k="Číslo" n="Číslo" inf="CisloVlaku" fnt="16" align="RIGHT">
                  <pos c1="3" c2="10" />
                </div>
                <div k="Cíl" n="Cíl" inf="CilNazev" fnt="16" align="LEFT" trn="TRANSLATE" t1="Stanice">
                  <pos r="1" c1="0" c2="23" />
                </div>
              </t>
            </CATALOG>
            """);

        var catalog = TableCatalogFile.Read(_dir);
        Assert.IsFalse(TableCatalogFile.Write(_dir, catalog));
        var table = catalog[0];
        TableCatalogFile.Write(_dir, [table with { Columns = [table.Columns[0] with { End = 12 }, table.Columns[1]] }]);

        var text = Text(TableCatalogFile.FileName);
        StringAssert.Contains(text, "<!-- predlohy -->");
        StringAssert.Contains(text, """<div k="Cíl" n="Cíl" inf="CilNazev" fnt="16" align="LEFT" trn="TRANSLATE" t1="Stanice">""");
        Assert.AreEqual(12, TableCatalogFile.Read(_dir)[0].Columns[0].End);
    }
}
