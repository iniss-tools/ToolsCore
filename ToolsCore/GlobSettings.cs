using ToolsCore.XML;

namespace ToolsCore;

/// <summary>
///     Obsahuje globalne (staticke) nastavenia pre program.
/// </summary>
public static class GlobSettings
{
    /// <summary>
    ///     Prave pouzivany styl programu.
    /// </summary>
    public static Style UsingStyle { get; set; } = null!;

    /// <summary>
    ///     Pisma pre ovladacie prvky programu.
    /// </summary>
    public static ControlFonts Fonts { get; set; } = null!;
}
