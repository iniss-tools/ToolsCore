using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ToolsCore.Commands;

namespace ToolsCore.XML;

/// <summary>
/// Klavesove skratky prikazov ulozene v nastaveniach programu podla identifikatora prikazu. V config.xml je kazda
/// skratka prvok s nazvom prikazu a atributom <c>sc</c> (<c>&lt;OpenGVD sc="CtrlO" /&gt;</c>, kombinacia mimo
/// <see cref="Shortcut" /> ako <c>Ctrl+Alt+K</c>). Prikaz bez zaznamu ma predvolenu skratku; neznamy prvok alebo hodnota
/// sa pri nacitani preskoci.
/// </summary>
public sealed class ShortcutMap : IXmlSerializable
{
    private const string ATTRIBUTE = "sc";

    private readonly Dictionary<string, Keys> _shortcuts = new(StringComparer.Ordinal);

    /// <summary>
    /// Pocet ulozenych skratiek.
    /// </summary>
    public int Count => _shortcuts.Count;

    /// <summary>
    /// Skratka prikazu; bez zaznamu predvolena.
    /// </summary>
    public Keys Get(CommandInfo command) =>
        _shortcuts.TryGetValue(command.Id, out var shortcut) ? shortcut : command.DefaultKeys;

    /// <summary>
    /// Nastavi skratku prikazu.
    /// </summary>
    public void Set(string id, Keys shortcut) => _shortcuts[id] = shortcut;

    /// <summary>
    /// Vrati prikazu predvolenu skratku (zaznam sa odstrani).
    /// </summary>
    public void Reset(string id) => _shortcuts.Remove(id);

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
                if (KeyNames.TryParseSetting(reader.GetAttribute(ATTRIBUTE), out var shortcut))
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
            writer.WriteAttributeString(ATTRIBUTE, KeyNames.ToSetting(shortcut));
            writer.WriteEndElement();
        }
    }
}
