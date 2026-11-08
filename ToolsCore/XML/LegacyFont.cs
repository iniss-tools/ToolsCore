using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Jedno pismo starsej konfiguracie.
/// </summary>
public sealed class LegacyFont
{
    /// <summary>
    /// Pismo.
    /// </summary>
    [XmlElement("f")]
    public XmlFont? Font { get; set; }
}
