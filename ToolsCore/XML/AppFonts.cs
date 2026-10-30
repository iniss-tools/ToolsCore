using System.Globalization;
using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Pismo zvolene pouzivatelom: rodina a velkost v bodoch (rez je vzdy obycajny, tucne urcuje farebna schema).
/// </summary>
public sealed record FontChoice
{
    /// <summary>
    /// Pre XmlSerializer.
    /// </summary>
    public FontChoice()
    {
    }

    /// <summary>
    /// Pismo s danou rodinou a velkostou.
    /// </summary>
    public FontChoice(string family, float size)
    {
        Family = family;
        Size = size;
    }

    /// <summary>
    /// Rodina pisma.
    /// </summary>
    [XmlAttribute("family")]
    public string Family { get; set; } = "";

    /// <summary>
    /// Velkost v bodoch.
    /// </summary>
    [XmlAttribute("size")]
    public float Size { get; set; }

    /// <summary>
    /// Volba podla existujuceho pisma (rodina a velkost v bodoch).
    /// </summary>
    public static FontChoice Of(Font font) => new(font.FontFamily.Name, font.SizeInPoints);

    /// <summary>
    /// Vytvori pismo; chybajucu rodinu nahradi system.
    /// </summary>
    public Font ToFont() => new(Family, Size, FontStyle.Regular, GraphicsUnit.Point);

    /// <summary>
    /// Ta ista rodina a velkost ako <paramref name="font" />.
    /// </summary>
    public bool Matches(Font font) =>
        string.Equals(Family, font.FontFamily.Name, StringComparison.OrdinalIgnoreCase) && Math.Abs(Size - font.SizeInPoints) < 0.01f;

    /// <inheritdoc />
    public override string ToString() => Family + ", " + Size.ToString(CultureInfo.CurrentCulture);
}

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

    // systemove pisma - SystemFonts vytvara pri kazdom volani nove pismo
    private static readonly Font SystemControls = SystemFonts.DefaultFont;
    private static readonly Font SystemMenus = SystemFonts.MenuFont ?? SystemFonts.DefaultFont;

    private Font? _interface;
    private Font? _cells;
    private Font? _headers;
    private Font? _editor;

    /// <summary>
    /// Vlastne pismo rozhrania; <see langword="null" /> = systemove pisma (text okien a ponuky ako vo Windows).
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
    public Font Controls => Interface is null ? SystemControls : _interface ??= Interface.ToFont();

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
    public static Font DefaultInterface => SystemControls;

    /// <summary>
    /// Prevezme pisma zo starsej konfiguracie (pismo popisov = rozhranie, bunky tabuliek = tabulky); hodnoty
    /// zhodne so systemovym pismom sa neprevezmu.
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
        return choice.Matches(SystemControls) ? null : choice;
    }

    /// <inheritdoc />
    public bool Equals(AppFonts? other) =>
        other is not null && Equals(Interface, other.Interface) && Equals(Tables, other.Tables) && Equals(Editor, other.Editor);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Interface, Tables, Editor);
}

/// <summary>
/// Pisma v konfiguracii do verzie s <see cref="AppFonts" /> (prvok <c>ControlFonts</c>) - len na citanie.
/// </summary>
public sealed class LegacyControlFonts
{
    /// <summary>
    /// Pismo popisov formularov.
    /// </summary>
    [XmlElement("Labels")]
    public LegacyFont? Labels { get; set; }

    /// <summary>
    /// Pismo buniek tabuliek.
    /// </summary>
    [XmlElement("TableCells")]
    public LegacyFont? TableCells { get; set; }
}

/// <summary>
/// Jedno pismo starsej konfiguracie.
/// </summary>
public sealed class LegacyFont
{
    /// <summary>
    /// Pismo.
    /// </summary>
    [XmlElement("f")]
    public XmlFont? Font { get; set; }
}
