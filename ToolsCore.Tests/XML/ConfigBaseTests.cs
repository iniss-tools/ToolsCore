using System.Xml;
using System.Xml.Serialization;
using ToolsCore.XML;

namespace ToolsCore.Tests.XML;

/// <summary>
/// Konfiguracia programu zo starsich verzii: zrusene volby sa ignoruju, rezim ponuky sa prevezme do panela nastrojov,
/// pisma popisov a buniek do pisma rozhrania a tabuliek.
/// </summary>
[TestClass]
public class ConfigBaseTests
{
    [TestMethod]
    [DataRow("0", true)]
    [DataRow("1", true)]
    [DataRow("2", false)]
    public void StaraKonfiguracia_RezimPonuky_PrevezmeSaDoPanelaNastrojov(string desktopMenu, bool showToolbar)
    {
        var config = Deserialize($"""
            <CONFIG>
              <DebugModeGUI>2</DebugModeGUI>
              <DesktopMenu>{desktopMenu}</DesktopMenu>
              <ClassicGUI>true</ClassicGUI>
              <Language>CZ</Language>
              <ShowRowsHeader>false</ShowRowsHeader>
              <LoggingInfo>false</LoggingInfo>
              <FitLastColumn>false</FitLastColumn>
            </CONFIG>
            """);

        Assert.AreEqual(showToolbar, config.ShowToolbar);
        Assert.AreEqual(AppLanguage.Czech, config.Language);
    }

    [TestMethod]
    public void Ulozenie_ZruseneVolby_SaNezapisuju()
    {
        var xml = Serialize(new ConfigBase { ShowToolbar = false });

        Assert.Contains("<ShowToolbar>false</ShowToolbar>", xml);
        Assert.DoesNotContain("DesktopMenu", xml);
        Assert.IsFalse(Deserialize(xml).ShowToolbar);
    }

    [TestMethod]
    public void Kopia_PrenesieZobrazeniePanelaNastrojov()
    {
        var copy = new ConfigBase { ShowToolbar = false } with { };

        Assert.IsFalse(copy.ShowToolbar);
    }

    [TestMethod]
    public void StaraKonfiguracia_SystemovePisma_OstanuPredvolene()
    {
        var system = SystemFonts.DefaultFont;
        var config = Deserialize($"""
            <CONFIG>
              <ControlFonts>
                <Labels><f family="{system.FontFamily.Name}" gunit="pt" size="{Size(system)}" style="r" set="238" /></Labels>
                <Buttons><f family="{system.FontFamily.Name}" gunit="pt" size="{Size(system)}" style="r" set="238" /></Buttons>
                <Menu><f family="Segoe UI" gunit="pt" size="9" style="r" /></Menu>
                <TableCells><f family="{system.FontFamily.Name}" gunit="pt" size="{Size(system)}" style="r" set="238" /></TableCells>
              </ControlFonts>
            </CONFIG>
            """);

        Assert.IsNull(config.Fonts.Interface);
        Assert.IsNull(config.Fonts.Tables);
        Assert.IsNull(config.Fonts.Editor);
    }

    [TestMethod]
    public void StaraKonfiguracia_ZmenenePisma_PrevezmuSaDoRozhraniaATabuliek()
    {
        var config = Deserialize("""
            <CONFIG>
              <ControlFonts>
                <Labels><f family="Segoe UI" gunit="pt" size="10" style="r" /></Labels>
                <Buttons><f family="Arial" gunit="pt" size="12" style="b" /></Buttons>
                <TableCells><f family="Tahoma" gunit="pt" size="11" style="r" /></TableCells>
              </ControlFonts>
            </CONFIG>
            """);

        Assert.AreEqual(new FontChoice("Segoe UI", 10), config.Fonts.Interface);
        Assert.AreEqual(new FontChoice("Tahoma", 11), config.Fonts.Tables);
        Assert.DoesNotContain("ControlFonts", Serialize(config));
    }

    [TestMethod]
    public void Pisma_UlozenieANacitanie_ZachovajuVolby()
    {
        var config = new ConfigBase();
        config.Fonts.Interface = new FontChoice("Segoe UI", 9.5f);
        config.Fonts.Editor = new FontChoice("Cascadia Mono", 11);

        var loaded = Deserialize(Serialize(config));

        Assert.AreEqual(config.Fonts, loaded.Fonts);
        Assert.IsNull(loaded.Fonts.Tables);
        Assert.AreEqual(9.5f, loaded.Fonts.Cells.SizeInPoints, 0.01f, "tabulky bez vlastneho pisma maju pismo rozhrania");
    }

    [TestMethod]
    public void Kopia_PismaSuNezavisle()
    {
        var config = new ConfigBase();
        var copy = config with { };

        copy.Fonts.Tables = new FontChoice("Arial", 12);

        Assert.IsNull(config.Fonts.Tables);
        Assert.AreNotEqual(config.Fonts, copy.Fonts);
    }

    private static string Size(Font font) => font.SizeInPoints.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Serialize(ConfigBase config)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(ConfigBase)).Serialize(writer, config);
        return writer.ToString();
    }

    private static ConfigBase Deserialize(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        return (ConfigBase)new XmlSerializer(typeof(ConfigBase)).Deserialize(reader)!;
    }
}
