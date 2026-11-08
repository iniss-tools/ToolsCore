namespace ToolsCore.Iniss.Registry;

/// <summary>Hodnota v jednej vrstve konfiguracie.</summary>
/// <param name="Location">miesto</param>
/// <param name="Name">nazov, pod ktorym tam lezi (aj stary nazov bez jednotky)</param>
/// <param name="Raw">surova hodnota</param>
/// <param name="State">stav</param>
public sealed record RegLayerValue(RegLocation Location, string Name, RegRawValue Raw, RegLayerState State);
