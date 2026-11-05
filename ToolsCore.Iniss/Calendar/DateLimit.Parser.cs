using System.Globalization;
using System.Text.RegularExpressions;
using ToolsCore.Iniss.Properties;

namespace ToolsCore.Iniss.Calendar;

/// <summary>
/// Prevod textu poznamky na mapu dni (BitArray).
/// </summary>
public partial class DateLimit
{
    /// <summary>
    /// Rozlozi text poznamky na useky a vysledok zapise do bitoveho pola.
    /// </summary>
    private void ParseText()
    {
        var state = new ParseState();

        while (SkipWhiteSpace())
        {
            var token = ExtractToken();

            if (token != "," && TryReadToken(token, state))
            {
                _position += token.Length;
                continue;
            }

            // ciarka mimo zoznamu dni ukoncuje usek
            if (state.DateLevel != DateLevel.On)
            {
                FlushData(state, false);
                _position += token.Length;
                continue;
            }

            // v zozname dni sa usek ukonci az vtedy, ked dalsi token uz nie je pevny kod dna
            if (!NextTokenIsDayCode(token))
                FlushData(state, false);

            _position += token.Length;
        }

        FlushData(state, false);
        ApplyParsedData(state.Level);
    }

    /// <summary>
    /// Spracuje jeden token textu poznamky.
    /// </summary>
    /// <returns>
    /// <see langword="false"/>, ak token len prepol parser do rezimu zoznamu dni a este sa ma
    /// posudit v kontexte nasledujuceho tokenu.
    /// </returns>
    private bool TryReadToken(string token, ParseState state)
    {
        if (TokenIsMsg(token, Message.And))
        {
            // "v 6 a 7" - spojka v zozname dni ho len predlzuje, usek ukonci az pred datumom
            if (state.DateLevel != DateLevel.On || !NextTokenIsDayCode(token))
                FlushData(state, true);
        }
        else if (TokenIsMsg(token, Message.Runs, Message.RunsAlt))
        {
            FlushData(state, false);
            state.Level = Level.Runs;
            state.DateLevel = DateLevel.Date;
        }
        else if (TokenIsMsg(token, Message.RunsNot, Message.RunsNotAlt))
        {
            FlushData(state, false);
            state.Level = Level.RunsNot;
            state.DateLevel = DateLevel.Date;
        }
        else if (TokenIsMsg(token, Message.From))
            state.DateLevel = DateLevel.From;
        else if (TokenIsMsg(token, Message.To))
            state.DateLevel = DateLevel.To;
        else if (TokenIsMsg(token, Message.On))
        {
            state.DateLevel = DateLevel.On;
            state.Days = DayType.None;
        }
        else if (state.DateLevel == DateLevel.On)
            state.Days |= GetDayType(token);
        else if (IsDayType(token) || IsDayRange(token))
        {
            // pevny kod dna bez uvodnej predlozky
            state.DateLevel = DateLevel.On;
            state.Days = DayType.None;
            return false;
        }
        else
            ReadDate(token, state);

        return true;
    }

    /// <summary>
    /// Zapise datum z tokenu do stavu parsera podla toho, ci ide o "od", "do", alebo o jeden den.
    /// </summary>
    private void ReadDate(string token, ParseState state)
    {
        var date = GetDate(token, state.DateLevel == DateLevel.To);

        switch (state.DateLevel)
        {
            case DateLevel.From:
                state.From = date;
                state.SingleDate = false;
                break;
            case DateLevel.To:
                state.To = date;
                state.SingleDate = false;
                break;
            default:
                state.From = date;
                state.To = date;
                state.SingleDate = true;
                break;
        }
    }

    /// <summary>
    /// Nazrie za zadany token bez posunutia pozicie a vrati, ci nasledujuci token este patri do
    /// zoznamu pevnych kodov dni (jednoznakovy kod alebo rozsah tvaru "1-5").
    /// </summary>
    private bool NextTokenIsDayCode(string token)
    {
        var position = _position;
        _position += token.Length;

        var next = SkipWhiteSpace() ? ExtractToken() : "??";
        _position = position;

        return next.Length <= 1 || next.Contains('-') && next.Length == 3;
    }

