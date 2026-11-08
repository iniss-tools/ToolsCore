namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Kod hlasenia. Kody <c>Iniss*</c> zodpovedaju chybam prekladaca INISSu (RCIniss.dll 8001-8012),
/// ostatne su kontroly GVDEditora nad ramec INISSu.
/// </summary>
public enum ExprDiagnosticCode
{
    // --- chyby prekladaca INISSu ---
    InissUnterminatedString,
    InissNumberExpected,
    InissUnknownSymbol,
    InissExpected,
    InissUnexpected,
    InissBadDateFormat,
    InissBadTimeFormat,

    // --- semanticke kontroly GVDEditora ---
    /// <summary><c>Typ_X</c> sa nasiel len medzi zabudovanymi nazvami, nie v TrTypes.txt.</summary>
    TrainTypeNotInTrTypes,

    /// <summary><c>Typ_X</c> sa nasiel len bez ohladu na velkost pismen alebo diakritiku.</summary>
    TrainTypeInexactMatch,

    /// <summary>ID stanice v argumente neexistuje v grafikone.</summary>
    UnknownStation,

    /// <summary>Kolaj v argumente neexistuje v Pozice.txt.</summary>
    UnknownTrack,

    /// <summary>Dopravca v argumente neexistuje vo Vlastnik.txt.</summary>
    UnknownOperator,

    /// <summary>Porovnanie masky (<c>PRIZNAK(x) == 1</c>) - funkcia vracia masku, nie 0/1.</summary>
    MaskComparedToNumber,

    /// <summary>Delenie konstantnou nulou - INISS pri vyhodnoteni spadne.</summary>
    DivisionByZero,

    /// <summary>Spojky roznej priority bez zatvoriek (<c>a &amp;&amp; b || c</c>) - INISS viaze sprava.</summary>
    MixedLogicalOperators,

    /// <summary>Retazenie <c>-</c> alebo <c>/</c> bez zatvoriek - INISS viaze sprava.</summary>
    RightAssociativeArithmetic,

    /// <summary>Negacia pred porovnanim (<c>!a == b</c>) - plati na cele porovnanie.</summary>
    NegationOfComparison,

    /// <summary>Znamienko pred suctom (<c>-a + b</c>) - plati na cely sucet.</summary>
    SignOfSum,

    /// <summary>Cislo s veducou nulou - INISS ho cita osmickovo.</summary>
    OctalLiteral,

    /// <summary>Funkcia zavisla od kontextu (<c>VYLUKAZDE</c>, <c>ZPOZDENI</c>).</summary>
    ContextDependentFunction,

    /// <summary>Podmienka je konstanta (okrem <c>1</c>).</summary>
    ConstantCondition
}
