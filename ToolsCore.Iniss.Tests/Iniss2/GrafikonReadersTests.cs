using ToolsCore.Iniss.Calendar;
using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2;
using ToolsCore.Iniss2.Grafikon;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Citanie grafikonu INISS2 z vlastnych fiktivnych dat: zoznam grafikonov, vlaky, kolaje a pohlad na stanicu.
/// </summary>
[TestClass]
public class GrafikonReadersTests
{
    private const int Lipovec = 5400010;
    private const int Horany = 5400020;
    private const int MimoInstalacie = 5400030;

    private static readonly DirListGroup Group = new("2030", [
        new DirListItem("Lipovec", Lipovec, "", "", DirListItemFlags.None),
        new DirListItem("Horany", Horany, "", "", DirListItemFlags.None)
    ]);

    private string _dataDir = null!;

    [TestInitialize]
    public void Init()
    {
        _dataDir = Path.Combine(Path.GetTempPath(), "GrafikonReadersTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dataDir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dataDir, true);

    private void Write(string relativeFile, string text)
    {
        var file = Path.Combine(_dataDir, relativeFile);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, text, Encodings.Win1250);
    }

    private GvFile ReadGv(string trains)
    {
        Write(@"2030\GV.XML", $"""
            <?xml version="1.0" encoding="windows-1250"?>
            <GV f="1" l="C">
              <s>{Lipovec}</s>
              {trains}
            </GV>
            """);
        return GvFile.Read(_dataDir, Group);
    }

    private StationTimetable ReadTimetable(string trains, string positions)
    {
        ReadGv(trains);
        Write(@"2030\Lipovec\PositionsDef.xml", """<POZICE><pos k="1" n="1" /><pos k="2" n="2" /></POZICE>""");
        Write(@"2030\Lipovec\Positions.xml", $"<POZICESPOJU>{positions}</POZICESPOJU>");

        var item = new DirListItem("Lipovec", Lipovec, "", "", DirListItemFlags.None);
        return StationTimetable.Read(_dataDir, new DirListGroup("2030", [item]), item);
    }

    [TestMethod]
    public void DirList_CitaGrafikonyStaniceAPriznaky()
    {
        Write("DirList.xml", """
            <DL>
              <g n="2030">
                <i s="5400010" d="Lipovec" tl="TABULE" al="ZVUK" />
                <i s="5400020" d="Horany" f="ZASTAVKA" />
                <i s="5400030" d="Rozvetvie" f="ODBOCKA" />
                <i s="5400040" d="Medzi" f="MEZILEHLA" />
              </g>
              <g n="2031" />
            </DL>
            """);
        Write(@"2030\Lipovec\SR70.TXT", "5400010\r\n");
        Write(@"2030\Horany\SR70.TXT", "5400020\r\n");
        Write(@"2030\Rozvetvie\SR70.TXT", "5400030\r\n");
        Write(@"2030\Medzi\SR70.TXT", "5400040\r\n");

        var groups = DirListXml.Read(_dataDir);

        Assert.HasCount(2, groups);
        Assert.AreEqual("2030", groups[0].Name);
        Assert.IsEmpty(groups[1].Items);
        Assert.AreEqual(new DirListItem("Lipovec", Lipovec, "TABULE", "ZVUK", DirListItemFlags.None), groups[0].Items[0]);
        Assert.AreEqual(DirListItemFlags.Stop, groups[0].Items[1].Flags);
        Assert.AreEqual(DirListItemFlags.Junction, groups[0].Items[2].Flags);
        Assert.AreEqual(DirListItemFlags.Stop | DirListItemFlags.Junction, groups[0].Items[3].Flags);
    }

    [TestMethod]
    public void DirList_CisloStaniceBerieZoSuboruNieZAtributu()
    {
        Write("DirList.xml", """<DL><g n="2030"><i s="111" d="Lipovec" /></g></DL>""");
        Write(@"2030\Lipovec\SR70.TXT", "; Záznam stanice: Lipovec\r\n; vytvoreno: 1.1.2030\r\n5400010\r\n");

        Assert.AreEqual(Lipovec, DirListXml.Read(_dataDir)[0].Items[0].Station);
    }

    [TestMethod]
    public void DirList_BezPlatnehoCislaVSubore_ChybaAtributNepomoze()
    {
        Write("DirList.xml", """<DL><g n="2030"><i s="5400010" d="Lipovec" /></g></DL>""");
        Write(@"2030\Lipovec\SR70.TXT", "; bez cisla\r\n5400010 Lipovec\r\n");

        Assert.Throws<FormatException>(() => DirListXml.Read(_dataDir));
    }

    [TestMethod]
    public void DirList_BezSuboruCislaStanice_Chyba()
    {
        Write("DirList.xml", """<DL><g n="2030"><i s="5400010" d="Lipovec" /></g></DL>""");

        Assert.Throws<IOException>(() => DirListXml.Read(_dataDir));
    }

    [TestMethod]
    public void DirList_NeznamyPriznak_Chyba()
    {
        Write("DirList.xml", """<DL><g n="2030"><i d="Lipovec" f="NEZNAMY" /></g></DL>""");
        Write(@"2030\Lipovec\SR70.TXT", "5400010\r\n");

        Assert.Throws<FormatException>(() => DirListXml.Read(_dataDir));
    }

    [TestMethod]
    public void Gv_CitaVlakVariantuABody()
    {
        var gv = ReadGv($"""
            <v o="2030-01-07" d="2030-01-20">
              <v jd="jede v 1-5" d="Fiktivni drahy" f="O NP" p="poznamka">
                <z c="{Horany}" o="480" dcv="1001" dv="Os" nv="Javor" />
                <z c="{MimoInstalacie}" f="P DB" />
                <z c="{Lipovec}" p="1450" cp="1" o="5" co="1" f="Z PR X" cv="1002" />
              </v>
            </v>
            """);

        Assert.AreEqual(Lipovec, gv.Stations.Single());
        var train = gv.Trains.Single();
        Assert.AreEqual(new DateOnly(2030, 1, 7), train.From);
        Assert.AreEqual(new DateOnly(2030, 1, 20), train.To);

        var variant = train.Variants.Single();
        Assert.AreEqual("jede v 1-5", variant.Validity);
        Assert.AreEqual("Fiktivni drahy", variant.Operator);
        Assert.AreEqual(GvVariantFlags.O | GvVariantFlags.NP, variant.Flags);
        Assert.AreEqual("poznamka", variant.Note);

        var first = variant.Stops[0];
        Assert.IsNull(first.Arrival);
        Assert.AreEqual(480, first.Departure);
        Assert.AreEqual("Os", first.TrainType);
        Assert.AreEqual("Javor", first.TrainName);
        Assert.IsFalse(first.IsInterest);

        Assert.IsFalse(variant.Stops[1].HasTime);
        Assert.AreEqual(GvStopFlags.Passing | GvStopFlags.OperatingPoint, variant.Stops[1].Flags);

        var last = variant.Stops[2];
        Assert.AreEqual(1450, last.Arrival);
        Assert.AreEqual(1, last.ArrivalDay);
        Assert.AreEqual(GvStopFlags.Interest | GvStopFlags.Pr | GvStopFlags.OnRequest, last.Flags);
        Assert.AreEqual("1002", last.Number, "bez dcv plati cislo z cv");
        Assert.IsTrue(last.IsInterest);
    }

    [TestMethod]
    [DataRow("""<v o="2030-01-07" d="2030-01-20"><v f="NEZNAMY" /></v>""")]
    [DataRow("""<v o="2030-01-07" d="2030-01-20"><v><z c="5400010" f="NEZNAMY" /></v></v>""")]
    public void Gv_NeznamyPriznak_SuborSaNenacita(string trains)
    {
        Assert.Throws<FormatException>(() => ReadGv(trains));
    }

    [TestMethod]
    public void Gv_BezDatumovPlatiOdZaciatkuDoKoncaKalendara()
    {
        var gv = ReadGv($"""<v><v><z c="{Lipovec}" o="480" /></v></v>""");

        var train = gv.Trains.Single();
        Assert.AreEqual(DateOnly.MinValue, train.From);
        Assert.AreEqual(DateOnly.MaxValue, train.To);
        Assert.IsTrue(train.DaysOf(train.Variants.Single())[0]);
    }

    [TestMethod]
    public void Gv_StanicaMimoGrafikonuNieJeZaujmova()
    {
        var gv = ReadGv($"""
            <s>{MimoInstalacie}</s>
            <v o="2030-01-07" d="2030-01-20">
              <v><z c="{MimoInstalacie}" o="480" dcv="1001" f="Z" /><z c="{Lipovec}" p="500" f="Z" /></v>
            </v>
            """);

        Assert.AreEqual(Lipovec, gv.Stations.Single());
        var stops = gv.Trains.Single().Variants.Single().Stops;
        Assert.IsFalse(stops[0].IsInterest);
        Assert.IsTrue(stops[1].IsInterest);
    }

    [TestMethod]
    public void Gv_PrejazdZaujmovouStanicouNieJeZastavenie()
    {
        var stop = new GvStop { Station = Lipovec, Flags = GvStopFlags.Passing | GvStopFlags.Interest };

        Assert.IsFalse(stop.IsInterest);
    }

    [TestMethod]
    public void Gv_ZmenaCislaPlatiPriOdchodeZoStanice()
    {
        var gv = ReadGv($"""
            <v o="2030-01-07" d="2030-01-20">
              <v>
                <z c="{Horany}" o="480" dcv="1001" dv="Os" />
                <z c="{MimoInstalacie}" p="490" o="491" />
                <z c="{Lipovec}" p="500" o="510" f="Z" dcv="2002" dv="Sp" />
                <z c="{MimoInstalacie}" p="520" />
              </v>
            </v>
            """);
        var variant = gv.Trains[0].Variants[0];

        Assert.AreEqual("1001", variant.NumberSource(2, true)?.Number);
        Assert.AreEqual("2002", variant.NumberSource(2, false)?.Number);
        Assert.AreEqual("2002", variant.NumberSource(3, true)?.Number);
        Assert.AreEqual("1001", variant.NumberSource(0, true)?.Number, "vychodzia stanica nema predchadzajuci usek");
    }

    [TestMethod]
    public void Gv_TrasaBezCislaNemaZdroj()
    {
        var variant = new GvVariant { Stops = [new GvStop { Station = Lipovec }] };

        Assert.IsNull(variant.NumberSource(0, false));
    }

    [TestMethod]
    public void Gv_PrvaVariantaMaPrednostPredDalsimi()
    {
        var gv = ReadGv($"""
            <v o="2030-01-07" d="2030-01-20">
              <v jd="jede v 1-5"><z c="{Lipovec}" o="480" dcv="1001" /></v>
              <v jd="jede v 5,6"><z c="{Lipovec}" o="490" dcv="1001" /></v>
              <v><z c="{Lipovec}" o="500" dcv="1001" /></v>
            </v>
            """);
        var train = gv.Trains[0];

        int Count(GvVariant variant) => train.ActiveDaysOf(variant).Cast<bool>().Count(day => day);

        Assert.AreEqual(10, Count(train.Variants[0]));
        Assert.AreEqual(2, Count(train.Variants[1]), "piatky patria prvej variante");
        Assert.AreEqual(2, Count(train.Variants[2]));
    }

    [TestMethod]
    [DataRow("", 14)]
    [DataRow("-", 0)]
    [DataRow("--", 0)]
    [DataRow("11000000000001", 3)]
    [DataRow("jede v 1-5", 10)]
    [DataRow("jede 8.I.", 1)]
    public void Validity_PoznamkyAkoServer(string text, int expected)
    {
        var days = GvValidity.Days(new DateOnly(2030, 1, 7), new DateOnly(2030, 1, 20), text);

        Assert.AreEqual(14, days.Length);
        Assert.AreEqual(expected, days.Cast<bool>().Count(day => day));
    }

    [TestMethod]
    public void Validity_SviatkySuVzdyCeske()
    {
        // utorok 6.1.2026 je sviatok len na Slovensku, streda 28.10.2026 len v Cesku
        var january = GvValidity.Days(new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 7), "jede v +");
        var october = GvValidity.Days(new DateOnly(2026, 10, 27), new DateOnly(2026, 10, 29), "jede v +");

        Assert.AreEqual(DateLimit.Locale.Sk, DateLimit.Loc);
        Assert.IsFalse(january[1]);
        Assert.IsTrue(october[1]);
    }

