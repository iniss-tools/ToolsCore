using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Triedy liniek k tabuliam a cisla komunikacnych liniek.
/// </summary>
[TestClass]
public class DriverClassesTests
{
    [TestMethod]
    [DataRow("COM2", 2)]
    [DataRow("com12", 12)]
    [DataRow("4=TCP://192.168.10.20:8090", 4)]
    [DataRow(" 7 =UDP://host:5000", 7)]
    [DataRow("TCP://host:5000", null)]
    [DataRow(@"\PIPE\infotab", null)]
    [DataRow("", null)]
    public void CisloLinky_ZPortu(string port, int? expected)
    {
        Assert.AreEqual(expected, DriverClasses.LineNumber(port));
    }

    [TestMethod]
    public void Trieda_VyrobcoviaRodiny()
    {
        Assert.IsTrue(DriverClasses.AcceptsAutomatically(4, 8));
        Assert.IsFalse(DriverClasses.Accepts(4, 5));
        Assert.IsTrue(DriverClasses.AcceptsAutomatically(5, 12), "ERP patri na linku Elektrocasu, nie ELEN");
        Assert.IsTrue(DriverClasses.Accepts(128, 4), "vzdialeny INISS preposle aj ELEN");
        Assert.IsFalse(DriverClasses.AcceptsAutomatically(128, 4), "na vzdialeny INISS sa tabula automaticky nepriradi");
        Assert.IsFalse(DriverClasses.Accepts(128, 0));
        Assert.IsTrue(DriverClasses.Accepts(130, 5));
        Assert.IsFalse(DriverClasses.Accepts(130, 4));
        Assert.IsFalse(DriverClasses.Accepts(77, 4));
    }

    [TestMethod]
    public void Trieda_KazdaMaNazov()
    {
        foreach (var c in DriverClasses.All)
            Assert.AreNotEqual(c.Class.ToString(System.Globalization.CultureInfo.CurrentCulture), c.Name, $"trieda {c.Class}");
    }
}
