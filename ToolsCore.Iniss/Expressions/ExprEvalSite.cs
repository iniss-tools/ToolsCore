namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Miesto, z ktoreho sa vyraz vyhodnocuje - rozhoduje o vysledku <c>VYLUKAZDE</c> a <c>ZPOZDENI</c>
/// (INISS: globalna premenna DAT_00559178).
/// </summary>
public enum ExprEvalSite
{
    /// <summary>Ine tabule a StateDgm (INISS -1): vyluka na prichode alebo odchode, vacsie z meskani.</summary>
    Default,

    /// <summary>Katalogova tabula, ktorej prvy stlpec je prichodovy (INISS 1): vyluka a meskanie prichodu.</summary>
    ArrivalTable,

    /// <summary>INISS 0, 2, 3 - vyluka a meskanie odchodu (v 3.39 sa nepouziva).</summary>
    DepartureTable
}