    [TestMethod]
    public void Validity_ChybnaPoznamkaNejedeZiadnyDen()
    {
        var days = GvValidity.Days(new DateOnly(2030, 1, 7), new DateOnly(2030, 1, 13), "toto nie je poznamka");

        Assert.HasCount(7, days);
        Assert.IsFalse(days.HasAnySet());
    }

    [TestMethod]
    [DataRow(400, true)]
    [DataRow(401, false)]
    public void Validity_TextovaPoznamkaNajviac400Dni(int count, bool valid)
    {
        var from = new DateOnly(2030, 1, 7);
        var to = from.AddDays(count - 1);

        Assert.AreEqual(valid, GvValidity.Days(from, to, "jede v 1-5").HasAnySet());
        Assert.AreEqual(valid, GvValidity.Days(from, to, new string('1', count - 1) + "0").HasAnySet());
        Assert.IsTrue(GvValidity.Days(from, to, "").HasAllSet(), "prazdna poznamka plati vzdy");
    }

    [TestMethod]
    public void Validity_KoniecPredZaciatkomNemaZiadnyDen()
    {
        var from = new DateOnly(2030, 1, 7);

        Assert.IsEmpty(GvValidity.Days(from, from.AddDays(-2), "").Cast<bool>());
        Assert.IsEmpty(GvValidity.Days(from, from.AddDays(-2), "jede v 1-5").Cast<bool>());
    }