    /// <summary>
    /// Ulozi rozparsovany usek a pripravi stav parsera na dalsi usek.
    /// </summary>
    /// <param name="state">Stav parsera.</param>
    /// <param name="and">Usek je s nasledujucim usekom spojeny spojkou "a".</param>
    private void FlushData(ParseState state, bool and)
    {
        if (state.From == DateTime.MinValue && state.To == DateTime.MinValue && state.Days == DayType.None)
            return;

        var data = new ParseData
        {
            From = state.From,
            To = state.To,
            Level = state.Level,
            Days = state.Days,
            And = and,
            SingleDate = state.SingleDate
        };

        // pevne kody dni sa uvadzaju az za poslednym usekom, plati vsak pre vsetky obdobia spojene spojkou "a".
        // samostatny datum pred "a" (Export3: "ide 26.XII. a od 26.III. v 7") nimi obmedzeny nie je
        if (state.Days != DayType.None)
            for (var i = _parsedData.Count - 1; i >= 0 && _parsedData[i].And && _parsedData[i].Days == DayType.None; i--)
                if (!_parsedData[i].SingleDate)
                    _parsedData[i].Days = state.Days;

        _parsedData.Add(data);

        state.From = DateTime.MinValue;
        state.To = DateTime.MinValue;
        state.Days = DayType.None;
        state.DateLevel = DateLevel.Date;
        state.SingleDate = false;
    }

    /// <summary>
    /// Prenesie vsetky rozparsovane useky do bitoveho pola.
    /// </summary>
    private void ApplyParsedData(Level level)
    {
        if (_parsedData.Count == 0)
            FlushData(new ParseState { Level = level, From = DateFrom }, false);

        // poznamka zacinajuca "nejde" znamena, ze vlak inak ide kazdy den
        if (_parsedData[0].Level == Level.RunsNot)
            _bits!.SetAll(true);

        // jednotlive datumy maju prednost pred obdobiami, aj ked su zapisane skor
        // (Export3: "ide v 7,1.IX.,nejde od 25.VIII. do 6.IX." - 1.IX. ide)
        foreach (var parseData in _parsedData.Where(d => !d.SingleDate).Concat(_parsedData.Where(d => d.SingleDate)))
        {
            if (parseData.From == DateTime.MinValue)
                parseData.From = DateFrom;

            if (parseData.To == DateTime.MinValue)
                parseData.To = DateTo;

            var lastDay = DateDiff(DateFrom, parseData.To);

            for (var day = DateDiff(DateFrom, parseData.From); day <= lastDay; day++)
                if (day >= 0 && day <= MaxDay &&
                    (parseData.Days == DayType.None || (GetDayType(day, true) & parseData.Days) != DayType.None))
                    _bits![day] = parseData.Level == Level.Runs;
        }
    }

    /// <summary>
    /// Posunie poziciu na najblizsi neprazdny znak.
    /// </summary>
    /// <returns><see langword="true"/>, ak v texte este nejaky znak zostal.</returns>
    private bool SkipWhiteSpace()
    {
        while (_position < _text.Length && _text[_position] <= ' ')
            _position++;

        return _position < _text.Length;
    }

    /// <summary>
    /// Vrati token na aktualnej pozicii bez toho, aby poziciu posunul. Tokenom je bud samotna
    /// ciarka, alebo znaky az po najblizsiu medzeru ci ciarku.
    /// </summary>
    private string ExtractToken()
    {
        var pos = _position + 1;

        while (pos < _text.Length && _text[pos] > ' ' && _text[pos] != ',' && _text[_position] != ',')
            pos++;

        return _text.Substring(_position, pos - _position);
    }

