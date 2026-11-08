namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Polozka slovnika tabulky TabTab: text pred prekodovanim a po nom, kazdy s pismom.
/// </summary>
/// <param name="KeyText">text pred prekodovanim</param>
/// <param name="KeyFont">pismo kluca; <see cref="TabTabTable.AnyFont" /> = zhoda pri akomkolvek pisme</param>
/// <param name="ValueText">text po prekodovani</param>
/// <param name="ValueFont">pismo hodnoty; <see cref="TabTabTable.AnyFont" /> = pismo vstupu, <see cref="TabTabTable.NoFont" /> = bez pisma</param>
public sealed record TabTabEntry(string KeyText, int KeyFont, string ValueText, int ValueFont);
