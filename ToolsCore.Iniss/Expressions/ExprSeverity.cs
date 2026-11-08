namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Zavaznost hlasenia.
/// </summary>
public enum ExprSeverity
{
    /// <summary>Informacia - vyraz je v poriadku, ale stoji za pozornost.</summary>
    Info,

    /// <summary>Varovanie - INISS vyraz prelozi, ale pravdepodobne nerobi to, co autor chcel.</summary>
    Warning,

    /// <summary>Chyba - INISS vyraz neprelozi (hlasenie zodpoveda jeho textu).</summary>
    Error
}
