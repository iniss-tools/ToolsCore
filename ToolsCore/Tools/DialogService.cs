namespace ToolsCore.Tools;

/// <summary>
/// Dialogy cez <see cref="ExControls.ExMessageBox" /> (tmavy rezim) - predvolena implementacia.
/// </summary>
public sealed class DialogService : IDialogService
{
    /// <inheritdoc />
    public void ShowError(string text) => Utils.ShowError(text);

    /// <inheritdoc />
    public DialogResult ShowQuestion(string text, MessageBoxButtons buttons = MessageBoxButtons.YesNo) => Utils.ShowQuestion(text, buttons);

    /// <inheritdoc />
    public DialogResult ShowWarning(string text, MessageBoxButtons buttons = MessageBoxButtons.OK) => Utils.ShowWarning(text, buttons);

    /// <inheritdoc />
    public DialogResult ShowInfo(string text, string? title = null, MessageBoxButtons buttons = MessageBoxButtons.OK) =>
        Utils.ShowInfo(text, title, buttons);
}
