using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Grafikon;
using ToolsCore.Iniss2.Tables;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Subory nastavenia stanice a grafikonu, ktore server len cita (PositionsDef, GV.stanice, ReportKeys, ConstReports,
/// StaHlasB/C, Default.Stations, TTTrasy, TTStanice, DirList): uplny model a zapis na mieste s komentarmi.
/// </summary>
[TestClass]
public class StationSetupFilesTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "StationSetupFilesTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private string Write(string relativeFile, string text)
    {
        var file = Path.Combine(_dir, relativeFile);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, text, Encodings.Win1250);
        return file;
    }

    private string Text(string relativeFile) => File.ReadAllText(Path.Combine(_dir, relativeFile), Encodings.Win1250);

    [TestMethod]
    public void PositionsDef_SektorAZapisNaMieste()
    {
        Write("PositionsDef.xml", """
            <?xml version="1.0" encoding="windows-1250"?>
            <POZICE>
              <!-- kolaje upravil technik -->
              <pos k="1" n="1" tabkolej="1" tabnast="I" zvukkolej="0100" zvuknast="0200" />
              <pos k="2A" n="2" s="A" tabkolej="2" tabsektor="A" tabnast="II" zvukkolej="0101" zvuknast="0201" zvuksektor="0301" />
              <pos k="X" n="Zrušený" f="COMMON NOAUDIO" />
            </POZICE>
            """);

        var tracks = PositionsDefFile.Read(_dir);
        Assert.AreEqual(("A", "A", "0301"), (tracks[1].Sector, tracks[1].SectorText, tracks[1].SectorSound));
        Assert.IsFalse(PositionsDefFile.Write(_dir, tracks));

        PositionsDefFile.Write(_dir, [tracks[0] with { PlatformText = "II" }, tracks[1], tracks[2]]);

        var text = Text("PositionsDef.xml");
        StringAssert.Contains(text, "<!-- kolaje upravil technik -->");
        StringAssert.Contains(text, """<pos k="2A" n="2" s="A" tabkolej="2" tabsektor="A" tabnast="II" zvukkolej="0101" zvuknast="0201" zvuksektor="0301" />""");
        Assert.AreEqual("II", PositionsDefFile.Read(_dir)[0].PlatformText);
        Assert.AreEqual(TrackFlags.Common | TrackFlags.NoAudio, PositionsDefFile.Read(_dir)[2].Flags);
    }

    [TestMethod]
    public void TTTrasy_TrasySoStanicamiAPismom()
    {
        Write(RouteTextsFile.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <TTEXTSTRC>
              <b n="TrasaDoSmeru" f="FONET">
                <r cat="Odjezdová Internet" div="Směr" />
                <t t="Dolné Mesto#Horná Lehota" i="2">
                  <s k="5400010" />
                  <s k="5400020" f="S" />
                </t>
                <!-- bez textu -->
                <t>
                  <s k="5400010" />
                </t>
              </b>
            </TTEXTSTRC>
            """);

        var sets = RouteTextsFile.Read(_dir);
        var set = sets.Single();
        Assert.IsTrue(set.Fonet);
        Assert.AreEqual(new TextTarget("Odjezdová Internet", "Směr"), set.Targets.Single());
        Assert.AreEqual(("Dolné Mesto#Horná Lehota", 2), (set.Routes[0].Text, set.Routes[0].Font));
        CollectionAssert.AreEqual(new[] { new RouteStation(5400010, false), new RouteStation(5400020, true) }, set.Routes[0].Stations.ToList());
        Assert.AreEqual(("", TrainTextSet.NoFont), (set.Routes[1].Text, set.Routes[1].Font));
        Assert.IsFalse(RouteTextsFile.Write(_dir, sets));

        RouteTextsFile.Write(_dir, [set with { Routes = [set.Routes[0], set.Routes[1] with { Text = "Horany" }] }]);

        StringAssert.Contains(Text(RouteTextsFile.FileName), "<!-- bez textu -->");
        Assert.AreEqual("Horany", RouteTextsFile.Read(_dir).Single().Routes[1].Text);
    }

    [TestMethod]
    public void TTStanice_TextyStanic()
    {
        Write(StationTextsFile.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <TTEXTSST>
              <b n="CiloveStanice">
                <r cat="Odjezdová Internet" div="Cíl" />
                <s k="5400020" t="Dolné Mesto" />
                <s k="5400030" t="Horná Lehota" i="2" />
              </b>
            </TTEXTSST>
            """);

        var sets = StationTextsFile.Read(_dir);
        CollectionAssert.AreEqual(new[] { new StationText(5400020, "Dolné Mesto", TrainTextSet.NoFont), new StationText(5400030, "Horná Lehota", 2) },
            sets.Single().Stations.ToList());
        Assert.IsFalse(StationTextsFile.Write(_dir, sets));

        StationTextsFile.Write(_dir, [sets[0] with { Stations = [.. sets[0].Stations, new StationText(5400040, "Brezová", TrainTextSet.NoFont)] }]);

        StringAssert.Contains(Text(StationTextsFile.FileName), """<s k="5400030" t="Horná Lehota" i="2" />""");
        Assert.HasCount(3, StationTextsFile.Read(_dir).Single().Stations);
    }

    [TestMethod]
    public void TTVlaky_OdstranenieTextovVlakovNechaSady()
    {
        Write(TrainTextsFile.FileName, """
            <TTEXTS>
              <b n="Poznamka">
                <r cat="Odjezdová Internet" div="Poznámka" />
                <t v="1001"><t s="Jede přes Javor" /></t>
              </b>
            </TTEXTS>
            """);

        TrainTextsFile.RemoveTrains(_dir);

        var set = TrainTextsFile.Read(_dir).Single();
        Assert.AreEqual(("Poznamka", 1), (set.Name, set.Targets.Count));
        Assert.IsEmpty(set.Trains);
    }

    [TestMethod]
    public void HlaseniaStanice_ZapisNaMiesteSKomentarmi()
    {
        Write(ReportKeysFile.FileName, """
            <ReportKeys>
              <!-- typy hlaseni -->
              <r n="Přijede" k="P" v="V P K" />
              <r n="Odjede" k="O" v="V P" />
            </ReportKeys>
            """);
        Write("ConstReports.xml", """
            <?xml version="1.0" encoding="windows-1250"?>
            <CONSTREPORTS>
              <r k="4" n="Prestup" ik="Přestup_v_zadané_stanici"><s>5400030</s></r>
            </CONSTREPORTS>
            """);
        Write(ReportPatternsFile.ShortFileName, """
            <STAHLAS>
              <vzor><z s="5400020" /><z s="5400030" h="true" /></vzor>
            </STAHLAS>
            """);

        var keys = ReportKeysFile.Read(_dir);
        var reports = ConstReportsFile.Read(_dir);
        var patterns = ReportPatternsFile.Read(_dir, ReportPatternsFile.ShortFileName);
        Assert.IsFalse(ReportKeysFile.Write(_dir, keys));
        Assert.IsFalse(ConstReportsFile.Write(_dir, reports));
        Assert.IsFalse(ReportPatternsFile.Write(_dir, ReportPatternsFile.ShortFileName, patterns));

        ReportKeysFile.Write(_dir, [keys[0], keys[1] with { Categories = "V P K" }]);
        ConstReportsFile.Write(_dir, [reports[0], new ConstReport(5, "Odrieknutie", "Odřeknutí_vlaku_na_úseku", [5400010, 5400020])]);
        ReportPatternsFile.Write(_dir, ReportPatternsFile.ShortFileName, [[.. patterns[0], new ReportPatternStop(5400040, true)]]);

        StringAssert.Contains(Text(ReportKeysFile.FileName), "<!-- typy hlaseni -->");
        Assert.AreEqual("V P K", ReportKeysFile.Read(_dir)[1].Categories);
        StringAssert.Contains(Text("ConstReports.xml"), """<r k="4" n="Prestup" ik="Přestup_v_zadané_stanici"><s>5400030</s></r>""");
        CollectionAssert.AreEqual(new[] { 5400010, 5400020 }, ConstReportsFile.Read(_dir)[1].Stations.ToList());
        Assert.HasCount(3, ReportPatternsFile.Read(_dir, ReportPatternsFile.ShortFileName).Single());
    }

    [TestMethod]
    public void Grafikon_NazvyStanicAPredvoleneStanice()
    {
        Write(GvStationsFile.FileName, """
            <?xml version="1.0" encoding="windows-1250" ?>
            <!--
            vytvořeno:   14.1.2030 11:02
            -->
            <ST>
              <s k="5400010" n="Lipovec" />
            </ST>
            """);
        Write(DefaultStationsFile.FileName, """
            <STANICE>
              <s st="5400010" n="Lipovec" />
              <s st="5400020" n="Dolné Mesto" k="false" />
            </STANICE>
            """);

        GvStationsFile.Write(_dir, [.. GvStationsFile.Read(_dir), new GvStation(5400020, "Dolné Mesto")]);
        var stations = DefaultStationsFile.Read(_dir);
        DefaultStationsFile.Write(_dir, [stations[0] with { Long = false }, stations[1]]);

        StringAssert.Contains(Text(GvStationsFile.FileName), "vytvořeno:   14.1.2030 11:02");
        Assert.HasCount(2, GvStationsFile.Read(_dir));
        Assert.AreEqual((true, false), (DefaultStationsFile.Read(_dir)[0].Short, DefaultStationsFile.Read(_dir)[0].Long));
        StringAssert.Contains(Text(DefaultStationsFile.FileName), """<s st="5400020" n="Dolné Mesto" k="false" />""");
    }

    [TestMethod]
    public void DirList_AkoVSuboreAPridanieStanice()
    {
        Write("DirList.xml", """
            <DL>
              <!-- grafikon 2030 -->
              <g n="2030">
                <i s="5400010" d="01 Lipovec" tl="TABULE" al="ZVUK" />
                <i s="5400020" d="02 Horany" f="MEZILEHLA" />
              </g>
            </DL>
            """);

        var groups = DirListXml.Load(_dir);
        Assert.AreEqual(DirListItemFlags.Stop | DirListItemFlags.Junction, groups[0].Items[1].Flags);
        Assert.IsFalse(DirListXml.Write(_dir, groups));

        DirListXml.AddStation(_dir, "2030", new DirListItem("03 Brezova", 5400040, "", "", DirListItemFlags.Stop));
        DirListXml.AddStation(_dir, "2031", new DirListItem("01 Lipovec", 5400010, "", "", DirListItemFlags.None));

        var text = Text("DirList.xml");
        StringAssert.Contains(text, "<!-- grafikon 2030 -->");
        StringAssert.Contains(text, """<i s="5400020" d="02 Horany" f="MEZILEHLA" />""");
        var after = DirListXml.Load(_dir);
        Assert.AreEqual(("03 Brezova", DirListItemFlags.Stop), (after[0].Items[2].Directory, after[0].Items[2].Flags));
        Assert.AreEqual("2031", after[1].Name);
    }
}
