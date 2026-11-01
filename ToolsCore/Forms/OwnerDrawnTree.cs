namespace ToolsCore.Forms;

/// <summary>
/// Spolocne spravanie stromov s vlastnym kreslenim textu (<see cref="TreeViewDrawMode.OwnerDrawText" />) na celu
/// sirku riadka - stranky klavesovych skratiek a farebnej schemy. <c>FullRowSelect</c> sa pouzit neda: system potom
/// kresli vlavo od textu vlastnu plochu vyberu.
/// </summary>
internal static class OwnerDrawnTree
{
    /// <summary>
    /// Klik do prazdnej casti riadka za textom polozku vyberie (inak by vyberal len klik na text).
    /// </summary>
    public static void SelectOnRowClick(TreeView tree) =>
        tree.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Left)
                return;

            var hit = tree.HitTest(e.Location);
            if (hit.Node is not null && hit.Location == TreeViewHitTestLocations.RightOfLabel)
                tree.SelectedNode = hit.Node;
        };

    /// <summary>
    /// Riadok polozky od zaciatku textu po pravy okraj stromu. Zacina o 2 body skor - system pred kreslenim textu
    /// vyplni vyber o bod sirsi nez je obdlznik textu a ten by ostal vidno ako tenky pas inej farby.
    /// </summary>
    public static Rectangle Row(TreeView tree, Rectangle textBounds)
    {
        var x = Math.Max(0, textBounds.X - 2);
        return new Rectangle(x, textBounds.Y, Math.Max(textBounds.Right - x, tree.ClientSize.Width - x), textBounds.Height);
    }
}
