using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Kopirovanie priecinka (import grafikonu, rozdelenie blokov).
/// </summary>
[TestClass]
public class CopyDirectoryTests
{
    private string _root = null!;

    [TestInitialize]
    public void Init() => _root = Directory.CreateTempSubdirectory("copydir").FullName;

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_root, true);

    [TestMethod]
    public void CopyDirectory_BezPodpriecinkov_VytvoriCielovyPriecinok()
    {
        var source = Directory.CreateDirectory(Path.Combine(_root, "Horna.2026")).FullName;
        File.WriteAllText(Path.Combine(source, "GRAFIKON.txt"), "hlavicka");
        var target = Path.Combine(_root, "DATA", "Horna.2026");

        Utils.CopyDirectory(source, target);

        Assert.AreEqual("hlavicka", File.ReadAllText(Path.Combine(target, "GRAFIKON.txt")));
    }

    [TestMethod]
    public void CopyDirectory_PodpriecinkyAPrazdnyPriecinok_SkopirujeStrukturu()
    {
        var source = Directory.CreateDirectory(Path.Combine(_root, "src")).FullName;
        Directory.CreateDirectory(Path.Combine(source, "FONTS", "Prazdny"));
        File.WriteAllText(Path.Combine(source, "FONTS", "a.fnt"), "a");
        // nazov zdroja sa v ceste suboru opakuje - nahradenie retazca by cestu pokazilo
        File.WriteAllText(Path.Combine(source, "src.txt"), "b");
        var target = Path.Combine(_root, "dst");

        Utils.CopyDirectory(source, target);

        Assert.AreEqual("a", File.ReadAllText(Path.Combine(target, "FONTS", "a.fnt")));
        Assert.AreEqual("b", File.ReadAllText(Path.Combine(target, "src.txt")));
        Assert.IsTrue(Directory.Exists(Path.Combine(target, "FONTS", "Prazdny")));
    }
}
