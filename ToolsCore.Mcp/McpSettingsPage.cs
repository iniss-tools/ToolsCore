using System.ComponentModel;
using System.Globalization;
using ToolsCore.Mcp.Properties;
using ToolsCore.Tools;

namespace ToolsCore.Mcp;

/// <summary>
/// Stranka nastaveni Agenti (MCP): zapnutie servera, port so stavom, kopirovanie konfiguracie pre Claude Code a VS Code alebo
/// adresy a kluca pre iny klient MCP, novy kluc a povolenie zmien. Program ju vlozi do svojho okna nastaveni, nastavi texty o svojich datach
/// (<see cref="HintText" />, <see cref="WritesText" />, <see cref="WritesHintText" />), pri otvoreni zavola
/// <see cref="LoadSettings" /> a pri ulozeni <see cref="SaveSettings" />.
/// </summary>
public partial class McpSettingsPage : UserControl
{
    private McpSettings? _settings;
    private IMcpServerStatus? _server;

    public McpSettingsPage()
    {
        InitializeComponent();
    }

    /// <summary>Uvodny text stranky - k akym datam sa agent pripoji.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string HintText
    {
        get => lHint.Text;
        set => lHint.Text = value;
    }

    /// <summary>Text volby povolenia zmien.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string WritesText
    {
        get => cboxWrites.Text;
        set => cboxWrites.Text = value;
    }

    /// <summary>Vysvetlenie povolenia zmien.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string WritesHintText
    {
        get => lWritesHint.Text;
        set => lWritesHint.Text = value;
    }

    /// <summary>
    /// Nastavenie programu do stranky. Novy kluc meni <paramref name="settings" /> hned, ostatne hodnoty az
    /// <see cref="SaveSettings" />.
    /// </summary>
    /// <param name="settings">nastavenie servera v konfiguracii programu</param>
    /// <param name="server">beziaci server - jeho stav sa ukaze na stranke; null = bez stavu (harness snimok)</param>
    public void LoadSettings(McpSettings settings, IMcpServerStatus? server)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _server = server;
        cboxEnabled.Checked = settings.Enabled;
        cboxWrites.Checked = settings.AllowWrites;
        nudPort.Value = Math.Clamp(settings.Port, (int)nudPort.Minimum, (int)nudPort.Maximum);
        UpdateStatus();
    }

    /// <summary>Hodnoty stranky do nastavenia programu; zapnuty server dostane kluc, ak ho este nema.</summary>
    public void SaveSettings()
    {
        if (_settings is not { } settings)
            return;
        settings.Enabled = cboxEnabled.Checked;
        settings.AllowWrites = cboxWrites.Checked;
        settings.Port = decimal.ToInt32(nudPort.Value);
        if (settings.Enabled)
            settings.EnsureKey();
    }

    /// <summary>Nastavenie servera zo stranky (kluc sa nemeni - ten meni len tlacidlo Novy kluc).</summary>
    private McpSettings? FromPage()
    {
        if (_settings?.Clone() is not { } settings)
            return null;
        settings.Enabled = cboxEnabled.Checked;
        settings.AllowWrites = cboxWrites.Checked;
        settings.Port = decimal.ToInt32(nudPort.Value);
        return settings;
    }

    /// <summary>Stranka sa lisi od nastavenia, s ktorym server bezi - prejavi sa az po ulozeni.</summary>
    private bool Pending => FromPage() is { } page && _settings is { } settings && !page.SameAs(_server?.Settings ?? settings);

    private void UpdateStatus()
    {
        bClaude.Enabled = bVsCode.Enabled = bAddress.Enabled = cboxWrites.Enabled = cboxEnabled.Checked;
        lStatus.Text = Pending ? Resources.Mcp_Pending
            : _server?.Address is { } address ? string.Format(CultureInfo.CurrentCulture, Resources.Mcp_Running, address)
            : _server?.Error is { } error ? string.Format(CultureInfo.CurrentCulture, Resources.Mcp_Failed, error)
            : Resources.Mcp_Off;
    }

    private void Setting_Changed(object? sender, EventArgs e) => UpdateStatus();

    /// <summary>Konfiguracia pre klienta do schranky; kluc sa vytvori, ak este nie je.</summary>
    private void CopyConfig(Func<string, string, string, string> text)
    {
        if (_settings is not { } settings || FromPage() is not { } page)
            return;
        var key = settings.EnsureKey();
        Clipboard.SetText(text(settings.ServerName, page.Address, key));
        lStatus.Text = Pending ? Resources.Mcp_CopiedPending : Resources.Mcp_Copied;
    }

    private void bClaude_Click(object? sender, EventArgs e) =>
        CopyConfig((name, address, key) => $"claude mcp add --transport http {name} {address} --header \"Authorization: Bearer {key}\"");

    // patri do pouzivatelskeho mcp.json VS Code (MCP: Open User Configuration), nie do priecinka projektu - je v nom kluc
    private void bVsCode_Click(object? sender, EventArgs e) =>
        CopyConfig((name, address, key) => $$"""
            {
              "servers": {
                "{{name}}": {
                  "type": "http",
                  "url": "{{address}}",
                  "headers": { "Authorization": "Bearer {{key}}" }
                }
              }
            }
            """);

    // iny klient MCP (Streamable HTTP): adresa servera a hlavicka s klucom, zapis podla jeho konfiguracie
    private void bAddress_Click(object? sender, EventArgs e) =>
        CopyConfig((_, address, key) => $"{address}{Environment.NewLine}Authorization: Bearer {key}");

    private void bNewKey_Click(object? sender, EventArgs e)
    {
        if (_settings is not { } settings)
            return;
        if (settings.Key.Length > 0 && Utils.ShowQuestion(Resources.Mcp_NewKey) != DialogResult.Yes)
            return;
        settings.Key = settings.CreateKey();
        UpdateStatus();
    }
}
