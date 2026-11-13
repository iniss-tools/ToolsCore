using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Trieda reprezentujuca strukturu pre konfiguracny subor vo formate XML.
/// </summary>
[XmlRoot("CONFIG")]
public record ConfigBase()
{
    /// <summary>
    /// Zobrazit panel nastrojov hlavneho okna (ponuka je vzdy).
    /// </summary>
    [XmlElement("ShowToolbar"), DefaultValue(true)]
    public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Povodne nastavenie ponuky a panela nastrojov (<c>0</c> = oboje, <c>1</c> = len panel, <c>2</c> = len ponuka) -
    /// len na prevzatie do <see cref="ShowToolbar" /> zo starsej konfiguracie, nezapisuje sa.
    /// </summary>
    [XmlElement("DesktopMenu"), Browsable(false)]
    public string? LegacyDesktopMenu
    {
        get => null;
        set
        {
            if (value == "2")
                ShowToolbar = false;
        }
    }

    /// <summary>
    /// Nastavi jazyk pouzivatelskeho rozhrania (GUI).
    /// </summary>
    [XmlElement("Language"), DefaultValue(AppLanguage.Slovak)]
    public AppLanguage Language { get; set; } = AppLanguage.Slovak;

    /// <summary>
    /// Nastavi, co sa ma diat ihned po zapnuti programu.
    /// </summary>
    [XmlElement("Startup"), DefaultValue(StartupType.EmptyWindow)]
    public StartupType Startup { get; set; } = StartupType.EmptyWindow;

    /// <summary>
    /// Povolit alebo zakazat viacero instancii tohto programu.
    /// </summary>
    [XmlElement("MoreInstances"), DefaultValue(false)]
    public bool MoreInstance { get; set; }

    /// <summary>
    /// Pisma rozhrania, tabuliek a editora.
    /// </summary>
    [XmlElement("Fonts")]
    public AppFonts Fonts { get; set; } = new();

    /// <summary>
    /// Pisma starsej konfiguracie (pismo pre kazdy druh prvku) - len na prevzatie do <see cref="Fonts" />,
    /// nezapisuje sa.
    /// </summary>
    [XmlElement("ControlFonts"), Browsable(false)]
    public LegacyControlFonts? LegacyFonts
    {
        get => null;
        set
        {
            if (value is not null)
                Fonts.TakeLegacy(value);
        }
    }

    /// <summary>
    /// Klavesove skratky prikazov programu (zmenene oproti predvolenym).
    /// </summary>
    [XmlElement("Shortcuts")]
    public ShortcutMap Shortcuts { get; set; } = new();

    /// <summary>
    /// Zapamatane usporiadanie stlpcov tabuliek (poradie, viditelnost a sirka).
    /// </summary>
    [XmlArray("Grids"), XmlArrayItem("Grid")]
    public List<GridLayout> Grids { get; set; } = [];

    /// <summary>
    /// Usporiadanie stlpcov tabulky; <see langword="null" /> = predvolene z navrhu.
    /// </summary>
    public GridLayout? GridOf(string id) => Grids.FirstOrDefault(g => g.Id == id);

    /// <summary>
    /// Zapamata usporiadanie stlpcov tabulky; <see langword="null" /> = predvolene (zapamatane sa zmaze).
    /// </summary>
    public void SetGrid(string id, GridLayout? layout)
    {
        Grids.RemoveAll(g => g.Id == id);
        if (layout is not null)
            Grids.Add(layout);
    }

    /// <summary>
    /// Rozlozenie ukotvitelnych panelov hlavneho okna (<c>ExDockManager.SaveLayout</c>); <see langword="null" /> = predvolene.
    /// </summary>
    [XmlElement("DockLayout")]
    public string? DockLayout { get; set; }

    /// <summary>
    /// Vrati alebo nastavi, ci program pri spusteni zistuje novu verziu (najviac raz za den, len zostava z vydania).
    /// </summary>
    [XmlElement("CheckUpdates"), DefaultValue(true)]
    public bool CheckUpdates { get; set; } = true;

    [XmlIgnore]
    public virtual string? LinkAppSettingsGuide { get; }

    protected ConfigBase(ConfigBase original)
    {
        ShowToolbar = original.ShowToolbar;
        Startup = original.Startup;
        Language = original.Language;
        MoreInstance = original.MoreInstance;
        Fonts = original.Fonts with { };
        CheckUpdates = original.CheckUpdates;
        DockLayout = original.DockLayout;
        Shortcuts = original.Shortcuts.Clone();
        Grids = original.Grids.Select(g => g.Clone()).ToList();
    }
}
