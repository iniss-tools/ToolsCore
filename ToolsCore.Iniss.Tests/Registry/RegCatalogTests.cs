using System.Globalization;
using ToolsCore.Iniss.Registry;

namespace ToolsCore.Tests.Registry;

/// <summary>
/// Katalog nastaveni INISSu vygenerovany z dokumentacie registra.
/// </summary>
[TestClass]
public class RegCatalogTests
{
    private static IEnumerable<string> AllTextKeys() =>
        RegCatalog.Sections.Select(s => s.DescriptionKey)
            .Concat(RegCatalog.Sections.SelectMany(s => s.Settings).Select(s => s.DescriptionKey))
            .Concat(RegCatalog.Sections.SelectMany(s => s.Settings).SelectMany(s => s.Choices).Select(c => c.TextKey))
            .Concat(RegCatalog.Sections.SelectMany(s => s.Settings).Select(s => s.DynamicDefaultKey).OfType<string>())
            .Distinct();

    [TestMethod]
    [DataRow("sk")]
    [DataRow("cs")]
    public void Texty_KazdyKlucMaTextVOboch(string culture)
    {
        var missing = AllTextKeys().Where(k => string.IsNullOrWhiteSpace(RegTexts.GetIn(k, CultureInfo.GetCultureInfo(culture)))).ToList();

        Assert.IsEmpty(missing, string.Join(", ", missing));
    }

    [TestMethod]
    public void Texty_CeskeSaLisiaOdSlovenskych()
    {
        var key = RegCatalog.FindSection("Driver")!.DescriptionKey;

        Assert.AreNotEqual(RegTexts.GetIn(key, CultureInfo.GetCultureInfo("sk")), RegTexts.GetIn(key, CultureInfo.GetCultureInfo("cs")));
    }

    [TestMethod]
    public void Sekcie_NazvyHodnotSuJedinecne()
    {
        foreach (var section in RegCatalog.Sections)
        {
            var duplicates = section.Settings.GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.IsEmpty(duplicates, $"{section.Name}: {string.Join(", ", duplicates)}");
        }
    }

    [TestMethod]
    public void Sekcie_KazdeNastavenieVieSvojuSekciu()
    {
        foreach (var section in RegCatalog.Sections)
        foreach (var setting in section.Settings)
            Assert.AreSame(section, setting.Section);
    }

    [TestMethod]
    public void Farby_JeIchStyridsatDva()
    {
        var colors = RegCatalog.FindSection("Colors")!.Settings.Where(s => s.Kind == RegNameKind.Color).ToList();

        Assert.HasCount(RegCatalog.ColorCount, colors);
        CollectionAssert.AreEqual(Enumerable.Range(0, RegCatalog.ColorCount).ToList(), colors.Select(c => c.ColorIndex).ToList());
    }

    [TestMethod]
    [DataRow("Driver", "Driver", true)]
    [DataRow("Driver7", "Driver", true)]
    [DataRow("Driver07", "Driver", true)]
    [DataRow("Driver98", "Driver", true)]
    [DataRow("Volume8", "Volume", true)]
    [DataRow("Volume9", null, false)]
    [DataRow("Driver99", null, false)]
    [DataRow("environment", "Environment", true)]
    public void FindSection_CislovaneAjBezRozliseniaVelkosti(string name, string? expected, bool found)
    {
        var section = RegCatalog.FindSection(name);

        Assert.AreEqual(found, section is not null);
        Assert.AreEqual(expected, section?.Name);
    }

    [TestMethod]
    public void StaryNazov_LenPriHodnotachCitanychAjBezJednotky()
    {
        var legacy = RegCatalog.Sections.SelectMany(s => s.Settings).Where(s => s.LegacyName is not null)
            .Select(s => $"{s.Section.Name}\\{s.Name}").ToList();

        Assert.HasCount(10, legacy);
        Assert.AreEqual("TableTimeoutConst", RegCatalog.FindSection("Driver")!.Find("TableTimeoutConst [ms]")!.LegacyName);
        Assert.IsNull(RegCatalog.FindSection("Grafikon")!.Find("MinStay [m]")!.LegacyName);
    }

    [TestMethod]
    public void Tables_HodnotyTabulSuPerUserBlackOutSpolocny()
    {
        var tables = RegCatalog.FindSection("Tables")!;

        Assert.AreEqual(RegHive.Machine, tables.Hive);
        Assert.IsNull(tables.Find("BlackOut")!.HiveOverride);
        Assert.AreEqual(RegHive.User, tables.Settings.Single(s => s.Name == "Enabled<N>").HiveOverride);
    }

    [TestMethod]
    [DataRow("3.39", 3, 39)]
    [DataRow("3.34.6", 3, 34)]
    [DataRow("3.00", 3, 0)]
    [DataRow("3.10", 3, 10)]
    public void Verzia_ZTextuSuboru(string text, int major, int minor)
    {
        Assert.AreEqual(new RegVersion(major, minor), RegVersion.Parse(text));
    }

    [TestMethod]
    public void Verzia_DostupnostPodlaSinceAUntil()
    {
        var recvPort = RegCatalog.FindSection("Client")!.Find("RecvPort")!;
        var nazStanice = RegCatalog.FindSection("Implementation")!.Find("NazStanice")!;

        Assert.IsTrue(new RegVersion(3, 10) > new RegVersion(3, 0));
        Assert.IsTrue(recvPort.IsAvailableIn(new RegVersion(3, 39)));
        Assert.IsFalse(recvPort.IsAvailableIn(new RegVersion(3, 34)));
        Assert.IsTrue(nazStanice.IsAvailableIn(new RegVersion(3, 34)));
        Assert.IsFalse(nazStanice.IsAvailableIn(new RegVersion(3, 39)));
        Assert.IsFalse(nazStanice.IsAvailableIn(null));
    }

    [TestMethod]
    public void Pozostatky_ZPoznamokDokumentacie()
    {
        Assert.IsTrue(RegCatalog.IsLeftover("ColumnWidth", "Width2"));
        Assert.IsTrue(RegCatalog.IsLeftover("PathNames", "ErrorLog"));
        Assert.IsTrue(RegCatalog.IsLeftover("Environment", "BusTxt"));
        Assert.IsFalse(RegCatalog.IsLeftover("Environment", "Logging"));
    }
}
