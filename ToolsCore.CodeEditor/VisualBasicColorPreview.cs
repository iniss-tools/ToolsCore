using ScintillaNET;
using ToolsCore.XML;
using Style = ToolsCore.XML.Style;

namespace ToolsCore.CodeEditor;

/// <summary>
/// Nahlad farieb editora kodu VB.NET na stranke farebnej schemy: ukazka vyrazu zvyraznena ako v editore (aj nazvy metod),
/// s cislami riadkov a podciarknutim chyby a upozornenia. Klik na cast textu vyberie jej polozku schemy.
/// </summary>
internal sealed class VisualBasicColorPreview : Scintilla, IColorPreview
{
    // ukazka kodu servera INISS2 (kod VB.NET, nie text rozhrania)
    private const string Sample = "#Region \"Nastup\"\r\n" +
                                  "' vlak stoji a ma cas na nastup\r\n" +
                                  "If .Druh = \"R\" AndAlso .PlanDobaPobytu < 30 Then\r\n" +
                                  "    Return .DatumCasOdj(True).AddSeconds(-600)\r\n" +
                                  "End If\r\n" +
                                  "Return NeznamaPremenna\r\n" +
                                  "Dim nepouzita As Integer\r\n" +
                                  "#End Region";

    private const int ErrorIndicator = 8;
    private const int WarningIndicator = 9;
    private const string ErrorWord = "NeznamaPremenna";
    private const string WarningWord = "nepouzita";

    private static readonly Dictionary<int, string> StyleKeys = new()
    {
        [ScintillaNET.Style.Vb.Default] = "VisualBasic.Text",
        [ScintillaNET.Style.Vb.Identifier] = "VisualBasic.Text",
        [ScintillaNET.Style.Vb.Keyword] = "VisualBasic.Keyword",
        [ScintillaNET.Style.Vb.String] = "VisualBasic.String",
        [ScintillaNET.Style.Vb.Number] = "VisualBasic.Number",
        [ScintillaNET.Style.Vb.Comment] = "VisualBasic.Comment",
        [ScintillaNET.Style.Vb.Preprocessor] = "VisualBasic.Preprocessor",
        [ScintillaNET.Style.Vb.Operator] = "VisualBasic.Operator"
    };

    public VisualBasicColorPreview()
    {
        BorderStyle = ScintillaNET.BorderStyle.None;
        HScrollBar = false;
        VScrollBar = false;
        Margins[1].Width = 0;
        Margins[0].Type = MarginType.Number;
        Indicators[ErrorIndicator].Style = IndicatorStyle.Squiggle;
        Indicators[WarningIndicator].Style = IndicatorStyle.Squiggle;
    }

    /// <inheritdoc />
    public Control Control => this;

    /// <inheritdoc />
    public event EventHandler<string>? KeyClicked;

    /// <inheritdoc />
    public void ShowScheme(Style style)
    {
        if (style is not IVisualBasicStyle visualBasic)
            return;

        var scheme = visualBasic.VisualBasicScheme;
        ReadOnly = false;
        using (var font = new Font("Consolas", 10f))
        {
            StyleResetDefault();
            Styles[ScintillaNET.Style.Default].Font = font.Name;
            Styles[ScintillaNET.Style.Default].SizeF = font.SizeInPoints;
        }

        VisualBasicSyntax.Apply(this, scheme);
        Text = Sample;
        Margins[0].Width = TextWidth(ScintillaNET.Style.LineNumber, "999");
        Mark(ErrorIndicator, ErrorWord);
        Mark(WarningIndicator, WarningWord);
        VisualBasicMethods.Mark(this, 0, Lines.Count - 1);
        ReadOnly = true;
    }

    private void Mark(int indicator, string word)
    {
        IndicatorCurrent = indicator;
        IndicatorFillRange(Text.IndexOf(word, StringComparison.Ordinal), word.Length);
    }

    /// <inheritdoc />
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.X < Margins[0].Width)
        {
            KeyClicked?.Invoke(this, "VisualBasic.LineNumber");
            return;
        }

        var position = CharPositionFromPointClose(e.X, e.Y);
        if (position < 0)
            return;
        // podciarknutie ma prednost pred stylom textu pod nim
        if (IndicatorAllOnFor(position) is var indicators && (indicators & (1u << ErrorIndicator)) != 0)
            KeyClicked?.Invoke(this, "VisualBasic.Error");
        else if ((indicators & (1u << WarningIndicator)) != 0)
            KeyClicked?.Invoke(this, "VisualBasic.Warning");
        else if ((indicators & (1u << VisualBasicMethods.Indicator)) != 0)
            KeyClicked?.Invoke(this, "VisualBasic.Method");
        else if (StyleKeys.TryGetValue(GetStyleAt(position), out var key))
            KeyClicked?.Invoke(this, key);
    }
}
