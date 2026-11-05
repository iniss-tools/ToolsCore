using System.Collections;

namespace ToolsCore.Iniss.Calendar;

/// <summary>
/// Prevod mapy dni (BitArray) na text poznamky - hladanie intervalov, tyzdennych vzorov a vynimiek.
/// </summary>
public partial class DateLimit
{
    /// <summary>
    /// Vrati, ci sa v rozsahu <paramref name="from"/>-<paramref name="to"/> vyskytuju obe hodnoty bitov.
    /// </summary>
    private static bool HasMixedBits(BitArray bits, int from, int to)
    {
        for (var i = from + 1; i <= to; i++)
            if (bits[i] != bits[from])
                return true;

        return false;
    }

    /// <summary>
    /// Vrati novu kopiu bitoveho pola orezanu na rozsah <paramref name="from"/>-<paramref name="to"/>.
    /// </summary>
    private static BitArray Slice(BitArray bits, int from, int to)
    {
        var result = new BitArray(to - from + 1);

        for (var i = from; i <= to; i++)
            result[i - from] = bits[i];

        return result;
    }

    /// <summary>
    /// Odstrani z obmedzenia jednotlive dni, ktore nepatria medzi platne dni.
    /// </summary>
    private static void ReduceDates(IList<DateLimitInfo> limits, BitArray validBits)
    {
        for (var i = limits.Count - 1; i >= 0; i--)
        {
            var info = limits[i];

            if (info.ListRuns is { Count: > 0 })
                ReduceDates(info.ListRuns, validBits);

            if (info.ListRunsNot is { Count: > 0 })
                ReduceDates(info.ListRunsNot, validBits);

            if (info is { Type: DayType.None, From: > 0 } && info.From == info.To && !validBits[info.From])
                limits.RemoveAt(i);
        }
    }

    /// <summary>
    /// Vytvori text pre aktualne bitove pole <see cref="_bits"/>.
    /// </summary>
    /// <param name="isNot">Spracovat negovane bitove pole, teda vytvorit zapis v tvare "nejde ...".</param>
    /// <param name="infosCount">Pocet useku vysledneho obmedzenia - mensi pocet znamena jednoduchsi zapis.</param>
    /// <param name="length">Dlzka vysledneho textu bez znaciek {}.</param>
    /// <param name="validBits">Dni, ktore ma zmysel v poznamke uvadzat.</param>
    private string FormatBits(bool isNot, out int infosCount, out int length, BitArray? validBits)
    {
        infosCount = 0;
        length = 0;

        if (isNot)
            _bits = _bits!.Not();

        try
        {
            var limits = ProcessInterval(InitialMinRunLength, 0, MaxDay);

            if (validBits != null && validBits.Length == _bits!.Length && limits.Count > 0)
                ReduceDates(limits, validBits);

            infosCount = limits.Count;
            _marksLength = 0;

            var formatted = Format(limits, isNot);

            if (string.IsNullOrEmpty(formatted))
                return formatted;

            if (AltForm)
            {
                // v skratenom tvare sa uvodne "ide" vypusta a pred zoznamom dni sa neopakuje predlozka
                if (formatted.StartsWith(MsgText(Message.RunsAlt), StringComparison.Ordinal))
                    formatted = formatted[MsgText(Message.RunsAlt).Length..];

                formatted = formatted.Replace(MsgText(Message.RunsAlt) + MsgText(Message.On), MsgText(Message.RunsAlt));
                formatted = formatted.Replace(MsgText(Message.RunsNotAlt) + MsgText(Message.On), MsgText(Message.RunsNotAlt));
            }

            length = formatted.Length - _marksLength;

            return formatted;
        }
        finally
        {
            if (isNot)
                _bits = _bits!.Not();
        }
    }

    /// <summary>
    /// Rozlozi interval <paramref name="from"/>-<paramref name="to"/> na useky datumoveho obmedzenia.
    /// </summary>
    /// <param name="minCount">Minimalna dlzka useku, ktory sa este oplati oddelit.</param>
    /// <param name="from">Zaciatok intervalu.</param>
    /// <param name="to">Koniec intervalu.</param>
    private List<DateLimitInfo> ProcessInterval(int minCount, int from, int to)
    {
        ReduceInterval(ref from, ref to);

        var limits = GetSingleDays(from, to) ?? SplitIsolatedDays(minCount, from, to);

        if (limits != null)
            return limits;

        var okCount = new DayCounter();
        var badCount = new DayCounter();
        var grouping = DayGrouping.None;

        // interval sa oplati skusit popisat tyzdennym vzorom len ak dni v tyzdni nie su vsetky rovnake
        var weekPattern = ScanDays(from, to, okCount, badCount, ref grouping) &&
                          !AllSet(okCount) && !badCount.AllZero && to - from > 6;

        if (!weekPattern)
        {
            limits = GetIntervals(from, to);

            if (limits != null)
                return limits;

            limits = SplitInterval(minCount, from, to);

            if (limits != null)
                return limits;
        }

        while (limits == null)
        {
            limits = ScanWeekDays(minCount, from, to);
            minCount -= Math.Max(minCount >> 1, 1);

            if (!weekPattern)
            {
                // rozdelenie ma prednost, ale ked sa nepodari, nesmie zahodit vysledok zo ScanWeekDays
                var split = SplitInterval(minCount, from, to);

                if (split != null)
                    return split;
            }
        }

        return limits;
    }

