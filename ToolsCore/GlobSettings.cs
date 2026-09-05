using ToolsCore.XML;

// ReSharper disable ClassNeverInstantiated.Global
namespace ToolsCore;

/// <summary>
///     Obsahuje globalne (staticke) nastavenia pre program.
/// </summary>
public class GlobSettings
{
    /// <summary>
    ///     URL odkaz na stranku, na ktorej mozu pouzivatelia najst odkaz na stiahnutie aktualizacie programu.
    /// </summary>
    public static string LinkUpdater = null!;

    /// <summary>
    ///     Prave pouzivany styl programu.
    /// </summary>
    public static Style UsingStyle = null!;

    /// <summary>
    ///     Pisma pre ovladacie prvky programu.
    /// </summary>
    public static ControlFonts Fonts = null!;
}