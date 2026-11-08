using System.Globalization;
using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Registry;

namespace ToolsCore.Iniss.Tables;

/// <summary>Zistenie o linke alebo tabuli s parametrami; text v jazyku UI skladaju zdroje kniznice.</summary>
/// <param name="Severity">zavaznost</param>
/// <param name="Code">druh</param>
/// <param name="Args">parametre textu (cisla v invariantnom tvare)</param>
public sealed record DriverLineProblem(RegSeverity Severity, DriverLineProblemCode Code, IReadOnlyList<string> Args)
{
    /// <summary>Text pre obsluhu.</summary>
    public string Text => Code switch
    {
        DriverLineProblemCode.LineOutOfRange => Format(Resources.InissSettings_LineOutOfRange),
        DriverLineProblemCode.LineDuplicate => Format(Resources.InissSettings_LineDuplicate),
        DriverLineProblemCode.WrongFamily => Format(Resources.InissSettings_LineWrongFamily),
        DriverLineProblemCode.NoTables => Resources.InissSettings_LineNoTables,
        DriverLineProblemCode.NoAutomaticLine => Resources.InissSettings_Unserved_NoAuto,
        _ => Format(Resources.InissSettings_Unserved_NoDriver)
    };

    private string Format(string text) => string.Format(CultureInfo.CurrentCulture, text, [.. Args]);

    internal static DriverLineProblem Of(RegSeverity severity, DriverLineProblemCode code, params object?[] args) =>
        new(severity, code, args.Select(a => Convert.ToString(a, CultureInfo.InvariantCulture) ?? "").ToList());
}
