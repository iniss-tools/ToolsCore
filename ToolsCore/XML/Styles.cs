using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Serilog;
using ToolsCore.Iniss.Tools;
using ToolsCore.Properties;

namespace ToolsCore.XML;

/// <summary>
/// Farebne schemy programu v subore <c>STYLES.cfg</c>. Zapisuju sa len zmeny voci zakladu schemy (verzia 2);
/// stary subor s uplnymi hodnotami sa pri nacitani prevedie a zalohuje ako <c>STYLES.v1.cfg</c>.
/// </summary>
public class Styles<T> : IStyleSet where T : Style, new()
{
    private const string RootName = "SCHEMES";
    private const string LegacyRootName = "STYLES";
    private const string BaseLight = "light";
    private const string BaseDark = "dark";

    /// <summary>
    /// Zoznam schem.
    /// </summary>
    public List<T> StyleList { get; set; }

    /// <inheritdoc />
    public string Active { get; set; } = StyleNames.Light;

    /// <inheritdoc />
    public bool FollowSystem { get; set; }

    /// <inheritdoc />
    public string LightScheme { get; set; } = StyleNames.Light;

    /// <inheritdoc />
    public string DarkScheme { get; set; } = StyleNames.Dark;

    /// <summary>
    /// Prazdny zoznam.
    /// </summary>
    public Styles() : this(new List<T>())
    {
    }

    /// <summary>
    /// Zoznam s danymi schemami.
    /// </summary>
    public Styles(List<T> styleList)
    {
        StyleList = styleList;
    }

    /// <summary>
    /// Kopia vsetkych schem a vyberu (pre okno nastaveni).
    /// </summary>
    public Styles(Styles<T> original) : this()
    {
        foreach (var style in original.StyleList)
            StyleList.Add((T)style.Copy(style.Name));
        Active = original.Active;
        FollowSystem = original.FollowSystem;
        LightScheme = original.LightScheme;
        DarkScheme = original.DarkScheme;
    }

    /// <summary>
    /// Zabudovane schemy (svetla a tmava) bez zmien.
    /// </summary>
    public static Styles<T> CreateDefault() => new(new List<T> { CreateStyle(StyleNames.Light, SchemeBase.Light), CreateStyle(StyleNames.Dark, SchemeBase.Dark) });

    /// <summary>
    /// Nova schema bez zmien.
    /// </summary>
    public static T CreateStyle(string name, SchemeBase schemeBase)
    {
        var style = new T { Name = name, Base = schemeBase };
        style.Apply();
        return style;
    }

    /// <inheritdoc />
    public Style Create(string name, SchemeBase schemeBase) => CreateStyle(name, schemeBase);

    /// <inheritdoc />
    public Style? Find(string name) => StyleList.FirstOrDefault(s => s.Name == name);

    /// <inheritdoc />
    public Style Current(bool windowsDark)
    {
        if (FollowSystem)
            return Find(windowsDark ? DarkScheme : LightScheme) ?? Find(windowsDark ? StyleNames.Dark : StyleNames.Light) ?? StyleList[0];
        return Find(Active) ?? StyleList[0];
    }

    #region Zoznam

    /// <summary>
    /// Pocet schem.
    /// </summary>
    public int Count => StyleList.Count;

    /// <summary>
    /// Schema podla poradia.
    /// </summary>
    public T this[int index]
    {
        get => StyleList[index];
        set => StyleList[index] = value;
    }

    /// <summary>
    /// Schema podla nazvu.
    /// </summary>
    public T this[string key] => StyleList.First(i => i.Name == key);

    /// <summary>
    /// Poradie schemy.
    /// </summary>
    public int IndexOf(T item) => StyleList.IndexOf(item);

    /// <summary>
    /// Prida schemu.
    /// </summary>
    public void Add(T value) => StyleList.Add(value);

    /// <summary>
    /// Vlozi schemu.
    /// </summary>
    public void Insert(int index, T item) => StyleList.Insert(index, item);

    /// <summary>
    /// Odstrani schemu.
    /// </summary>
    public bool Remove(T item) => StyleList.Remove(item);

    /// <summary>
    /// Prechod schemami typu programu.
    /// </summary>
    public List<T>.Enumerator GetEnumerator() => StyleList.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    IEnumerator<Style> IEnumerable<Style>.GetEnumerator() => StyleList.GetEnumerator();

