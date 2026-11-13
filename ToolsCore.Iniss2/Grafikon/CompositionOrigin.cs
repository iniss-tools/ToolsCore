namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Povod textu radenia (prvok <c>editvl</c>). Server ani klient s nim nepracuju, pri zapise ho len zachovaju.
/// </summary>
/// <param name="Source">zapis vety, napriklad nazov vzoru a jeho parametre (atribut <c>prepis</c>)</param>
/// <param name="Texts">veta v jednotlivych jazykoch: kluc jazyka a text</param>
public sealed record CompositionOrigin(string Source, IReadOnlyList<(string Language, string Text)> Texts);
