using System.Globalization;

namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Hodnota, ktoru INISS cita dvakrat - ako cislo (<c>I:</c>) alebo ako vyraz (<c>S:</c>) vyhodnocovany pre kazdy vlak.
/// </summary>
public sealed class StateDgmDynamic : IEquatable<StateDgmDynamic>
{
    private StateDgmDynamic(int? number, string? expression)
    {
        Number = number;
        Expression = expression;
    }

    /// <summary>Ciselna hodnota (zapis <c>I:"kluc"=n</c>).</summary>
    public int? Number { get; }

    /// <summary>Vyraz (zapis <c>S:"kluc"="vyraz"</c>).</summary>
    public string? Expression { get; }

    /// <summary>Hodnota je vyraz.</summary>
    public bool IsExpression => Expression != null;

    /// <summary>Text hodnoty tak, ako sa zobrazi (cislo alebo vyraz).</summary>
    public string Text => Expression ?? Number!.Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Ciselna hodnota.</summary>
    public static StateDgmDynamic FromNumber(int n) => new(n, null);

    /// <summary>Vyraz. Ak je to cely ciselny literal, vrati ciselnu hodnotu (INISS ich cita rovnako).</summary>
    public static StateDgmDynamic FromExpression(string expr)
    {
        var t = expr.Trim();
        return StateDgmReader.TryStrtol(t, out var n) ? new StateDgmDynamic(n, null) : new StateDgmDynamic(null, expr);
    }

    /// <summary>Konstanta pre kluc <c>Wait</c> - meno udalosti, alebo null pre <see cref="StateDgmWaitEvent.None" />.</summary>
    public static StateDgmDynamic? FromWait(StateDgmWaitEvent e) => e == StateDgmWaitEvent.None ? null : new StateDgmDynamic(null, WaitName(e));

    // mena udalosti v jazyku vyrazov INISSu (ExprConstants) - zapisuju sa do suboru, preto nezavisia od mien clenov enumu;
    // poradie podla hodnoty
    private static readonly (StateDgmWaitEvent Event, string Key)[] WaitKeys =
    [
        (StateDgmWaitEvent.Odj, "Odj"),
        (StateDgmWaitEvent.Vj, "Vj"),
        (StateDgmWaitEvent.Zcv, "ZCV"),
        (StateDgmWaitEvent.Ovc, "OVC"),
        (StateDgmWaitEvent.Vvc, "VVC")
    ];

    /// <summary>Mena udalosti spojene <c>|</c> (napr. <c>VVC|Vj</c>).</summary>
    public static string WaitName(StateDgmWaitEvent e) =>
        string.Join("|", WaitKeys.Where(w => e.HasFlag(w.Event)).Select(w => w.Key));

    /// <summary>Udalost podla mena v jazyku vyrazov (napr. <c>VVC</c>); rozlisuje velkost pismen.</summary>
    /// <returns><see langword="false" />, ak meno nie je udalost ILTISu.</returns>
    public static bool TryParseWait(string key, out StateDgmWaitEvent e)
    {
        foreach (var w in WaitKeys)
        {
            if (!string.Equals(w.Key, key, StringComparison.Ordinal))
                continue;
            e = w.Event;
            return true;
        }

        e = StateDgmWaitEvent.None;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(StateDgmDynamic? other) => other != null && Number == other.Number && Expression == other.Expression;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as StateDgmDynamic);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Number, Expression);

    /// <inheritdoc />
    public override string ToString() => Text;
}