    /// <summary>
    /// Oddeli jednotlive dni na zaciatku alebo konci intervalu, ktore od zvysku deli dlha medzera,
    /// aby nerozbili tyzdenny vzor zvysku (napr. "ide 26.XII.,od 28.III. v 7").
    /// </summary>
    /// <returns><see langword="null"/>, ak take dni v intervale nie su.</returns>
    private List<DateLimitInfo>? SplitIsolatedDays(int minCount, int from, int to)
    {
        // najdlhsi zaciatok s najviac MaxIsolatedDays dnami jazdy, za ktorym nasleduje dlha medzera
        var leadingTo = -1;
        var runDays = 0;

        for (var day = from; day <= to && runDays < MaxIsolatedDays; day++)
        {
            if (RunsNot(day))
                continue;

            runDays++;
            var next = day + 1;

            while (next <= to && RunsNot(next))
                next++;

            if (next <= to && next - day - 1 >= MinIsolationGap)
                leadingTo = day;
        }

        // to iste od konca intervalu
        var trailingFrom = -1;
        runDays = 0;

        for (var day = to; day >= from && runDays < MaxIsolatedDays; day--)
        {
            if (RunsNot(day))
                continue;

            runDays++;
            var prev = day - 1;

            while (prev >= from && RunsNot(prev))
                prev--;

            if (prev >= from && day - prev - 1 >= MinIsolationGap)
                trailingFrom = day;
        }

        if (leadingTo < 0 && trailingFrom < 0)
            return null;

        var restFrom = leadingTo < 0 ? from : leadingTo + 1;
        var restTo = trailingFrom < 0 ? to : trailingFrom - 1;

        // okraje sa prekryvaju, alebo zvysok nema tyzdenny vzor, ktory by oddelenie okrajov zachranilo
        if (restFrom > restTo || !HasRuns(restFrom, restTo) || !HasWeekPattern(restFrom, restTo))
            return null;

        List<DateLimitInfo>? limits = null;

        if (leadingTo >= 0)
            AddIntervals(ref limits, ProcessInterval(minCount, from, leadingTo));

        AddIntervals(ref limits, ProcessInterval(minCount, restFrom, restTo));

        if (trailingFrom >= 0)
            AddIntervals(ref limits, ProcessInterval(minCount, trailingFrom, to));

        return limits;
    }

    /// <summary>
    /// Vrati, ci vlak v rozsahu <paramref name="from"/>-<paramref name="to"/> aspon raz ide.
    /// </summary>
    private bool HasRuns(int from, int to)
    {
        for (var day = from; day <= to; day++)
            if (Runs(day))
                return true;

        return false;
    }

    /// <summary>
    /// Vrati, ci sa jazda v rozsahu <paramref name="from"/>-<paramref name="to"/> riadi dnami v tyzdni -
    /// niektore typy dni su prevazne jazdne a ine prevazne nejazdne.
    /// </summary>
    private bool HasWeekPattern(int from, int to)
    {
        var okCount = new DayCounter();
        var badCount = new DayCounter();
        var grouping = DayGrouping.None;

        ReduceInterval(ref from, ref to);
        ScanDays(from, to, okCount, badCount, ref grouping);

        bool running = false, notRunning = false;

        for (var index = DayIndex.Monday; index <= DayIndex.Holiday; index++)
        {
            running |= okCount[index] > badCount[index] * 2;
            notRunning |= badCount[index] > okCount[index] * 2;
        }

        return running && notRunning;
    }

    /// <summary>
    /// Postupne skusi vsetky sposoby rozdelenia intervalu na kratsie useky.
    /// </summary>
    private List<DateLimitInfo>? SplitInterval(int minCount, int from, int to) =>
        SplitAtRunBlocks(minCount, from, to) ??
        SplitAtLongRun(minCount, from, to) ??
        SplitLeadingRun(minCount, from, to) ??
        SplitTrailingRun(minCount, from, to);

    /// <summary>
    /// Rozdeli interval na useky ohranicene dnami, kedy vlak nejde.
    /// </summary>
    /// <remarks>
    /// Povodny kod tu porovnaval dlzku jedineho dna s <paramref name="minCount"/>, takze usek
    /// vznikne az vtedy, ked <paramref name="minCount"/> klesne na 1 alebo nizsie.
    /// </remarks>
    private List<DateLimitInfo>? SplitAtRunBlocks(int minCount, int from, int to)
    {
        List<DateLimitInfo>? limits = null;
        var blockFrom = -1;

        for (var day = from; day <= to; day++)
            if (Runs(day))
            {
                if (blockFrom < 0)
                    blockFrom = day;
            }
            else if (minCount <= 1 && blockFrom >= 0)
            {
                AddIntervals(ref limits, ProcessInterval(minCount, blockFrom, day - 1));
                blockFrom = -1;
            }

        if (blockFrom >= 0 && limits != null)
            AddIntervals(ref limits, ProcessInterval(minCount, blockFrom, to));

        return limits;
    }

    /// <summary>
    /// Najde vnutri intervalu dostatocne dlhy suvisly usek a rozdeli interval na cast pred nim,
    /// samotny usek a cast za nim.
    /// </summary>
    private List<DateLimitInfo>? SplitAtLongRun(int minCount, int from, int to)
    {
        List<DateLimitInfo>? limits = null;
        var runFrom = -1;
        var runLength = 0;

        for (var day = from; day <= to; day++)
        {
            if (Runs(day))
            {
                runLength++;

                if (runFrom < 0)
                    runFrom = day;

                continue;
            }

            if (runLength >= MinInnerRunLength)
            {
                if (runFrom > from)
                    limits = ProcessInterval(minCount, from, runFrom - 1);

                AddInterval(ref limits, new DateLimitInfo(runFrom, day - 1));
                AddIntervals(ref limits, ProcessInterval(minCount, day, to));

                return limits;
            }

            runLength = 0;
            runFrom = -1;
        }

        return null;
    }

