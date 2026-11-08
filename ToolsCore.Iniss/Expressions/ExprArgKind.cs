// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Tvar argumentov funkcie.
/// </summary>
public enum ExprArgKind
{
    /// <summary>Bez zatvoriek (napr. <c>ZPOZDENI</c>).</summary>
    None,

    /// <summary>Volitelny ciselny argument - bez neho vracia hodnotu, s nim porovnanie (napr. <c>TYP(Typ_R)</c>).</summary>
    OptionalNumber,

    /// <summary>Volitelny argument, ktory moze byt cislo alebo retazec (<c>CVLAKU</c>).</summary>
    OptionalNumberOrString,

    /// <summary>Povinny ciselny argument (<c>ZESMERU(id)</c>).</summary>
    RequiredNumber,

    /// <summary>Povinny retazcovy argument (<c>KOLEJPRIJ("1")</c>).</summary>
    RequiredString
}
