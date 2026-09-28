namespace ToolsCore.Tools;

/// <summary>
///     Trieda reprezentujuca zoznam vlastnosti bez polí.
/// </summary>
public class TxtProps
{
    private const string EX_MESSAGE = "Súbor {0}: Očakávala sa vlastnosť {1}.";

    private readonly string _fileName;
    private readonly Dictionary<string, string> _dictionary;

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="TxtProps"/>.
    /// </summary>
    /// <param name="file">Cesta k suboru do/z ktore sa budu ukladat/nacitat subory.</param>
    /// <param name="write">Ak je false, zoznam vlastnosti a hodnot sa nacita zo suboru.</param>
    public TxtProps(string file, bool write = false)
    {
        _fileName = file;
        _dictionary = new Dictionary<string, string>();

        if (!write) 
            LoadFromFile(file);
    }

    /// <summary>
    ///     Vrati hodnotu vlastnosti, alebo <paramref name="defValue"/> ak
    ///     zadany nazov vlastnosti <paramref name="field"/> nebol najdeny v slovniku vlastnosti,
    ///     pricom sa nevyvola ziadna vynimka.
    /// </summary>
    /// <param name="field">Nazov vlastnosti.</param>
    /// <param name="defValue">Predvolena hodnota.</param>
    /// <returns>hodnotu vlastnosti, alebo <paramref name="defValue"/> ak zadany nazov vlastnosti nebol najdeny v slovniku vlastnosti.</returns>
    public string Get(string field, string defValue) => Get(field, false) ?? defValue;

    /// <summary>
    ///     Vrati hodnotu vlastnosti. Ak zadany nazov vlastnosti <paramref name="field"/> nenajde v slovniku vlastnosti,
    ///     vyvola vynimku.
    /// </summary>
    /// <param name="field">Nazov vlastnosti.</param>
    /// <returns>hodnotu vlastnosti.</returns>
    /// <exception cref="ArgumentNullException">Ak nebola najdena vlastnost v slovniku.</exception>
    public string Get(string field) => Get(field, true)!;

    /// <summary>
    ///     Vrati hodnotu vlastnosti. Ak zadany nazov vlastnosti <paramref name="field"/> nenajde v slovniku vlastnosti,
    ///     vrati <see langword="null"/>, alebo vyvola vynimku podla parametra <paramref name="nullSensitive"/>.
    /// </summary>
    /// <param name="field">Nazov vlastnosti.</param>
    /// <param name="nullSensitive"><see langword="true"/>, ak sa pri nenajdenej vlastnosti ma vyvolat vynimka, pri <see langword="false"/> vrati v tomto pripade <see langword="null"/>.</param>
    /// <returns>hodnotu vlastnosti, alebo <see langword="null"/> ak parameter nullSensitive bol nastaveny na <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Ak nebola najdena vlastnost v slovniku a zaroven parameter <paramref name="nullSensitive"/> bol nastaveny na <see langword="true"/>.</exception>
    public string? Get(string field, bool nullSensitive = true)
    {
        if (nullSensitive)
            return _dictionary.ContainsKey(field) 
                ? _dictionary[field] : throw new ArgumentNullException(nameof(field), string.Format(EX_MESSAGE, _fileName, field));

        return _dictionary.ContainsKey(field) ? _dictionary[field] : null;
    }

    /// <summary>
    ///     Nastavi hodnotu vlastnosti. Ak zadany nazov vlastnosti <paramref name="field"/> nenajde v slovniku vlastnosti,
    ///     vytvori novu vlastnost s tymto nazvom a nastavi jej zadanu hodnotu specifikovanu v parametri <paramref name="value"/>.
    /// </summary>
    /// <param name="field">Nazov vlastnosti.</param>
    /// <param name="value">Hodnota vlastnosti.</param>
    public void Set(string field, object value)
    {
        if (!_dictionary.ContainsKey(field))
            _dictionary.Add(field, value.ToString() ?? "");
        else
            _dictionary[field] = value.ToString() ?? "";
    }

    /// <summary>
    ///     Ulozi zoznam vlastnosti do suboru.
    /// </summary>
    public void Save()
    {
        var file = new StreamWriter(_fileName, false, Encodings.Win1250);

        foreach (var prop in _dictionary.Keys.ToArray())
            if (!string.IsNullOrWhiteSpace(_dictionary[prop]))
                file.WriteLine(prop + "=" + _dictionary[prop]);

        file.Close();
    }

    /// <summary>
    ///     Nacita zoznam vlastnosti zo suboru.
    /// </summary>
    private void LoadFromFile(string file)
    {
        foreach (var line in File.ReadAllLines(file, Encodings.Win1250))
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("'") || !line.Contains('=')) 
                continue;

            var index = line.IndexOf('=');
            var key = line.Substring(0, index).Trim();
            var value = line.Substring(index + 1).Trim();

            if (value.StartsWith("\"") && value.EndsWith("\"") ||
                value.StartsWith("'") && value.EndsWith("'"))
                value = value.Substring(1, value.Length - 2);

            try
            {
                _dictionary.Add(key, value);
            }
            catch
            {
                /* ignored */
            }
        }
    }
}