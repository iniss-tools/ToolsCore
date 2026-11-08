namespace ToolsCore.Iniss.StateDgm;

/// <summary>
/// Typovany stavovy diagram - obsah suboru StateDgm.txt.
/// </summary>
public sealed class StateDgmDiagram
{
    /// <summary>Hlavickove komentare (<c>C:"…"</c>).</summary>
    public List<string> HeaderComments { get; } = [];

    /// <summary>Vzhlady tlacidiel.</summary>
    public List<StateDgmDesign> Designs { get; } = [];

    /// <summary>Vlastne casove body.</summary>
    public List<StateDgmTimePoint> TimePoints { get; } = [];

    /// <summary>Vyraz urcujuci kategoriu vlaku; null = predvolene <c>INDCAT6</c>.</summary>
    public string? IndCat { get; set; }

    /// <summary>Kategorie.</summary>
    public List<StateDgmCategory> Categories { get; } = [];

    /// <summary>Nezname polozky bloku <c>CtrlDesign</c>.</summary>
    public List<StateDgmItem> DesignExtras { get; } = [];

    /// <summary>Nezname polozky hlavicky <c>StateDgm</c>.</summary>
    public List<StateDgmItem> HeaderExtras { get; } = [];

    /// <summary>Nezname polozky skupiny <c>StateDgmCtrls</c> (mimo CtrlDesign a StateDgm).</summary>
    public List<StateDgmItem> CtrlsExtras { get; } = [];

    /// <summary>Nezname bloky na najvyssej urovni (mimo <c>StateDgmCtrls</c>).</summary>
    public List<StateDgmItem> RootExtras { get; } = [];

    /// <summary>Upozornenia z posledneho nacitania.</summary>
    public List<StateDgmLoadWarning> Warnings { get; } = [];

    /// <summary>Vyraz IndCat, ktory INISS skutocne pouzije.</summary>
    public string EffectiveIndCat => string.IsNullOrWhiteSpace(IndCat) ? StateDgmKeys.DefaultIndCat : IndCat;

    /// <summary>Vsetky kluce casovych bodov - zabudovane aj vlastne.</summary>
    public IEnumerable<string> AllTimePointKeys => StateDgmKeys.BuiltInTimePoints.Concat(TimePoints.Select(t => t.Key));

    /// <summary>Vzhlad podla kluca.</summary>
    public StateDgmDesign? FindDesign(string key) => Designs.FirstOrDefault(d => d.Key == key);

    /// <summary>Kategoria podla kluca.</summary>
    public StateDgmCategory? FindCategory(string key) => Categories.FirstOrDefault(c => c.Key == key);

    /// <summary>Nacita diagram z textu suboru. Chyby syntaxe vyhadzuje <see cref="StateDgmParseException" />.</summary>
    public static StateDgmDiagram Parse(string text) => StateDgmConverter.FromTree(StateDgmReader.Read(text));

    /// <summary>Nacita diagram zo suboru v kodovani windows-1250.</summary>
    public static StateDgmDiagram Load(string path) => StateDgmConverter.FromTree(StateDgmReader.ReadFile(path));

    /// <summary>Zapise diagram do textu suboru (kanonicky tvar s tabulatormi a komentarmi).</summary>
    public string ToText() => StateDgmWriter.Write(this);

    /// <summary>Zapise diagram do suboru v kodovani windows-1250.</summary>
    public void Save(string path) => File.WriteAllText(path, ToText(), Tools.Encodings.Win1250);
}
