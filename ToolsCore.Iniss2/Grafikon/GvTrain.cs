using System.Collections;

namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Vlak grafikonu s obdobim platnosti a variantmi. Prva varianta, ktora v dany den ide, je ta platna.
/// </summary>
/// <param name="From">prvy den platnosti</param>
/// <param name="To">posledny den platnosti</param>
/// <param name="Variants">varianty vlaku</param>
public sealed record GvTrain(DateOnly From, DateOnly To, IReadOnlyList<GvVariant> Variants)
{
    /// <summary>
    /// Dni jazdy varianty v obdobi vlaku. Bit <c>i</c> plati pre den <see cref="From" /> plus <c>i</c> dni.
    /// </summary>
    public BitArray DaysOf(GvVariant variant)
    {
        ArgumentNullException.ThrowIfNull(variant);
        return variant.Days is { } days ? new BitArray(days) : GvValidity.Days(From, To, variant.Validity);
    }

    /// <summary>
    /// Dni, ked plati prave tato varianta - bez dni, ktore patria niektorej variante pred nou.
    /// </summary>
    public BitArray ActiveDaysOf(GvVariant variant)
    {
        var days = DaysOf(variant);

        foreach (var earlier in Variants.TakeWhile(candidate => !ReferenceEquals(candidate, variant)))
            days.And(DaysOf(earlier).Not());

        return days;
    }
}
