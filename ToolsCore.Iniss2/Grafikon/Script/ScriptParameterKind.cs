namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Co parameter vstavanej metody skriptu hlaseni ocakava (pre napovedu a kontrolu v editore).
/// </summary>
public enum ScriptParameterKind
{
    /// <summary>Skupina zvukov banky, napr. <c>"K1"</c>.</summary>
    Group,

    /// <summary>Logicka hodnota, napr. prichod (<c>True</c>) alebo odchod.</summary>
    Bool,

    /// <summary>Cele cislo, napr. limit meskania alebo cislo vlaku.</summary>
    Int,

    /// <summary>Cas v minutach alebo datum a cas vlaku, napr. <c>VlakS.CasPrij</c>.</summary>
    Time,

    /// <summary>Kolaj vlaku, napr. <c>VlakS.PozicePrijSkut</c>.</summary>
    Position,

    /// <summary>Zoznam cisel stanic, napr. stanice vzoru alebo <c>VlakS.StaniceDoSmeru(ModifikatorHlaseni)</c>.</summary>
    Stations,

    /// <summary>Text.</summary>
    Text,

    /// <summary>Funkcia skriptu (<c>AddressOf …</c>).</summary>
    Delegate
}
