using System.Globalization;
using System.Text.RegularExpressions;

namespace ToolsCore.Iniss.Calendar;

/// <summary>
/// Texty poznamok (SK/CZ) a ich vyber podla jazyka.
/// </summary>
public partial class DateLimit
{
    /// <summary>
    /// Kluce k textom
    /// </summary>
    public enum Message
    {
        Error,
        Empty,

        RunsDaily,
        RunsNever,
        RunsNeverAlt,
        RunsNext,
        RunsNot,
        RunsNotAlt,
        Runs,
        RunsAlt,

        From,
        To,
        And,
        On,

        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,

        Workday,
        Holiday,

        Jan,
        Feb,
        Mar,
        Apr,
        May,
        Jun,
        Jul,
        Aug,
        Sep,
        Oct,
        Nov,
        Dec
    }

    private static readonly string[] MessagesCz =
    [
        // Error, Empty
        "chyba", "",
        // RunsDaily .. RunsAlt
        "jede denně", "t.č. nejede", "nikdy", "zatím nejede", "nejede ", "kromě ", "jede ", "včetně ",
        // From, To, And, On
        "od ", "do ", " a ", "v ",
        // Monday .. Holiday
        "1", "2", "3", "4", "5", "6", "7", "X", "+",
        // Jan .. Dec
        "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"
    ];

    private static readonly string[] MessagesSk =
    [
        // Error, Empty
        "chyba", "",
        // RunsDaily .. RunsAlt
        "ide denne", "t.č. nejde", "nikdy", "zatiaľ nejde", "nejde ", "okrem ", "ide ", "vrátane ",
        // From, To, And, On
        "od ", "do ", " a ", "v ",
        // Monday .. Holiday
        "1", "2", "3", "4", "5", "6", "7", "X", "+",
        // Jan .. Dec
        "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"
    ];

    /// <summary>
    /// Vrati text správy.
    /// </summary>
    public static string MsgText(Message message)
    {
        return Loc switch
        {
            Locale.Cz => MessagesCz[(int)message],
            Locale.Sk => MessagesSk[(int)message],
            _ => "?"
        };
    }

    /// <summary>
    /// Text správy so zohlednenim alternativnej formulacie.
    /// </summary>
    public string AltMsgText(Message msg1, Message msg2) => MsgText(AltForm ? msg2 : msg1);

    /// <summary>
    /// Vrati, ci token zodpoveda niektorej zo zadanych sprav.
    /// </summary>
    private static bool TokenIsMsg(string token, params Message[] msgs)
    {
        foreach (var msg in msgs)
        {
            if (string.Equals(token, MsgText(msg).Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            if (!MessagePatterns.TryGetValue(msg, out var pattern))
                continue;

            // jednoslovny vzor nesmie zabrat viacslovny token
            if (token.Contains(' ') && !pattern.Contains(' '))
                continue;

            if (Regex.IsMatch(token, pattern))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Vrati cislo mesiaca ako rimsku cislicu alebo ako cislo podla nastavenia.
    /// </summary>
    private string MsgMonth(int month) => _monthRoman ? MsgText(Message.Jan + month - 1) : month.ToString(CultureInfo.CurrentCulture);

    /// <summary>
    /// Vrati nazov typu dna, pripadne obaleny znackami {}. Dlzka znaciek sa pripocita
    /// k <see cref="_marksLength"/>, aby sa nezapocitala do dlzky poznamky.
    /// </summary>
    private string MsgDayType(Message message)
    {
        if (!InsertMarks)
            return MsgText(message);

        _marksLength += 2;
        return "{" + MsgText(message) + "}";
    }
}
