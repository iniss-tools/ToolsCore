using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2;
using ToolsCore.Iniss2.Grafikon;
using ToolsCore.Iniss2.Tables;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Zistenie beziaceho servera a pomocne udaje instalacie: funkcie LogText, spolocne tabulky TabTab a priecinok dat.
/// </summary>
[TestClass]
public class Iniss2ServerTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "Iniss2ServerTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private void Write(string relativeFile, string text)
    {
        var file = Path.Combine(_dir, relativeFile);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, text, Encodings.Win1250);
    }

    [TestMethod]
    public void Server_MutexZnamenaBehVRelaciiNieTejtoInstalacie()
    {
        Write(Iniss2Server.ExecutableName, "");
        using var mutex = new Mutex(false, Iniss2Server.MutexName);

        Assert.IsTrue(Iniss2Server.IsRunning());
        Assert.IsFalse(Iniss2Server.IsRunning(_dir));
    }

    [TestMethod]
    [DataRow("IniSrv.exe")]
    [DataRow("_IniSrv.exe")]
    public void Server_ZamknutyProgramZnamenaBeh(string executable)
    {
        Write(executable, "");
        using var running = new FileStream(Path.Combine(_dir, executable), FileMode.Open, FileAccess.Read, FileShare.Read);

        Assert.IsTrue(Iniss2Server.IsRunning(_dir));
    }

    [TestMethod]
    public void Server_NespustenyProgramAleboPriecinokBezProgramuNebezi()
    {
        Write(Iniss2Server.ExecutableName, "");
        Write(@"kopia\Data\DirList.xml", "<DL />");

        Assert.IsFalse(Iniss2Server.IsRunning(_dir));
        Assert.IsFalse(Iniss2Server.IsRunning(Path.Combine(_dir, "kopia")));
        Assert.IsFalse(Iniss2Server.IsRunning(Path.Combine(_dir, "nie je")));
    }

    [TestMethod]
    public void LogText_FunkcieSOznacenim()
    {
        var functions = LogTextFile.Functions([
            "Public Class LogText",
            "  <Dodatek(\"Bufet\")> <Popis(\"x\")>",
            "  Public Function D1006(ByVal v As VlakS) As String",
            "  ' Public Function Vypnuta(",
            "  Sub vyluka_ve_stanici(ByVal v As VlakS)",
            "End Class"
        ]);

        CollectionAssert.AreEquivalent(new[] { "Dodatek", "Popis" }, functions["d1006"].ToArray());
        Assert.IsTrue(functions.ContainsKey("Vyluka_ve_stanici"));
        Assert.IsFalse(functions.ContainsKey("Vypnuta"));
        Assert.IsNull(LogTextFile.ReadFunctions(_dir));
    }

    [TestMethod]
    public void LogText_PopisZPrvehoOznaceniaSTextom()
    {
        var descriptions = LogTextFile.Descriptions([
            "Public Class LogText",
            "  <Dodatek(\"Bufet \"\"U vlaku\"\"\")> <Popis(\"x\")>",
            "  Public Function D1006(ByVal v As VlakS) As String",
            "  <Vlastny()> _",
            "  Sub Bez_popisu(ByVal v As VlakS)",
            "End Class"
        ]);

        Assert.AreEqual("Bufet \"U vlaku\"", descriptions["d1006"]);
        Assert.IsFalse(descriptions.ContainsKey("Bez_popisu"));
        Assert.IsNull(LogTextFile.ReadDescriptions(_dir));
    }

    [TestMethod]
    public void TabTabGlobal_PlatiPrvyVPoradiGrafikonov()
    {
        Write("DirList.xml", """<DL><g n="2031" /><g n="2030" /><g n="2029" /></DL>""");
        Write(@"2030\TabTabGlobal.xml", """<TABTAB><t n="Druh2030" /></TABTAB>""");
        Write(@"2029\TabTabGlobal.xml", """<TABTAB><t n="Druh2029" /></TABTAB>""");

        CollectionAssert.AreEqual(new[] { "Druh2030" }, TabTabXml.ReadGlobal(_dir).Select(table => table.Name).ToArray());
    }

    [TestMethod]
    public void DataDirectory_DataServerAleboInstalacia()
    {
        Write(@"Server\Data\Config.xml", "<INISSCFG />");
        Write(@"Server\Data\DirList.xml", "<DL />");
        var data = Path.Combine(_dir, "Server", "Data");

        Assert.AreEqual(data, Iniss2Installation.DataDirectory(data));
        Assert.AreEqual(data, Iniss2Installation.DataDirectory(Path.Combine(_dir, "Server")));
        Assert.AreEqual(data, Iniss2Installation.DataDirectory(_dir));
        Assert.IsNull(Iniss2Installation.DataDirectory(Path.Combine(_dir, "nie je")));
    }
}
