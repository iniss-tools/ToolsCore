using ToolsCore.Tools;

namespace ToolsCore.StateDgm;

/// <summary>
///     Chyba syntaxe suboru StateDgm.txt (INISS by nacitanie diagramu prerusil).
/// </summary>
public sealed class StateDgmParseException(string message, int line) : Exception(message)
{
    /// <summary>Cislo riadka (0-based), na ktorom sa chyba nasla.</summary>
    public int Line { get; } = line;
}

/// <summary>
///     Vysledok nacitania suboru StateDgm.txt do stromu.
/// </summary>
public sealed class StateDgmTextFile
{
    /// <summary>Cislo verzie zapisu z prveho riadka (INISS 3.39 prijme len <c>0001</c>).</summary>
    public string Version { get; set; } = StateDgmReader.VERSION;

    /// <summary>Hlavickove komentare <c>C:"…"</c> - text za <c>C:</c> po koniec riadka, bez uvodzoviek.</summary>
    public List<string> HeaderComments { get; } = [];

    /// <summary>Korenova skupina; jej podskupiny su bloky <c>P:</c> (bezne jediny <c>StateDgmCtrls</c>).</summary>
    public StateDgmGroup Root { get; } = new("");
}

/// <summary>
///     Citac suboru StateDgm.txt verny INISSu 3.39:
///     <list type="bullet">
///         <item>prvy token musi byt verzia <c>0001</c>, zvysok riadka sa ignoruje;</item>
///         <item><c>;</c> a <c>C:</c> su komentare po koniec riadka (aj neukonceny retazec za <c>C:</c> je v poriadku);</item>
///         <item>za pismenom typu musi nasledovat <c>:</c>; typy <c>P</c>, <c>G</c> (skupina, cesta cez <c>\</c> alebo <c>/</c>),
///         <c>S</c>, <c>I</c>, <c>B</c>; <c>A</c> INISS pozna, ale v grafikonoch sa nevyskytuje a citac ho odmietne;</item>
///         <item>retazce v uvodzovkach s C-escapes (<c>\n \t \r \a \b \f \v \xHH \\ \"</c>), mozu presahovat cez riadok, max. 1024 znakov;</item>
///         <item>cislo: znaky <c>+-0-9xA-Fa-f</c> a strtol so zakladom 0 (<c>0x08</c>, <c>010</c> = 8);</item>
///         <item>pravdivostna hodnota: <c>Ano</c>/<c>Yes</c> = ano, <c>Ne</c>/<c>No</c> = nie (rozlisuju sa velke pismena);</item>
///         <item>hodnota <c>#</c> namiesto textu/cisla kluc odstrani;</item>
///         <item>opakovane meno skupiny vytvori dalsiu skupinu (nie zlucenie) - preto funguje <c>G:"Event"</c> bez cisla;
///         medzilanky cesty <c>P:"A\\B\\C"</c> sa hladaju a vytvoria len ked chybaju.</item>
///     </list>
/// </summary>
public static class StateDgmReader
{
    /// <summary>Jedina verzia zapisu, ktoru INISS 3.39 prijme.</summary>
    public const string VERSION = "0001";

    private const int MAX_STRING = 0x400;

    /// <summary>
    ///     Nacita text suboru do stromu. Pri chybe syntaxe vyhodi <see cref="StateDgmParseException" />.
    /// </summary>
    public static StateDgmTextFile Read(string text)
    {
        var r = new Cursor(text);
        var file = new StateDgmTextFile();

        r.SkipWhite();
        var version = r.ReadToken(4);
        if (version != VERSION)
            throw new StateDgmParseException($"Neznáma verzia zápisu „{version}“ – očakáva sa {VERSION} na prvom riadku", r.Line);
        file.Version = version;
        r.SkipLine();

        ReadBody(r, file, file.Root, false);
        return file;
    }

    /// <summary>
    ///     Nacita subor v kodovani INISSu (windows-1250).
    /// </summary>
    public static StateDgmTextFile ReadFile(string path) => Read(File.ReadAllText(path, Encodings.Win1250));

