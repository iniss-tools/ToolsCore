using ToolsCore.Iniss.Expressions;

namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Vstup skladania textu jedneho stlpca katalogovej tabule.
/// </summary>
public sealed class TabTabColumnInput
{
    /// <summary>Vlak, pre ktory sa text sklada.</summary>
    public required IExprTrainContext Train { get; init; }

    /// <summary>Miesto vyhodnotenia (prichodova tabula alebo ine) - ovplyvnuje <c>VYLUKAZDE</c>, <c>ZPOZDENI</c> a pravidlo <c>#VYLUKA</c>.</summary>
    public ExprEvalSite Site { get; init; } = ExprEvalSite.Default;

    /// <summary>"Teraz" pre <c>DATE</c>/<c>TIME</c>.</summary>
    public DateTime? Now { get; init; }

    /// <summary>Hodnota, ktoru stlpec pocita sam (podla TYPE_ITEMS_IDX).</summary>
    public TabTabValue OwnValue { get; init; } = TabTabValue.Empty;

    /// <summary>Text z TTexts.txt pre tento stlpec a vlak; <see langword="null"/>, ak nie je.</summary>
    public TabTabValue? TextsValue { get; init; }

    /// <summary>TYPE_ITEMS_DIVTYPE stlpca (0-4).</summary>
    public int DivType { get; init; }

    /// <summary>TYPE_ITEMS_IDX stlpca - pri 3 az 5 (smer, ciel) plati nahradny text vyluky/odklonu z programu.</summary>
    public int TypeItemsIdx { get; init; }

    /// <summary>Sekcia TAB1; <see langword="null"/>, ak nie je.</summary>
    public TabTabSection? Tab1 { get; init; }

    /// <summary>Sekcia TAB2; <see langword="null"/>, ak nie je.</summary>
    public TabTabSection? Tab2 { get; init; }

    /// <summary>Hodnota ineho stlpca tej istej tabule pre <c>%meno%</c>; <see langword="null"/>, ak stlpec nie je.</summary>
    public Func<string, TabTabValue?>? ColumnValue { get; init; }

    /// <summary>Nahradny text pri vyluke nastaveny v INISSe (krok 7); prazdny = nie je.</summary>
    public string FallbackLockoutText { get; init; } = "";

    /// <summary>Nahradny text pri odklone nastaveny v INISSe (krok 7); prazdny = nie je.</summary>
    public string FallbackDeflectText { get; init; } = "";

    /// <summary>Ci tabula vie striedat casti textu (<c>#</c>) - <c>#MERGE</c> potom spaja po castiach; inak ako <c>#MERGE2</c>.</summary>
    public bool MergeByParts { get; init; } = true;
}
