using System.ComponentModel;
using AutocompleteMenuNS;
using ExControls;
using ScintillaNET;
using ToolsCore.Icons;

namespace ToolsCore.CodeEditor;

// tema, VB.NET, podciarknute zistenia s popisom pri mysi a dopĺňanie mien
public partial class CodeEditorBox
{
    private const int ErrorIndicator = 8;
    private const int WarningIndicator = 9;

    private readonly List<CodeIssue> _issues = [];
    private AutocompleteMenu? _completion;
    private ImageList? _completionImages;

    /// <summary>
    /// Jednoriadkovy editor (vyraz v atribute XML): bez cisel riadkov, Enter nevlozi novy riadok.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SingleLine { get; set; }

    /// <summary>
    /// Text editora.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Code
    {
        get => scintillaEditor.Text;
        set
        {
            scintillaEditor.Text = value;
            scintillaEditor.EmptyUndoBuffer();
            SwitchedDocument();
        }
    }

    /// <summary>
    /// Farby a pismo podla schemy programu: pozadie, cisla riadkov, kurzor, vyber, ramik a tmave posuvniky.
    /// </summary>
    /// <param name="style">schema programu</param>
    /// <param name="font">pismo editora</param>
    public void ApplyTheme(XML.Style style, Font font)
    {
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(font);
        var sc = scintillaEditor;
        var scheme = style.ControlsColorScheme;
        sc.StyleResetDefault();
        sc.Styles[ScintillaNET.Style.Default].Font = font.Name;
        sc.Styles[ScintillaNET.Style.Default].SizeF = font.SizeInPoints;
        sc.Styles[ScintillaNET.Style.Default].BackColor = scheme.Box.BackColor;
        sc.Styles[ScintillaNET.Style.Default].ForeColor = scheme.Box.ForeColor;
        sc.StyleClearAll();
        sc.Styles[ScintillaNET.Style.LineNumber].BackColor = scheme.Button.BackColor;
        sc.Styles[ScintillaNET.Style.LineNumber].ForeColor = scheme.Button.ForeColor;
        sc.Styles[ScintillaNET.Style.CallTip].BackColor = scheme.Panel.BackColor;
        sc.Styles[ScintillaNET.Style.CallTip].ForeColor = scheme.Panel.ForeColor;
        sc.CaretForeColor = scheme.Box.ForeColor;
        // vyber textu vo farbe zvyraznenia temy, nie farbou ramika
        sc.SetSelectionBackColor(true, scheme.Highlight.BackColor);
        sc.SetSelectionForeColor(true, scheme.Highlight.ForeColor);
        sc.SetAdditionalSelBack(scheme.Highlight.BackColor);
        sc.SetAdditionalSelFore(scheme.Highlight.ForeColor);
        if (!style.ControlsDefaultStyle)
            sc.BorderStyle = ScintillaNET.BorderStyle.None;
        if (style.DarkScrollBar)
        {
            VScrollBarControl.SetTheme(WindowsTheme.DarkExplorer);
            HScrollBarControl.SetTheme(WindowsTheme.DarkExplorer);
        }

        if (_completion is not null)
            ApplyCompletionColors(style);
    }

    /// <summary>
    /// Editor kodu VB.NET: tema, zvyraznenie, cisla riadkov, odsadenie a podciarkovanie zisteni.
    /// </summary>
    /// <param name="style">schema programu</param>
    /// <param name="font">pismo editora</param>
    public void UseVisualBasic(XML.Style style, Font font)
    {
        ArgumentNullException.ThrowIfNull(style);
        ApplyTheme(style, font);
        var sc = scintillaEditor;
        var dark = AppIcons.IsDarkBackground(style.ControlsColorScheme.Box.BackColor);
        // farby editora zo schemy programu, inak predvolene podla jasu pozadia
        if (style is IVisualBasicStyle visualBasic)
        {
            VisualBasicSyntax.Apply(sc, visualBasic.VisualBasicScheme);
            sc.Styles[ScintillaNET.Style.CallTip].BackColor = style.ControlsColorScheme.Panel.BackColor;
            sc.Styles[ScintillaNET.Style.CallTip].ForeColor = style.ControlsColorScheme.Panel.ForeColor;
        }
        else
        {
            VisualBasicSyntax.Apply(sc, dark);
        }

        sc.TabWidth = 4;
        sc.UseTabs = false;
        sc.IndentWidth = 4;
        sc.ViewWhitespace = WhitespaceMode.Invisible;
        sc.Margins[0].Type = MarginType.Number;
        UpdateLineNumberWidth();
        sc.TextChanged -= LineNumbers_TextChanged;
        sc.TextChanged += LineNumbers_TextChanged;
        sc.KeyDown -= SingleLine_KeyDown;
        sc.KeyDown += SingleLine_KeyDown;
        if (SingleLine)
            sc.Margins[1].Width = 0;

        sc.Indicators[ErrorIndicator].Style = IndicatorStyle.Squiggle;
        sc.Indicators[WarningIndicator].Style = IndicatorStyle.Squiggle;
        if (style is not IVisualBasicStyle)
        {
            sc.Indicators[ErrorIndicator].ForeColor = dark ? Color.FromArgb(244, 71, 71) : Color.Red;
            sc.Indicators[WarningIndicator].ForeColor = dark ? Color.FromArgb(220, 170, 50) : Color.FromArgb(200, 130, 0);
        }
        sc.MouseDwellTime = 400;
        sc.DwellStart -= Issues_DwellStart;
        sc.DwellStart += Issues_DwellStart;
        sc.DwellEnd -= Issues_DwellEnd;
        sc.DwellEnd += Issues_DwellEnd;
    }

    private void LineNumbers_TextChanged(object? sender, EventArgs e) => UpdateLineNumberWidth();

