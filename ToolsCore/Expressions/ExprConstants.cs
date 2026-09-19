using System.Globalization;

// ReSharper disable StringLiteralTypo
namespace ToolsCore.Expressions;

/// <summary>
///     Kontext, v ktorom sa vyraz preklada. Urcuje, ktore konstanty prekladac prijme.
/// </summary>
public enum ExprContext
{
    /// <summary>Podmienka v TabTab.txt alebo kluc StateDgm.txt okrem <c>Wait</c> (INISS mode 1).</summary>
    Condition,

    /// <summary>Kluc <c>Wait</c> v StateDgm.txt - navyse prijma konstanty udalosti ILTISu (INISS mode 2).</summary>
    StateDgmWait
}

/// <summary>
///     Skupina konstanty.
/// </summary>
public enum ExprConstantGroup
{
    Position,
    Flag,
    State,
    IltisEvent
}

/// <summary>
///     Pomenovana konstanta jazyka vyrazov.
/// </summary>
/// <param name="Name">Meno.</param>
/// <param name="Value">Hodnota.</param>
/// <param name="Group">Skupina.</param>
/// <param name="IsEnglish">Ci ide o anglicky alias.</param>
public sealed record ExprConstantInfo(string Name, int Value, ExprConstantGroup Group, bool IsEnglish = false);

/// <summary>
///     Vysledok rozpoznania konstanty.
/// </summary>
/// <param name="Value">Hodnota.</param>
/// <param name="Constant">Popis konstanty, ak islo o pomenovanu konstantu.</param>
/// <param name="TrainType">Index druhu vlaku, ak islo o <c>Typ_…</c>.</param>
public readonly record struct ExprConstantMatch(int Value, ExprConstantInfo? Constant, ExprTrainTypeMatch? TrainType);

/// <summary>
///     Vysledok rozpoznania <c>Typ_…</c>.
/// </summary>
/// <param name="Index">Index v zabudovanej tabulke (0-94).</param>
/// <param name="Key">Kluc za predponou.</param>
/// <param name="FromTrTypes">Ci sa kluc nasiel v TrTypes.txt (inak medzi zabudovanymi nazvami).</param>
/// <param name="Exact">Ci sa nasiel presne (inak bez ohladu na velkost pismen alebo diakritiku).</param>
public readonly record struct ExprTrainTypeMatch(int Index, string Key, bool FromTrTypes, bool Exact);

/// <summary>
///     Symboly z dat grafikonu, ktore prekladac a validator potrebuju. Vsetky cleny su nepovinne -
///     predvolene implementacie znamenaju "neviem".
/// </summary>
public interface IExprSymbolProvider
{
    /// <summary>
    ///     Kluce druhov vlakov z TrTypes.txt (2. stlpec) s indexom v zabudovanej tabulke.
    ///     <see langword="null"/>, ak nie su dostupne.
    /// </summary>
    IReadOnlyDictionary<string, int>? TrainTypeKeys => null;

    /// <summary>Ci existuje stanica s danym ID. <see langword="null"/> = neviem.</summary>
    bool? StationExists(int id) => null;

    /// <summary>Ci existuje kolaj s danym nazvom. <see langword="null"/> = neviem.</summary>
    bool? TrackExists(string name) => null;

    /// <summary>Ci existuje dopravca s danym nazvom. <see langword="null"/> = neviem.</summary>
    bool? OperatorExists(string name) => null;
}

/// <summary>
///     Konstanty jazyka vyrazov a ich rozpoznavanie.
/// </summary>
public static class ExprConstants
{
    /// <summary>Predpona konstanty druhu vlaku.</summary>
    public const string TypePrefix = "Typ_";

    /// <summary>Anglicka predpona konstanty druhu vlaku.</summary>
    public const string TypePrefixEnglish = "Type_";

