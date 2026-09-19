// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
namespace ToolsCore.Expressions;

/// <summary>
///     Funkcie jazyka vyrazov INISS. Ciselna hodnota je index tokenu v tabulke prekladaca INISSu
///     (1-based, pozri docs/iniss/formaty-suborov/local/vyrazy.mdx).
/// </summary>
public enum ExprFunction
{
    CVLAKU = 27,
    POZICE = 28,
    TYP = 29,
    PRIZNAK = 30,
    STAV = 31,
    NAKLTYP = 32,
    VYLUKAZDE = 33,
    ODKLON = 34,
    ZPOZDENIPRIJ = 35,
    ZPOZDENIODJ = 36,
    ZPOZDENI = 37,
    DOBAPOBYTU = 38,
    PLANDOBAPOBYTU = 39,
    ZAJMSTANICE = 40,
    VYCHSTANICE = 41,
    CILSTANICE = 42,
    MISTNI = 43,
    CIZI = 44,
    ZESMERU = 45,
    DOSMERU = 46,
    KOLEJPRIJ = 47,
    KOLEJODJ = 48,
    DATUMPRIJ = 49,
    DATUMODJ = 50,
    CASPRIJ = 51,
    CASODJ = 52,
    INDCAT6 = 53,
    INDCAT8 = 54,
    DATE = 55,
    TIME = 56,
    OPERATOR = 57,
    LINKAPRIJ = 58,
    LINKAODJ = 59,
    ZMENALINKY = 60,
    NAZVLAKU = 61,
    TRAINNUM = 62,
    POSITION = 63,
    TYPE = 64,
    FLAG = 65,
    STATE = 66,
    CARGO = 67,
    LOCKOUT = 68,
    DEFLECT = 69,
    DELAYARR = 70,
    DELAYDEP = 71,
    DELAY = 72,
    STAYTIME = 73,
    PLANSTAYTIME = 74,
    HOMESTATION = 75,
    BASESTATION = 76,
    ENDSTATION = 77,
    LOCAL = 78,
    OUTSIDE = 79,
    FROMSTATION = 80,
    TOSTATION = 81,
    TRACKARR = 82,
    TRACKDEP = 83,
    DATEARR = 84,
    DATEDEP = 85,
    TIMEARR = 86,
    TIMEDEP = 87
}

/// <summary>
///     Tvar argumentov funkcie.
/// </summary>
public enum ExprArgKind
{
    /// <summary>Bez zatvoriek (napr. <c>ZPOZDENI</c>).</summary>
    None,

    /// <summary>Volitelny ciselny argument - bez neho vracia hodnotu, s nim porovnanie (napr. <c>TYP(Typ_R)</c>).</summary>
    OptionalNumber,

    /// <summary>Volitelny argument, ktory moze byt cislo alebo retazec (<c>CVLAKU</c>).</summary>
    OptionalNumberOrString,

    /// <summary>Povinny ciselny argument (<c>ZESMERU(id)</c>).</summary>
    RequiredNumber,

    /// <summary>Povinny retazcovy argument (<c>KOLEJPRIJ("1")</c>).</summary>
    RequiredString
}

/// <summary>
///     Vyznam argumentu funkcie - pouziva sa pri semantickej kontrole a napovede.
/// </summary>
public enum ExprArgMeaning
{
    None,
    TrainNumber,
    Position,
    TrainType,
    Flags,
    State,
    StationId,
    TrackName,
    OperatorName,
    LineName,
    TrainName
}

/// <summary>
///     Druh hodnoty, ktoru funkcia vracia.
/// </summary>
public enum ExprValueKind
{
    /// <summary><c>0</c> alebo <c>1</c>.</summary>
    Bool,

    /// <summary>Cele cislo.</summary>
    Number,

    /// <summary>Bitova maska (napr. <c>PRIZNAK</c>).</summary>
    Mask,

    /// <summary>Pocet minut.</summary>
    Minutes,

    /// <summary>Pocet sekund od polnoci.</summary>
    TimeOfDay,

    /// <summary>OLE datum - pocet dni od 30. 12. 1899.</summary>
    Date,

