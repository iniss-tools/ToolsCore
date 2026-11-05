using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Tables;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Linky a tabule INISS2 z vlastnych fiktivnych dat a presmerovanie linky na simulator.
/// </summary>
[TestClass]
public class TablesReadersTests
{
    private const string Lines = """
        <?xml version="1.0" encoding="windows-1250"?>
        <!-- Linky stanice Lipovec -->
        <CommLines>
          <!-- <line k="Stara" typ="TCP_CLIENT" line="1.1.1.1:1" /> -->
          <line
            k="Tabule"
            typ="UDP_CLIENT"
            protocol="ITP"
            line="10.0.0.1:10001"
            watchdog="60"
          />
          <line k="Zvuk" typ="TCP_CLIENT" protocol="AKSIGNAL" line="10.0.0.2:20" />
          <line k="Seriova" typ="SERIAL" protocol="ELEN" line="COM1" omit="true" />
        </CommLines>
        """;

    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "TablesReadersTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private void Write(string name, string text) => File.WriteAllText(Path.Combine(_dir, name), text, Encodings.Win1250);

    private static Dictionary<string, string> Target(string key, string address) => new() { [key] = address };

    [TestMethod]
    public void CommLines_CitaLinkyVPoradiSuboru()
    {
        Write(CommLinesXml.FileName, Lines);

        var lines = CommLinesXml.Read(_dir);

        Assert.HasCount(3, lines);
        Assert.AreEqual(new CommLine("Tabule", CommLineType.UdpClient, "ITP", "10.0.0.1:10001"), lines[0]);
        Assert.IsTrue(lines[0].CanRedirect);
        Assert.IsNull(lines[0].OriginalAddress);
        Assert.IsTrue(lines[1].CanRedirect);
        Assert.IsFalse(lines[2].CanRedirect);
        Assert.IsTrue(lines[2].Omitted);
    }

    [TestMethod]
    public void CommLines_ChybajuciSuborJePrazdnyZoznam()
    {
        Assert.IsEmpty(CommLinesXml.Read(_dir));
    }

    [TestMethod]
    public void Redirect_MeniLenAdresuAPridaKomentar()
    {
        var updated = CommLinesXml.Redirect(Lines, Target("Tabule", "127.0.0.1:47001"));

        StringAssert.Contains(updated, "line=\"127.0.0.1:47001\"");
        StringAssert.Contains(updated, "<!-- TableSimulator: povodne line=\"10.0.0.1:10001\" -->");
        StringAssert.Contains(updated, "<!-- Linky stanice Lipovec -->", "komentare ostavaju");
        StringAssert.Contains(updated, "<!-- <line k=\"Stara\" typ=\"TCP_CLIENT\" line=\"1.1.1.1:1\" /> -->", "zakomentovana linka sa nemeni");
        StringAssert.Contains(updated, "watchdog=\"60\"");
        StringAssert.Contains(updated, "line=\"10.0.0.2:20\"", "ina linka sa nemeni");
    }

    [TestMethod]
    public void Redirect_ZrusenieVratiPresnePovodnyObsah()
    {
        var updated = CommLinesXml.Redirect(Lines, Target("Tabule", "127.0.0.1:47001"));

        Assert.AreEqual(Lines, CommLinesXml.Redirect(updated, new Dictionary<string, string>()));
    }

    [TestMethod]
    public void Redirect_ZrusenieZachovaKonceRiadkov()
    {
        var crlf = Lines.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal);
        var updated = CommLinesXml.Redirect(crlf, Target("Tabule", "127.0.0.1:47001"));

