namespace ToolsCore.Tools;

/// <summary>
///     Trieda reprezentujuca zoznam poli s obsahom bez vlastnosti.
/// </summary>
public class TxtPropsAreas
{
    private readonly string _fileName;
    private readonly Dictionary<string, string> _dictionary;

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="TxtPropsAreas"/>.
    /// </summary>
    /// <param name="file">Cesta k suboru do/z ktore sa budu ukladat/nacitat subory.</param>
    /// <param name="write">Ak je false, zoznam vlastnosti a hodnot sa nacita zo suboru.</param>
    public TxtPropsAreas(string file, bool write = false)
    {
        _fileName = file;
        _dictionary = new Dictionary<string, string>();

        if (!write)
            LoadFromFile(file);
    }

    /// <summary>
    ///     Vrati obsah pola zadaneho nazvom pola <paramref name="area"/>.
    ///     Ak pole s takymto nazvom nenajde, vrati <see langword="null"/>.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <returns></returns>
    public string? Get(string area) => _dictionary.ContainsKey(area) ? _dictionary[area] : null;

    /// <summary>
    ///     Vrati zoznam vsetkych nazvov poli, ktore sa nachadzaju v slovniku.
    /// </summary>
    /// <returns>zoznam vsetkych nazvov poli, ktore sa nachadzaju v slovniku.</returns>
    public IEnumerable<string> GetAreas() => _dictionary.Keys.ToList();

    /// <summary>
    ///     Nastavi obsah pola. Ak zadany nazov pola <paramref name="area"/> nenajde v slovniku poli,
    ///     vytvori nove pole s tymto nazvom a nastavi jej zadany obsah specifikovany v parametri <paramref name="value"/>.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <param name="value">Obsah pola.</param>
    public void Set(string area, object value)
    {
        if (_dictionary.ContainsKey(area))
            _dictionary[area] = value.ToString() ?? "";
        else
            _dictionary.Add(area, value.ToString() ?? "");
    }

    /// <summary>
    ///     Text pred prvou sekciou (uvodne komentare suboru); pri ulozeni sa zapise spat.
    /// </summary>
    public string Preamble { get; set; } = "";

    /// <summary>
    ///     Ulozi zoznam poli do suboru.
    /// </summary>
    public void Save()
    {
        using var file = new StreamWriter(_fileName, false, Encodings.Win1250);

        if (!string.IsNullOrEmpty(Preamble))
        {
            file.WriteLine(Preamble);
            file.WriteLine();
        }

        foreach (var area in _dictionary.Keys)
        {
            file.WriteLine("[" + area + "]");
            file.WriteLine(_dictionary[area].TrimEnd('\r', '\n'));
            file.WriteLine();
        }
    }

    /// <summary>
    ///     Nacita zoznam poli zo suboru tak, ako ho cita INISS: hlavicka je riadok, ktoreho prvy neprazdny znak
    ///     je <c>[</c>, nazov siaha po prve <c>]</c>; vsetko ostatne (aj komentare a prazdne riadky) patri
    ///     do textu aktualnej sekcie. Pri opakovanom nazve sekcie plati posledna (ako v INISSe).
    /// </summary>
    private void LoadFromFile(string file)
    {
        string? actualArea = null;
        var sb = new StringBuilder();

        void Flush()
        {
            var text = sb.ToString().TrimEnd('\r', '\n');
            if (actualArea is null)
            {
                Preamble = text;
            }
            else
            {
                if (_dictionary.ContainsKey(actualArea))
                    Log.Warning($"{Path.GetFileName(file)}: sekcia [{actualArea}] je v súbore viackrát – použije sa posledná (ako v INISSe).");
                _dictionary[actualArea] = text;
            }
            sb.Clear();
        }

        foreach (var line in File.ReadAllLines(file, Encodings.Win1250))
        {
            var trimmed = line.TrimStart(' ', '\t');
            if (trimmed.StartsWith('[') && trimmed.IndexOf(']', 1) is var close and > 0)
            {
                Flush();
                actualArea = trimmed[1..close];
                continue;
            }

            sb.AppendLine(line);
        }

        Flush();
    }
}