    /// <summary>
    /// Oddeli dostatocne dlhy suvisly usek na zaciatku intervalu.
    /// </summary>
    private List<DateLimitInfo>? SplitLeadingRun(int minCount, int from, int to)
    {
        var day = from;
        var runLength = 0;

        while (day <= to && Runs(day))
        {
            runLength++;
            day++;
        }

        if (runLength < MinEdgeRunLength)
            return null;

        List<DateLimitInfo>? limits = null;

        AddInterval(ref limits, new DateLimitInfo(from, from + runLength - 1));
        AddIntervals(ref limits, ProcessInterval(minCount, from + runLength, to));

        return limits;
    }

    /// <summary>
    /// Oddeli dostatocne dlhy suvisly usek na konci intervalu.
    /// </summary>
    private List<DateLimitInfo>? SplitTrailingRun(int minCount, int from, int to)
    {
        var day = to;
        var runLength = 0;

        while (day >= from && Runs(day))
        {
            runLength++;
            day--;
        }

        if (runLength < MinEdgeRunLength)
            return null;

        var limits = ProcessInterval(minCount, from, to - runLength);
        AddInterval(ref limits, new DateLimitInfo(to - runLength + 1, to));

        return limits;
    }

    /// <summary>
    /// Hlada najdlhsi usek, ktory sa da popisat tyzdennym vzorom (napr. "ide 1-5") spolu so zoznamom
    /// vynimiek z neho. Zvysok intervalu spracuje rekurzivne.
    /// </summary>
    private List<DateLimitInfo>? ScanWeekDays(int minCount, int from, int to)
    {
        List<DateLimitInfo>? limits = null;
        var okCount = new DayCounter();
        var badCount = new DayCounter();

        for (var lastDay = to; lastDay >= from + 7; lastDay--)
        {
            if (lastDay < to && RunsNot(lastDay))
                continue;

            var grouping = DayGrouping.None;
            ScanDays(from, lastDay, okCount, badCount, ref grouping);

            // do vzoru sa dostanu len tie typy dni, ktore v useku vyrazne prevazuju
            var hasPattern = false;

            for (var index = DayIndex.Monday; index <= DayIndex.Holiday; index++)
                if (okCount[index] > badCount[index] * 2 && badCount[index] <= MaxBadDays)
                {
                    okCount[index] = 1;
                    hasPattern = true;
                }
                else
                    okCount[index] = 0;

            if (hasPattern)
            {
                // useky, ktore idu nad ramec vzoru
                var extraRuns = 0;

                for (var day = from; day <= lastDay; day++)
                    if (Runs(day) && okCount[GetDayIndex(day, grouping)] == 0 &&
                        (day == from || RunsNot(day - 1) || okCount[GetDayIndex(day - 1, grouping)] != 0))
                        extraRuns++;

                // useky dni vzoru, v ktorych vlak nejde
                var missingRuns = 0;
                var scan = from;

                while (scan <= lastDay)
                    if (okCount[GetDayIndex(scan, grouping)] != 0 && RunsNot(scan))
                    {
                        missingRuns++;

                        while (scan <= lastDay && RunsNot(scan))
                            scan++;
                    }
                    else
                        scan++;

                var exceptions = extraRuns + missingRuns;

                if (exceptions <= MaxExceptions ||
                    lastDay - from > LongPeriodDays && exceptions <= MaxExceptionsLongPeriod)
                {
                    if (missingRuns > 0)
                    {
                        // usek nesmie koncit dnom vzoru, v ktorom vlak nejde
                        var day = lastDay;

                        while (okCount[GetDayIndex(day, grouping)] == 0)
                            day--;

                        if (RunsNot(day))
                            continue;
                    }

                    // ak sa vzor opakuje aj v nasledujucich tyzdnoch, usek este nekonci tu
                    if (exceptions > 0 && lastDay < to - 21 && lastDay > 13 &&
                        (EqualPattern(lastDay - 6, lastDay + 1) && EqualPattern(lastDay - 6, lastDay + 8) &&
                         EqualPattern(lastDay - 6, lastDay + 15) ||
                         EqualPattern(lastDay - 13, lastDay + 1) && EqualPattern(lastDay - 13, lastDay + 8) &&
                         EqualPattern(lastDay - 13, lastDay + 15)))
                        continue;

                    if (exceptions > 3 && lastDay - from > 35)
                    {
                        var startOkCount = new DayCounter();
                        var startBadCount = new DayCounter();
                        var startGrouping = DayGrouping.None;

                        // ak ma zaciatok intervalu iny vzor, spracuje sa samostatne
                        if (ScanDays(from, from + 20, startOkCount, startBadCount, ref startGrouping) &&
                            GetDayType(okCount) != GetDayType(startOkCount))
                        {
                            var day = from + 20;

                            while (day <= lastDay - 1 && ScanDays(from, day, startOkCount, startBadCount, ref startGrouping))
                                day++;

                            lastDay = day - 1;
                            AddIntervals(ref limits, ProcessInterval(minCount, from, lastDay));

                            if (limits != null)
                            {
                                if (lastDay < to)
                                    AddIntervals(ref limits, ProcessInterval(minCount, lastDay + 1, to));

                                break;
                            }
                        }
                    }

                    var dayFrom = GetBetterDayFrom(from, okCount, grouping);
                    var dayTo = GetBetterDayTo(lastDay, okCount, grouping);

                    if (dayFrom > 0 && okCount[GetDayIndex(dayFrom, grouping)] == 0)
                    {
                        // usek zacina mimo vzoru - jeho zaciatok sa oddeli
                        var start = dayFrom;

                        while (Runs(dayFrom))
                            dayFrom++;

                        if (dayFrom > start)
                        {
                            AddIntervals(ref limits, ProcessInterval(minCount, start, dayFrom - 1));

                            while (RunsNot(dayFrom))
                                dayFrom++;

                            from = dayFrom;
                        }
                    }

                    var limit = new DateLimitInfo(dayFrom, dayTo);

                    if (dayFrom != dayTo)
                        limit.Type = CheckDayType(dayFrom, dayTo, GetDayType(okCount));

                    AddInterval(ref limits, limit);

                    if (extraRuns > 0)
                        AddExtraRuns(limits[^1], from, lastDay, okCount, grouping);

                    if (missingRuns > 0)
                        AddMissingRuns(limits[^1], from, lastDay, dayFrom, dayTo, okCount, grouping);
                }
            }

            if (limits != null)
            {
                if (lastDay < to)
                    AddIntervals(ref limits, ProcessInterval(minCount, lastDay + 1, to));

                break;
            }
        }

        return limits;
    }

