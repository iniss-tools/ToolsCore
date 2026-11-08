using System.Xml.Serialization;

namespace ToolsCore.XML;

public enum StartupType
{
    /// <summary>
    /// Otvori hlavne prazdne okno.
    /// </summary>
    [XmlEnum(Name = "EmptyWindow")]
    EmptyWindow,

    /// <summary>
    /// Otvori hlavne okno s posledne otvorenym projektom.
    /// </summary>
    [XmlEnum(Name = "LastProject")]
    LastProject
}
