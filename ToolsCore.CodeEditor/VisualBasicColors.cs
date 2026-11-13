using ToolsCore.CodeEditor.Properties;
using ToolsCore.XML;

namespace ToolsCore.CodeEditor;

/// <summary>
/// Polozky farebnej schemy editora kodu VB.NET; predvolene farby su podla Visual Studia pre svetlu a tmavu schemu.
/// </summary>
public static class VisualBasicColors
{
    private const string TextId = "VisualBasic.Text";
    private const ColorParts SyntaxParts = ColorParts.Fore | ColorParts.Bold;

    /// <summary>Kategoria Editor kodu VB.NET.</summary>
    public static ColorCategory Category { get; } = new("VisualBasic", () => Resources.Scheme_VisualBasic, () => new VisualBasicColorPreview());

    private static readonly Color DarkText = Color.FromArgb(220, 220, 220);
    // pozadie ako pole (box) tmavej schemy, aby editor nesplynul s panelom okna
    private static readonly Color DarkBack = Color.FromArgb(37, 37, 38);

    private static ColorKey Key(string id, Func<string> name, ColorValue light, ColorValue dark, ColorParts parts, Func<VisualBasicScheme, ColorSetting> setting,
        string? contrastWith = TextId) =>
        new("VisualBasic." + id, Category, name, light, dark, parts, style => setting(((IVisualBasicStyle)style).VisualBasicScheme), null, contrastWith);

    private static ColorKey Syntax(string id, Func<string> name, int light, int dark, Func<VisualBasicScheme, ColorSetting> setting) =>
        Key(id, name, new ColorValue(Rgb(light), Color.Empty), new ColorValue(Rgb(dark), Color.Empty), SyntaxParts, setting);

    private static Color Rgb(int value) => Color.FromArgb(255, Color.FromArgb(value));

    /// <summary>Text a pozadie editora.</summary>
    public static ColorKey Text { get; } = Key("Text", () => Resources.Scheme_VbText, new ColorValue(Color.Black, Color.White), new ColorValue(DarkText, DarkBack),
        ColorParts.ForeBack, scheme => scheme.Text, null);

    /// <summary>
    /// Polozky v poradi stromu na stranke schemy.
    /// </summary>
    public static IReadOnlyList<ColorKey> Keys { get; } =
    [
        Text,
        Syntax("Keyword", () => Resources.Scheme_VbKeyword, 0x0000FF, 0x569CD6, scheme => scheme.Keyword),
        Syntax("String", () => Resources.Scheme_VbString, 0xA31515, 0xD69D85, scheme => scheme.String),
        Syntax("Number", () => Resources.Scheme_VbNumber, 0x098658, 0xB5CEA8, scheme => scheme.Number),
        Syntax("Comment", () => Resources.Scheme_VbComment, 0x008000, 0x57A64A, scheme => scheme.Comment),
        Syntax("Preprocessor", () => Resources.Scheme_VbPreprocessor, 0x808080, 0x9B9B9B, scheme => scheme.Preprocessor),
        Key("Operator", () => Resources.Scheme_VbOperator, new ColorValue(Color.Black, Color.Empty), new ColorValue(DarkText, Color.Empty), SyntaxParts,
            scheme => scheme.Operator),
        Key("LineNumber", () => Resources.Scheme_VbLineNumber, new ColorValue(Rgb(0x2B91AF), Color.White), new ColorValue(Rgb(0x858585), DarkBack),
            ColorParts.ForeBack, scheme => scheme.LineNumber, null),
        Key("Error", () => Resources.Scheme_VbError, new ColorValue(Color.Red, Color.Empty), new ColorValue(Color.FromArgb(244, 71, 71), Color.Empty),
            ColorParts.Fore, scheme => scheme.Error),
        Key("Warning", () => Resources.Scheme_VbWarning, new ColorValue(Color.FromArgb(200, 130, 0), Color.Empty),
            new ColorValue(Color.FromArgb(220, 170, 50), Color.Empty), ColorParts.Fore, scheme => scheme.Warning)
    ];
}
