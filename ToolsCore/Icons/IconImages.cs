namespace ToolsCore.Icons;

/// <summary>
/// Ikony z banky <see cref="AppIcons" /> pre prvky WinForms mimo poloziek <c>Icon*</c>: obrazok pre prvok (bunka mriezky,
/// <see cref="PictureBox" />), zoznamy obrazkov (<see cref="ImageList" />) pre stromy a ponuky, ikona chyby pre
/// <see cref="ErrorProvider" /> a ikony dialogov. Kluc obrazka je nazov ikony, s odznakom <c>nazov+odznak</c> (napr. <c>train+add</c>).
/// </summary>
public static class IconImages
{
    /// <summary>
    /// Logicka velkost ikon v zozname (pri 96 DPI).
    /// </summary>
    public const int Size = 16;

    /// <summary>
    /// Logicka velkost ikony dialogu so spravou.
    /// </summary>
    public const int DialogSize = 32;

    private static readonly Dictionary<(int Size, bool Dark), Icon> ErrorIcons = [];
    private static readonly Lock ErrorIconsLock = new();

    /// <summary>
    /// Ikona pre prvok vo velkosti podla jeho DPI a vo variante podla jeho pozadia (pri mriezke podla pozadia buniek).
    /// </summary>
    /// <param name="owner">prvok, ktory ikonu zobrazuje</param>
    /// <param name="name">nazov ikony, s odznakom <c>nazov+odznak</c></param>
    /// <param name="size">logicka velkost (pri 96 DPI)</param>
    /// <returns>obrazok zo spolocnej vyrovnavacej pamate (nemenit ani neuvolnovat) alebo null, ak ikona neexistuje</returns>
    public static Bitmap? For(Control owner, string name, int size = Size)
    {
        ArgumentNullException.ThrowIfNull(owner);
        var back = owner is DataGridView grid ? grid.DefaultCellStyle.BackColor : owner.BackColor;
        var (icon, badge) = Split(name);
        return AppIcons.Get(icon, badge, owner.LogicalToDeviceUnits(size), AppIcons.IsDarkBackground(back));
    }

    /// <summary>
    /// Ikona chyby pre <see cref="ErrorProvider.Icon" /> - <c>status-error</c> vo velkosti podla DPI prvku.
    /// </summary>
    /// <param name="owner">prvok, pri ktorom sa chyba zobrazuje</param>
    /// <returns>ikona zo spolocnej vyrovnavacej pamate (neuvolnovat); systemova, ak by v banke chybala</returns>
    public static Icon ErrorIcon(Control owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        var size = owner.LogicalToDeviceUnits(Size);
        var dark = AppIcons.IsDarkBackground(owner.BackColor);
        lock (ErrorIconsLock)
        {
            if (!ErrorIcons.TryGetValue((size, dark), out var icon) && AppIcons.Get("status-error", null, size, dark) is { } bitmap)
                // handle ikony sa neuvolnuje - ikon je najviac par (velkost x variant) na cely beh programu
                ErrorIcons[(size, dark)] = icon = Icon.FromHandle(bitmap.GetHicon());
            return icon ?? SystemIcons.Error;
        }
    }

    /// <summary>
    /// Ikona dialogu so spravou (chyba, varovanie, informacia, otazka) vo velkosti <see cref="DialogSize" />.
    /// </summary>
    /// <param name="icon">druh ikony dialogu</param>
    /// <param name="owner">prvok, ktory ikonu zobrazuje</param>
    /// <returns>obrazok zo spolocnej vyrovnavacej pamate alebo null pri <see cref="MessageBoxIcon.None" /></returns>
    public static Bitmap? ForMessage(MessageBoxIcon icon, Control owner) => icon switch
    {
        MessageBoxIcon.Error => For(owner, "status-error", DialogSize),
        MessageBoxIcon.Warning => For(owner, "status-warning", DialogSize),
        MessageBoxIcon.Information => For(owner, "status-info", DialogSize),
        MessageBoxIcon.Question => For(owner, "status-question", DialogSize),
        _ => null
    };

    /// <summary>
    /// Naplni zoznam ikonami vo velkosti <see cref="Size" /> podla DPI prvku a vo variante podla jeho pozadia;
    /// povodne obrazky odstrani. Volat po nastaveni temy, pri jej zmene znova.
    /// </summary>
    /// <param name="list">zoznam obrazkov</param>
    /// <param name="owner">prvok, ktory zoznam zobrazuje</param>
    /// <param name="keys">kluce obrazkov v poradi indexov</param>
    public static void Fill(ImageList list, Control owner, IEnumerable<string> keys) =>
        Fill(list, owner.LogicalToDeviceUnits(Size), AppIcons.IsDarkBackground(owner.BackColor), keys);

    /// <summary>
    /// Naplni zoznam ikonami danej velkosti; povodne obrazky odstrani.
    /// </summary>
    /// <param name="list">zoznam obrazkov</param>
    /// <param name="size">velkost v bodoch obrazovky</param>
    /// <param name="dark">variant pre tmave pozadie</param>
    /// <param name="keys">kluce obrazkov v poradi indexov</param>
    public static void Fill(ImageList list, int size, bool dark, IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(list);
        list.Images.Clear();
        list.ColorDepth = ColorDepth.Depth32Bit;
        list.ImageSize = new Size(size, size);
        foreach (var key in keys.Distinct(StringComparer.Ordinal))
        {
            var (name, badge) = Split(key);
            if (AppIcons.Get(name, badge, size, dark) is { } image)
                list.Images.Add(key, image);
        }
    }

    private static (string Name, string? Badge) Split(string key)
    {
        var plus = key.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? (key, null) : (key[..plus], key[(plus + 1)..]);
    }
}
