using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Adresy formularov GitHub Issues a skrytie priecinka pouzivatela v texte hlasenia.
/// </summary>
[TestClass]
public class IssueReportTests
{
    private const string Repository = "https://github.com/iniss-tools/GVDEditor";

    [TestMethod]
    public void BugUrl_BezChyby_PredvyplniVerziuAWindows()
    {
        var url = IssueReport.BugUrl(Repository, "1.10", "Microsoft Windows 10.0.26300", null);

        Assert.AreEqual(Repository + "/issues/new?template=chyba.yml&verzia=1.10&system=Microsoft%20Windows%2010.0.26300", url);
    }

    [TestMethod]
    public void BugUrl_KratkaChyba_JeCela()
    {
        const string error = "Súbor 'a&b#c.txt' chýba\r\n   at X()";

        var url = IssueReport.BugUrl(Repository, "1.10", "Windows", error);

        Assert.AreEqual(error, Field(url, "chyba"));
    }

    [TestMethod]
    public void BugUrl_DlhaChyba_SkratiSaSPoznamkou()
    {
        var error = "Chyba ť😀: " + new string('ž', 5000);

        var url = IssueReport.BugUrl(Repository, "1.10", "Windows", error);

        Assert.IsLessThanOrEqualTo(IssueReport.MaxUrlLength, url.Length);
        var field = Field(url, "chyba");
        Assert.StartsWith("Chyba ť😀: žžž", field);
        Assert.Contains("schránke", field);
    }

    [TestMethod]
    public void FeatureUrl_FormularNavrhu() =>
        Assert.AreEqual(Repository + "/issues/new?template=navrh.yml", IssueReport.FeatureUrl(Repository));

    [TestMethod]
    [DataRow(@"C:\Users\fil\a.txt", @"%USERPROFILE%\a.txt")]
    [DataRow(@"Path 'c:\users\FIL\a.txt' is denied", @"Path '%USERPROFILE%\a.txt' is denied")]
    [DataRow(@"C:\Users\fil", "%USERPROFILE%")]
    [DataRow(@"C:\Users\filip\a.txt", @"C:\Users\filip\a.txt")]
    [DataRow(@"D:\INISS\DATA", @"D:\INISS\DATA")]
    public void HideUserFolder_LenPriecinokPouzivatela(string text, string expected) =>
        Assert.AreEqual(expected, IssueReport.HideUserFolder(text, @"C:\Users\fil"));

    private static string Field(string url, string name) =>
        Uri.UnescapeDataString(new Uri(url).Query.TrimStart('?').Split('&').Single(part => part.StartsWith(name + "=", StringComparison.Ordinal))[(name.Length + 1)..]);
}
