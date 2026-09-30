using ToolsCore.Tools;

namespace ToolsCore.Forms;

public partial class FAboutApp : Form
{
    public FAboutApp(string textAbout, Image? icon = null)
    {
        InitializeComponent();
        this.ApplyThemeAndFonts();

        lAppName.Text = Application.ProductName;
        lAppVersion.Text = Application.ProductVersion;
        lText.Text = textAbout;
        if (icon != null)
            picIcon.Image = icon;
        lAppName.Font = new Font(lAppName.Font, FontStyle.Bold);

        linkWeb.Text = LinkConsts.LinkInfoApp;
        linkEmail.Text = LinkConsts.Email;
    }

    private void OnWebLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Utils.OpenShell(LinkConsts.LinkInfoApp);

    private void OnEmailLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Utils.OpenShell("mailto:" + LinkConsts.Email);

    private void OnHelpButtonClicked(object sender, CancelEventArgs e) => Utils.OpenShell(LinkConsts.LinkInfoApp);
}