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
    public bool DisableFontEdit => true;

    /// <inheritdoc />
    [XmlIgnore] 
    public Font Font { get; set; } = null!;

    /// <inheritdoc />
    [XmlIgnore] 
    public string Name => Resources.ColorScheme_Controls;

    [XmlIgnore]
    private static readonly Dictionary<string, ColorSetting> Props = new()
    {
        [nameof(Button)] = new(SystemColors.ButtonFace, SystemColors.ControlText, true) {Name = GlobalResources.NameColorSettings_Button},
        [nameof(Label)] = new(SystemColors.Control, SystemColors.ControlText, true) { Name = GlobalResources.NameColorSettings_Label },
        [nameof(Box)] = new(Color.White, SystemColors.ControlText, true) { Name = GlobalResources.NameColorSettings_Box },
        [nameof(Border)] = new(SystemColors.WindowFrame, true) { Name = GlobalResources.NameColorSettings_Border },
        [nameof(Panel)] = new(SystemColors.Control, SystemColors.ControlText, true) { Name = GlobalResources.NameColorSettings_Panel },
        [nameof(Mark)] = new(SystemColors.ControlText, true) { Name = GlobalResources.NameColorSettings_Mark },
        [nameof(Highlight)] = new(SystemColors.Highlight, SystemColors.HighlightText, true) { Name = GlobalResources.NameColorSettings_Highlight },
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

    private static ColorSetting InitProperty(string propname) => Props[propname] with { };

    private static void AssignProperty(ref ColorSetting? prop, string propname)
    {
        if (prop is null)
            InitProperty(propname);
        else
        {
            prop.Name = Props[propname].Name;
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
        Font = original.Font;
    }

    #endregion
}