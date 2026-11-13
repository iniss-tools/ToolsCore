using ExControls;
using Serilog;

namespace ToolsCore.Tools;

/// <summary>
/// Ukotvitelne panely hlavneho okna (<see cref="ExDockManager" /> z ExControls): spravca s textami ponuky okna v jazyku
/// programu a obnovenie rozlozenia z nastaveni (<see cref="XML.ConfigBase.DockLayout" />). Temu a pisma dostanu aj skryte
/// a plavajuce panely cez <see cref="FormUtils.ApplyThemeAndFonts" />.
/// </summary>
public static class AppDock
{
    /// <summary>
    /// Vytvori spravcu panelov na celu plochu okna.
    /// </summary>
    public static ExDockManager Create() => new()
    {
        Name = "dockMain",
        Dock = DockStyle.Fill,
        Strings = new ExDockStrings
        {
            Hide = GlobalResources.Dock_Hide,
            AutoHide = GlobalResources.Dock_AutoHide,
            FloatWindow = GlobalResources.Dock_Float,
            Dock = GlobalResources.Dock_Dock,
            Options = GlobalResources.Dock_Options
        }
    };

    /// <summary>
    /// Obnovi ulozene rozlozenie; neplatne (napr. z inej verzie kniznice) sa zaloguje a ostane doterajsie.
    /// </summary>
    /// <param name="dock">spravca so vsetkymi panelmi</param>
    /// <param name="layout">ulozene rozlozenie; <see langword="null" /> = bez zmeny</param>
    public static void Restore(ExDockManager dock, string? layout)
    {
        ArgumentNullException.ThrowIfNull(dock);
        try
        {
            dock.LoadLayout(layout);
        }
        catch (FormatException e)
        {
            Log.Warning(e, "Rozlozenie panelov okna sa nepodarilo obnovit, ostava predvolene");
        }
    }
}
