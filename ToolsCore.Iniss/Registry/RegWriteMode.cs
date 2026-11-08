namespace ToolsCore.Iniss.Registry;

/// <summary>Ako hodnota v registri vznika.</summary>
public enum RegWriteMode
{
    /// <summary>Zalozi sa sama pri prvom spusteni s predvolenou hodnotou (spatny zapis).</summary>
    Auto,

    /// <summary>Zalozi sa sama a program ju pocas behu prepisuje.</summary>
    AutoAndApp,

    /// <summary>Nezalozi sa sama, zapisuje ju program pri zmene nastavenia.</summary>
    App,

    /// <summary>Nezalozi sa sama; kym ju niekto nezalozi rucne, plati predvolena hodnota.</summary>
    None
}
