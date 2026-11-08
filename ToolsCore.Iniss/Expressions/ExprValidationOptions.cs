namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Nastavenia semantickej kontroly.
/// </summary>
public sealed class ExprValidationOptions
{
    /// <summary>Kontext prekladu.</summary>
    public ExprContext Context { get; init; } = ExprContext.Condition;

    /// <summary>Symboly grafikonu; <see langword="null"/> vypne kontroly proti datam.</summary>
    public IExprSymbolProvider? Symbols { get; init; }

    /// <summary>Ci ide o podmienku (TabTab, AutoCondition) - zapina kontrolu konstantnej podmienky.</summary>
    public bool IsCondition { get; init; } = true;

    /// <summary>Ci hlasit funkcie zavisle od kontextu (<c>VYLUKAZDE</c>, <c>ZPOZDENI</c>).</summary>
    public bool ReportContextDependent { get; init; } = true;
}
