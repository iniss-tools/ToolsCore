namespace ToolsCore.Iniss.Registry;

/// <summary>Linka, ktoru subor .INI presmerovava na simulator tabul, a co v nom bolo predtym.</summary>
/// <param name="Section">sekcia (Driver, Driver0…)</param>
/// <param name="OriginalPort">povodny TablePort v .INI; null = v .INI nebol (sekcia bola len v registri alebo bez neho)</param>
/// <param name="RegistryOnly">sekcia bola len v registri - do .INI ju pridalo presmerovanie celu</param>
public sealed record RedirectedLine(string Section, string? OriginalPort, bool RegistryOnly);
