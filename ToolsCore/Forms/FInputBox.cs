using System.Collections;
using ToolsCore.Tools;

namespace ToolsCore.Forms;

public partial class FInputBox : Form
{
    public string NewValue { get; private set; } = "";

    private readonly IEnumerable _listToCheck;
    private readonly Func<object, string, bool> _comparator;

    public FInputBox(IEnumerable listToCheck, Func<object, string, bool> comparator, string oldValue = "")
    {
        InitializeComponent();
        this.ApplyThemeAndFonts();

        tbValue.Text = oldValue;
        _listToCheck = listToCheck;
        _comparator = comparator;
    }

    private void BOK_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.None;

        var value = tbValue.Text;
        if (string.IsNullOrWhiteSpace(value))
        {
            Utils.ShowError("Hodnota nemôže byť prázdna.");
            return;
        }

        if (_listToCheck.Cast<object>().Any(item => _comparator(item, value)))
        {
            Utils.ShowError("Hodnota je neplatná.");
            return;
        }

        NewValue = value;
        DialogResult = DialogResult.OK;
    }
}