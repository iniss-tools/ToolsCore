using System.Reflection;
using ToolsCore.Tools;
using Application = System.Windows.Forms.Application;

namespace ToolsCore;

/// <summary>
///     Priečinky, do ktorých program zapisuje svoje údaje.
/// </summary>
/// <remarks>
///     Nastavenia a logy sa ukladajú do <c>%LocalAppData%\&lt;názov programu&gt;</c>, nie vedľa
///     programu. Priečinok s programom totiž pri inštalácii pre všetkých používateľov leží
///     v <c>Program Files</c>, kam bežný proces zapisovať nemôže - a keďže programy majú vlastný
///     manifest, nefunguje ani virtualizácia UAC do <c>VirtualStore</c>.
/// </remarks>
public static class AppPaths
{
    private static string? _dataDir;

    /// <summary>
    ///     Priečinok s údajmi programu: <c>%LocalAppData%\&lt;názov programu&gt;</c>.
    /// </summary>
    public static string DataDir => _dataDir ??= Utils.CombinePath(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName)!;

    /// <summary>
    ///     Priečinok s konfiguračnými súbormi.
    /// </summary>
    public static string ConfigDir => Utils.CombinePath(DataDir, FileConsts.CONFIG_PATH)!;

    /// <summary>
    ///     Názov programu, pod ktorým vznikne priečinok v <c>%LocalAppData%</c>.
    /// </summary>
    private static string AppName
    {
        get
        {
            var name = Assembly.GetEntryAssembly()?.GetName().Name;
            return string.IsNullOrEmpty(name) ? Application.ProductName! : name;
        }
    }
}
