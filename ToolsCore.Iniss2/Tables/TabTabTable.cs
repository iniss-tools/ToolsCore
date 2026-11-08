namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Pomenovana tabulka pre stlpce tabul: vyraz VB.NET a slovnik.
/// </summary>
/// <param name="Name">nazov, na ktory sa odkazuje stlpec predlohy</param>
public sealed record TabTabTable(string Name)
{
    /// <summary>Text bez pisma.</summary>
    public const int NoFont = -1;

    /// <summary>Kluc: akekolvek pismo; hodnota: pismo vstupu.</summary>
    public const int AnyFont = -2;

    /// <summary>Popis pre citatela suboru.</summary>
    public string Description { get; init; } = "";

    /// <summary>Obsluha vidi prekodovany text.</summary>
    public bool ViewValues { get; init; }

    /// <summary>Slovnik nerozlisuje velkost pismen.</summary>
    public bool IgnoreCase { get; init; }

    /// <summary>Telo funkcie VB.NET, ktora vrati text stlpca; prazdne = bez vyrazu.</summary>
    public string Expression { get; init; } = "";

    /// <summary>Slovnik; dvojica textu a pisma kluca sa nesmie opakovat.</summary>
    public IReadOnlyList<TabTabEntry> Entries { get; init; } = [];
}