    private static readonly string MosTrainXml = $"""
        <v o="2030-01-07" d="2030-01-13">
          <v>
            <z c="{Horany}" o="480" dcv="1001" dv="Os" />
            <z c="{Lipovec}" p="500" o="502" f="Z" />
          </v>
        </v>
        """;

    private GvTrain ReadMos(string records, string? trains = null)
    {
        Write(@"2030\Lipovec\MOS.xml", $"<MOS>{records}</MOS>");
        return ReadGv(trains ?? MosTrainXml).Trains.Single();
    }

    private static string Bits(System.Collections.BitArray days) => string.Concat(days.Cast<bool>().Select(day => day ? '1' : '0'));

    [TestMethod]
    public void Mos_CitaZaznamyAChybajuciSuborJePrazdny()
    {
        Assert.IsEmpty(MosFile.Read(Path.Combine(_dataDir, "2030", "Lipovec")));

        Write(@"2030\Lipovec\MOS.xml", """<MOS><v cv="2002" /><v jd="jede v 1-5" cv="1003" /></MOS>""");

        CollectionAssert.AreEqual(new[] { new MosTrain("2002", ""), new MosTrain("1003", "jede v 1-5") }, MosFile.Read(Path.Combine(_dataDir, "2030", "Lipovec")));
    }

