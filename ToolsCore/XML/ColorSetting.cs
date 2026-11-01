namespace ToolsCore.XML;

/// <summary>
/// Farby polozky farebnej schemy, ako ich pouziva program (hodnoty nastavi <see cref="Style.Apply()" /> podla
/// zakladu schemy a zmien pouzivatela).
/// </summary>
public sealed class ColorSetting
{
    /// <summary>
    /// Farba pozadia.
    /// </summary>
    public Color BackColor { get; set; }

    /// <summary>
    /// Farba popredia.
    /// </summary>
    public Color ForeColor { get; set; }

    /// <summary>
    /// Pouzit tucne pismo.
    /// </summary>
    public bool Bold { get; set; }
}
