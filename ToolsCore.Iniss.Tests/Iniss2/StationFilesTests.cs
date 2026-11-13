using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Grafikon;
using ToolsCore.Iniss2.Tables;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Atributy suborov INISS2, ktore server len prenasa alebo ich ma v pamati: surovy GV.XML, priznak <c>de</c>, povod radenia,
/// priznak sady textov a obdobie vyluk bez konca.
/// </summary>
[TestClass]
public class StationFilesTests
{
    private const int Lipovec = 5400010;
    private const int Horany = 5400020;

    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "StationFilesTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private void Write(string relativeFile, string text)
    {
        var file = Path.Combine(_dir, relativeFile);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, text, Encodings.Win1250);
    }

    private const string Gv = """
        <?xml version="1.0" encoding="windows-1250"?>
        <GV f="487537" l="C">
          <s>5400010</s>
          <s>5400030</s>
          <sl>5400010</sl>
          <v o="2030-12-15" d="2031-12-13">
            <v jd="jede v X">
              <z c="5400005" o="480" cv="1001" dv="MOs" l="NAD1" />
              <z c="5400010" p="495" o="497" f="Z" dcv="2002" cv="2002A" dv="Os" />
              <z c="5400030" p="510" f="Z" />
            </v>
          </v>
        </GV>
        """;

    [TestMethod]
    public void Gv_LoadCitaSuborAkoJe()
    {
        Write(@"2030\GV.XML", Gv);

        var gv = GvFile.Load(Path.Combine(_dir, "2030"));
        var stops = gv.Trains[0].Variants[0].Stops;

        Assert.AreEqual((487537, "C"), (gv.Flags, gv.Language));
        CollectionAssert.AreEqual(new[] { Lipovec, 5400030 }, gv.Stations.ToArray());
        CollectionAssert.AreEqual(new[] { Lipovec }, gv.LimitedStations.ToArray());
        Assert.AreEqual(("", "1001", "NAD1", "MOs"), (stops[0].Number, stops[0].DisplayNumber, stops[0].ReplacementLine, stops[0].TrainType));
        Assert.AreEqual(("2002", "2002A"), (stops[1].Number, stops[1].DisplayNumber));
        Assert.IsTrue(stops[2].Flags.HasFlag(GvStopFlags.Interest));
    }

    [TestMethod]
    public void Gv_PohladServeraMeniLenPamat()
    {
        Write(@"2030\GV.XML", Gv);
        var groupDir = Path.Combine(_dir, "2030");
        var gv = GvFile.Load(groupDir);

        var server = gv.ForServer(new HashSet<int> { Lipovec }, new Dictionary<int, List<MosTrain>>());
        var stops = server.Trains[0].Variants[0].Stops;

        CollectionAssert.AreEqual(new[] { Lipovec }, server.Stations.ToArray());
        Assert.AreEqual(("1001", "Os"), (stops[0].Number, stops[0].TrainType));
        Assert.IsFalse(stops[2].Flags.HasFlag(GvStopFlags.Interest));
        Assert.IsFalse(gv.Write(groupDir));
    }

    [TestMethod]
    public void Gv_ZapisAPrecitanieZachovaVsetkyAtributy()
    {
        Write(@"2030\GV.XML", Gv);
        var groupDir = Path.Combine(_dir, "2030");
        var gv = GvFile.Load(groupDir);
        File.Delete(Path.Combine(groupDir, GvFile.FileName));

        gv.Write(groupDir);

        var read = GvFile.Load(groupDir);
        Assert.AreEqual((487537, "C"), (read.Flags, read.Language));
        CollectionAssert.AreEqual(gv.LimitedStations.ToArray(), read.LimitedStations.ToArray());
        CollectionAssert.AreEqual(gv.Trains[0].Variants[0].Stops.ToArray(), read.Trains[0].Variants[0].Stops.ToArray());
    }

    [TestMethod]
    public void UpravaPreDen_ZapisAPrecitanie()
    {
        PositionsFile.Write(_dir, [new TrainPosition("1001", "1", "1", "jede 3.V.") { EditedForDay = true }]);
        DodatkyFile.Write(_dir, [new TrainAddition("1001", "D1006", "", "") { EditedForDay = true }]);
        ForeignFile.Write(_dir, [new TrainLanguages("1001", "", ["GB"]) { EditedForDay = true }]);
        TrainReportStationsFile.Write(_dir, [new TrainReportStations("1001", "") { EditedForDay = true, LongTo = [Horany] }]);
        TrainTextsFile.Write(_dir, [new TrainTextSet("Ciel") { Trains = [new TrainText("1001", "", "Horany", -1) { EditedForDay = true }] }]);

        Assert.IsTrue(PositionsFile.Read(_dir).Single().EditedForDay);
        Assert.IsTrue(DodatkyFile.Read(_dir).Single().EditedForDay);
        Assert.IsTrue(ForeignFile.Read(_dir).Single().EditedForDay);
        Assert.IsTrue(TrainReportStationsFile.Read(_dir).Single().EditedForDay);
        Assert.IsTrue(TrainTextsFile.Read(_dir).Single().Trains.Single().EditedForDay);
        StringAssert.Contains(File.ReadAllText(Path.Combine(_dir, DodatkyFile.FileName), Encodings.Win1250), """<v cv="1001" de="true" k="D1006" rep="" />""");
    }

    [TestMethod]
    public void StaHlasVl_PoradieAtributovAkoServer()
    {
        TrainReportStationsFile.Write(_dir, [new TrainReportStations("1001", "jede v 6")]);

        StringAssert.Contains(File.ReadAllText(Path.Combine(_dir, TrainReportStationsFile.FileName), Encodings.Win1250), """<v jd="jede v 6" cv="1001" />""");
    }

    [TestMethod]
    public void Razeni_PovodTextuSaZachova()
    {
        Write(RazeniFile.FileName, """
            <RAZENI>
              <razvl jd="jede v 6" de="true" v="1001" est="5400020" rep="Pp">
                <editvl prepis="prestup ST(Horany)">
                  <text l="CZ" t="ve stanici Horany přestup" />
                </editvl>
                <snd>CZ/Slova/vestan</snd>
              </razvl>
            </RAZENI>
            """);

        var composition = RazeniFile.Read(_dir).Single();
        File.Delete(Path.Combine(_dir, RazeniFile.FileName));
        RazeniFile.Write(_dir, [composition]);
        var read = RazeniFile.Read(_dir).Single();

        Assert.IsTrue(read.EditedForDay);
        Assert.AreEqual("prestup ST(Horany)", read.Origin!.Source);
        CollectionAssert.AreEqual(new[] { ("CZ", "ve stanici Horany přestup") }, read.Origin.Texts.ToArray());
        CollectionAssert.AreEqual(new[] { "CZ/Slova/vestan" }, read.Sounds.ToArray());
    }

    [TestMethod]
    public void TTVlaky_PriznakSady()
    {
        Write(TrainTextsFile.FileName, """<TTEXTS><b n="Fonet" f="FONET"><t v="1001"><t /></t></b><b n="Bez" f="NONE"><t v="1002"><t /></t></b></TTEXTS>""");

        var sets = TrainTextsFile.Read(_dir);
        File.Delete(Path.Combine(_dir, TrainTextsFile.FileName));
        TrainTextsFile.Write(_dir, sets);

        CollectionAssert.AreEqual(new[] { true, false }, TrainTextsFile.Read(_dir).Select(set => set.Fonet).ToArray());
        Assert.DoesNotContain("NONE", File.ReadAllText(Path.Combine(_dir, TrainTextsFile.FileName), Encodings.Win1250));
    }

    [TestMethod]
    public void Vyluky_BezKoncaSaZapiseBezDo()
    {
        Write(VylukyFile.FileName, """<VYLUKY><v cv="1001" k="7" od="2030-12-15" /></VYLUKY>""");

        var lockout = VylukyFile.Read(_dir).Single();
        File.Delete(Path.Combine(_dir, VylukyFile.FileName));
        VylukyFile.Write(_dir, [lockout]);

        Assert.AreEqual(DateOnly.MinValue, lockout.To);
        Assert.DoesNotContain("do=", File.ReadAllText(Path.Combine(_dir, VylukyFile.FileName), Encodings.Win1250));
    }
}
