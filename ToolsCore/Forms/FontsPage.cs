using ExControls;
using ToolsCore.Properties;
using ToolsCore.Tools;
using ToolsCore.XML;

namespace ToolsCore.Forms;

/// <summary>
/// Stranka Pisma v nastaveniach programu: pismo rozhrania, tabuliek a editora (editor len ak ho program ma),
/// kazde s volbou vlastneho pisma a nahladom.
/// </summary>
public partial class FontsPage : UserControl
{
    private readonly FontRow _interface;
    private readonly FontRow _tables;
    private readonly FontRow _editor;
    private List<string> _families = [];
    private List<string>? _monospaced;
    private bool _loading;

    /// <summary>
    /// Vytvori stranku.
    /// </summary>
    public FontsPage()
    {
        InitializeComponent();
        _interface = new FontRow(cboxInterfaceCustom, cbInterfaceFamily, nudInterfaceSize, lInterfacePreview);
        _tables = new FontRow(cboxTablesCustom, cbTablesFamily, nudTablesSize, lTablesPreview);
        _editor = new FontRow(cboxEditorCustom, cbEditorFamily, nudEditorSize, lEditorPreview);
        foreach (var row in Rows)
            row.Preview.Text = Resources.Fonts_Preview;
    }

    private IEnumerable<FontRow> Rows => [_interface, _tables, _editor];

    /// <summary>
    /// Zobrazi pisma z nastaveni.
    /// </summary>
    /// <param name="fonts">pisma z nastaveni</param>
    /// <param name="editorHint">popis pouzitia pisma editora; <see langword="null" /> = program editor nema</param>
    public void LoadFonts(AppFonts fonts, string? editorHint)
    {
        _loading = true;
        _families = Utils.GetSystemFontNames();
        cbInterfaceFamily.DataSource = _families.ToList();
        cbTablesFamily.DataSource = _families.ToList();

        gbEditor.Visible = editorHint is not null;
        if (editorHint is not null)
        {
            lEditorHint.Text = editorHint;
            var editor = fonts.Editor ?? AppFonts.DefaultEditor;
            cboxMonospaced.Checked = Monospaced().Contains(editor.Family, StringComparer.OrdinalIgnoreCase);
            cbEditorFamily.DataSource = (cboxMonospaced.Checked ? Monospaced() : _families).ToList();
            _editor.Load(fonts.Editor, AppFonts.DefaultEditor);
        }

        _interface.Load(fonts.Interface, FontChoice.Of(AppFonts.DefaultInterface));
        _tables.Load(fonts.Tables, _interface.Effective);
        _loading = false;
        foreach (var row in Rows)
            row.RefreshPreview();
    }

    /// <summary>
    /// Zapise zvolene pisma do nastaveni.
    /// </summary>
    public void SaveTo(AppFonts fonts)
    {
        fonts.Interface = _interface.Value;
        fonts.Tables = _tables.Value;
        if (gbEditor.Visible)
            fonts.Editor = _editor.Value;
    }

    private List<string> Monospaced()
    {
        if (_monospaced is not null)
            return _monospaced;

        using var graphics = CreateGraphics();
        _monospaced = [];
        foreach (var family in _families)
        {
            using var font = new Font(family, 12);
            if (Utils.IsFontMonospaced(graphics, font))
                _monospaced.Add(family);
        }

        return _monospaced;
    }

    private FontRow RowOf(object? sender) => Rows.First(r => r.Owns(sender));

    private void CboxCustom_CheckedChanged(object? sender, EventArgs e)
    {
        if (_loading)
            return;

        var row = RowOf(sender);
        row.CustomChanged();
        OnRowChanged(row);
    }

    private void FontControl_Changed(object? sender, EventArgs e)
    {
        if (_loading)
            return;

        var row = RowOf(sender);
        row.RefreshPreview();
        OnRowChanged(row);
    }

    private void OnRowChanged(FontRow row)
    {
        // tabulky bez vlastneho pisma maju pismo rozhrania
        if (row == _interface)
            _tables.SetDefault(_interface.Effective);
    }

    private void CboxMonospaced_CheckedChanged(object? sender, EventArgs e)
    {
        if (_loading)
            return;

        var selected = _editor.Effective;
        _loading = true;
        cbEditorFamily.DataSource = (cboxMonospaced.Checked ? Monospaced() : _families).ToList();
        _loading = false;
        _editor.Show(selected);
        _editor.RefreshPreview();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            foreach (var row in Rows)
                row.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Jedno pismo na stranke: vlastne pismo ano/nie, rodina, velkost a nahlad.
    /// </summary>
    private sealed class FontRow(ExCheckBox custom, ExComboBox family, ExNumericUpDown size, Label preview) : IDisposable
    {
        private FontChoice _default = AppFonts.DefaultEditor;
        private Font? _previewFont;

        public Label Preview => preview;

        /// <summary>
        /// Vlastne pismo; <see langword="null" /> = predvolene.
        /// </summary>
        public FontChoice? Value => custom.Checked && family.SelectedItem is string name ? new FontChoice(name, (float)size.Value) : null;

        /// <summary>
        /// Pismo, ktore sa pouzije.
        /// </summary>
        public FontChoice Effective => Value ?? _default;

        public bool Owns(object? control) => control == custom || control == family || control == size;

        public void Load(FontChoice? value, FontChoice defaultChoice)
        {
            _default = defaultChoice;
            custom.Checked = value is not null;
            Show(value ?? defaultChoice);
            RefreshEnabled();
        }

        public void SetDefault(FontChoice defaultChoice)
        {
            _default = defaultChoice;
            if (custom.Checked)
                return;

            Show(defaultChoice);
            RefreshPreview();
        }

        public void CustomChanged()
        {
            // po zruseni vlastneho pisma ukaze predvolene; vlastne pismo ma celu velkost (systemove napr. 8,25)
            if (!custom.Checked)
                Show(_default);
            else
                size.Value = Math.Clamp(Math.Round(size.Value), size.Minimum, size.Maximum);
            RefreshEnabled();
            RefreshPreview();
        }

        public void Show(FontChoice choice)
        {
            if (family.DataSource is List<string> names && !names.Contains(choice.Family, StringComparer.OrdinalIgnoreCase))
            {
                names.Insert(0, choice.Family);
                family.DataSource = null;
                family.DataSource = names;
            }

            family.SelectedItem = (family.DataSource as List<string>)?.FirstOrDefault(n => string.Equals(n, choice.Family, StringComparison.OrdinalIgnoreCase));
            size.Value = Math.Clamp((decimal)choice.Size, size.Minimum, size.Maximum);
        }

        public void RefreshPreview()
        {
            var old = _previewFont;
            _previewFont = Effective.ToFont();
            preview.Font = _previewFont;
            old?.Dispose();
        }

        private void RefreshEnabled() => family.Enabled = size.Enabled = custom.Checked;

        public void Dispose() => _previewFont?.Dispose();
    }
}
