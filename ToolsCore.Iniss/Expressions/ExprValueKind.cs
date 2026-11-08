// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Druh hodnoty, ktoru funkcia vracia.
/// </summary>
public enum ExprValueKind
{
    /// <summary><c>0</c> alebo <c>1</c>.</summary>
    Bool,

    /// <summary>Cele cislo.</summary>
    Number,

    /// <summary>Bitova maska (napr. <c>PRIZNAK</c>).</summary>
    Mask,

    /// <summary>Pocet minut.</summary>
    Minutes,

    /// <summary>Pocet sekund od polnoci.</summary>
    TimeOfDay,

    /// <summary>OLE datum - pocet dni od 30. 12. 1899.</summary>
    Date,

    /// <summary>ID stanice.</summary>
    StationId,

    /// <summary>Index druhu vlaku (0-94).</summary>
    TrainType,

    /// <summary>Pozicia vlaku (1-3).</summary>
    Position,

    /// <summary>Cislo kategorie stavoveho diagramu.</summary>
    Category
}
