using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ToolsCore.Commands;

namespace ToolsCore.XML;

/// <summary>
/// Klavesove skratky prikazov ulozene v nastaveniach programu podla identifikatora prikazu. V config.xml je kazda
/// skratka prvok s nazvom prikazu a atributom <c>sc</c> (<c>&lt;OpenGVD sc="CtrlO" /&gt;</c>). Prikaz bez zaznamu ma
/// predvolenu skratku; neznamy prvok alebo hodnota sa pri nacitani preskoci.
/// </summary>
public sealed class ShortcutMap : IXmlSerializable
{
    private const string ATTRIBUTE = "sc";

    private readonly Dictionary<string, Shortcut> _shortcuts = new(StringComparer.Ordinal);

    /// <summary>
    /// Pocet ulozenych skratiek.
    /// </summary>
    public int Count => _shortcuts.Count;

    /// <summary>
    /// Skratka prikazu; bez zaznamu predvolena.
    /// </summary>
    public Shortcut Get(CommandInfo command) =>
        _shortcuts.TryGetValue(command.Id, out var shortcut) ? shortcut : command.DefaultShortcut;

    /// <summary>
    /// Nastavi skratku prikazu.
    /// </summary>
    public void Set(string id, Shortcut shortcut) => _shortcuts[id] = shortcut;

    /// <summary>
    /// Nezavisla kopia skratiek.
    /// </summary>
    public ShortcutMap Clone()
    {
        var copy = new ShortcutMap();
        foreach (var (id, shortcut) in _shortcuts)
            copy._shortcuts[id] = shortcut;
        return copy;
    }

    /// <summary>
    /// Riadky na upravu skratiek v nastaveniach - v poradi prikazov <paramref name="commands" />.
    /// </summary>
    public List<CmdShortcut> ToRows(IEnumerable<CommandInfo> commands) =>
        commands.Select(c => new CmdShortcut(Get(c), c.Text, c.Id)).ToList();

    /// <summary>
    /// Riadky s predvolenymi skratkami prikazov.
    /// </summary>
    public static List<CmdShortcut> DefaultRows(IEnumerable<CommandInfo> commands) => new ShortcutMap().ToRows(commands);

    /// <summary>
    /// Prevezme skratky z upravenych riadkov nastaveni.
    /// </summary>
    public void SetFromRows(IEnumerable<CmdShortcut> rows)
    {
        foreach (var row in rows)
            Set(row.PropertyName, row.Shortcut.Value);
    }

    XmlSchema? IXmlSerializable.GetSchema() => null;

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        _shortcuts.Clear();
        var empty = reader.IsEmptyElement;
        reader.ReadStartElement();
        if (empty)
            return;

        reader.MoveToContent();
        while (reader.NodeType != XmlNodeType.EndElement && !reader.EOF)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var value = reader.GetAttribute(ATTRIBUTE);
                if (value is not null && Enum.TryParse<Shortcut>(value, out var shortcut) && Enum.IsDefined(shortcut))
                    _shortcuts[reader.LocalName] = shortcut;
                reader.Skip();
            }
            else
            {
                reader.Read();
            }

            reader.MoveToContent();
        }

        reader.ReadEndElement();
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        foreach (var (id, shortcut) in _shortcuts)
        {
            writer.WriteStartElement(id);
            writer.WriteAttributeString(ATTRIBUTE, XmlEnum<Shortcut>.EnumToString(shortcut));
            writer.WriteEndElement();
        }
    }
}
