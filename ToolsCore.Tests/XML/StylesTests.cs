using System.Globalization;
using System.Xml.Linq;
using ToolsCore.XML;

namespace ToolsCore.Tests.XML;

/// <summary>
/// Farebne schemy: hodnoty zo zakladu a zmien, ulozenie len zmien (verzia 2), prevod stareho suboru stylov a vyber
/// schemy podla rezimu Windows.
/// </summary>
[TestClass]
public class StylesTests
{
    private const string LegacyFile = """
        <STYLES>
          <Style name="Default" used="false">
            <ControlsColorScheme>
              <Button bold="false" b="ButtonFace" f="ControlText" />
              <Panel bold="false" b="Control" f="ControlText" />
              <Highlight bold="false" b="Highlight" f="HighlightText" />
            </ControlsColorScheme>
          </Style>
          <Style name="Dark" used="false">
            <DefaultStyle>false</DefaultStyle>
            <DarkTitlebar>true</DarkTitlebar>
            <ControlsColorScheme>
              <Button bold="false" b="#333337" f="White" />
              <Panel bold="false" b="#1E1E1E" f="White" />
              <Highlight bold="false" b="#0078D7" f="White" />
            </ControlsColorScheme>
          </Style>
          <Style name="Moja" used="true">
            <ControlsColorScheme>
              <Panel bold="false" b="#101010" f="White" />
              <Highlight bold="false" b="#FF8800" f="Black" />
            </ControlsColorScheme>
          </Style>
        </STYLES>
        """;

    [TestMethod]
    public void Zaklad_UrcujeFarbyATmaveZahlavie()
    {
        var styles = Styles<Style>.CreateDefault();
        var light = styles[StyleNames.Light];
        var dark = styles[StyleNames.Dark];

        Assert.AreEqual(SystemColors.ButtonFace, light.ControlsColorScheme.Button.BackColor);
        Assert.AreEqual(Color.FromArgb(30, 30, 30).ToArgb(), dark.ControlsColorScheme.Panel.BackColor.ToArgb());
        Assert.AreEqual(dark.ControlsColorScheme.Panel.BackColor, dark.ControlsColorScheme.StatusBar.BackColor, "stavovy riadok ako panel");
        Assert.IsTrue(dark.DarkTitleBar && dark.DarkScrollBar && !dark.ControlsDefaultStyle);
        Assert.IsTrue(!light.DarkTitleBar && light.ControlsDefaultStyle);
    }

    [TestMethod]
    public void Zmena_PrepiseLenZadanuCast()
    {
        var style = Styles<Style>.CreateStyle("Moja", SchemeBase.Dark);

        style.SetOverride(UiColors.Highlight, new ColorOverride(Back: Color.Orange));

        Assert.AreEqual(Color.Orange, style.ControlsColorScheme.Highlight.BackColor);
        Assert.AreEqual(Color.White, style.ControlsColorScheme.Highlight.ForeColor, "popredie zo zakladu");
        style.SetOverride(UiColors.Highlight, new ColorOverride());
        Assert.IsEmpty(style.Overrides, "prazdna zmena = hodnota zo zakladu");
        Assert.AreEqual(Color.FromArgb(0, 120, 215).ToArgb(), style.ControlsColorScheme.Highlight.BackColor.ToArgb());
    }

    [TestMethod]
    public void StarySubor_PrevediePodlaZakladuAZapiseLenRozdiely()
    {
        var styles = Styles<Style>.FromLegacy(XElement.Parse(LegacyFile));

        Assert.IsEmpty(styles[StyleNames.Light].Overrides);
        Assert.IsEmpty(styles[StyleNames.Dark].Overrides);
        var custom = styles["Moja"];
        Assert.AreEqual(SchemeBase.Dark, custom.Base, "tmavy panel = tmavy zaklad");
        Assert.AreEqual("Moja", styles.Active);
        CollectionAssert.AreEquivalent(new[] { "Ui.Panel", "Ui.Highlight" }, custom.Overrides.Keys.ToArray());
        Assert.IsNull(custom.Overrides["Ui.Panel"].Fore, "biely text je aj v tmavom zaklade");
        Assert.AreEqual(Color.Black, custom.ControlsColorScheme.Highlight.ForeColor);
    }

    [TestMethod]
    public void Ulozenie_ZachovaZmenyAVyber()
    {
        var styles = Styles<Style>.CreateDefault();
        var custom = styles[StyleNames.Dark].Copy("Moja");
        custom.SetOverride(UiColors.Box, new ColorOverride(Color.Yellow, Color.Navy, null));
        styles.Add((Style)custom);
        styles.FollowSystem = true;
        styles.DarkScheme = "Moja";

        var xml = styles.ToXml();
        var loaded = Styles<Style>.FromXml(XElement.Parse(xml.ToString()));

        Assert.AreEqual(1, xml.Descendants("Color").Count(), "zapisu sa len zmeny");
        Assert.IsTrue(loaded.FollowSystem);
        Assert.AreEqual("Moja", loaded.Current(windowsDark: true).Name);
        Assert.AreEqual(StyleNames.Light, loaded.Current(windowsDark: false).Name);
        Assert.AreEqual(Color.Navy.ToArgb(), loaded["Moja"].ControlsColorScheme.Box.BackColor.ToArgb());
        Assert.AreEqual(SchemeBase.Dark, loaded["Moja"].Base);
    }

    [TestMethod]
    public void NovySubor_BezZabudovanychSchem_IchDoplni()
    {
        var styles = Styles<Style>.FromXml(XElement.Parse("""<SCHEMES version="2" active="Chybajuca"><Scheme name="Moja" base="dark" /></SCHEMES>"""));

        CollectionAssert.AreEqual(new[] { StyleNames.Light, StyleNames.Dark, "Moja" }, styles.Select(s => s.Name).ToArray());
        Assert.AreEqual(StyleNames.Light, styles.Current(false).Name, "neznama schema = prva");
    }

    [TestMethod]
    public void Kopia_JeNezavisla()
    {
        var styles = Styles<Style>.CreateDefault();
        var copy = new Styles<Style>(styles);

        copy[StyleNames.Light].SetOverride(UiColors.Panel, new ColorOverride(Back: Color.Pink));

        Assert.IsEmpty(styles[StyleNames.Light].Overrides);
        Assert.AreNotEqual(Color.Pink, styles[StyleNames.Light].ControlsColorScheme.Panel.BackColor);
    }

    [TestMethod]
    [DataRow("sk", "Tlačidlo", "Svetlá")]
    [DataRow("cs", "Tlačítko", "Světlá")]
    public void Nazvy_VJazykuProgramu(string culture, string button, string light)
    {
        var old = CultureInfo.CurrentUICulture;
        try
        {
            var style = Styles<Style>.CreateDefault()[StyleNames.Light];
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            Assert.AreEqual(button, UiColors.Button.Name);
            Assert.AreEqual(light, style.ToString());
        }
        finally
        {
            CultureInfo.CurrentUICulture = old;
        }
    }

    [TestMethod]
    public void Kontrast_PodlaWcag()
    {
        Assert.AreEqual(21, ColorContrast.Ratio(Color.Black, Color.White), 0.01);
        Assert.AreEqual(1, ColorContrast.Ratio(Color.Red, Color.Red), 0.01);
        Assert.IsLessThan(ColorContrast.MinimumText, ColorContrast.Ratio(Color.Gray, Color.White));
    }
}