    [TestMethod]
    public void Mos_DruhZGrafikonuSaNeuplatni()
    {
        var gv = ReadGv($"""<v o="2030-01-07" d="2030-01-13"><v><z c="{Lipovec}" o="480" dcv="1001" dv="MOs" /></v></v>""");

        Assert.AreEqual("Os", gv.Trains.Single().Variants.Single().Stops[0].TrainType);
    }

    [TestMethod]
    public void Mos_ZaznamPlatnyVsetkyDni_ZmeniDruhZaciatkuUseku()
    {
        var variant = ReadMos("""<v cv="1001" />""").Variants.Single();

        Assert.AreEqual("MOs", variant.Stops[0].TrainType, "druh sa meni v bode s cislom vlaku, nie v stanici zo zaznamu");
        Assert.IsNull(variant.Days);
    }

    [TestMethod]
    public void Mos_ZaznamPlatnyCastDni_RozdeliVariantu()
    {
        var train = ReadMos("""<v cv="1001" jd="0011100" />""");

        Assert.HasCount(2, train.Variants);
        Assert.AreEqual("Os", train.Variants[0].Stops[0].TrainType);
        Assert.AreEqual("1100011", Bits(train.DaysOf(train.Variants[0])));
        Assert.AreEqual("MOs", train.Variants[1].Stops[0].TrainType);
        Assert.AreEqual("0011100", Bits(train.DaysOf(train.Variants[1])));
        Assert.AreEqual("0011100", Bits(train.ActiveDaysOf(train.Variants[1])));
    }