    /// <summary>Konstanty prijimane v kazdom kontexte (tabulky INISSu 0x5591b0 a 0x559230).</summary>
    public static readonly IReadOnlyList<ExprConstantInfo> Common =
    [
        new("Poz_V", 1, ExprConstantGroup.Position),
        new("Poz_P", 2, ExprConstantGroup.Position),
        new("Poz_K", 3, ExprConstantGroup.Position),
        new("Prizn_Vyl", 0x300, ExprConstantGroup.Flag),
        new("Prizn_VylP", 0x100, ExprConstantGroup.Flag),
        new("Prizn_VylO", 0x200, ExprConstantGroup.Flag),
        new("Prizn_M", 0x1000, ExprConstantGroup.Flag),
        new("Prizn_O", 0x2000, ExprConstantGroup.Flag),
        new("Prizn_D", 0x4000, ExprConstantGroup.Flag),
        new("Prizn_X", 0x8000, ExprConstantGroup.Flag),
        new("Prizn_R", 0x10000, ExprConstantGroup.Flag),
        new("Prizn_L", 0x20000, ExprConstantGroup.Flag),
        new("Prizn_N", 0x40000, ExprConstantGroup.Flag),
        new("Prizn_Pre", 0x80000, ExprConstantGroup.Flag),
        new("Stav_Odbaven", 0x20, ExprConstantGroup.State),
        new("Stav_Stoji", 0x80, ExprConstantGroup.State),

        new("Pos_B", 1, ExprConstantGroup.Position, true),
        new("Pos_T", 2, ExprConstantGroup.Position, true),
        new("Pos_E", 3, ExprConstantGroup.Position, true),
        new("Flag_Lockout", 0x300, ExprConstantGroup.Flag, true),
        new("Flag_LockoutA", 0x100, ExprConstantGroup.Flag, true),
        new("Flag_LockoutD", 0x200, ExprConstantGroup.Flag, true),
        new("State_Dispatched", 0x20, ExprConstantGroup.State, true),
        new("State_Standing", 0x80, ExprConstantGroup.State, true)
    ];

    /// <summary>Konstanty udalosti ILTISu - len v kluci <c>Wait</c> (tabulka INISSu 0x559270).</summary>
    public static readonly IReadOnlyList<ExprConstantInfo> IltisEvents =
    [
        new("VVC", unchecked((int)0x80000000), ExprConstantGroup.IltisEvent),
        new("OVC", 0x40000000, ExprConstantGroup.IltisEvent),
        new("ZCV", 0x20000000, ExprConstantGroup.IltisEvent),
        new("Vj", 0x10000000, ExprConstantGroup.IltisEvent),
        new("Odj", 0x08000000, ExprConstantGroup.IltisEvent)
    ];

    private static readonly Dictionary<string, ExprConstantInfo> CommonByName =
        Common.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, ExprConstantInfo> IltisByName =
        IltisEvents.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Rozpozna identifikator ako konstantu.
    /// </summary>
    /// <param name="name">Identifikator.</param>
    /// <param name="context">Kontext prekladu.</param>
    /// <param name="symbols">Symboly grafikonu (kluce TrTypes.txt); moze byt <see langword="null"/>.</param>
    /// <returns>Hodnota, alebo <see langword="null"/>, ak identifikator nie je konstanta.</returns>
    public static ExprConstantMatch? Resolve(string name, ExprContext context, IExprSymbolProvider? symbols)
    {
        if (CommonByName.TryGetValue(name, out var c))
            return new ExprConstantMatch(c.Value, c, null);

        if (context == ExprContext.StateDgmWait && IltisByName.TryGetValue(name, out c))
            return new ExprConstantMatch(c.Value, c, null);

        string? key = null;
        if (name.StartsWith(TypePrefix, StringComparison.OrdinalIgnoreCase))
            key = name[TypePrefix.Length..];
        else if (name.StartsWith(TypePrefixEnglish, StringComparison.OrdinalIgnoreCase))
            key = name[TypePrefixEnglish.Length..];

        if (string.IsNullOrEmpty(key))
            return null;

        var type = ExprTrainTypes.Resolve(key, symbols?.TrainTypeKeys);
        return type is null ? null : new ExprConstantMatch(type.Value.Index, null, type);
    }
}

/// <summary>
///     Zabudovana tabulka druhov vlakov INISSu (95 miest) a hladanie <c>Typ_…</c>.
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
    ///     Nazvy druhov podla indexu; prazdne miesta su <see langword="null"/>.
    /// </summary>
    public static IReadOnlyList<string?> BuiltIn => Names;

    /// <summary>
    ///     Vrati zabudovany nazov druhu na indexe, alebo <see langword="null"/>.
    /// </summary>
    public static string? NameOf(int index) => index is >= 0 and < Count ? Names[index] : null;

    /// <summary>
    ///     Index zabudovaneho druhu podla presneho nazvu, alebo -1.
    /// </summary>
    public static int IndexOf(string name) => Array.IndexOf(Names, name);

    /// <summary>
    ///     Ci je druh na indexe nakladny alebo sluzobny (<c>NAKLTYP</c>).
    /// </summary>
    public static bool IsCargo(int index) => index is >= CargoFirst and <= CargoLast;

    /// <summary>
    ///     Najde druh vlaku ako INISS: najprv medzi klucmi TrTypes.txt, potom medzi zabudovanymi nazvami;
    ///     v oboch presne, potom bez ohladu na velkost pismen, potom aj bez diakritiky.
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
