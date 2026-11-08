// ReSharper disable StringLiteralTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Vysledok rozpoznania <c>Typ_…</c>.
/// </summary>
/// <param name="Index">Index v zabudovanej tabulke (0-94).</param>
/// <param name="Key">Kluc za predponou.</param>
/// <param name="FromTrTypes">Ci sa kluc nasiel v TrTypes.txt (inak medzi zabudovanymi nazvami).</param>
/// <param name="Exact">Ci sa nasiel presne (inak bez ohladu na velkost pismen alebo diakritiku).</param>
public readonly record struct ExprTrainTypeMatch(int Index, string Key, bool FromTrTypes, bool Exact);
