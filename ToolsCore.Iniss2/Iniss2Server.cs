using System.ComponentModel;
using System.Diagnostics;
using Serilog;

namespace ToolsCore.Iniss2;

/// <summary>
/// Zistenie, ci server INISS2 bezi. Server drzi data v pamati a pri uprave z klienta subory prepise, zapisovat smie len
/// program pri zastavenom serveri. Server cita vzdy priecinok <c>Data</c> vedla svojho programu.
/// </summary>
public static class Iniss2Server
{
    /// <summary>
    /// Mutex, ktory server vytvori pri starte. Je v mennom priestore relacie (bez <c>Global\</c>), pri volbe
    /// <c>MoreInstance</c> v Config.xml ho server nevytvara a neurcuje, ktora instalacia bezi.
    /// </summary>
    public const string MutexName = "INISRV_MUTEX";

    /// <summary>
    /// Nazov programu servera v priecinku servera.
    /// </summary>
    public const string ExecutableName = "IniSrv.exe";

    // aj kopie programu v priecinku servera, napr. _IniSrv.exe
    private const string ExecutablePattern = "*IniSrv*.exe";

    /// <summary>
    /// Niektory server INISS2 v tejto relacii bezi - existuje jeho mutex.
    /// </summary>
    public static bool IsRunning()
    {
        if (!Mutex.TryOpenExisting(MutexName, out var mutex))
            return false;

        mutex.Dispose();
        return true;
    }

    /// <summary>
    /// Server tejto instalacie bezi: niektory program servera v jej priecinku je spusteny (aj z inej relacie, ako sluzba alebo
    /// pri volbe <c>MoreInstance</c>). Priecinok bez programu servera ziadny server necita.
    /// </summary>
    /// <param name="serverDir">priecinok servera</param>
    public static bool IsRunning(string serverDir)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverDir);

        if (!Directory.Exists(serverDir))
            return false;

        return Directory.EnumerateFiles(serverDir, ExecutablePattern).Any(executable => IsLocked(executable) ?? IsProcessRunning(executable));
    }

    // spusteny program Windows zamkne na zapis; null = zamok sa neda zistit (subor bez prava na zapis)
    private static bool? IsLocked(string executable)
    {
        try
        {
            using var stream = new FileStream(executable, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (IOException)
        {
            return true;
        }
    }

    // proces programu servera; ked cestu procesu nemozno precitat (ina relacia, ine prava), plati, ze bezi
    private static bool IsProcessRunning(string executable)
    {
        var name = Path.GetFileNameWithoutExtension(executable);
        foreach (var process in Process.GetProcessesByName(name))
        {
            using (process)
            {
                try
                {
                    if (string.Equals(process.MainModule?.FileName, executable, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch (Exception e) when (e is Win32Exception or InvalidOperationException)
                {
                    Log.Debug(e, "Cestu procesu {Process} sa nepodarilo zistit, server {Executable} sa berie ako beziaci", process.Id, executable);
                    return true;
                }
            }
        }

        return false;
    }
}
