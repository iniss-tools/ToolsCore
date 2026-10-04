using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Nastroje nad konfiguraciou - klon, export do .INI, porovnanie a prevzatie, vycistenie, vetva HKCU.
/// </summary>
[TestClass]
public class RegToolsTests
{
    private static InissConfigSource Source(RegBranch? user = null, RegBranch? machine = null, RegBranch? virtualStore = null,
        InissRunMode mode = InissRunMode.Normal) => new()
    {
        AppName = "INISS - Test",
        Version = new RegVersion(3, 39),
        RunMode = mode,
        User = user ?? RegBranch.Missing,
        Machine = machine ?? new RegBranch(),
        VirtualStore = virtualStore ?? RegBranch.Missing
    };

    private static ResolvedConfig Resolve(RegBranch? user = null, RegBranch? machine = null, RegBranch? virtualStore = null) =>
        RegResolver.Resolve(Source(user, machine, virtualStore));

    [TestMethod]
    public void Klon_KazdaVetvaNaSvojeMiestoAjPrazdnaHkcu()
    {
        var src = Source(new RegBranch(), new RegBranch().Set("PathNames", "LogPath", RegRawValue.String(@"Logy\")).AddSection("Prazdna"));

        var ops = RegTools.ClonePlan(src).Ops;

        CollectionAssert.Contains(ops.ToList(), RegWriteOp.CreateKey(RegLocation.User, ""));
        CollectionAssert.Contains(ops.ToList(), RegWriteOp.CreateKey(RegLocation.Machine, "Prazdna"));
        CollectionAssert.Contains(ops.ToList(), new RegWriteOp(RegLocation.Machine, "PathNames", "LogPath", RegRawValue.String(@"Logy\")));
        Assert.IsFalse(ops.Any(o => o.Location == RegLocation.VirtualStore), "VirtualStore neexistuje");
    }

    [TestMethod]
    public void ExportIni_LenZapisaneHodnoty()
    {
        var config = Resolve(machine: new RegBranch().Set("Loging", "TableLogMode", RegRawValue.Dword(15)));

        var ini = RegTools.ToIni(config, false);
        var all = RegTools.ToIni(config, true);

        Assert.AreEqual("15", ini.GetString("Loging", "TableLogMode"));
        Assert.HasCount(1, ini.SectionNames);
        Assert.IsNotNull(all.GetString("Environment", "Logging"), "s predvolenymi aj ostatne");
    }

    [TestMethod]
    public void Porovnanie_RozdielyAPrevzatie()
    {
        var current = Resolve(machine: new RegBranch()
            .Set("Loging", "TableLogMode", RegRawValue.Dword(15))
            .Set("PathNames", "LogPath", RegRawValue.String(@"Logy\")));
        var other = Resolve(machine: new RegBranch()
            .Set("Loging", "TableLogMode", RegRawValue.Dword(7))
            .Set("PathNames", "LogPath", RegRawValue.String(@"Logy\"))
            .Set("Environment", "Logging", RegRawValue.Dword(1)));

        var diffs = RegTools.Compare(current, other).Where(d => !d.IsEqual).ToList();

        CollectionAssert.AreEquivalent(new[] { "Environment\\Logging", "Loging\\TableLogMode" }, diffs.Select(d => d.Section + "\\" + d.Name).ToArray());
        var changes = RegTools.TakeOver(diffs, RegWriteTarget.Registry);
        Assert.AreEqual(7, changes.Single(c => c.Name == "TableLogMode").Value);
        Assert.AreEqual(1, changes.Single(c => c.Name == "Logging").Value);
    }

    [TestMethod]
    public void Prevzatie_PredvolenaVDruhejZmazeHodnotu()
    {
        var current = Resolve(machine: new RegBranch().Set("Loging", "TableLogMode", RegRawValue.Dword(15)));
        var other = Resolve();

        var diff = RegTools.Compare(current, other).Single(d => !d.IsEqual);

        Assert.IsFalse(diff.OtherExplicit);
        Assert.IsNull(RegTools.TakeOver([diff], RegWriteTarget.Registry).Single().Value);
    }

    [TestMethod]
    public void Porovnanie_SuborRegAIni()
    {
        var template = Source();
        var reg = RegFile.Parse("Windows Registry Editor Version 5.00\r\n[HKEY_LOCAL_MACHINE\\SOFTWARE\\CHAPS\\Ina\\Loging]\r\n\"TableLogMode\"=dword:00000003\r\n");

        var fromReg = RegResolver.Resolve(RegTools.SourceFromRegFile(reg, template));
        var fromIni = RegResolver.Resolve(RegTools.SourceFromIni(InissIniFile.Parse("[Loging]\r\nTableLogMode=5\r\n"), template));

        Assert.AreEqual("Ina", fromReg.Source.AppName, "aplikacia zo suboru, ked aktualna v nom nie je");
        Assert.AreEqual(3, fromReg.Find("Loging", "TableLogMode")!.Value);
        Assert.AreEqual(5, fromIni.Find("Loging", "TableLogMode")!.Value);
    }

    [TestMethod]
    public void Cistenie_DruhyPoloziek()
    {
        var machine = new RegBranch()
            .Set("Environment", "BusTxt", RegRawValue.String(""))
            .Set("Environment", "Loggin", RegRawValue.Dword(1))
            .Set("Environment", "Logging", RegRawValue.String("1"))
            .Set("Driver", "TableTimeoutConst", RegRawValue.Dword(250))
            .Set("Driver", "TableTimeoutConst [ms]", RegRawValue.Dword(400))
            .Set("Driver", "PollingInterval", RegRawValue.Dword(100))
            .Set("PathNames", "LogPath", RegRawValue.String(@"Logy\"))
            .Set("ILTIS", "X", RegRawValue.Dword(1));
        var virtualStore = new RegBranch().Set("PathNames", "LogPath", RegRawValue.String(@"DATA\"));

        var items = RegTools.FindCleanup(Resolve(machine: machine, virtualStore: virtualStore));

        RegCleanupItem Item(string name) => items.Single(i => i.Name == name);
        Assert.AreEqual(RegCleanupKind.Leftover, Item("BusTxt").Kind);
        Assert.AreEqual(RegCleanupKind.Unknown, Item("Loggin").Kind);
        Assert.IsFalse(Item("Loggin").Recommended);
        Assert.AreEqual(RegCleanupKind.WrongType, Item("Logging").Kind);
        Assert.AreEqual(RegRawValue.Dword(1), Item("Logging").Replacement);
        Assert.AreEqual(RegCleanupKind.LegacyGhost, Item("TableTimeoutConst").Kind);
        Assert.AreEqual(RegCleanupKind.LegacyName, Item("PollingInterval").Kind);
        Assert.AreEqual("PollingInterval [ms]", Item("PollingInterval").NewName);
        Assert.AreEqual(RegCleanupKind.VirtualStoreCopy, Item("LogPath").Kind);
        Assert.AreEqual(RegRawValue.String(@"Logy\"), Item("LogPath").Reference);
        Assert.AreEqual(RegCleanupKind.UnknownSection, items.Single(i => i.Section == "ILTIS").Kind);

        var ops = RegTools.CleanupPlan(items).Ops;
        CollectionAssert.Contains(ops.ToList(), new RegWriteOp(RegLocation.Machine, "Environment", "Logging", RegRawValue.Dword(1)));
        CollectionAssert.Contains(ops.ToList(), new RegWriteOp(RegLocation.Machine, "Driver", "PollingInterval [ms]", RegRawValue.Dword(100)));
        CollectionAssert.Contains(ops.ToList(), new RegWriteOp(RegLocation.Machine, "Driver", "PollingInterval", null));
        CollectionAssert.Contains(ops.ToList(), new RegWriteOp(RegLocation.VirtualStore, "PathNames", "LogPath", null));
        CollectionAssert.Contains(ops.ToList(), RegWriteOp.DeleteSection(RegLocation.Machine, "ILTIS"));
    }

    [TestMethod]
    public void VetvaHkcu_ZalozenieSKopiouPerUserHodnot()
    {
        var config = Resolve(machine: new RegBranch()
            .Set("Tables", "Enabled3", RegRawValue.Dword(0))
            .Set("Tables", "BlackOut", RegRawValue.Dword(1)));

        var ops = RegTools.CreateUserBranchPlan(config, true).Ops;

        Assert.AreEqual(RegWriteOp.CreateKey(RegLocation.User, ""), ops[0]);
        CollectionAssert.Contains(ops.ToList(), new RegWriteOp(RegLocation.User, "Tables", "Enabled3", RegRawValue.Dword(0)));
        Assert.IsFalse(ops.Any(o => o.Name == "BlackOut"), "BlackOut je pre cely pocitac");
        Assert.HasCount(1, RegTools.CreateUserBranchPlan(config, false).Ops);
    }

    [TestMethod]
    public void VetvaHkcu_ZrusenieSPresunomDoHklm()
    {
        var config = Resolve(user: new RegBranch().Set("Tables", "Enabled3", RegRawValue.Dword(0)));

        var ops = RegTools.RemoveUserBranchPlan(config, true).Ops;

        Assert.AreEqual(new RegWriteOp(RegLocation.Machine, "Tables", "Enabled3", RegRawValue.Dword(0)), ops[0]);
        Assert.AreEqual(RegWriteOp.DeleteBranch(RegLocation.User), ops[^1]);
    }
}
