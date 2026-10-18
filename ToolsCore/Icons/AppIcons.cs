using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using Svg;

namespace ToolsCore.Icons;

/// <summary>
/// Banka ikon SVG programov (Fluent UI System Icons, zdroj a licencia v <c>Icons/LICENSES.md</c>) vykreslovanych ostro
/// v danej velkosti, so svetlym a tmavym variantom a volitelnym odznakom v pravom dolnom rohu.
/// </summary>
/// <remarks>
/// Kazda ikona je nakreslena zvlast pre niekolko velkosti (16, 20, 24, 28, 32) s ciarami hrubky 1 presne na bodoch:
/// <c>Icons/Svg/nazov_velkost.svg</c> a variant pre tmave pozadie <c>nazov_velkost_dark.svg</c>, odznaky
/// <c>Icons/Badges</c> rovnako. Vykresli sa najvacsia kresba alebo jej cely nasobok, ktory sa do velkosti zmesti
/// (<see cref="Choose" />), a vystredi sa na cele body - ikona sa nikdy nezvacsuje o necely nasobok, pri ktorom by sa
/// ciary rozmazali. Nove ikony prida <c>tools/import_icons.py</c>, odznaky <c>tools/gen_badges.py</c>.
/// </remarks>
public static class AppIcons
{
    private const string IconPrefix = "icons/";
    private const string BadgePrefix = "badges/";
    private const string DarkSuffix = "_dark";
    private const string Extension = ".svg";

    private static readonly HashSet<string> Resources = [.. typeof(AppIcons).Assembly.GetManifestResourceNames()];
    private static readonly ILookup<string, int> IconDesigns = Designs(IconPrefix);
    private static readonly ILookup<string, int> BadgeDesigns = Designs(BadgePrefix);
    private static readonly Dictionary<(string Name, string? Badge, int Size, bool Dark), Bitmap?> Cache = [];
    private static readonly Lock CacheLock = new();

    /// <summary>
    /// Nazvy ikon banky.
    /// </summary>
    public static IReadOnlyList<string> Names { get; } = [.. IconDesigns.Select(g => g.Key).Order(StringComparer.Ordinal)];

    /// <summary>
    /// Nazvy odznakov (podikon), ktore sa daju pridat k ikone.
    /// </summary>
    public static IReadOnlyList<string> Badges { get; } = [.. BadgeDesigns.Select(g => g.Key).Order(StringComparer.Ordinal)];

