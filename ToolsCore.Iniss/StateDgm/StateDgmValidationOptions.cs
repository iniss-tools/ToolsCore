using ToolsCore.Iniss.Expressions;

namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Nastavenie kontroly.
/// </summary>
public sealed class StateDgmValidationOptions
{
    /// <summary>Typy hlaseni z lokalneho Categori.txt; null = ReportKey sa nekontroluje.</summary>
    public IReadOnlyCollection<string>? ReportKeys { get; init; }

    /// <summary>Symboly pre vyrazy (druhy vlakov, stanice…).</summary>
    public IExprSymbolProvider? Symbols { get; init; }

    /// <summary>Stanica prijima spravy GTN - kluc <c>Wait</c> ma zmysel; false = upozornit.</summary>
    public bool HasGtn { get; init; } = true;

    /// <summary>Hlasit aj informacie (slepe stavy).</summary>
    public bool ReportInfos { get; init; } = true;
}
