using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Pisma programu: rozhranie (okna, ponuky, tlacidla, stavovy riadok), tabulky (bunky aj hlavicky) a editor
/// (len GVDEditor - editor TabTab a stavoveho diagramu). Neuvedene pismo = predvolene.
/// </summary>
public sealed record AppFonts
{
    /// <summary>
    /// Predvolene pismo editora.
    /// </summary>
    public static FontChoice DefaultEditor { get; } = new("Consolas", 10);

    // predvolene pismo rozhrania (pismo WinForms od .NET Core) a ponuk; SystemFonts vytvara pri kazdom volani nove pismo
    private static readonly Font DefaultControls = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
    private static readonly Font SystemMenus = SystemFonts.MenuFont ?? DefaultControls;

    // predvolene pismo starsich verzii - v starej konfiguracii neznamena volbu pouzivatela
    private static readonly Font LegacyDefault = SystemFonts.DefaultFont;

    private Font? _interface;
    private Font? _cells;
    private Font? _headers;
    private Font? _editor;

    /// <summary>
    /// Vlastne pismo rozhrania; <see langword="null" /> = Segoe UI 9, ponuky ako vo Windows.
    /// </summary>
    [XmlElement("Interface")]
    public FontChoice? Interface
    {
        get;
        set
        {
            field = value;
            _interface = _cells = _headers = null;
        }
    }

    /// <summary>
    /// Vlastne pismo tabuliek (bunky aj hlavicky stlpcov); <see langword="null" /> = ako rozhranie.
    /// </summary>
    [XmlElement("Tables")]
    public FontChoice? Tables
    {
        get;
        set
        {
            field = value;
            _cells = _headers = null;
        }
    }

    /// <summary>
    /// Vlastne pismo editora; <see langword="null" /> = <see cref="DefaultEditor" />.
    /// </summary>
    [XmlElement("Editor")]
    public FontChoice? Editor
    {
        get;
        set
        {
            field = value;
            _editor = null;
        }
    }

    /// <summary>
    /// Pismo okien a tlacidiel.
    /// </summary>
    [XmlIgnore]
    public Font Controls => Interface is null ? DefaultControls : _interface ??= Interface.ToFont();

    /// <summary>
    /// Pismo ponuk, panelov nastrojov a stavoveho riadka.
    /// </summary>
    [XmlIgnore]
    public Font Menus => Interface is null ? SystemMenus : Controls;

    /// <summary>
    /// Pismo buniek tabuliek.
    /// </summary>
    [XmlIgnore]
    public Font Cells => Tables is null ? Controls : _cells ??= Tables.ToFont();

    /// <summary>
    /// Pismo hlaviciek stlpcov tabuliek.
    /// </summary>
    [XmlIgnore]
    public Font Headers => Tables is null ? Menus : _headers ??= Tables.ToFont();

    /// <summary>
    /// Pismo editora.
    /// </summary>
    [XmlIgnore]
    public Font EditorFont => _editor ??= (Editor ?? DefaultEditor).ToFont();

    /// <summary>
    /// Predvolene pismo rozhrania (bez vlastneho pisma).
    /// </summary>
    public static Font DefaultInterface => DefaultControls;

    /// <summary>
    /// Prevezme pisma zo starsej konfiguracie (pismo popisov = rozhranie, bunky tabuliek = tabulky); hodnoty
    /// zhodne s predvolenym pismom (aj starsich verzii) sa neprevezmu.
    /// </summary>
    internal void TakeLegacy(LegacyControlFonts legacy)
    {
        if (Changed(legacy.Labels) is { } labels)
            Interface = labels;
        if (Changed(legacy.TableCells) is { } cells)
            Tables = cells;
    }

    private static FontChoice? Changed(LegacyFont? legacy)
    {
        if (legacy?.Font is null)
            return null;

        using var font = XmlFont.ToFont(legacy.Font);
        var choice = FontChoice.Of(font);
        return choice.Matches(DefaultControls) || choice.Matches(LegacyDefault) ? null : choice;
    }

    /// <inheritdoc />
    public bool Equals(AppFonts? other) =>
        other is not null && Equals(Interface, other.Interface) && Equals(Tables, other.Tables) && Equals(Editor, other.Editor);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Interface, Tables, Editor);
}