    /// <summary>
    /// Doplni do obmedzenia useky, v ktorych vlak ide nad ramec tyzdenneho vzoru.
    /// </summary>
    private void AddExtraRuns(DateLimitInfo limit, int from, int lastDay, DayCounter okCount, DayGrouping grouping)
    {
        var day = from;

        while (day <= lastDay)
            if (okCount[GetDayIndex(day, grouping)] == 0 && Runs(day))
            {
                var blockFrom = day;

                while (day <= lastDay && Runs(day))
                    day++;

                // dni, ktore uz pokryva vzor, sa na konci useku neuvadzaju
                var blockTo = day - 1;

                while (okCount[GetDayIndex(blockTo, grouping)] != 0)
                    blockTo--;

                AddPeriod(limit.ListRuns, blockFrom, blockTo);
            }
            else
                day++;
    }

    /// <summary>
    /// Doplni do obmedzenia useky dni tyzdenneho vzoru, v ktorych vlak nejde.
    /// </summary>
    private void AddMissingRuns(DateLimitInfo limit, int from, int lastDay, int dayFrom, int dayTo,
        DayCounter okCount, DayGrouping grouping)
    {
        var day = from;

        while (day <= lastDay)
            if (okCount[GetDayIndex(day, grouping)] != 0 && RunsNot(day))
            {
                var gapFrom = day;
                var gapTo = day;
                var patternDayCount = 0;
                var patternDays = new int[2];

                while (day <= lastDay && RunsNot(day))
                {
                    if (okCount[GetDayIndex(day, grouping)] != 0)
                    {
                        gapTo = day;

                        if (patternDayCount < patternDays.Length)
                            patternDays[patternDayCount] = gapTo;

                        patternDayCount++;
                    }

                    day++;
                }

                if (patternDayCount < 3)
                {
                    // jeden alebo dva dni sa vypisu ako samostatne datumy
                    for (var i = 0; i < patternDayCount; i++)
                        AddDay(limit.ListRunsNot, patternDays[i]);
                }
                else
                {
                    // tri a viac dni sa zapisu ako obdobie roztiahnute na cele okolie bez jazdy
                    while (gapFrom >= dayFrom && RunsNot(gapFrom))
                        gapFrom--;

                    gapFrom++;

                    while (gapTo <= dayTo && RunsNot(gapTo))
                        gapTo++;

                    gapTo--;

                    AddPeriod(limit.ListRunsNot, gapFrom, gapTo);

                    if (gapTo > day)
                        day = gapTo;
                }
            }
            else
                day++;
    }

    /// <summary>
    /// Pokusi sa zredukovat interval <paramref name="from"/>-<paramref name="to"/>.
    /// </summary>
    /// <param name="from">zaciatok intervalu</param>
    /// <param name="to">koniec intervalu</param>
    private void ReduceInterval(ref int from, ref int to)
    {
        while (from < to && RunsNot(from))
            from++;

        while (from < to && RunsNot(to))
            to--;
    }

    /// <summary>
    /// Vrati jednotlive intervaly datumoveho obmedzenia.
    /// </summary>
    /// <param name="from">zaciatok intervalu</param>
    /// <param name="to">koniec intervalu</param>
    private List<DateLimitInfo>? GetIntervals(int from, int to)
    {
        var runEnds = 0;
        var runStarts = 0;

        for (var day = from; day < to; day++)
            if (Runs(day) && RunsNot(day + 1))
                runEnds++;
            else if (Runs(day + 1) && RunsNot(day))
                runStarts++;

        if (Runs(to))
            runEnds++;
        else
            runStarts++;

        // vypis obdobi ma zmysel len ak je useku, kedy vlak ide, malo
        if (!(runEnds <= 2 && runEnds <= runStarts || runEnds == 1 && runStarts == 0))
            return null;

        var info = new DateLimitInfo();
        var blockFrom = -1;

        for (var day = from; day <= to; day++)
            if (Runs(day))
            {
                if (blockFrom < 0)
                    blockFrom = day;
            }
            else if (blockFrom >= 0)
            {
                AddPeriod(info.ListRuns, blockFrom, day - 1);
                blockFrom = -1;
            }

        if (blockFrom >= 0)
            AddPeriod(info.ListRuns, blockFrom, to);

        return [info];
    }

