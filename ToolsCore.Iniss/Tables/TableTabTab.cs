namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Definuje spravanie obsahu sekcie katalogovej tabule.
/// </summary>
/// <remarks>
/// Entita s identitou - stlpce katalogovych tabul sa odkazuju na instanciu (TAB1/TAB2), porovnava sa referenciou.
/// </remarks>
public sealed class TableTabTab
{
    /// <summary>
    /// Predvolený TabTab - žiadny.
    /// </summary>
    public static readonly TableTabTab Empty = new() { Key = "Žiadny", Text = "" };

    /// <summary>
    /// Kluc TabTab.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Obsah TabTab ako text.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <inheritdoc />
    public override string ToString() => Key;
}