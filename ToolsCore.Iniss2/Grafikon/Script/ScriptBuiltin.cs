namespace ToolsCore.Iniss2.Grafikon.Script;

/// <summary>
/// Vstavana metoda servera, ktoru hlasenie vola ako krok (napr. druh vlaku, kolaj, zoznam stanic).
/// </summary>
/// <param name="Method">nazov metody v triede <c>LogTextBase</c></param>
/// <param name="Parameters">parametre v poradi servera</param>
public sealed record ScriptBuiltin(string Method, IReadOnlyList<ScriptParameter> Parameters)
{
    /// <summary>Pocet povinnych parametrov.</summary>
    public int Required => Parameters.Count(parameter => !parameter.Optional);

    /// <summary>
    /// Argumenty zodpovedaju parametrom: poradove najviac pocet parametrov, pomenovane podla nazvu parametra (bez ohladu
    /// na velkost pismen) za poradovymi, kazdy parameter najviac raz a vsetky povinne zadane.
    /// </summary>
    /// <param name="arguments">argumenty ako v kode</param>
    public bool Accepts(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var given = new bool[Parameters.Count];
        var position = 0;
        var named = false;
        foreach (var argument in arguments)
        {
            if (ScriptArguments.Named(argument) is { } name)
            {
                named = true;
                var index = Parameters.ToList().FindIndex(parameter => string.Equals(parameter.Name, name.Name, StringComparison.OrdinalIgnoreCase));
                if (index < 0 || given[index])
                    return false;
                given[index] = true;
                continue;
            }

            if (named || position >= Parameters.Count)
                return false;
            given[position++] = true;
        }

        return Parameters.Select((parameter, index) => parameter.Optional || given[index]).All(ok => ok);
    }

    /// <summary>
    /// Argument pre parameter: poradovy na jeho mieste alebo pomenovany; <see langword="null" />, ak nie je zadany.
    /// </summary>
    public string? Argument(IReadOnlyList<string> arguments, int parameter)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        foreach (var argument in arguments)
        {
            if (ScriptArguments.Named(argument) is { } named && string.Equals(named.Name, Parameters[parameter].Name, StringComparison.OrdinalIgnoreCase))
                return named.Value;
        }

        return parameter < arguments.Count && ScriptArguments.Named(arguments[parameter]) is null ? arguments[parameter] : null;
    }
}