    /// <summary>
    /// Vrati obmedzenie zapisane tyzdennym vzorom alebo vypisom jednotlivych dni, ak je takyto
    /// zapis mozny. Inak vrati <see langword="null"/>.
    /// </summary>
    private List<DateLimitInfo>? GetSingleDays(int from, int to)
    {
        var runDays = 0;

        for (var day = from; day <= to; day++)
            if (Runs(day))
                runDays++;

        if (runDays == 0)
            return null;

        var okCount = new DayCounter();
        var badCount = new DayCounter();
        var grouping = DayGrouping.None;

        var weekPattern = ScanDays(from, to, okCount, badCount, ref grouping);
        var totalDays = to - from + 1;

        // bez tyzdenneho vzoru sa jednotlive dni vypisu len ak ich je malo
        if (!weekPattern && !(runDays <= 6 && (runDays == totalDays || runDays <= totalDays + 1 - runDays)))
            return null;

        var info = new DateLimitInfo();

        if (weekPattern && to - from > 8)
        {
            info.From = GetBetterDayFrom(from, okCount, grouping);
            info.To = GetBetterDayTo(to, okCount, grouping);
            info.Type = GetDayType(okCount);
        }
        else
        {
            for (var day = from; day <= to; day++)
                if (Runs(day))
                    AddDay(info.ListRuns, day);
        }

        return [info];
    }

    /// <summary>
    /// Prida prvky (intervaly) zoznamu <paramref name="appendIntervals"/> do zoznamu <paramref name="baseIntervals"/>.<br></br>
    /// Ak <paramref name="baseIntervals"/> je <see langword="null"/>, priradi referenciu <paramref name="appendIntervals"/> do <paramref name="baseIntervals"/>.
    /// </summary>
    /// <param name="baseIntervals">zakladny zoznam</param>
    /// <param name="appendIntervals">zoznam na priradenie do zakladneho zoznamu</param>
    private static void AddIntervals(ref List<DateLimitInfo>? baseIntervals, List<DateLimitInfo> appendIntervals)
    {
        if (baseIntervals == null)
        {
            baseIntervals = appendIntervals;
            return;
        }

        baseIntervals.AddRange(appendIntervals);
    }

    /// <summary>
    /// Prida prvok <paramref name="interval"/> do zoznamu <paramref name="baseIntervals"/>.<br></br>
    /// Ak <paramref name="baseIntervals"/> je <see langword="null"/>, vytvori sa nova instancia triedy <see cref="List{DateLimitInfo}"/>.
    /// </summary>
    /// <param name="baseIntervals">zakladny zoznam</param>
    /// <param name="interval">interval na pridanie</param>
    private static void AddInterval([System.Diagnostics.CodeAnalysis.NotNull] ref List<DateLimitInfo>? baseIntervals, DateLimitInfo interval)
    {
        baseIntervals ??= [];
        baseIntervals.Add(interval);
    }

    /// <summary>
    /// Prida jeden den ako periodu s rovnakym zaciatkom aj koncom.
    /// </summary>
    private static void AddDay(List<DateLimitInfo> runs, int day) => AddPeriod(runs, day, day);

    /// <summary>
    /// Prida periodu. Ak nadvazuje na poslednu periodu v zozname, obe sa spoja do jednej.
    /// </summary>
    /// <param name="runs">intervaly</param>
    /// <param name="from">ide od</param>
    /// <param name="to">ide do</param>
    private static void AddPeriod(List<DateLimitInfo> runs, int from, int to)
    {
        if (from > 0 && runs.Count > 0 && runs[^1].To == from - 1)
        {
            runs[^1].To = to;
            return;
        }

        runs.Add(new DateLimitInfo(from, to));
    }

    /// <summary>
    /// Vrati, ci v zadany den vlak IDE.
    /// </summary>
    /// <param name="day">Den na posudenie.</param>
    private bool Runs(int day) => _bits![day];

    /// <summary>
    /// Vrati, ci v zadany den vlak NEJDE.
    /// </summary>
    /// <param name="day">Den na posudenie.</param>
    private bool RunsNot(int day) => !Runs(day);

    /// <summary>
    /// Prida zadany den do <see cref="StringBuilder"/>a, ktory pred pridanim sformatuje.
    /// </summary>
    /// <param name="day">Den, ktory sa ma pridat na koniec buildera.</param>
    private void AppendDay(int day)
    {
        AppendComma();
        _builder.Append(FormatDay(day));
    }

    /// <summary>
    /// Sformatuje zadany den. Ak ma rovnaky mesiac ako naposledy vypisany den, mesiac sa
    /// z predchadzajuceho datumu v builderi odstrani (zapise sa teda len raz, napr. "1.,5.I.").
    /// </summary>
    /// <param name="day">Den, ktory sa ma sformatovat.</param>
    /// <returns>sformatovany den ako retazec.</returns>
    private string FormatDay(int day)
    {
        var date = DateFrom.AddDays(day);
        var month = MsgMonth(date.Month) + ".";

        if (!DateUnique(date))
            month += date.Year;

        if (!string.IsNullOrEmpty(_lastMonth) && _lastMonth == month)
        {
            var index = _builder.ToString().LastIndexOf(_lastMonth, StringComparison.Ordinal);
            _builder.Remove(index, _lastMonth.Length);
        }
        else
            _lastMonth = month;

        return $"{date.Day}.{_lastMonth}";
    }

