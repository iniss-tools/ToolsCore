using ToolsCore.XML;

namespace ToolsCore;

/// <summary>
/// Obsahuje globalne (staticke) nastavenia pre program.
/// </summary>
public static class GlobSettings
{
    /// <summary>
    /// Prave pouzivany styl programu.
    /// </summary>
    public static Style UsingStyle { get; set; } = null!;

    /// <summary>
    /// Pisma programu (po ulozeni nastaveni ich tabulky prevezmu cez <see cref="Tools.FormUtils.RefreshFonts" />).
    /// </summary>
    public static AppFonts Fonts { get; set; } = new();
}
