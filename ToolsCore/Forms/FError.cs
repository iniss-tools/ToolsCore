using ToolsCore.Icons;
using ToolsCore.Tools;

namespace ToolsCore.Forms;

public partial class FError : Form
{
    private FError(string msg)
    {
        InitializeComponent();
        this.ApplyThemeAndFonts();

        pbIcon.Image = IconImages.For(pbIcon, "status-error", IconImages.DialogSize);
        rtbMessage.Text = msg;
        rtbMessage.Font = new Font("Consolas", 10);
    }

    private void bOK_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }

    public static void ShowError(string msg)
    {
        var dialog = new FError(msg);
        dialog.ShowDialog();
    }
}