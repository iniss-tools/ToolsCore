using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Subor .reg - zapis vetiev a planov, citanie suborov z regeditu (UTF-16, REGEDIT4, pokracovanie riadkov).
/// </summary>
[TestClass]
public class RegFileTests
{
    [TestMethod]
    public void Citanie_HklmHkcuAVirtualStoreSoSpravnymTypom()
    {
        const string text = "Windows Registry Editor Version 5.00\r\n\r\n"
                            + "[HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\CHAPS\\INISS - A]\r\n\r\n"
                            + "[HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\CHAPS\\INISS - A\\PathNames]\r\n"
                            + "\"LogPath\"=\"Logy\\\\\"\r\n"
                            + "\"Cesta \\\"x\\\"\"=\"a\"\r\n"
                            + "@=\"predvolena\"\r\n\r\n"
                            + "[HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\CHAPS\\INISS - A\\Loging]\r\n"
                            + "\"TableLogMode\"=dword:0000000f\r\n"
                            + "\"Zmazat\"=-\r\n\r\n"
                            + "[HKEY_CURRENT_USER\\Software\\CHAPS\\INISS - A\\Tables]\r\n"
                            + "\"Enabled0\"=dword:ffffffff\r\n\r\n"
                            + "[HKEY_CURRENT_USER\\Software\\Classes\\VirtualStore\\MACHINE\\SOFTWARE\\WOW6432Node\\CHAPS\\INISS - A\\PathNames]\r\n"
                            + "\"LogPath\"=\"DATA\\\\\"\r\n\r\n"
                            + "[HKEY_LOCAL_MACHINE\\SOFTWARE\\Ina\\Vec]\r\n"
                            + "\"X\"=dword:00000001\r\n\r\n"
                            + "[-HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\CHAPS\\INISS - A\\Stare]\r\n";

        var branches = RegFile.Parse(text);

        Assert.HasCount(3, branches);
        var machine = branches.Single(b => b.Location == RegLocation.Machine).Branch;
        Assert.AreEqual(@"Logy\", machine.Get("PathNames", "LogPath")!.Text);
        Assert.AreEqual("a", machine.Get("PathNames", "Cesta \"x\"")!.Text);
        Assert.AreEqual(15, machine.Get("Loging", "TableLogMode")!.Number);
        Assert.IsNull(machine.Get("Loging", "Zmazat"));
        Assert.IsFalse(machine.HasSection("Stare"));
        Assert.AreEqual(-1, branches.Single(b => b.Location == RegLocation.User).Branch.Get("Tables", "Enabled0")!.Number);
        Assert.AreEqual(@"DATA\", branches.Single(b => b.Location == RegLocation.VirtualStore).Branch.Get("PathNames", "LogPath")!.Text);
        Assert.IsTrue(branches.All(b => b.AppName == "INISS - A"));
    }

    [TestMethod]
    public void ZapisACitanie_RoundTripVratane32BitovejCestyABinary()
    {
        var branch = new RegBranch()
            .Set("Font", "font weight", RegRawValue.Dword(700))
            .Set("ColumnWidth", "LBVlaky", RegRawValue.Binary(Enumerable.Range(0, 40).Select(i => (byte)i).ToArray()))
            .Set("PathNames", "LogPath", RegRawValue.String(@"C:\Log ""x"""))
            .AddSection("Prazdna");
        var file = new RegFile();
        file.AddBranch(branch, RegFile.MachineRoot, "INISS");
        file.AddBranch(new RegBranch().Set("Tables", "Enabled1", RegRawValue.Dword(0)), RegFile.UserRoot, "INISS");

        var back = RegFile.Parse(file.ToString());

        var machine = back.Single(b => b.Location == RegLocation.Machine).Branch;
        Assert.AreEqual(700, machine.Get("Font", "font weight")!.Number);
        CollectionAssert.AreEqual(branch.Get("ColumnWidth", "LBVlaky")!.Bytes, machine.Get("ColumnWidth", "LBVlaky")!.Bytes);
        Assert.AreEqual(@"C:\Log ""x""", machine.Get("PathNames", "LogPath")!.Text);
        Assert.IsTrue(machine.HasSection("Prazdna"));
        Assert.AreEqual(0, back.Single(b => b.Location == RegLocation.User).Branch.Get("Tables", "Enabled1")!.Number);
    }

    [TestMethod]
    public void Citanie_Regedit4APokracovanieHex()
    {
        const string text = "REGEDIT4\n\n[HKEY_LOCAL_MACHINE\\SOFTWARE\\CHAPS\\INISS\\ColumnWidth]\n"
                            + "\"LBVlaky\"=hex:01,02,\\\n  03,04\n\"Rozsirene\"=hex(2):25,00,00,00\n";

        var branch = RegFile.Parse(text).Single().Branch;

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, branch.Get("ColumnWidth", "LBVlaky")!.Bytes);
        Assert.AreEqual(RegRawKind.Other, branch.Get("ColumnWidth", "Rozsirene")!.Kind);
    }

    [TestMethod]
    public void Citanie_InySuborJeChyba() => Assert.ThrowsExactly<FormatException>(() => RegFile.Parse("[PathNames]\r\nLogPath=x\r\n"));

    [TestMethod]
    public void Plan_ZalozenieKlucaAZmazanieVetvy()
    {
        var plan = new RegWritePlan([RegWriteOp.CreateKey(RegLocation.User, ""), RegWriteOp.DeleteBranch(RegLocation.Machine)]);

        StringAssert.Contains(RegFile.FromPlan(plan, "INISS", RegLocation.User).ToString(), @"[HKEY_CURRENT_USER\Software\CHAPS\INISS]");
        StringAssert.Contains(RegFile.FromPlan(plan, "INISS", RegLocation.Machine).ToString(), @"[-HKEY_LOCAL_MACHINE\SOFTWARE\CHAPS\INISS]");
        Assert.IsFalse(plan.Ops[0].IsDelete);
        Assert.IsTrue(plan.Ops[1].IsSectionDelete);
    }
}
