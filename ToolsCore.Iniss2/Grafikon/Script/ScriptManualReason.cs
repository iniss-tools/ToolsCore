namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Preco funkcia skriptu ostava rucnym kodom.
/// </summary>
public enum ScriptManualReason
{
    /// <summary>Funkcia je hlasenie upravitelne v editore.</summary>
    None,

    /// <summary>Funkcia nema prave jedno oznacenie hlasenia (pomocna funkcia alebo viac oznaceni).</summary>
    NotReport,

    /// <summary>Podpis: nie je <c>Public Sub</c> alebo ma ine parametre nez zoznam stanic.</summary>
    Signature,

    /// <summary>Oznacenie: popis nie je text alebo ma neznamy pomenovany argument.</summary>
    Attribute,

    /// <summary>Telo obsahuje prikaz, ktory model neopise (podmienka, volanie, premenna…).</summary>
    Statement,

    /// <summary>Model by funkciu zapisal inak, nez je v kode (napr. komentar na neobvyklom mieste).</summary>
    Layout
}
