using System.Globalization;

// ReSharper disable StringLiteralTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Zabudovana tabulka druhov vlakov INISSu (95 miest) a hladanie <c>Typ_…</c>.
/// </summary>
public static class ExprTrainTypes
{
    /// <summary>Pocet miest v tabulke.</summary>
    public const int Count = 95;

    /// <summary>Prvy index nakladnych a sluzobnych druhov (<c>NAKLTYP</c>).</summary>
    public const int CargoFirst = 73;

    /// <summary>Posledny index nakladnych a sluzobnych druhov (<c>NAKLTYP</c>).</summary>
    public const int CargoLast = 94;

    private static readonly string?[] Names = BuildNames();

    /// <summary>
    /// Nazvy druhov podla indexu; prazdne miesta su <see langword="null"/>.
    /// </summary>
    public static IReadOnlyList<string?> BuiltIn => Names;

    /// <summary>
    /// Vrati zabudovany nazov druhu na indexe, alebo <see langword="null"/>.
    /// </summary>
    public static string? NameOf(int index) => index is >= 0 and < Count ? Names[index] : null;

    /// <summary>
    /// Index zabudovaneho druhu podla presneho nazvu, alebo -1.
    /// </summary>
    public static int IndexOf(string name) => Array.IndexOf(Names, name);

    /// <summary>
    /// Ci je druh na indexe nakladny alebo sluzobny (<c>NAKLTYP</c>).
    /// </summary>
    public static bool IsCargo(int index) => index is >= CargoFirst and <= CargoLast;

    /// <summary>
    /// Najde druh vlaku ako INISS: najprv medzi klucmi TrTypes.txt, potom medzi zabudovanymi nazvami;
    /// v oboch presne, potom bez ohladu na velkost pismen, potom aj bez diakritiky.
    /// </summary>
    /// <param name="key">Text za predponou <c>Typ_</c>.</param>
    /// <param name="trTypesKeys">Kluce z TrTypes.txt s indexmi; <see langword="null"/>, ak nie su k dispozicii.</param>
    public static ExprTrainTypeMatch? Resolve(string key, IReadOnlyDictionary<string, int>? trTypesKeys)
    {
        if (trTypesKeys is not null)
        {
            foreach (var (k, idx) in trTypesKeys)
                if (string.Equals(k, key, StringComparison.Ordinal))
                    return new ExprTrainTypeMatch(idx, key, true, true);
            foreach (var (k, idx) in trTypesKeys)
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                    return new ExprTrainTypeMatch(idx, key, true, false);
            foreach (var (k, idx) in trTypesKeys)
                if (string.Equals(StripDiacritics(k), StripDiacritics(key), StringComparison.OrdinalIgnoreCase))
                    return new ExprTrainTypeMatch(idx, key, true, false);
        }

        for (var i = 0; i < Count; i++)
            if (Names[i] is not null && string.Equals(Names[i], key, StringComparison.Ordinal))
                return new ExprTrainTypeMatch(i, key, false, true);
        for (var i = 0; i < Count; i++)
            if (Names[i] is not null && string.Equals(Names[i], key, StringComparison.OrdinalIgnoreCase))
                return new ExprTrainTypeMatch(i, key, false, false);
        var stripped = StripDiacritics(key);
        for (var i = 0; i < Count; i++)
            if (Names[i] is not null && string.Equals(StripDiacritics(Names[i]!), stripped, StringComparison.OrdinalIgnoreCase))
                return new ExprTrainTypeMatch(i, key, false, false);

        return null;
    }

    private static string StripDiacritics(string s)
    {
        var norm = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(norm.Length);
        foreach (var ch in norm)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string?[] BuildNames()
    {
        var n = new string?[Count];
        string[] a = ["Os", "MOs", "Sp", "Zr", "SPR", "Bus", "Loď", "Lan"];
        for (var i = 0; i < a.Length; i++) n[i] = a[i];
        for (var i = 1; i <= 9; i++) n[16 + i] = "Os" + i;
        string[] b = ["R", "Ex", "REX", "ER"];
        for (var i = 0; i < b.Length; i++) n[26 + i] = b[i];
        for (var i = 1; i <= 9; i++) n[38 + i] = "R" + i;
        string[] c = ["EC", "IC", "SC", "ICE", "EN", "NZ", "TGV"];
        for (var i = 0; i < c.Length; i++) n[48 + i] = c[i];
        for (var i = 1; i <= 9; i++) n[63 + i] = "X" + i;
        string[] d = ["Sl", "Nákl", "Rn", "Rp"];
        for (var i = 0; i < d.Length; i++) n[73 + i] = d[i];
        for (var i = 1; i <= 9; i++) n[85 + i] = "Sl" + i;
        return n;
    }
}