    /// <summary>
    /// Prida znak ciarky (,) na koniec <see cref="StringBuilder"/>a.
    /// </summary>
    private void AppendComma()
    {
        var last = _builder.Length - 1;

        if (last > 0 && _builder[last] != ' ' && _builder[last] != ',')
            _builder.Append(',');
    }

    /// <summary>
    /// Prida znak medzery ( ) na koniec <see cref="StringBuilder"/>a.
    /// </summary>
    private void AppendSpace()
    {
        var last = _builder.Length - 1;

        if (last > 0 && _builder[last] > ' ')
            _builder.Append(' ');
    }

    /// <summary>
    /// Spocita, kolkokrat vlak v intervale ide a nejde v jednotlivych typoch dni. Ak prevazuje
    /// jazda podla pracovnych dni a sviatkov, prepne <paramref name="grouping"/> na toto zlucenie
    /// a pocitadla dni v tyzdni vynuluje.
    /// </summary>
    /// <returns>
    /// <see langword="true"/>, ak sa jazda da uplne popisat typmi dni, teda ziadny typ dna nie je
    /// zaroven jazdny aj nejazdny.
    /// </returns>
    private bool ScanDays(int from, int to, DayCounter okCount, DayCounter badCount, ref DayGrouping grouping)
    {
        okCount.Clear();
        badCount.Clear();

        var dayIndex = GetDayIndex(from, DayGrouping.None);
        var saturdays = 0;
        var totalBad = 0;

        for (var day = from; day <= to; day++)
        {
            var counter = Runs(day) ? okCount : badCount;

            if (_specDays)
            {
                var type = GetDayType(day);

                if ((type & DayType.Workday) != DayType.None)
                    counter[DayIndex.Workday]++;
                else if ((type & DayType.Holiday) != DayType.None)
                {
                    counter[DayIndex.Holiday]++;

                    if (dayIndex == DayIndex.Saturday && Runs(day))
                        saturdays++;
                }
            }

            counter[dayIndex]++;

            if (RunsNot(day))
                totalBad++;

            dayIndex = GetNextDayIndex(dayIndex);
        }

        if (totalBad == 0)
            return false;

        if (_specDays)
            ApplySpecDays(okCount, badCount, saturdays, ref grouping);

        for (var index = DayIndex.Monday; index <= DayIndex.Holiday; index++)
            if (okCount[index] != 0 && badCount[index] != 0)
                return false;

        return true;
    }

    /// <summary>
    /// Rozhodne, ci sa jazda lepsie popise dnami v tyzdni, alebo pracovnymi dnami a sviatkami,
    /// a pocitadla nepouziteho popisu vynuluje.
    /// </summary>
    private static void ApplySpecDays(DayCounter okCount, DayCounter badCount, int saturdays, ref DayGrouping grouping)
    {
        // sobota, ktora je zaroven sviatkom, sa nepocita do sviatkov, ak sa jazda riadi sobotami
        if (okCount[DayIndex.Saturday] > 2 * badCount[DayIndex.Saturday] &&
            okCount[DayIndex.Holiday] < 2 * badCount[DayIndex.Holiday])
            okCount[DayIndex.Holiday] -= saturdays;

        var weekDayScore = 0;
        var specDayScore = 0;

        for (var index = DayIndex.Monday; index <= DayIndex.Holiday; index++)
        {
            if (okCount[index] <= 0 || okCount[index] <= badCount[index])
                continue;

            if (index <= DayIndex.Sunday)
                weekDayScore += okCount[index] - badCount[index];

            if (index is DayIndex.Saturday or DayIndex.Workday or DayIndex.Holiday)
                specDayScore += okCount[index] - badCount[index];
        }

        if (specDayScore <= weekDayScore)
        {
            // jazda sa popise dnami v tyzdni - pracovne dni a sviatky sa nepouziju
            okCount[DayIndex.Workday] = 0;
            okCount[DayIndex.Holiday] = 0;
            badCount[DayIndex.Workday] = 0;
            badCount[DayIndex.Holiday] = 0;
            return;
        }

        grouping = DayGrouping.WorkdayHoliday;

        for (var index = DayIndex.Monday; index <= DayIndex.Sunday; index++)
            if (index != DayIndex.Saturday)
            {
                okCount[index] = 0;
                badCount[index] = 0;
            }

        if (okCount[DayIndex.Holiday] > 2 * badCount[DayIndex.Holiday] &&
            okCount[DayIndex.Saturday] < 2 * badCount[DayIndex.Saturday])
            okCount[DayIndex.Saturday] -= saturdays;
        else
            grouping |= DayGrouping.KeepSaturday;
    }

    /// <summary>
    /// Posunie zaciatok useku na prvy den patriaci do tyzdenneho vzoru. Ak vzor siaha az na
    /// zaciatok grafikonu, vrati 0 - zaciatok sa potom v poznamke neuvadza.
    /// </summary>
    private int GetBetterDayFrom(int from, DayCounter okCount, DayGrouping grouping)
    {
        if (from >= 7)
            return from;

        var day = from;

        while (day >= 0 && (Runs(day) || okCount[GetDayIndex(day, grouping)] == 0))
            day--;

        if (day < 0)
            return 0;

        day = from;

        while (okCount[GetDayIndex(day, grouping)] == 0)
            day++;

        return day;
    }

