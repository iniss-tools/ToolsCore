using System.Text;
using System.Xml;
using ToolsCore.Iniss.Entities;
using ToolsCore.Iniss2.RawBank;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Citanie banky INISS2 z FyzBank.xml a zapis spat.
/// </summary>
[TestClass]
public class FyzBankXmlReaderTests
{
    private const string FULL_BANK = """
        <?xml version="1.0" encoding="UTF-8"?>
        <FYZBANK xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
          <lang default="true" k="SK" n="Slovenčina" d="SK" date="2018-11-26T14:34:12.0084456+01:00" location="Dolné Mesto">
            <g k="R1" d="R1\">
              <z k="9900100" t="Dolné Mesto &amp; okolie" f="9900100.EWA" l="1020" md5="9ae55cb7eec4efbea9d7e188b07d3ff1" />
              <z k="9900150" t="Horná Ves" f="..\N5\9900150.WAV" l="900" fid="3" md5="b9dc6543707165f38e3c47e259149500" />
              <z k="9900160" t="Bez nahrávky," f="9900160.EWA" />
            </g>
            <g k="Prazdna" d="Prazdna\" />
          </lang>
        </FYZBANK>
        """;

    private string _bank = null!;

    [TestInitialize]
    public void Init()
    {
        _bank = Path.Combine(Path.GetTempPath(), "FyzBankXmlReaderTests_" + Guid.NewGuid().ToString("N")) + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(_bank);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_bank, true);

    private string Create(string folder, string content)
    {
        var dir = Path.Combine(_bank, folder);
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, FyzBankXmlWriter.FileName);
        File.WriteAllText(file, content, new UTF8Encoding(true));
        return file;
    }

    private static string Lang(string key, bool isDefault = false) =>
        $"""<FYZBANK><lang {(isDefault ? "default=\"true\" " : "")}k="{key}" n="{key}" d="{key}" /></FYZBANK>""";

    private FyzLanguage ReadFullBank()
    {
        Create("SK", FULL_BANK);
        var language = FyzBankXmlReader.ReadLanguages(_bank).Single();
        FyzBankXmlReader.ReadSounds(_bank, language);
        return language;
    }

    [TestMethod]
    public void ReadLanguages_UdajeJazykaBezSkupin()
    {
        Create("SK", FULL_BANK);

        var language = FyzBankXmlReader.ReadLanguages(_bank).Single();

        Assert.AreEqual("SK", language.Key);
        Assert.AreEqual("Slovenčina", language.Name);
        Assert.AreEqual("SK\\", language.RelativePath);
        Assert.AreEqual(FyzBankXmlWriter.FileName, language.FileDefName);
        Assert.IsTrue(language.IsBasic);
        Assert.AreEqual(new DateTimeOffset(2018, 11, 26, 14, 34, 12, TimeSpan.FromHours(1)).AddTicks(84456), language.Date);
        Assert.AreEqual("Dolné Mesto", language.Location);
        Assert.IsNull(language.Groups);
    }

    [TestMethod]
    public void ReadLanguages_PreskociPriecinkyBezSuboruAZoradiPodlaNazvu()
    {
        Create("SK", Lang("SK"));
        Create("CZ", Lang("CZ", true));
        Directory.CreateDirectory(Path.Combine(_bank, "Prazdny"));

        var languages = FyzBankXmlReader.ReadLanguages(_bank);

        CollectionAssert.AreEqual(new[] { "CZ", "SK" }, languages.Select(l => l.Key).ToArray());
    }

    [TestMethod]
    [DataRow("<FYZBANK />")]
    [DataRow("""<FYZBANK><lang k="A" /><lang k="B" /></FYZBANK>""")]
    [DataRow("""<INA><lang k="A" /></INA>""")]
    public void ReadLanguages_SuborBezPravejednehoJazykaPreskoci(string content)
    {
        Create("SK", content);

        Assert.IsEmpty(FyzBankXmlReader.ReadLanguages(_bank));
    }

    [TestMethod]
    public void Read_KodovanieBerieZHlavickySuboru()
    {
        // server kodovanie banky nepredpoklada, riadi sa hlavickou - banka v CP1250 mu vyhovuje rovnako
        var dir = Path.Combine(_bank, "SK");
        Directory.CreateDirectory(dir);
        var content = FULL_BANK.Replace("UTF-8", "windows-1250");
        File.WriteAllBytes(Path.Combine(dir, FyzBankXmlWriter.FileName), CodePagesEncodingProvider.Instance.GetEncoding(1250)!.GetBytes(content));

        var language = FyzBankXmlReader.ReadLanguages(_bank).Single();
        FyzBankXmlReader.ReadSounds(_bank, language);

        Assert.AreEqual("Slovenčina", language.Name);
        Assert.AreEqual("Dolné Mesto", language.Location);
        Assert.AreEqual("Dolné Mesto & okolie", language.Groups[0].Sounds[0].Text);
    }

    [TestMethod]
    public void ReadLanguages_ChybneXmlHlasi()
    {
        Create("SK", "<FYZBANK><lang");

        Assert.ThrowsExactly<XmlException>(() => FyzBankXmlReader.ReadLanguages(_bank));
    }

    [TestMethod]
    public void ReadLanguages_CestaJazykaJePriecinokSuboru()
    {
        Create("SK2", Lang("SK"));

        Assert.AreEqual("SK2\\", FyzBankXmlReader.ReadLanguages(_bank).Single().RelativePath);
    }

    [TestMethod]
    public void ReadSounds_NazvySuKluceACestaSaRozdeli()
    {
        var language = ReadFullBank();

        Assert.HasCount(2, language.Groups);
        var group = language.Groups[0];
        Assert.AreEqual("R1", group.Key);
        Assert.AreEqual("R1", group.Name);
        Assert.AreEqual("R1\\", group.RelativePath);
        Assert.HasCount(3, group.Sounds);
        Assert.IsEmpty(language.Groups[1].Sounds);

        var first = group.Sounds[0];
        Assert.AreEqual("9900100", first.Key);
        Assert.AreEqual("9900100", first.Name);
        Assert.AreEqual("Dolné Mesto & okolie", first.Text);
        Assert.AreEqual("9900100.EWA", first.FileName);
        Assert.AreEqual("", first.AdditionalRelativePath);
        Assert.AreEqual(1020, first.Duration);
        Assert.AreEqual("9ae55cb7eec4efbea9d7e188b07d3ff1", first.Md5);

        var second = group.Sounds[1];
        Assert.AreEqual("9900150.WAV", second.FileName);
        Assert.AreEqual("..\\N5\\", second.AdditionalRelativePath);
        Assert.AreEqual(3, second.Fid);
    }

    [TestMethod]
    public void ReadSounds_BezDlzkyAMd5()
    {
        var sound = ReadFullBank().Groups[0].Sounds[2];

        Assert.AreEqual(0, sound.Duration);
        Assert.IsNull(sound.Md5);
    }

    [TestMethod]
    public void Zapis_NacitanaBankaSaZapiseRovnako()
    {
        var language = ReadFullBank();
        using var stream = new MemoryStream();

        FyzBankXmlWriter.Write(stream, language, language.IsBasic, language.Date!.Value, s => s.Md5);

        // od povodneho suboru sa lisi len zapis kodovania v hlavicke
        var written = new UTF8Encoding(false).GetString(stream.ToArray()).TrimStart('﻿');
        Assert.AreEqual(FULL_BANK.Replace("\r\n", "\n").Replace("UTF-8", "utf-8"), written.Replace("\r\n", "\n"));
    }
}
