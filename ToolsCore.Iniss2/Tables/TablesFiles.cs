using System.Xml.Linq;
using ToolsCore.Iniss.Tables;

namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Cast tabule - jej obsah a katalogova predloha.
/// </summary>
/// <param name="Name">nazov</param>
/// <param name="CatalogKey">kluc predlohy v katalogu stanice</param>
/// <param name="Kind">druh casti, napriklad DEPARTURE</param>
/// <param name="Records">pocet zaznamov</param>
public sealed record TablePart(string Name, string CatalogKey, string Kind, int Records)
{
    /// <summary>Druh casti s informacnym textom.</summary>
    public const string Info = "INFO";

    /// <summary>Druh casti bez vlakov.</summary>
    public const string NotDefined = "NODEF";

    /// <summary>Kluce kolaji, ktorych vlaky cast ukazuje.</summary>
    public IReadOnlyList<string> Tracks { get; init; } = [];

    /// <summary>Priority kolaji podla kluca; chybajuca = 0.</summary>
    public IReadOnlyDictionary<string, int> TrackPriorities { get; init; } = new Dictionary<string, int>();

    /// <summary>Cisla stanic oddelene medzerou, ktorych vlaky cast ukazuje; prazdne = stanica tabule.</summary>
    public string Stations { get; init; } = "";

    /// <summary>Kluc tabule, na ktorej cast nadvazuje; prazdny = cast zacina.</summary>
    public string Previous { get; init; } = "";
}

/// <summary>
/// Tabula stanice.
/// </summary>
/// <param name="Key">kluc</param>
/// <param name="Name">nazov</param>
/// <param name="Line">kluc komunikacnej linky; prazdny = tabula nema zariadenie</param>
/// <param name="Address">adresa na linke</param>
/// <param name="SaveFile">subor XML v priecinku logov; prazdny = tabula sa do XML neuklada</param>
/// <param name="Parts">casti tabule</param>
public sealed record TableDefinition(string Key, string Name, string Line, int Address, string SaveFile, IReadOnlyList<TablePart> Parts)
{
    /// <summary>Bez adresy - tabula so zariadenim nekomunikuje.</summary>
    public const int NoAddress = -1;

    /// <summary>Cisla zaznamov s otocenymi sipkami.</summary>
    public string ReverseArrows { get; init; } = "";
}

/// <summary>
/// Tabule stanice - subor Tables.xml v priecinku stanice.
/// </summary>
public static class TablesFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "Tables.xml";

    /// <summary>
    /// Nacita tabule stanice. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<TableDefinition> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("t").Select(table => new TableDefinition(
            Iniss2Xml.Text(table, "k"),
            Iniss2Xml.Text(table, "n"),
            Iniss2Xml.Text(table, "lin"),
            Iniss2Xml.Number(table, "addr"),
            Iniss2Xml.Text(table, "save"),
            table.Elements("part").Select(ReadPart).ToList())
        {
            ReverseArrows = Iniss2Xml.Text(table, "revarrows")
        }).ToList();
    }

    /// <summary>
    /// Zapise tabule. Adresa sa zapisuje vzdy, aj bez zariadenia (-1).
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="tables">tabule s roznymi klucmi</param>
    public static void Write(string stationDir, IEnumerable<TableDefinition> tables) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("TABLES", tables.Select(table => new XElement("t",
            new XAttribute("k", table.Key), Iniss2Xml.Optional("n", table.Name), Iniss2Xml.Optional("lin", table.Line), new XAttribute("addr", table.Address),
            Iniss2Xml.Optional("save", table.SaveFile), Iniss2Xml.Optional("revarrows", table.ReverseArrows), table.Parts.Select(WritePart)))));

    private static TablePart ReadPart(XElement part) =>
        new(Iniss2Xml.Text(part, "n"), Iniss2Xml.Text(part, "ck"), Iniss2Xml.Text(part, "t"), Iniss2Xml.Number(part, "rec"))
        {
            Tracks = part.Elements("pos").Select(position => Iniss2Xml.Text(position, "k")).ToList(),
            TrackPriorities = part.Elements("pos").Where(position => position.Attribute("pri") is not null)
                .ToDictionary(position => Iniss2Xml.Text(position, "k"), position => Iniss2Xml.Number(position, "pri")),
            Stations = Iniss2Xml.Text(part, "st"),
            Previous = Iniss2Xml.Text(part, "prev")
        };

    private static XElement WritePart(TablePart part) => new("part",
        Iniss2Xml.Optional("n", part.Name), new XAttribute("ck", part.CatalogKey), new XAttribute("t", part.Kind), new XAttribute("rec", part.Records),
        Iniss2Xml.Optional("st", part.Stations), Iniss2Xml.Optional("prev", part.Previous),
        part.Tracks.Select(track => new XElement("pos", new XAttribute("k", track), Iniss2Xml.Optional("pri", part.TrackPriorities.GetValueOrDefault(track)))));
}

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