    [TestMethod]
    public void Mos_ZaznamBezDniVlaku_NicNemeni()
    {
        var train = ReadMos("""<v cv="1001" jd="-" />""");

        Assert.AreEqual("Os", train.Variants.Single().Stops[0].TrainType);
    }

    [TestMethod]
    public void Mos_PlatiPrvyZaznamSCislom()
    {
        var train = ReadMos("""<v cv="1001" jd="-" /><v cv="1001" />""");

        Assert.AreEqual("Os", train.Variants.Single().Stops[0].TrainType);
    }

    [TestMethod]
    public void Mos_DenZaznamuJeDenVStanici()
    {
        var train = ReadMos("""<v cv="1001" jd="0100000" />""", $"""
            <v o="2030-01-07" d="2030-01-13">
              <v>
                <z c="{Horany}" o="1430" dcv="1001" dv="Os" />
                <z c="{Lipovec}" p="10" cp="1" f="Z" />
              </v>
            </v>
            """);

        Assert.AreEqual("1000000", Bits(train.DaysOf(train.Variants[1])), "nocny vlak je v stanici o den neskor");
    }

    [TestMethod]
    [DataRow("Sp", "f=\"Z\"")]
    [DataRow("Os", "f=\"Z P\"")]
    [DataRow("Os", "")]
    public void Mos_IneDruhyANezaujmoveStaniceSaPreskocia(string type, string flags)
    {
        var train = ReadMos("""<v cv="1001" />""", $"""
            <v o="2030-01-07" d="2030-01-13">
              <v>
                <z c="{Horany}" o="480" dcv="1001" dv="{type}" />
                <z c="{Lipovec}" p="500" o="502" {flags} />
              </v>
            </v>
            """);

        Assert.AreEqual(type, train.Variants.Single().Stops[0].TrainType);
    }

    [TestMethod]
    public void Mos_ZmenaCislaZacinaNovyUsek()
    {
        var train = ReadMos("""<v cv="1002" />""", $"""
            <v o="2030-01-07" d="2030-01-13">
              <v>
                <z c="{Horany}" o="480" dcv="1001" dv="Os" />
                <z c="{MimoInstalacie}" p="490" o="491" dcv="1002" dv="Os" />
                <z c="{Lipovec}" p="500" f="Z" />
              </v>
            </v>
            """);

        var stops = train.Variants.Single().Stops;
        Assert.AreEqual("Os", stops[0].TrainType);
        Assert.AreEqual("MOs", stops[1].TrainType);
    }

    [TestMethod]
    public void Positions_CitaKolajeVlakovVPoradiSuboru()
    {
        Write(@"Lipovec\Positions.xml", """
            <POZICESPOJU>
              <pos jd="v 6,+" cv="1001" p="3" o="4" />
              <pos cv="1001" p="1" />
              <pos cv="1002" o="5" />
            </POZICESPOJU>
            """);

        var positions = PositionsFile.Read(Path.Combine(_dataDir, "Lipovec"));

        Assert.HasCount(3, positions);
        Assert.AreEqual(new TrainPosition("1001", "3", "4", "v 6,+"), positions[0]);
        Assert.AreEqual(new TrainPosition("1001", "1", "1", ""), positions[1], "bez odchodu plati kolaj prichodu");
        Assert.AreEqual(new TrainPosition("1002", "", "5", ""), positions[2]);
    }

