using System.Xml.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ToolsCore.XML;

/// <summary>
///     Reprezentuje klavesovu skratku pouzivanu na hlavnej pracovnej ploche programu.
/// </summary>
public record CmdShortcut
{
    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="CmdShortcut"/>.
    /// </summary>
    public CmdShortcut()
    {
    }

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="CmdShortcut"/> podla struktury <see cref="ShortcutName"/>.
    /// </summary>
    /// <param name="shortcut"></param>
    public CmdShortcut(ShortcutName shortcut)
    {
        Shortcut = shortcut;
    }

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="CmdShortcut"/>.
    /// </summary>
    public CmdShortcut(ShortcutName shortcut, string name, string propertyName)
    {
        Shortcut = shortcut;
        Name = name;
        PropertyName = propertyName;
    }

    [XmlIgnore]
    public string PropertyName { get; set; } = null!;

    /// <summary>
    ///     Názov použitia klávesovej skratky.
    /// </summary>
    [XmlIgnore, Localizable(true)]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Klávesová skratka ako štruktúra <see cref="ShortcutName"/>.
    /// </summary>
    [XmlIgnore]
    public ShortcutName Shortcut { get; set; }

    /// <summary>
    ///     XML reprezentácia klávesovej skratky.
    /// </summary>
    [XmlAttribute("sc"), Browsable(false)]
    public string ShortcutXML
    {
        get => XmlEnum<Shortcut>.EnumToString(Shortcut.Value);
        set => Shortcut = new ShortcutName(XmlEnum<Shortcut>.StringToEnum(value));
    }

    public static implicit operator Keys(CmdShortcut? cmd)
    {
        if (cmd is null)
            return (Keys)System.Windows.Forms.Shortcut.None;

        return (Keys)cmd.Shortcut.Value;
    }
}

/// <summary>
///     Štruktúra <see cref="Shortcut" /> pridaná o predefinovanie metódy <see cref="ToString" />, ktorá vracia
///     reprezentáciu klávesovej skratky vo formáte: X+Y.
///     Ak je <see cref="Value" /> <see cref="Shortcut.None" />, vráti "(Žiadna)".
/// </summary>
/// <remarks>
///     Vytvori novu instanciu struktury <see cref="ShortcutName"/> podla enumeracie <see cref="Shortcut"/>.
/// </remarks>
/// <param name="shortcut"></param>
public readonly struct ShortcutName(Shortcut shortcut)
{
    /// <summary>
    ///     Klávesová skratka ako štruktúra.
    /// </summary>
    public Shortcut Value { get; } = shortcut;

    public static implicit operator Shortcut(ShortcutName sn) => sn.Value;

    public static implicit operator ShortcutName(Shortcut s) => new(s);

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == Shortcut.None) 
            return "(Žiadna)";
        return new KeysConverter().ConvertToString((Keys)Value) ?? "";
    }
}