    /// <summary>
    /// Ikona vykreslena do stvorca danej velkosti, s odznakom v pravom dolnom rohu. Ikona pod odznakom a v jeho
    /// tesnom okoli sa vymaze, aby sa odznak od nej oddelil.
    /// </summary>
    /// <param name="name">nazov ikony z <see cref="Names" /></param>
    /// <param name="badge">nazov odznaku z <see cref="Badges" /> alebo null</param>
    /// <param name="size">velkost v bodoch obrazovky</param>
    /// <param name="dark">variant pre tmave pozadie</param>
    /// <returns>obrazok zo spolocnej vyrovnavacej pamate (nemenit ani neuvolnovat) alebo null, ak ikona neexistuje</returns>
    public static Bitmap? Get(string name, string? badge, int size, bool dark)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        var key = (name, badge, size, dark);
        lock (CacheLock)
        {
            if (!Cache.TryGetValue(key, out var bitmap))
                Cache[key] = bitmap = Render(name, badge, size, dark);
            return bitmap;
        }
    }

    /// <summary>
    /// Ci ma ikona na pozadi danej farby pouzit tmavy variant.
    /// </summary>
    public static bool IsDarkBackground(Color color) => color.A >= 128 && color.R * 299 + color.G * 587 + color.B * 114 < 128_000;

    /// <summary>
    /// Kresba a jej cely nasobok, ktore sa vykreslia do stvorca danej velkosti: najvacsie, co sa zmesti (pri rovnakej
    /// vysledku vacsia kresba, napr. 48 = 2 x 24, nie 3 x 16). Ak sa nezmesti ani najmensia kresba, nasobok je 0 -
    /// najmensia kresba sa zmensi na danu velkost.
    /// </summary>
    /// <param name="designs">nakreslene velkosti ikony</param>
    /// <param name="size">velkost v bodoch obrazovky</param>
    internal static (int Design, int Scale) Choose(IEnumerable<int> designs, int size)
    {
        var sorted = designs.OrderDescending().ToList();
        (int Design, int Scale) best = (sorted[^1], 0);
        foreach (var design in sorted)
            for (var scale = 1; design * scale <= size; scale++)
                if (design * scale > best.Design * best.Scale)
                    best = (design, scale);
        return best;
    }

    // nakreslene velkosti podla nazvov prostriedkov prefix + nazov_velkost[_dark].svg
    private static ILookup<string, int> Designs(string prefix) =>
        Resources.Where(r => r.StartsWith(prefix, StringComparison.Ordinal) && r.EndsWith(Extension, StringComparison.Ordinal))
            .Select(r => r[prefix.Length..^Extension.Length])
            .Where(n => !n.EndsWith(DarkSuffix, StringComparison.Ordinal))
            .Select(n => (Name: n[..n.LastIndexOf('_')], Size: int.Parse(n[(n.LastIndexOf('_') + 1)..], CultureInfo.InvariantCulture)))
            .ToLookup(d => d.Name, d => d.Size, StringComparer.Ordinal);

    private static Bitmap? Render(string name, string? badge, int size, bool dark)
    {
        if (!IconDesigns.Contains(name))
            return null;

        var (design, scale) = Choose(IconDesigns[name], size);
        var drawn = scale == 0 ? size : design * scale;
        var bitmap = Draw(Resource(IconPrefix, name, design, dark), drawn);
        // odznak z rovnakej mriezky ako ikona
        if (badge is not null && BadgeDesigns[badge].Contains(design))
        {
            using var overlay = Draw(Resource(BadgePrefix, badge, design, dark), drawn);
            Overlay(bitmap, overlay, Math.Max(1, scale));
        }

        return drawn == size ? bitmap : Center(bitmap, size);
    }

    // na tmavom pozadi tmavy variant, ak ho kresba ma
    private static string Resource(string prefix, string name, int design, bool dark)
    {
        var light = $"{prefix}{name}_{design.ToString(CultureInfo.InvariantCulture)}";
        return dark && Resources.Contains(light + DarkSuffix + Extension) ? light + DarkSuffix + Extension : light + Extension;
    }

    // mensia kresba do stredu stvorca - posun o cele body, aby ostali ciary ostre
    private static Bitmap Center(Bitmap drawn, int size)
    {
        using (drawn)
        {
            var source = ReadPixels(drawn);
            var pixels = new int[size * size];
            var offset = (size - drawn.Width) / 2;
            for (var y = 0; y < drawn.Height; y++)
                Array.Copy(source, y * drawn.Width, pixels, (y + offset) * size + offset, drawn.Width);

            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            WritePixels(bitmap, pixels);
            return bitmap;
        }
    }

    private static Bitmap Draw(string resource, int size)
    {
        using var stream = typeof(AppIcons).Assembly.GetManifestResourceStream(resource)!;
        var document = SvgDocument.Open<SvgDocument>(stream);
        document.Width = size;
        document.Height = size;

        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        // stredy bodov na .5 ako v SVG - inak sa ciara hrubky 1 na suradnici x.5 rozmaze do dvoch bodov
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var renderer = SvgRenderer.FromGraphics(graphics);
        document.Draw(renderer);
        return bitmap;
    }

    // odznak navrch ikony; ikona sa pod nim a do vzdialenosti gap okolo neho vymaze (vedla plneho odznaku uplne,
    // pri vyhladenom okraji odznaku ciastocne - vyrez ma tiez vyhladeny okraj)
    private static void Overlay(Bitmap target, Bitmap badge, int gap)
    {
        var size = target.Width;
        var icon = ReadPixels(target);
        var over = ReadPixels(badge);
        var result = new int[icon.Length];

        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var halo = 0;
            for (var dy = -gap; dy <= gap; dy++)
            for (var dx = -gap; dx <= gap; dx++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < size && ny < size && dx * dx + dy * dy <= 2 * gap * gap)
                    halo = Math.Max(halo, Alpha(over[ny * size + nx]));
            }

            halo = Math.Min(255, halo * 2);
            var i = y * size + x;
            var iconAlpha = Alpha(icon[i]) * (255 - halo) / 255;
            var badgeAlpha = Alpha(over[i]);
            var alpha = badgeAlpha + iconAlpha * (255 - badgeAlpha) / 255;
            if (alpha == 0)
                continue;

            var pixel = alpha << 24;
            for (var shift = 0; shift <= 16; shift += 8)
            {
                var channel = ((over[i] >> shift & 0xFF) * badgeAlpha + (icon[i] >> shift & 0xFF) * iconAlpha * (255 - badgeAlpha) / 255) / alpha;
                pixel |= Math.Min(channel, 255) << shift;
            }

            result[i] = pixel;
        }

        WritePixels(target, result);
    }

    private static int Alpha(int pixel) => (pixel >> 24) & 0xFF;

    private static int[] ReadPixels(Bitmap bitmap)
    {
        var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var pixels = new int[bitmap.Width * bitmap.Height];
            for (var y = 0; y < bitmap.Height; y++)
                Marshal.Copy(data.Scan0 + y * data.Stride, pixels, y * bitmap.Width, bitmap.Width);
            return pixels;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static void WritePixels(Bitmap bitmap, int[] pixels)
    {
        var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (var y = 0; y < bitmap.Height; y++)
                Marshal.Copy(pixels, y * bitmap.Width, data.Scan0 + y * data.Stride, bitmap.Width);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }
}