    /// <inheritdoc />
    Style IReadOnlyList<Style>.this[int index] => StyleList[index];

    /// <inheritdoc />
    void IStyleSet.Add(Style style) => Add((T)style);

    /// <inheritdoc />
    bool IStyleSet.Remove(Style style) => style is T t && Remove(t);

    #endregion

    /// <summary>
    /// Nacita schemy zo suboru; chybajuci subor vytvori so zabudovanymi schemami, stary subor prevedie.
    /// </summary>
    /// <exception cref="ArgumentException">schema je v subore viackrat</exception>
    public static Styles<T> ReadData(string fileName)
    {
        if (!File.Exists(fileName) || new FileInfo(fileName).Length == 0)
        {
            var created = CreateDefault();
            WriteData(fileName, created);
            Log.Information("Subor stylov {File} vytvoreny so zabudovanymi schemami", fileName);
            return created;
        }

        var root = Parse(File.ReadAllBytes(fileName));
        if (root.Name.LocalName == LegacyRootName)
        {
            var migrated = FromLegacy(root);
            var backup = Path.Combine(Path.GetDirectoryName(fileName)!, Path.GetFileNameWithoutExtension(fileName) + ".v1" + Path.GetExtension(fileName));
            if (!File.Exists(backup))
                File.Copy(fileName, backup);
            WriteData(fileName, migrated);
            Log.Information("Subor stylov {File} prevedeny zo starej verzie, povodny v {Backup}", fileName, backup);
            return migrated;
        }

        var styles = FromXml(root);
        if (styles.EnsureBuiltIn())
            WriteData(fileName, styles);
        return styles;
    }

    /// <summary>
    /// Zapise schemy do suboru.
    /// </summary>
    public static void WriteData(string fileName, Styles<T> styles)
    {
        var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
        using var writer = XmlWriter.Create(fileName, settings);
        new XDocument(styles.ToXml()).Save(writer);
    }

    /// <summary>
    /// Schemy ako XML (verzia 2 - len zmeny voci zakladu).
    /// </summary>
    public XElement ToXml()
    {
        var root = new XElement(RootName,
            new XAttribute("version", 2),
            new XAttribute("active", Active),
            new XAttribute("followSystem", FollowSystem ? "true" : "false"),
            new XAttribute("light", LightScheme),
            new XAttribute("dark", DarkScheme));
        foreach (var style in StyleList)
        {
            var scheme = new XElement("Scheme", new XAttribute("name", style.Name), new XAttribute("base", style.IsDark ? BaseDark : BaseLight));
            foreach (var (id, o) in style.Overrides.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                var color = new XElement("Color", new XAttribute("id", id));
                if (o.Fore is { } fore)
                    color.Add(new XAttribute("f", XmlColor.FromColor(fore)));
                if (o.Back is { } back)
                    color.Add(new XAttribute("b", XmlColor.FromColor(back)));
                if (o.Bold is { } bold)
                    color.Add(new XAttribute("bold", bold ? "true" : "false"));
                scheme.Add(color);
            }

            root.Add(scheme);
        }

        return root;
    }

    /// <summary>
    /// Schemy z XML verzie 2.
    /// </summary>
    public static Styles<T> FromXml(XElement root)
    {
        var styles = new Styles<T>
        {
            Active = (string?)root.Attribute("active") ?? StyleNames.Light,
            FollowSystem = (bool?)root.Attribute("followSystem") ?? false,
            LightScheme = (string?)root.Attribute("light") ?? StyleNames.Light,
            DarkScheme = (string?)root.Attribute("dark") ?? StyleNames.Dark
        };
        foreach (var scheme in root.Elements("Scheme"))
        {
            var style = new T
            {
                Name = (string?)scheme.Attribute("name") ?? "",
                Base = (string?)scheme.Attribute("base") == BaseDark ? SchemeBase.Dark : SchemeBase.Light
            };
            foreach (var color in scheme.Elements("Color"))
            {
                var o = new ColorOverride(ColorAttr(color, "f"), ColorAttr(color, "b"), (bool?)color.Attribute("bold"));
                if ((string?)color.Attribute("id") is { Length: > 0 } id && !o.IsEmpty)
                    style.Overrides[id] = o;
            }

            styles.Add(style);
        }

        styles.CheckDuplicates();
        styles.EnsureBuiltIn();
        foreach (var style in styles)
            style.Apply();
        return styles;
    }