    /// <summary>
    /// Posunie koniec useku na posledny den patriaci do tyzdenneho vzoru. Ak vzor siaha az na
    /// koniec grafikonu, vrati <see cref="MaxDay"/> - koniec sa potom v poznamke neuvadza.
    /// </summary>
    private int GetBetterDayTo(int to, DayCounter okCount, DayGrouping grouping)
    {
        if (to <= MaxDay - 7)
            return to;

        var day = to;

        while (day <= MaxDay && (Runs(day) || okCount[GetDayIndex(day, grouping)] == 0))
            day++;

        if (day > MaxDay)
            return MaxDay;

        day = to;

        while (okCount[GetDayIndex(day, grouping)] == 0)
            day--;

        return day;
    }

    /// <summary>
    /// Vrati, ci su nastavene vsetky dni v tyzdni, alebo vsetky pracovne dni spolu so sviatkami.
    /// </summary>
    private static bool AllSet(DayCounter count)
    {
        var dayType = GetDayType(count);
        return (dayType & DayType.All1) == DayType.All1 || (dayType & DayType.All2) == DayType.All2;
    }

    /// <summary>
    /// Vrati, ci sa tyzdenny vzor od dna <paramref name="from"/> zhoduje so vzorom od dna <paramref name="to"/>.
    /// </summary>
    private bool EqualPattern(int from, int to)
    {
        for (var offset = 0; offset <= 6; offset++)
            if (Runs(from + offset) != Runs(to + offset))
                return false;

        return true;
    }

    /// <summary>
    /// Sformatuje vsetky useky obmedzenia do vysledneho textu poznamky.
    /// </summary>
    private string Format(IList<DateLimitInfo> limits, bool isNot)
    {
        if (limits.Count == 0)
            return MsgText(Message.Empty);

        Merge(limits);
        _builder.Length = 0;
        var level = Level.Undefined;

        for (var i = 0; i < limits.Count; i++)
            FormatInfo(limits[i], i + 1 < limits.Count ? limits[i + 1] : null, isNot, ref level);

        return _builder.ToString();
    }

    /// <summary>
    /// Sformatuje jeden usek obmedzenia.
    /// </summary>
    /// <param name="info">Usek na sformatovanie.</param>
    /// <param name="next">Nasledujuci usek, alebo <see langword="null"/> pri poslednom useku.</param>
    /// <param name="isNot">Text sa tvori z negovaneho bitoveho pola.</param>
    /// <param name="level">Naposledy vypisana uvodna spojka - opakovane sa nevypisuje.</param>
    private void FormatInfo(DateLimitInfo info, DateLimitInfo? next, bool isNot, ref Level level)
    {
        AppendComma();
        _lastMonth = null;

        if (info is { AllIsSet: true, From: 0 } && info.To == MaxDay && !info.RunsNot)
        {
            _builder.Append(MsgText(Message.RunsDaily));
            return;
        }

        // pri negovanom poli maju vyznamy "ide" a "nejde" opacny vyznam
        var mainLevel = isNot ? Level.RunsNot : Level.Runs;
        var mainText = isNot 
            ? AltMsgText(Message.RunsNot, Message.RunsNotAlt) 
            : AltMsgText(Message.Runs, Message.RunsAlt);
        
        var exceptionLevel = isNot ? Level.Runs : Level.RunsNot;
        var exceptionText = isNot 
            ? AltMsgText(Message.Runs, Message.RunsAlt) 
            : AltMsgText(Message.RunsNot, Message.RunsNotAlt);

        var baseLength = _builder.Length;

        if (info.HaveDays || info.From != 0 || info.To != 0)
        {
            AppendPeriod(info.From, info.To);

            if (info.HaveDays && next != null && next.Type == info.Type && info is { Runs: false, RunsNot: false })
                _builder.Append(MsgText(Message.And));
            else if (info.HaveDays)
                AppendDays(info.Type);

            if (_builder.Length > baseLength)
            {
                if (level != mainLevel)
                {
                    _builder.Insert(baseLength, mainText);
                    level = mainLevel;
                }
                else if (next == null && baseLength > 0 && _builder[baseLength - 1] == ',' && !info.HaveDays)
                {
                    // posledny usek sa k predchadzajucemu pripoji spojkou namiesto ciarky
                    baseLength--;
                    _builder.Remove(baseLength, 1);
                    _builder.Insert(baseLength, MsgText(Message.And));
                }
            }
        }

        foreach (var run in info.ListRuns)
        {
            if (_builder.Length == baseLength && level != mainLevel)
            {
                _builder.Append(mainText);
                level = mainLevel;
            }

            AppendPeriod(run.From, run.To);
        }

        _lastMonth = null;

        for (var i = 0; i < info.ListRunsNot.Count; i++)
        {
            AppendComma();

            if (i == 0 && level != exceptionLevel)
            {
                _builder.Append(exceptionText);
                level = exceptionLevel;
            }

            AppendPeriod(info.ListRunsNot[i].From, info.ListRunsNot[i].To);
        }
    }

    /// <summary>
    /// Spoji susedne useky obmedzenia, ktore sa daju zapisat spolocne.
    /// </summary>
    private static void Merge(IList<DateLimitInfo> limits)
    {
        var i = 0;

        while (i + 1 < limits.Count)
            if (limits[i].Merge(limits[i + 1]))
                limits.RemoveAt(i + 1);
            else
                i++;
    }

