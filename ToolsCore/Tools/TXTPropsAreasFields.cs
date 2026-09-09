namespace ToolsCore.Tools;

/// <summary>
///     Trieda reprezentujuca zoznam vlastnosti zorganyzovanych v poliach.
///     Kazde pole moze obsahovat este sekciu s komentarom.
/// </summary>
public class TxtPropsAreasFields
{
    private const string EX_MESSAGE = "Súbor {0}: V oblasti {1} sa očakávala vlastnosť {2}.";

    private readonly string _fileName;
    private readonly Dictionary<string, Dictionary<string, string>> _areas;
    private readonly Dictionary<string, string> _areasComments;

    /// <summary>
    ///     Vytvori novu instanciu triedy <see cref="TxtPropsAreasFields"/>.
    /// </summary>
    /// <param name="file">Cesta k suboru do/z ktore sa budu ukladat/nacitat subory.</param>
    /// <param name="write">Ak je false, zoznam vlastnosti a hodnot sa nacita zo suboru.</param>
    public TxtPropsAreasFields(string file, bool write = false)
    {
        _fileName = file;
        _areas = new Dictionary<string, Dictionary<string, string>>();
        _areasComments = new Dictionary<string, string>();

        if (!write) 
            LoadFromFile(file);
    }

    /// <summary>
    ///     Vrati hodnotu vlastnosti s nazvom <paramref name="field"/>, ktora sa ma nachadzat v poli <paramref name="area"/>,
    ///     alebo <paramref name="defValue"/> ak zadany nazov vlastnosti <paramref name="field"/> nebol najdeny v slovniku vlastnosti pre dane pole,
    ///     pricom sa nevyvola ziadna vynimka.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <param name="field">Nazov vlastnosti.</param>
    /// <param name="defValue">Predvolena hodnota.</param>
    /// <returns>hodnotu vlastnosti, alebo <paramref name="defValue"/> ak zadany nazov vlastnosti nebol najdeny v slovniku vlastnosti.</returns>
    public string Get(string area, string field, string defValue) => Get(area, field, false) ?? defValue;

    /// <summary>
    ///     Vrati hodnotu vlastnosti s nazvom <paramref name="field"/> v poli <paramref name="area"/>. Vyvola vynimku,
    ///     ak zadany nazov vlastnosti nebol najdeny.
    /// </summary>
    public string Get(string area, string field) => Get(area, field, true)!;

    public string? Get(string area, string field, bool throwIfNull = true)
    {
        if (throwIfNull)
            if (_areas.ContainsKey(area) && _areas[area].ContainsKey(field))
                return _areas[area][field];
            else
                throw new ArgumentNullException(nameof(area), string.Format(EX_MESSAGE, _fileName, area, field));

        return _areas.ContainsKey(area) && _areas[area].ContainsKey(field) ? _areas[area][field] : null;
    }

    /// <summary>
    ///     Vrati slovnik vsetkych vlastnosti s ich hodnotami pola s nazvom <paramref name="area"/>.
    ///     Ak pole s tymto nazvom nenajde vrati <see langword="null"/>.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <returns>slovnik vlastnosti alebo <see langword="null"/>, ak pole s nazov <paramref name="area"/> v sloniku poli nenajde.</returns>
    public Dictionary<string, string>? Get(string area) => _areas.ContainsKey(area) ? _areas[area] : null;

    /// <summary>
    ///     Vrati cely slovnik s poliami a ich vlastnostami.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Dictionary<string, string>> GetAll() => _areas;

    /// <summary>
    ///     Vrati zoznam poli.
    /// </summary>
    /// <returns></returns>
    public List<string> GetAreas() => _areas.Keys.ToList();

    /// <summary>
    ///     Nastavi hodnotu <paramref name="value"/> vlastnosti s nazvom <paramref name="field"/> do pola s nazvom <paramref name="area"/>.
    ///     Ak zadany nazov pola alebo vlastnosti nenajde,
    ///     vytvori nove pole resp. vlastnost s tymto nazvom a nastavi jej hodnotu specifikovanu v parametri <paramref name="value"/>.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <param name="field">Nazov vlastnosti.</param>
    /// <param name="value">Hodnota vlastnosti.</param>
    /// <param name="type">Urcuje, ako sa ma metoda spravat k hodnote vlastnosti <paramref name="value"/>.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Set(string area, string field, object? value, WriteType type = WriteType.WriteNumber)
    {
        if (type != WriteType.WriteNumber)
        {
            if (value == null)
            {
                if (type is WriteType.WriteStringANSI or WriteType.WriteStringUTF8)
                    throw new ArgumentNullException(nameof(area), string.Format(EX_MESSAGE, _fileName, area, field));

                value = "".Quote();
            }
            else
            {
                var vals = value as string;

                value = type is WriteType.WriteStringANSI or WriteType.WriteStringANSINullable ?
                    vals!.Quote().UTFtoANSI() :
                    vals!.Quote();
            }
        }

        if (_areas.ContainsKey(area))
        {
            if (!_areas[area].ContainsKey(field))
                _areas[area].Add(field, value?.ToString() ?? "");
            else
                _areas[area][field] = value?.ToString() ?? "";
        }
        else
        {
            var keys = new Dictionary<string, string> { { field, value?.ToString() ?? "" } };
            _areas.Add(area, keys);
        }
    }

