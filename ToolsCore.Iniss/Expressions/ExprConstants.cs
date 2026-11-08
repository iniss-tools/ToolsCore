// ReSharper disable StringLiteralTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Konstanty jazyka vyrazov a ich rozpoznavanie.
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
    /// Rozpozna identifikator ako konstantu.
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
