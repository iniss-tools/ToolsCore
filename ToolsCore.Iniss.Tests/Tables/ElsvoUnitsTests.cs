using System.Diagnostics.CodeAnalysis;
using ToolsCore.Iniss.Tables;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tests.Tables;

/// <summary>
/// Spinacie jednotky ELSVO zo zvukovych okruhov Audio.txt (stlpec 8 E&lt;n&gt;, stlpec 9 adresa).
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class ElsvoUnitsTests
{
    [TestMethod]
    [DataRow("E3", 3)]
    [DataRow(" EE12 ", 12)]
    [DataRow("E99", 99)]
    [DataRow("E0", null)]
    [DataRow("E100", null)]
    [DataRow("E", null)]
    [DataRow("e3", null)]
    [DataRow("12", null)]
    [DataRow("Zx", null)]
    [DataRow("", null)]
    public void Linka_TvarStlpca8(string value, int? line) => Assert.AreEqual(line, ElsvoUnits.Line(value));

    [TestMethod]
    [DataRow("", 0)]
    [DataRow("5", 5)]
    [DataRow(" 261 ", 5)]
    [DataRow("7x", 7)]
    [DataRow("x", 0)]
    [DataRow("-1", 255)]
    public void Adresa_DolnyBajtCisla(string value, int address) => Assert.AreEqual(address, ElsvoUnits.Address(value));

    [TestMethod]
    public void Citanie_OkruhyZluceneDoJednotiek()
    {
        var dir = Directory.CreateTempSubdirectory("elsvo");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "audio.TXT"), string.Join("\r\n",
                "; stanica, nazov, protokol, fronta, -, zariadenie, vstup, zosilnovac, ustredna, uzol",
                "",
                "1234,Nástupište 1,N1,F1,,,,E3,5",
                "1234,Nástupište 2,N2,F1,,,,E3,5",
                "1234,Hala,HALA,F2,,,,EE3,",
                "  ; odsadeny komentar,x,y,,,,,E3,9",
                "1234,Ústredňa,U,F3,,,,12,5",
                "1234,Kratky",
                "TEST,,TEST,F4,,,,E4,1",
                "/koniec",
                "1234,Za koncom,Z,F5,,,,E3,7"), Encodings.Win1250);

            var units = ElsvoUnits.Read(dir.FullName);

            CollectionAssert.AreEqual(new[] { "3/5 Nástupište 1, Nástupište 2", "3/0 Hala", "4/1 TEST" },
                units.Select(u => $"{u.Line}/{u.Address} {u.Name}").ToArray());
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [TestMethod]
    public void Citanie_BezSuboruZiadneJednotky()
    {
        var dir = Directory.CreateTempSubdirectory("elsvo");
        try
        {
            Assert.IsEmpty(ElsvoUnits.Read(dir.FullName));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
