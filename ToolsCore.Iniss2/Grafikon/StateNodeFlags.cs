namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Priznaky stavu a automatiky, ako ich pise statedgm.xml.
/// </summary>
public static class StateNodeFlags
{
    /// <summary>Pomocny stav, vlak don neprejde.</summary>
    public const string Auxiliary = "AUXSTATE";

    /// <summary>Vlak stoji v stanici.</summary>
    public const string Standing = "STOJI";

    /// <summary>Kolaj je potvrdena.</summary>
    public const string TrackConfirmed = "POTVRZENAKOLEJ";

    /// <summary>Vlak je vybaveny.</summary>
    public const string Dispatched = "ODBAVEN";

    /// <summary>Spolu s <see cref="Dispatched" /> skryje vlak v zozname obsluhy.</summary>
    public const string Shadow = "SHADOW";

    /// <summary>Vlak prave stoji.</summary>
    public const string StandingNow = "NYNISTOJI";

    /// <summary>Vlak je na odchodovych castiach tabul.</summary>
    public const string OnDeparture = "JENAODJEZDOVE";

    /// <summary>Vlak je na prichodovych castiach tabul.</summary>
    public const string OnArrival = "JENAPRIJEZDOVE";

    /// <summary>Vlak je na nastupistnych a podchodovych castiach tabul.</summary>
    public const string OnPlatform = "JENASMEROVYCH";

    /// <summary>Na tabuli sa ukaze nastupiste.</summary>
    public const string ShowPlatform = "JEZOBRAZENAPOZICE";

    /// <summary>K nastupistu sa ukaze aj kolaj.</summary>
    public const string ShowTrack = "JEZOBRAZENAKOLEJ";

    /// <summary>Automatika bezi, aj ked stanicu neriadi ziadny klient.</summary>
    public const string KeepWithoutClient = "NEVYPINATBEZKLIENTA";
}
