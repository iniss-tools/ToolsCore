namespace ToolsCore.Tools;

/// <summary>
/// Dialogy s hlasenim pre kod, ktory ich potrebuje nahradit (hlavne okno, sluzby, testy). Okna a stranky mozu
/// pouzivat priamo <see cref="Utils" />.ShowError/ShowQuestion/...; tam nahrada nema zmysel.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Chybova hlaska.
    /// </summary>
    void ShowError(string text);

    /// <summary>
    /// Otazka.
    /// </summary>
    /// <returns>vysledok dialogu</returns>
    DialogResult ShowQuestion(string text, MessageBoxButtons buttons = MessageBoxButtons.YesNo);

    /// <summary>
    /// Varovanie.
    /// </summary>
    /// <returns>vysledok dialogu</returns>
    DialogResult ShowWarning(string text, MessageBoxButtons buttons = MessageBoxButtons.OK);

    /// <summary>
    /// Informacia.
    /// </summary>
    /// <returns>vysledok dialogu</returns>
    DialogResult ShowInfo(string text, string? title = null, MessageBoxButtons buttons = MessageBoxButtons.OK);
}
