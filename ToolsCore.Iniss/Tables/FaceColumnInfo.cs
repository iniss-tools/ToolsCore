namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Stlpec katalogovej tabule, ktory sa ukazuje pri vlaku - nad nim byva na cele tabule natlaceny popis.
/// </summary>
/// <param name="Line">riadok zaznamu (TYPE_ITEMS_LINE, od 0)</param>
/// <param name="Start">zaciatok stlpca v bodoch</param>
/// <param name="End">koniec stlpca v bodoch (prvy bod za stlpcom)</param>
/// <param name="Align">zarovnanie (TYPE_ITEMS_ALIGN: 0 vlavo, 1 vpravo, 2 na stred)</param>
/// <param name="Content">druh obsahu (TYPE_ITEMS_IDX - cislo FILL_SECTION)</param>
/// <param name="Name">nazov stlpca pre obsluhu</param>
public sealed record FaceColumnInfo(int Line, int Start, int End, int Align, int Content, string Name);
