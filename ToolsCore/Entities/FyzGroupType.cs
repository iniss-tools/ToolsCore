using ToolsCore.Tools;

namespace ToolsCore.Entities;

/// <summary>
///     Trieda reprezentujúca typ priečinka v zvukovej banke.
/// </summary>
public sealed class FyzGroupType : Enumeration<FyzGroupType>
{
    private FyzGroupType(int id, string name, string description = "") : base(id, name, description)
    {
    }

    /// <summary>
    ///     Vráti tuto inštanciu triedy (používané pre GUI).
    /// </summary>
    public FyzGroupType This => this;

    /// <inheritdoc />
    public override string ToString() => $"{Name} - {Description}";

    /// <summary>
    ///     Prevedie kľúč skupiny na jej typ, ak je kľúč neznámy, vráti <see cref="UNCATEGORIZED"/>.
    /// </summary>
    /// <param name="name">Kľúč skupiny (názov sa môže líšiť, napr. kľúč VlakNum s názvom "Číslovky").</param>
    /// <returns>typ priečinka</returns>
    public new static FyzGroupType Parse(string name)
    {
        return name.ToUpper() switch
        {
            "C1" => C1,
            "C2" => C2,
            "C3" => C3,
            "C9" => C9,
            "CISLO_" => CISLO_,
            "CISLO1" => CISLO1,
            "CISLO2" => CISLO2,
            "CISLO3" => CISLO3,
            "CISLO4" => CISLO4,
            "CISLO5" => CISLO5,
            "CISLO6" => CISLO6,
            "CISLO7" => CISLO7,
            "CISLO8" => CISLO8,
            "CISLO9" => CISLO9,
            "DODATKY" => DODATKY,
            "DOPRAVCA" => DOPRAVCA,
            "O1" => DOPRAVCA,
            "DZ" => DZ,
            "K1" => K1,
            "K2" => K2,
            "K3" => K3,
            "K4" => K4,
            "LINKA" => LINKA,
            "N1" => N1,
            "N2" => N2,
            "N3" => N3,
            "N4" => N4,
            "N5" => N5,
            "N10" => N10,
            "N11" => N11,
            "N12" => N12,
            "N13" => N13,
            "POZ1" => POZ1,
            "POZ2" => POZ2,
            "POZ3" => POZ3,
            "POZ7" => POZ7,
            "R1" => R1,
            "R2" => R2,
            "R3" => R3,
            "R4" => R4,
            "ŘAZENÍ" => RAZENI,
            "RAZENI" => RAZENI,
            "REKLAMA" => REKLAMA,
            "SLOVA" => SLOVA,
            "V1" => V1,
            "V2" => V2,
            "V4" => V4,
            "V8" => V8,
            "V14" => V14,
            "VETY" => VETY,
            "VLAKNUM" => VLAKNUM,
            "VOZY1" => VOZY1,
            "VOZY1M" => VOZY1M,
            "VOZY2" => VOZY2,
            "VOZY2M" => VOZY2M,
            "VOZY3" => VOZY3,
            "VOZY3M" => VOZY3M,
            "VOZY4" => VOZY4,
            "VOZY4M" => VOZY4M,
            "VOZY5" => VOZY5,
            "VOZY5M" => VOZY5M,
            "VOZY6" => VOZY6,
            "VOZY6M" => VOZY6M,
            "VOZY7" => VOZY7,
            "VOZY7M" => VOZY7M,
            "VOZY8" => VOZY8,
            "VOZY8M" => VOZY8M,
            "ZNELKY" => ZNELKY,
            _ => UNCATEGORIZED
        };
    }

