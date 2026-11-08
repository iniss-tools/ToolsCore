namespace ToolsCore.Iniss2.Tables;

/// <summary>
/// Text vlaku: pre cislo a den plati prvy zaznam, zaznam s poznamkou preto ide pred zaznam bez nej.
/// </summary>
/// <param name="Number">cislo vlaku pri odchode zo stanice</param>
/// <param name="Validity">datumova poznamka; prazdna = plati vzdy</param>
/// <param name="Text">text; <c>#</c> oddeluje casti, prazdny text udaj vyprazdni</param>
/// <param name="Font">cislo pisma; -1 = pismo udaja</param>
public sealed record TrainText(string Number, string Validity, string Text, int Font);
