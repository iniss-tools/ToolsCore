namespace ToolsCore.Iniss.Registry;

/// <summary>Stav hodnoty v jednej vrstve.</summary>
public enum RegLayerState
{
    /// <summary>Z tejto vrstvy INISS hodnotu berie.</summary>
    Used,

    /// <summary>Prebija ju vrstva s vyssou prioritou.</summary>
    Shadowed,

    /// <summary>Zly typ - INISS ju neuzna.</summary>
    WrongType,

    /// <summary>INISS tuto vrstvu pre nastavenie necita (iny koren registra, rezim spustenia).</summary>
    NotRead
}
