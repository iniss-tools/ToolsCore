using System.Globalization;
using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Registry;

namespace ToolsCore.Iniss.Tables;

/// <summary>Linka k tabuliam (sekcia Driver*) s tabulami, ktore jej INISS priradi, a zisteniami.</summary>
/// <param name="Section">sekcia (Driver, Driver0…)</param>
/// <param name="Line">cislo komunikacnej linky alebo null (INISS ju berie ako linku 0)</param>
/// <param name="Class">trieda linky</param>
/// <param name="Port">komunikacny kanal (TablePort)</param>
/// <param name="Active">INISS linku zalozi (cislo v rozsahu a nepouzite skorsou linkou)</param>
/// <param name="Tables">tabule, ktore INISS na linku posiela</param>
/// <param name="Problems">zistenia</param>
public sealed record DriverLine(
    string Section, int? Line, int Class, string Port, bool Active, IReadOnlyList<LineTable> Tables, IReadOnlyList<DriverLineProblem> Problems)
{
    /// <summary>Kratky popis do stromu: <c>linka 3 · ELEN</c>.</summary>
    public string Summary => string.Format(CultureInfo.CurrentCulture, Resources.InissSettings_LineSummary,
        Line?.ToString(CultureInfo.CurrentCulture) ?? "?", DriverClasses.Find(Class)?.Name ?? Class.ToString(CultureInfo.CurrentCulture));
}