    /// <summary>
    /// Prevedie token na datum. Rok sa v poznamke uvadzat nemusi - vtedy sa odvodi z platnosti grafikonu.
    /// </summary>
    /// <param name="token">Token s datumom.</param>
    /// <param name="checkLast">Token je koncovym datumom obdobia.</param>
    private DateTime GetDate(string token, bool checkLast)
    {
        if (token.Contains('-'))
            throw new ParseException(Resources.DateLimit_UseFromTo, _position);

        var dotIndex = token.IndexOf('.');

        if (dotIndex < 0 || !int.TryParse(token.AsSpan(0, dotIndex), out var day) || day is <= 0 or > 31)
            throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_BadDate, token), _position);

        string rest;

        if (dotIndex + 1 < token.Length)
            rest = token[(dotIndex + 1)..];
        else
        {
            // den je uvedeny samostatne (napr. "1.,5.I."), mesiac sa hlada v zvysku textu
            rest = _text[(_position + token.Length)..];
            var match = Regex.Match(rest, "\\.[0-9IXV]+\\.");

            if (!match.Success)
                rest = "";
            else
                rest = match.Index + match.Value.Length + 4 > rest.Length
                    ? match.Value[1..]
                    : rest.Substring(match.Index + 1, match.Value.Length + 3);
        }

        var monthEnd = rest.IndexOf('.');

        if (monthEnd < 0)
            throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_BadDate, token), _position);

        var month = GetMonth(rest[..monthEnd]);
        var yearSet = int.TryParse(rest.AsSpan(monthEnd + 1), out var year) && year is >= 2000 and < 2100;

        if (!yearSet)
            year = month < DateFrom.Month || month == DateFrom.Month && day < DateFrom.Day
                ? DateFrom.Year + 1
                : DateFrom.Year;

        var date = CreateDate(year, month, day, token);

        if (date > DateTo && !yearSet)
        {
            if (checkLast)
            {
                if (_skipDateRangeCheck)
                    return DateTo;

                throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_EndOutOfRange, FormatDate(date)), _position);
            }

            date = CreateDate(year - 1, month, day, token);
        }

        if (date >= DateFrom && date <= DateTo)
            return date;

        if (!_skipDateRangeCheck)
            throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_OutOfRange, FormatDate(date)), _position);

        // datum bez roku sa posunie o rok dopredu, ak tak lezi blizsie k platnosti grafikonu
        if (!yearSet && date < DateFrom &&
            DateFrom.Subtract(date).TotalDays > date.AddYears(1).Subtract(DateTo).TotalDays)
            date = date.AddYears(1);

        return date;
    }

    /// <summary>
    /// Vytvori datum a neplatnu kombinaciu prevedie na <see cref="ParseException"/>.
    /// </summary>
    private DateTime CreateDate(int year, int month, int day, string token)
    {
        try
        {
            return new DateTime(year, month, day);
        }
        catch (Exception)
        {
            throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_BadDate, token), _position);
        }
    }

    /// <summary>
    /// Prevedie cislo mesiaca alebo jeho rimsku cislicu na cislo mesiaca.
    /// </summary>
    private int GetMonth(string month)
    {
        if (int.TryParse(month, out var number))
            return number;

        // rimske cislice mesiacov su na konci pola sprav, hlada sa az od nich (znak "X" je aj kodom pracovneho dna)
        var index = Array.IndexOf(MessagesCz, month.ToUpper(CultureInfo.CurrentCulture), (int)Message.Jan);

        if (index is < (int)Message.Jan or > (int)Message.Dec)
            throw new ParseException(string.Format(CultureInfo.CurrentCulture, Resources.DateLimit_BadMonth, month), _position);

        return index - (int)Message.Jan + 1;
    }

    /// <summary>
    /// Chyba pri parsovani textu poznamky.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message, int pos) : base(message) => Position = pos;

        public int Position { get; }
    }

    /// <summary>
    /// Priebezny stav parsera textovej poznamky.
    /// </summary>
    private sealed class ParseState
    {
        public DayType Days;
        public DateLevel DateLevel = DateLevel.Date;
        public DateTime From;
        public Level Level = Level.Runs;
        public bool SingleDate;
        public DateTime To;
    }

    /// <summary>
    /// Jeden rozparsovany usek poznamky.
    /// </summary>
    private class ParseData
    {
        public bool And;
        public DayType Days;
        public DateTime From;
        public Level Level = Level.Runs;

        /// <summary>Usek je jeden datum zapisany bez "od"/"do".</summary>
        public bool SingleDate;

        public DateTime To;
    }
}
