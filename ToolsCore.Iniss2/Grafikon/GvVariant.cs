using System.Collections;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Varianta vlaku - trasa a casy pre dni z datumovej poznamky.
/// </summary>
public sealed record GvVariant
{
    /// <summary>Datumova poznamka (atribut <c>jd</c>); prazdna = kazdy den platnosti vlaku.</summary>
    public string Validity { get; init; } = "";

    /// <summary>
    /// Dni jazdy varianty rozdelenej podla MOS.xml; bit <c>i</c> plati pre zaciatok platnosti vlaku plus <c>i</c> dni.
    /// <see langword="null" /> = dni urcuje <see cref="Validity" />.
    /// </summary>
    public BitArray? Days { get; init; }

    /// <summary>Dopravca.</summary>
    public string Operator { get; init; } = "";

    /// <summary>Priznaky varianty.</summary>
    public GvVariantFlags Flags { get; init; }

    /// <summary>Poznamka.</summary>
    public string Note { get; init; } = "";

    /// <summary>Body trasy v poradi jazdy.</summary>
    public required IReadOnlyList<GvStop> Stops { get; init; }

    /// <summary>
    /// Bod trasy, od ktoreho plati cislo a druh vlaku v bode <paramref name="index" />. Prichod plati este z predchadzajuceho
    /// useku, vlak moze v stanici zmenit cislo.
    /// </summary>
    /// <param name="index">index bodu trasy</param>
    /// <param name="arrival">pri prichode; inak pri odchode</param>
    /// <returns>bod so zmenou alebo <see langword="null" />, ak trasa cislo nema.</returns>
    public GvStop? NumberSource(int index, bool arrival)
    {
        for (var i = arrival && index > 0 ? index - 1 : index; i >= 0; i--)
        {
            if (Stops[i].Number.Length > 0)
                return Stops[i];
        }

        return null;
    }
}
