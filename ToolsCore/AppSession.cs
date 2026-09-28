using ToolsCore.XML;

namespace ToolsCore;

/// <summary>
/// Nastavenia programu, ktore platia po cely jeho beh: konfiguracia, zoznam stylov a prave pouzivany styl.
/// </summary>
/// <remarks>Vytvara ju <see cref="AppInit.Initialization{TC,TS}" /> pri starte programu.</remarks>
/// <typeparam name="TConfig">Konfiguracia programu (config.xml).</typeparam>
/// <typeparam name="TStyle">Styl programu (styles.xml).</typeparam>
public sealed class AppSession<TConfig, TStyle>(TConfig config, Styles<TStyle> styles, TStyle usingStyle)
    where TConfig : ConfigBase where TStyle : Style
{
    /// <summary>
    /// Konfiguracia programu.
    /// </summary>
    public TConfig Config { get; set; } = config;

    /// <summary>
    /// Vsetky styly programu.
    /// </summary>
    public Styles<TStyle> Styles { get; set; } = styles;

    /// <summary>
    /// Prave pouzivany styl. Nastavi aj <see cref="GlobSettings.UsingStyle" />, podla ktoreho sa stylizuju
    /// okna a prvky z kniznic.
    /// </summary>
    public TStyle UsingStyle
    {
        get;
        set
        {
            field = value;
            GlobSettings.UsingStyle = value;
        }
    } = usingStyle;
}
