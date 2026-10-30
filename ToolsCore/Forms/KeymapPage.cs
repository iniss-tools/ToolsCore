using System.Globalization;
using ToolsCore.Commands;
using ToolsCore.Properties;
using ToolsCore.XML;

namespace ToolsCore.Forms;

/// <summary>
/// Stranka klavesovych skratiek v nastaveniach programu: strom prikazov podla ponuky okna so skratkami vpravo,
/// hladanie podla nazvu aj stlacenim skratky, len zmenene skratky a konflikty v ramci jedneho okna.
/// </summary>
public partial class KeymapPage : UserControl
{
    private const string PathSeparator = " › ";

    private IReadOnlyList<KeymapNode> _roots = [];
    private ShortcutMap _map = new();
    private Keys? _filterKeys;
    private Font? _boldFont;

    /// <summary>
    /// Vytvori stranku.
    /// </summary>
    public KeymapPage()
    {
        InitializeComponent();
        tbFilter.CaptureKeys = false;
        tbFilter.HintText = Resources.Keymap_SearchHint;
        sbShortcut.HintText = Resources.Keymap_PressKeys;
    }

    /// <summary>
    /// Skratky upravovane strankou (zmeny sa zapisuju priamo sem).
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ShortcutMap Map => _map;

    /// <summary>
    /// Naplni stranku stromom prikazov a skratkami, ktore bude upravovat.
    /// </summary>
    public void LoadCommands(IReadOnlyList<KeymapNode> roots, ShortcutMap map)
    {
        _roots = roots;
        _map = map;
        RebuildTree(null);
    }

    private IEnumerable<CommandInfo> AllCommands => _roots.SelectMany(r => r.Commands());

    private bool IsChanged(CommandInfo command) => _map.Get(command) != command.DefaultKeys;

    // ostatne prikazy toho isteho okna s rovnakou skratkou
    private List<CommandInfo> ConflictsOf(CommandInfo command, Keys keys) =>
        keys == Keys.None
            ? []
            : AllCommands.Where(c => c.Id != command.Id && c.Window == command.Window && _map.Get(c) == keys).ToList();

    #region Strom

    private void RebuildTree(string? selectId)
    {
        selectId ??= (tvCommands.SelectedNode?.Tag as KeymapNode)?.Command?.Id;
        var text = _filterKeys is null ? tbFilter.Text.Trim() : "";
        var filtering = text.Length > 0 || _filterKeys is not null || cboxChangedOnly.Checked;

        tvCommands.BeginUpdate();
        tvCommands.Nodes.Clear();
        foreach (var root in _roots)
        {
            if (CreateNode(root, text, false) is { } node)
                tvCommands.Nodes.Add(node);
        }

        if (filtering)
            tvCommands.ExpandAll();
        else
            foreach (TreeNode root in tvCommands.Nodes)
            {
                root.Expand();
                foreach (TreeNode menu in root.Nodes)
                    menu.Expand();
            }

        tvCommands.SelectedNode = FindNode(tvCommands.Nodes, selectId) ?? FirstCommandNode(tvCommands.Nodes);
        tvCommands.SelectedNode?.EnsureVisible();
        tvCommands.EndUpdate();
        UpdateDetail();
    }

    // uzol prejde filtrom, ak mu zodpoveda sam alebo niektory potomok; skupina zodpovedajuca nazvom ukaze vsetky prikazy
    private TreeNode? CreateNode(KeymapNode node, string text, bool parentMatches)
    {
        var matches = parentMatches || MatchesText(node, text);
        var children = node.Children.Select(child => CreateNode(child, text, matches && node.Command is null)).OfType<TreeNode>().ToList();
        var own = node.Command is not null && matches && MatchesKeys(node.Command) && (!cboxChangedOnly.Checked || IsChanged(node.Command));
        if (!own && children.Count == 0)
            return null;

        var treeNode = new TreeNode(node.Text) { Tag = node };
        treeNode.Nodes.AddRange(children.ToArray());
        return treeNode;
    }

    private static bool MatchesText(KeymapNode node, string text) =>
        text.Length == 0
        || node.Text.Contains(text, StringComparison.CurrentCultureIgnoreCase)
        || (node.Command is { } command && command.Text.Contains(text, StringComparison.CurrentCultureIgnoreCase));

