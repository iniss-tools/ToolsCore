namespace ToolsCore.Updates;

/// <summary>Co pouzivatel zvolil v okne novej verzie.</summary>
public enum UpdateChoice
{
    /// <summary>Neskor - pri dalsom spusteni sa program opyta znova.</summary>
    Later,

    /// <summary>Na tuto verziu pri spusteni neupozornovat.</summary>
    Skip,

    /// <summary>Instalator je stiahnuty v <see cref="FUpdateApp.InstallerPath" />, program sa ma zavriet a spustit ho.</summary>
    Install
}
