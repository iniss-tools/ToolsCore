// ReSharper disable StringLiteralTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Kontext, v ktorom sa vyraz preklada. Urcuje, ktore konstanty prekladac prijme.
/// </summary>
public enum ExprContext
{
    /// <summary>Podmienka v TabTab.txt alebo kluc StateDgm.txt okrem <c>Wait</c> (INISS mode 1).</summary>
    Condition,

    /// <summary>Kluc <c>Wait</c> v StateDgm.txt - navyse prijma konstanty udalosti GTN (INISS mode 2).</summary>
    StateDgmWait
}