    private bool MatchesKeys(CommandInfo command) => _filterKeys is not { } keys || _map.Get(command) == keys;

    private static TreeNode? FindNode(System.Collections.IEnumerable nodes, string? id)
    {
        if (id is null)
            return null;
        foreach (TreeNode node in nodes)
        {
            if ((node.Tag as KeymapNode)?.Command?.Id == id)
                return node;
            if (FindNode(node.Nodes, id) is { } found)
                return found;
        }

        return null;
    }

    private static TreeNode? FirstCommandNode(System.Collections.IEnumerable nodes)
    {
        foreach (TreeNode node in nodes)
        {
            if ((node.Tag as KeymapNode)?.Command is not null)
                return node;
            if (FirstCommandNode(node.Nodes) is { } found)
                return found;
        }

        return null;
    }

    private void TvCommands_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
        if (e.Node?.Tag is not KeymapNode node || e.Bounds.IsEmpty)
        {
            e.DrawDefault = true;
            return;
        }

        var selected = (e.State & TreeNodeStates.Selected) != 0;
        var highlight = GlobSettings.UsingStyle?.ControlsColorScheme.Highlight;
        var back = selected ? highlight?.BackColor ?? SystemColors.Highlight : tvCommands.BackColor;
        var fore = selected ? highlight?.ForeColor ?? SystemColors.HighlightText : tvCommands.ForeColor;
        var row = new Rectangle(e.Bounds.X, e.Bounds.Y, Math.Max(e.Bounds.Width, tvCommands.ClientSize.Width - e.Bounds.X), e.Bounds.Height);
        using (var brush = new SolidBrush(back))
            e.Graphics.FillRectangle(brush, row);

        var command = node.Command;
        var font = command is not null && IsChanged(command) ? _boldFont ??= new Font(tvCommands.Font, FontStyle.Bold) : tvCommands.Font;
        const TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
        TextRenderer.DrawText(e.Graphics, node.Text, font, row, fore, flags | TextFormatFlags.Left);

        if (command is null || _map.Get(command) is not (var keys and not Keys.None))
            return;

