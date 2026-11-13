using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Grafikon;
using ToolsCore.Iniss2.Tables;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Subory s kodom VB.NET, ktore server len cita (statedgm.xml, TabTab.xml, TabTabGlobal.xml, LogText.vb): text vyrazov
/// presne ako v subore a zapis na mieste s komentarmi.
/// </summary>
[TestClass]
public class CodeFilesTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "CodeFilesTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private string Write(string file, string text)
    {
        var path = Path.Combine(_dir, file);
        File.WriteAllText(path, text, Encodings.Win1250);
        return path;
    }

    private string Text(string file) => File.ReadAllText(Path.Combine(_dir, file), Encodings.Win1250);

    [TestMethod]
    public void Statedgm_VyrazyAPodmienkaSNazvomZapisNaMieste()
    {
        Write(StateDiagramFile.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <SD xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" indg=".IndCat3">
              <g ds="#Výchozí vlak">
            <!-- pomocne stavy -->
                <n k="Kolej" f="AUXSTATE">
                  <b t="Kolej" p="0" />
                </n>
                <n k="Nástup" r="Nástup" f="STOJI  NYNISTOJI">
                  <c n="pred odchodom">((.Node.sKey = "#Start") andalso (.PlanDobaPobytu &lt; 5))</c>
                  <b t="Nástup" p="6" />
                  <a t=".DatumCasOdj(True).AddSeconds(-600)" m="1" h="2">
                    <c>.StaniceJeRizena()</c>
                  </a>
                </n>
              </g>
            </SD>
            """);

        var diagram = StateDiagramFile.Read(_dir)!;
        var node = diagram.Graphs[0].Nodes[1];
        Assert.AreEqual(("pred odchodom", "((.Node.sKey = \"#Start\") andalso (.PlanDobaPobytu < 5))"), (node.ConditionName, node.Condition));
        Assert.AreEqual((".StaniceJeRizena()", 2), (node.Automation!.Condition, node.Automation.Variant));
        CollectionAssert.AreEqual(new[] { "STOJI", "NYNISTOJI" }, node.Flags.ToList());
        Assert.IsFalse(StateDiagramFile.Write(_dir, diagram));

        var changed = node with { Automation = node.Automation with { Time = ".DatumCasOdj(True).AddSeconds(-900)" } };
        StateDiagramFile.Write(_dir, diagram with { Graphs = [diagram.Graphs[0] with { Nodes = [diagram.Graphs[0].Nodes[0], changed] }] });

        var text = Text(StateDiagramFile.FileName);
        StringAssert.Contains(text, "<!-- pomocne stavy -->");
        StringAssert.Contains(text, """<c n="pred odchodom">((.Node.sKey = "#Start") andalso (.PlanDobaPobytu &lt; 5))</c>""");
        StringAssert.Contains(text, """f="STOJI  NYNISTOJI""");
        StringAssert.Contains(text, """<a t=".DatumCasOdj(True).AddSeconds(-900)" m="1" h="2">""");
        StringAssert.Contains(text, "xmlns:xsi");
    }

    [TestMethod]
    public void TabTab_VyrazPresneAkoVSubore()
    {
        Write(TabTabXml.GlobalFileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <TABTAB>
              <!-- tabulky tabul -->
              <t n="PRIZNAK">
                <expr>
                  Select Case .Druh
                    Case "R"
                        Return "(červené pole)"
                  End Select
                </expr>
                <k>
                  <k s="" f="-2" />
                  <v f="-2" />
                </k>
              </t>
              <t n="Smer" d="smer jazdy">
                <expr>
                  Return sOrig
                </expr>
              </t>
            </TABTAB>
            """);
        var file = Path.Combine(_dir, TabTabXml.GlobalFileName);

        var tables = TabTabXml.Read(file);
        StringAssert.StartsWith(tables[0].Expression, "\n      Select Case .Druh");
        StringAssert.EndsWith(tables[0].Expression, "End Select\n    ");
        Assert.IsFalse(TabTabXml.Write(file, tables));

        TabTabXml.Write(file, [tables[0], tables[1] with { Expression = "\n      If .JeOdklonen Then Return \"ODKLON\"\n      Return sOrig\n    " }]);

        var text = Text(TabTabXml.GlobalFileName);
        StringAssert.Contains(text, "<!-- tabulky tabul -->");
        StringAssert.Contains(text, """<k s="" f="-2" />""");
        StringAssert.Contains(text, "If .JeOdklonen Then Return \"ODKLON\"");
        Assert.AreEqual(tables[0].Expression, TabTabXml.Read(file)[0].Expression);
    }

    [TestMethod]
    public void LogText_TextBezZmenyNezapise()
    {
        Write(LogTextFile.FileName, "Public Class LogText\r\n    ' hlásenia\r\nEnd Class\r\n");

        var text = LogTextFile.ReadText(_dir)!;
        Assert.IsFalse(LogTextFile.WriteText(_dir, text));
        Assert.IsTrue(LogTextFile.WriteText(_dir, text.Replace("hlásenia", "hlásenia stanice", StringComparison.Ordinal)));
        StringAssert.Contains(Text(LogTextFile.FileName), "' hlásenia stanice\r\n");
    }
}
