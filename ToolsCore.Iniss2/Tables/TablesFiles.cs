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

    /// <summary>Kluce kolaji, ktorych vlaky cast ukazuje.</summary>
    public IReadOnlyList<string> Tracks { get; init; } = [];
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
public sealed record TableDefinition(string Key, string Name, string Line, int Address, string SaveFile, IReadOnlyList<TablePart> Parts);

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
            table.Elements("part").Select(ReadPart).ToList())).ToList();
    }

    private static TablePart ReadPart(XElement part) =>
        new(Iniss2Xml.Text(part, "n"), Iniss2Xml.Text(part, "ck"), Iniss2Xml.Text(part, "t"), Iniss2Xml.Number(part, "rec"))
        {
            Tracks = part.Elements("pos").Select(position => Iniss2Xml.Text(position, "k")).ToList()
        };
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
        ["Linka"] = TableFillSection.LinkaOdchod
    };

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
                Columns = table.Elements("div").Select(ReadColumn).ToList()
            })
            .ToList();
    }

    private static CatalogColumn ReadColumn(XElement column)
    {
        var position = column.Element("pos");

        return new CatalogColumn(Iniss2Xml.Text(column, "k"), Iniss2Xml.Text(column, "inf"), position is null ? -1 : Iniss2Xml.Number(position, "r"),
            position is null ? 0 : Iniss2Xml.Number(position, "c1"), position is null ? 0 : Iniss2Xml.Number(position, "c2"), Iniss2Xml.Text(column, "align"));
    }
}
