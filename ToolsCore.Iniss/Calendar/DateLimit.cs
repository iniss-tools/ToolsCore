using System.Collections;
using System.Globalization;
using ToolsCore.Iniss.Properties;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ToolsCore.Iniss.Calendar;

/// <summary>
/// Trieda pre zpracovavanie bitoveho pola do poznamky a naopak.
/// </summary>
public partial class DateLimit
{

    /// <summary>
    /// Pevne kody dni tak, ako sa zapisuju v poznamke. Poradie znakov zodpoveda poradiu bitov
    /// v <see cref="DayType"/>, teda '1' je pondelok, 'X' pracovny den a '+' sviatok.
    /// </summary>
    private const string WeekDaySigns = "1234567";

    private const string DayTypeSigns = WeekDaySigns + "X+";

    /// <summary>Pocet poloziek <see cref="DayIndex"/> (pondelok az sviatok).</summary>
    private const int DayIndexCount = (int)DayIndex.Holiday + 1;

    /// <summary>Minimalna dlzka useku, s ktorou zacina delenie celeho grafikonu.</summary>
    private const int InitialMinRunLength = 80;

    /// <summary>Minimalna dlzka suvisleho useku na zaciatku/konci intervalu, ktory sa oddeli samostatne.</summary>
    private const int MinEdgeRunLength = 14;

    /// <summary>Minimalna dlzka suvisleho useku vnutri intervalu, okolo ktoreho sa interval rozdeli.</summary>
    private const int MinInnerRunLength = 30;

    /// <summary>Najvyssi pocet dni jedneho typu, ktore smu vybocovat z tyzdenneho vzoru.</summary>
    private const int MaxBadDays = 8;

    /// <summary>Najvyssi pocet vynimiek z tyzdenneho vzoru.</summary>
    private const int MaxExceptions = 13;

    /// <summary>Najvyssi pocet vynimiek z tyzdenneho vzoru pre obdobie dlhsie ako <see cref="LongPeriodDays"/>.</summary>
    private const int MaxExceptionsLongPeriod = 19;

    private const int LongPeriodDays = 350;

    /// <summary>Najvyssi pocet jednotlivych dni na okraji intervalu, ktore sa oddelia od zvysku.</summary>
    private const int MaxIsolatedDays = 6;

    /// <summary>Najkratsia medzera bez jazdy, za ktorou sa jednotlive dni na okraji intervalu oddelia.</summary>
    private const int MinIsolationGap = 28;

    /// <summary>Najvacsia medzera medzi dvoma useky, ktore este mozno spojit do jedneho.</summary>
    private const int MaxMergeGap = 60;

    private readonly StringBuilder _builder;

    private readonly List<ParseData> _parsedData;

    private readonly bool _allowRunsDaily;

    private readonly bool _fromToday;

    private readonly int _maxDays;

    private readonly bool _monthRoman;

    private readonly bool _skipDateRangeCheck;

    private readonly bool _specDays;

    private BitArray? _bits;

    /// <summary>Dlzka znaciek {}, ktore sa nepocitaju do dlzky vyslednej poznamky.</summary>
    private int _marksLength;

    /// <summary>Naposledy vypisany mesiac - sluzi na potlacenie jeho opakovania.</summary>
    private string? _lastMonth;

    private int _position;

    private string _text = null!;

    /// <summary>
    /// Jazyk generovaných datumových obmedzeni.
    /// </summary>
    public enum Locale
    {
        Cz,
        Sk
    }

    /// <summary>
    /// Vzory, ktorymi sa pri parsovani rozpoznavaju sprAvy aj v skratenom alebo inojazycnom tvare.
    /// Spravy, ktore v zozname nie su, sa porovnavaju len na presnu zhodu.
    /// </summary>
    private static readonly Dictionary<Message, string> MessagePatterns = new()
    {
        { Message.RunsDaily, "^jede denně|^ide denne" },
        { Message.RunsNever, @"^t\.č\. ne" },
        { Message.RunsNext, "^zat" },
        { Message.RunsNot, "^n" },
        { Message.RunsNotAlt, "^kr|^ok" },
        { Message.Runs, "^j|^i" },
        { Message.RunsAlt, "^vč|^vr" },
        { Message.From, "^o" },
        { Message.To, "^d" }
    };