    [TestMethod]
    public void Positions_ChybajuciSuborJePrazdnyZoznam()
    {
        Assert.IsEmpty(PositionsFile.Read(_dataDir));
    }

    [TestMethod]
    public void PositionsDef_CitaKolajeSTextamiAPriznakmi()
    {
        Write(@"Lipovec\PositionsDef.xml", """
            <?xml version="1.0" encoding="windows-1250"?>
            <POZICE>
              <pos k="1a" n="Kolej 1" tabkolej="1" tabnast="I" zvukkolej="0100" zvuknast="0200" f="NOAUDIO ALTERTRAN" />
              <pos k="-" n="----" f="SEPARATOR" />
              <pos k="x" n="Vynechaná" f="OMIT" />
              <pos k="z" n="Zrušený vlak" f="COMMON" />
            </POZICE>
            """);

        var tracks = PositionsDefFile.Read(Path.Combine(_dataDir, "Lipovec"));

        Assert.AreEqual(new TrackDefinition
        {
            Key = "1a",
            Name = "Kolej 1",
            TrackText = "1",
            PlatformText = "I",
            TrackSound = "0100",
            PlatformSound = "0200",
            Flags = TrackFlags.NoAudio | TrackFlags.AlterTran
        }, tracks[0]);
        Assert.IsTrue(tracks[0].IsRealTrack);
        Assert.IsFalse(tracks[1].IsRealTrack);
        Assert.AreEqual(TrackFlags.Separator, tracks[2].Flags);
        Assert.AreEqual("Vynechaná", tracks[2].Name);
        Assert.AreEqual("Zrušený vlak", tracks[3].Name);
        Assert.IsFalse(tracks[3].IsRealTrack);
    }

    private static string Train(string validity = "") => $"""
        <v o="2030-01-07" d="2030-01-20">
          <v jd="{validity}">
            <z c="{Horany}" o="480" dcv="1001" dv="Os" />
            <z c="{Lipovec}" p="500" o="510" f="Z" dcv="2002" dv="Sp" />
            <z c="{MimoInstalacie}" p="520" />
          </v>
        </v>
        """;

    [TestMethod]
    public void Timetable_ObsahujeLenZastaveniaVoZvolenejStanici()
    {
        var timetable = ReadTimetable(Train() + $"""
            <v o="2030-01-07" d="2030-01-20">
              <v>
                <z c="{Horany}" o="600" dcv="1003" />
                <z c="{Lipovec}" p="610" o="611" f="P Z" />
                <z c="{MimoInstalacie}" p="620" />
              </v>
            </v>
            """, "");

        var call = timetable.Calls.Single();
        Assert.AreEqual(1, call.Index);
        Assert.AreEqual("1001", call.ArrivalNumber);
        Assert.AreEqual("2002", call.DepartureNumber);
        Assert.AreEqual("Sp", call.TrainType);
        Assert.IsFalse(call.IsFirst);
        Assert.IsFalse(call.IsLast);
        Assert.HasCount(2, timetable.Tracks);
    }

    [TestMethod]
    public void Timetable_VCielovejStaniciPlatiCisloPriPrichode()
    {
        var timetable = ReadTimetable($"""
            <v o="2030-01-07" d="2030-01-20">
              <v>
                <z c="{Horany}" o="480" dcv="1001" dv="Os" nv="Javor" />
                <z c="{Lipovec}" p="500" f="Z" dcv="2002" dv="Sp" nv="Buk" />
              </v>
            </v>
            """, "");
        var call = timetable.Calls.Single();

        Assert.IsTrue(call.IsLast);
        Assert.AreEqual("1001", call.Number);
        Assert.AreEqual("Os", call.TrainType);
        Assert.AreEqual("Javor", call.TrainName);
        Assert.AreEqual("2002", call.DepartureNumber, "kolaj server hlada podla cisla pri odchode");
    }