    private void SingleLine_KeyDown(object? sender, KeyEventArgs e)
    {
        // Enter v jednoriadkovom editore patri oknu (predvolene tlacidlo), nie textu
        if (SingleLine && e.KeyCode == Keys.Enter)
            e.SuppressKeyPress = true;
    }

    // sirka okraja s cislami riadkov podla poctu cislic; jednoriadkovy editor ich nema
    private void UpdateLineNumberWidth()
    {
        if (SingleLine)
        {
            scintillaEditor.Margins[0].Width = 0;
            return;
        }

        var digits = Math.Max(2, scintillaEditor.Lines.Count.ToString(System.Globalization.CultureInfo.InvariantCulture).Length);
        scintillaEditor.Margins[0].Width = scintillaEditor.TextWidth(ScintillaNET.Style.LineNumber, new string('9', digits + 1));
    }

    /// <summary>
    /// Podciarkne zistenia; predchadzajuce zmizne.
    /// </summary>
    /// <param name="issues">zistenia s polohou v texte editora</param>
    public void SetIssues(IEnumerable<CodeIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        var sc = scintillaEditor;
        _issues.Clear();
        foreach (var indicator in new[] { ErrorIndicator, WarningIndicator })
        {
            sc.IndicatorCurrent = indicator;
            sc.IndicatorClearRange(0, sc.TextLength);
        }

        foreach (var issue in issues)
        {
            var start = Math.Clamp(issue.Start, 0, sc.TextLength);
            var length = issue.Length > 0 ? issue.Length : DefaultLength(start);
            length = Math.Clamp(length, 0, sc.TextLength - start);
            _issues.Add(issue with { Start = start, Length = length });
            if (length == 0)
                continue;
            sc.IndicatorCurrent = issue.IsError ? ErrorIndicator : WarningIndicator;
            sc.IndicatorFillRange(start, length);
        }
    }

    // bez dlzky sa podciarkne slovo na polohe, inak zvysok riadku
    private int DefaultLength(int start)
    {
        var sc = scintillaEditor;
        var end = sc.WordEndPosition(start, true);
        if (end > start)
            return end - start;
        var line = sc.Lines[sc.LineFromPosition(start)];
        return Math.Max(0, line.EndPosition - start);
    }

    private void Issues_DwellStart(object? sender, DwellEventArgs e)
    {
        if (e.Position < 0)
            return;
        var messages = _issues.Where(issue => e.Position >= issue.Start && e.Position <= issue.Start + issue.Length).Select(issue => issue.Message).ToList();
        if (messages.Count > 0)
            scintillaEditor.CallTipShow(e.Position, string.Join("\n", messages));
    }

    private void Issues_DwellEnd(object? sender, DwellEventArgs e) => scintillaEditor.CallTipCancel();

    /// <summary>
    /// Mena, ktore editor ponukne pri pisani: cleny za bodkou, ostatne od dvoch znakov.
    /// </summary>
    /// <param name="symbols">mena</param>
    /// <param name="style">schema programu (farby ponuky)</param>
    public void SetSymbols(IEnumerable<CodeSymbol> symbols, XML.Style style)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(style);
        if (_completion is null)
        {
            _completionImages = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
            _completion = new AutocompleteMenu
            {
                TargetControlWrapper = new ScintillaWrapper(scintillaEditor),
                AppearInterval = 300,
                MinFragmentLength = 2,
                SearchPattern = @"[\w\.]",
                MaximumSize = new Size(LogicalToDeviceUnits(380), LogicalToDeviceUnits(260)),
                ToolTipDuration = 60000,
                ImageList = _completionImages,
                Font = Font
            };
            Disposed += (_, _) =>
            {
                _completion.Dispose();
                _completionImages.Dispose();
            };
        }

        ApplyCompletionColors(style);
        _completion.SetAutocompleteItems(symbols.OrderBy(symbol => symbol.Name, StringComparer.OrdinalIgnoreCase).Select(Item).ToList());
    }

    private void ApplyCompletionColors(XML.Style style)
    {
        if (_completion is null || _completionImages is null)
            return;
        var panel = style.ControlsColorScheme.Panel;
        _completion.Colors.BackColor = panel.BackColor;
        _completion.Colors.ForeColor = panel.ForeColor;
        _completion.Colors.SelectedForeColor = panel.ForeColor;
        // poradie podla ImageIndex poloziek: clen, metoda, meno
        IconImages.Fill(_completionImages, LogicalToDeviceUnits(IconImages.Size), AppIcons.IsDarkBackground(panel.BackColor),
            ["number-symbol", "math-formula", "code"]);
    }

    private static AutocompleteItem Item(CodeSymbol symbol)
    {
        AutocompleteItem item = symbol.Kind switch
        {
            CodeSymbolKind.Member => new MethodAutocompleteItem(symbol.Name) { ImageIndex = 0 },
            CodeSymbolKind.MemberMethod => new MethodAutocompleteItem(symbol.Name) { ImageIndex = 1 },
            CodeSymbolKind.Global => new AutocompleteItem(symbol.Name) { ImageIndex = 2 },
            _ => new AutocompleteItem(symbol.Name) { ImageIndex = -1 }
        };
        item.ToolTipTitle = symbol.Signature;
        item.ToolTipText = symbol.Description;
        return item;
    }

    /// <summary>
    /// Otvori okno hladania (a nahradzania) v tomto editore.
    /// </summary>
    /// <param name="owner">okno, nad ktorym sa ukaze</param>
    /// <param name="replace">otvorit kartu nahradzania</param>
    public void ShowFindReplace(IWin32Window owner, bool replace)
    {
        var form = new FFindReplace(scintillaEditor, replace);
        form.FormClosed += (_, _) => form.Dispose();
        form.Show(owner);
    }
}
