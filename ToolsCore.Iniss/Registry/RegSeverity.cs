namespace ToolsCore.Iniss.Registry;

/// <summary>Zavaznost diagnostiky.</summary>
public enum RegSeverity
{
    /// <summary>Informacia.</summary>
    Info,

    /// <summary>Upozornenie.</summary>
    Warning,

    /// <summary>Chyba - INISS bude pracovat inak, nez sa zrejme ocakava.</summary>
    Error
}
