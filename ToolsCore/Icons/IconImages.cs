namespace ToolsCore.Icons;

/// <summary>
/// Zoznamy obrazkov (<see cref="ImageList" />) s ikonami z banky <see cref="AppIcons" /> - pre stromy, zoznamy a ponuky,
/// ktore beru obrazky zo zoznamu. Kluc obrazka je nazov ikony, s odznakom <c>nazov+odznak</c> (napr. <c>train+add</c>).
/// </summary>
public static class IconImages
{
    /// <summary>
    /// Logicka velkost ikon v zozname (pri 96 DPI).
    /// </summary>
    public const int Size = 16;

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
