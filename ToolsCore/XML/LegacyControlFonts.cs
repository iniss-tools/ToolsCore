using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Pisma v konfiguracii do verzie s <see cref="AppFonts" /> (prvok <c>ControlFonts</c>) - len na citanie.
/// </summary>
public sealed class LegacyControlFonts
{
    /// <summary>
    /// Pismo popisov formularov.
    /// </summary>
    [XmlElement("Labels")]
    public LegacyFont? Labels { get; set; }

    /// <summary>
    /// Pismo buniek tabuliek.
    /// </summary>
    [XmlElement("TableCells")]
    public LegacyFont? TableCells { get; set; }
}