    [TestMethod]
    public void Timetable_KolajHladaPodlaCislaPriOdchode()
    {
        var timetable = ReadTimetable(Train(), """<pos cv="1001" p="1" /><pos cv="2002" p="2" />""");

        var position = timetable.PositionFor(timetable.Calls[0], new DateOnly(2030, 1, 8));

        Assert.AreEqual("2", position?.ArrivalTrack);
    }

    [TestMethod]
    public void Timetable_PlatiPrvyZaznamSPlatnouPoznamkou()
    {
        var timetable = ReadTimetable(Train(), """<pos jd="v 6" cv="2002" p="2" /><pos cv="2002" p="1" />""");
        var call = timetable.Calls[0];

        Assert.AreEqual("2", timetable.PositionFor(call, new DateOnly(2030, 1, 12))?.ArrivalTrack, "sobota");
        Assert.AreEqual("1", timetable.PositionFor(call, new DateOnly(2030, 1, 14))?.ArrivalTrack, "pondelok");
    }

    [TestMethod]
    public void Timetable_MimoObdobiaVlakuKolajNema()
    {
        var timetable = ReadTimetable(Train(), """<pos cv="2002" p="1" />""");

        Assert.IsNull(timetable.PositionFor(timetable.Calls[0], new DateOnly(2030, 1, 21)));
    }

    [TestMethod]
    public void Timetable_VlakBezZaznamuKolajNema()
    {
        var timetable = ReadTimetable(Train(), """<pos cv="1001" p="1" />""");

        Assert.IsNull(timetable.PositionFor(timetable.Calls[0], new DateOnly(2030, 1, 8)));
    }

    [TestMethod]
    public void Timetable_NocnyVlakMaDenVStaniciPodlaPrichodu()
    {
        // v pondelok vychadza, do stanice pride v utorok; kolaj sa vybera podla dna v stanici
        var timetable = ReadTimetable($"""
            <v o="2030-01-07" d="2030-01-20">
              <v>
                <z c="{Horany}" o="1430" dcv="3003" />
                <z c="{Lipovec}" p="10" cp="1" o="15" co="1" f="Z" />
              </v>
            </v>
            """, """<pos jd="v 2" cv="3003" p="2" /><pos cv="3003" p="1" />""");
        var call = timetable.Calls[0];

        Assert.AreEqual(new DateOnly(2030, 1, 8), call.DateAtStation(new DateOnly(2030, 1, 7)));
        Assert.IsTrue(call.IsLast);
        Assert.AreEqual("2", timetable.PositionFor(call, new DateOnly(2030, 1, 7))?.ArrivalTrack);
        Assert.AreEqual("1", timetable.PositionFor(call, new DateOnly(2030, 1, 8))?.ArrivalTrack);
    }

    [TestMethod]
    public void Timetable_VychodziaStanicaMaDenOdchodu()
    {
        var timetable = ReadTimetable($"""
            <v o="2030-01-07" d="2030-01-20">
              <v>
                <z c="{Lipovec}" o="15" co="1" f="Z" dcv="4004" />
                <z c="{Horany}" p="30" cp="1" />
              </v>
            </v>
            """, "");
        var call = timetable.Calls[0];

        Assert.IsTrue(call.IsFirst);
        Assert.AreEqual("4004", call.ArrivalNumber);
        Assert.AreEqual(new DateOnly(2030, 1, 8), call.DateAtStation(new DateOnly(2030, 1, 7)));
    }

    [TestMethod]
    public void Config_CitaPortGtn()
    {
        Write("Config.xml", """<INISSCFG GTNPort="1090" />""");

        Assert.AreEqual(1090, Iniss2Config.Read(_dataDir).GtnPort);
    }

    [TestMethod]
    public void Config_BezSuboruAleboAtributuJeGtnVypnute()
    {
        Assert.AreEqual(0, Iniss2Config.Read(_dataDir).GtnPort);

        Write("Config.xml", "<INISSCFG />");

        Assert.AreEqual(0, Iniss2Config.Read(_dataDir).GtnPort);
    }
}
