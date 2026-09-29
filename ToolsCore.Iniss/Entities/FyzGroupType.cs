using ToolsCore.Iniss.Properties;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss.Entities;

/// <summary>
/// Trieda reprezentujúca typ priečinka v zvukovej banke.
/// </summary>
public sealed class FyzGroupType : Enumeration<FyzGroupType>
{
    private FyzGroupType(int id, string name, string description = "") : base(id, name, description)
    {
    }

    /// <summary>
    /// Vráti tuto inštanciu triedy (používané pre GUI).
    /// </summary>
    public FyzGroupType This => this;

    /// <inheritdoc />
    public override string ToString() => $"{Name} - {Description}";

    /// <summary>
    /// Prevedie kľúč skupiny na jej typ, ak je kľúč neznámy, vráti <see cref="Uncategorized"/>.
    /// </summary>
    /// <param name="name">Kľúč skupiny (názov sa môže líšiť, napr. kľúč VlakNum s názvom "Číslovky").</param>
    /// <returns>typ priečinka</returns>
    public new static FyzGroupType Parse(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "C1" => C1,
            "C2" => C2,
            "C3" => C3,
            "C9" => C9,
            "CISLO_" => Cislo,
            "CISLO1" => Cislo1,
            "CISLO2" => Cislo2,
            "CISLO3" => Cislo3,
            "CISLO4" => Cislo4,
            "CISLO5" => Cislo5,
            "CISLO6" => Cislo6,
            "CISLO7" => Cislo7,
            "CISLO8" => Cislo8,
            "CISLO9" => Cislo9,
            "DODATKY" => Dodatky,
            "DOPRAVCA" => Dopravca,
            "O1" => Dopravca,
            "DZ" => Dz,
            "K1" => K1,
            "K2" => K2,
            "K3" => K3,
            "K4" => K4,
            "LINKA" => Linka,
            "N1" => N1,
            "N2" => N2,
            "N3" => N3,
            "N4" => N4,
            "N5" => N5,
            "N10" => N10,
            "N11" => N11,
            "N12" => N12,
            "N13" => N13,
            "POZ1" => Poz1,
            "POZ2" => Poz2,
            "POZ3" => Poz3,
            "POZ7" => Poz7,
            "R1" => R1,
            "R2" => R2,
            "R3" => R3,
            "R4" => R4,
            "ŘAZENÍ" => Razeni,
            "RAZENI" => Razeni,
            "REKLAMA" => Reklama,
            "SLOVA" => Slova,
            "V1" => V1,
            "V2" => V2,
            "V4" => V4,
            "V8" => V8,
            "V14" => V14,
            "VETY" => Vety,
            "VLAKNUM" => Vlaknum,
            "VOZY1" => Vozy1,
            "VOZY1M" => Vozy1M,
            "VOZY2" => Vozy2,
            "VOZY2M" => Vozy2M,
            "VOZY3" => Vozy3,
            "VOZY3M" => Vozy3M,
            "VOZY4" => Vozy4,
            "VOZY4M" => Vozy4M,
            "VOZY5" => Vozy5,
            "VOZY5M" => Vozy5M,
            "VOZY6" => Vozy6,
            "VOZY6M" => Vozy6M,
            "VOZY7" => Vozy7,
            "VOZY7M" => Vozy7M,
            "VOZY8" => Vozy8,
            "VOZY8M" => Vozy8M,
            "ZNELKY" => Znelky,
            _ => Uncategorized
        };
    }

    #region VALUES

