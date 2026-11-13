using System.Globalization;
using System.Xml;

namespace ToolsCore.Iniss2.Xml;

/// <summary>
/// Polohy prvkov v texte suboru XML - podla nich sa subor upravuje bez zmeny ostatnych znakov.
/// </summary>
internal static class XmlSpanTree
{
    /// <summary>
    /// Zisti polohy vsetkych prvkov textu.
    /// </summary>
    /// <param name="text">obsah suboru</param>
    /// <returns>korenovy prvok</returns>
    /// <exception cref="XmlException">text nie je XML</exception>
    /// <exception cref="InvalidOperationException">polohy z citaca nesedia s textom</exception>
    public static XmlSpanNode Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var lines = LineStarts(text);
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreComments = false, IgnoreWhitespace = false };
        using var reader = XmlReader.Create(new StringReader(text), settings);
        var info = (IXmlLineInfo)reader;

        var stack = new Stack<XmlSpanNode>();
        XmlSpanNode? root = null;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = Offset(lines, info);
                var start = name - 1;
                Expect(text, start, "<");

                var tagEnd = TagEnd(text, name);
                var empty = reader.IsEmptyElement;
                var node = new XmlSpanNode { Start = start, StartTagEnd = tagEnd, IsEmpty = empty };
                if (empty)
                {
                    node.ContentEnd = tagEnd;
                    node.End = tagEnd;
                }

                if (stack.Count > 0)
                    stack.Peek().Children.Add(node);
                else
                    root = node;

                if (!empty)
                    stack.Push(node);
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                var name = Offset(lines, info);
                Expect(text, name - 2, "</");

                var node = stack.Pop();
                node.ContentEnd = name - 2;
                node.End = TagEnd(text, name);
            }
        }

        return root ?? throw new InvalidOperationException("Text has no root element.");
    }

    // zaciatky riadkov ako ich pocita citac XML: CRLF, CR aj LF su jeden koniec riadku
    private static List<int> LineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                starts.Add(i + 1);
            }
            else if (text[i] == '\n')
            {
                starts.Add(i + 1);
            }
        }

        return starts;
    }

    private static int Offset(List<int> lines, IXmlLineInfo info) => lines[info.LineNumber - 1] + info.LinePosition - 1;

    private static void Expect(string text, int index, string value)
    {
        if (index < 0 || string.CompareOrdinal(text, index, value, 0, value.Length) != 0)
            throw new InvalidOperationException(string.Create(CultureInfo.InvariantCulture, $"Unexpected element position {index}."));
    }

    // index za znakom > znacky, ktorej nazov zacina na indexe from; > v hodnote atributu znacku nekonci
    private static int TagEnd(string text, int from)
    {
        var quote = '\0';
        for (var i = from; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                if (c == quote)
                    quote = '\0';
            }
            else if (c is '"' or '\'')
            {
                quote = c;
            }
            else if (c == '>')
            {
                return i + 1;
            }
        }

        throw new InvalidOperationException("Unterminated tag.");
    }
}
