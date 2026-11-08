namespace ToolsCore.XML;

/// <summary>
/// Hodnota polozky farebnej schemy.
/// </summary>
/// <param name="Fore">farba popredia</param>
/// <param name="Back">farba pozadia (<see cref="Color.Empty" /> = polozka pozadie nema)</param>
/// <param name="Bold">tucne pismo</param>
public readonly record struct ColorValue(Color Fore, Color Back, bool Bold = false);
