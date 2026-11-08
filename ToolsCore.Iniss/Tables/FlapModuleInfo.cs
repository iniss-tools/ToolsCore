namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Modul listovej tabule: na riadku zaznamu <paramref name="Line" /> zaberie pozicie od <paramref name="Position" />
/// (znaky spravy, jeden znak = jeden kod listu); kody <paramref name="Span" /> pozicii spolu urcuju list zo zoznamu
/// <paramref name="List" />.
/// </summary>
/// <param name="Line">riadok v ramci zaznamu (TYPE_ITEMS_LINE, od 0)</param>
/// <param name="Position">prva pozicia (START / 8)</param>
/// <param name="Span">pocet pozicii</param>
/// <param name="List">meno zoznamu listov (sekcia TabTab); null = kod sa ukaze, ako prisiel</param>
public sealed record FlapModuleInfo(int Line, int Position, int Span, string? List);
