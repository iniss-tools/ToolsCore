using System.Windows.Forms.Design;
using ToolsCore.Converters;

namespace ToolsCore.Icons;

/// <summary>
/// Rozbalovacie tlacidlo panela nastrojov s ikonou z banky <see cref="AppIcons" />.
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
[DesignerCategory("Code")]
public class IconToolStripDropDownButton : ToolStripDropDownButton
{
    private ItemIcon? _icon;

    // lenivo - konstruktor ToolStripItem nastavuje Image skor, nez by sa pole priradilo v konstruktore
    private ItemIcon Icon => _icon ??= new ItemIcon(this, () => base.Image, image => base.Image = image);

    /// <inheritdoc />
    protected override Padding DefaultPadding => ItemIcon.ButtonPadding(Owner);

    /// <inheritdoc cref="ItemIcon.Name" />
    [Category("Appearance"), Description(ItemIcon.NameDescription), DefaultValue(null), TypeConverter(typeof(IconNameConverter))]
    public string? IconName { get => Icon.Name; set => Icon.Name = value; }

    /// <inheritdoc cref="ItemIcon.Badge" />
    [Category("Appearance"), Description(ItemIcon.BadgeDescription), DefaultValue(null), TypeConverter(typeof(IconBadgeConverter))]
    public string? IconBadge { get => Icon.Badge; set => Icon.Badge = value; }

    /// <inheritdoc />
    public override Image? Image { get => Icon.Image; set => Icon.Image = value; }

    private bool ShouldSerializeImage() => Icon.ShouldSerializeImage();
}
