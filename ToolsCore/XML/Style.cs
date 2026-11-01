using System.Xml.Linq;
using ToolsCore.Properties;

namespace ToolsCore.XML;

/// <summary>
/// Farebna schema programu: zaklad (svetla alebo tmava schema) a zmeny pouzivatela voci nemu. Farby, ktore cita
/// program (<see cref="ControlsColorScheme" /> a schemy programu), nastavi <see cref="Apply()" />.
/// </summary>
public class Style
{
    /// <summary>
    /// Nazov schemy (pri zabudovanych <see cref="StyleNames" />).
    /// </summary>
    public string Name { get; set; } = StyleNames.Light;

    /// <summary>
    /// Zaklad schemy - predvolene farby a tmave zahlavie, posuvniky a vlastne kreslenie prvkov.
    /// </summary>
    public SchemeBase Base { get; set; }

    /// <summary>
    /// Zmeny voci zakladu podla <see cref="ColorKey.Id" /> (aj polozky, ktore program nepozna - zachovaju sa).
    /// </summary>
    public Dictionary<string, ColorOverride> Overrides { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Farby ovladacich prvkov.
    /// </summary>
    public ControlsColorScheme ControlsColorScheme { get; } = new();

    /// <summary>
    /// Tmava schema.
    /// </summary>
    public bool IsDark => Base == SchemeBase.Dark;

    /// <summary>
    /// Okna maju tmave zahlavie.
    /// </summary>
    public bool DarkTitleBar => IsDark;

    /// <summary>
    /// Prvky maju tmave posuvniky.
    /// </summary>
    public bool DarkScrollBar => IsDark;

    /// <summary>
    /// Ovladacie prvky maju systemovy vzhlad (svetla schema), inak ich kresli program farbami schemy.
    /// </summary>
    public bool ControlsDefaultStyle => !IsDark;

    /// <summary>
    /// Zabudovana schema (svetla alebo tmava) - neda sa premenovat ani odstranit.
    /// </summary>
    public bool IsBuiltIn => Name is StyleNames.Light or StyleNames.Dark;

    /// <summary>
    /// Polozky schemy programu.
    /// </summary>
    public virtual IReadOnlyList<ColorKey> ColorKeys => UiColors.Keys;

    /// <summary>
    /// Hodnota polozky zo zakladu schemy.
    /// </summary>
    public ColorValue DefaultOf(ColorKey key) => key.DefaultFor(Base);

    /// <summary>
    /// Hodnota polozky so zmenami pouzivatela.
    /// </summary>
    public ColorValue ValueOf(ColorKey key)
    {
        var value = DefaultOf(key);
        if (!Overrides.TryGetValue(key.Id, out var o))
            return value;

        return new ColorValue(o.Fore ?? value.Fore, o.Back ?? value.Back, o.Bold ?? value.Bold);
    }

    /// <summary>
    /// Zmena polozky (<see langword="null" /> = bez zmeny).
    /// </summary>
    public ColorOverride? OverrideOf(ColorKey key) => Overrides.GetValueOrDefault(key.Id);

    /// <summary>
    /// Nastavi zmenu polozky a prepocita jej farby; prazdna zmena = hodnota zo zakladu.
    /// </summary>
    public void SetOverride(ColorKey key, ColorOverride? value)
    {
        if (value is null || value.IsEmpty)
            Overrides.Remove(key.Id);
        else
            Overrides[key.Id] = value;
        Apply(key);
    }

    /// <summary>
    /// Zmeni zaklad schemy a prepocita farby.
    /// </summary>
    public void SetBase(SchemeBase value)
    {
        Base = value;
        Apply();
    }

    /// <summary>
    /// Nastavi farby, ktore cita program, podla zakladu a zmien.
    /// </summary>
    public void Apply()
    {
        foreach (var key in ColorKeys)
            Apply(key);
    }

    private void Apply(ColorKey key)
    {
        var setting = key.Setting(this);
        var value = ValueOf(key);
        setting.ForeColor = value.Fore;
        setting.BackColor = value.Back;
        setting.Bold = value.Bold;
    }

    /// <summary>
    /// Kopia schemy s novym nazvom (zaklad aj zmeny).
    /// </summary>
    public Style Copy(string name)
    {
        var copy = CreateEmpty();
        copy.Name = name;
        copy.Base = Base;
        foreach (var (id, value) in Overrides)
            copy.Overrides[id] = value;
        copy.Apply();
        return copy;
    }

    /// <summary>
    /// Nova prazdna schema typu programu.
    /// </summary>
    protected virtual Style CreateEmpty() => new();

    /// <summary>
    /// Precita udaje stareho suboru stylov, ktore nie su farbami (napr. pismo editora).
    /// </summary>
    /// <param name="element">prvok stylu zo stareho suboru</param>
    protected internal virtual void ReadLegacy(XElement element)
    {
    }

    /// <summary>
    /// Nazov schemy v jazyku programu (zabudovane schemy maju preklad).
    /// </summary>
    public string DisplayName => Name switch
    {
        StyleNames.Light => Resources.Scheme_Light,
        StyleNames.Dark => Resources.Scheme_Dark,
        _ => Name
    };

    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
