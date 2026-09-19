using System.Globalization;

namespace ToolsCore.Expressions;

/// <summary>
///     Miesto, z ktoreho sa vyraz vyhodnocuje - rozhoduje o vysledku <c>VYLUKAZDE</c> a <c>ZPOZDENI</c>
///     (INISS: globalna premenna DAT_00559178).
/// </summary>
public enum ExprEvalSite
{
    /// <summary>Ine tabule a StateDgm (INISS -1): vyluka na prichode alebo odchode, vacsie z meskani.</summary>
    Default,

    /// <summary>Katalogova tabula, ktorej prvy stlpec je prichodovy (INISS 1): vyluka a meskanie prichodu.</summary>
    ArrivalTable,

    /// <summary>INISS 0, 2, 3 - vyluka a meskanie odchodu (v 3.39 sa nepouziva).</summary>
    DepartureTable
}

/// <summary>
///     Udaje o vlaku, ktore evaluator potrebuje. Casy a meskania su v sekundach; funkcie jazyka
///     ich prevadzaju na minuty rovnako ako INISS (celociselne delenie 60).
/// </summary>
public interface IExprTrainContext
{
    /// <summary>Text cisla vlaku (napr. <c>"1234"</c>); <c>CVLAKU</c> z neho cita vedúce cislice.</summary>
    string TrainNumber { get; }

    /// <summary>Pozicia vlaku: 1 vychozi, 2 prechadzajuci, 3 konciaci.</summary>
    int Position { get; }

    /// <summary>Index druhu vlaku v zabudovanej tabulke (0-94).</summary>
    int TrainTypeIndex { get; }

    /// <summary>Slovo priznakov vlaku (bity <c>Prizn_*</c>; najnizsi bajt su ine priznaky).</summary>
    uint Flags { get; }

    /// <summary>Stavove bity vlaku (<c>Stav_*</c>).</summary>
    uint State { get; }

    /// <summary>Ci ma vlak odklon.</summary>
    bool IsDeflected { get; }

    /// <summary>Meskanie prichodu v sekundach.</summary>
    int ArrivalDelaySeconds { get; }

    /// <summary>Meskanie odchodu v sekundach.</summary>
    int DepartureDelaySeconds { get; }

    /// <summary>Doba pobytu v sekundach (s meskaniami).</summary>
    int StayTimeSeconds { get; }

    /// <summary>Planovana doba pobytu v sekundach.</summary>
    int PlannedStayTimeSeconds { get; }

    /// <summary>ID stanice zaujmu.</summary>
    int HomeStationId { get; }

    /// <summary>ID vychodiskovej stanice.</summary>
    int BaseStationId { get; }

    /// <summary>ID cielovej stanice.</summary>
    int EndStationId { get; }

    /// <summary>Ci je vlak miestny.</summary>
    bool IsLocal { get; }

    /// <summary>Ci vlak prichadza zo smeru danej stanice.</summary>
    bool IsFromStation(int stationId);

    /// <summary>Ci vlak odchadza smerom na danu stanicu.</summary>
    bool IsToStation(int stationId);

    /// <summary>Nazov kolaje prichodu; <c>""</c>, ak nie je.</summary>
    string ArrivalTrack { get; }

    /// <summary>Nazov kolaje odchodu; <c>""</c>, ak nie je.</summary>
    string DepartureTrack { get; }

    /// <summary>Datum prichodu.</summary>
    DateOnly ArrivalDate { get; }

    /// <summary>Datum odchodu.</summary>
    DateOnly DepartureDate { get; }

    /// <summary>Cas prichodu.</summary>
    TimeOnly ArrivalTime { get; }

    /// <summary>Cas odchodu.</summary>
    TimeOnly DepartureTime { get; }

    /// <summary>Nazov dopravcu.</summary>
    string OperatorName { get; }

    /// <summary>Linka na prichode.</summary>
    string ArrivalLine { get; }

    /// <summary>Linka na odchode.</summary>
    string DepartureLine { get; }

    /// <summary>Nazov vlaku.</summary>
    string TrainName { get; }
}

/// <summary>
///     Chyba pri vyhodnoteni vyrazu (v INISSe by znamenala pad programu).
/// </summary>
public sealed class ExprEvaluationException(string message, ExprNode node) : Exception(message)
{
    /// <summary>Uzol, pri ktorom chyba nastala.</summary>
    public ExprNode Node { get; } = node;
}