        var keysColor = ConflictsOf(command, keys).Count > 0 ? WarningColor(back) : selected ? fore : Blend(fore, back);
        var keysRow = row with { Width = row.Width - 8 };
        TextRenderer.DrawText(e.Graphics, KeyNames.Display(keys), tvCommands.Font, keysRow, keysColor, flags | TextFormatFlags.Right);
    }

    private static Color Blend(Color a, Color b) => Color.FromArgb((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2);

    private static Color WarningColor(Color back) =>
        back.GetBrightness() < 0.5f ? Color.FromArgb(255, 120, 110) : Color.FromArgb(196, 43, 28);

    private void TvCommands_AfterSelect(object? sender, TreeViewEventArgs e) => UpdateDetail();

    private void TvCommands_Resize(object? sender, EventArgs e) => tvCommands.Invalidate();

    #endregion

    #region Filtre

    private void TbFilter_TextChanged(object? sender, EventArgs e)
    {
        if (_filterKeys is null)
            RebuildTree(null);
    }

    private void TbFilter_KeysCaptured(object? sender, KeysEventArgs e)
    {
        if (!cboxByKeys.Checked)
            return;

        // Esc a Backspace filter zrusia
        _filterKeys = e.Keys is Keys.Escape or Keys.Back ? null : e.Keys;
        tbFilter.Text = _filterKeys is { } keys ? KeyNames.Display(keys) : "";
        RebuildTree(null);
    }

    private void CboxByKeys_CheckedChanged(object? sender, EventArgs e)
    {
        _filterKeys = null;
        tbFilter.CaptureKeys = cboxByKeys.Checked;
        tbFilter.HintText = cboxByKeys.Checked ? Resources.Keymap_PressKeys : Resources.Keymap_SearchHint;
        tbFilter.Text = "";
        RebuildTree(null);
        tbFilter.Focus();
    }

    private void CboxChangedOnly_CheckedChanged(object? sender, EventArgs e) => RebuildTree(null);

    private void BResetAll_Click(object? sender, EventArgs e)
    {
        foreach (var command in AllCommands)
            _map.Reset(command.Id);
        RebuildTree(null);
    }

    #endregion

    #region Detail

    private CommandInfo? SelectedCommand => (tvCommands.SelectedNode?.Tag as KeymapNode)?.Command;

    private void UpdateDetail(string? message = null)
    {
        var command = SelectedCommand;
        pDetail.Enabled = command is not null;
        lCommand.Text = tvCommands.SelectedNode is { } selected ? PathOf(selected) : "";
        if (command is null)
        {
            sbShortcut.Text = "";
            llDefault.Visible = false;
            lConflict.Visible = llRemoveConflict.Visible = false;
            return;
        }

        var keys = _map.Get(command);
        sbShortcut.Text = keys == Keys.None ? "" : KeyNames.Display(keys);
        bRemove.Enabled = keys != Keys.None;
        llDefault.Visible = IsChanged(command);
        llDefault.Text = string.Format(CultureInfo.CurrentCulture, Resources.Keymap_RestoreDefault, KeyNames.Display(command.DefaultKeys));

        var conflicts = ConflictsOf(command, keys);
        if (message is not null)
        {
            lConflict.Text = message;
            lConflict.Visible = true;
            llRemoveConflict.Visible = false;
        }
        else if (conflicts.Count > 0)
        {
            lConflict.Text = string.Format(CultureInfo.CurrentCulture, Resources.Keymap_Conflict,
                string.Join(", ", conflicts.Select(c => PathOf(FindNode(tvCommands.Nodes, c.Id)) ?? c.Text)));
            lConflict.Visible = llRemoveConflict.Visible = true;
            llRemoveConflict.Left = lConflict.Right + 4;
        }
        else
        {
            lConflict.Visible = llRemoveConflict.Visible = false;
        }

        lConflict.ForeColor = WarningColor(pDetail.BackColor);
    }

    // cesta bez korena okna, ak je v strome len hlavne okno
    private string? PathOf(TreeNode? node)
    {
        if (node is null)
            return null;
        var parts = new List<string>();
        for (var n = node; n is not null; n = n.Parent)
            parts.Insert(0, n.Text);
        if (_roots.Count == 1 && parts.Count > 1)
            parts.RemoveAt(0);
        return string.Join(PathSeparator, parts);
    }

    private void SbShortcut_KeysCaptured(object? sender, KeysEventArgs e)
    {
        if (SelectedCommand is not { } command)
            return;

        if (e.Keys == Keys.Escape)
        {
            tvCommands.Focus();
            return;
        }

        if (!KeyNames.IsValid(e.Keys))
        {
            UpdateDetail(string.Format(CultureInfo.CurrentCulture, Resources.Keymap_Invalid, KeyNames.Display(e.Keys)));
            return;
        }

        SetShortcut(command, e.Keys);
    }

    private void SetShortcut(CommandInfo command, Keys keys)
    {
        if (keys == command.DefaultKeys)
            _map.Reset(command.Id);
        else
            _map.Set(command.Id, keys);
        if (cboxChangedOnly.Checked || _filterKeys is not null)
            RebuildTree(command.Id);
        else
        {
            tvCommands.Invalidate();
            UpdateDetail();
        }
    }

    private void BRemove_Click(object? sender, EventArgs e)
    {
        if (SelectedCommand is { } command)
            SetShortcut(command, Keys.None);
    }

    private void LlDefault_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (SelectedCommand is { } command)
            SetShortcut(command, command.DefaultKeys);
    }

    private void LlRemoveConflict_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (SelectedCommand is not { } command)
            return;
        foreach (var other in ConflictsOf(command, _map.Get(command)))
            _map.Set(other.Id, Keys.None);
        tvCommands.Invalidate();
        UpdateDetail();
    }

    private void TvCommands_KeyDown(object? sender, KeyEventArgs e)
    {
        if (SelectedCommand is null)
            return;
        switch (e.KeyCode)
        {
            case Keys.Enter:
                sbShortcut.Focus();
                e.Handled = e.SuppressKeyPress = true;
                break;
            case Keys.Delete:
                BRemove_Click(sender, e);
                e.Handled = true;
                break;
        }
    }

    #endregion

    /// <inheritdoc />
    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        _boldFont?.Dispose();
        _boldFont = null;
        lCommand.Font = new Font(Font, FontStyle.Bold);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _boldFont?.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }
}
