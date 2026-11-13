using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Grafikon;
using ToolsCore.Iniss2.Tables;
using ToolsCore.Iniss2.Xml;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Zapis suborov INISS2 upravou na mieste: zmenia sa len zmenene prvky, komentare, zapis ostatnych prvkov a to, co zapisovac
/// nepozna, ostanu po znakoch.
/// </summary>
[TestClass]
public class Iniss2XmlWriteTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "Iniss2XmlWriteTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    // subor s CRLF ako ho zapisuje server a rucne upravy (komentar, zarovnanie, /> bez medzery)
    private string File(string name, params string[] lines)
    {
        var path = Path.Combine(_dir, name);
        System.IO.File.WriteAllText(path, string.Join("\r\n", lines) + "\r\n", Encodings.Win1250);
        return path;
    }

    private static string Text(string path) => System.IO.File.ReadAllText(path, Encodings.Win1250);

    private static string Lines(params string[] lines) => string.Join("\r\n", lines) + "\r\n";

    private string Positions() => File(PositionsFile.FileName,
        """<?xml version="1.0" encoding="windows-1250" ?>""",
        "<!-- upravene 1.1.2030: kolaj vlaku 1002 -->",
        "<POZICESPOJU>",
        """  <pos cv="1001"  p="1"   o="1"/>""",
        "  <!-- docasne -->",
        """  <pos cv="1002" p="2" o="2" />""",
        """  <pos cv="1003" p="3" o="3" />""",
        "</POZICESPOJU>");

    [TestMethod]
    public void Zapis_BezZmenySuborNezapise()
    {
        var path = Positions();
        var before = System.IO.File.ReadAllBytes(path);

        Assert.IsFalse(PositionsFile.Write(_dir, PositionsFile.Read(_dir)));

        CollectionAssert.AreEqual(before, System.IO.File.ReadAllBytes(path));
    }

    [TestMethod]
    public void Zapis_ZmenaMeniLenSvojPrvok()
    {
        var path = Positions();
        var positions = PositionsFile.Read(_dir);
        positions[2] = positions[2] with { ArrivalTrack = "5", DepartureTrack = "5" };

        Assert.IsTrue(PositionsFile.Write(_dir, positions));

        Assert.AreEqual(Lines(
            """<?xml version="1.0" encoding="windows-1250" ?>""",
            "<!-- upravene 1.1.2030: kolaj vlaku 1002 -->",
            "<POZICESPOJU>",
            """  <pos cv="1001"  p="1"   o="1"/>""",
            "  <!-- docasne -->",
            """  <pos cv="1002" p="2" o="2" />""",
            """  <pos cv="1003" p="5" o="5" />""",
            "</POZICESPOJU>"), Text(path));
    }

    [TestMethod]
    public void Zapis_VymazanieOdstraniRiadokKomentarOstane()
    {
        var path = Positions();
        var positions = PositionsFile.Read(_dir);
        positions.RemoveAt(1);

        PositionsFile.Write(_dir, positions);

        Assert.AreEqual(Lines(
            """<?xml version="1.0" encoding="windows-1250" ?>""",
            "<!-- upravene 1.1.2030: kolaj vlaku 1002 -->",
            "<POZICESPOJU>",
            """  <pos cv="1001"  p="1"   o="1"/>""",
            "  <!-- docasne -->",
            """  <pos cv="1003" p="3" o="3" />""",
            "</POZICESPOJU>"), Text(path));
    }

    [TestMethod]
    public void Zapis_NovyZaznamMaOdsadenieSusedovAPoradieZoznamu()
    {
        var path = Positions();
        var positions = PositionsFile.Read(_dir);
        positions.Insert(0, new TrainPosition("1000", "4", "4", "jede v 6"));
        positions.Add(new TrainPosition("1004", "4", "4", ""));

        PositionsFile.Write(_dir, positions);

        Assert.AreEqual(Lines(
            """<?xml version="1.0" encoding="windows-1250" ?>""",
            "<!-- upravene 1.1.2030: kolaj vlaku 1002 -->",
            "<POZICESPOJU>",
            """  <pos jd="jede v 6" cv="1000" p="4" o="4" />""",
            """  <pos cv="1001"  p="1"   o="1"/>""",
            "  <!-- docasne -->",
            """  <pos cv="1002" p="2" o="2" />""",
            """  <pos cv="1003" p="3" o="3" />""",
            """  <pos cv="1004" p="4" o="4" />""",
            "</POZICESPOJU>"), Text(path));
        CollectionAssert.AreEqual(positions, PositionsFile.Read(_dir));
    }

    [TestMethod]
    public void Zapis_NeznameAtributyAPrvkyOstanu()
    {
        var path = File(DodatkyFile.FileName,
            "<DODATKY>",
            "  <info verzia=\"2\" />",
            """  <v cv="1001" k="D1006" rep="" pozn="z klienta" />""",
            "</DODATKY>");
        var additions = DodatkyFile.Read(_dir);

        DodatkyFile.Write(_dir, [additions[0] with { Reports = "Pp" }]);

        Assert.AreEqual(Lines(
            "<DODATKY>",
            "  <info verzia=\"2\" />",
            """  <v cv="1001" k="D1006" rep="Pp" pozn="z klienta" />""",
            "</DODATKY>"), Text(path));
    }

    [TestMethod]
    public void Zapis_PrazdnyKorenDostaneZaznamy()
    {
        var path = File(DodatkyFile.FileName, """<?xml version="1.0" encoding="windows-1250"?>""", "<DODATKY />");

        DodatkyFile.Write(_dir, [new TrainAddition("1001", "D1006", "", "")]);

        Assert.AreEqual(Lines(
            """<?xml version="1.0" encoding="windows-1250"?>""",
            "<DODATKY>",
            """  <v cv="1001" k="D1006" rep="" />""",
            "</DODATKY>"), Text(path));
    }

    [TestMethod]
    public void Zapis_ZnakMimoWin1250SaZapiseOdkazomNaZnak()
    {
        File(TrainTextsFile.FileName, "<TTEXTS>", """  <b n="Ciel"><t v="1001"><t s="Lipovec" /></t></b>""", "</TTEXTS>");
        var set = TrainTextsFile.Read(_dir)[0];

        TrainTextsFile.Write(_dir, [set with { Trains = [set.Trains[0] with { Text = "Lipovec Ω" }] }]);

        StringAssert.Contains(Text(Path.Combine(_dir, TrainTextsFile.FileName)), "s=\"Lipovec &#x3A9;\"");
        Assert.AreEqual("Lipovec Ω", TrainTextsFile.Read(_dir)[0].Trains[0].Text);
    }

    [TestMethod]
    public void Zapis_ZmenaBoduTrasyNemeniOstatneVariantyANamespaceKorena()
    {
        var path = File(GvFile.FileName,
            """<?xml version="1.0" encoding="windows-1250"?>""",
            """<GV xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" f="1" l="C">""",
            "  <s>5400010</s>",
            """  <v o="2030-12-15" d="2031-12-13">""",
            """    <v jd="jede v X" d="Fiktivní dráhy">""",
            """      <z c="5400005" o="480" dcv="1001" dv="Os"/>""",
            """      <z c="5400010" p="495" f="Z" />""",
            "    </v>",
            "    <!-- sobota -->",
            """    <v jd="jede v 6">""",
            """      <z c="5400005" o="500" dcv="1001" dv="Os" />""",
            """      <z c="5400010" p="515" f="Z" />""",
            "    </v>",
            "  </v>",
            "</GV>");
        var gv = GvFile.Load(_dir);
        var train = gv.Trains[0];
        var saturday = train.Variants[1];
        var changed = saturday with { Stops = [saturday.Stops[0] with { Departure = 505 }, saturday.Stops[1]] };

        (gv with { Trains = [train with { Variants = [train.Variants[0], changed] }] }).Write(_dir);

        var text = Text(path);
        StringAssert.Contains(text, """<GV xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" f="1" l="C">""");
        StringAssert.Contains(text, """      <z c="5400005" o="480" dcv="1001" dv="Os"/>""");
        StringAssert.Contains(text, "    <!-- sobota -->");
        StringAssert.Contains(text, """      <z c="5400005" o="505" dcv="1001" dv="Os" />""");
        Assert.AreEqual(505, GvFile.Load(_dir).Trains[0].Variants[1].Stops[0].Departure);
    }

    [TestMethod]
    public void Zapis_NovySuborAkoServer()
    {
        MosFile.Write(_dir, [new MosTrain("1001", "jede v 1-5")]);

        Assert.AreEqual(Lines(
            """<?xml version="1.0" encoding="windows-1250"?>""",
            "<MOS>",
            """  <v jd="jede v 1-5" cv="1001" />""",
            "</MOS>"), Text(Path.Combine(_dir, MosFile.FileName)).TrimEnd() + "\r\n");
    }

    [TestMethod]
    public void Zapis_PrvkyNeodpovedajuZapisovacu_ZapiseSaCelyAkoServer()
    {
        // sadu bez textov vlakov zapisovac vynecha ako server, prvky suboru sa preto nedaju sparovat
        var path = File(TrainTextsFile.FileName,
            "<!-- komentar -->",
            "<TTEXTS>",
            """  <b n="Prazdna" />""",
            """  <b n="Ciel"><t v="1001"><t s="Lipovec" /></t></b>""",
            "</TTEXTS>");
        var sets = TrainTextsFile.Read(_dir);

        TrainTextsFile.Write(_dir, [sets[1] with { Trains = [sets[1].Trains[0] with { Text = "Horany" }] }]);

        var read = TrainTextsFile.Read(_dir);
        Assert.HasCount(1, read);
        Assert.AreEqual("Horany", read[0].Trains[0].Text);
        Assert.DoesNotContain("komentar", Text(path));
        Assert.IsFalse(System.IO.File.Exists(path + ".tmp"));
    }

    [TestMethod]
    [DataRow("abc", "abc", "=a =b =c")]
    [DataRow("abc", "axc", "=a -b +x =c")]
    [DataRow("abc", "", "-a -b -c")]
    [DataRow("", "ab", "+a +b")]
    [DataRow("abcd", "acbd", "=a -b =c +b =d")]
    public void Rozdiel_NajdlhsiaSpolocnaPodpostupnost(string older, string newer, string expected)
    {
        var a = older.Select(c => c.ToString()).ToList();
        var b = newer.Select(c => c.ToString()).ToList();

        var steps = SequenceDiff.Compute(a, b).Select(step => step.Kind switch
        {
            SequenceDiff.Kind.Equal => "=" + a[step.Old],
            SequenceDiff.Kind.Delete => "-" + a[step.Old],
            _ => "+" + b[step.New]
        });

        Assert.AreEqual(expected, string.Join(' ', steps));
    }
}
