// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Popis funkcie jazyka vyrazov.
/// </summary>
/// <param name="Function">Funkcia.</param>
/// <param name="Name">Meno (ceske/slovenske alebo anglicke - kazdy alias ma vlastny zaznam).</param>
/// <param name="Canonical">Kanonicka podoba (ceska), na ktoru sa alias mapuje pri vyhodnocovani.</param>
/// <param name="ArgKind">Tvar argumentov.</param>
/// <param name="ArgMeaning">Vyznam argumentu.</param>
/// <param name="Returns">Druh hodnoty bez argumentu.</param>
/// <param name="ContextDependent">Vysledok zavisi od kontextu vyhodnotenia (prichodova tabula vs. ine).</param>
public sealed record ExprFunctionInfo(
    ExprFunction Function,
    string Name,
    ExprFunction Canonical,
    ExprArgKind ArgKind,
    ExprArgMeaning ArgMeaning,
    ExprValueKind Returns,
    bool ContextDependent = false)
{
    /// <summary>Ci je zaznam anglickym aliasom.</summary>
    public bool IsAlias => Function != Canonical;

    /// <summary>Ci funkcia pripusta argument.</summary>
    public bool AcceptsArgument => ArgKind != ExprArgKind.None;

    /// <summary>Ci funkcia argument vyzaduje.</summary>
    public bool RequiresArgument => ArgKind is ExprArgKind.RequiredNumber or ExprArgKind.RequiredString;
}