        Assert.IsFalse(updated.Replace("\r\n", "", StringComparison.Ordinal).Contains('\n', StringComparison.Ordinal));
        Assert.AreEqual(crlf, CommLinesXml.Redirect(updated, new Dictionary<string, string>()));
    }

    [TestMethod]
    public void Redirect_OpakovanieNechaPovodnuAdresu()
    {
        var first = CommLinesXml.Redirect(Lines, Target("Tabule", "127.0.0.1:47001"));
        var second = CommLinesXml.Redirect(first, Target("Tabule", "127.0.0.1:47009"));

        StringAssert.Contains(second, "line=\"127.0.0.1:47009\"");
        Assert.AreEqual(1, second.Split("TableSimulator:").Length - 1);
        Assert.AreEqual(Lines, CommLinesXml.Redirect(second, new Dictionary<string, string>()));
    }

    [TestMethod]
    public void Redirect_LinkaMimoZoznamuSaVrati()
    {
        var both = new Dictionary<string, string> { ["Tabule"] = "127.0.0.1:47001", ["Zvuk"] = "127.0.0.1:47002" };
        var first = CommLinesXml.Redirect(Lines, both);
        var second = CommLinesXml.Redirect(first, Target("Zvuk", "127.0.0.1:47002"));

        StringAssert.Contains(second, "line=\"10.0.0.1:10001\"");
        StringAssert.Contains(second, "line=\"127.0.0.1:47002\"");
    }

    [TestMethod]
    public void Write_ReadVidiPresmerovanieAPovodnuAdresu()
    {
        Write(CommLinesXml.FileName, Lines);

        CommLinesXml.Write(_dir, Target("Tabule", "127.0.0.1:47001"));

        var line = CommLinesXml.Read(_dir)[0];
        Assert.AreEqual("127.0.0.1:47001", line.Address);
        Assert.AreEqual("10.0.0.1:10001", line.OriginalAddress);
        Assert.IsNull(CommLinesXml.Read(_dir)[1].OriginalAddress);
        Assert.IsFalse(File.Exists(Path.Combine(_dir, CommLinesXml.FileName + ".tmp")));
    }

    [TestMethod]
    public void Write_ZachovaDiakritikuVKodovaniServera()
    {
        Write(CommLinesXml.FileName, Lines.Replace("Linky stanice", "Žluťoučké linky", StringComparison.Ordinal));

        CommLinesXml.Write(_dir, Target("Tabule", "127.0.0.1:47001"));

        StringAssert.Contains(File.ReadAllText(Path.Combine(_dir, CommLinesXml.FileName), Encodings.Win1250), "Žluťoučké linky");
    }

    [TestMethod]
    public void Tables_CitaTabuleACasti()
    {
        Write(TablesFile.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <TABLES>
              <t k="Odjezdy" n="Lipovec" save="5400010O.xml">
                <part n="Lipovec - odjezdy" ck="Odjezdová" t="DEPARTURE" rec="10" />
              </t>
              <t k="Nástupiště 1" n="Nástupiště 1" lin="Tabule" addr="2">
                <part n="Nástupiště 1" ck="Nástupištní LCD" t="PLATFORM" rec="1"><pos k="1" /></part>
                <part n="Info" ck="Info LCD" t="INFO" rec="1" />
              </t>
            </TABLES>
            """);

        var tables = TablesFile.Read(_dir);

        Assert.HasCount(2, tables);
        Assert.AreEqual("5400010O.xml", tables[0].SaveFile);
        Assert.AreEqual("", tables[0].Line);
        Assert.AreEqual("Tabule", tables[1].Line);
        Assert.AreEqual(2, tables[1].Address);
        Assert.AreEqual(new TablePart("Nástupiště 1", "Nástupištní LCD", "PLATFORM", 1), tables[1].Parts[0]);
    }

    [TestMethod]
    public void Catalog_CitaDruhPredlohy()
    {
        Write(TableCatalogFile.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <CATALOG>
              <t k="Odjezdová" n="Odjezdová web" cls="INTERNET"><div k="Druh" /></t>
              <t k="Nástupištní LCD" n="Nástupištní" cls="LCD1" />
            </CATALOG>
            """);

        var catalog = TableCatalogFile.Read(_dir);

        Assert.IsFalse(catalog[0].IsDevice);
        Assert.IsTrue(catalog[1].IsDevice);
        Assert.AreEqual("LCD1", catalog[1].Class);
    }

    [TestMethod]
    public void Tables_ChybajuceSuborySuPrazdneZoznamy()
    {
        Assert.IsEmpty(TablesFile.Read(_dir));
        Assert.IsEmpty(TableCatalogFile.Read(_dir));
    }
}
