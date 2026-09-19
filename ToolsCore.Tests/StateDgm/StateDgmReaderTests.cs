using System.Diagnostics.CodeAnalysis;
using ToolsCore.StateDgm;

namespace ToolsCore.Tests.StateDgm;

/// <summary>
///     Citac stromu StateDgm.txt - syntax podla INISSu 3.39.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class StateDgmReaderTests
{
    [TestMethod]
    public void Reader_BasicSyntax()
    {
        const string text = "0001\r\nC:\"Popis\"\r\nC:\"neukončený\r\n; komentár\r\nP:\"A\\\\B\"\r\n{\t; komentár za zátvorkou\r\n\tS:\"Key\"=\"x\\\"y\\\\z\\n\"\r\n\tI:\"Hex\"=0x18 ; SVSA\r\n\tI:\"Oct\"=010\r\n\tI:\"Neg\"=-1200\r\n\tB:\"Ano\"=Ano B:\"Ne\"=No\r\n\tG:\"Sub\" {S:\"Class\"=\"C\" I:\"N\"=1}\r\n\tG:\"Sub\" {I:\"N\"=2}\r\n\tS:\"Del\"=#\r\n}\r\nP:\"A\\\\C\" { }\r\n";
        var f = StateDgmReader.Read(text);

        CollectionAssert.AreEqual(new[] { "Popis", "neukončený" }, f.HeaderComments);
        var a = f.Root.Group("A")!;
        Assert.AreEqual(2, a.Groups.Count(), "A\\\\B a A\\\\C zdieľajú medzičlánok A");
        var b = a.Group("B")!;
        Assert.AreEqual("x\"y\\z\n", b.GetString("Key"));
        Assert.AreEqual(0x18, b.GetInt("Hex"));
        Assert.AreEqual("0x18", b.Value("Hex")!.Raw);
        Assert.AreEqual(8, b.GetInt("Oct"));
        Assert.AreEqual(-1200, b.GetInt("Neg"));
        Assert.IsNull(b.Value("Neg")!.Raw);
        Assert.IsTrue(b.GetBool("Ano"));
        Assert.IsFalse(b.GetBool("Ne"));
        Assert.AreEqual(2, b.GroupsNamed("Sub").Count(), "opakované meno skupiny vytvorí ďalšiu skupinu");
        Assert.AreEqual(2, b.Group("Sub")!.GetInt("N"), "pri hľadaní podľa mena vyhráva posledná");
        Assert.IsTrue(b.Values.Single(v => v.Key == "Del").IsRemoval);
        Assert.IsNull(b.Value("Del"));
        Assert.AreEqual(4, b.Line);
        Assert.AreEqual(6, b.Value("Key")!.Line);
    }

    [TestMethod]
    public void Reader_Errors()
    {
        Throws("0002\r\nP:\"A\" {}", "verzia", 0);
        Throws("0001\r\nX:\"A\" {}", "Neznámy typ", 1);
        Throws("0001\r\nP \"A\" {}", "očakáva :", 1);
        Throws("0001\r\nP:\"A\"\r\nS:\"k\"=\"v\"", "očakáva {", 2);
        Throws("0001\r\nP:\"A\" {\r\nS:\"k\"=\"v\"\r\n", "koniec súboru", 3);
        Throws("0001\r\n}", "mimo skupiny", 1);
        Throws("0001\r\nP:\"A\" { I:\"k\"=1x2 }", "nie je číslo", 1);
        Throws("0001\r\nP:\"A\" { I:\"k\"= }", "číselnú hodnotu", 1);
        Throws("0001\r\nP:\"A\" { B:\"k\"=ano }", "nie je Ano ani Ne", 1);
        Throws("0001\r\nP:\"A\" { S:\"k\"=abc }", "reťazec v úvodzovkách", 1);
        Throws("0001\r\nP:\"A\" { S:\"k\" \"abc\" }", "očakáva =", 1);
        Throws("0001\r\nP:\"A\" { S:\"k\"=\"abc }", "Neukončený reťazec", 1);
        Throws("0001\r\nP:\"A\" { A:\"k\"=00 }", "Typ A:", 1);
    }

    [TestMethod]
    public void Reader_StringEscapes()
    {
        var f = StateDgmReader.Read("0001\r\nP:\"A\" { S:\"k\"=\"\\x41\\x4a\\t\\a\\q\\x\" }");
        Assert.AreEqual("AJ\t\aqx", f.Root.Group("A")!.GetString("k"));
    }

    [TestMethod]
    public void Writer_EscapesRoundTrip()
    {
        var text = "a\"b\\c\nd\te\u0001";
        var f = StateDgmReader.Read("0001\r\nP:\"A\" { " + StateDgmWriter.Str("k", text) + " }");
        Assert.AreEqual(text, f.Root.Group("A")!.GetString("k"));
    }

    private static void Throws(string text, string fragment, int line)
    {
        try
        {
            StateDgmReader.Read(text);
            Assert.Fail($"očakávala sa chyba „{fragment}“ pre: {text}");
        }
        catch (StateDgmParseException e)
        {
            StringAssert.Contains(e.Message, fragment, $"text: {text}");
            Assert.AreEqual(line, e.Line, $"riadok chyby pre: {text}");
        }
    }
}