    /// <summary>
    ///     Nastavi zoznam vlastnosti pre pole s nazvom area.
    ///     Ak pole s tymto nazvom nenajde, vytvori nove pole s tymto nazvom a nastavi jej tieto vlastnosti.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <param name="fields">Slovnik vlastnisti a ich hodnot.</param>
    public void Set(string area, Dictionary<string, object> fields)
    {
        foreach (var field in fields.Keys)
            if (_areas.ContainsKey(area))
            {
                if (!_areas[area].ContainsKey(field))
                    _areas[area].Add(field, fields[field].ToString() ?? "");
                else
                    _areas[area][field] = fields[field].ToString() ?? "";
            }
            else
            {
                var keys = new Dictionary<string, string>
                {
                    { field, fields[field].ToString() ?? "" }
                };
                _areas.Add(area, keys);
            }
    }

    /// <summary>
    ///     Vrati komentar, ktory sa ma nachadzat v poli s nazvom <paramref name="area"/>.
    ///     Ak pole s tymto nazvom nenajde, vrati <see cref="string.Empty"/>.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <returns>text komentaru, alebo <see cref="string.Empty"/> ak pole s nazvom <paramref name="area"/> nenajde v slovniku poli.</returns>
    public string GetComment(string area) => _areasComments.ContainsKey(area) ? _areasComments[area] : "";

    /// <summary>
    ///     Nastavi komentar pre dane pole. Ak zadany nazov pola <paramref name="area"/> nenajde v slovniku poli,
    ///     vytvori novu pole s tymto nazvom a nastavi mu zadany komentar.
    /// </summary>
    /// <param name="area">Nazov pola.</param>
    /// <param name="comment">Komentár.</param>
    public void SetComment(string area, string comment)
    {
        if (_areasComments.ContainsKey(area))
            _areasComments[area] = comment;
        else
            _areasComments.Add(area, comment);
    }

    /// <summary>
    ///     Ulozi zoznam poli s vlastnostami do suboru.
    /// </summary>
    public void Save()
    {
        var file = new StreamWriter(_fileName, false, Encodings.Win1250);

        foreach (var area in _areas.Keys)
        {
            file.WriteLine("[" + area + "]");
            if (_areasComments.ContainsKey(area))
            {
                var lines = _areasComments[area].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for (var i = 0; i < lines.Length; i++)
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        lines[i] = lines[i].Insert(0, ";");
                        file.WriteLine(lines[i]);
                    }
            }

            foreach (var field in _areas[area].Keys.Where(field => !string.IsNullOrWhiteSpace(_areas[area][field])))
                file.WriteLine(field + "=" + _areas[area][field]);


            file.WriteLine(Environment.NewLine);
        }

        file.Close();
    }

    /// <summary>
    ///     Nacita zoznam poli s vlastnostami zo suboru.
    /// </summary>
    private void LoadFromFile(string file)
    {
        var comments = new StringBuilder();
        var actualArea = "";
        var propInArea = new Dictionary<string, string>();

        foreach (var line in File.ReadAllLines(file, Encodings.Win1250))
        {
            if (!string.IsNullOrEmpty(line) && !line.StartsWithAny(';','#', '\'') && line.StartsWith("[") && line.EndsWith("]"))
            {
                if (propInArea.Count != 0 || !string.IsNullOrEmpty(actualArea))
                {
                    _areas.Add(actualArea, propInArea);
                    _areasComments.Add(actualArea, comments.ToString());
                }

                propInArea = new Dictionary<string, string>();
                var s = line.Replace("[", "");
                actualArea = s.Replace("]", "");
                comments = new StringBuilder();
            }
            else if (!string.IsNullOrEmpty(line) && line.StartsWith(";"))
            {
                comments.AppendLine(line.Substring(1));
            }

            if (!string.IsNullOrEmpty(line) &&
                !line.StartsWith(";") && 
                !line.StartsWith("#") &&
                !line.StartsWith("'") && 
                line.Contains('='))
            {
                var index = line.IndexOf('=');
                var key = line.Substring(0, index).Trim();
                var value = line.Substring(index + 1).Trim();

                if (value.StartsWith("\"") && value.EndsWith("\"") ||
                    value.StartsWith("'") && value.EndsWith("'"))
                    value = value.Substring(1, value.Length - 2);

                try
                {
                    propInArea.Add(key, value);
                }
                catch
                {
                     /*ignored*/
                }
            }
        }

        _areas.Add(actualArea, propInArea);
        _areasComments.Add(actualArea, comments.ToString());
    }
}

/// <summary>
///     Typ zapisu prvku do slovnika.
/// </summary>
public enum WriteType
{
    /// <summary>
    ///     Zapis cisla (hodnota bez uvodzoviek).
    /// </summary>
    WriteNumber,

    /// <summary>
    ///     Zapis retazca UTF8 (v pripade ze je prvok <see langword="null" />, vyhodi vynimku).
    /// </summary>
    WriteStringUTF8,

    /// <summary>
    ///     Zapis retazca UTF8 (v pripade ze je prvok <see langword="null" />, vlozi do slovnika "").
    /// </summary>
    WriteStringUTF8Nullable,

    /// <summary>
    ///     Zapis retazca ANSI (v pripade ze je prvok <see langword="null" />, vyhodi vynimku).
    /// </summary>
    WriteStringANSI,

    /// <summary>
    ///     Zapis retazca ANSI (v pripade ze je prvok <see langword="null" />, vlozi do slovnika "").
    /// </summary>
    WriteStringANSINullable
}