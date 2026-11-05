using ToolsCore.Iniss2;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Hladanie instalacie INISS2 podla obsahu priecinkov.
/// </summary>
[TestClass]
public class Iniss2InstallationTests
{
    private string _root = null!;

    [TestInitialize]
    public void Init()
    {
        _root = Path.Combine(Path.GetTempPath(), "Iniss2InstallationTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_root, true);

    private void Create(string relativeFile)
    {
        var file = Path.Combine(_root, relativeFile);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, "");
    }

    [TestMethod]
    public void Find_KorenInstalacieNajdePriecinokServera()
    {
        Create(@"Server\RawBank\SK\FyzBank.xml");

        Assert.AreEqual(Path.Combine(_root, "Server"), Iniss2Installation.Find(_root)?.ServerDir);
    }

    [TestMethod]
    public void Find_PriamoPriecinokServera()
    {
        Create(@"RawBank\SK\FyzBank.xml");

        Assert.AreEqual(_root, Iniss2Installation.Find(_root)?.ServerDir);
    }

    [TestMethod]
    public void Find_ServerBezJazykovSpoznaPodlaKonfiguracie()
    {
        Create(@"Server\Data\Config.xml");

        Assert.AreEqual(Path.Combine(_root, "Server"), Iniss2Installation.Find(_root)?.ServerDir);
    }

    [TestMethod]
    public void Find_BankaSoSuboromDatJeInissTriNula()
    {
        // export z RawBankEditora nechava FyzBank.xml v banke 3.39 - rozhoduje FyzBank.dat
        Create(@"RAWBANK\FyzBank.dat");
        Create(@"RAWBANK\SK\FyzBank.xml");

        Assert.IsNull(Iniss2Installation.Find(_root));
    }

    [TestMethod]
    public void Find_PrazdnyPriecinokNieJeInstalacia()
    {
        Assert.IsNull(Iniss2Installation.Find(_root));
    }

    [TestMethod]
    public void RawBankDir_MaKoncovuLomku()
    {
        var installation = new Iniss2Installation(@"C:\INISS2\Server");

        Assert.AreEqual(@"C:\INISS2\Server\RawBank\", installation.RawBankDir);
        Assert.AreEqual(@"C:\INISS2\Server\Data", installation.DataDir);
    }

    [TestMethod]
    public void GtnLogFile_KazdyDenVlastnySubor()
    {
        var installation = new Iniss2Installation(@"C:\INISS2\Server");

        Assert.AreEqual(@"C:\INISS2\Server\LOG\GTN2030-01-07.log", installation.GtnLogFile(new DateOnly(2030, 1, 7)));
    }
}
