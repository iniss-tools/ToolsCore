using System.Collections;
using System.Reflection;
using System.Xml.Serialization;
using ToolsCore.Tools;

namespace ToolsCore.XML;

/// <summary>
///     Trieda reprezentujuca zoznam stylov definovanych pre GUI programu
/// </summary>
[XmlRoot("STYLES")]
public class Styles<T> : IEnumerable<T>, IList where T : Style
{
    private readonly object _sync = new();

    /// <summary>
    ///     Zoznam vsetkych stylov s vlastnostami
    /// </summary>
    [XmlIgnore] 
    public List<T> StyleList { get; set; }

    /// <summary>
    ///     Konstruktor
    /// </summary>
    public Styles() : this(new List<T>())
    {
    }

    public Styles(List<T> styleList)
    {
        StyleList = styleList;
    }

    /// <inheritdoc />
    public int IndexOf(object? item) => StyleList.IndexOf((item as T)!);

    /// <inheritdoc />
    public void Insert(int index, object? item) => StyleList.Insert(index, (item as T)!);

    /// <inheritdoc />
    public void RemoveAt(int index) => StyleList.RemoveAt(index);

    object? IList.this[int index]
    {
        get => StyleList[index];
        set => StyleList[index] = (value as T)!;
    }

    /// <summary>
    ///     Indexer pre jednoduchsi vyber z listu stylov
    /// </summary>
    /// <param name="index">index prvku</param>
    public T this[int index]
    {
        get => StyleList[index];
        set => StyleList[index] = value;
    }

    /// <summary>
    ///     Indexer pre jednoduchsi vyber z listu stylov. Vrati styl podla nazvu stylu
    /// </summary>
    /// <param name="key"></param>
    public T this[string key] => StyleList.First(i => i.Name == key);

    /// <inheritdoc />
    public void Remove(object? item) => StyleList.Remove((item as T)!);

    /// <inheritdoc />
    public void CopyTo(Array array, int index)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     Vrati pocet stylov v zozname
    /// </summary>
    public int Count => StyleList.Count;

    /// <inheritdoc />
    public object SyncRoot => _sync;

    /// <inheritdoc />
    public bool IsSynchronized => false;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public bool IsFixedSize => false;

    /// <summary>
    ///     Prida styl do zoznamu stylov
    /// </summary>
    /// <param name="style"></param>
    public void Add(T style) => StyleList.Add(style);

    /// <inheritdoc />
    int IList.Add(object? value)
    {
        StyleList.Add((value as T)!);
        return Count;
    }

    /// <inheritdoc />
    public void Clear() => StyleList.Clear();

    /// <inheritdoc />
    public bool Contains(object? item) => item is T t && StyleList.Contains(t);

    /// <summary>
    ///     Nacitava data z konfiguracneho suboru
    /// </summary>
    /// <param name="fileName">cesta k suboru</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Ak konfiguacny subor obsahoval nelatnu hodnotu</exception>
    public static Styles<T> ReadData(string fileName)
    {
        Styles<T>? styles = null;
        try
        {
            var text = File.ReadAllText(fileName, Encodings.Win1250);
            if (!string.IsNullOrEmpty(text))
            {
                styles = (Styles<T>)XmlSerialization.Deserialize(text, typeof(Styles<T>))!;

                var ids = new List<string>(styles.StyleList.Count);
                ids.AddRange(styles.StyleList.Select(style => style.Name));

                var query = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (query.Count != 0)
                {
                    var error = $"Chyba v súbore štýlov: Štýl {query[0]} je zadefinovaný viackrát.";
                    Log.Error(error);
                    throw new ArgumentException(error);
                }

                var rewrite = false;

                if (!ids.Contains(StyleNames.LIGHT))
                {
                    styles.StyleList.Insert(0, GetDefaultStyle(false));
                    rewrite = true;
                }

                if (!ids.Contains(StyleNames.DARK))
                {
                    styles.StyleList.Insert(1, GetDefaultStyle(true));
                    rewrite = true;
                }

                if (rewrite) 
                    WriteData(fileName, styles);

                //keby malo viacero stylov nastavene Used na true
                try
                {
                    var _ = styles.SingleOrDefault(s => s.Used);
                }
                catch (Exception)
                {
                    foreach (var style in styles)
                    {
                        style.Used = false;
                    }
                }

                //keby nemal ziaden styl nastavene Used na true
                var usedStyle = styles.FirstOrDefault(s => s.Used);
                
                if (usedStyle is null)
                {
                    styles[0].Used = true;
                }
            }
        }
        catch (FileNotFoundException)
        {
            //ignored, styles will be null
        }

        if (styles == null)
        {
            styles = new Styles<T>();

            styles.StyleList.Insert(0, GetDefaultStyle(false));
            styles.StyleList.Insert(1, GetDefaultStyle(true));
            WriteData(fileName, styles);
        }

        return styles;
    }

    /// <summary>
    ///     Zapise data do konfiguracneho suboru
    /// </summary>
    /// <param name="fileName">cesta k suboru</param>
    /// <param name="obj">data</param>
    public static void WriteData(string fileName, object obj) => XmlSerialization.SerializeToFile(fileName, obj);

    public static T GetDefaultStyle(bool dark)
    {
        var type = typeof(T);
        var property = type.GetProperty(dark ? nameof(Style.DefaultDarkStyle) : nameof(Style.DefaultLightStyle),
            BindingFlags.Public | BindingFlags.Static);
        return (property?.GetValue(null) as T)!;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => StyleList.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Styles(Styles<T> original) : this()
    {
        StyleList = new List<T>();
        foreach (var style in original.StyleList)
            StyleList.Add(style with { });
    }
}