using ToolsCore.Forms;
using ToolsCore.Properties;

namespace ToolsCore.XML;

/// <summary>
/// Polozky farebnej schemy ovladacich prvkov - spolocne pre vsetky programy.
/// </summary>
public static class UiColors
{
    private static readonly Color DarkPanel = Color.FromArgb(30, 30, 30);
    private static readonly Color DarkButton = Color.FromArgb(51, 51, 55);

    /// <summary>
    /// Kategoria Ovladacie prvky.
    /// </summary>
    public static ColorCategory Category { get; } = new("Ui", () => Resources.ColorScheme_Controls, () => new UiColorPreview());

    /// <summary>Panel a pozadie okien.</summary>
    public static ColorKey Panel { get; } = new("Ui.Panel", Category, () => GlobalResources.NameColorSettings_Panel,
        new ColorValue(SystemColors.ControlText, SystemColors.Control), new ColorValue(Color.White, DarkPanel),
        ColorParts.ForeBack, s => s.ControlsColorScheme.Panel, "ControlsColorScheme/Panel");

    /// <summary>Stitky.</summary>
    public static ColorKey Label { get; } = new("Ui.Label", Category, () => GlobalResources.NameColorSettings_Label,
        new ColorValue(SystemColors.ControlText, SystemColors.Control), new ColorValue(Color.White, DarkButton),
        ColorParts.ForeBack, s => s.ControlsColorScheme.Label, "ControlsColorScheme/Label");

    /// <summary>Tlacidla.</summary>
    public static ColorKey Button { get; } = new("Ui.Button", Category, () => GlobalResources.NameColorSettings_Button,
        new ColorValue(SystemColors.ControlText, SystemColors.ButtonFace), new ColorValue(Color.White, DarkButton),
        ColorParts.ForeBack, s => s.ControlsColorScheme.Button, "ControlsColorScheme/Button");

    /// <summary>Boxy - polia, zoznamy a tabulky.</summary>
    public static ColorKey Box { get; } = new("Ui.Box", Category, () => GlobalResources.NameColorSettings_Box,
        new ColorValue(SystemColors.ControlText, Color.White), new ColorValue(Color.White, Color.FromArgb(37, 37, 38)),
        ColorParts.ForeBack, s => s.ControlsColorScheme.Box, "ControlsColorScheme/Box");

    /// <summary>Okraje prvkov.</summary>
    public static ColorKey Border { get; } = new("Ui.Border", Category, () => GlobalResources.NameColorSettings_Border,
        new ColorValue(SystemColors.WindowFrame, Color.Empty), new ColorValue(Color.FromArgb(62, 62, 66), Color.Empty),
        ColorParts.Fore, s => s.ControlsColorScheme.Border, "ControlsColorScheme/Border");

    /// <summary>Znacka v zaskrtavacich poliach a prepinacoch.</summary>
    public static ColorKey Mark { get; } = new("Ui.Mark", Category, () => GlobalResources.NameColorSettings_Mark,
        new ColorValue(SystemColors.ControlText, Color.Empty), new ColorValue(Color.FromArgb(165, 165, 165), Color.Empty),
        ColorParts.Fore, s => s.ControlsColorScheme.Mark, "ControlsColorScheme/Mark");

    /// <summary>Zvyraznenie vybranej polozky.</summary>
    public static ColorKey Highlight { get; } = new("Ui.Highlight", Category, () => GlobalResources.NameColorSettings_Highlight,
        new ColorValue(SystemColors.HighlightText, SystemColors.Highlight), new ColorValue(Color.White, Color.FromArgb(0, 120, 215)),
        ColorParts.ForeBack, s => s.ControlsColorScheme.Highlight, "ControlsColorScheme/Highlight");

    /// <summary>Stavovy riadok hlavneho okna (predvolene ako panel).</summary>
    public static ColorKey StatusBar { get; } = new("Ui.StatusBar", Category, () => GlobalResources.NameColorSettings_StatusBar,
        new ColorValue(SystemColors.ControlText, SystemColors.Control), new ColorValue(Color.White, DarkPanel),
        ColorParts.ForeBack, s => s.ControlsColorScheme.StatusBar, "ControlsColorScheme/StatusBar");

    /// <summary>
    /// Vsetky polozky v poradi stromu na stranke schemy.
    /// </summary>
    public static IReadOnlyList<ColorKey> Keys { get; } = [Panel, Label, Button, Box, Border, Mark, Highlight, StatusBar];
}
