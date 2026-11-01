using System.IO.Compression;
using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Export, import a obnova nastaveni programu: import a obnova sa vykonaju az pri starte, povodne subory sa zalohuju.
/// </summary>
[TestClass]
public class SettingsTransferTests
{
    private string _dir = null!;
    private string _config = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "SettingsTransferTests-" + Guid.NewGuid().ToString("N"));
        _config = Path.Combine(_dir, "config");
        Directory.CreateDirectory(_config);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    [TestMethod]
    public void ExportAImport_PrenesieSuboryAZalohujePovodne()
    {
        WriteConfig("<CONFIG><Language>CZ</Language></CONFIG>", "<SCHEMES version=\"2\" />");
        var zip = Path.Combine(_dir, "nastavenia.zip");
        SettingsTransfer.Export(zip, _config, "GVDEditor");
        WriteConfig("<CONFIG><Language>SK</Language></CONFIG>", "<SCHEMES version=\"2\" active=\"Dark\" />");

        Assert.IsNull(SettingsTransfer.Check(zip, "GVDEditor"));
        SettingsTransfer.ScheduleImport(zip, _config);
        Assert.Contains("SK", File.ReadAllText(Path.Combine(_config, FileConsts.FileConfig)), "import az pri starte");

        Assert.IsTrue(SettingsTransfer.ApplyPending(_config));

        Assert.Contains("CZ", File.ReadAllText(Path.Combine(_config, FileConsts.FileConfig)));
        var backup = Directory.GetDirectories(Path.Combine(_config, "backup")).Single();
        Assert.Contains("SK", File.ReadAllText(Path.Combine(backup, FileConsts.FileConfig)));
        Assert.IsFalse(SettingsTransfer.ApplyPending(_config), "pripraveny import sa vykona raz");
    }

    [TestMethod]
    public void Check_NastaveniaInehoProgramu_Odmietne()
    {
        WriteConfig("<CONFIG />", null);
        var zip = Path.Combine(_dir, "nastavenia.zip");
        SettingsTransfer.Export(zip, _config, "RawBankEditor");

        Assert.Contains("RawBankEditor", SettingsTransfer.Check(zip, "GVDEditor")!);
    }

    [TestMethod]
    public void Check_SuborBezNastaveni_Odmietne()
    {
        var zip = Path.Combine(_dir, "ine.zip");
        using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
            archive.CreateEntry("readme.txt");
        var text = Path.Combine(_dir, "text.zip");
        File.WriteAllText(text, "nie je zip");

        Assert.IsNotNull(SettingsTransfer.Check(zip, "GVDEditor"));
        Assert.IsNotNull(SettingsTransfer.Check(text, "GVDEditor"));
    }

    [TestMethod]
    public void Import_IneSuboryVArchive_NerozbaliAniMimoPriecinka()
    {
        var zip = Path.Combine(_dir, "nastavenia.zip");
        using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
        {
            Write(archive, FileConsts.FileConfig, "<CONFIG />");
            Write(archive, "../zly.cfg", "x");
            Write(archive, "ine.txt", "x");
        }

        SettingsTransfer.ScheduleImport(zip, _config);
        SettingsTransfer.ApplyPending(_config);

        Assert.IsTrue(File.Exists(Path.Combine(_config, FileConsts.FileConfig)));
        Assert.IsFalse(File.Exists(Path.Combine(_dir, "zly.cfg")));
        Assert.IsFalse(File.Exists(Path.Combine(_config, "ine.txt")));
    }

    [TestMethod]
    public void Obnova_ZmazeNastaveniaPoZalohe()
    {
        WriteConfig("<CONFIG />", "<SCHEMES />");

        SettingsTransfer.ScheduleReset(_config);
        SettingsTransfer.ApplyPending(_config);

        Assert.IsFalse(File.Exists(Path.Combine(_config, FileConsts.FileConfig)));
        Assert.IsFalse(File.Exists(Path.Combine(_config, FileConsts.FileStyles)));
        Assert.HasCount(1, Directory.GetDirectories(Path.Combine(_config, "backup")));
    }

    private void WriteConfig(string config, string? styles)
    {
        File.WriteAllText(Path.Combine(_config, FileConsts.FileConfig), config);
        if (styles is not null)
            File.WriteAllText(Path.Combine(_config, FileConsts.FileStyles), styles);
    }

    private static void Write(ZipArchive archive, string name, string text)
    {
        using var writer = new StreamWriter(archive.CreateEntry(name).Open());
        writer.Write(text);
    }
}