    /// <summary>ID stanice.</summary>
    StationId,

    /// <summary>Index druhu vlaku (0-94).</summary>
    TrainType,

    /// <summary>Pozicia vlaku (1-3).</summary>
    Position,

    /// <summary>Cislo kategorie stavoveho diagramu.</summary>
    Category
}

/// <summary>
///     Popis funkcie jazyka vyrazov.
/// </summary>
/// <param name="Function">Funkcia.</param>
/// <param name="Name">Meno (ceske/slovenske alebo anglicke - kazdy alias ma vlastny zaznam).</param>
/// <param name="Canonical">Kanonicka podoba (ceska), na ktoru sa alias mapuje pri vyhodnocovani.</param>
/// <param name="ArgKind">Tvar argumentov.</param>
/// <param name="ArgMeaning">Vyznam argumentu.</param>
/// <param name="Returns">Druh hodnoty bez argumentu.</param>
/// <param name="ContextDependent">Vysledok zavisi od kontextu vyhodnotenia (prichodova tabula vs. ine).</param>
public sealed record ExprFunctionInfo(
    ExprFunction Function,
    string Name,
    ExprFunction Canonical,
    ExprArgKind ArgKind,
    ExprArgMeaning ArgMeaning,
    ExprValueKind Returns,
    bool ContextDependent = false)
{
    /// <summary>Ci je zaznam anglickym aliasom.</summary>
    public bool IsAlias => Function != Canonical;

    /// <summary>Ci funkcia pripusta argument.</summary>
    public bool AcceptsArgument => ArgKind != ExprArgKind.None;

    /// <summary>Ci funkcia argument vyzaduje.</summary>
    public bool RequiresArgument => ArgKind is ExprArgKind.RequiredNumber or ExprArgKind.RequiredString;
}

