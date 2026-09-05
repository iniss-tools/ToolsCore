using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
///     Trieda reprezentujuca strukturu pre konfiguracny subor vo formate XML.
/// </summary>
[XmlRoot("CONFIG")]
public record ConfigBase()
{
    /// <summary>
    ///     Mod zobrazovania chybovych hlasok v GUI.
    /// </summary>
    [XmlElement("DebugModeGUI")]
    public DebugMode DebugModeGUI { get; set; } = DebugMode.OnlyMessage;

    /// <summary>
    ///     Zobrazenie menu v hlavnom dialogu programu.
    /// </summary>
    [XmlElement("DesktopMenu")]
    public DesktopMenu DesktopMenuMode { get; set; } = DesktopMenu.MsTs;

    /// <summary>
    ///     Pouzivat klasicky dizajn komponetov v GUI.
    /// </summary>
    [XmlElement("ClassicGUI"), DefaultValue(false)]
    public bool ClassicGUI { get; set; }

    /// <summary>
    ///     Nastavi jazyk pouzivatelskeho rozhrania (GUI).
    /// </summary>
    [XmlElement("Language"), DefaultValue(AppLanguage.Slovak)]
    public AppLanguage Language { get; set; } = AppLanguage.Slovak;

    /// <summary>
    ///     Nastavi, co sa ma diat ihned po zapnuti programu.
    /// </summary>
    [XmlElement("Startup"), DefaultValue(StartupType.EmptyWindow)]
    public StartupType Startup { get; set; } = StartupType.EmptyWindow;

    /// <summary>
    ///     Povolit alebo zakazat viacero instancii tohto programu.
    /// </summary>
    [XmlElement("MoreInstances"), DefaultValue(false)]
    public bool MoreInstance { get; set; }

    /// <summary>
    ///     Ci sa ma prisposobit posledny stlpec v tabulke na pracovnej ploche programu.
    /// </summary>
    [XmlElement("FitLastColumn"), DefaultValue(true)]
    public bool FitLastColumn { get; set; } = true;

    /// <summary>
    ///     Nastavenia pisiem pre jednotlive komponenty GUI.
    /// </summary>
    [XmlElement("ControlFonts"), TypeConverter(typeof(ExpandableObjectConverter))]
    public ControlFonts Fonts { get; set; } = new();

    /// <summary>
    ///     Vrati alebo nastavi, ci sa maju zobrazovat hlavicky riadkov v tabulke na pracovnej ploche programu.
    /// </summary>
    [XmlElement("ShowRowsHeader"), DefaultValue(true)]
    public bool ShowRowsHeader { get; set; } = true;

    /// <summary>
    ///      Vrati alebo nastavi ci sa maju logovat informacie a oznamy.
    ///     <see langword="true"/> ak logovat informacie o aplikacii, inak <see langword="false"/>.
    /// </summary>
    [XmlElement("LoggingInfo"), DefaultValue(true)]
    public bool LoggingInfo { get; set; } = true;

    /// <summary>
    ///     Vrati alebo nastavi ci sa maju logovat chyby.
    ///     <see langword="true"/> ak logovat chyby a vynimky, inak <see langword="false"/>.
    /// </summary>
    [XmlElement("LoggingError"), DefaultValue(true)]
    public bool LoggingError { get; set; } = true;

    [XmlIgnore]
    public virtual string? LinkAppSettingsGuide { get; }

    protected ConfigBase(ConfigBase original)
    {
        DebugModeGUI = original.DebugModeGUI;
        DesktopMenuMode = original.DesktopMenuMode;
        Startup = original.Startup;
        ClassicGUI = original.ClassicGUI;
        Language = original.Language;
        MoreInstance = original.MoreInstance;
        FitLastColumn = original.FitLastColumn;
        Fonts = original.Fonts with { };
        ShowRowsHeader = original.ShowRowsHeader;
        LoggingInfo = original.LoggingInfo;
        LoggingError = original.LoggingError;
    }
}

/// <summary>
///     Typ zobrazenia MenuStrip alebo ToolStrip na pracovnej ploche programu.
/// </summary>
public enum DesktopMenu
{
    /// <summary>
    ///     ToolStrip aj MenuStrip.
    /// </summary>
    [XmlEnum(Name = "0")]
    MsTs,

    /// <summary>
    ///     Len ToolStrip.
    /// </summary>
    [XmlEnum(Name = "1")] 
    TsOnly,

    /// <summary>
    ///     Len MenuStrip.
    /// </summary>
    [XmlEnum(Name = "2")] 
    MsOnly
}

/// <summary>
///     Typ debuggovania.
/// </summary>
public enum DebugMode
{
    /// <summary>
    ///     Normalne chybove hlasky.
    /// </summary>
    [XmlEnum(Name = "0")] 
    OnlyMessage,

    /// <summary>
    ///     Zobrazovanie detailnejsich chyb. hlasok.
    /// </summary>
    [XmlEnum(Name = "1")] 
    DetailInfo,

    /// <summary>
    ///     Bez chybovych hlasok (spadnutie programu).
    /// </summary>
    [XmlEnum(Name = "2")] 
    AppCrash
}

/// <summary>
///     Jazyk aplikacie.
/// </summary>
public enum AppLanguage
{
    /// <summary>
    ///     Slovensky jazyk.
    /// </summary>
    [XmlEnum(Name = "SK")]
    Slovak,

    /// <summary>
    ///     Cesky jazyk.
    /// </summary>
    [XmlEnum(Name = "CZ")] 
    Czech
}

public enum StartupType
{
    /// <summary>
    ///     Otvori hlavne prazdne okno.
    /// </summary>
    [XmlEnum(Name = "EmptyWindow")]
    EmptyWindow,

    /// <summary>
    ///     Otvori hlavne okno s posledne otvorenym projektom.
    /// </summary>
    [XmlEnum(Name = "LastProject")]
    LastProject
}