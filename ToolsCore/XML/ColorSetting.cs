using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
///     Trieda reprezentujúca nastavenia farieb a písma pre GUI.
/// </summary>
public record ColorSetting
{
    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="ColorSetting"/>.
    /// </summary>
    public ColorSetting()
    {
    }

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="ColorSetting"/> so zadafinovanymi farbami pozadia, popredia
    ///     a ci sa bude dat menit pismo pre dane nastavenie v GUI.
    /// </summary>
    public ColorSetting(Color backColor, Color foreColor, bool disableFontBoldEdit = false)
    {
        BackColor = backColor;
        ForeColor = foreColor;
        DisableFontBoldEdit = disableFontBoldEdit;
        DisableBackColorEdit = false;
    }

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="ColorSetting"/> so zadafinovanymi farbami popredia
    ///     a ci sa bude dat menit pismo pre dane nastavenie v GUI.
    /// </summary>
    public ColorSetting(Color foreColor, bool disableFontBoldEdit = false)
    {
        BackColor = default;
        ForeColor = foreColor;
        DisableFontBoldEdit = disableFontBoldEdit;
        DisableBackColorEdit = true;
    }

    /// <summary>
    ///     Názov nastavenia, ktoré sa bude zobrazovať v GUI.
    ///     Lokalizovateľná
    /// </summary>
    [XmlIgnore, Localizable(true)]  
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Farba pozadia.
    /// </summary>
    [XmlIgnore] 
    public Color BackColor { get; set; }

    /// <summary>
    ///     Farba popredia.
    /// </summary>
    [XmlIgnore] 
    public Color ForeColor { get; set; }

    /// <summary>
    ///     Použiť hrubý rez písma.
    /// </summary>
    [XmlAttribute("bold")] 
    public bool Bold { get; set; }

    /// <summary>
    ///     Nastavi, aby pouzivatel nemohol menit farbu pozadia - BackColor.
    /// </summary>
    [XmlIgnore] 
    public bool DisableBackColorEdit { get; set; }

    /// <summary>
    ///     Nastavi, aby pouzivatel nemohol menit pismo.
    /// </summary>
    [XmlIgnore] 
    public bool DisableFontBoldEdit { get; set; }

    /// <summary>
    ///     Farba pozadia vo formáte XML atribútu.
    /// </summary>
    [XmlAttribute("b"), DefaultValue("#00000000")]
    public string ColorBackAttr
    {
        get => XmlColor.FromColor(BackColor);
        set => BackColor = XmlColor.ToColor(value);
    }

    /// <summary>
    ///     Farba popredia vo formáte XML atribútu.
    /// </summary>
    [XmlAttribute("f")]
    public string ColorForeAttr
    {
        get => XmlColor.FromColor(ForeColor);
        set => ForeColor = XmlColor.ToColor(value);
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}