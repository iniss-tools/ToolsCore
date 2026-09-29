using System.Xml;
using System.Xml.Serialization;
using ToolsCore.Commands;
using ToolsCore.XML;

namespace ToolsCore.Tests.XML;

/// <summary>
/// Klavesove skratky v config.xml - format sa nezmenil, stare nastavenia sa nacitaju.
/// </summary>
[TestClass]
public class ShortcutMapTests
{
    private static readonly CommandInfo Open = new("OpenGVD", "Otvoriť", Shortcut.CtrlO);
    private static readonly CommandInfo New = new("NewGVD", "Nový", Shortcut.None);
    private static readonly CommandInfo Save = new("Save", "Uložiť", Shortcut.CtrlS);

    /// <summary>
    /// Obal ako v nastaveniach programu.
    /// </summary>
    public class Config
    {
        [XmlElement("Shortcuts")]
        public ShortcutMap Shortcuts { get; set; } = new();

        public bool After { get; set; }
    }

    private static Config Read(string xml)
    {
        using var reader = new StringReader(xml);
        return (Config)new XmlSerializer(typeof(Config)).Deserialize(XmlReader.Create(reader))!;
    }

    private static string Write(Config config)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(Config)).Serialize(writer, config);
        return writer.ToString();
    }

    [TestMethod]
    public void Read_StaryFormatNastaveni_NacitaSkratkyPodlaNazvuPrvku()
    {
        var config = Read("""
            <Config>
              <Shortcuts>
                <NewGVD sc="CtrlN" />
                <OpenGVD sc="None" />
              </Shortcuts>
              <After>true</After>
            </Config>
            """);

        Assert.AreEqual(Shortcut.CtrlN, config.Shortcuts.Get(New));
        Assert.AreEqual(Shortcut.None, config.Shortcuts.Get(Open));
        Assert.AreEqual(Shortcut.CtrlS, config.Shortcuts.Get(Save));
        Assert.IsTrue(config.After, "prvky za skratkami sa musia nacitat");
    }

    [TestMethod]
    public void Read_NeplatnaHodnotaAPrazdnyZoznam_Preskoci()
    {
        var config = Read("""
            <Config>
              <Shortcuts>
                <OpenGVD sc="Nezname" />
                <Save sc="12345" />
                <NewGVD />
              </Shortcuts>
              <After>true</After>
            </Config>
            """);

        Assert.AreEqual(0, config.Shortcuts.Count);
        Assert.IsTrue(config.After);

        var empty = Read("<Config><Shortcuts /><After>true</After></Config>");
        Assert.AreEqual(0, empty.Shortcuts.Count);
        Assert.IsTrue(empty.After);
    }

    [TestMethod]
    public void Write_ZapisANacitanie_ZachovaSkratky()
    {
        var config = new Config { After = true };
        config.Shortcuts.Set(Open.Id, Shortcut.CtrlShiftO);
        config.Shortcuts.Set(Save.Id, Shortcut.None);

        var xml = Write(config);
        var loaded = Read(xml);

        StringAssert.Contains(xml, "<OpenGVD sc=\"CtrlShiftO\" />");
        Assert.AreEqual(Shortcut.CtrlShiftO, loaded.Shortcuts.Get(Open));
        Assert.AreEqual(Shortcut.None, loaded.Shortcuts.Get(Save));
        Assert.IsTrue(loaded.After);
    }

    [TestMethod]
    public void Rows_UpravaVNastaveniach_PrejdeDoSkratiekAKopiaJeNezavisla()
    {
        var map = new ShortcutMap();
        var copy = map.Clone();
        var rows = copy.ToRows([New, Open]);
        Assert.AreEqual("Nový", rows[0].Name);
        Assert.AreEqual(Shortcut.CtrlO, rows[1].Shortcut.Value);

        rows[1].Shortcut = Shortcut.F2;
        copy.SetFromRows(rows);

        Assert.AreEqual(Shortcut.F2, copy.Get(Open));
        Assert.AreEqual(Shortcut.CtrlO, map.Get(Open));
        CollectionAssert.AreEqual(new[] { Shortcut.None, Shortcut.CtrlO },
            ShortcutMap.DefaultRows([New, Open]).Select(r => r.Shortcut.Value).ToArray());
    }
}
