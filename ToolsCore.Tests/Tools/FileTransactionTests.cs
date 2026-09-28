using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
///     Transakcne ukladanie (grafikon, banka zvukov) - pri chybe sa subory vratia do povodneho stavu.
/// </summary>
[TestClass]
public class FileTransactionTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "FileTransactionTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "EXPORT3A.txt"), "povodny");
        File.WriteAllText(Path.Combine(_dir, "VYLUKA.txt"), "vyluka");
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var file in Directory.GetFiles(_dir, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(_dir, true);
    }

    [TestMethod]
    public void Priecinok_Rollback_VratiZmeneneAOdstraniNoveSubory()
    {
        var transaction = new FileTransaction(_dir);
        File.WriteAllText(Path.Combine(_dir, "EXPORT3A.txt"), "novy");
        File.WriteAllText(Path.Combine(_dir, "RAZENI1.txt"), "novy subor");

        Assert.IsTrue(transaction.TryRollback());
        Assert.AreEqual("povodny", File.ReadAllText(Path.Combine(_dir, "EXPORT3A.txt")));
        Assert.IsFalse(File.Exists(Path.Combine(_dir, "RAZENI1.txt")));
        Assert.IsFalse(Directory.Exists(transaction.BackupPath));
    }

    [TestMethod]
    public void Priecinok_Rollback_NezmenenySuborLenNaCitanie_NevadiObnoveniu()
    {
        // zapis zlyhal prave na subore len na citanie - ten ostal nezmeneny a obnova ho nesmie prepisovat
        var readOnly = Path.Combine(_dir, "VYLUKA.txt");
        File.SetAttributes(readOnly, FileAttributes.ReadOnly);

        var transaction = new FileTransaction(_dir);
        File.WriteAllText(Path.Combine(_dir, "EXPORT3A.txt"), "novy");

        Assert.IsTrue(transaction.TryRollback());
        Assert.AreEqual("povodny", File.ReadAllText(Path.Combine(_dir, "EXPORT3A.txt")));
    }

    [TestMethod]
    public void Commit_ZahodiZalohu()
    {
        var transaction = new FileTransaction(_dir);
        File.WriteAllText(Path.Combine(_dir, "EXPORT3A.txt"), "novy");

        transaction.Commit();
        Assert.AreEqual("novy", File.ReadAllText(Path.Combine(_dir, "EXPORT3A.txt")));
        Assert.IsFalse(Directory.Exists(transaction.BackupPath));
    }

    [TestMethod]
    public void Subory_Rollback_VratiSuboryZRoznychPriecinkovSRovnakymNazvom()
    {
        // FYZZVUK.DAT kazdeho jazyka banky ma rovnaky nazov, len iny priecinok
        var sk = Path.Combine(_dir, "SK", "FYZZVUK.DAT");
        var cz = Path.Combine(_dir, "CZ", "FYZZVUK.DAT");
        Directory.CreateDirectory(Path.GetDirectoryName(sk)!);
        Directory.CreateDirectory(Path.GetDirectoryName(cz)!);
        File.WriteAllText(sk, "sk");
        File.WriteAllText(cz, "cz");

        var transaction = new FileTransaction([sk, cz]);
        File.WriteAllText(sk, "novy sk");
        File.WriteAllText(cz, "novy cz");

        Assert.IsTrue(transaction.TryRollback());
        Assert.AreEqual("sk", File.ReadAllText(sk));
        Assert.AreEqual("cz", File.ReadAllText(cz));
    }

    [TestMethod]
    public void Subory_Rollback_ZmazeLenVymenovaneNoveSubory()
    {
        var created = Path.Combine(_dir, "GB", "FYZZVUK.DAT");
        var other = Path.Combine(_dir, "INE.txt");

        var transaction = new FileTransaction([created, Path.Combine(_dir, "EXPORT3A.txt")]);
        Directory.CreateDirectory(Path.GetDirectoryName(created)!);
        File.WriteAllText(created, "novy jazyk");
        File.WriteAllText(other, "nezavisly subor");

        Assert.IsTrue(transaction.TryRollback());
        Assert.IsFalse(File.Exists(created));
        Assert.IsTrue(File.Exists(other), "subor mimo transakcie sa nesmie zmazat");
    }
}
