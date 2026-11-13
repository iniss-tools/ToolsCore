namespace ToolsCore.Iniss2.Grafikon;

/// <summary>
/// Preco server datumovu poznamku nerozvinie - varianta alebo zaznam potom neplati v ziadny den.
/// </summary>
public enum GvValidityProblem
{
    /// <summary>Poznamka sa rozvinie.</summary>
    None,

    /// <summary>Textova poznamka alebo retazec nul a jednotiek v obdobi dlhsom nez 400 dni.</summary>
    PeriodTooLong,

    /// <summary>Chybna poznamka, retazec nul a jednotiek inej dlzky ako obdobie alebo prazdne obdobie.</summary>
    Invalid
}
