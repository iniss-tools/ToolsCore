using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
///     Obsahuje zoznam všetkých nastaviteľných komponentov, pre ktoré sa nastavuje ich písmo.
/// </summary>
[DefaultProperty(nameof(Labels))]
public record ControlFonts()
{
    private const string FontsCategory = "Písma";

    private static AppFont DefaultLabelsFont { get; } = new(SystemFonts.DefaultFont);
    private static AppFont DefaultButtonsFont { get; } = new(SystemFonts.DefaultFont);
    private static AppFont DefaultMenuFont { get; } = new(SystemFonts.MenuFont!);
    private static AppFont DefaultColsHeaderFont { get; } = new(SystemFonts.MenuFont!);
    private static AppFont DefaultTableCellsFont { get; } = new(SystemFonts.DefaultFont);
    private static AppFont DefaultStateRowFont { get; } = new(SystemFonts.MenuFont!);

    /// <summary>
    ///     Nastavenie písma pre Labels.
    /// </summary>
    [XmlElement("Labels")]
    [DisplayName("Text vo formulároch")]
    [Category(FontsCategory)]
    public AppFont Labels { get; set => field = OrDefault(value, DefaultLabelsFont); } = DefaultLabelsFont;

    private bool ShouldSerializeLabels() => !Equals(Labels.Font, DefaultLabelsFont.Font);

    /// <summary>
    ///     Nastavenie písma pre Buttons.
    /// </summary>
    [XmlElement("Buttons")] 
    [DisplayName("Tlačidlá formulárov")]
    [Category(FontsCategory)]
    public AppFont Buttons { get; set => field = OrDefault(value, DefaultButtonsFont); } = DefaultButtonsFont;

    private bool ShouldSerializeButtons() => !Equals(Buttons.Font, DefaultButtonsFont.Font);

    /// <summary>
    ///     Nastavenie písma pre Menu.
    /// </summary>
    [XmlElement("Menu")]
    [DisplayName("Menu")]
    [Category(FontsCategory)]
    public AppFont Menu { get; set => field = OrDefault(value, DefaultMenuFont); } = DefaultMenuFont;

    private bool ShouldSerializeMenu() => !Equals(Menu.Font, DefaultMenuFont.Font);

    /// <summary>
    ///     Nastavenie písma pre hlavičku śtĺpcov v DataGridView.
    /// </summary>
    [XmlElement("ColsHeaders")]
    [DisplayName("Hlavičky tabuliek")]
    [Category(FontsCategory)]
    public AppFont ColsHeader { get; set => field = OrDefault(value, DefaultColsHeaderFont); } = DefaultColsHeaderFont;

    private bool ShouldSerializeColsHeader() => !Equals(ColsHeader.Font, DefaultColsHeaderFont.Font);

    /// <summary>
    ///     Nastavenie písma pre obsah v DataGridView.
    /// </summary>
    [XmlElement("TableCells")]
    [DisplayName("Bunky tabuliek")]
    [Category(FontsCategory)]
    public AppFont TableCells { get; set => field = OrDefault(value, DefaultTableCellsFont); } = DefaultTableCellsFont;

    private bool ShouldSerializeTableCells() => !Equals(TableCells.Font, DefaultTableCellsFont.Font);

    /// <summary>
    ///     Nastavenie písma pre stavový riadok v dolnej časti pracovnej plochy programu.
    /// </summary>
    [XmlElement("StateRow")]
    [DisplayName("Stavový riadok")]
    [Category(FontsCategory)]
    public AppFont StateRow { get; set => field = OrDefault(value, DefaultStateRowFont); } = DefaultStateRowFont;

    private bool ShouldSerializeStateRow() => !Equals(StateRow.Font, DefaultStateRowFont.Font);

    /// <summary>
    ///     Prázdne písmo (napr. po vymazaní textu v PropertyGrid, alebo chýbajúce v súbore) nahradí predvoleným
    ///     písmom položky - null by inak zlyhal pri ukladaní konfigurácie aj pri kopírovaní nastavení.
    /// </summary>
    private static AppFont OrDefault(AppFont? value, AppFont defaultFont) => value?.Font is null ? defaultFont : value;

    /// <summary>
    ///     Vráti zoznam všetkých nastaviteľných komponentov, pre ktoré sa nastavuje ich písmo.
    /// </summary>
    /// <returns>zoznam komponentov.</returns>
    public List<AppFont> GetValues() => new() { Labels, Buttons, Menu, ColsHeader, TableCells, StateRow };

    protected ControlFonts(ControlFonts orig)
    {
        Labels = orig.Labels with { };
        Buttons = orig.Buttons with { };
        Menu = orig.Menu with { };
        ColsHeader = orig.ColsHeader with { };
        TableCells = orig.TableCells with { };
        StateRow = orig.StateRow with { };
    }
}