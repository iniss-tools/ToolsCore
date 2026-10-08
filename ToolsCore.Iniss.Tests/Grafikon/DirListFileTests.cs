using System.Diagnostics.CodeAnalysis;
using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss.Grafikon;

namespace ToolsCore.Tests.Grafikon;

/// <summary>
/// DirList.TXT: grafikon priamo v DATA (bez DirList.TXT) nesmie zapis zoznamu skryt pred INISSom - existujuci
/// prazdny subor INISS berie ako prazdny zoznam a nenacita ziadny grafikon.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class DirListFileTests
{
    // "~" = grafikon priamo v DATA, "|" oddeluje polozky aj riadky, null = subor neexistuje
    private const string DataRoot = "~";

    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "DirListFileTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private string File => Path.Combine(_dir, GvdFileConsts.FileDirlist);

    private List<DirList> Dirs(string names) =>
        names.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n == DataRoot
                ? new DirList { DirName = "", FullPath = _dir }
                : new DirList { DirName = n, FullPath = Path.Combine(_dir, n) })
            .ToList();

    [TestMethod]
    [DataRow(null, DataRoot, false, null, DisplayName = "len grafikon v DATA, subor chyba - nezalozi sa")]
    [DataRow("A.2019,,,,", DataRoot, false, "A.2019,,,,", DisplayName = "len grafikon v DATA, subor existuje - neprepise sa")]
    [DataRow(null, "", false, null, DisplayName = "prazdny zoznam, subor chyba - nezalozi sa")]
    [DataRow("A.2019,,,,", "", true, "", DisplayName = "vsetky grafikony odstranene - subor sa vyprazdni")]
    [DataRow(null, DataRoot + "|B.2020", true, "B.2020,,,,", DisplayName = "novy grafikon vedla DATA - zapise sa len novy")]
    [DataRow("A.2019,,,,", "A.2019|B.2020", true, "A.2019,,,,|B.2020,,,,", DisplayName = "bezne priecinky - zapisu sa")]
    public void DirList_ZapisBezRiadkov_NezaloziAniNeprepiseSubor(string? before, string names, bool expectedWritten, string? expectedAfter)
    {
        if (before != null)
            System.IO.File.WriteAllLines(File, before.Split('|'), Encodings.Win1250);

        var written = DirListFile.WriteFile(File, Dirs(names));

        Assert.AreEqual(expectedWritten, written);
        if (expectedAfter == null)
        {
            Assert.IsFalse(System.IO.File.Exists(File));
            return;
        }

        var after = System.IO.File.ReadAllLines(File, Encodings.Win1250);
        CollectionAssert.AreEqual(expectedAfter.Split('|', StringSplitOptions.RemoveEmptyEntries), after);
    }

    [TestMethod]
    public void DirList_RoundTrip_ZachovaPoradieAPriznaky()
    {
        // priznaky sa zapisu tak, ako su - aj neupraveny zapis s oddelovacom a malymi pismenami
        string[] lines = ["B.2020,4,,\"z, k\",0xFF8000", "A.2019,,7,ZOK3,", "C.2021,,,,"];
        System.IO.File.WriteAllLines(File, lines, Encodings.Win1250);

        var dirs = DirListFile.Read(_dir);
        Assert.AreEqual("B.2020|A.2019|C.2021", string.Join("|", dirs.Select(d => d.DirName)));
        Assert.AreEqual("z, k", dirs[0].Flags);
        Assert.AreEqual("ZK", DirListFlags.Parse(dirs[0].Flags).ToString());

        // posun A.2019 na zaciatok a nove priznaky pre C.2021
        (dirs[0], dirs[1]) = (dirs[1], dirs[0]);
        dirs[2].Flags = new DirListFlags(false, false, DirListTrainCreation.CreateWithoutCategori, 2).ToString();
        DirListFile.Write(_dir, dirs);

        var after = System.IO.File.ReadAllLines(File, Encodings.Win1250);
        CollectionAssert.AreEqual(new[] { "A.2019,,7,ZOK3,", "B.2020,4,,\"z, k\",0xFF8000", "C.2021,,,M2," }, after);
    }
}
