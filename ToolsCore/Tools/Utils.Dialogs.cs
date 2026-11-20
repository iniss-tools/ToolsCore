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

    /// <summary>
    /// Zobrazi dialog suboru alebo priecinka modalne k oknu <paramref name="owner" /> na vlastnom vlakne STA. Dialog
    /// Windows zruseny kratko po prvom otvoreni v procese caka na dokoncenie inicializacie shellu az 6 s, hoci jeho okno
    /// hned zmizne - okno programu by zatial nereagovalo. Takto je okno vlastnika po zatvoreni dialogu hned volne
    /// a vysledok pride, az ked dialog skonci. Kym predchadzajuci dialog v procese nedobehol, dalsi sa neotvori
    /// (vrati <see cref="DialogResult.Cancel" />) - opakovany klik by ho inak otvoril cez dobiehajuci.
    /// </summary>
    /// <param name="dialog">dialog (OpenFileDialog, SaveFileDialog, FolderBrowserDialog); kym uloha nedobehne, nemenit ho</param>
    /// <param name="owner">okno, ktore je pocas dialogu zakazane - formular, nie vnoreny prvok</param>
    /// <returns>
    /// vysledok dialogu; pokracovanie po await bezi znova na vlakne okna, vlastnik mohol byt medzitym zatvoreny
    /// (skontrolovat <c>IsDisposed</c>)
    /// </returns>
    [ExcludeFromCodeCoverage]
    public static Task<DialogResult> ShowDialogAsync(this CommonDialog dialog, IWin32Window owner)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        ArgumentNullException.ThrowIfNull(owner);

        // handle sa cita na vlakne okna - Control.Handle z ineho vlakna nie je dovoleny
        var handle = new WindowHandle(owner.Handle);
        if (Interlocked.Exchange(ref _fileDialogRunning, 1) == 1)
            return Task.FromResult(DialogResult.Cancel);

        var result = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                var dialogResult = dialog.ShowDialog(handle);
                Volatile.Write(ref _fileDialogRunning, 0);
                result.SetResult(dialogResult);
            }
            catch (Exception e)
            {
                Volatile.Write(ref _fileDialogRunning, 0);
                result.SetException(e);
            }
        })
        {
            IsBackground = true,
            Name = "FileDialog"
        };
        thread.SetApartmentState(ApartmentState.STA);
        try
        {
            thread.Start();
        }
        catch
        {
            Volatile.Write(ref _fileDialogRunning, 0);
            throw;
        }

        return result.Task;
    }

    /// <summary>1, kym dialog z <see cref="ShowDialogAsync" /> este bezi (aj po zmiznuti jeho okna).</summary>
    private static int _fileDialogRunning;

    /// <summary>Okno zadane len handlom (vlastnik dialogu na inom vlakne).</summary>
    private sealed class WindowHandle(IntPtr handle) : IWin32Window
    {
        public IntPtr Handle { get; } = handle;
    }
}
