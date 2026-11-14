using ScintillaNET;
using ToolsCore.CodeEditor;

namespace ToolsCore.Tests.CodeEditor;

/// <summary>
/// Farba nazvu metody v editore VB.NET: indikator nad identifikatorom pred <c>(</c> (volanie aj definicia), nie nad polom
/// v deklaracii, typom, klucovym slovom, retazcom ani komentarom.
/// </summary>
[STATestClass]
public class VisualBasicMethodsTests
{
    private static List<string> MarkedWords(string code)
    {
        using var scintilla = new Scintilla();
        scintilla.CreateControl();
        VisualBasicSyntax.Apply(scintilla, false);
        scintilla.Text = code;

        VisualBasicMethods.Mark(scintilla, 0, scintilla.Lines.Count - 1);

        var words = new List<string>();
        for (var position = 0; position < scintilla.TextLength;)
        {
            if ((scintilla.IndicatorAllOnFor(position) & (1u << VisualBasicMethods.Indicator)) == 0)
            {
                position++;
                continue;
            }

            var end = scintilla.Indicators[VisualBasicMethods.Indicator].End(position);
            words.Add(scintilla.GetTextRange(position, end - position));
            position = end;
        }

        return words;
    }

    [TestMethod]
    public void Metoda_VolanieAjDefinicia()
    {
        const string code = "Function Cas(ByVal x As Integer) As Date\r\n" +
                            "    Return .DatumCasOdj(True).AddSeconds (-600)\r\n" +
                            "End Function";

        CollectionAssert.AreEqual(new[] { "Cas", "DatumCasOdj", "AddSeconds" }, MarkedWords(code));
    }

    [TestMethod]
    public void Metoda_BezPolaTypuKlucovehoSlovaATextu()
    {
        const string code = "Dim pole(5) As Integer\r\n" +
                            "Dim zoznam As New List(Of String)\r\n" +
                            "Dim x = CType(.Druh, String) & \"Volanie(1)\" ' Komentar(2)\r\n" +
                            "If .Druh = \"R\" Then Return Format(.Cas, \"HH:mm\")";

        CollectionAssert.AreEqual(new[] { "Format" }, MarkedWords(code));
    }

    [TestMethod]
    public void Mark_ZmazePovodneOznacenie()
    {
        using var scintilla = new Scintilla();
        scintilla.CreateControl();
        VisualBasicSyntax.Apply(scintilla, false);
        scintilla.Text = "Return Foo(1)";
        VisualBasicMethods.Mark(scintilla, 0, 0);

        scintilla.Text = "Return Foo + 1";
        VisualBasicMethods.Mark(scintilla, 0, 0);

        Assert.AreEqual(0u, scintilla.IndicatorAllOnFor(8) & (1u << VisualBasicMethods.Indicator));
    }
}
