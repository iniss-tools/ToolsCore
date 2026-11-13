namespace ToolsCore.Iniss2.Xml;

/// <summary>
/// Poloha prvku v texte suboru XML.
/// </summary>
internal sealed class XmlSpanNode
{
    /// <summary>Index znaku <c>&lt;</c> zaciatocnej znacky.</summary>
    public int Start { get; init; }

    /// <summary>Index za znakom <c>&gt;</c> zaciatocnej znacky.</summary>
    public int StartTagEnd { get; init; }

    /// <summary>Prvok je zapisany ako prazdny (<c>&lt;a /&gt;</c>).</summary>
    public bool IsEmpty { get; init; }

    /// <summary>Index znaku <c>&lt;</c> koncovej znacky; pri prazdnom prvku <see cref="StartTagEnd" />.</summary>
    public int ContentEnd { get; set; }

    /// <summary>Index za koncom prvku.</summary>
    public int End { get; set; }

    /// <summary>Vnorene prvky v poradi textu.</summary>
    public List<XmlSpanNode> Children { get; } = [];

    /// <summary>Index za zaciatocnou znackou, kde zacina obsah prvku.</summary>
    public int ContentStart => StartTagEnd;
}
