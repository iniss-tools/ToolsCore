namespace ToolsCore.XML;

/// <summary>
/// Kategoria poloziek farebnej schemy (vetva stromu na stranke schemy) s volitelnym nahladom.
/// </summary>
/// <param name="Id">identifikator (zaciatok identifikatorov jej poloziek)</param>
/// <param name="Name">nazov v jazyku programu</param>
/// <param name="CreatePreview">nahlad kategorie; <see langword="null" /> = bez nahladu</param>
public sealed record ColorCategory(string Id, Func<string> Name, Func<IColorPreview>? CreatePreview = null);
