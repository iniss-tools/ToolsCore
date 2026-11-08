// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Register funkcii jazyka vyrazov.
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
    /// Vsetky funkcie v poradi tabulky INISSu (ceske mena, potom anglicke aliasy).
    /// </summary>
    public static IReadOnlyList<ExprFunctionInfo> All => Infos;

    /// <summary>
    /// Najde funkciu podla mena (bez ohladu na velkost pismen).
    /// </summary>
    public static ExprFunctionInfo? Find(string name) => ByName.GetValueOrDefault(name);

    /// <summary>
    /// Vrati popis funkcie.
    /// </summary>
    public static ExprFunctionInfo Get(ExprFunction function) => ByFunction[function];
}
