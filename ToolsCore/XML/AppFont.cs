using System.Drawing.Design;
using System.Xml.Serialization;
using JetBrains.Annotations;
using ToolsCore.Converters;

namespace ToolsCore.XML;

/// <summary>
///     Trieda reprezentujuca nastavenie pisma pre určity ovladaci prvok programu.
/// </summary>
[TypeConverter(typeof(AppFontConverter))]
[Editor(typeof(AppFontEditor), typeof (UITypeEditor))]
public record AppFont()
{
    public AppFont(Font font) : this()
    {
        Font = font;
    }

    protected AppFont(AppFont orig)
    {
        Name = orig.Name;
        Font = new Font(orig.Font.FontFamily, orig.Font.Size, orig.Font.Style, orig.Font.Unit, orig.Font.GdiCharSet);
    }

    /// <summary>
    ///     Názov použitia písma pre program.
    /// </summary>
    [XmlIgnore, Localizable(true), Browsable(false)]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Písmo.
    /// </summary>
    [XmlIgnore]
    public Font Font { get; private set; } = null!;

    /// <summary>
    ///     Príklad pre vizualizáciu písma.
    /// </summary>
    [XmlIgnore]
    [Browsable(false)]
    [UsedImplicitly]
    public string Example => "OK1932Šč./jkl";

    /// <summary>
    ///     Textový komentár k vlastnostiam tohto písma.
    /// </summary>
    [XmlIgnore]
    [Browsable(false)]
    public string FontDescription =>
        $"{Font.Name}, {Math.Round(Font.SizeInPoints)}{(Font.Bold ? ", tučné " : "")}{(Font.Italic ? ", kurzíva " : "")}{(Font.Strikeout ? ", prečiarknuté " : "")}{(Font.Underline ? ",podčiarkuté " : "")}";

    /// <summary>
    ///     XML reprezentácia písma.
    /// </summary>
    [XmlElement(Type = typeof(XmlFont), ElementName = "f"),Browsable(false)]
    public XmlFont FontXML
    {
        get => XmlFont.FromFont(Font);
        set => Font = XmlFont.ToFont(value);
    }

    /// <summary>
    ///     Returns Font object from this AppFont instance.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static implicit operator Font(AppFont f) => f.Font;

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrEmpty(Name) ? Font.Name : Name;
}