    /// <summary>
    /// Prevedie stary subor stylov (uplne hodnoty kazdeho stylu): zaklad podla nazvu, vlastny styl podla tmaveho
    /// zahlavia alebo jasu panela; zapisu sa len hodnoty odlisne od zakladu.
    /// </summary>
    public static Styles<T> FromLegacy(XElement root)
    {
        var styles = new Styles<T>();
        foreach (var element in root.Elements())
        {
            var name = (string?)element.Attribute("name") ?? "";
            var style = new T { Name = name, Base = LegacyBase(element, name) };
            foreach (var key in style.ColorKeys)
            {
                if (key.LegacyPath is null || LegacyElement(element, key.LegacyPath) is not { } item)
                    continue;

                var o = LegacyOverride(item, key, style.DefaultOf(key));
                if (!o.IsEmpty)
                    style.Overrides[key.Id] = o;
            }

            style.ReadLegacy(element);
            styles.Add(style);
            if ((bool?)element.Attribute("used") == true)
                styles.Active = name;
        }

        styles.CheckDuplicates();
        styles.EnsureBuiltIn();
        if (styles.Find(styles.Active) is null)
            styles.Active = StyleNames.Light;
        foreach (var style in styles)
            style.Apply();
        return styles;
    }

    private static SchemeBase LegacyBase(XElement element, string name)
    {
        switch (name)
        {
            case StyleNames.Light:
                return SchemeBase.Light;
            case StyleNames.Dark:
                return SchemeBase.Dark;
        }

        if ((string?)element.Element("DarkTitlebar") == "true")
            return SchemeBase.Dark;

        // vlastny styl dostane zaklad, ktoremu sa viac podoba - podla jasu panela
        var panel = LegacyElement(element, "ControlsColorScheme/Panel");
        return ColorAttr(panel, "b") is { } back && ColorContrast.Luminance(back) < 0.5 ? SchemeBase.Dark : SchemeBase.Light;
    }

    private static XElement? LegacyElement(XElement element, string path)
    {
        XElement? current = element;
        foreach (var part in path.Split('/'))
            current = current?.Element(part);
        return current;
    }

    private static ColorOverride LegacyOverride(XElement item, ColorKey key, ColorValue value)
    {
        Color? fore = null, back = null;
        bool? bold = null;
        if (key.Editable.HasFlag(ColorParts.Fore) && ColorAttr(item, "f") is { } f && f.ToArgb() != value.Fore.ToArgb())
            fore = f;
        if (key.Editable.HasFlag(ColorParts.Back) && ColorAttr(item, "b") is { } b && b.ToArgb() != value.Back.ToArgb())
            back = b;
        if (key.Editable.HasFlag(ColorParts.Bold) && (bool?)item.Attribute("bold") is { } bo && bo != value.Bold)
            bold = bo;
        return new ColorOverride(fore, back, bold);
    }

    private static Color? ColorAttr(XElement? element, string name) =>
        (string?)element?.Attribute(name) is { Length: > 0 } text ? XmlColor.ToColor(text) : null;

    private static XElement Parse(byte[] bytes)
    {
        // stary subor je v kodovani windows-1250 (podla deklaracie), novy v UTF-8
        var head = Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 100));
        var encoding = head.Contains("windows-1250", StringComparison.OrdinalIgnoreCase) ? Encodings.Win1250 : Encoding.UTF8;
        var text = encoding.GetString(bytes).TrimStart('﻿');
        return XDocument.Parse(text).Root!;
    }

    private void CheckDuplicates()
    {
        var duplicate = StyleList.GroupBy(s => s.Name).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is null)
            return;

        var error = string.Format(CultureInfo.CurrentCulture, Resources.Styles_Duplicate, duplicate.Key);
        Log.Error("Styl {Style} je v subore stylov viackrat", duplicate.Key);
        throw new ArgumentException(error);
    }

    /// <returns><see langword="true" />, ak niektora zabudovana schema chybala.</returns>
    private bool EnsureBuiltIn()
    {
        var changed = false;
        if (Find(StyleNames.Light) is null)
        {
            StyleList.Insert(0, CreateStyle(StyleNames.Light, SchemeBase.Light));
            changed = true;
        }

        if (Find(StyleNames.Dark) is null)
        {
            StyleList.Insert(1, CreateStyle(StyleNames.Dark, SchemeBase.Dark));
            changed = true;
        }

        return changed;
    }
}
