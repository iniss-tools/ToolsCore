namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Vyluka vlaku v grafikone.
/// </summary>
/// <param name="Number">cislo vlaku</param>
/// <param name="Key">cislo zaznamu ConstReports.xml alebo nazov funkcie <c>Vyluka</c> v LogText</param>
/// <param name="From">prvy den obdobia</param>
/// <param name="To">posledny den obdobia</param>
/// <param name="Validity">datumova poznamka v obdobi; prazdna = vsetky dni</param>
public sealed record TrainLockout(string Number, string Key, DateOnly From, DateOnly To, string Validity);