    /// <summary>
    /// Vytvori novu instanciu triedy <see cref="DateLimit"/>.
    /// </summary>
    /// <param name="from">Pociatocny datum platnosti GVD.</param>
    /// <param name="to">Koncovy datum platnosti GVD.</param>
    /// <param name="specDays">Pouzivat sviatky a pracovne dni.</param>
    /// <param name="allowRunsDaily">Vracat tiez - ide denne.</param>
    /// <param name="fromToday">Zohladnit az vzhladom ku dnesku.</param>
    /// <param name="insertMarks">Vkladat znacky {}.</param>
    /// <param name="maxDays">Pocet dni do buducnosti (maximalny poradovy index dna).</param>
    /// <param name="monthRoman">Cisla mesiacov rimskymi cislicami.</param>
    /// <param name="skipDateRangeCheck">Preskakovat u datumu chyby pre datum mimo grafikonu.</param>
    /// <param name="altForm">Pouzit zkrateny tvar poznamky.</param>
    /// <param name="today">Ktory datum pouzit ako dnesok (ak sa neuvedie, pouzije sa skutocny dnesok).</param>
    public DateLimit(DateOnly from, DateOnly to,
        bool specDays = true, bool allowRunsDaily = false,
        bool fromToday = false, bool insertMarks = true,
        int maxDays = 0, bool monthRoman = true,
        bool skipDateRangeCheck = false, bool altForm = false,
        DateTime? today = null)
        : this(from.ToDateTime(TimeOnly.MinValue), to.ToDateTime(TimeOnly.MinValue), specDays, allowRunsDaily, fromToday, insertMarks, maxDays, monthRoman,
            skipDateRangeCheck, altForm, today)
    {
    }

