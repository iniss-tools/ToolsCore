namespace ToolsCore.Iniss2.Xml;

/// <summary>
/// Rozdiel dvoch postupnosti - najdlhsia spolocna podpostupnost, zvysok su vymazania a vlozenia.
/// </summary>
internal static class SequenceDiff
{
    /// <summary>Najvacsi pocet poli tabulky; vacsi rozdiel sa berie ako jeden usek zmien.</summary>
    private const long MaxCells = 4_000_000;

    /// <summary>
    /// Druh kroku rozdielu.
    /// </summary>
    public enum Kind
    {
        /// <summary>Prvok je v oboch postupnostiach.</summary>
        Equal,

        /// <summary>Prvok je len v starej postupnosti.</summary>
        Delete,

        /// <summary>Prvok je len v novej postupnosti.</summary>
        Insert
    }

    /// <summary>
    /// Krok rozdielu.
    /// </summary>
    /// <param name="Kind">druh</param>
    /// <param name="Old">index v starej postupnosti; -1 pri vlozeni</param>
    /// <param name="New">index v novej postupnosti; -1 pri vymazani</param>
    public readonly record struct Step(Kind Kind, int Old, int New);

    /// <summary>
    /// Kroky, ktore zmenia staru postupnost na novu, v poradi postupnosti. Vymazania useku idu pred jeho vlozenia.
    /// </summary>
    public static List<Step> Compute(IReadOnlyList<string> older, IReadOnlyList<string> newer)
    {
        ArgumentNullException.ThrowIfNull(older);
        ArgumentNullException.ThrowIfNull(newer);

        var prefix = 0;
        while (prefix < older.Count && prefix < newer.Count && older[prefix] == newer[prefix])
            prefix++;

        var suffix = 0;
        while (suffix < older.Count - prefix && suffix < newer.Count - prefix && older[older.Count - 1 - suffix] == newer[newer.Count - 1 - suffix])
            suffix++;

        var steps = new List<Step>(Math.Max(older.Count, newer.Count));
        for (var i = 0; i < prefix; i++)
            steps.Add(new Step(Kind.Equal, i, i));

        Middle(older, newer, prefix, older.Count - suffix, prefix, newer.Count - suffix, steps);

        for (var i = 0; i < suffix; i++)
            steps.Add(new Step(Kind.Equal, older.Count - suffix + i, newer.Count - suffix + i));

        return steps;
    }

    private static void Middle(IReadOnlyList<string> older, IReadOnlyList<string> newer, int oldFrom, int oldTo, int newFrom, int newTo, List<Step> steps)
    {
        var n = oldTo - oldFrom;
        var m = newTo - newFrom;

        if (n == 0 || m == 0 || (long)n * m > MaxCells)
        {
            for (var i = oldFrom; i < oldTo; i++)
                steps.Add(new Step(Kind.Delete, i, -1));
            for (var j = newFrom; j < newTo; j++)
                steps.Add(new Step(Kind.Insert, -1, j));
            return;
        }

        // dlzka spolocnej podpostupnosti od pozicie (i, j) do konca
        var table = new int[n + 1, m + 1];
        for (var i = n - 1; i >= 0; i--)
        {
            for (var j = m - 1; j >= 0; j--)
            {
                table[i, j] = older[oldFrom + i] == newer[newFrom + j]
                    ? table[i + 1, j + 1] + 1
                    : Math.Max(table[i + 1, j], table[i, j + 1]);
            }
        }

        var deletes = new List<Step>();
        var inserts = new List<Step>();

        void Flush()
        {
            steps.AddRange(deletes);
            steps.AddRange(inserts);
            deletes.Clear();
            inserts.Clear();
        }

        int x = 0, y = 0;
        while (x < n && y < m)
        {
            if (older[oldFrom + x] == newer[newFrom + y])
            {
                Flush();
                steps.Add(new Step(Kind.Equal, oldFrom + x, newFrom + y));
                x++;
                y++;
            }
            else if (table[x + 1, y] >= table[x, y + 1])
            {
                deletes.Add(new Step(Kind.Delete, oldFrom + x, -1));
                x++;
            }
            else
            {
                inserts.Add(new Step(Kind.Insert, -1, newFrom + y));
                y++;
            }
        }

        for (; x < n; x++)
            deletes.Add(new Step(Kind.Delete, oldFrom + x, -1));
        for (; y < m; y++)
            inserts.Add(new Step(Kind.Insert, -1, newFrom + y));
        Flush();
    }
}
