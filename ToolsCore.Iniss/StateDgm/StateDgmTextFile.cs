namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Vysledok nacitania suboru StateDgm.txt do stromu.
/// </summary>
public sealed class StateDgmTextFile
{
    /// <summary>Cislo verzie zapisu z prveho riadka (INISS 3.39 prijme len <c>0001</c>).</summary>
    public string Version { get; set; } = StateDgmReader.Version;

    /// <summary>Hlavickove komentare <c>C:"…"</c> - text za <c>C:</c> po koniec riadka, bez uvodzoviek.</summary>
    public List<string> HeaderComments { get; } = [];

    /// <summary>Korenova skupina; jej podskupiny su bloky <c>P:</c> (bezne jediny <c>StateDgmCtrls</c>).</summary>
    public StateDgmGroup Root { get; } = new("");
}
