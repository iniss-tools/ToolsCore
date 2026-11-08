// ReSharper disable StringLiteralTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Vysledok rozpoznania konstanty.
/// </summary>
/// <param name="Value">Hodnota.</param>
/// <param name="Constant">Popis konstanty, ak islo o pomenovanu konstantu.</param>
/// <param name="TrainType">Index druhu vlaku, ak islo o <c>Typ_…</c>.</param>
public readonly record struct ExprConstantMatch(int Value, ExprConstantInfo? Constant, ExprTrainTypeMatch? TrainType);
