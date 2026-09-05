using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
///     Trieda definujuca farby pre ovladacie prvky GUI.
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
    public string Name => "Ovládacie prvky";

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
    ///     Styl pre tlacidla.
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
    ///     Styl pre všetky štítky.
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
    ///     Styl pre boxy - ComboBox, ListBox....
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
    ///     Farba okrajov ovladacich prvkov (nastavovat iba ForeColor).
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
    ///     Styl panelu.
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
    ///     Farba značiek - pouzite ako značka vo vnutri RadioButton a CheckBox (nastavovat iba ForeColor).
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
    ///     Styl pre oznacenie prave aktivneho ovladacieho prvku resp. jeho casti.
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

    private static void AssignProperty(ref ColorSetting prop, string propname)
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

    // The backing fields (_button, _label, ...) are indirectly assigned here through the property
    // setters (which also apply AssignProperty's Name/DisableXxx side effect) - Roslyn's definite-assignment
    // analysis for record copy constructors doesn't credit assignment through a property setter, nor does
    // it apply the fields' own declaration-site initializers here (both run for the primary constructor only).
#pragma warning disable CS8618
    protected ControlsColorScheme(ControlsColorScheme original)
    {
        Button = original.Button with { };
        Label = original.Label with { };
        Box = original.Box with { };
        Border = original.Border with { };
        Panel = original.Panel with { };
        Mark = original.Mark with { };
        Highlight = original.Highlight with { };
    }
#pragma warning restore CS8618

    #endregion
}