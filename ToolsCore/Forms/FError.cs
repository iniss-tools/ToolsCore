using System.Globalization;
using ToolsCore.Icons;
using ToolsCore.Tools;

namespace ToolsCore.Forms;

/// <summary>
/// Okno chyby: kratka sprava, na poziadanie podrobnosti (cely popis vynimky), kopirovanie do schranky a priecinok logov.
/// </summary>
public partial class FError : Form
{
    private readonly string _copyText;
    private readonly int _detailsHeight;

    private FError(string message, string? details)
    {
        InitializeComponent();
        this.ApplyThemeAndFonts();

        pbIcon.Image = IconImages.For(pbIcon, "status-error", IconImages.DialogSize);
        lTitle.Font = new Font(lTitle.Font, FontStyle.Bold);
        lMessage.Text = message;
        rtbDetails.Font = new Font("Consolas", 9.75f);
        rtbDetails.Text = details ?? "";
        llDetails.Visible = details is not null;
        _copyText = string.Format(CultureInfo.InvariantCulture, "{0} {1}\r\n{2:yyyy-MM-dd HH:mm:ss}\r\n\r\n{3}",
            Application.ProductName, Application.ProductVersion, DateTime.Now, details ?? message);

        // podrobnosti su na zaciatku skryte - okno ma len hlavicku a tlacidla
        _detailsHeight = panelDetails.Height;
        panelDetails.Visible = false;
        ClientSize = ClientSize with { Height = ClientSize.Height - _detailsHeight };
        MinimumSize = MinimumSize with { Height = Height };
    }

    private void llDetails_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        var show = !panelDetails.Visible;
        panelDetails.Visible = show;
        llDetails.Text = show ? Properties.Resources.Error_HideDetails : Properties.Resources.Error_ShowDetails;
        Height += show ? _detailsHeight : -_detailsHeight;
    }

    private void bCopy_Click(object? sender, EventArgs e) => Clipboard.SetText(_copyText);

    private void bLogs_Click(object? sender, EventArgs e) => ErrorReport.OpenLogFolder();

    /// <summary>
    /// Ukaze spravu o chybe bez podrobnosti.
    /// </summary>
    public static void ShowError(string message, IWin32Window? owner = null)
    {
        using var dialog = new FError(message, null);
        dialog.ShowDialog(owner);
    }

    /// <summary>
    /// Ukaze vynimku: jej spravu a v podrobnostiach cely popis vratane zasobnika volani.
    /// </summary>
    public static void ShowException(Exception exception, IWin32Window? owner = null)
    {
        using var dialog = new FError(exception.Message, exception.ToString());
        dialog.ShowDialog(owner);
    }
}
