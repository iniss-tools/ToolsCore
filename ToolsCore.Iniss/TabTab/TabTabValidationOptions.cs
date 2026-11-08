using ToolsCore.Iniss.Expressions;

namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Nastavenia kontroly sekcie.
/// </summary>
public sealed class TabTabValidationOptions
{
    /// <summary>Symboly grafikonu pre vyrazy a kolaje; <see langword="null"/> vypne kontroly proti datam.</summary>
    public IExprSymbolProvider? Symbols { get; init; }

    /// <summary>
    /// Mena stlpcov katalogovych tabul, ktore sekciu pouzivaju (pre <c>%meno%</c>);
    /// <see langword="null"/> kontrolu vypne.
    /// </summary>
    public IReadOnlyCollection<string>? ColumnNames { get; init; }

    /// <summary>Ci hlasit funkcie zavisle od kontextu (<c>VYLUKAZDE</c>, <c>ZPOZDENI</c>).</summary>
    public bool ReportContextDependent { get; init; }
}
