using System.Xml.Serialization;

namespace ToolsCore.XML;

/// <summary>
/// Jazyk aplikacie.
/// </summary>
public enum AppLanguage
{
    /// <summary>
    /// Slovensky jazyk.
    /// </summary>
    [XmlEnum(Name = "SK")]
    Slovak,

    /// <summary>
    /// Cesky jazyk.
    /// </summary>
    [XmlEnum(Name = "CZ")] 
    Czech
}
