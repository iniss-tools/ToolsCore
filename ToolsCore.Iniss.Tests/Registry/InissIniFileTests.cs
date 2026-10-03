using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Subor .INI vedla INISSu - citanie ako GetPrivateProfileInt/String a uprava so zachovanim ostatnych riadkov.
/// </summary>
[TestClass]
public class InissIniFileTests
{
    private const string Sample = "; prebija register\r\n[Environment]\r\nLogging = 1 \r\nOdklonTxt=\" Odklon \"\r\nlogging=0\r\n\r\n[Driver0]\r\nTablePort=TCP://10.0.0.20:4001\r\n[environment]\r\nTableStatus=5\r\n";

    [TestMethod]
    public void Citanie_BezRozliseniaVelkostiPrvySekciaAjNazov()
    {
        var ini = InissIniFile.Parse(Sample);

        Assert.AreEqual(1, ini.GetNumber("ENVIRONMENT", "logging"));
        // druha sekcia s rovnakym menom sa necita
        Assert.IsNull(ini.GetNumber("Environment", "TableStatus"));
        Assert.AreEqual(" Odklon ", ini.GetString("Environment", "OdklonTxt"));
        Assert.IsTrue(ini.HasSection("driver0"));
        CollectionAssert.AreEqual(new[] { "Environment", "Driver0" }, ini.SectionNames.ToArray());
    }

    [TestMethod]
    [DataRow("200", 200)]
    [DataRow("-1", -1)]
    [DataRow(" 15 ms", 15)]
    [DataRow("0x10", 0)]
    [DataRow("abc", 0)]
    [DataRow("", 0)]
    public void Cislo_AkoGetPrivateProfileInt(string text, int expected)
    {
        Assert.AreEqual(expected, InissIniFile.ParseNumber(text));
    }

    [TestMethod]
    public void Cislo_ZnackaChybajucejHodnotyJeAkoKebyNebola()
    {
        var ini = InissIniFile.Parse("[A]\r\nX=-1936946036\r\nY=#deflt#\r\n");

        Assert.IsNull(ini.GetNumber("A", "X"));
        Assert.IsNull(ini.GetString("A", "Y"));
    }

    [TestMethod]
    public void Uprava_BezZmienJeTextRovnaky()
    {
        Assert.AreEqual(Sample, InissIniFile.Parse(Sample).ToString());
    }

    [TestMethod]
    public void Uprava_PrepisePridaAZalozi()
    {
        var ini = InissIniFile.Parse(Sample);

        ini.Set("environment", "LOGGING", 0);
        ini.Set("Driver0", "TableClass", 128);
        ini.Set("Grafikon", "MinStay [m]", 3);
        ini.Set("Environment", "OdklonTxt", " text ");

        var text = ini.ToString();
        StringAssert.Contains(text, "Logging=0\r\n");
        StringAssert.Contains(text, "[Driver0]\r\nTablePort=TCP://10.0.0.20:4001\r\nTableClass=128\r\n");
        StringAssert.EndsWith(text, "\r\n[Grafikon]\r\nMinStay [m]=3\r\n");
        Assert.AreEqual(" text ", InissIniFile.Parse(text).GetString("Environment", "OdklonTxt"));
        StringAssert.StartsWith(text, "; prebija register\r\n");
    }

    [TestMethod]
    public void Uprava_OdstraniHodnotuASekciu()
    {
        var ini = InissIniFile.Parse(Sample);

        Assert.IsTrue(ini.Remove("Environment", "Logging"));
        Assert.IsFalse(ini.Remove("Environment", "Nic"));
        Assert.IsTrue(ini.RemoveSection("Driver0"));

        // po odstraneni prveho vyskytu plati druhy riadok s rovnakym nazvom
        Assert.AreEqual(0, ini.GetNumber("Environment", "Logging"));
        Assert.IsFalse(ini.HasSection("Driver0"));
    }
}
