using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss2.Devices;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Svetla z Lights.xml a audio linky z Audio.xml servera INISS2 z vlastnych fiktivnych dat.
/// </summary>
[TestClass]
public class DevicesReadersTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "DevicesReadersTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, true);

    private void Write(string name, string text) => File.WriteAllText(Path.Combine(_dir, name), text, Encodings.Win1250);

    [TestMethod]
    public void Lights_CitaAtributyAPredvoleneHodnoty()
    {
        Write(LightsXml.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <Lights>
              <l st="5412340" jm="Nástupište 1" typ="AKSIGNAL" lin="Lipovec_Svetlo" addr="3" ppp="5" ppo="6" pso="2" psp="3" zh="10" />
              <!-- svetlo bez linky a adresy -->
              <l st="5412340" jm="Podchod" typ="AKSIGNALUH" />
              <l jm="Bez stanice" lin_podr="Svetlo2" />
            </Lights>
            """);

        var lights = LightsXml.Read(_dir);

        Assert.HasCount(3, lights);
        Assert.AreEqual(new LightDefinition(5412340, "Nástupište 1", LightsXml.AkSignal, "Lipovec_Svetlo")
        {
            Address = 3, BeforeArrival = 5, BeforeDeparture = 6, AfterDeparture = 2, AfterArrival = 3, MinOff = 10
        }, lights[0]);
        Assert.AreEqual(new LightDefinition(5412340, "Podchod", LightsXml.AkSignalUh, ""), lights[1]);
        Assert.AreEqual(new LightDefinition(0, "Bez stanice", LightsXml.AkSignal, "") { SubLine = "Svetlo2" }, lights[2], "predvoleny typ AKSIGNAL");
    }

    [TestMethod]
    public void Lights_ChybajuciSuborJePrazdny() => Assert.IsEmpty(LightsXml.Read(_dir));

    [TestMethod]
    public void Lights_ZlaAdresaJeChybaSuboru()
    {
        // server subor cita serializatorom XML - necislo v ciselnom atribute zastavi start
        Write(LightsXml.FileName, """<Lights><l st="1" jm="A" addr="x" /></Lights>""");

        Assert.ThrowsExactly<FormatException>(() => LightsXml.Read(_dir));
    }

    [TestMethod]
    public void LightLog_CitaRiadkyServera()
    {
        // riadky v tvare, aky zapisuje serializator XML servera - cas s posunom, on/off s posunom aj bez neho
        File.WriteAllText(LightLogFile.PathFor(_dir, new DateOnly(2030, 1, 7)), string.Join(Environment.NewLine,
            "",
            "<L d=\"2030-01-07\" c=\"10:22:26.6407653+01:00\" s=\"Lipovec\" n=\"Nástupište 1\" u=\"ChangeState\" r=\"OFF\" st=\"NOTCONNECTED\" />",
            "<L d=\"2030-01-07\" c=\"10:28:00.0150374+01:00\" s=\"Lipovec\" n=\"Nástupište 1\" u=\"ChangeForTrain\" r=\"AUTO\" t=\"true\" st=\"LIGHTON TWILIGHTON\" on=\"2030-01-07T10:31:00\" off=\"2030-01-07T10:38:00+01:00\" dcv=\"6412\" />",
            "<L d=\"2030-01-07\" c=\"10:29",
            "<L d=\"2030-01-07\" c=\"10:30:00+01:00\" s=\"Lipovec\" n=\"Nástupište 1\" u=\"SetMode\" m=\"true\" r=\"FORCELIGHT\" />"), Encodings.Win1250);

        var entries = LightLogFile.Read(LightLogFile.PathFor(_dir, new DateOnly(2030, 1, 7)));

        Assert.HasCount(3, entries, "poskodeny riadok sa preskoci");
        var plus1 = TimeSpan.FromHours(1);
        Assert.AreEqual(new DateTimeOffset(2030, 1, 7, 10, 22, 26, plus1).AddTicks(6407653), entries[0].At);
        Assert.AreEqual("Nástupište 1", entries[0].Name);
        Assert.AreEqual(LightLogFile.ChangeState, entries[0].Event);
        CollectionAssert.AreEqual(new[] { "NOTCONNECTED" }, entries[0].State.ToArray());

        var train = entries[1];
        Assert.IsTrue(train.TrainNeedsLight);
        Assert.AreEqual("AUTO", train.Mode);
        CollectionAssert.AreEqual(new[] { "LIGHTON", "TWILIGHTON" }, train.State.ToArray());
        Assert.AreEqual(new DateTimeOffset(2030, 1, 7, 10, 38, 0, plus1), train.TurnOff);
        Assert.AreEqual(new DateTime(2030, 1, 7, 10, 31, 0), train.TurnOn!.Value.DateTime, "bez posunu miestny cas");
        Assert.AreEqual("6412", train.Train);

        Assert.IsTrue(entries[2].Manual);
        Assert.AreEqual(LightLogFile.SetMode, entries[2].Event);
        Assert.IsNull(entries[2].TurnOn);
        Assert.IsEmpty(LightLogFile.Read(LightLogFile.PathFor(_dir, new DateOnly(2030, 1, 8))));
    }

    [TestMethod]
    public void Audio_CitaVystupLinkuAKanal()
    {
        Write(AudioXml.FileName, """
            <?xml version="1.0" encoding="windows-1250"?>
            <AUDIO>
              <line st="5400010" jm="Lipovec" zkr="Lipovec" fr="Lipovec" wave="0" t0="06:00:00" t1="20:00:00" />
              <line st="5400020" jm="Horná Lehota" fr="Lehota" out="AKSIGNALUH" port="Lehota_UH" ch="2" />
              <line st="5400030" jm="Dolná Lehota" fr="Dolna" out="AKSIGNAL" port="Dolna_AK" addr="7" wave="1" />
            </AUDIO>
            """);

        var lines = AudioXml.Read(_dir);

        Assert.HasCount(3, lines);
        Assert.AreEqual(new AudioLineDefinition(5400010, "Lipovec", AudioXml.None, "") { Queue = "Lipovec", Wave = "0" }, lines[0]);
        Assert.AreEqual(new AudioLineDefinition(5400020, "Horná Lehota", AudioXml.AkSignalUh, "Lehota_UH") { Queue = "Lehota", Channel = "2" }, lines[1]);
        Assert.AreEqual(new AudioLineDefinition(5400030, "Dolná Lehota", AudioXml.AkSignal, "Dolna_AK") { Queue = "Dolna", Wave = "1", Address = 7 }, lines[2]);
    }
}
