using System.Xml.Serialization;
using ToolsCore.Properties;

namespace ToolsCore.XML;

/// <summary>
/// Trieda definujuca farby pre ovladacie prvky GUI.
/// </summary>
public record ControlsColorScheme() : IColorScheme
{
    /// <inheritdoc />
    [XmlIgnore] 
    public string Name => Resources.ColorScheme_Controls;
    
    [XmlIgnore]
    private static readonly Dictionary<string, ColorSetting> Props = new()
    {
        [nameof(Button)] = new(SystemColors.ButtonFace, SystemColors.ControlText, true) { NameSource = () => GlobalResources.NameColorSettings_Button },
        [nameof(Label)] = new(SystemColors.Control, SystemColors.ControlText, true) { NameSource = () => GlobalResources.NameColorSettings_Label },
        [nameof(Box)] = new(Color.White, SystemColors.ControlText, true) { NameSource = () => GlobalResources.NameColorSettings_Box },
        [nameof(Border)] = new(SystemColors.WindowFrame, true) { NameSource = () => GlobalResources.NameColorSettings_Border },
        [nameof(Panel)] = new(SystemColors.Control, SystemColors.ControlText, true) { NameSource = () => GlobalResources.NameColorSettings_Panel },
        [nameof(Mark)] = new(SystemColors.ControlText, true) { NameSource = () => GlobalResources.NameColorSettings_Mark },
        [nameof(Highlight)] = new(SystemColors.Highlight, SystemColors.HighlightText, true) { NameSource = () => GlobalResources.NameColorSettings_Highlight },
    };

    #region Properties

    /// <summary>
    /// Styl pre tlacidla.
    /// </summary>
    [XmlElement("Button")]
    public ColorSetting Button
    {
        get => field ??= InitProperty(nameof(Button));
        set
        {
            field = value;
            AssignProperty(ref field, nameof(Button));
        }
    } = InitProperty(nameof(Button));

    /// <summary>
    /// Styl pre všetky štítky.
    /// </summary>
    [XmlElement("Label")]
    public ColorSetting Label
    {
        get => field ??= InitProperty(nameof(Label));
        set
        {
            field = value;
            AssignProperty(ref field, nameof(Label));
        }
    } = InitProperty(nameof(Label));

    /// <summary>
    /// Styl pre boxy - ComboBox, ListBox....
    /// </summary>
    [XmlElement("Box")]
    public ColorSetting Box
    {
        get => field ??= InitProperty(nameof(Box));
        set
        {
            field = value;
            AssignProperty(ref field, nameof(Box));
        }
    } = InitProperty(nameof(Box));

    /// <summary>
    /// Farba okrajov ovladacich prvkov (nastavovat iba ForeColor).
    /// </summary>
    [XmlElement("Border")]
    public ColorSetting Border
    {
        get => field ??= InitProperty(nameof(Border));
        set
        {
            field = value;
            AssignProperty(ref field, nameof(Border));
        }
    } = InitProperty(nameof(Border));

    /// <summary>
    /// Styl panelu.
    /// </summary>
    [XmlElement("Panel")]
    public ColorSetting Panel
    {
        get => field ??= InitProperty(nameof(Panel));
        set
        {
            field = value;
            AssignProperty(ref field, nameof(Panel));
        }
    } = InitProperty(nameof(Panel));

    /// <summary>
    /// Farba značiek - pouzite ako značka vo vnutri RadioButton a CheckBox (nastavovat iba ForeColor).
    /// </summary>
    [XmlElement("Mark")]
    public ColorSetting Mark
    {
        get => field ??= InitProperty(nameof(Mark));
        set
        {
            field = value;
            AssignProperty(ref field, nameof(Mark));
        }
    } = InitProperty(nameof(Mark));

    /// <summary>
    /// Styl pre oznacenie prave aktivneho ovladacieho prvku resp. jeho casti.
    /// </summary>
    [XmlElement("Highlight")]
    public ColorSetting Highlight
    {
        get => field ??= InitProperty(nameof(Highlight));
        set
        {
            field = value;
            AssignProperty(ref field, nameof(Highlight));
        }
    } = InitProperty(nameof(Highlight));

    // stavovy riadok mal v starsich styloch farbu zvyraznenia (volba HighlightStatusBar); bez zaznamu v styles.xml
    // dostane farbu panela daneho stylu - preto sa odvodzuje az pri prvom pouziti, ked je panel nacitany
    private ColorSetting? _statusBar;

    /// <summary>
    /// Farby stavoveho riadka hlavneho okna. Predvolene ako panel stylu.
    /// </summary>
    [XmlElement("StatusBar")]
    public ColorSetting StatusBar
    {
        get => _statusBar ??= new ColorSetting(Panel.BackColor, Panel.ForeColor, true) { NameSource = StatusBarName };
        set
        {
            _statusBar = value;
            if (_statusBar is not null)
            {
                _statusBar.NameSource = StatusBarName;
                _statusBar.DisableBackColorEdit = false;
                _statusBar.DisableFontBoldEdit = true;
            }
        }
    }

    private static string StatusBarName() => GlobalResources.NameColorSettings_StatusBar;

    private static ColorSetting InitProperty(string propname) => Props[propname] with { };

    private static void AssignProperty(ref ColorSetting? prop, string propname)
    {
        if (prop is null)
            prop = InitProperty(propname);
        else
        {
            prop.NameSource = Props[propname].NameSource;
            prop.DisableBackColorEdit = Props[propname].DisableBackColorEdit;
            prop.DisableFontBoldEdit = Props[propname].DisableFontBoldEdit;
        }
    }

    // kopia farby uz ma nazov a priznaky z Props (priradil ich setter povodnej instancie) - staci kopirovat polia
    protected ControlsColorScheme(ControlsColorScheme original)
    {
        if (original.Button != null) Button = original.Button with { };
        if (original.Label != null) Label = original.Label with { };
        if (original.Box != null) Box = original.Box with { };
        if (original.Border != null) Border = original.Border with { };
        if (original.Panel != null) Panel = original.Panel with { };
        if (original.Mark != null) Mark = original.Mark with { };
        if (original.Highlight != null) Highlight = original.Highlight with { };
        if (original._statusBar != null) StatusBar = original._statusBar with { };
    }

    #endregion
}