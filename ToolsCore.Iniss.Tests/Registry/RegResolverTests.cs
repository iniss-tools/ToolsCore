using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Vyhodnotenie konfiguracie INISSu - ucinna hodnota a jej zdroj tak, ako ich INISS 3.39 nacita pri starte.
/// </summary>
[TestClass]
public class RegResolverTests
{
    private static readonly RegVersion V339 = new(3, 39);

    private static ResolvedConfig Resolve(RegBranch? user = null, RegBranch? machine = null, RegBranch? virtualStore = null, string? ini = null,
        InissRunMode mode = InissRunMode.Normal, RegVersion? version = null, IReadOnlyList<string>? colors = null,
        IReadOnlyDictionary<int, RegTableInfo>? tables = null) =>
        RegResolver.Resolve(new InissConfigSource
        {
            AppName = "INISS - Test",
            RunMode = mode,
            Version = version ?? V339,
            User = user ?? RegBranch.Missing,
            Machine = machine ?? new RegBranch(),
            VirtualStore = virtualStore ?? RegBranch.Missing,
            Ini = ini is null ? null : InissIniFile.Parse(ini),
            ColorNames = colors,
            Tables = tables
        });

    private static bool Has(ResolvedSetting setting, RegDiagnosticCode code) => setting.Diagnostics.Any(d => d.Code == code);

    [TestMethod]
    public void BezHodnot_PredvolenaHodnota()
    {
        var logging = Resolve().Find("Loging", "TableLogMode")!;

        Assert.AreEqual(3, logging.Value);
        Assert.AreEqual(RegSource.Default, logging.Source);
        Assert.IsTrue(logging.IsDefault);
        Assert.AreEqual(RegLocation.Machine, logging.RegistryLocation);
    }

    [TestMethod]
    public void HklmBezVetvyHkcu_PerUserSekciaZHklm()
    {
        var machine = new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(1));

        var logging = Resolve(machine: machine).Find("Environment", "Logging")!;