#pragma warning disable 1591
    public static readonly FyzGroupType Uncategorized = new(0, Resources.FyzGroupType_UNCATEGORIZED);
    public static readonly FyzGroupType C1 = new(1, "C1", Resources.FyzGroupType_C1);
    public static readonly FyzGroupType C2 = new(2, "C2", Resources.FyzGroupType_C2);
    public static readonly FyzGroupType C3 = new(3, "C3", Resources.FyzGroupType_C3);
    public static readonly FyzGroupType C9 = new(4, "C9", Resources.FyzGroupType_C9);
    public static readonly FyzGroupType Cislo = new(5, "CISLO_", Resources.FyzGroupType_CISLO_);
    public static readonly FyzGroupType Cislo1 = new(6, "CISLO1", Resources.FyzGroupType_CISLO1);
    public static readonly FyzGroupType Cislo2 = new(7, "CISLO2", Resources.FyzGroupType_CISLO2);
    public static readonly FyzGroupType Cislo3 = new(8, "CISLO3", Resources.FyzGroupType_CISLO3);
    public static readonly FyzGroupType Cislo4 = new(9, "CISLO4", Resources.FyzGroupType_CISLO4);
    public static readonly FyzGroupType Cislo5 = new(10, "CISLO5", Resources.FyzGroupType_CISLO5);
    public static readonly FyzGroupType Cislo6 = new(11, "CISLO6", Resources.FyzGroupType_CISLO6);
    public static readonly FyzGroupType Cislo7 = new(12, "CISLO7", Resources.FyzGroupType_CISLO7);
    public static readonly FyzGroupType Cislo8 = new(13, "CISLO8", Resources.FyzGroupType_CISLO8);
    public static readonly FyzGroupType Cislo9 = new(14, "CISLO9", Resources.FyzGroupType_CISLO9);
    public static readonly FyzGroupType Dodatky = new(15, "DODATKY", Resources.FyzGroupType_DODATKY);
    public static readonly FyzGroupType Dopravca = new(16, "DOPRAVCA/O1", Resources.FyzGroupType_DOPRAVCA);
    public static readonly FyzGroupType Dz = new(17, "DZ", Resources.FyzGroupType_DZ);
    public static readonly FyzGroupType K1 = new(18, "K1", Resources.FyzGroupType_K1);
    public static readonly FyzGroupType K2 = new(19, "K2", Resources.FyzGroupType_K2);
    public static readonly FyzGroupType K3 = new(20, "K3", Resources.FyzGroupType_K3);
    public static readonly FyzGroupType K4 = new(21, "K4", Resources.FyzGroupType_K4);
    public static readonly FyzGroupType Linka = new(22, "LINKA", Resources.FyzGroupType_LINKA);
    public static readonly FyzGroupType N1 = new(23, "N1", Resources.FyzGroupType_N1);
    public static readonly FyzGroupType N2 = new(24, "N2", Resources.FyzGroupType_N2);
    public static readonly FyzGroupType N3 = new(25, "N3", Resources.FyzGroupType_N3);
    public static readonly FyzGroupType N4 = new(26, "N4", Resources.FyzGroupType_N4);
    public static readonly FyzGroupType N5 = new(27, "N5", Resources.FyzGroupType_N5);
    public static readonly FyzGroupType N10 = new(28, "N10", Resources.FyzGroupType_N10);
    public static readonly FyzGroupType N11 = new(29, "N11", Resources.FyzGroupType_N11);
    public static readonly FyzGroupType N12 = new(30, "N12", Resources.FyzGroupType_N12);
    public static readonly FyzGroupType N13 = new(31, "N13", Resources.FyzGroupType_N13);
    public static readonly FyzGroupType Poz1 = new(32, "Poz1", Resources.FyzGroupType_POZ1);
    public static readonly FyzGroupType Poz2 = new(33, "Poz2", Resources.FyzGroupType_POZ2);
    public static readonly FyzGroupType Poz3 = new(34, "Poz3", Resources.FyzGroupType_POZ3);
    public static readonly FyzGroupType Poz7 = new(35, "Poz7", Resources.FyzGroupType_POZ7);
    public static readonly FyzGroupType R1 = new(36, "R1", Resources.FyzGroupType_R1);
    public static readonly FyzGroupType R2 = new(37, "R2", Resources.FyzGroupType_R2);
    public static readonly FyzGroupType R3 = new(38, "R3", Resources.FyzGroupType_R3);
    public static readonly FyzGroupType R4 = new(39, "R4", Resources.FyzGroupType_R4);
    public static readonly FyzGroupType Razeni = new(40, "ŘAZENÍ", Resources.FyzGroupType_RAZENI);
    public static readonly FyzGroupType Reklama = new(41, "REKLAMA", Resources.FyzGroupType_REKLAMA);
    public static readonly FyzGroupType Slova = new(42, "SLOVA", Resources.FyzGroupType_SLOVA);
    public static readonly FyzGroupType V1 = new(43, "V1", Resources.FyzGroupType_V1);
    public static readonly FyzGroupType V2 = new(44, "V2", Resources.FyzGroupType_V2);
    public static readonly FyzGroupType V4 = new(45, "V4", Resources.FyzGroupType_V4);
    public static readonly FyzGroupType V8 = new(46, "V8", Resources.FyzGroupType_V8);
    public static readonly FyzGroupType V14 = new(47, "V14", Resources.FyzGroupType_V14);
    public static readonly FyzGroupType Vety = new(48, "VETY", Resources.FyzGroupType_VETY);
    public static readonly FyzGroupType Vlaknum = new(49, "VLAKNUM", Resources.FyzGroupType_VLAKNUM);
    public static readonly FyzGroupType Vozy1 = new(50, "VOZY1", Resources.FyzGroupType_VOZY1);
    public static readonly FyzGroupType Vozy1M = new(51, "VOZY1M", Resources.FyzGroupType_VOZY1M);
    public static readonly FyzGroupType Vozy2 = new(52, "VOZY2", Resources.FyzGroupType_VOZY2);
    public static readonly FyzGroupType Vozy2M = new(53, "VOZY2M", Resources.FyzGroupType_VOZY2M);
    public static readonly FyzGroupType Vozy3 = new(54, "VOZY3", Resources.FyzGroupType_VOZY3);
    public static readonly FyzGroupType Vozy3M = new(55, "VOZY3M", Resources.FyzGroupType_VOZY3M);
    public static readonly FyzGroupType Vozy4 = new(56, "VOZY4", Resources.FyzGroupType_VOZY4);
    public static readonly FyzGroupType Vozy4M = new(57, "VOZY4M", Resources.FyzGroupType_VOZY4M);
    public static readonly FyzGroupType Vozy5 = new(58, "VOZY5", Resources.FyzGroupType_VOZY5);
    public static readonly FyzGroupType Vozy5M = new(59, "VOZY5M", Resources.FyzGroupType_VOZY5M);
    public static readonly FyzGroupType Vozy6 = new(60, "VOZY6", Resources.FyzGroupType_VOZY6);
    public static readonly FyzGroupType Vozy6M = new(61, "VOZY6M", Resources.FyzGroupType_VOZY6M);
    public static readonly FyzGroupType Vozy7 = new(62, "VOZY7", Resources.FyzGroupType_VOZY7);
    public static readonly FyzGroupType Vozy7M = new(63, "VOZY7M", Resources.FyzGroupType_VOZY7M);
    public static readonly FyzGroupType Vozy8 = new(64, "VOZY8", Resources.FyzGroupType_VOZY8);
    public static readonly FyzGroupType Vozy8M = new(65, "VOZY8M", Resources.FyzGroupType_VOZY8M);
    public static readonly FyzGroupType Znelky = new(66, "ZNELKY", Resources.FyzGroupType_ZNELKY);
#pragma warning restore 1591

    #endregion
}