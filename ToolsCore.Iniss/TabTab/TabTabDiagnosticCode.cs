namespace ToolsCore.Iniss.TabTab;

/// <summary>
/// Kod hlasenia kontroly sekcie TabTab. Kody <c>Iniss*</c> zodpovedaju chybam, ktore INISS zapisuje do logu.
/// </summary>
public enum TabTabDiagnosticCode
{
    /// <summary><c>unknown magic item</c> - nezname meno udalosti za <c>=</c>.</summary>
    InissUnknownMagicItem,

    /// <summary><c>the items count is uneven</c> - <c>#SWITCH</c> s neparnym poctom poloziek.</summary>
    InissItemsCountUneven,

    /// <summary><c>the items count is not multiple of 3</c>.</summary>
    InissItemsCountNotMultipleOf3,

    /// <summary>Chyba prekladu podmienky (text chyby je text INISSu).</summary>
    InissConditionError,

    /// <summary><c>the position … was not found</c> - kolaj z <c>#POZODJ_</c> nie je v Pozice.txt.</summary>
    InissUnknownTrack,

    /// <summary>Varovanie z kontroly vyrazu (kod je v <see cref="TabTabDiagnostic.ExprCode"/>).</summary>
    Condition,

    /// <summary><c>%meno%</c> odkazuje na stlpec, ktory tabula nema.</summary>
    UnknownColumn,

    /// <summary>Pravidlo s udalostou bez poloziek.</summary>
    EmptyEventList,

    /// <summary>Polozka za podmienkou, ktora je vzdy splnena - nikdy sa nepouzije.</summary>
    UnreachableItem,

    /// <summary>Hlavicka <c>[…]</c> vnutri textu sekcie - pri ulozeni by vznikla nova sekcia.</summary>
    SectionHeaderInside,

    /// <summary><c>[</c> bez <c>]</c> - INISS riadok ignoruje.</summary>
    BadSectionHeader,

    /// <summary>Riadok bez <c>=</c> s neznamou volbou - INISS ho ignoruje.</summary>
    UnknownOption,

    /// <summary>Prava strana pravidla je prazdna.</summary>
    EmptyRight,

    /// <summary>Udalost zapisana inou velkostou pismen (<c>#switch</c>) - INISS ju nepozna.</summary>
    EventCase,

    /// <summary>Neparny pocet uvodzoviek v texte.</summary>
    UnbalancedQuotes,

    /// <summary><c>{…}</c>, ktore nie je platny zapis pisma na konci textu.</summary>
    BadFontCode,

    /// <summary><c>{n}</c> vnutri uvodzoviek - je textom, nie pismom.</summary>
    FontInsideQuotes,

    /// <summary>Za <c>\</c> na konci riadka nieco nasleduje (komentar, medzery) - INISS pokracovanie nerozpozna.</summary>
    BrokenContinuation,

    /// <summary>Komentarovy riadok vnutri viacriadkoveho pravidla - INISS nim pravidlo ukonci.</summary>
    CommentInsideRule
}