    #region VALUES

#pragma warning disable 1591
    public static readonly FyzGroupType UNCATEGORIZED = new(0, "Nezaradené");
    public static readonly FyzGroupType C1 = new(1, "C1", "hodiny");
    public static readonly FyzGroupType C2 = new(2, "C2", "minúty, napr.: 2 minúty.");
    public static readonly FyzGroupType C3 = new(3, "C3", "minúty, napr.: 2 minúty,");
    public static readonly FyzGroupType C9 = new(4, "C9", "minúty pri meškaní");
    public static readonly FyzGroupType CISLO_ = new(5, "CISLO_", "číslo s čiarkou, napr.: šesť,");
    public static readonly FyzGroupType CISLO1 = new(6, "CISLO1", "číslo, napr.: šesť");
    public static readonly FyzGroupType CISLO2 = new(7, "CISLO2", "číslo s predponami, napr.: 01");
    public static readonly FyzGroupType CISLO3 = new(8, "CISLO3", "číslo na konci vety, napr.: šesť.");
    public static readonly FyzGroupType CISLO4 = new(9, "CISLO4", "číslo so spojkou \"a\",napr.: a šesť");
    public static readonly FyzGroupType CISLO5 = new(10, "CISLO5", "číslo so spojkou \"a\" s čiarkou,napr.: a šesť,");
    public static readonly FyzGroupType CISLO6 = new(11, "CISLO6", "číslo so spojkou \"a\" na konci vety,napr.: a šesť.");
    public static readonly FyzGroupType CISLO7 = new(12, "CISLO7", "číslo so spojkou \"až\",napr.: až šesť");
    public static readonly FyzGroupType CISLO8 = new(13, "CISLO8", "číslo so spojkou \"až\",napr.: až šesť,");
    public static readonly FyzGroupType CISLO9 = new(14, "CISLO9", "číslo so spojkou \"až\" na konci vety,napr.: až šesť.");
    public static readonly FyzGroupType DODATKY = new(15, "DODATKY", "dodatkové hlásenia");
    public static readonly FyzGroupType DOPRAVCA = new(16, "DOPRAVCA/O1", "názvy dopravcov");
    public static readonly FyzGroupType DZ = new(17, "DZ", "dôvody meškania");
    public static readonly FyzGroupType K1 = new(18, "K1", "názov koľaje na konci vety, napr.: koľaj dva.");
    public static readonly FyzGroupType K2 = new(19, "K2", "názov koľaje s čiarkou, napr.: koľaj dva,");
    public static readonly FyzGroupType K3 = new(20, "K3", "názov koľaje s \"Po\", napr.: Po koľaji dva");
    public static readonly FyzGroupType K4 = new(21, "K4", "názov koľaje  s predložkou \"pri\", napr.: pri koľaji dva");
    public static readonly FyzGroupType LINKA = new(22, "LINKA", "názvy liniek");
    public static readonly FyzGroupType N1 = new(23, "N1", "názov nástupištia s predložkou \"k\", napr.: k nástupišťu dva,");
    public static readonly FyzGroupType N2 = new(24, "N2", "názov nástupištia s predložkou \"pri\", napr.: pri nástupišti dva,");
    public static readonly FyzGroupType N3 = new(25, "N3", "názov nástupištia s predložkou \"na\", napr.: na nástupišTE dva,");
    public static readonly FyzGroupType N4 = new(26, "N4", "názov nástupištia s predložkou \"Pri\", napr.: Pri nástupišti jedna");
    public static readonly FyzGroupType N5 = new(27, "N5", "názov nástupištia s predložkou \"na\", napr.: na nástupišTI dva,");
    public static readonly FyzGroupType N10 = new(28, "N10", "názov koľaje s \"na\" na konci vety (bez nástupíšť), napr.: na koľaj dva.");
    public static readonly FyzGroupType N11 = new(29, "N11", "názov koľaje s \"na\" s čiarkou (bez nástupíšť), napr.: na koľaj dva,");
    public static readonly FyzGroupType N12 = new(30, "N12", "názov koľaje s \"Pri\" na začiatku vety (bez nástupíšť), napr.: Pri koľaji dva");
    public static readonly FyzGroupType N13 = new(31, "N13", "názov koľaje s \"na\" na konci vety (bez nástupíšť), napr.: na koľaJI dva.");
    public static readonly FyzGroupType POZ1 = new(32, "Poz1", "poznámky k vlaku, napr.: na konci vlaku");
    public static readonly FyzGroupType POZ2 = new(33, "Poz2", "poznámky k vlaku s čiarkou, napr.: na konci vlaku,");
    public static readonly FyzGroupType POZ3 = new(34, "Poz3", "poznámky k vlaku na konci vety, napr.: na konci vlaku.");
    public static readonly FyzGroupType POZ7 = new(35, "Poz7", "poznámky k vlaku, napr.: na konci vlaku");
    public static readonly FyzGroupType R1 = new(36, "R1", "názov stanice s čiarkou, napr.: Abda,");
    public static readonly FyzGroupType R2 = new(37, "R2", "názov stanice so spojkou \"a\" s čiarkou, napr.: a Abda,");
    public static readonly FyzGroupType R3 = new(38, "R3", "názov stanice napr.: Abda");
    public static readonly FyzGroupType R4 = new(39, "R4", "názov stanice na konci vety.: Abda.");
    public static readonly FyzGroupType RAZENI = new(40, "ŘAZENÍ", "celé hlásenie o radení");
    public static readonly FyzGroupType REKLAMA = new(41, "REKLAMA", "reklamné hlásenia");
    public static readonly FyzGroupType SLOVA = new(42, "SLOVA", "slová");
    public static readonly FyzGroupType V1 = new(43, "V1", "druh vlaku, napr.: osobný vlak");
    public static readonly FyzGroupType V2 = new(44, "V2", "druh vlaku s predložkou \"do\", napr.: do osobného vlaku");
    public static readonly FyzGroupType V4 = new(45, "V4", "druh vlaku na začiatku vety, napr.: Osobný vlak");
    public static readonly FyzGroupType V8 = new(46, "V8", "názvy vlakov");
    public static readonly FyzGroupType V14 = new(47, "V14", "druh vlaku s príponami na zač. vety");
    public static readonly FyzGroupType VETY = new(48, "VETY", "ucelené vetné hlásenia");
    public static readonly FyzGroupType VLAKNUM = new(49, "VLAKNUM", "čísla použ. na číslovanie vlakov");
    public static readonly FyzGroupType VOZY1 = new(50, "VOZY1", "vlastnosti vozňa, napr.: reštauračný vozeň");
    public static readonly FyzGroupType VOZY1M = new(51, "VOZY1M", "vlastnosti vozňov (množ.č), napr.: reštauračný vozeň");
    public static readonly FyzGroupType VOZY2 = new(52, "VOZY2", "vlastnosti vozňa s čiarkou, napr.: reštauračný vozeň,");
    public static readonly FyzGroupType VOZY2M = new(53, "VOZY2M", "vlastnosti vozňov s čiarkou (množ.č), napr.: reštauračný vozeň,");
    public static readonly FyzGroupType VOZY3 = new(54, "VOZY3", "vlastnosti vozňa na konci vety, napr.: reštauračný vozeň.");
    public static readonly FyzGroupType VOZY3M = new(55, "VOZY3M", "vlastnosti vozňov na konci vety (množ.č), napr.: reštauračný vozeň.");
    public static readonly FyzGroupType VOZY4 = new(56, "VOZY4", "vlastnosti vozňa so spojkou \"a\", napr.: a reštauračný vozeň");
    public static readonly FyzGroupType VOZY4M = new(57, "VOZY4M", "vlastnosti vozňov so spojkou \"a\" (množ.č), napr.: a reštauračný vozeň");
    public static readonly FyzGroupType VOZY5 = new(58, "VOZY5", "vlastnosti vozňa so spojkou \"a\" s čiarkou, napr.: a reštauračný vozeň,");
    public static readonly FyzGroupType VOZY5M = new(59, "VOZY5M", "vlastnosti vozňov so spojkou \"a\" s čiarkou (množ.č), napr.: a reštauračný vozeň,");
    public static readonly FyzGroupType VOZY6 = new(60, "VOZY6", "vlastnosti vozňa so spojkou \"a\" na konci vety, napr.: a reštauračný vozeň,");
    public static readonly FyzGroupType VOZY6M = new(61, "VOZY6M", "vlastnosti vozňov so spojkou \"a\" na konci vety (množ.č), napr.: a reštauračný vozeň,");
    public static readonly FyzGroupType VOZY7 = new(62, "VOZY7", "vlastnosti vozňa, napr.: reštauračný vozeň");
    public static readonly FyzGroupType VOZY7M = new(63, "VOZY7M", "vlastnosti vozňov (množ.č), napr.: reštauračný vozeň");
    public static readonly FyzGroupType VOZY8 = new(64, "VOZY8", "vlastnosti vozňa s čiarkou, napr.: reštauračný vozeň,");
    public static readonly FyzGroupType VOZY8M = new(65, "VOZY8M", "vlastnosti vozňov s čiarkou (množ.č), napr.: reštauračný vozeň,");
    public static readonly FyzGroupType ZNELKY = new(66, "ZNELKY", "znelky");
#pragma warning restore 1591

    #endregion
}