/// <summary>
///     Vyhodnocovac vyrazov - verna kopia INISSu (FUN_004a6a3e v 3.39).
/// </summary>
public sealed class ExprEvaluator
{
    private static readonly CompareInfo Czech = CultureInfo.GetCultureInfo("cs-CZ").CompareInfo;

    private readonly IExprTrainContext? _train;
    private readonly ExprEvalSite _site;
    private readonly DateTime _now;

    /// <summary>
    ///     Vytvori evaluator.
    /// </summary>
    /// <param name="train">Vlak; <see langword="null"/> dovoluje vyhodnotit len vyrazy bez funkcii.</param>
    /// <param name="site">Miesto vyhodnotenia.</param>
    /// <param name="now">"Teraz" pre <c>DATE</c> a <c>TIME</c>; predvolene skutocny cas.</param>
    public ExprEvaluator(IExprTrainContext? train, ExprEvalSite site = ExprEvalSite.Default, DateTime? now = null)
    {
        _train = train;
        _site = site;
        _now = now ?? DateTime.Now;
    }

    /// <summary>
    ///     Pokusi sa vycislit vyraz, ktory neobsahuje funkcie.
    /// </summary>
    public static bool TryFoldConstant(ExprNode node, out int value)
    {
        value = 0;
        if (node.Descendants().Any(n => n is ExprFunctionNode))
            return false;
        try
        {
            value = new ExprEvaluator(null).Evaluate(node);
            return true;
        }
        catch (ExprEvaluationException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Vyhodnoti vyraz. Vysledok je 32-bitove cislo; pravda = nenulove.
    /// </summary>
    public int Evaluate(ExprNode node)
    {
        switch (node)
        {
            case ExprNumberNode n:
                return n.Value;

            case ExprParenNode p:
                return Evaluate(p.Inner);

            case ExprStringNode s:
                throw new ExprEvaluationException("Reťazec mimo argumentu funkcie", s);

            case ExprUnaryNode u:
            {
                var v = Evaluate(u.Operand);
                return u.Operator switch
                {
                    ExprTokenKind.BitNot => ~v,
                    ExprTokenKind.Not or ExprTokenKind.NotWord => v == 0 ? 1 : 0,
                    ExprTokenKind.Plus => v,
                    ExprTokenKind.Minus => unchecked(-v),
                    ExprTokenKind.Odd => v % 2,
                    _ => throw new ExprEvaluationException("Interní chyba překladu", u)
                };
            }

            case ExprBinaryNode b:
            {
                // INISS vyhodnoti obe strany vzdy (bez skrateneho vyhodnocovania)
                var l = Evaluate(b.Left);
                var r = Evaluate(b.Right);
                switch (b.Operator)
                {
                    case ExprTokenKind.BitAnd: return l & r;
                    case ExprTokenKind.BitOr: return l | r;
                    case ExprTokenKind.BitXor: return l ^ r;
                    case ExprTokenKind.And or ExprTokenKind.AndWord: return l != 0 && r != 0 ? 1 : 0;
                    case ExprTokenKind.Or or ExprTokenKind.OrWord: return l != 0 || r != 0 ? 1 : 0;
                    case ExprTokenKind.Equal: return l == r ? 1 : 0;
                    case ExprTokenKind.NotEqual: return l != r ? 1 : 0;
                    case ExprTokenKind.LessOrEqual: return l <= r ? 1 : 0;
                    case ExprTokenKind.Less: return l < r ? 1 : 0;
                    case ExprTokenKind.GreaterOrEqual: return l >= r ? 1 : 0;
                    case ExprTokenKind.Greater: return l > r ? 1 : 0;
                    case ExprTokenKind.Plus: return unchecked(l + r);
                    case ExprTokenKind.Minus: return unchecked(l - r);
                    case ExprTokenKind.Star: return unchecked(l * r);
                    case ExprTokenKind.Slash:
                        if (r == 0) throw new ExprEvaluationException("Delenie nulou", b);
                        return l == int.MinValue && r == -1 ? int.MinValue : l / r;
                    case ExprTokenKind.Percent:
                        if (r == 0) throw new ExprEvaluationException("Delenie nulou", b);
                        return r == -1 ? 0 : l % r;
                    default:
                        throw new ExprEvaluationException("Interní chyba překladu", b);
                }
            }

            case ExprConditionalNode c:
                return Evaluate(c.Condition) != 0 ? Evaluate(c.WhenTrue) : Evaluate(c.WhenFalse);

            case ExprFunctionNode f:
                return EvaluateFunction(f);

            default:
                throw new ExprEvaluationException("Interní chyba překladu", node);
        }
    }

    private int EvaluateFunction(ExprFunctionNode f)
    {
        var t = _train ?? throw new ExprEvaluationException("Vyhodnotenie funkcie vyžaduje vlak", f);
        var arg = f.Argument;

        switch (f.Canonical)
        {
            case ExprFunction.CVLAKU:
                if (arg is null) return Strtol10(t.TrainNumber);
                if (arg is ExprStringNode s) return string.Equals(t.TrainNumber, s.Value, StringComparison.Ordinal) ? 1 : 0;
                return Strtol10(t.TrainNumber) == Evaluate(arg) ? 1 : 0;

            case ExprFunction.POZICE:
                return ValueOrEquals(t.Position, arg);

            case ExprFunction.TYP:
                return ValueOrEquals(t.TrainTypeIndex, arg);

            case ExprFunction.PRIZNAK:
                return arg is null ? (int)(t.Flags & 0xFFFFFF00u) : (int)(t.Flags & (uint)Evaluate(arg));

            case ExprFunction.STAV:
            {
                if (arg is null) return (int)t.State;
                var mask = (uint)Evaluate(arg);
                return (t.State & mask) == mask ? 1 : 0;
            }

            case ExprFunction.NAKLTYP:
                return ExprTrainTypes.IsCargo(t.TrainTypeIndex) ? 1 : 0;

            case ExprFunction.VYLUKAZDE:
            {
                var mask = _site switch
                {
                    ExprEvalSite.ArrivalTable => 0x100u,
                    ExprEvalSite.DepartureTable => 0x200u,
                    _ => 0x300u
                };
                return (t.Flags & mask) != 0 ? 1 : 0;
            }

            case ExprFunction.ODKLON:
                return t.IsDeflected ? 1 : 0;

            case ExprFunction.ZPOZDENIPRIJ:
                return t.ArrivalDelaySeconds / 60;

            case ExprFunction.ZPOZDENIODJ:
                return t.DepartureDelaySeconds / 60;

            case ExprFunction.ZPOZDENI:
                return _site switch
                {
                    ExprEvalSite.ArrivalTable => t.ArrivalDelaySeconds / 60,
                    ExprEvalSite.DepartureTable => t.DepartureDelaySeconds / 60,
                    _ => Math.Max(t.ArrivalDelaySeconds / 60, t.DepartureDelaySeconds / 60)
                };

            case ExprFunction.DOBAPOBYTU:
                return t.StayTimeSeconds / 60;

            case ExprFunction.PLANDOBAPOBYTU:
                return t.PlannedStayTimeSeconds / 60;

            case ExprFunction.ZAJMSTANICE:
                return ValueOrEquals(t.HomeStationId, arg);

            case ExprFunction.VYCHSTANICE:
                return ValueOrEquals(t.BaseStationId, arg);

            case ExprFunction.CILSTANICE:
                return ValueOrEquals(t.EndStationId, arg);

            case ExprFunction.MISTNI:
                return t.IsLocal ? 1 : 0;

            case ExprFunction.CIZI:
                return t.IsLocal ? 0 : 1;

            case ExprFunction.ZESMERU:
                return t.IsFromStation(Evaluate(arg!)) ? 1 : 0;

            case ExprFunction.DOSMERU:
                return t.IsToStation(Evaluate(arg!)) ? 1 : 0;

            case ExprFunction.KOLEJPRIJ:
                return string.Equals(t.ArrivalTrack, StringArg(f), StringComparison.Ordinal) ? 1 : 0;

            case ExprFunction.KOLEJODJ:
                return string.Equals(t.DepartureTrack, StringArg(f), StringComparison.Ordinal) ? 1 : 0;

            case ExprFunction.DATUMPRIJ:
                return OleDate(t.ArrivalDate);

            case ExprFunction.DATUMODJ:
                return OleDate(t.DepartureDate);

            case ExprFunction.CASPRIJ:
                return Seconds(t.ArrivalTime);

            case ExprFunction.CASODJ:
                return Seconds(t.DepartureTime);

            case ExprFunction.INDCAT6:
                return IndCat6(t.Position, t.Flags);

            case ExprFunction.INDCAT8:
                return IndCat8(t.Position, t.Flags);

            case ExprFunction.DATE:
                return OleDate(DateOnly.FromDateTime(_now));

            case ExprFunction.TIME:
                return Seconds(TimeOnly.FromDateTime(_now));

            case ExprFunction.OPERATOR:
                return CzechEquals(t.OperatorName, StringArg(f)) ? 1 : 0;

            case ExprFunction.LINKAPRIJ:
                return string.Equals(t.ArrivalLine, StringArg(f), StringComparison.Ordinal) ? 1 : 0;

            case ExprFunction.LINKAODJ:
                return string.Equals(t.DepartureLine, StringArg(f), StringComparison.Ordinal) ? 1 : 0;

            case ExprFunction.ZMENALINKY:
                return string.Equals(t.ArrivalLine, t.DepartureLine, StringComparison.Ordinal) ? 0 : 1;

            case ExprFunction.NAZVLAKU:
                return CzechEquals(t.TrainName, StringArg(f)) ? 1 : 0;

            default:
                throw new ExprEvaluationException("Interní chyba překladu", f);
        }
    }

    private int ValueOrEquals(int value, ExprNode? arg) =>
        arg is null ? value : value == Evaluate(arg) ? 1 : 0;

    private static string StringArg(ExprFunctionNode f) =>
        f.Argument is ExprStringNode s ? s.Value : throw new ExprEvaluationException("Interní chyba překladu", f);

    /// <summary>
    ///     Kategoria pre sestkategoriovy diagram: 1 V, 2 P, 3 K; 4 V s vylukou odchodu,
    ///     5 P s akoukolvek vylukou, 6 K s vylukou prichodu; -1 pri neznamej pozicii.
    /// </summary>
    public static int IndCat6(int position, uint flags) => position switch
    {
        1 => (flags & 0x200) != 0 ? 4 : 1,
        2 => (flags & 0x300) != 0 ? 5 : 2,
        3 => (flags & 0x100) != 0 ? 6 : 3,
        _ => -1
    };

    /// <summary>
    ///     Kategoria pre osemkategoriovy diagram: ako <see cref="IndCat6"/>, ale prechadzajuci vlak
    ///     rozlisuje vyluku len prichodu (5), oboch (6) a len odchodu (7); K s vylukou prichodu je 8.
    /// </summary>
    public static int IndCat8(int position, uint flags) => position switch
    {
        1 => (flags & 0x200) != 0 ? 4 : 1,
        2 => (flags & 0x300) switch { 0x100 => 5, 0x300 => 6, 0x200 => 7, _ => 2 },
        3 => (flags & 0x100) != 0 ? 8 : 3,
        _ => -1
    };

    /// <summary>OLE datum - pocet dni od 30. 12. 1899.</summary>
    public static int OleDate(DateOnly d) => d.DayNumber - ExprLexer.OleEpoch.DayNumber;

    /// <summary>Pocet sekund od polnoci.</summary>
    public static int Seconds(TimeOnly t) => (t.Hour * 60 + t.Minute) * 60 + t.Second;

    /// <summary><c>strtol(s, NULL, 10)</c> - veduce medzery, znamienko a cislice; zvysok sa ignoruje.</summary>
    public static int Strtol10(string s)
    {
        var i = 0;
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        var neg = false;
        if (i < s.Length && s[i] is '+' or '-')
        {
            neg = s[i] == '-';
            i++;
        }
        long v = 0;
        while (i < s.Length && char.IsAsciiDigit(s[i]))
        {
            v = v * 10 + (s[i] - '0');
            if (v > int.MaxValue) return neg ? int.MinValue : int.MaxValue;
            i++;
        }
        return (int)(neg ? -v : v);
    }

    /// <summary>
    ///     Porovnanie ako v INISS: ceske triedenie bez ohladu na velkost pismen.
    /// </summary>
    public static bool CzechEquals(string a, string b) =>
        Czech.Compare(a, b, CompareOptions.IgnoreCase) == 0;
}