    /// <inheritdoc cref="DateLimit(DateOnly, DateOnly, bool, bool, bool, bool, int, bool, bool, bool, DateTime?)" />
    public DateLimit(DateTime from, DateTime to,
        bool specDays = true, bool allowRunsDaily = false,
        bool fromToday = false, bool insertMarks = true,
        int maxDays = 0, bool monthRoman = true,
        bool skipDateRangeCheck = false, bool altForm = false,
        DateTime? today = null)
    {
        if (to < from)
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_ToBeforeFrom, to, from));

        DateFrom = from;
        DateTo = to;
        AltForm = altForm;
        Today = today ?? DateTime.Today;

        _builder = new StringBuilder();
        _parsedData = [];

        _specDays = specDays;
        _allowRunsDaily = allowRunsDaily;
        _fromToday = fromToday;
        InsertMarks = insertMarks;
        _maxDays = maxDays;
        _monthRoman = monthRoman;
        _skipDateRangeCheck = skipDateRangeCheck;
    }

    /// <summary>
    /// Jazyk generovanych poznamok.
    /// </summary>
    public static Locale Loc { get; set; } = Locale.Sk;

    /// <summary>
    /// Krajina, ktorej sviatky plati pre tuto poznamku. Bez hodnoty plati <see cref="Loc" />.
    /// </summary>
    public Locale? HolidayLocale { get; init; }

    /// <summary>
    /// Urcuje, ci sa nazvy typov dni obalia znackami {}.
    /// </summary>
    public bool InsertMarks { get; set; }

    /// <summary>
    /// Vrati celkovy pocet dni grafikonu.
    /// </summary>
    public int TotalDays => MaxDay + 1;

    /// <summary>
    /// Vrati maximalny poradovy index dna.
    /// </summary>
    public int MaxDay => DateDiff(DateFrom, DateTo);

    /// <summary>
    /// Vrati pociatocny datum.
    /// </summary>
    public DateTime DateFrom { get; private set; }

    /// <summary>
    /// Vrati koncovy datum.
    /// </summary>
    public DateTime DateTo { get; private set; }

    /// <summary>
    /// Obdobie platnosti ako text.
    /// </summary>
    public string TextFromTo => $"{FormatDate(DateFrom)} - {FormatDate(DateTo)}";

    /// <summary>
    /// Vrati priznak alternativneho tvaru textu.
    /// </summary>
    public bool AltForm { get; }

    /// <summary>
    /// Vrati datum pouzity ako "dnes".
    /// </summary>
    public DateTime Today { get; }

    /// <summary>
    /// Vytvori bitove pole pre priznaky ide/nejde.
    /// </summary>
    public BitArray CreateBitArray() => new(TotalDays);

    /// <summary>
    /// Formatuje datum do formatu "dd:MM:yyyy".
    /// </summary>
    public static string FormatDate(DateTime date) => date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Vrati vzdialenost medzi datumami ako <see cref="int"/>.
    /// </summary>
    public static int DateDiff(DateTime from, DateTime to) => (int)Math.Round((to - from).TotalDays);

    /// <summary>
    /// Konvertuje bitove pole do textovej poznamky.
    /// </summary>
    /// <param name="bits">Bitove pole.</param>
    /// <param name="cycle">Posunutie vzhladom k bitovemu polu.</param>
    /// <param name="validBits">Dni, ktore ma zmysel v poznamke uvadzat.</param>
    /// <returns>
    /// Text poznamky.
    /// </returns>
    public string BitArrayToText(BitArray? bits, int cycle = 0, BitArray? validBits = null)
    {
        if (bits == null || bits.Length != TotalDays)
            throw new ArgumentException(Resources.DateLimit_BadBitArray, nameof(bits));

        var originalFrom = DateFrom;
        var originalTo = DateTo;

        try
        {
            DateFrom = DateFrom.AddDays(cycle);
            DateTo = DateTo.AddDays(cycle);

            var minIndex = 0;
            var maxIndex = MaxDay;

            if (_maxDays > 0 && DateTo > Today.AddDays(_maxDays))
            {
                DateTo = Today.AddDays(_maxDays);

                if (DateTo < DateFrom)
                    return MsgText(Message.RunsNext);

                maxIndex = DateDiff(DateFrom, DateTo);
            }

            if (_fromToday)
            {
                if (Today > DateTo)
                    return AltMsgText(Message.RunsNever, Message.RunsNeverAlt);

                if (Today > DateFrom)
                {
                    minIndex = DateDiff(DateFrom, Today);
                    DateFrom = Today;
                }
            }

            if (!HasMixedBits(bits, minIndex, maxIndex))
                return bits[minIndex]
                    ? MsgText(_allowRunsDaily ? Message.RunsDaily : Message.Empty)
                    : AltMsgText(Message.RunsNever, Message.RunsNeverAlt);

            if (minIndex > 0 || maxIndex != bits.Length - 1)
            {
                if (validBits != null && validBits.Count == bits.Count)
                    validBits = Slice(validBits, minIndex, maxIndex);

                bits = Slice(bits, minIndex, maxIndex);
            }

            _bits = bits;

            var positive = FormatBits(false, out var positiveCount, out var positiveLength, validBits);
            var negative = FormatBits(true, out var negativeCount, out var negativeLength, validBits);

            // negativny tvar sa uprednostni len ak je vyrazne kratsi, alebo kratsi a zaroven jednoduchsi
            var negativeIsBetter =
                negativeLength + (positiveLength > 40 ? 20 : 25) < positiveLength ||
                negativeLength < positiveLength && negativeCount < positiveCount;

            return negativeIsBetter ? negative : positive;
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            DateFrom = originalFrom;
            DateTo = originalTo;
            _bits = null;
        }

        return "";
    }

    /// <summary>
    /// Konvertuje textovu poznamku do bitoveho pole.
    /// </summary>
    /// <param name="text">Text poznamky.</param>
    /// <returns>Text poznamky.</returns>
    public BitArray TextToBitArray(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text) || TokenIsMsg(text.Trim(), Message.RunsDaily, Message.Runs, Message.RunsAlt))
                return new BitArray(TotalDays, true);

            if (TokenIsMsg(text.Trim(), Message.RunsNever, Message.RunsNeverAlt, Message.RunsNot, Message.RunsNotAlt))
                return CreateBitArray();

            _bits = CreateBitArray();
            _text = text;
            _position = 0;
            _parsedData.Clear();

            ParseText();

            return _bits;
        }
        catch (ParseException)
        {
            throw;
        }
        catch (Exception)
        {
            return new BitArray(0);
        }
    }

    /// <summary>
    /// Vrati, ci sa datumove obmedzenia ako texty prekryvaju.
    /// </summary>
    /// <param name="dl1">Text datumoveho obmedenia.</param>
    /// <param name="dl2">Text datumoveho obmedenia.</param>
    public bool Overlap(string dl1, string dl2)
    {
        var limit = TextAnd(dl1, dl2);
        return limit != "" && limit != MsgText(Message.RunsNever) && limit != MsgText(Message.RunsNeverAlt);
    }

    /// <summary>
    /// Logicka operacia AND medzi textami.
    /// </summary>
    public string TextAnd(params string[] texts) => Combine((first, second) => first.And(second), texts);

    /// <summary>
    /// Logicka operacia OR medzi textami.
    /// </summary>
    public string TextOr(params string[] texts) => Combine((first, second) => first.Or(second), texts);

    /// <summary>
    /// Logicka operacia XOR medzi textami.
    /// </summary>
    public string TextXor(params string[] texts) => Combine((first, second) => first.Xor(second), texts);

    /// <summary>
    /// Logicka operacia NOT podla textu.
    /// </summary>
    public string TextNot(string text) => BitArrayToText(TextToBitArray(text).Not());

    /// <summary>
    /// Vykona logicku operaciu <paramref name="operation"/> nad bitovymi polami vsetkych textov
    /// a vysledok prevedie spat na text.
    /// </summary>
    private string Combine(Func<BitArray, BitArray, BitArray> operation, string[] texts)
    {
        switch (texts.Length)
        {
            case 0:
                return "";
            case 1:
                return texts[0];
        }

        var bits = TextToBitArray(texts[0]);

        for (var i = 1; i < texts.Length; i++)
            bits = operation(bits, TextToBitArray(texts[i]));

        return BitArrayToText(bits);
    }
}
