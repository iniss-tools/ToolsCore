// ReSharper disable StringLiteralTypo

namespace ToolsCore.Iniss.Expressions;

/// <summary>
/// Symboly z dat grafikonu, ktore prekladac a validator potrebuju. Vsetky cleny su nepovinne -
/// predvolene implementacie znamenaju "neviem".
/// </summary>
public interface IExprSymbolProvider
{
    /// <summary>
    /// Kluce druhov vlakov z TrTypes.txt (2. stlpec) s indexom v zabudovanej tabulke.
    /// <see langword="null"/>, ak nie su dostupne.
    /// </summary>
    IReadOnlyDictionary<string, int>? TrainTypeKeys => null;

    /// <summary>Ci existuje stanica s danym ID. <see langword="null"/> = neviem.</summary>
    bool? StationExists(int id) => null;

    /// <summary>Ci existuje kolaj s danym nazvom. <see langword="null"/> = neviem.</summary>
    bool? TrackExists(string name) => null;

    /// <summary>Ci existuje dopravca s danym nazvom. <see langword="null"/> = neviem.</summary>
    bool? OperatorExists(string name) => null;
}
