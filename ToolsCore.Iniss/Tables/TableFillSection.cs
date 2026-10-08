using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Urcuje obsah sekcie na katalogovej tabuli.
/// </summary>
public sealed class TableFillSection : Enumeration<TableFillSection>
{
    private TableFillSection(int id, string name) : base(id, name)
    {
    }

    /// <summary>
    /// Odkaz na seba, pre potreby DataSource.
    /// </summary>
    public TableFillSection This => this;

    /// <summary>
    /// Konvertuje ID typu obsahu sekcie na objekt.
    /// </summary>
    /// <param name="id">Cislo zo sekcie [FILL_SECTION] v ModeTabs.TXT (TYPE_ITEMS_IDX v TKatalog.TXT).</param>
    /// <returns>Objekt alebo <see langword="null" />, ak INISS take cislo nepozna.</returns>
    public static TableFillSection? Parse(int id) => GetValues().FirstOrDefault(section => section.Id == id);

    #region VALUES

    // Cisla su zabudovane v INISSe (ArgsPanel_*); poradie a nazvy zodpovedaju zoznamu v INISS 3.39.
    public static readonly TableFillSection NotDefined = new(0, Resources.FillSection_NotDefined);
    public static readonly TableFillSection Free = new(1, Resources.FillSection_Free);
    public static readonly TableFillSection VychadzajucaStanica = new(2, Resources.FillSection_VychadzajucaStanica);
    public static readonly TableFillSection StaniceZoSmeru = new(3, Resources.FillSection_StaniceZoSmeru);
    public static readonly TableFillSection StaniceDoSmeruNastupiste = new(4, Resources.FillSection_StaniceDoSmeruNastupiste);
    public static readonly TableFillSection StaniceDoSmeru = new(5, Resources.FillSection_StaniceDoSmeru);
    public static readonly TableFillSection CielovaStanicaNastupiste = new(6, Resources.FillSection_CielovaStanicaNastupiste);
    public static readonly TableFillSection CielovaStanicaPodchod = new(7, Resources.FillSection_CielovaStanicaPodchod);
    public static readonly TableFillSection CielovaStanica = new(8, Resources.FillSection_CielovaStanica);
    public static readonly TableFillSection CasOdchodu = new(9, Resources.FillSection_CasOdchodu);
    public static readonly TableFillSection CasPrichodu = new(10, Resources.FillSection_CasPrichodu);
    public static readonly TableFillSection MeskaniePrichod = new(11, Resources.FillSection_MeskaniePrichod);
    public static readonly TableFillSection MeskanieOdchod = new(12, Resources.FillSection_MeskanieOdchod);
    public static readonly TableFillSection MeskaniePrichodPopis = new(13, Resources.FillSection_MeskaniePrichodPopis);
    public static readonly TableFillSection MeskanieOdchodPopis = new(14, Resources.FillSection_MeskanieOdchodPopis);
    public static readonly TableFillSection TypVlaku = new(15, Resources.FillSection_TypVlaku);
    public static readonly TableFillSection CisloVlaku = new(16, Resources.FillSection_CisloVlaku);
    public static readonly TableFillSection TypCisloVlaku = new(17, Resources.FillSection_TypCisloVlaku);
    public static readonly TableFillSection NazovVlaku = new(18, Resources.FillSection_NazovVlaku);
    public static readonly TableFillSection KolajPrichod = new(19, Resources.FillSection_KolajPrichod);
    public static readonly TableFillSection KolajOdchod = new(20, Resources.FillSection_KolajOdchod);
    public static readonly TableFillSection NastupistePrichod = new(21, Resources.FillSection_NastupistePrichod);
    public static readonly TableFillSection NastupisteOdchod = new(22, Resources.FillSection_NastupisteOdchod);
    public static readonly TableFillSection VlakStojiVStanici = new(23, Resources.FillSection_VlakStojiVStanici);
    public static readonly TableFillSection TextLine1 = new(24, Resources.FillSection_TextLine1);
    public static readonly TableFillSection TextLine2 = new(25, Resources.FillSection_TextLine2);
    public static readonly TableFillSection TypNazovOrCislo = new(26, Resources.FillSection_TypNazovOrCislo);
    public static readonly TableFillSection TypCisloVlaku6Chars = new(27, Resources.FillSection_TypCisloVlaku6Chars);
    public static readonly TableFillSection HexTypVlaku = new(28, Resources.FillSection_HexTypVlaku);
    public static readonly TableFillSection TypMedzeraCisloVlaku = new(29, Resources.FillSection_TypMedzeraCisloVlaku);
    public static readonly TableFillSection NastupisteKolajPrichod = new(30, Resources.FillSection_NastupisteKolajPrichod);
    public static readonly TableFillSection NastupisteKolajOdchod = new(31, Resources.FillSection_NastupisteKolajOdchod);
    public static readonly TableFillSection KolajAltPrichod = new(32, Resources.FillSection_KolajAltPrichod);
    public static readonly TableFillSection KolajAltOdchod = new(33, Resources.FillSection_KolajAltOdchod);
    public static readonly TableFillSection Dopravca = new(34, Resources.FillSection_Dopravca);
    public static readonly TableFillSection CisloVlaku35 = new(35, Resources.FillSection_CisloVlaku35);
    public static readonly TableFillSection LinkaOdchod = new(36, Resources.FillSection_LinkaOdchod);
    public static readonly TableFillSection LinkaPrichod = new(37, Resources.FillSection_LinkaPrichod);
    public static readonly TableFillSection CisloVlaku38 = new(38, Resources.FillSection_CisloVlaku38);

    #endregion
}