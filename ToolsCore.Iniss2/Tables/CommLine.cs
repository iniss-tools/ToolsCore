namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Komunikacna linka servera.
/// </summary>
/// <param name="Key">kluc - podla neho sa na linku odkazuju tabule</param>
/// <param name="Type">sposob spojenia</param>
/// <param name="Protocol">protokol, napriklad ITP</param>
/// <param name="Address">atribut <c>line</c> - adresa alebo port</param>
public sealed record CommLine(string Key, CommLineType Type, string Protocol, string Address)
{
    /// <summary>Server linku vynechava (atribut <c>omit</c>).</summary>
    public bool Omitted { get; init; }

    /// <summary>Povodna adresa, ak je linka presmerovana na simulator.</summary>
    public string? OriginalAddress { get; init; }

    /// <summary>
    /// Linka sa da presmerovat: server sa na nej pripaja na adresu, alebo je seriova a presmerovanie z nej urobi TCP_CLIENT.
    /// Seriova linka ITP nie - server cita odpoved ITP cely paket naraz len pri UDP_CLIENT, cez TCP by ju nedocital.
    /// </summary>
    public bool CanRedirect => Type is CommLineType.UdpClient or CommLineType.TcpClient || Type == CommLineType.Serial && Protocol is not ("ITP" or "ITP2");
}