/// <summary>
///     Register funkcii jazyka vyrazov.
/// </summary>
public static class ExprFunctions
{
    private static readonly ExprFunctionInfo[] Infos =
    [
        new(ExprFunction.CVLAKU, "CVLAKU", ExprFunction.CVLAKU, ExprArgKind.OptionalNumberOrString, ExprArgMeaning.TrainNumber, ExprValueKind.Number),
        new(ExprFunction.POZICE, "POZICE", ExprFunction.POZICE, ExprArgKind.OptionalNumber, ExprArgMeaning.Position, ExprValueKind.Position),
        new(ExprFunction.TYP, "TYP", ExprFunction.TYP, ExprArgKind.OptionalNumber, ExprArgMeaning.TrainType, ExprValueKind.TrainType),
        new(ExprFunction.PRIZNAK, "PRIZNAK", ExprFunction.PRIZNAK, ExprArgKind.OptionalNumber, ExprArgMeaning.Flags, ExprValueKind.Mask),
        new(ExprFunction.STAV, "STAV", ExprFunction.STAV, ExprArgKind.OptionalNumber, ExprArgMeaning.State, ExprValueKind.Mask),
        new(ExprFunction.NAKLTYP, "NAKLTYP", ExprFunction.NAKLTYP, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.VYLUKAZDE, "VYLUKAZDE", ExprFunction.VYLUKAZDE, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool, true),
        new(ExprFunction.ODKLON, "ODKLON", ExprFunction.ODKLON, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.ZPOZDENIPRIJ, "ZPOZDENIPRIJ", ExprFunction.ZPOZDENIPRIJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.ZPOZDENIODJ, "ZPOZDENIODJ", ExprFunction.ZPOZDENIODJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.ZPOZDENI, "ZPOZDENI", ExprFunction.ZPOZDENI, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes, true),
        new(ExprFunction.DOBAPOBYTU, "DOBAPOBYTU", ExprFunction.DOBAPOBYTU, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.PLANDOBAPOBYTU, "PLANDOBAPOBYTU", ExprFunction.PLANDOBAPOBYTU, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.ZAJMSTANICE, "ZAJMSTANICE", ExprFunction.ZAJMSTANICE, ExprArgKind.OptionalNumber, ExprArgMeaning.StationId, ExprValueKind.StationId),
        new(ExprFunction.VYCHSTANICE, "VYCHSTANICE", ExprFunction.VYCHSTANICE, ExprArgKind.OptionalNumber, ExprArgMeaning.StationId, ExprValueKind.StationId),
        new(ExprFunction.CILSTANICE, "CILSTANICE", ExprFunction.CILSTANICE, ExprArgKind.OptionalNumber, ExprArgMeaning.StationId, ExprValueKind.StationId),
        new(ExprFunction.MISTNI, "MISTNI", ExprFunction.MISTNI, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.CIZI, "CIZI", ExprFunction.CIZI, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.ZESMERU, "ZESMERU", ExprFunction.ZESMERU, ExprArgKind.RequiredNumber, ExprArgMeaning.StationId, ExprValueKind.Bool),
        new(ExprFunction.DOSMERU, "DOSMERU", ExprFunction.DOSMERU, ExprArgKind.RequiredNumber, ExprArgMeaning.StationId, ExprValueKind.Bool),
        new(ExprFunction.KOLEJPRIJ, "KOLEJPRIJ", ExprFunction.KOLEJPRIJ, ExprArgKind.RequiredString, ExprArgMeaning.TrackName, ExprValueKind.Bool),
        new(ExprFunction.KOLEJODJ, "KOLEJODJ", ExprFunction.KOLEJODJ, ExprArgKind.RequiredString, ExprArgMeaning.TrackName, ExprValueKind.Bool),
        new(ExprFunction.DATUMPRIJ, "DATUMPRIJ", ExprFunction.DATUMPRIJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Date),
        new(ExprFunction.DATUMODJ, "DATUMODJ", ExprFunction.DATUMODJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Date),
        new(ExprFunction.CASPRIJ, "CASPRIJ", ExprFunction.CASPRIJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.TimeOfDay),
        new(ExprFunction.CASODJ, "CASODJ", ExprFunction.CASODJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.TimeOfDay),
        new(ExprFunction.INDCAT6, "INDCAT6", ExprFunction.INDCAT6, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Category),
        new(ExprFunction.INDCAT8, "INDCAT8", ExprFunction.INDCAT8, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Category),
        new(ExprFunction.DATE, "DATE", ExprFunction.DATE, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Date),
        new(ExprFunction.TIME, "TIME", ExprFunction.TIME, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.TimeOfDay),
        new(ExprFunction.OPERATOR, "OPERATOR", ExprFunction.OPERATOR, ExprArgKind.RequiredString, ExprArgMeaning.OperatorName, ExprValueKind.Bool),
        new(ExprFunction.LINKAPRIJ, "LINKAPRIJ", ExprFunction.LINKAPRIJ, ExprArgKind.RequiredString, ExprArgMeaning.LineName, ExprValueKind.Bool),
        new(ExprFunction.LINKAODJ, "LINKAODJ", ExprFunction.LINKAODJ, ExprArgKind.RequiredString, ExprArgMeaning.LineName, ExprValueKind.Bool),
        new(ExprFunction.ZMENALINKY, "ZMENALINKY", ExprFunction.ZMENALINKY, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.NAZVLAKU, "NAZVLAKU", ExprFunction.NAZVLAKU, ExprArgKind.RequiredString, ExprArgMeaning.TrainName, ExprValueKind.Bool),

        new(ExprFunction.TRAINNUM, "TRAINNUM", ExprFunction.CVLAKU, ExprArgKind.OptionalNumberOrString, ExprArgMeaning.TrainNumber, ExprValueKind.Number),
        new(ExprFunction.POSITION, "POSITION", ExprFunction.POZICE, ExprArgKind.OptionalNumber, ExprArgMeaning.Position, ExprValueKind.Position),
        new(ExprFunction.TYPE, "TYPE", ExprFunction.TYP, ExprArgKind.OptionalNumber, ExprArgMeaning.TrainType, ExprValueKind.TrainType),
        new(ExprFunction.FLAG, "FLAG", ExprFunction.PRIZNAK, ExprArgKind.OptionalNumber, ExprArgMeaning.Flags, ExprValueKind.Mask),
        new(ExprFunction.STATE, "STATE", ExprFunction.STAV, ExprArgKind.OptionalNumber, ExprArgMeaning.State, ExprValueKind.Mask),
        new(ExprFunction.CARGO, "CARGO", ExprFunction.NAKLTYP, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.LOCKOUT, "LOCKOUT", ExprFunction.VYLUKAZDE, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool, true),
        new(ExprFunction.DEFLECT, "DEFLECT", ExprFunction.ODKLON, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.DELAYARR, "DELAYARR", ExprFunction.ZPOZDENIPRIJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.DELAYDEP, "DELAYDEP", ExprFunction.ZPOZDENIODJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.DELAY, "DELAY", ExprFunction.ZPOZDENI, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes, true),
        new(ExprFunction.STAYTIME, "STAYTIME", ExprFunction.DOBAPOBYTU, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.PLANSTAYTIME, "PLANSTAYTIME", ExprFunction.PLANDOBAPOBYTU, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Minutes),
        new(ExprFunction.HOMESTATION, "HOMESTATION", ExprFunction.ZAJMSTANICE, ExprArgKind.OptionalNumber, ExprArgMeaning.StationId, ExprValueKind.StationId),
        new(ExprFunction.BASESTATION, "BASESTATION", ExprFunction.VYCHSTANICE, ExprArgKind.OptionalNumber, ExprArgMeaning.StationId, ExprValueKind.StationId),
        new(ExprFunction.ENDSTATION, "ENDSTATION", ExprFunction.CILSTANICE, ExprArgKind.OptionalNumber, ExprArgMeaning.StationId, ExprValueKind.StationId),
        new(ExprFunction.LOCAL, "LOCAL", ExprFunction.MISTNI, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.OUTSIDE, "OUTSIDE", ExprFunction.CIZI, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Bool),
        new(ExprFunction.FROMSTATION, "FROMSTATION", ExprFunction.ZESMERU, ExprArgKind.RequiredNumber, ExprArgMeaning.StationId, ExprValueKind.Bool),
        new(ExprFunction.TOSTATION, "TOSTATION", ExprFunction.DOSMERU, ExprArgKind.RequiredNumber, ExprArgMeaning.StationId, ExprValueKind.Bool),
        new(ExprFunction.TRACKARR, "TRACKARR", ExprFunction.KOLEJPRIJ, ExprArgKind.RequiredString, ExprArgMeaning.TrackName, ExprValueKind.Bool),
        new(ExprFunction.TRACKDEP, "TRACKDEP", ExprFunction.KOLEJODJ, ExprArgKind.RequiredString, ExprArgMeaning.TrackName, ExprValueKind.Bool),
        new(ExprFunction.DATEARR, "DATEARR", ExprFunction.DATUMPRIJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Date),
        new(ExprFunction.DATEDEP, "DATEDEP", ExprFunction.DATUMODJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.Date),
        new(ExprFunction.TIMEARR, "TIMEARR", ExprFunction.CASPRIJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.TimeOfDay),
        new(ExprFunction.TIMEDEP, "TIMEDEP", ExprFunction.CASODJ, ExprArgKind.None, ExprArgMeaning.None, ExprValueKind.TimeOfDay)
    ];

    private static readonly Dictionary<string, ExprFunctionInfo> ByName =
        Infos.ToDictionary(i => i.Name, i => i, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<ExprFunction, ExprFunctionInfo> ByFunction =
        Infos.ToDictionary(i => i.Function, i => i);

    /// <summary>
    ///     Vsetky funkcie v poradi tabulky INISSu (ceske mena, potom anglicke aliasy).
    /// </summary>
    public static IReadOnlyList<ExprFunctionInfo> All => Infos;

    /// <summary>
    ///     Najde funkciu podla mena (bez ohladu na velkost pismen).
    /// </summary>
    public static ExprFunctionInfo? Find(string name) => ByName.GetValueOrDefault(name);

    /// <summary>
    ///     Vrati popis funkcie.
    /// </summary>
    public static ExprFunctionInfo Get(ExprFunction function) => ByFunction[function];
}
