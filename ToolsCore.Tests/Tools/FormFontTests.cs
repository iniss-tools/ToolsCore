using ToolsCore.Tools;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Pismo okna (<see cref="FormUtils.SetFormFont" />): okno s pevnou velkostou sa zvacsi podla obsahu, okno s menitelnou
/// velkostou sa da zmensit az po <see cref="Form.MinimumSize" /> - AutoSize (GrowOnly) by ho drzal na velkosti obsahu.
/// </summary>
[STATestClass]
public class FormFontTests
{
    [TestMethod]
    public void PismoOkna_PevneOknoAutoSizeMenitelneSaDaZmensit()
    {
        using var dialog = new Form { FormBorderStyle = FormBorderStyle.FixedDialog };
        using var sizable = new Form { FormBorderStyle = FormBorderStyle.Sizable, MinimumSize = new Size(200, 150), ClientSize = new Size(400, 300) };
        sizable.Controls.Add(new Button { Location = new Point(300, 260), Size = new Size(80, 25), Anchor = AnchorStyles.Bottom | AnchorStyles.Right });

        dialog.SetFormFont();
        sizable.SetFormFont();
        sizable.Size = sizable.MinimumSize;

        Assert.IsTrue(dialog.AutoSize);
        Assert.IsFalse(sizable.AutoSize);
        Assert.AreEqual(sizable.MinimumSize, sizable.Size);
    }
}
