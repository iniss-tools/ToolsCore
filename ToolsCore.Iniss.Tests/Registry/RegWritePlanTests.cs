using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Plan zapisu zmien - zmena musi mat u INISSu ucinok v oboch rezimoch spustenia; subor .reg na import do HKLM.
/// </summary>
[TestClass]
public class RegWritePlanTests
{
    private static ResolvedConfig Resolve(RegBranch? user = null, RegBranch? machine = null, RegBranch? virtualStore = null, string? ini = null) =>
        RegResolver.Resolve(new InissConfigSource
        {
            AppName = "INISS - Test",
            Version = new RegVersion(3, 39),
            User = user ?? RegBranch.Missing,
            Machine = machine ?? new RegBranch(),
            VirtualStore = virtualStore ?? RegBranch.Missing,
            Ini = ini is null ? null : InissIniFile.Parse(ini)
        });

    [TestMethod]
    public void Register_DoHklmAZmazeKopiuVoVirtualStoreAjVIni()
    {
        var config = Resolve(machine: new RegBranch().Set("PathNames", "LogPath", RegRawValue.String(@"Logy\")),
            virtualStore: new RegBranch().Set("PathNames", "LogPath", RegRawValue.String(@"DATA\")),
            ini: "[PathNames]\r\nLogPath=INI\\\r\n");

        var plan = RegWritePlanner.Plan(config, [new RegChange("PathNames", "LogPath", RegValueType.String, @"LOG\", RegWriteTarget.Registry)]);

        CollectionAssert.AreEqual(new[]
        {
            new RegWriteOp(RegLocation.Machine, "PathNames", "LogPath", RegRawValue.String(@"LOG\")),
            new RegWriteOp(RegLocation.VirtualStore, "PathNames", "LogPath", null),
            new RegWriteOp(RegLocation.Ini, "PathNames", "LogPath", null)
        }, plan.Ops.ToArray());
        Assert.IsTrue(plan.WritesMachine);
    }

    [TestMethod]
    public void Register_IniSaNechaPodlaVolby()
    {
        var config = Resolve(ini: "[Loging]\r\nTableLogMode=15\r\n");
        var change = new RegChange("Loging", "TableLogMode", RegValueType.Dword, 7, RegWriteTarget.Registry);

        var plan = RegWritePlanner.Plan(config, [change], removeIniOverride: false);

        Assert.IsFalse(plan.WritesIni);
        CollectionAssert.AreEqual(new[] { change }, plan.IniStillOverrides.ToArray());
    }

    [TestMethod]
    public void Register_PerUserSVetvouHkcuDoHkcu()
    {
        var config = Resolve(user: new RegBranch());

        var plan = RegWritePlanner.Plan(config, [new RegChange("Environment", "Logging", RegValueType.Bool, 1, RegWriteTarget.Registry)]);

        Assert.AreEqual(RegLocation.User, plan.Ops.Single().Location);
        Assert.IsFalse(plan.WritesMachine);
    }

    [TestMethod]
    public void Register_StaryNazovSaZmaze()
    {
        var config = Resolve(machine: new RegBranch().Set("Driver", "TableTimeoutConst", RegRawValue.Dword(250)));

        var plan = RegWritePlanner.Plan(config, [new RegChange("Driver", "TableTimeoutConst [ms]", RegValueType.Dword, 300, RegWriteTarget.Registry)]);

        Assert.IsTrue(plan.Ops.Contains(new RegWriteOp(RegLocation.Machine, "Driver", "TableTimeoutConst", null)));
    }

    [TestMethod]
    public void Ini_ZapiseLenDoSuboru()
    {
        var config = Resolve(machine: new RegBranch().Set("Loging", "TableLogMode", RegRawValue.Dword(3)));

        var plan = RegWritePlanner.Plan(config, [new RegChange("Loging", "TableLogMode", RegValueType.Dword, 15, RegWriteTarget.Ini)]);

        CollectionAssert.AreEqual(new[] { new RegWriteOp(RegLocation.Ini, "Loging", "TableLogMode", RegRawValue.Dword(15)) }, plan.Ops.ToArray());
    }

    [TestMethod]
    public void ObnovitPredvolenu_ZmazeZoVsetkychVrstiev()
    {
        var config = Resolve(user: new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(1)),
            machine: new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(1)),
            virtualStore: new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(0)),
            ini: "[Environment]\r\nLogging=1\r\n");

        var plan = RegWritePlanner.Plan(config, [new RegChange("Environment", "Logging", RegValueType.Bool, null, RegWriteTarget.Registry)]);

        CollectionAssert.AreEquivalent(new[] { RegLocation.Ini, RegLocation.User, RegLocation.VirtualStore, RegLocation.Machine },
            plan.Ops.Select(o => o.Location).ToArray());
        Assert.IsTrue(plan.Ops.All(o => o.IsDelete));
    }

    [TestMethod]
    public void Register_SekciaZIniRegisterNecita()
    {
        var config = Resolve(ini: "[Driver0]\r\nTableClass=4\r\n");

        var plan = RegWritePlanner.Plan(config, [new RegChange("Driver0", "NumRetries", RegValueType.Dword, 3, RegWriteTarget.Registry)]);

        Assert.HasCount(1, plan.RegistryNotRead);
    }

    [TestMethod]
    public void OdstranenieSekcie_ZoVsetkychMiestAjZIni()
    {
        var config = Resolve(machine: new RegBranch().Set("Driver3", "TablePort", RegRawValue.String("COM3")),
            virtualStore: new RegBranch().Set("Driver3", "TableClass", RegRawValue.Dword(4)),
            ini: "[Driver3]\r\nTablePort=COM4\r\n");

        var plan = RegWritePlanner.PlanRemoveSection(config, "Driver3");

        CollectionAssert.AreEquivalent(new[] { RegLocation.Ini, RegLocation.VirtualStore, RegLocation.Machine }, plan.Ops.Select(o => o.Location).ToArray());
        Assert.IsTrue(plan.Ops.All(o => o.IsSectionDelete));
        StringAssert.Contains(RegFile.FromPlan(plan, "INISS - Test", RegLocation.Machine).ToString(), @"[-HKEY_LOCAL_MACHINE\SOFTWARE\CHAPS\INISS - Test\Driver3]");
    }

    [TestMethod]
    public void RegFile_FormatHodnotAZmazanie()
    {
        var file = new RegFile();
        var path = RegFile.SectionPath(RegFile.MachineRoot, "INISS - Test", "Driver0");

        file.Set(path, "TableClass", RegRawValue.Dword(128));
        file.Set(path, "RtcLight", RegRawValue.Dword(-1));
        file.Set(path, "TablePort", RegRawValue.String(@"\\PIPE\""x"""));
        file.Set(path, "State", RegRawValue.Binary([0x01, 0xAB]));
        file.Delete(path, "TableTimeoutConst");

        Assert.AreEqual(
            "Windows Registry Editor Version 5.00\r\n\r\n[HKEY_LOCAL_MACHINE\\SOFTWARE\\CHAPS\\INISS - Test\\Driver0]\r\n"
            + "\"TableClass\"=dword:00000080\r\n\"RtcLight\"=dword:ffffffff\r\n\"TablePort\"=\"\\\\\\\\PIPE\\\\\\\"x\\\"\"\r\n"
            + "\"State\"=hex:01,ab\r\n\"TableTimeoutConst\"=-\r\n",
            file.ToString());
    }

    [TestMethod]
    public void RegFile_ZPlanuLenOperacieDanehoMiesta()
    {
        var config = Resolve(virtualStore: new RegBranch().Set("Pauses", "EndPause", RegRawValue.Dword(100)));
        var plan = RegWritePlanner.Plan(config, [new RegChange("Pauses", "EndPause", RegValueType.Dword, 300, RegWriteTarget.Registry)]);

        var text = RegFile.FromPlan(plan, "INISS - Test", RegLocation.Machine).ToString();

        StringAssert.Contains(text, "[HKEY_LOCAL_MACHINE\\SOFTWARE\\CHAPS\\INISS - Test\\Pauses]\r\n\"EndPause\"=dword:0000012c\r\n");
        Assert.IsFalse(text.Contains("=-", StringComparison.Ordinal), "zmazanie kopie vo VirtualStore nepatri do importu HKLM");
    }
}