        Assert.AreEqual(1, logging.Value);
        Assert.AreEqual(RegSource.Machine, logging.Source);
        Assert.IsFalse(logging.IsDefault);
    }

    [TestMethod]
    public void VetvaHkcu_PerUserSekciaZHkcuHklmSaIgnoruje()
    {
        var user = new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(0));
        var machine = new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(1)).Set("Loging", "TableLogMode", RegRawValue.Dword(15));

        var config = Resolve(user, machine);
        var logging = config.Find("Environment", "Logging")!;

        Assert.IsTrue(config.UserBranchActive);
        Assert.AreEqual(0, logging.Value);
        Assert.AreEqual(RegSource.User, logging.Source);
        Assert.AreEqual(RegLocation.User, logging.RegistryLocation);
        Assert.IsTrue(Has(logging, RegDiagnosticCode.MachineIgnored));
        // spolocna sekcia ostava v HKLM
        Assert.AreEqual(RegSource.Machine, config.Find("Loging", "TableLogMode")!.Source);
    }

    [TestMethod]
    public void VetvaHkcuBezHodnoty_PredvolbaNieHklm()
    {
        var user = new RegBranch();
        var machine = new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(1));

        var logging = Resolve(user, machine).Find("Environment", "Logging")!;

        Assert.AreEqual(0, logging.Value);
        Assert.AreEqual(RegSource.Default, logging.Source);
    }

    [TestMethod]
    public void VirtualStore_BezPravSpravcuPrebijaHklm()
    {
        var machine = new RegBranch().Set("PathNames", "LogPath", RegRawValue.String(@"Logy\"));
        var store = new RegBranch().Set("PathNames", "LogPath", RegRawValue.String(@"DATA\"));

        var normal = Resolve(machine: machine, virtualStore: store).Find("PathNames", "LogPath")!;
        var elevated = Resolve(machine: machine, virtualStore: store, mode: InissRunMode.Elevated).Find("PathNames", "LogPath")!;

        Assert.AreEqual(@"DATA\", normal.Value);
        Assert.AreEqual(RegSource.VirtualStore, normal.Source);
        Assert.IsTrue(Has(normal, RegDiagnosticCode.ShadowedByVirtualStore));
        Assert.AreEqual(@"Logy\", elevated.Value);
        Assert.AreEqual(RegSource.Machine, elevated.Source);
        Assert.IsTrue(Has(elevated, RegDiagnosticCode.VirtualStoreIgnored));
    }

    [TestMethod]
    public void VirtualStore_RovnakaHodnotaBezUpozornenia()
    {
        var machine = new RegBranch().Set("Pauses", "EndPause", RegRawValue.Dword(500));
        var store = new RegBranch().Set("Pauses", "EndPause", RegRawValue.Dword(500));

        var endPause = Resolve(machine: machine, virtualStore: store).Find("Pauses", "EndPause")!;

        Assert.IsEmpty(endPause.Diagnostics);
        CollectionAssert.AreEqual(new[] { RegLayerState.Used, RegLayerState.Shadowed }, endPause.Layers.Select(l => l.State).ToArray());
    }

    [TestMethod]
    public void Ini_PrebijaRegister()
    {
        var machine = new RegBranch().Set("Loging", "TableLogMode", RegRawValue.Dword(3));

        var mode = Resolve(machine: machine, ini: "[Loging]\r\nTableLogMode=15\r\n").Find("Loging", "TableLogMode")!;

        Assert.AreEqual(15, mode.Value);
        Assert.AreEqual(RegSource.Ini, mode.Source);
        Assert.IsTrue(Has(mode, RegDiagnosticCode.ShadowedByIni));
        Assert.AreEqual(RegLocation.Ini, mode.Layers[0].Location);
    }

    [TestMethod]
    public void Ini_NeplatneCisloUpozorni()
    {
        var mode = Resolve(ini: "[Loging]\r\nTableLogMode=0x0F\r\n").Find("Loging", "TableLogMode")!;

        Assert.AreEqual(0, mode.Value);
        Assert.IsTrue(Has(mode, RegDiagnosticCode.IniInvalidNumber));
    }

    [TestMethod]
    public void Driver_PredvolbyPodlaTriedy()
    {
        var machine = new RegBranch().Set("Driver0", "TableClass", RegRawValue.Dword(128)).Set("Driver0", "TablePort", RegRawValue.String("COM3"));

        var config = Resolve(machine: machine);
        var timeout = config.Find("Driver0", "TableTimeoutConst [ms]")!;

        Assert.AreEqual(300, timeout.Value);
        Assert.AreEqual(RegSource.ClassDefault, timeout.Source);
        Assert.AreEqual("57600,n,8,1", config.Find("Driver0", "TablePortParam")!.Value);
        Assert.AreEqual("DCD,DSR", config.Find("Driver0", "CheckModem")!.Value);
        Assert.IsNull(config.FindSection("Driver1"), "neexistujuca linka sa nevypisuje");
    }

    [TestMethod]
    public void Driver_SietovyPortVynulujeSeriovePredvolby()
    {
        var machine = new RegBranch().Set("Driver", "TablePort", RegRawValue.String("3=TCP://10.0.0.20:4001"));

        var config = Resolve(machine: machine);

        Assert.AreEqual("", config.Find("Driver", "TablePortParam")!.Value);
        Assert.AreEqual(0, config.Find("Driver", "SetRTS")!.Value);
        Assert.AreEqual(46, config.Find("Driver", "SyncTimeInterval [s]")!.Value);
        Assert.IsEmpty(config.Find("Driver", "TablePort")!.Diagnostics);
    }

    [TestMethod]
    public void Driver_SietovyPortBezCislaLinkyJeChyba()
    {
        var machine = new RegBranch().Set("Driver2", "TablePort", RegRawValue.String("TCP://10.0.0.20:4001"));

        var port = Resolve(machine: machine).Find("Driver2", "TablePort")!;

        Assert.AreEqual(RegSeverity.Error, port.Diagnostics.Single(d => d.Code == RegDiagnosticCode.NetworkPortWithoutLine).Severity);
    }

    [TestMethod]
    public void Driver_SekciaZIniNecitaRegister()
    {
        var machine = new RegBranch().Set("Driver0", "TableClass", RegRawValue.Dword(4)).Set("Driver0", "NumRetries", RegRawValue.Dword(9));

        var config = Resolve(machine: machine, ini: "[Driver0]\r\nTableClass=128\r\n[Driver5]\r\nTablePort=COM5\r\n");
        var section = config.FindSection("Driver0")!;

        Assert.IsTrue(section.FromIni);
        Assert.AreEqual(128, section.Find("TableClass")!.Value);
        // NumRetries je len v registri - pri sekcii zo suboru plati predvolba
        Assert.AreEqual(5, section.Find("NumRetries")!.Value);
        Assert.AreEqual(RegLayerState.NotRead, section.Find("NumRetries")!.Layers.Single().State);
        Assert.IsTrue(section.Diagnostics.Any(d => d.Code == RegDiagnosticCode.IniSectionReplacesRegistry));
        Assert.IsNotNull(config.FindSection("Driver5"), "sekcia len v .INI sa zalozi");
    }

    [TestMethod]
    public void ZlyTyp_PlatiPredvolbaAINISSHodnotuPrepise()
    {
        var machine = new RegBranch().Set("Grafikon", "MinStay [m]", RegRawValue.String("3"));

        var minStay = Resolve(machine: machine).Find("Grafikon", "MinStay [m]")!;

        Assert.AreEqual(5, minStay.Value);
        Assert.AreEqual(RegSource.Default, minStay.Source);
        Assert.AreEqual(RegLayerState.WrongType, minStay.Layers.Single().State);
        Assert.IsTrue(Has(minStay, RegDiagnosticCode.WrongType));
    }

    [TestMethod]
    public void PrazdnyText_SoSpatnymZapisomJeAkoChybajuci()
    {
        var machine = new RegBranch().Set("Zvuky", "FileNameFyzManag", RegRawValue.String(""));

        var fyzBank = Resolve(machine: machine).Find("Zvuky", "FileNameFyzManag")!;

        Assert.AreEqual("FYZBANK.DAT", fyzBank.Value);
        Assert.IsTrue(Has(fyzBank, RegDiagnosticCode.EmptyText));
    }

    [TestMethod]
    public void StaryNazov_SamotnyPlatiAPremenujeSa()
    {
        var machine = new RegBranch().Set("Driver", "TableTimeoutConst", RegRawValue.Dword(250));

        var timeout = Resolve(machine: machine).Find("Driver", "TableTimeoutConst [ms]")!;

        Assert.AreEqual(250, timeout.Value);
        Assert.IsTrue(Has(timeout, RegDiagnosticCode.LegacyName));
    }

    [TestMethod]
    public void StaryNazov_VedlaNovehoSaNecita()
    {
        var machine = new RegBranch().Set("Driver", "TableTimeoutConst", RegRawValue.Dword(250)).Set("Driver", "TableTimeoutConst [ms]", RegRawValue.Dword(400));

        var timeout = Resolve(machine: machine).Find("Driver", "TableTimeoutConst [ms]")!;

        Assert.AreEqual(400, timeout.Value);
        Assert.IsTrue(Has(timeout, RegDiagnosticCode.LegacyGhost));
    }

    [TestMethod]
    public void Verzia_StarsiaVerziaHodnotuNecita()
    {
        var machine = new RegBranch().Set("Environment", "Logging", RegRawValue.Dword(1));

        var logging = Resolve(machine: machine, version: new RegVersion(3, 34)).Find("Environment", "Logging")!;

        Assert.AreEqual(RegSource.NotRead, logging.Source);
        Assert.IsNull(logging.Value);
        Assert.IsTrue(Has(logging, RegDiagnosticCode.NotInVersion));
    }

    [TestMethod]
    public void Tables_HodnotyTabulZHkcuBlackOutZHklm()
    {
        var user = new RegBranch().Set("Tables", "Enabled3", RegRawValue.Dword(0));
        var machine = new RegBranch().Set("Tables", "BlackOut", RegRawValue.Dword(1)).Set("Tables", "Enabled3", RegRawValue.Dword(1));

        var config = Resolve(user, machine, tables: new Dictionary<int, RegTableInfo> { [0] = new("Odchodová", "Test.2025", 4, 2), [1] = new("Internet", "Test.2025", 5, 0) });
        var tables = config.FindSection("Tables")!;

        Assert.AreEqual(0, tables.Find("Enabled3")!.Value);
        Assert.AreEqual(RegSource.User, tables.Find("Enabled3")!.Source);
        Assert.AreEqual(1, tables.Find("BlackOut")!.Value);
        Assert.AreEqual(RegSource.Default, tables.Find("Enabled0")!.Source, "tabula zo zoznamu sa ukaze aj bez hodnoty");
        Assert.AreEqual("Odchodová", tables.Find("Enabled0")!.Table?.Name);
        Assert.IsNull(tables.Find("Enabled3")!.Table, "tabula mimo dat");
        Assert.IsNull(tables.Find("Enabled2"));
    }

    [TestMethod]
    public void Tables_VynutenyJasPodlaVyrobcuTabule()
    {
        var tables = new Dictionary<int, RegTableInfo>
        {
            [0] = new("ELEN", "T", 8, 1), [1] = new("APEL", "T", 10, 1), [2] = new("ELEKON", "T", 13, 1), [3] = new("LCD1", "T", 5, 1), [4] = new("bez predlohy", "T", null, 1)
        };

        var section = Resolve(tables: tables).FindSection("Tables")!;

        CollectionAssert.AreEqual(new object?[] { 99, 0, 5, -1, null }, Enumerable.Range(0, 5).Select(i => section.Find($"ForceLight{i}")!.Value).ToArray());
    }

    [TestMethod]
    public void Volume_MikrofonPredvolenePodlaSekcie()
    {
        var machine = new RegBranch().AddSection("Volume").AddSection("Volume2").Set("Volume2", "Wave", RegRawValue.Dword(40000));

        var config = Resolve(machine: machine);

        Assert.AreEqual(1, config.Find("Volume", "Mikrofon")!.Value);
        Assert.AreEqual(0, config.Find("Volume2", "Mikrofon")!.Value);
        Assert.AreEqual(40000, config.Find("Volume2", "Wave")!.Value, "prvok mixera");
    }

    [TestMethod]
    public void Farby_NazvyZJazykovejKniznice()
    {
        var names = Enumerable.Range(0, RegCatalog.ColorCount).Select(i => $"Barva {i}").ToList();
        var user = new RegBranch().Set("Colors", "Barva 0", RegRawValue.Dword(255)).Set("Colors", "Obyčajný vlak [Text-Nevybratý]", RegRawValue.Dword(0));

        var colors = Resolve(user, colors: names).FindSection("Colors")!;

        Assert.AreEqual(255, colors.Find("Barva 0")!.Value);
        Assert.IsTrue(colors.Diagnostics.Any(d => d.Code == RegDiagnosticCode.ColorOtherLanguage && d.Name == "Obyčajný vlak [Text-Nevybratý]"));
    }

    [TestMethod]
    public void Nezname_PozostatkyAPreklepy()
    {
        var machine = new RegBranch()
            .Set("Environment", "BusTxt", RegRawValue.String(""))
            .Set("Environment", "Loggin", RegRawValue.Dword(1))
            .Set("ColumnWidth", "LBNovy", RegRawValue.String("x"))
            .Set("ILTIS", "X", RegRawValue.Dword(1));

        var config = Resolve(machine: machine);
        var diags = config.Diagnostics.ToList();

        Assert.IsTrue(diags.Any(d => d.Code == RegDiagnosticCode.Leftover && d.Name == "BusTxt"));
        Assert.IsTrue(diags.Any(d => d.Code == RegDiagnosticCode.Unknown && d.Name == "Loggin"));
        Assert.IsFalse(diags.Any(d => d.Name == "LBNovy"), "ColumnWidth ma nazvy zoznamov za behu");
        Assert.IsTrue(diags.Any(d => d.Code == RegDiagnosticCode.UnknownSection && d.Section == "ILTIS"));
    }

    [TestMethod]
    public void AmplifierPort_DalsieVystupyPodlaCisla()
    {
        var machine = new RegBranch().Set("Zvuky", "AmplifierPort", RegRawValue.String("COM1")).Set("Zvuky", "AmplifierPort2", RegRawValue.String("COM4"));

        var zvuky = Resolve(machine: machine).FindSection("Zvuky")!;

        Assert.AreEqual("COM4", zvuky.Find("AmplifierPort2")!.Value);
        Assert.IsEmpty(zvuky.Diagnostics);
    }
}
