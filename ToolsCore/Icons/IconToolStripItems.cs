using System.Windows.Forms.Design;
using ToolsCore.Converters;

namespace ToolsCore.Icons;

// Polozky ponuk a panelov nastrojov s ikonou z banky AppIcons. Navrhar uklada len nazov ikony a odznaku (Image nie),
// ikona sa kresli v presnej velkosti ImageScalingSize panela a podla jasu pozadia v svetlom alebo tmavom variante.
// Tlacidla maju vacsi predvoleny okraj (ItemIcon.ButtonPaddingSize), polozka ponuky nie.
// Triedy maju rovnake cleny, spolocna cast je ItemIcon.

/// <summary>
/// Tlacidlo panela nastrojov s ikonou z banky <see cref="AppIcons" />.
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
[DesignerCategory("Code")]
public class IconToolStripButton : ToolStripButton
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

/// <summary>
/// Tlacidlo s rozbalovacou castou s ikonou z banky <see cref="AppIcons" />.
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
[DesignerCategory("Code")]
public class IconToolStripSplitButton : ToolStripSplitButton
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

/// <summary>
/// Polozka ponuky s ikonou z banky <see cref="AppIcons" />.
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
[DesignerCategory("Code")]
public class IconToolStripMenuItem : ToolStripMenuItem
{
    private ItemIcon? _icon;

    // lenivo - konstruktor ToolStripItem nastavuje Image skor, nez by sa pole priradilo v konstruktore
    private ItemIcon Icon => _icon ??= new ItemIcon(this, () => base.Image, image => base.Image = image);

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

/// <summary>
/// Popis panela nastrojov s ikonou z banky <see cref="AppIcons" />.
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
[DesignerCategory("Code")]
public class IconToolStripLabel : ToolStripLabel
{
    private ItemIcon? _icon;

    // lenivo - konstruktor ToolStripItem nastavuje Image skor, nez by sa pole priradilo v konstruktore
    private ItemIcon Icon => _icon ??= new ItemIcon(this, () => base.Image, image => base.Image = image);

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

/// <summary>
/// Popis stavoveho riadka s ikonou z banky <see cref="AppIcons" />.
/// </summary>
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip)]
[DesignerCategory("Code")]
public class IconToolStripStatusLabel : ToolStripStatusLabel
{
    private ItemIcon? _icon;

    // lenivo - konstruktor ToolStripItem nastavuje Image skor, nez by sa pole priradilo v konstruktore
    private ItemIcon Icon => _icon ??= new ItemIcon(this, () => base.Image, image => base.Image = image);

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

/// <summary>
/// Ikona polozky z banky <see cref="AppIcons" /> - spolocna cast tried <c>Icon*</c>.
/// </summary>
/// <remarks>
/// Rozlozenie panela (<c>ToolStripItem.PreferredImageSize</c>) cita ulozeny obrazok polozky, nie vlastnost <c>Image</c>:
/// polozka ma preto ulozenu ikonu z casu nastavenia a kresli sa vzdy ikona pre aktualnu velkost a pozadie.
/// Setter <c>Image</c> porovnava s getterom - ulozenie ho obchadza (<see cref="_storing" />).
/// </remarks>
/// <param name="item">polozka</param>
/// <param name="stored">ulozeny obrazok polozky (<c>base.Image</c>)</param>
/// <param name="store">ulozenie obrazka polozky</param>
internal sealed class ItemIcon(ToolStripItem item, Func<Image?> stored, Action<Image?> store)
{
    public const string NameDescription = "Icon from the AppIcons bank drawn in the exact size of the strip's ImageScalingSize, " +
                                          "in the dark variant on a dark background. Takes precedence over Image.";

    public const string BadgeDescription = "Badge drawn in the bottom right corner of the icon (e.g. add, edit, delete).";

    /// <summary>
    /// Okraj tlacidla panela nastrojov okolo ikony - vacsie tlacidla ako v Rideri (v bodoch pri 96 DPI).
    /// </summary>
    public const int ButtonPaddingSize = 2;

    private Bitmap? _image;
    private int _size;
    private bool _dark;
    private bool _storing;
    private string? _name;
    private string? _badge;

    /// <summary>
    /// Nazov ikony z <see cref="AppIcons.Names" />; ma prednost pred obrazkom polozky.
    /// </summary>
    public string? Name
    {
        get => _name;
        set
        {
            value = string.IsNullOrEmpty(value) ? null : value;
            if (_name == value)
                return;
            _name = value;
            Store();
        }
    }

    /// <summary>
    /// Nazov odznaku z <see cref="AppIcons.Badges" /> v pravom dolnom rohu ikony.
    /// </summary>
    public string? Badge
    {
        get => _badge;
        set
        {
            value = string.IsNullOrEmpty(value) ? null : value;
            if (_badge == value)
                return;
            _badge = value;
            Store();
        }
    }

    /// <summary>
    /// Obrazok polozky: s ikonou ikona pre aktualnu velkost a pozadie (nastavenie sa ignoruje), inak ulozeny obrazok.
    /// </summary>
    public Image? Image
    {
        get => _storing ? stored() : Resolve() ?? stored();
        set
        {
            if (Name is null)
                store(value);
        }
    }

    /// <summary>
    /// Predvoleny okraj tlacidla s ikonou v paneli nastrojov prepocitany podla DPI panela.
    /// </summary>
    public static Padding ButtonPadding(ToolStrip? owner) => new(owner?.LogicalToDeviceUnits(ButtonPaddingSize) ?? ButtonPaddingSize);

    /// <summary>
    /// Ci navrhar uklada obrazok polozky - s ikonou nie.
    /// </summary>
    public bool ShouldSerializeImage() => Name is null && stored() is not null && item.ImageIndex < 0 && string.IsNullOrEmpty(item.ImageKey);

    private Bitmap? Resolve()
    {
        if (Name is null)
            return null;

        var scaling = item.Owner?.ImageScalingSize ?? new Size(16, 16);
        var size = Math.Max(1, Math.Min(scaling.Width, scaling.Height));
        var dark = AppIcons.IsDarkBackground(Background());
        if (_image is null || size != _size || dark != _dark)
        {
            _image = AppIcons.Get(Name, Badge, size, dark);
            _size = size;
            _dark = dark;
        }

        return _image;
    }

    private void Store()
    {
        _image = null;
        var image = Resolve();
        _storing = true;
        try
        {
            store(image);
        }
        finally
        {
            _storing = false;
        }
    }

    private Color Background()
    {
        // pred rozlozenim panela polozka nema rodica a jej BackColor je predvolena farba, nie farba panela
        var color = item.GetCurrentParent() is null && item.Owner is { } owner ? owner.BackColor : item.BackColor;
        // priehladny panel (napr. v zahlavi okna) - rozhoduje farba pod nim
        for (Control? control = item.Owner; color.A < 128 && control is not null; control = control.Parent)
            color = control.BackColor;
        return color;
    }
}
