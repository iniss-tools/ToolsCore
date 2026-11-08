// ReSharper disable StringLiteralTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Pomenovana konstanta jazyka vyrazov.
/// </summary>
/// <param name="Name">Meno.</param>
/// <param name="Value">Hodnota.</param>
/// <param name="Group">Skupina.</param>
/// <param name="IsEnglish">Ci ide o anglicky alias.</param>
public sealed record ExprConstantInfo(string Name, int Value, ExprConstantGroup Group, bool IsEnglish = false);
