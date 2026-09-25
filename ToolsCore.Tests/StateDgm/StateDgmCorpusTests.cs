using System.Diagnostics.CodeAnalysis;
using System.Text;
using ToolsCore.StateDgm;

namespace ToolsCore.Tests.StateDgm;

/// <summary>
///     Realne subory StateDgm.txt: kazdy sa musi nacitat, previest na typovany model, zapisat a po opatovnom
///     nacitani dat rovnaky vyznam (rovnake skupiny v rovnakom poradi, rovnake kluce a hodnoty).
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class StateDgmCorpusTests
{
    /// <summary>Kluce, ktore zapisovac dopocitava, a aliasy, ktore normalizuje - do porovnania nejdu.</summary>
    private static readonly HashSet<string> Ignored =
    [
        StateDgmKeys.NUM_DESIGNS, StateDgmKeys.NUM_TIME_POINTS, StateDgmKeys.NUM_CATEGORIES, StateDgmKeys.NUM_STATES,
        StateDgmKeys.NUM_EVENTS, StateDgmKeys.NUM_CONTROLS, StateDgmKeys.NUM_STARTERS
    ];

    

    /// <summary>
    ///     Vyznamovy vypis stromu: skupiny podla zakladneho mena v poradi, hodnoty zoradene podla kluca, bez klucov Num….
    /// </summary>
    private static string Dump(StateDgmGroup g)
    {
        var sb = new StringBuilder();
        Dump(g, sb, 0);
        return sb.ToString();
    }

    private static void Dump(StateDgmGroup g, StringBuilder sb, int depth)
    {
        var pad = new string(' ', depth * 2);
        var baseName = StateDgmGroup.BaseName(g.Name);
        sb.Append(pad).Append('[').Append(baseName).Append(']').Append('\n');
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var v in g.Values.Where(v => !Ignored.Contains(v.Key)))
        {
            var n = Normalize(v);
            if (n == null) continue;
            var eq = n.IndexOf('=');
            values[n[..eq]] = n[(eq + 1)..];
        }

        // predvolene hodnoty INISSu - zapisovac ich zapise vzdy, povodny subor ich mohol vynechat
        foreach (var (k, dv) in Defaults(baseName))
            values.TryAdd(k, dv);
        values.Remove(StateDgmKeys.NAME, out var name);
        if (!string.IsNullOrEmpty(name)) values[StateDgmKeys.NAME] = name;

        foreach (var (k, v) in values.OrderBy(x => x.Key, StringComparer.Ordinal))
            sb.Append(pad).Append("  ").Append(k).Append('=').Append(v).Append('\n');
        // poradie skupin rozneho druhu nie je vyznamove (nezname skupiny idu pri zapise na koniec),
        // poradie skupin rovnakeho druhu ano - stabilne triedenie podla zakladneho mena
        foreach (var sub in g.Groups.OrderBy(x => StateDgmGroup.BaseName(x.Name), StringComparer.Ordinal)) Dump(sub, sb, depth + 1);
    }

    private static IEnumerable<(string, string)> Defaults(string baseName)
    {
        switch (baseName)
        {
            case StateDgmKeys.CATEGORIE:
                yield return (StateDgmKeys.COMMENT, "");
                yield return (StateDgmKeys.ICON, "0");
                break;
            case StateDgmKeys.STATE:
                yield return (StateDgmKeys.ICON, "0");
                yield return (StateDgmKeys.ATTR, "0");
                yield return (StateDgmKeys.DEFAULT_CONTROL, "0");
                break;
            case StateDgmKeys.DO_STATE:
            case StateDgmKeys.UNDO_STATE:
                yield return (StateDgmKeys.CLASS, StateDgmKeys.CLASS_TABLE_SET);
                yield return (StateDgmKeys.ON_DEP_TABLE, "0");
                yield return (StateDgmKeys.ON_ARR_TABLE, "0");
                yield return (StateDgmKeys.ON_PLATFORM_TABLE, "0");
                yield return (StateDgmKeys.SHOW_POSITION, "0");
                yield return (StateDgmKeys.SHOW_TRACK, "1");
                break;
            case StateDgmKeys.DESIGN:
                yield return (StateDgmKeys.DEF_PUSH_BTN, "0");
                yield return (StateDgmKeys.CLASS, StateDgmKeys.CLASS_DESIGN_BTN);
                break;
            case StateDgmKeys.TIME_POINT:
                yield return (StateDgmKeys.TIME_POINT_KEY1, "");
                yield return (StateDgmKeys.TIME_POINT_KEY2, "");
                yield return (StateDgmKeys.TIME_POINT_OFFSET1, "0");
                yield return (StateDgmKeys.TIME_POINT_OFFSET2, "0");
                yield return (StateDgmKeys.OPERATOR, StateDgmKeys.OPERATOR_MIN);
                break;
            case StateDgmKeys.STARTER:
                yield return (StateDgmKeys.CLASS, StateDgmKeys.CLASS_STARTER);
                yield return (StateDgmKeys.TIME_OFFSET, "0");
                yield return (StateDgmKeys.START_LATER_TOO, "0");
                break;
            case StateDgmKeys.CONTROL:
                yield return (StateDgmKeys.CTRL_ID, "0");
                break;
        }
    }

    private static string? Normalize(StateDgmValue v)
    {
        if (v.IsRemoval) return $"{v.Key}=#";
        // ciselny WaitPath sa normalizuje na Wait=VVC
        if (v.Key == StateDgmKeys.WAIT_PATH && v.Kind == StateDgmValueKind.Int)
            return v.Number == 0 ? null : $"{StateDgmKeys.WAIT}=VVC";
        var key = v.Key switch
        {
            StateDgmKeys.ON_DEP_TABLE_OLD => StateDgmKeys.ON_DEP_TABLE,
            StateDgmKeys.ON_ARR_TABLE_OLD => StateDgmKeys.ON_ARR_TABLE,
            StateDgmKeys.ON_PLATFORM_TABLE_OLD => StateDgmKeys.ON_PLATFORM_TABLE,
            StateDgmKeys.SHOW_POSITION_OLD => StateDgmKeys.SHOW_POSITION,
            StateDgmKeys.SHOW_TRACK_OLD => StateDgmKeys.SHOW_TRACK,
            _ => v.Key
        };
        return v.Kind switch
        {
            StateDgmValueKind.String => $"{key}={v.Text}",
            StateDgmValueKind.Int => $"{key}={v.Number}",
            _ => $"{key}={(v.Flag ? 1 : 0)}"
        };
    }
}
