namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Sposob spojenia linky.
/// </summary>
public enum CommLineType
{
    /// <summary>Linka bez spojenia alebo s neznamym klucom.</summary>
    None,

    /// <summary>Seriovy port (kluc SERIAL).</summary>
    Serial,

    /// <summary>Server pocuva datagramy (kluc UDP_SERVER).</summary>
    UdpServer,

    /// <summary>Server posiela datagramy na adresu (kluc UDP_CLIENT).</summary>
    UdpClient,

    /// <summary>Server pocuva spojenia TCP (kluc TCP_SERVER).</summary>
    TcpServer,

    /// <summary>Server sa pripaja na adresu cez TCP (kluc TCP_CLIENT).</summary>
    TcpClient
}
