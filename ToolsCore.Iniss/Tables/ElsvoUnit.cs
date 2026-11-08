namespace ToolsCore.Iniss.Tables;

/// <summary>
/// Spinacia jednotka zosilnovaca ELSVO, ako ju INISS pozna z okruhov <c>Audio.txt</c>.
/// </summary>
/// <param name="Line">cislo linky (stlpec 8 <c>E&lt;n&gt;</c>)</param>
/// <param name="Address">adresa jednotky na linke (dolny bajt stlpca 9)</param>
/// <param name="Circuits">nazvy okruhov, ktore jednotka spina, v poradi suboru</param>
public sealed record ElsvoUnit(int Line, int Address, IReadOnlyList<string> Circuits)
{
    /// <summary>Nazov jednotky pre obsluhu: okruhy oddelene ciarkou.</summary>
    public string Name => string.Join(", ", Circuits);
}
