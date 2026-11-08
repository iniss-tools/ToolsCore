using ToolsCore.Iniss.Tables;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Udaj katalogovej predlohy - co sa na tabuli ukazuje a kde.
/// </summary>
/// <param name="Key">kluc</param>
/// <param name="Content">druh obsahu (atribut <c>inf</c>), napriklad CasOdjezdu</param>
/// <param name="Row">riadok zaznamu od 0; zaporny = udaj sa na tabuli nezobrazuje</param>
/// <param name="Start">zaciatok stlpca</param>
/// <param name="End">koniec stlpca (prvy za stlpcom)</param>
/// <param name="Align">zarovnanie (LEFT, RIGHT, CENTER)</param>
public sealed record CatalogColumn(string Key, string Content, int Row, int Start, int End, string Align)
{
    // INISS2 ma vlastne cisla obsahu, spolocne su len nazvy; udaje bez obdoby v INISS 3.39 tu nie su
    private static readonly Dictionary<string, TableFillSection> Sections = new(StringComparer.Ordinal)
    {
        ["Free"] = TableFillSection.Free,
        ["VychoziNazev"] = TableFillSection.VychadzajucaStanica,
        ["PredNazev"] = TableFillSection.StaniceZoSmeru,
        ["PoNazevNastupiste"] = TableFillSection.StaniceDoSmeruNastupiste,
        ["PoNazev"] = TableFillSection.StaniceDoSmeru,
        ["CilNazevNastupiste"] = TableFillSection.CielovaStanicaNastupiste,
        ["CilNazevPodchod"] = TableFillSection.CielovaStanicaPodchod,
        ["CilNazev"] = TableFillSection.CielovaStanica,
        ["CasOdjezdu"] = TableFillSection.CasOdchodu,
        ["CasOdjezduAktualni"] = TableFillSection.CasOdchodu,
        ["CasPrijezdu"] = TableFillSection.CasPrichodu,
        ["CasPrijezduAktualni"] = TableFillSection.CasPrichodu,
        ["ZpozdeniPrijezd"] = TableFillSection.MeskaniePrichod,
        ["ZpozdeniOdjezd"] = TableFillSection.MeskanieOdchod,
        ["ZpozdeniPrijezdPopis"] = TableFillSection.MeskaniePrichodPopis,
        ["ZpozdeniOdjezdPopis"] = TableFillSection.MeskanieOdchodPopis,
        ["TypVlaku"] = TableFillSection.TypVlaku,
        ["CisloVlaku"] = TableFillSection.CisloVlaku,
        ["TypCisloVlaku"] = TableFillSection.TypCisloVlaku,
        ["NazevVlaku"] = TableFillSection.NazovVlaku,
        ["KolejPrijezd"] = TableFillSection.KolajPrichod,
        ["KolejOdjezd"] = TableFillSection.KolajOdchod,
        ["NastupistePrijezd"] = TableFillSection.NastupistePrichod,
        ["NastupisteOdjezd"] = TableFillSection.NastupisteOdchod,
        ["VlakStojiVeStanici"] = TableFillSection.VlakStojiVStanici,
        ["TextLine1"] = TableFillSection.TextLine1,
        ["TextLine2"] = TableFillSection.TextLine2,
        ["TypNazevOrCislo"] = TableFillSection.TypNazovOrCislo,
        ["TypCisloVlaku6"] = TableFillSection.TypCisloVlaku6Chars,
        ["HexTypVlakuEx"] = TableFillSection.HexTypVlaku,
        ["NastKolejPrijezd"] = TableFillSection.NastupisteKolajPrichod,
        ["NastKolejOdjezd"] = TableFillSection.NastupisteKolajOdchod,
        ["KolejAltPrijezd"] = TableFillSection.KolajAltPrichod,
        ["KolejAltOdjezd"] = TableFillSection.KolajAltOdchod,
        ["Dopravce"] = TableFillSection.Dopravca,
        ["Linka"] = TableFillSection.LinkaOdchod,
        ["TypCisloVlakuKompr"] = TableFillSection.TypMedzeraCisloVlaku
    };

    /// <summary>Druh obsahu, ktory server nedoplni.</summary>
    public const string NoContent = "Not";

    /// <summary>Text bez prekladu.</summary>
    public const string FreeTranslation = "FREE";

    /// <summary>Nazov udaja.</summary>
    public string Name { get; init; } = "";

    /// <summary>Cislo pisma; vyznam podla druhu zariadenia.</summary>
    public int Font { get; init; }

    /// <summary>Sposob prekladu (atribut <c>trn</c>): FREE, TABLE, TABLETIME, TRANSLATE alebo CHAR.</summary>
    public string Translation { get; init; } = FreeTranslation;

    /// <summary>Prva tabulka prekladu; prazdna = bez nej.</summary>
    public string Table1 { get; init; } = "";

    /// <summary>Druha tabulka prekladu (hodiny a minuty pri TABLETIME); prazdna = bez nej.</summary>
    public string Table2 { get; init; } = "";

    /// <summary>
    /// Druh obsahu INISS2 pre obsah INISS 3.39; null = INISS2 obdobu nema.
    /// </summary>
    /// <param name="section">obsah v cislovani INISS 3.39</param>
    public static string? ContentFor(TableFillSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (section == TableFillSection.NotDefined)
            return NoContent;

        // linka pri prichode: INISS2 berie linku prichodu na prichodovej casti
        if (section == TableFillSection.LinkaPrichod)
            return "Linka";

        // prvy nazov je obsah bez aktualizacie mieskania (CasOdjezdu pred CasOdjezduAktualni)
        return Sections.FirstOrDefault(pair => pair.Value == section).Key;
    }

    /// <summary>Udaj sa na tabuli zobrazuje.</summary>
    public bool IsVisible => Row >= 0;

    /// <summary>Druh obsahu v cislovani INISS 3.39; null = obsah tam nema obdobu.</summary>
    public TableFillSection? Section => Sections.GetValueOrDefault(Content);

    /// <summary>Udaj je informacny text.</summary>
    public bool IsTextLine => Content is "TextLine1" or "TextLine2";
}
