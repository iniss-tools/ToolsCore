namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Usek textu sekcie (fyzicke pozicie v texte, ako ho drzi editor).
/// </summary>
/// <param name="Start">Index prveho znaku.</param>
/// <param name="Length">Dlzka.</param>
public readonly record struct TabTabSpan(int Start, int Length)
{
    /// <summary>Index za poslednym znakom.</summary>
    public int End => Start + Length;

    /// <summary>Prazdny usek na danej pozicii.</summary>
    public static TabTabSpan At(int pos) => new(pos, 0);
}
