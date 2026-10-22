using System.Globalization;
using ToolsCore.Icons;
using ToolsCore.Iniss.Tools;
using ToolsCore.Iniss.Updates;
using ToolsCore.Properties;
using ToolsCore.Tools;

namespace ToolsCore.Updates;

/// <summary>Co pouzivatel zvolil v okne novej verzie.</summary>
public enum UpdateChoice
{
    /// <summary>Neskor - pri dalsom spusteni sa program opyta znova.</summary>
    Later,

    /// <summary>Na tuto verziu pri spusteni neupozornovat.</summary>
    Skip,

    /// <summary>Instalator je stiahnuty v <see cref="FUpdateApp.InstallerPath" />, program sa ma zavriet a spustit ho.</summary>
    Install
}

/// <summary>
/// Okno novej verzie programu: verzie, co je nove a volby Aktualizovat (stiahne instalator s kontrolou suctu),
/// Preskocit tuto verziu a Neskor. Program nainstalovany inak nez instalatorom dostane namiesto aktualizacie stranku vydania.
/// </summary>
public partial class FUpdateApp : Form
{
    private readonly UpdateRelease _release;
    private readonly HttpClient _http;
    private readonly bool _canInstall;
    private readonly string _downloadFolder;
    private CancellationTokenSource? _download;

    public FUpdateApp()
    {
        InitializeComponent();
        _release = new UpdateRelease();
        _http = null!;
        _downloadFolder = "";
    }

    internal FUpdateApp(UpdateSource source, UpdateRelease release, HttpClient http, bool canInstall, string downloadFolder) : this()
    {
        _release = release;
        _http = http;
        _canInstall = canInstall && release.Installer is not null;
        _downloadFolder = downloadFolder;
        this.ApplyThemeAndFonts();

        pbIcon.Image = IconImages.For(pbIcon, "status-info", IconImages.DialogSize);
        lTitle.Font = new Font(lTitle.Font, FontStyle.Bold);
        lTitle.Text = string.Format(CultureInfo.CurrentCulture, Resources.Update_Available, source.Program);
        lVersions.Text = string.Format(CultureInfo.CurrentCulture, Resources.Update_Versions, release.Version, ReleaseVersion.Format(source.Version));
        rtbNotes.Text = string.IsNullOrWhiteSpace(release.Notes) ? Resources.Update_NoNotes : release.Notes.Trim();
        linkPage.Visible = IsWebPage(release.Page);
        bUpdate.Text = _canInstall ? Resources.Update_Install : Resources.Update_Download;
        bUpdate.Enabled = _canInstall || IsWebPage(release.Page);
    }

    /// <summary>Volba pouzivatela.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public UpdateChoice Choice { get; private set; } = UpdateChoice.Later;

    /// <summary>Stiahnuty a overeny instalator pri <see cref="UpdateChoice.Install" />.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? InstallerPath { get; private set; }

    private static bool IsWebPage(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

    private async void OnUpdateClick(object sender, EventArgs e)
    {
        if (!_canInstall)
        {
            Utils.OpenShell(_release.Page);
            Close();
            return;
        }

        var installer = _release.Installer!;
        var path = Path.Combine(_downloadFolder, Path.GetFileName(new Uri(installer.Url).LocalPath));
        bUpdate.Enabled = bSkip.Enabled = false;
        progress.Value = 0;
        progress.Visible = true;
        using var download = new CancellationTokenSource();
        _download = download;
        try
        {
            Directory.CreateDirectory(_downloadFolder);
            await UpdateChecker.DownloadAsync(_http, installer, path, new Progress<double>(p => progress.Value = (int) Math.Round(p * progress.Maximum)),
                download.Token);
            InstallerPath = path;
            Choice = UpdateChoice.Install;
            DialogResult = DialogResult.OK;
        }
        catch (OperationCanceledException) when (download.IsCancellationRequested)
        {
            // okno sa zavrelo pocas stahovania
        }
        catch (Exception ex) when (AppUpdates.IsUpdateError(ex))
        {
            Log.Exception(ex, "Stahovanie instalatora");
            if (IsDisposed)
                return;
            progress.Visible = false;
            bUpdate.Enabled = bSkip.Enabled = true;
            Utils.ShowError(string.Format(CultureInfo.CurrentCulture, Resources.Update_DownloadFailed, ex.Message));
        }
        finally
        {
            _download = null;
        }
    }

    private void OnSkipClick(object sender, EventArgs e)
    {
        Choice = UpdateChoice.Skip;
        Close();
    }

    private void OnLaterClick(object sender, EventArgs e) => Close();

    private void OnPageLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Utils.OpenShell(_release.Page);

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _download?.Cancel();
        base.OnFormClosing(e);
    }
}
