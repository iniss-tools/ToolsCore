using ToolsCore.Forms;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Tools;

/// <summary>
/// Hlasenie chyb pouzivatelovi: chyba sa vzdy zapise do logu a ukaze v okne chyby s podrobnostami.
/// </summary>
public static class ErrorReport
{
    /// <summary>
    /// Premenna prostredia, ktora pri ladeni vypne zachytavanie chyb (hodnota <c>1</c>) - program pri chybe spadne
    /// a debugger sa zastavi na mieste chyby.
    /// </summary>
    public const string DebugVariable = "INISSTOOLS_DEBUG";

    /// <summary>
    /// Ci sa chyby zachytavaju a ukazu pouzivatelovi; vypina ich len <see cref="DebugVariable" /> pri ladeni.
    /// </summary>
    public static bool CatchErrors { get; } = Environment.GetEnvironmentVariable(DebugVariable) != "1";

    /// <summary>
    /// Zapise chybu do logu a ukaze ju v okne chyby.
    /// </summary>
    public static void Show(Exception exception, IWin32Window? owner = null)
    {
        Log.Exception(exception);
        FError.ShowException(exception, owner);
    }

    /// <summary>
    /// Otvori priecinok s logmi programu v Prieskumnikovi (prikaz Pomocnika aj tlacidlo okna chyby).
    /// </summary>
    public static void OpenLogFolder()
    {
        Directory.CreateDirectory(Log.LogDirectory);
        Utils.OpenShell(Log.LogDirectory);
    }
}
