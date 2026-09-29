using System.Diagnostics.CodeAnalysis;
using ExControls;

namespace ToolsCore.Tools;

/// <summary>
/// Dialogy s hlasenim - vzdy cez ExMessageBox (tmavy rezim), nikdy MessageBox.Show.
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// Zobrazi chybovu hlasku s tlacidlom OK a ikonou cerveneho kriza.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialogu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static void ShowError([Localizable(true)] string text, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        ExMessageBox.Show(text, GlobalResources.RError, buttons, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Zobrazi dialog s otazkou a ikonou bieleho otaznika v modrom kruhu.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialogu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static DialogResult ShowQuestion([Localizable(true)] string text, MessageBoxButtons buttons = MessageBoxButtons.YesNo)
    {
        return ExMessageBox.Show(text, GlobalResources.RQuestion, buttons, MessageBoxIcon.Question);
    }

    /// <summary>
    /// Zobrazi dialog s varovanim a ikonou cierneho vykricnika v zltom trojuholniku.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialogu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static DialogResult ShowWarning([Localizable(true)] string text, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        return ExMessageBox.Show(text, GlobalResources.RWarning, buttons, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Zobrazí dialog s informaciou a ikonou bieleho pismena 'i' v modrom kruhu.
    /// </summary>
    /// <param name="text">Text spravy.</param>
    /// <param name="title">Titulok dialogu.</param>
    /// <param name="buttons">Tlacidla, ktore sa zobrazia v dialógu.</param>
    /// <returns>vysledok dialogu.</returns>
    [ExcludeFromCodeCoverage]
    public static DialogResult ShowInfo([Localizable(true)] string text, string? title = null, MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        return ExMessageBox.Show(text, title ?? GlobalResources.RInfo, buttons, MessageBoxIcon.Information);
    }
}
