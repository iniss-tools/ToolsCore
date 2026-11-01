using System.Globalization;
using ToolsCore.Icons;
using ToolsCore.Properties;
using ToolsCore.Tools;
using ToolsCore.XML;

namespace ToolsCore.Forms;

/// <summary>
/// Stranka farebnej schemy v nastaveniach programu: vyber schemy s ponukou (duplikovat, premenovat, odstranit,
/// obnovit), strom poloziek podla kategorii, Tucne/Popredie/Pozadie so zdedenim zo zakladu schemy, upozornenie na
/// nizky kontrast a nahlad kategorie (klik vyberie polozku).
/// </summary>
public partial class ColorSchemePage : UserControl
{
    private readonly Dictionary<string, IColorPreview> _previews = [];
    private IStyleSet _styles = null!;
    private Style? _style;
    private IColorPreview? _preview;
    private Font? _boldFont;
    private bool _loading;

    /// <summary>
    /// Vytvori stranku.
    /// </summary>
    public ColorSchemePage()
    {
        InitializeComponent();
        OwnerDrawnTree.SelectOnRowClick(tvKeys);
        // variant ikony podla pozadia - po zmene schemy tlacidlom Pouzit sa vymeni
        picContrast.BackColorChanged += (_, _) => picContrast.Image = IconImages.For(picContrast, "status-warning");
    }

    /// <summary>
    /// Pribudla, zanikla alebo sa premenovala schema (zoznamy schem na inych strankach treba obnovit).
    /// </summary>
    public event EventHandler? SchemesChanged;

    /// <summary>
    /// Zobrazi schemy.
    /// </summary>
    /// <param name="styles">schemy (kopia pre okno nastaveni)</param>
    /// <param name="selected">schema vybrata na zaciatku</param>
    public void LoadSchemes(IStyleSet styles, Style selected)
    {
        _styles = styles;
        lKey.Font = _boldFont ??= new Font(Font, FontStyle.Bold);
        picContrast.Image = IconImages.For(picContrast, "status-warning");
        RefreshSchemes(selected);
    }

    /// <summary>
    /// Vyberie schemu (napr. po zmene na stranke Vzhlad).
    /// </summary>
    public void SelectScheme(Style style)
    {
        if (!ReferenceEquals(tscbScheme.SelectedItem, style))
            tscbScheme.SelectedItem = style;
    }

    private Style Style => _style!;

    private ColorKey? SelectedKey => tvKeys.SelectedNode?.Tag as ColorKey;

    #region Schemy

    private void RefreshSchemes(Style select)
    {
        _loading = true;
        tscbScheme.ComboBox.DataSource = null;
        tscbScheme.ComboBox.DataSource = _styles.Cast<Style>().ToList();
        tscbScheme.SelectedItem = select;
        _loading = false;
        ShowScheme(select);
    }

