using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using ToolsCore.Tools;

namespace ToolsCore.Mcp;

/// <summary>
/// Server MCP pre agentov (Claude Code, VS Code alebo iny klient MCP) - bezi v programe na tomto pocitaci a pristupuje sa
/// k nemu klucom.
/// Program ma odvodenu triedu s nazvom servera, zaciatkom kluca a predvolenym portom (kazdy program iny, mozu bezat
/// naraz); XmlSerializer uklada do config.xml len zapnutie, povolenie zmien, port a sifrovany kluc.
/// </summary>
public abstract class McpSettings
{
    private readonly byte[] _entropy;

    /// <param name="program">nazov programu - urcuje aj sifrovanie kluca (zmena = ulozeny kluc prestane platit)</param>
    /// <param name="serverName">nazov servera v konfiguracii klienta (napr. <c>gvdeditor</c>)</param>
    /// <param name="keyPrefix">zaciatok kluca, odlisi kluce programov (napr. <c>gvd_</c>)</param>
    /// <param name="defaultPort">predvoleny port</param>
    protected McpSettings(string program, string serverName, string keyPrefix, int defaultPort)
    {
        ServerName = serverName;
        KeyPrefix = keyPrefix;
        DefaultPort = defaultPort;
        Port = defaultPort;
        _entropy = Encoding.UTF8.GetBytes(program + ".McpServer.Key");
    }

    /// <summary>Nazov servera v konfiguracii klienta.</summary>
    public string ServerName { get; }

    /// <summary>Zaciatok kluca.</summary>
    public string KeyPrefix { get; }

    /// <summary>Predvoleny port programu.</summary>
    public int DefaultPort { get; }

    /// <summary>Server sa spusti s programom.</summary>
    [XmlAttribute("Enabled")]
    public bool Enabled { get; set; }

    /// <summary>Agent smie menit otvorene data (nastroje so zmenami); inak len cita.</summary>
    [XmlAttribute("AllowWrites")]
    public bool AllowWrites { get; set; }

    /// <summary>Port na 127.0.0.1.</summary>
    [XmlAttribute("Port")]
    public int Port { get; set; }

    /// <summary>Pristupovy kluc sifrovany pre aktualneho pouzivatela Windows (DPAPI).</summary>
    [XmlAttribute("Key")]
    public string ProtectedKey { get; set; } = "";

    /// <summary>Pristupovy kluc (<c>Authorization: Bearer</c>); prazdny = este nevytvoreny alebo ulozeny inym pouzivatelom.</summary>
    [XmlIgnore]
    public string Key
    {
        get => UserSecret.Unprotect(ProtectedKey, _entropy);
        set => ProtectedKey = UserSecret.Protect(value, _entropy);
    }

    /// <summary>Adresa servera MCP.</summary>
    [XmlIgnore]
    public string Address => $"http://127.0.0.1:{Port}{McpServerHost.Pattern}";

    /// <summary>Kluc; ak ziadny nie je, vytvori novy.</summary>
    public string EnsureKey()
    {
        var key = Key;
        if (key.Length > 0)
            return key;
        Key = key = CreateKey();
        return key;
    }

    /// <summary>Novy nahodny kluc: 32 bajtov v base64url za <see cref="KeyPrefix" />.</summary>
    public string CreateKey() =>
        KeyPrefix + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>Kopia (rovnaky typ programu).</summary>
    public McpSettings Clone() => (McpSettings)MemberwiseClone();

    /// <summary>Rovnake nastavenie - pri zmene sa server spusti znova (aj povolenie zmien - nastroje sa registruju pri starte).</summary>
    public bool SameAs(McpSettings other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Enabled == other.Enabled && AllowWrites == other.AllowWrites && Port == other.Port && Key == other.Key;
    }
}