    private static void ReadBody(Cursor r, StateDgmTextFile file, StateDgmGroup group, bool nested)
    {
        while (true)
        {
            r.SkipWhite();
            var c = r.Read();
            if (c == -1)
            {
                if (nested) throw new StateDgmParseException("Neočakávaný koniec súboru – chýba }", r.Line);
                return;
            }

            if (c == ';')
            {
                r.SkipLine();
                continue;
            }

            if (c == '}')
            {
                if (nested) return;
                throw new StateDgmParseException("Neočakávané } mimo skupiny", r.Line);
            }

            var line = r.Line;
            var colon = r.Read();
            if (colon != ':')
                throw new StateDgmParseException($"Za „{(char)c}“ sa očakáva :", line);

            switch (c)
            {
                case 'C':
                {
                    var rest = r.ReadLine().Trim();
                    if (!nested) file.HeaderComments.Add(StripCommentQuotes(rest));
                    break;
                }
                case 'P':
                case 'G':
                {
                    r.SkipWhite();
                    var name = r.ReadQuoted();
                    if (string.IsNullOrEmpty(name))
                        throw new StateDgmParseException("Skupina bez mena", line);
                    var sub = CreateGroup(group, name, line);
                    r.SkipWhite();
                    var brace = r.Read();
                    if (brace != '{')
                        throw new StateDgmParseException($"Za skupinou „{name}“ sa očakáva {{", r.Line);
                    ReadBody(r, file, sub, true);
                    break;
                }
                case 'S':
                {
                    var key = ReadKey(r, line);
                    if (r.Peek() == '#')
                    {
                        r.Read();
                        group.Items.Add(StateDgmValue.Removal(StateDgmValueKind.String, key));
                        break;
                    }

                    if (r.Peek() != '"')
                        throw new StateDgmParseException($"Hodnota kľúča „{key}“ musí byť reťazec v úvodzovkách", r.Line);
                    var text = r.ReadQuoted();
                    group.Items.Add(new StateDgmValue(key, text) { Line = line });
                    break;
                }
                case 'I':
                {
                    var key = ReadKey(r, line);
                    if (r.Peek() == '#')
                    {
                        r.Read();
                        group.Items.Add(StateDgmValue.Removal(StateDgmValueKind.Int, key));
                        break;
                    }

                    var raw = r.ReadWhile(ch => ch is '+' or '-' or 'x' or (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f'));
                    if (raw.Length == 0)
                        throw new StateDgmParseException($"Kľúč „{key}“ nemá číselnú hodnotu", r.Line);
                    if (!TryStrtol(raw, out var number))
                        throw new StateDgmParseException($"„{raw}“ nie je číslo (kľúč „{key}“)", r.Line);
                    // desiatkovy zapis sa pri zapise normalizuje, sestnastkovy a osmickovy sa zachova
                    var digits = raw.TrimStart('+', '-');
                    var keepRaw = digits.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || (digits.Length > 1 && digits[0] == '0');
                    group.Items.Add(new StateDgmValue(key, number, keepRaw ? raw : null) { Line = line });
                    break;
                }
                case 'B':
                {
                    var key = ReadKey(r, line);
                    if (r.Peek() == '#')
                    {
                        r.Read();
                        group.Items.Add(StateDgmValue.Removal(StateDgmValueKind.Bool, key));
                        break;
                    }

                    // INISS cita po prvy biely znak - "Ano}" na jednom riadku je chyba aj u neho
                    var word = r.ReadWhile(ch => ch is not (' ' or '\t' or '\r' or '\n'));
                    var flag = word switch
                    {
                        "Ano" or "Yes" => true,
                        "Ne" or "No" => false,
                        "" => throw new StateDgmParseException($"Kľúč „{key}“ nemá hodnotu Ano/Ne", r.Line),
                        _ => throw new StateDgmParseException($"„{word}“ nie je Ano ani Ne (kľúč „{key}“)", r.Line)
                    };
                    group.Items.Add(new StateDgmValue(key, flag) { Line = line });
                    break;
                }
                case 'A':
                    throw new StateDgmParseException("Typ A: (binárne dáta) sa v StateDgm.txt nepoužíva a editor ho nepodporuje", line);
                default:
                    throw new StateDgmParseException($"Neznámy typ položky „{(char)c}:“", line);
            }
        }
    }

    /// <summary><c>strtol(text, &amp;end, 0)</c> so znamienkom; cely text musi byt cislo.</summary>
    public static bool TryStrtol(string text, out int value)
    {
        value = 0;
        var neg = false;
        var i = 0;
        if (text.Length > 0 && text[0] is '+' or '-')
        {
            neg = text[0] == '-';
            i = 1;
        }

        if (!Expressions.ExprLexer.TryStrtol(text[i..], out var n)) return false;
        value = neg ? -n : n;
        return true;
    }

    /// <summary>Precita <c>"kluc"</c>, medzery a <c>=</c>; vrati kluc.</summary>
    private static string ReadKey(Cursor r, int line)
    {
        r.SkipWhite();
        var key = r.ReadQuoted();
        if (string.IsNullOrEmpty(key))
            throw new StateDgmParseException("Položka bez kľúča", line);
        r.SkipWhite();
        if (r.Read() != '=')
            throw new StateDgmParseException($"Za kľúčom „{key}“ sa očakáva =", r.Line);
        r.SkipWhite();
        return key;
    }

    /// <summary>
    ///     Vytvori skupinu podla cesty (<c>A\B\C</c>): medzilanky sa pouziju, ak uz existuju, posledny clanok
    ///     sa vzdy vytvori novy (ako INISS).
    /// </summary>
    private static StateDgmGroup CreateGroup(StateDgmGroup parent, string path, int line)
    {
        var parts = path.Split(['\\', '/'], StringSplitOptions.None);
        var cur = parent;
        for (var i = 0; i < parts.Length - 1; i++)
            cur = cur.Group(parts[i]) ?? cur.AddGroup(parts[i]);
        var g = cur.AddGroup(parts[^1]);
        g.Line = line;
        return g;
    }

    /// <summary><c>"text"</c> alebo <c>"text</c> (neukonceny) → <c>text</c>.</summary>
    private static string StripCommentQuotes(string s)
    {
        if (s.Length > 0 && s[0] == '"') s = s[1..];
        if (s.Length > 0 && s[^1] == '"') s = s[..^1];
        return s;
    }

    /// <summary>
    ///     Pohyb po texte s pocitanim riadkov.
    /// </summary>
    private sealed class Cursor(string text)
    {
        private int _pos;

        public int Line { get; private set; }

        public int Peek() => _pos < text.Length ? text[_pos] : -1;

        public int Read()
        {
            if (_pos >= text.Length) return -1;
            var c = text[_pos++];
            if (c == '\n') Line++;
            return c;
        }

        public void SkipWhite()
        {
            while (_pos < text.Length && text[_pos] is ' ' or '\t' or '\r' or '\n') Read();
        }

        public void SkipLine()
        {
            while (_pos < text.Length && text[_pos] != '\n') _pos++;
            if (_pos < text.Length) Read();
        }

        public string ReadLine()
        {
            var start = _pos;
            while (_pos < text.Length && text[_pos] != '\n') _pos++;
            var s = text[start.._pos].TrimEnd('\r');
            if (_pos < text.Length) Read();
            return s;
        }

        public string ReadToken(int max)
        {
            var sb = new StringBuilder();
            while (_pos < text.Length && sb.Length < max && !char.IsWhiteSpace(text[_pos])) sb.Append((char)Read());
            return sb.ToString();
        }

        public string ReadWhile(Func<char, bool> pred)
        {
            var sb = new StringBuilder();
            while (_pos < text.Length && pred(text[_pos])) sb.Append((char)Read());
            return sb.ToString();
        }

        /// <summary>Retazec v uvodzovkach s C-escapes. Bez uvodnej uvodzovky vrati prazdny text.</summary>
        public string ReadQuoted()
        {
            if (Peek() != '"') return "";
            var startLine = Line;
            Read();
            var sb = new StringBuilder();
            while (true)
            {
                var c = Read();
                if (c == -1) throw new StateDgmParseException("Neukončený reťazec", startLine);
                if (c == '"') break;
                if (sb.Length >= MAX_STRING) throw new StateDgmParseException($"Reťazec je dlhší než {MAX_STRING} znakov", startLine);
                if (c == '\\')
                {
                    var e = Read();
                    switch (e)
                    {
                        case -1: throw new StateDgmParseException("Neukončený reťazec", startLine);
                        case 'n': c = '\n'; break;
                        case 'r': c = '\r'; break;
                        case 't': c = '\t'; break;
                        case 'a': c = '\a'; break;
                        case 'b': c = '\b'; break;
                        case 'f': c = '\f'; break;
                        case 'v': c = '\v'; break;
                        case 'x':
                        case 'X':
                        {
                            var h1 = HexDigit(Peek());
                            if (h1 < 0)
                            {
                                c = e; // "\x" bez cislice INISS zapise ako pismeno x
                                break;
                            }

                            Read();
                            var h2 = HexDigit(Peek());
                            if (h2 >= 0)
                            {
                                Read();
                                c = (h1 << 4) | h2;
                            }
                            else
                            {
                                c = h1;
                            }

                            break;
                        }
                        default: c = e; break;
                    }
                }

                sb.Append((char)c);
            }

            return sb.ToString();
        }

        private static int HexDigit(int c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => c - 'a' + 10,
            >= 'A' and <= 'F' => c - 'A' + 10,
            _ => -1
        };
    }
}