    /// <summary>
    /// Pripoji zoznam typov dni, napr. "v 1-5,7". Tri a viac dni po sebe sa zapisu ako rozsah.
    /// </summary>
    private void AppendDays(DayType dayType)
    {
        AppendSpace();
        _builder.Append(MsgText(Message.On));

        if ((dayType & DayType.Workday) == DayType.Workday)
            _builder.Append(MsgDayType(Message.Workday));

        var day = 0;

        while (day <= 6)
        {
            if ((dayType & (DayType)(1 << day)) == DayType.None)
            {
                day++;
                continue;
            }

            var last = day;

            while (last < 6 && (dayType & (DayType)(1 << (last + 1))) != DayType.None)
                last++;

            if (last - day > 1)
            {
                AppendComma();
                _builder.Append(MsgDayType(Message.Monday + day)).Append('-').Append(MsgDayType(Message.Monday + last));
            }
            else
                while (day <= last)
                {
                    AppendComma();
                    _builder.Append(MsgDayType(Message.Monday + day));
                    day++;
                }

            day = last + 1;
        }

        if ((dayType & DayType.Holiday) != DayType.Holiday)
            return;

        AppendComma();
        _builder.Append(MsgDayType(Message.Holiday));
    }

    /// <summary>
    /// Pripoji obdobie. Hranice zhodne s hranicami grafikonu sa neuvadzaju, kratke obdobia sa
    /// vypisu ako jednotlive datumy.
    /// </summary>
    private void AppendPeriod(int dayFrom, int dayTo)
    {
        if (dayFrom == 0 && dayTo == MaxDay)
            return;

        if (dayTo - dayFrom <= 1)
        {
            for (var day = dayFrom; day <= dayTo; day++)
                AppendDay(day);

            return;
        }

        AppendComma();

        if (dayFrom > 0)
            _builder.Append(MsgText(Message.From)).Append(FormatDay(dayFrom));

        if (dayTo < MaxDay)
        {
            AppendSpace();
            _builder.Append(MsgText(Message.To)).Append(FormatDay(dayTo));
        }

        _lastMonth = null;
    }

    /// <summary>
    /// Vrati, ci den a mesiac zadaneho datumu pripadnu do platnosti grafikonu najviac raz,
    /// teda ci netreba k datumu uvadzat aj rok.
    /// </summary>
    private bool DateUnique(DateTime date)
    {
        var count = 0;

        for (var year = DateFrom.Year; year <= DateTo.Year; year++)
        {
            // 29.2. v nepriestupnom roku neexistuje
            if (date.Day > DateTime.DaysInMonth(year, date.Month))
                continue;

            var candidate = new DateTime(year, date.Month, date.Day);

            if (candidate >= DateFrom && candidate <= DateTo)
                count++;
        }

        return count <= 1;
    }

    private enum Level
    {
        /// <summary>
        /// Nedefinovane.
        /// </summary>
        Undefined,

        /// <summary>
        /// Vlak ide.
        /// </summary>
        Runs,

        /// <summary>
        /// Vlak nejde.
        /// </summary>
        RunsNot
    }

    /// <summary>
    /// Cast poznamky, ku ktorej sa vztahuje prave spracovany token.
    /// </summary>
    private enum DateLevel
    {
        Date,
        From,
        To,
        On
    }

    /// <summary>
    /// Jeden usek datumoveho obmedzenia - obdobie, typy dni a vynimky z nich.
    /// </summary>
    private class DateLimitInfo
    {
        /// <summary>Zoznam obdobi, kedy vlak navyse ide.</summary>
        public List<DateLimitInfo> ListRuns;

        /// <summary>Zoznam obdobi, kedy vlak nejde.</summary>
        public List<DateLimitInfo> ListRunsNot;

        public int From;
        public int To;
        public DayType Type;

        public DateLimitInfo()
        {
            ListRuns = [];
            ListRunsNot = [];
        }

        public DateLimitInfo(int dayFrom, int dayTo) : this()
        {
            From = dayFrom;
            To = dayTo;
        }

        /// <summary>Su nastavene vsetky dni v tyzdni, alebo pracovne dni spolu so sviatkami.</summary>
        public bool AllIsSet => (Type & DayType.All1) == DayType.All1 || (Type & DayType.All2) == DayType.All2;

        /// <summary>Typy dni treba v poznamke uviest.</summary>
        public bool HaveDays => !AllIsSet && Type > DayType.None;

        public bool Runs => ListRuns.Count > 0;

        public bool RunsNot => ListRunsNot.Count > 0;

        /// <summary>
        /// Pokusi sa pripojit nasledujuci usek k tomuto useku.
        /// </summary>
        /// <returns><see langword="true"/>, ak sa useky podarilo spojit.</returns>
        public bool Merge(DateLimitInfo info)
        {
            // useky sa spoja bud ak maju rovnake typy dni a lezia dostatocne blizko pri sebe,
            // alebo ak pripojeny usek neurcuje ziadne vlastne obdobie
            var compatible = HaveDays && Type == info.Type && To + MaxMergeGap > info.From &&
                             (Runs || info.Runs || RunsNot || info.RunsNot);
            var isEmpty = info is { HaveDays: false, From: 0, To: 0 };

            if (!compatible && !isEmpty)
                return false;

            if (info.To != 0 || info.From != 0)
            {
                // medzera medzi usekmi sa zapise ako obdobie, kedy vlak nejde
                if (Runs && ListRuns.Any(run => run.From > To && run.From < info.From || run.To > To && run.To < info.From))
                    return false;

                ListRunsNot.Add(new DateLimitInfo(To + 1, info.From - 1));
                To = info.To;
            }

            if (info.Runs)
            {
                if (Runs)
                    ListRuns.AddRange(info.ListRuns);
                else
                    ListRuns = info.ListRuns;
            }

            if (info.RunsNot)
            {
                if (RunsNot)
                    ListRunsNot.AddRange(info.ListRunsNot);
                else
                    ListRunsNot = info.ListRunsNot;
            }

            return true;
        }
    }
}
