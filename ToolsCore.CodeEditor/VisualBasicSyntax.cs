using ScintillaNET;
using ColorSetting = ToolsCore.XML.ColorSetting;

namespace ToolsCore.CodeEditor;

/// <summary>
/// Zvyraznenie kodu VB.NET (lexer Lexilla <c>vb</c>) vo farbach svetlej alebo tmavej temy podla Visual Studia.
/// </summary>
public static class VisualBasicSyntax
{
    /// <summary>Klucove slova VB.NET (lexer porovnava malymi pismenami).</summary>
    public const string Keywords =
        "addhandler addressof alias and andalso as boolean byref byte byval call case catch cbool cbyte cchar cdate cdbl cdec char cint " +
        "class clng cobj const continue csbyte cshort csng cstr ctype cuint culng cushort date decimal declare default delegate dim " +
        "directcast do double each else elseif end enum erase error event exit false finally for friend function get gettype " +
        "global goto handles if implements imports in inherits integer interface is isnot let lib like long loop me mod module " +
        "mustinherit mustoverride mybase myclass namespace narrowing new next not nothing notinheritable notoverridable object of on " +
        "operator option optional or orelse overloads overridable overrides paramarray partial private property protected public " +
        "raiseevent readonly redim rem removehandler resume return sbyte select set shadows shared short single static step stop " +
        "string structure sub synclock then throw to true try trycast typeof uinteger ulong ushort using when while widening with " +
        "withevents writeonly xor";

    /// <summary>
    /// Nastavi lexer a farby. Predvoleny styl (pismo a pozadie) musi byt nastaveny pred volanim.
    /// </summary>
    /// <param name="scintilla">editor</param>
    /// <param name="dark">tmava tema</param>
    public static void Apply(Scintilla scintilla, bool dark)
    {
        ArgumentNullException.ThrowIfNull(scintilla);
        scintilla.LexerName = "vb";
        scintilla.SetKeywords(0, Keywords);

        var text = scintilla.Styles[Style.Default].ForeColor;
        Color Pick(int light, int darkColor) => Color.FromArgb(255, Color.FromArgb(dark ? darkColor : light));

        void Set(int style, Color color, bool bold = false)
        {
            scintilla.Styles[style].ForeColor = color;
            scintilla.Styles[style].Bold = bold;
        }

        Set(Style.Vb.Default, text);
        Set(Style.Vb.Identifier, text);
        Set(Style.Vb.Operator, text);
        Set(Style.Vb.Comment, Pick(0x008000, 0x57A64A));
        Set(Style.Vb.CommentBlock, Pick(0x008000, 0x57A64A));
        Set(Style.Vb.Keyword, Pick(0x0000FF, 0x569CD6));
        Set(Style.Vb.Keyword2, Pick(0x2B91AF, 0x4EC9B0));
        Set(Style.Vb.Number, Pick(0x098658, 0xB5CEA8));
        Set(Style.Vb.HexNumber, Pick(0x098658, 0xB5CEA8));
        Set(Style.Vb.BinNumber, Pick(0x098658, 0xB5CEA8));
        Set(Style.Vb.String, Pick(0xA31515, 0xD69D85));
        Set(Style.Vb.StringEol, Pick(0xA31515, 0xD69D85));
        Set(Style.Vb.Date, Pick(0xA31515, 0xD69D85));
        Set(Style.Vb.Preprocessor, Pick(0x808080, 0x9B9B9B));
        Set(Style.Vb.Error, Pick(0xFF0000, 0xF44747));
    }

    /// <summary>
    /// Nastavi lexer a farby schemy programu: text a pozadie, zvyraznenie, cisla riadkov a podciarknutie zisteni.
    /// Pismo predvoleneho stylu musi byt nastavene pred volanim.
    /// </summary>
    /// <param name="scintilla">editor</param>
    /// <param name="scheme">farby editora kodu VB.NET</param>
    public static void Apply(Scintilla scintilla, VisualBasicScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scintilla);
        ArgumentNullException.ThrowIfNull(scheme);
        scintilla.LexerName = "vb";
        scintilla.SetKeywords(0, Keywords);
        scintilla.Styles[Style.Default].ForeColor = scheme.Text.ForeColor;
        scintilla.Styles[Style.Default].BackColor = scheme.Text.BackColor;
        scintilla.StyleClearAll();
        scintilla.CaretForeColor = scheme.Text.ForeColor;
        scintilla.Styles[Style.LineNumber].ForeColor = scheme.LineNumber.ForeColor;
        scintilla.Styles[Style.LineNumber].BackColor = scheme.LineNumber.BackColor;

        void Set(ColorSetting setting, params int[] styles)
        {
            foreach (var style in styles)
            {
                scintilla.Styles[style].ForeColor = setting.ForeColor;
                scintilla.Styles[style].Bold = setting.Bold;
            }
        }

        Set(scheme.Text, Style.Vb.Default, Style.Vb.Identifier, Style.Vb.Keyword2);
        Set(scheme.Operator, Style.Vb.Operator);
        Set(scheme.Comment, Style.Vb.Comment, Style.Vb.CommentBlock);
        Set(scheme.Keyword, Style.Vb.Keyword);
        Set(scheme.Number, Style.Vb.Number, Style.Vb.HexNumber, Style.Vb.BinNumber);
        Set(scheme.String, Style.Vb.String, Style.Vb.StringEol, Style.Vb.Date);
        Set(scheme.Preprocessor, Style.Vb.Preprocessor);
        Set(scheme.Error, Style.Vb.Error);
        scintilla.Indicators[8].ForeColor = scheme.Error.ForeColor;
        scintilla.Indicators[9].ForeColor = scheme.Warning.ForeColor;
    }
}
