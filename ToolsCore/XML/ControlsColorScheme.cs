namespace ToolsCore.XML;

/// <summary>
/// Farby ovladacich prvkov (polozky <see cref="UiColors" />).
/// </summary>
public sealed class ControlsColorScheme
{
    /// <summary>Tlacidla.</summary>
    public ColorSetting Button { get; } = new();

    /// <summary>Stitky.</summary>
    public ColorSetting Label { get; } = new();

    /// <summary>Boxy - ComboBox, ListBox, TextBox, tabulky.</summary>
    public ColorSetting Box { get; } = new();

    /// <summary>Okraje prvkov (len popredie).</summary>
    public ColorSetting Border { get; } = new();

    /// <summary>Panel a pozadie okien.</summary>
    public ColorSetting Panel { get; } = new();

    /// <summary>Znacka v RadioButton a CheckBox (len popredie).</summary>
    public ColorSetting Mark { get; } = new();

    /// <summary>Zvyraznenie vybranej polozky a aktivneho prvku.</summary>
    public ColorSetting Highlight { get; } = new();

    /// <summary>Stavovy riadok hlavneho okna.</summary>
    public ColorSetting StatusBar { get; } = new();
}
