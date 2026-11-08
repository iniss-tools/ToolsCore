namespace ToolsCore.Iniss.Registry;

/// <summary>
/// Vetva konfiguracie INISSu nacitana zo suboru <c>.reg</c>.
/// </summary>
/// <param name="Location">miesto registra, pod ktorym v subore je</param>
/// <param name="AppName">nazov aplikacie pod CHAPS</param>
/// <param name="Branch">sekcie a hodnoty</param>
public sealed record RegFileBranch(RegLocation Location, string AppName, RegBranch Branch);
