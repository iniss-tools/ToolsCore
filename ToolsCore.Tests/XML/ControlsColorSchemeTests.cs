using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using ToolsCore.XML;

namespace ToolsCore.Tests.XML;

/// <summary>
/// Farby ovladacich prvkov - nazvy v jazyku programu, nie v jazyku pri nacitani styles.xml.
/// </summary>
[TestClass]
public class ControlsColorSchemeTests
{
    // AppInit cita styles.xml skor, nez nastavi jazyk programu - nazvy sa nesmu zapamatat pri nacitani
    [TestMethod]
    [DataRow("sk", "cs", "Tlačítko", "Popisek", "Zvýraznění")]
    [DataRow("cs", "sk", "Tlačidlo", "Popisok", "Zvýraznenie")]
    public void NazvyFarieb_VJazykuProgramuPoNacitaniStylu(string loadCulture, string appCulture, string button, string label, string highlight)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(loadCulture);
            var light = Deserialize(Serialize(Style.DefaultLightStyle));
            var dark = Deserialize(Serialize(Style.DefaultDarkStyle));

            CultureInfo.CurrentUICulture = new CultureInfo(appCulture);
            foreach (var scheme in new[] { light.ControlsColorScheme, dark.ControlsColorScheme, dark.ControlsColorScheme with { }, Style.DefaultDarkStyle.ControlsColorScheme })
            {
                Assert.AreEqual(button, scheme.Button.Name);
                Assert.AreEqual(label, scheme.Label.ToString());
                Assert.AreEqual(highlight, scheme.Highlight.Name);
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = old;
        }
    }

    [TestMethod]
    public void Nacitanie_ZachovaFarbyAPriznakyZoSchemy()
    {
        var loaded = Deserialize(Serialize(Style.DefaultDarkStyle)).ControlsColorScheme;

        Assert.AreEqual(Color.FromArgb(62, 62, 66).ToArgb(), loaded.Border.ForeColor.ToArgb());
        Assert.IsTrue(loaded.Border.DisableBackColorEdit);
        Assert.IsTrue(loaded.Button.DisableFontBoldEdit);
        Assert.IsFalse(loaded.Button.DisableBackColorEdit);
    }

    private static string Serialize(Style style)
    {
        using var writer = new StringWriter();
        new XmlSerializer(typeof(Style)).Serialize(writer, style);
        return writer.ToString();
    }

    private static Style Deserialize(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        return (Style)new XmlSerializer(typeof(Style)).Deserialize(reader)!;
    }
}
