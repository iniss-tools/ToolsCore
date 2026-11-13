namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Odklon vlaku v stanici grafikonu.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Station">cislo stanice</param>
/// <param name="Key">cislo zaznamu ConstReports.xml alebo nazov funkcie <c>Odklon</c> v LogText</param>
/// <param name="From">prvy den obdobia</param>
/// <param name="To">posledny den obdobia; <see cref="DateOnly.MinValue" /> = bez konca (server vtedy pocita do 31. 12. 9999)</param>
/// <param name="Validity">datumova poznamka v obdobi; prazdna = vsetky dni</param>
public sealed record TrainDiversion(string Number, int Station, string Key, DateOnly From, DateOnly To, string Validity)
{
    /// <summary>Atribut <c>de</c>; server ho sam nenastavuje, pouziva ho len pri cisteni starych zaznamov.</summary>
    public bool EditedForDay { get; init; }
}
