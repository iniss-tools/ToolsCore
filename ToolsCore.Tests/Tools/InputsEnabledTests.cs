using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Zakazanie vstupov tabulky podrobnosti: popisy ostavaju povolene (zakazane by boli v tmavej scheme necitatelne), vstupy aj
/// vo vnorenych paneloch sa zakazu a znova povolia.
/// </summary>
[TestClass]
public class InputsEnabledTests
{
    [TestMethod]
    public void SetInputsEnabled_PopisyOstanuVstupyAjVnoreneSaZakazu()
    {
        using var table = new TableLayoutPanel();
        var label = new Label();
        var link = new LinkLabel();
        var text = new TextBox();
        var flow = new FlowLayoutPanel();
        var button = new Button();
        flow.Controls.Add(button);
        table.Controls.AddRange([label, link, text, flow]);

        FormUtils.SetInputsEnabled(table, false);

        Assert.IsTrue(label.Enabled && flow.Enabled, "popis a vnoreny panel povolene");
        Assert.IsFalse(link.Enabled || text.Enabled || button.Enabled, "odkaz a vstupy zakazane");

        FormUtils.SetInputsEnabled(table, true);

        Assert.IsTrue(link.Enabled && text.Enabled && button.Enabled);
    }
}
