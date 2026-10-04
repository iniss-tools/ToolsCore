using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Presmerovanie liniek na simulator tabul cez .INI - cela sekcia z registra, povodne hodnoty v komentaroch, navrat.
/// </summary>
[TestClass]
public class SimulatorRedirectTests
{
    private static ResolvedConfig Resolve(RegBranch machine, InissIniFile? ini = null) => RegResolver.Resolve(new InissConfigSource
    {
        AppName = "INISS - Test",
        Version = new RegVersion(3, 39),
        Machine = machine,
        Ini = ini
    });

    private static RegBranch TwoLines() => new RegBranch()
        .Set("Driver", "TableClass", RegRawValue.Dword(4))
        .Set("Driver", "TablePort", RegRawValue.String("COM3"))
        .Set("Driver", "TableTimeoutConst [ms]", RegRawValue.Dword(300))
        .Set("Driver0", "TableClass", RegRawValue.Dword(5))
        .Set("Driver0", "TablePort", RegRawValue.String("4=TCP://10.0.0.5:4001"));

    [TestMethod]
    public void TablePort_CisloLinkyAAdresaSimulatora()
    {
        Assert.AreEqual("3=TCP://127.0.0.1:47003", SimulatorRedirect.TablePort(3, "127.0.0.1", 47003));
    }

    [TestMethod]
    public void SekciaZRegistra_ZapiseSaCelaAPlati()
    {
        var config = Resolve(TwoLines());

        var ini = SimulatorRedirect.Update(config, null, new Dictionary<string, string> { ["Driver"] = "3=TCP://127.0.0.1:47003" });

        Assert.AreEqual("4", ini.GetString("Driver", "TableClass"));
        Assert.AreEqual("300", ini.GetString("Driver", "TableTimeoutConst [ms]"), "sekcia v .INI nahradi register - aj casovanie");
        Assert.AreEqual("1", ini.GetString("Environment", "OutToTableDriver"));
        Assert.IsFalse(ini.HasSection("Driver0"), "ina linka sa nemeni");
        var line = SimulatorRedirect.Find(ini).Single();
        Assert.AreEqual(new RedirectedLine("Driver", null, true), line);

        var effective = Resolve(TwoLines(), ini);
        Assert.AreEqual("3=TCP://127.0.0.1:47003", effective.Find("Driver", "TablePort")!.Value);
        Assert.AreEqual(300, effective.Find("Driver", "TableTimeoutConst [ms]")!.Value);
    }

    [TestMethod]
    public void Zrusenie_VratiSuborPresneAkoBol()
    {
        const string original = "; moja konfiguracia\r\n[Environment]\r\nOutToTableDriver=0\r\nLogging=1\r\n\r\n[Driver]\r\nTableClass=4\r\nTablePort=COM2\r\n";
        var before = InissIniFile.Parse(original);
        var config = Resolve(TwoLines(), before);

        var redirected = SimulatorRedirect.Update(config, before, new Dictionary<string, string>
        {
            ["Driver"] = "2=TCP://127.0.0.1:47002",
            ["Driver0"] = "4=TCP://127.0.0.1:47004"
        });
        var restored = SimulatorRedirect.Update(Resolve(TwoLines(), redirected), redirected, new Dictionary<string, string>());

        Assert.AreEqual("2=TCP://127.0.0.1:47002", redirected.GetString("Driver", "TablePort"));
        Assert.AreEqual(new RedirectedLine("Driver", "COM2", false), SimulatorRedirect.Find(redirected)[0]);
        Assert.AreEqual(original, restored.ToString());
    }

    [TestMethod]
    public void BezPovodnehoSuboru_ZrusenieNechaPrazdnySubor()
    {
        var config = Resolve(TwoLines());
        var redirected = SimulatorRedirect.Update(config, null, new Dictionary<string, string> { ["Driver0"] = "4=TCP://127.0.0.1:47004" });

        var restored = SimulatorRedirect.Update(Resolve(TwoLines(), redirected), redirected, new Dictionary<string, string>());

        Assert.IsTrue(restored.IsEmpty, restored.ToString());
    }

    [TestMethod]
    public void CiastocneZrusenie_DruhaLinkaOstane()
    {
        var config = Resolve(TwoLines());
        var both = SimulatorRedirect.Update(config, null, new Dictionary<string, string>
        {
            ["Driver"] = "3=TCP://127.0.0.1:47003",
            ["Driver0"] = "4=TCP://127.0.0.1:47004"
        });

        var one = SimulatorRedirect.Update(Resolve(TwoLines(), both), both, new Dictionary<string, string> { ["Driver0"] = "4=TCP://127.0.0.1:47004" });

        Assert.IsFalse(one.HasSection("Driver"), "sekcia z registra sa z .INI odstrani");
        Assert.AreEqual("Driver0", SimulatorRedirect.Find(one).Single().Section);
        Assert.AreEqual("1", one.GetString("Environment", "OutToTableDriver"));
    }

    [TestMethod]
    public void OpakovanePresmerovanie_PamataSiPrvyPovodnyStav()
    {
        var before = InissIniFile.Parse("[Driver]\r\nTableClass=4\r\nTablePort=COM2\r\n");
        var first = SimulatorRedirect.Update(Resolve(TwoLines(), before), before, new Dictionary<string, string> { ["Driver"] = "2=TCP://127.0.0.1:47002" });

        var second = SimulatorRedirect.Update(Resolve(TwoLines(), first), first, new Dictionary<string, string> { ["Driver"] = "2=TCP://10.0.0.9:48002" });
        var restored = SimulatorRedirect.Update(Resolve(TwoLines(), second), second, new Dictionary<string, string>());

        Assert.HasCount(1, second.Comments("Driver"));
        Assert.AreEqual("2=TCP://10.0.0.9:48002", second.GetString("Driver", "TablePort"));
        Assert.AreEqual("COM2", restored.GetString("Driver", "TablePort"));
        Assert.HasCount(0, restored.Comments("Driver"));
    }

    [TestMethod]
    public void IniKomentare_PridanieAOdstranenie()
    {
        var ini = InissIniFile.Parse("[A]\r\nx=1\r\n");

        ini.AddComment("A", "prva");
        ini.AddComment("B", "nova sekcia");

        CollectionAssert.AreEqual(new[] { "prva" }, ini.Comments("A").ToArray());
        Assert.AreEqual("[A]\r\n; prva\r\nx=1\r\n\r\n[B]\r\n; nova sekcia\r\n", ini.ToString());
        Assert.AreEqual(1, ini.RemoveComments("A", c => c == "prva"));
        Assert.AreEqual("1", ini.GetString("A", "x"));
    }
}
