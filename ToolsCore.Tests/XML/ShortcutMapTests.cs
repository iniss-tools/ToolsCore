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

        Assert.AreEqual((Keys)Shortcut.CtrlN, config.Shortcuts.Get(New));
        Assert.AreEqual((Keys)Shortcut.None, config.Shortcuts.Get(Open));
        Assert.AreEqual((Keys)Shortcut.CtrlS, config.Shortcuts.Get(Save));
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
        config.Shortcuts.Set(Open.Id, Keys.Control | Keys.Shift | Keys.O);
        config.Shortcuts.Set(Save.Id, Keys.None);

        var xml = Write(config);
        var loaded = Read(xml);

        StringAssert.Contains(xml, "<OpenGVD sc=\"CtrlShiftO\" />");
        Assert.AreEqual((Keys)Shortcut.CtrlShiftO, loaded.Shortcuts.Get(Open));
        Assert.AreEqual((Keys)Shortcut.None, loaded.Shortcuts.Get(Save));
        Assert.IsTrue(loaded.After);
    }

    [TestMethod]
    public void Write_KombinaciaMimoShortcut_ZapiseAkoTextANacitaSa()
    {
        var config = new Config { After = true };
        config.Shortcuts.Set(Open.Id, Keys.Control | Keys.Alt | Keys.K);

        var xml = Write(config);
        var loaded = Read(xml);

        StringAssert.Contains(xml, "<OpenGVD sc=\"Ctrl+Alt+K\" />");
        Assert.AreEqual(Keys.Control | Keys.Alt | Keys.K, loaded.Shortcuts.Get(Open));
    }

    [TestMethod]
    [DataRow("Shift+A")]
    [DataRow("A")]
    [DataRow("-1")]
    [DataRow("Ctrl+Nieco")]
    public void Read_NeplatnaKombinacia_Preskoci(string value)
    {
        var config = Read($"<Config><Shortcuts><OpenGVD sc=\"{value}\" /></Shortcuts><After>true</After></Config>");

        Assert.AreEqual(0, config.Shortcuts.Count);
        Assert.AreEqual(Open.DefaultKeys, config.Shortcuts.Get(Open));
    }

    [TestMethod]
    public void Reset_AKopia_VratiPredvolenuANezavisia()
    {
        var map = new ShortcutMap();
        map.Set(Open.Id, Keys.F2);
        var copy = map.Clone();

        copy.Reset(Open.Id);

        Assert.AreEqual((Keys)Shortcut.CtrlO, copy.Get(Open));
        Assert.AreEqual(Keys.F2, map.Get(Open));
    }
}
