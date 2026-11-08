namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Text s pismom - medzivysledok skladania textu stlpca.
/// </summary>
/// <param name="Text">Text.</param>
/// <param name="Font">Cislo pisma; <see langword="null"/> = nezmenene, <see cref="TabTabText.DefaultFont"/> = predvolene.</param>
public readonly record struct TabTabValue(string Text, int? Font)
{
    /// <summary>Prazdny text bez pisma.</summary>
    public static readonly TabTabValue Empty = new("", null);

    /// <summary>Ci text obsahuje <c>@</c> (odklad na dalsi zdroj).</summary>
    public bool HasPlaceholder => Text.Contains('@');

    /// <summary>Dosadi <paramref name="inner"/> za kazde <c>@</c>; pismo ostava vonkajsie, ak je zadane.</summary>
    public TabTabValue Fill(TabTabValue inner) => new(Text.Replace("@", inner.Text), Font ?? inner.Font);
}