/// <summary>
/// Katalogova predloha tabule.
/// </summary>
/// <param name="Key">kluc - podla neho sa na predlohu odkazuje cast tabule</param>
/// <param name="Name">nazov</param>
/// <param name="Class">druh zariadenia, napriklad LCD1; INTERNET = tabula bez zariadenia</param>
public sealed record CatalogTable(string Key, string Name, string Class)
{
    /// <summary>Druh predlohy bez zariadenia.</summary>
    public const string Internet = "INTERNET";

    /// <summary>Udaje predlohy v poradi suboru.</summary>
    public IReadOnlyList<CatalogColumn> Columns { get; init; } = [];

    /// <summary>Nazov banky pisem; povinny pri ELEN, ELENOLD, ELEN10, ELEN16 a ELEN16KAM.</summary>
    public string FontBank { get; init; } = "";

    /// <summary>Predloha ma zariadenie.</summary>
    public bool IsDevice => !string.Equals(Class, Internet, StringComparison.Ordinal);

    /// <summary>Riadkov tabule na jeden zaznam - podla najvyssieho riadku udajov, ako ich pocita server.</summary>
    public int LinesPerRecord => Columns.Count == 0 ? 1 : Math.Max(0, Columns.Max(column => column.Row)) + 1;
}

/// <summary>
/// Katalog tabul stanice - subor TCatalog.xml v priecinku stanice.
/// </summary>
public static class TableCatalogFile
{
    /// <summary>
    /// Nazov suboru v priecinku stanice.
    /// </summary>
    public const string FileName = "TCatalog.xml";

    /// <summary>
    /// Nacita predlohy stanice. Chybajuci subor je prazdny zoznam.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    public static List<CatalogTable> Read(string stationDir)
    {
        var file = Path.Combine(stationDir, FileName);
        if (!File.Exists(file))
            return [];

        return Iniss2Xml.Load(file).Root!.Elements("t")
            .Select(table => new CatalogTable(Iniss2Xml.Text(table, "k"), Iniss2Xml.Text(table, "n"), Iniss2Xml.Text(table, "cls"))
            {
                Columns = table.Elements("div").Select(ReadColumn).ToList(),
                FontBank = Iniss2Xml.Text(table, "f")
            })
            .ToList();
    }

    /// <summary>
    /// Zapise predlohy.
    /// </summary>
    /// <param name="stationDir">priecinok stanice</param>
    /// <param name="catalogs">predlohy s roznymi klucmi</param>
    public static void Write(string stationDir, IEnumerable<CatalogTable> catalogs) =>
        Iniss2Xml.Save(Path.Combine(stationDir, FileName), new XElement("CATALOG", catalogs.Select(catalog => new XElement("t",
            new XAttribute("k", catalog.Key), Iniss2Xml.Optional("n", catalog.Name), new XAttribute("cls", catalog.Class), Iniss2Xml.Optional("f", catalog.FontBank),
            catalog.Columns.Select(WriteColumn)))));

    private static CatalogColumn ReadColumn(XElement column)
    {
        var position = column.Element("pos");

        return new CatalogColumn(Iniss2Xml.Text(column, "k"), Iniss2Xml.Text(column, "inf"), position is null ? -1 : Iniss2Xml.Number(position, "r"),
            position is null ? 0 : Iniss2Xml.Number(position, "c1"), position is null ? 0 : Iniss2Xml.Number(position, "c2"), Iniss2Xml.Text(column, "align"))
        {
            Name = Iniss2Xml.Text(column, "n"),
            Font = Iniss2Xml.Number(column, "fnt"),
            Translation = Iniss2Xml.Text(column, "trn") is { Length: > 0 } translation ? translation : CatalogColumn.FreeTranslation,
            Table1 = Iniss2Xml.Text(column, "t1"),
            Table2 = Iniss2Xml.Text(column, "t2")
        };
    }

    private static XElement WriteColumn(CatalogColumn column) => new("div",
        new XAttribute("k", column.Key), Iniss2Xml.Optional("n", column.Name), new XAttribute("inf", column.Content), Iniss2Xml.Optional("fnt", column.Font),
        Iniss2Xml.Optional("align", column.Align),
        column.Translation == CatalogColumn.FreeTranslation ? null : new XAttribute("trn", column.Translation),
        Iniss2Xml.Optional("t1", column.Table1), Iniss2Xml.Optional("t2", column.Table2),
        new XElement("pos", Iniss2Xml.Optional("r", column.Row), Iniss2Xml.Optional("c1", column.Start), Iniss2Xml.Optional("c2", column.End)));
}