    private void TscbScheme_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_loading && tscbScheme.SelectedItem is Style style)
            ShowScheme(style);
    }

    private void ShowScheme(Style style)
    {
        var selectedId = SelectedKey?.Id ?? (tvKeys.SelectedNode?.Tag as ColorCategory)?.Id;
        _style = style;
        tsmiRename.Enabled = tsmiDelete.Enabled = !style.IsBuiltIn;
        lInherit.Text = string.Format(CultureInfo.CurrentCulture, Resources.Scheme_InheritHint,
            style.IsDark ? Resources.Scheme_Dark : Resources.Scheme_Light);

        tvKeys.BeginUpdate();
        tvKeys.Nodes.Clear();
        TreeNode? select = null;
        foreach (var group in style.ColorKeys.GroupBy(k => k.Category))
        {
            var category = new TreeNode(group.Key.Name()) { Tag = group.Key };
            if (group.Key.Id == selectedId)
                select = category;
            foreach (var key in group)
            {
                var node = new TreeNode(key.Name) { Tag = key };
                category.Nodes.Add(node);
                if (key.Id == selectedId)
                    select = node;
            }

            tvKeys.Nodes.Add(category);
        }

        tvKeys.ExpandAll();
        tvKeys.EndUpdate();
        tvKeys.SelectedNode = select ?? (tvKeys.Nodes.Count > 0 ? tvKeys.Nodes[0] : null);
        tvKeys.SelectedNode?.EnsureVisible();
        _preview?.ShowScheme(style);
    }

    private void TsmiDuplicate_Click(object? sender, EventArgs e)
    {
        if (AskName(Style.DisplayName) is not { } name)
            return;

        var copy = Style.Copy(name);
        _styles.Add(copy);
        RefreshSchemes(copy);
        SchemesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TsmiRename_Click(object? sender, EventArgs e)
    {
        if (Style.IsBuiltIn || AskName(Style.Name) is not { } name)
            return;

        var old = Style.Name;
        Style.Name = name;
        if (_styles.Active == old)
            _styles.Active = name;
        if (_styles.LightScheme == old)
            _styles.LightScheme = name;
        if (_styles.DarkScheme == old)
            _styles.DarkScheme = name;
        RefreshSchemes(Style);
        SchemesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TsmiDelete_Click(object? sender, EventArgs e)
    {
        if (Style.IsBuiltIn)
            return;

        var question = string.Format(CultureInfo.CurrentCulture, Resources.Scheme_DeleteConfirm, Style.DisplayName);
        if (Utils.ShowQuestion(question) != DialogResult.Yes)
            return;

        var name = Style.Name;
        _styles.Remove(Style);
        if (_styles.Active == name)
            _styles.Active = StyleNames.Light;
        if (_styles.LightScheme == name)
            _styles.LightScheme = StyleNames.Light;
        if (_styles.DarkScheme == name)
            _styles.DarkScheme = StyleNames.Dark;
        RefreshSchemes(_styles[0]);
        SchemesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TsmiResetScheme_Click(object? sender, EventArgs e)
    {
        var question = string.Format(CultureInfo.CurrentCulture, Resources.Scheme_ResetConfirm, Style.DisplayName);
        if (Utils.ShowQuestion(question) != DialogResult.Yes)
            return;

        Style.Overrides.Clear();
        Style.Apply();
        ShowScheme(Style);
    }

    private string? AskName(string initial)
    {
        // nazov nesmie byt zhodny s inou schemou ani s prekladom zabudovanej
        using var form = new FInputBox(_styles, (o, v) => v == ((Style)o).Name || v == ((Style)o).DisplayName, initial);
        return form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(form.NewValue) ? form.NewValue.Trim() : null;
    }

    #endregion

    #region Strom a nahlad

    private void TvKeys_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        var key = SelectedKey;
        ShowPreview(key?.Category ?? tvKeys.SelectedNode?.Tag as ColorCategory);
        ShowDetail(key);
    }

    private void TvKeys_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
        if (e.Node is null || e.Bounds.IsEmpty || _style is null)
            return;

        var selected = (e.State & TreeNodeStates.Selected) != 0;
        var highlight = GlobSettings.UsingStyle?.ControlsColorScheme.Highlight;
        var back = selected ? highlight?.BackColor ?? SystemColors.Highlight : tvKeys.BackColor;
        var fore = selected ? highlight?.ForeColor ?? SystemColors.HighlightText : tvKeys.ForeColor;
        var row = OwnerDrawnTree.Row(tvKeys, e.Bounds);
        using (var brush = new SolidBrush(back))
            e.Graphics.FillRectangle(brush, row);

        // kategorie a zmenene polozky tucne
        var bold = e.Node.Tag is not ColorKey key || Style.Overrides.ContainsKey(key.Id);
        var font = bold ? _boldFont ??= new Font(tvKeys.Font, FontStyle.Bold) : tvKeys.Font;
        TextRenderer.DrawText(e.Graphics, e.Node.Text, font, Rectangle.FromLTRB(e.Bounds.X, row.Top, row.Right, row.Bottom), fore,
            TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.Left);
    }

    private void ShowPreview(ColorCategory? category)
    {
        IColorPreview? preview = null;
        if (category?.CreatePreview is { } create && !_previews.TryGetValue(category.Id, out preview))
        {
            preview = create();
            preview.Control.Dock = DockStyle.Fill;
            preview.Control.Font = Font;
            preview.KeyClicked += Preview_KeyClicked;
            _previews[category.Id] = preview;
            pPreview.Controls.Add(preview.Control);
        }

        foreach (var p in _previews.Values)
            p.Control.Visible = p == preview;
        _preview = preview;
        pPreview.Visible = preview is not null;
        if (_style is not null)
            preview?.ShowScheme(_style);
    }

    private void Preview_KeyClicked(object? sender, string id)
    {
        var node = tvKeys.Nodes.Cast<TreeNode>().SelectMany(n => n.Nodes.Cast<TreeNode>()).FirstOrDefault(n => (n.Tag as ColorKey)?.Id == id);
        if (node is not null)
            tvKeys.SelectedNode = node;
    }

    #endregion

    #region Polozka

    private void ShowDetail(ColorKey? key)
    {
        _loading = true;
        // pri kategorii len jej nazov
        foreach (Control control in pDetail.Controls)
            control.Visible = key is not null || control == lKey;

        if (key is null)
        {
            lKey.Text = (tvKeys.SelectedNode?.Tag as ColorCategory)?.Name() ?? "";
            _loading = false;
            return;
        }

        var value = Style.ValueOf(key);
        var o = Style.OverrideOf(key);
        lKey.Text = key.Name;
        // casti, ktore polozka nema, su vidno, ale zakazane - panel nevyzera poloprazdny
        cboxBold.Enabled = key.Editable.HasFlag(ColorParts.Bold);
        cboxBold.Checked = value.Bold;
        cboxFore.Enabled = key.Editable.HasFlag(ColorParts.Fore);
        cboxFore.Checked = o?.Fore is not null;
        csFore.SelectedColor = value.Fore;
        csFore.Enabled = cboxFore.Enabled && cboxFore.Checked;
        cboxBack.Enabled = key.Editable.HasFlag(ColorParts.Back);
        cboxBack.Checked = o?.Back is not null;
        csBack.SelectedColor = value.Back;
        csBack.Enabled = cboxBack.Enabled && cboxBack.Checked;
        // zakazany LinkLabel je v tmavej teme necitatelny - odkaz len pri zmenenej polozke
        llReset.Visible = o is not null;
        _loading = false;
        UpdateContrast(key);
    }

    private void Detail_Changed(object? sender, EventArgs e)
    {
        if (_loading || SelectedKey is not { } key)
            return;

        var defaults = Style.DefaultOf(key);
        _loading = true;
        // bez zaskrtnutia ukaze farbu zo zakladu; zaskrtnutie zacne od nepriehladnej farby
        if (sender == cboxFore)
            csFore.SelectedColor = cboxFore.Checked ? Opaque(csFore.SelectedColor, key) : defaults.Fore;
        if (sender == cboxBack)
            csBack.SelectedColor = cboxBack.Checked ? Opaque(csBack.SelectedColor, key) : defaults.Back;
        csFore.Enabled = cboxFore.Enabled && cboxFore.Checked;
        csBack.Enabled = cboxBack.Enabled && cboxBack.Checked;
        _loading = false;

        var o = new ColorOverride(
            cboxFore.Checked ? csFore.SelectedColor : null,
            cboxBack.Checked ? csBack.SelectedColor : null,
            key.Editable.HasFlag(ColorParts.Bold) && cboxBold.Checked != defaults.Bold ? cboxBold.Checked : null);
        Style.SetOverride(key, o);
        llReset.Visible = !o.IsEmpty;
        tvKeys.Invalidate();
        _preview?.ShowScheme(Style);
        UpdateContrast(key);
    }

    private Color Opaque(Color color, ColorKey key) =>
        color.A == 255 ? color : ContextBack(key) ?? Style.ValueOf(UiColors.Box).Back;

    private void LlReset_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (SelectedKey is not { } key)
            return;

        Style.SetOverride(key, null);
        ShowDetail(key);
        tvKeys.Invalidate();
        _preview?.ShowScheme(Style);
    }

    /// <summary>
    /// Pozadie pod textom polozky bez vlastneho pozadia (napr. editor, tabulka).
    /// </summary>
    private Color? ContextBack(ColorKey key) =>
        key.ContrastWith is { } id && Style.ColorKeys.FirstOrDefault(k => k.Id == id) is { } context ? Style.ValueOf(context).Back : null;

    private void UpdateContrast(ColorKey key)
    {
        var value = Style.ValueOf(key);
        var back = key.Editable.HasFlag(ColorParts.Back) && value.Back.A == 255 ? value.Back : ContextBack(key);
        var low = back is { } b && key.Editable.HasFlag(ColorParts.Fore) && ColorContrast.Ratio(value.Fore, b) < ColorContrast.MinimumText;
        picContrast.Visible = lContrast.Visible = low;
        if (!low)
            return;

        // orezat, nie zaokruhlit - 4,47 nesmie vyzerat ako odporucane minimum 4,5
        var ratio = Math.Floor(ColorContrast.Ratio(value.Fore, back!.Value) * 10) / 10;
        lContrast.Text = string.Format(CultureInfo.CurrentCulture, Resources.Scheme_LowContrast,
            ratio.ToString("0.0", CultureInfo.CurrentCulture), ColorContrast.MinimumText.ToString("0.0", CultureInfo.CurrentCulture));
    }

    #endregion

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _boldFont?.Dispose();
        }

        base.Dispose(disposing);
    }
}
