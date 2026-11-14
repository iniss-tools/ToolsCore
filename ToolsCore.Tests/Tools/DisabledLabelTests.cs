using ToolsCore.Tools;
using ToolsCore.XML;

namespace ToolsCore.Tests.Tools;

/// <summary>
/// Zakazany popis v tmavej scheme: WinForms pocita farbu jeho textu z pozadia a na tmavom ho este stmavi. Popis bez
/// vlastneho pozadia preto dostane priehladne pozadie a zakazany text je sedy; vlastne pozadie popisu ostava a svetla
/// schema vrati povodne.
/// </summary>
[STATestClass]
public class DisabledLabelTests
{
    private static readonly Styles<Style> Schemes = Styles<Style>.CreateDefault();

    private static void Apply(Form form, Style style)
    {
        GlobSettings.UsingStyle = style;
        form.ApplyTheme();
    }

    [TestMethod]
    public void ZakazanyPopis_TmavaSchemaSedyText()
    {
        var previous = GlobSettings.UsingStyle;
        using var form = new Form { ClientSize = new Size(300, 100) };
        var label = new Label { Text = "WWWW", Font = new Font("Segoe UI", 20f, FontStyle.Bold), AutoSize = true, Enabled = false };
        form.Controls.Add(label);
        form.CreateControl();
        try
        {
            Apply(form, Schemes[StyleNames.Dark]);
            using var bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.ClientSize));

            var background = form.BackColor.GetBrightness();
            var lighter = 0;
            for (var x = label.Left; x < label.Right; x++)
            for (var y = label.Top; y < label.Bottom; y++)
            {
                if (bitmap.GetPixel(x, y).GetBrightness() > background + 0.3f)
                    lighter++;
            }

            Assert.IsGreaterThan(50, lighter, "text zakazaneho popisu je svetlejsi ako tmave pozadie");
        }
        finally
        {
            GlobSettings.UsingStyle = previous;
        }
    }

    [TestMethod]
    public void Popis_VlastnePozadieOstavaSvetlaSchemaVratiPovodne()
    {
        var previous = GlobSettings.UsingStyle;
        using var form = new Form();
        var plain = new Label { Text = "Popis" };
        var colored = new Label { Text = "Farba", BackColor = Color.Yellow };
        form.Controls.AddRange([plain, colored]);
        try
        {
            Apply(form, Schemes[StyleNames.Dark]);
            Assert.AreEqual((Color.Transparent, Color.Yellow), (plain.BackColor, colored.BackColor));

            Apply(form, Schemes[StyleNames.Light]);
            Assert.AreEqual(form.BackColor, plain.BackColor, "svetla schema: pozadie zdedene od okna");
            Assert.AreEqual(Color.Yellow, colored.BackColor);
        }
        finally
        {
            GlobSettings.UsingStyle = previous;
        }
    }
}
