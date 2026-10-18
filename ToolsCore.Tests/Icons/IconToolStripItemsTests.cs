using System.ComponentModel;
using ToolsCore.Converters;
using ToolsCore.Icons;

namespace ToolsCore.Tests.Icons;

/// <summary>
/// Polozky panela nastrojov s ikonou z banky: velkost podla panela, variant podla pozadia a ukladanie v navrhari.
/// </summary>
[TestClass]
public class IconToolStripItemsTests
{
    [TestMethod]
    public void IconName_IkonaVoVelkostiObrazkovPanela()
    {
        using var strip = new ToolStrip { ImageScalingSize = new Size(20, 20) };
        var button = new IconToolStripButton { IconName = "save" };
        strip.Items.Add(button);

        Assert.AreEqual(new Size(20, 20), button.Image!.Size);

        strip.ImageScalingSize = new Size(24, 24);
        Assert.AreEqual(new Size(24, 24), button.Image!.Size);
    }

    [TestMethod]
    public void IconName_RozlozeniePocitaSMiestomNaIkonu()
    {
        using var strip = new ToolStrip { ImageScalingSize = new Size(20, 20) };
        var icon = new IconToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image };
        using var bitmap = new Bitmap(20, 20);
        var plain = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, Image = bitmap, Padding = icon.Padding };
        strip.Items.AddRange([icon, plain]);

        icon.IconName = "save";

        Assert.AreEqual(plain.GetPreferredSize(Size.Empty), icon.GetPreferredSize(Size.Empty));
    }

    [TestMethod]
    public void IconName_TmavePozadie_TmavyVariant()
    {
        using var strip = new ToolStrip { ImageScalingSize = new Size(16, 16) };
        var button = new IconToolStripButton { IconName = "save" };
        strip.Items.Add(button);
        var light = button.Image;

        strip.BackColor = Color.FromArgb(43, 45, 48);

        Assert.AreSame(AppIcons.Get("save", null, 16, true), button.Image);
        Assert.AreNotSame(light, button.Image);
    }

    [TestMethod]
    public void IconBadge_IkonaSOdznakom()
    {
        using var strip = new ToolStrip { ImageScalingSize = new Size(16, 16) };
        var item = new IconToolStripMenuItem { IconName = "train", IconBadge = "add" };
        strip.Items.Add(item);

        Assert.AreSame(AppIcons.Get("train", "add", 16, false), item.Image);
    }

    [TestMethod]
    public void Image_SIkonou_NastavenieSaIgnoruje()
    {
        using var strip = new ToolStrip();
        var button = new IconToolStripSplitButton { IconName = "undo" };
        strip.Items.Add(button);
        var icon = button.Image;
        using var bitmap = new Bitmap(16, 16);

        // napr. stary obrazok z .resx po ApplyResources
        button.Image = bitmap;

        Assert.AreSame(icon, button.Image);
    }

    [TestMethod]
    public void Navrhar_SIkonouUkladaNazovANieObrazok()
    {
        var button = new IconToolStripDropDownButton { IconName = "folder", IconBadge = "add" };
        var properties = TypeDescriptor.GetProperties(button);

        Assert.IsFalse(properties[nameof(ToolStripItem.Image)]!.ShouldSerializeValue(button));
        Assert.IsTrue(properties[nameof(IconToolStripDropDownButton.IconName)]!.ShouldSerializeValue(button));
        Assert.IsTrue(properties[nameof(IconToolStripDropDownButton.IconBadge)]!.ShouldSerializeValue(button));
    }

    [TestMethod]
    public void Navrhar_BezIkony_UkladaObrazok()
    {
        using var bitmap = new Bitmap(16, 16);
        var button = new IconToolStripButton { Image = bitmap };
        var properties = TypeDescriptor.GetProperties(button);

        Assert.IsTrue(properties[nameof(ToolStripItem.Image)]!.ShouldSerializeValue(button));
        Assert.IsFalse(properties[nameof(IconToolStripButton.IconName)]!.ShouldSerializeValue(button));
    }

    [TestMethod]
    public void Navrhar_PonukaNazvovIkonAOdznakov()
    {
        var button = new IconToolStripButton();
        var properties = TypeDescriptor.GetProperties(button);

        var names = properties[nameof(IconToolStripButton.IconName)]!.Converter.GetStandardValues()!.Cast<string>().ToList();
        var badges = properties[nameof(IconToolStripButton.IconBadge)]!.Converter.GetStandardValues()!.Cast<string>().ToList();

        Assert.IsInstanceOfType<IconNameConverter>(properties[nameof(IconToolStripButton.IconName)]!.Converter);
        CollectionAssert.AreEqual(AppIcons.Names.ToList(), names);
        CollectionAssert.AreEqual(AppIcons.Badges.ToList(), badges);
    }

    [TestMethod]
    public void Tlacidla_VacsiPredvolenyOkraj_PolozkaPonukyNie()
    {
        var button = new IconToolStripSplitButton();

        Assert.AreEqual(new Padding(ItemIcon.ButtonPaddingSize), button.Padding);
        Assert.IsFalse(TypeDescriptor.GetProperties(button)[nameof(ToolStripItem.Padding)]!.ShouldSerializeValue(button));
        Assert.AreEqual(new ToolStripMenuItem().Padding, new IconToolStripMenuItem().Padding);
    }

    [TestMethod]
    public void StavovyPopis_IkonaPodlaNazvuANavrharNeukladaObrazok()
    {
        using var strip = new StatusStrip { ImageScalingSize = new Size(16, 16) };
        var label = new IconToolStripStatusLabel { IconName = "status-ok" };
        strip.Items.Add(label);

        label.IconName = "status-error";

        Assert.AreSame(AppIcons.Get("status-error", null, 16, false), label.Image);
        Assert.IsFalse(TypeDescriptor.GetProperties(label)[nameof(ToolStripItem.Image)]!.ShouldSerializeValue(label));
    }

    [TestMethod]
    public void IconName_PrazdnyNazov_OdstraniIkonu()
    {
        using var strip = new ToolStrip();
        var button = new IconToolStripButton { IconName = "save" };
        strip.Items.Add(button);

        button.IconName = "";

        Assert.IsNull(button.IconName);
        Assert.IsNull(button.Image);
    }
}
