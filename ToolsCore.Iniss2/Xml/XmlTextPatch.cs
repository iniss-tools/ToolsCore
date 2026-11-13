using System.Text;
using System.Xml;
using System.Xml.Linq;
using ToolsCore.Iniss.Tools;

namespace ToolsCore.Iniss2.Xml;

/// <summary>
/// Uprava suboru XML na mieste: zmeni len prvky, ktorych obsah sa zmenil, ostatne znaky suboru (komentare, odsadenie, zapis
/// atributov, entity) ostanu rovnake.
/// </summary>
/// <remarks>
/// Porovnavaju sa dva stromy zostavene tym istym zapisovacom: <c>older</c> z doterajsieho obsahu suboru a <c>desired</c>
/// z novych dat. Rovnake prvky sa v texte nemenia. Zmeneny prvok s rovnakym nazvom sa upravi do hlbky, nove prvky sa vlozia
/// s odsadenim susedov a zmazane zmiznu aj s riadkom. Atributy a prvky, ktore zapisovac nepozna, ostanu.
/// </remarks>
internal sealed class XmlTextPatch
{
    private const string IndentStep = "  ";

    // znaky mimo ASCII, ktore Windows-1250 vie zapisat; ostatne sa zapisu ako odkaz na znak
    private static readonly HashSet<char> Win1250Chars = BuildWin1250Chars();

    private readonly string _text;
    private readonly string _newline;
    private readonly List<Edit> _edits = [];

    private XmlTextPatch(string text)
    {
        _text = text;
        _newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
    }

    /// <summary>
    /// Novy obsah suboru.
    /// </summary>
    /// <param name="text">doterajsi obsah suboru</param>
    /// <param name="older">doterajsi obsah, ako ho vidi zapisovac (zapisovac nad precitanymi datami)</param>
    /// <param name="desired">novy obsah z toho isteho zapisovaca</param>
    /// <exception cref="InvalidOperationException">prvky suboru nezodpovedaju prvkom <paramref name="older" /></exception>
    public static string Apply(string text, XElement older, XElement desired)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(older);
        ArgumentNullException.ThrowIfNull(desired);

        var target = XDocument.Parse(text).Root!;
        var span = XmlSpanTree.Parse(text);
        if (target.Name != older.Name || older.Name != desired.Name)
            throw new InvalidOperationException("Root element differs.");

        var patch = new XmlTextPatch(text);
        patch.Merge(target, span, older, desired);
        return patch.Result();
    }

    private string Result()
    {
        // od konca, aby sa indexy neposuvali; pri rovnakom zaciatku najprv vymazanie, potom vlozenie
        var builder = new StringBuilder(_text);
        foreach (var edit in _edits.OrderByDescending(edit => edit.Start).ThenByDescending(edit => edit.End - edit.Start))
            builder.Remove(edit.Start, edit.End - edit.Start).Insert(edit.Start, edit.Text);

        return builder.ToString();
    }

    private void Merge(XElement target, XmlSpanNode span, XElement older, XElement desired)
    {
        if (Key(older) == Key(desired))
            return;

        var attributesChanged = !SameAttributes(older, desired);
        var olderChildren = older.Elements().ToList();
        var desiredChildren = desired.Elements().ToList();

        if (olderChildren.Count == 0 && desiredChildren.Count == 0)
        {
            MergeLeaf(target, span, older, desired, attributesChanged);
            return;
        }

        if (span.IsEmpty)
        {
            ReplaceElement(target, span, older, desired);
            return;
        }

        if (attributesChanged)
            _edits.Add(new Edit(span.Start, span.StartTagEnd, StartTag(target, older, desired, empty: false)));

        MergeChildren(target, span, olderChildren, desiredChildren);
    }

    private void MergeLeaf(XElement target, XmlSpanNode span, XElement older, XElement desired, bool attributesChanged)
    {
        var valueChanged = older.Value != desired.Value;

        // prazdny prvok dostava obsah alebo prvok s vnorenymi prvkami, ktore zapisovac nepozna, sa prepise cely
        if ((valueChanged && span.IsEmpty) || target.HasElements)
        {
            ReplaceElement(target, span, older, desired);
            return;
        }

        if (valueChanged)
            _edits.Add(new Edit(span.ContentStart, span.ContentEnd, Fragment(writer => writer.WriteString(desired.Value))));

        if (attributesChanged)
            _edits.Add(new Edit(span.Start, span.StartTagEnd, StartTag(target, older, desired, span.IsEmpty)));
    }

    private void MergeChildren(XElement target, XmlSpanNode span, List<XElement> older, List<XElement> desired)
    {
        var known = older.Concat(desired).Select(element => element.Name).ToHashSet();
        var children = target.Elements().Zip(span.Children).Where(pair => known.Contains(pair.First.Name)).ToList();
        if (children.Count != older.Count || children.Where((pair, i) => pair.First.Name != older[i].Name).Any())
            throw new InvalidOperationException("Elements of the file do not match the writer.");

        var indent = span.Children.Count > 0 ? LineIndent(span.Children[0].Start) : LineIndent(span.Start) + IndentStep;
        var steps = SequenceDiff.Compute(older.Select(Key).ToList(), desired.Select(Key).ToList());

        var anchor = span.ContentStart;
        var deletes = new List<int>();
        var inserts = new List<int>();

        void Hunk()
        {
            var pending = new StringBuilder();
            for (var k = 0; k < Math.Max(deletes.Count, inserts.Count); k++)
            {
                if (k < deletes.Count && k < inserts.Count && older[deletes[k]].Name == desired[inserts[k]].Name)
                {
                    Insert(anchor, pending);
                    var (element, node) = children[deletes[k]];
                    Merge(element, node, older[deletes[k]], desired[inserts[k]]);
                    anchor = node.End;
                    continue;
                }

                if (k < inserts.Count)
                    pending.Append(_newline).Append(indent).Append(Serialize(desired[inserts[k]], indent));
                if (k < deletes.Count)
                    Delete(children[deletes[k]].Second);
            }

            Insert(anchor, pending);
            deletes.Clear();
            inserts.Clear();
        }

        foreach (var step in steps)
        {
            switch (step.Kind)
            {
                case SequenceDiff.Kind.Delete:
                    deletes.Add(step.Old);
                    break;
                case SequenceDiff.Kind.Insert:
                    inserts.Add(step.New);
                    break;
                default:
                    Hunk();
                    anchor = children[step.Old].Second.End;
                    break;
            }
        }

        Hunk();
    }

    private void Insert(int at, StringBuilder text)
    {
        if (text.Length == 0)
            return;

        _edits.Add(new Edit(at, at, text.ToString()));
        text.Clear();
    }

    // prvok zmizne aj so zaciatkom riadku, ak pred nim na riadku nic nie je
    private void Delete(XmlSpanNode node)
    {
        var start = node.Start;
        var lineStart = start;
        while (lineStart > 0 && _text[lineStart - 1] is ' ' or '\t')
            lineStart--;

        if (lineStart > 0 && _text[lineStart - 1] == '\n')
            start = lineStart >= 2 && _text[lineStart - 2] == '\r' ? lineStart - 2 : lineStart - 1;
        else if (lineStart > 0 && _text[lineStart - 1] == '\r')
            start = lineStart - 1;

        _edits.Add(new Edit(start, node.End, ""));
    }

    private void ReplaceElement(XElement target, XmlSpanNode span, XElement older, XElement desired)
    {
        var element = new XElement(desired.Name, desired.Attributes().Concat(Unknown(target, older, desired)), desired.Nodes());
        _edits.Add(new Edit(span.Start, span.End, Serialize(element, LineIndent(span.Start))));
    }

    private string StartTag(XElement target, XElement older, XElement desired, bool empty)
    {
        var tag = Fragment(writer => new XElement(desired.Name, desired.Attributes().Concat(Unknown(target, older, desired))).WriteTo(writer));
        return empty ? tag : tag[..^2].TrimEnd() + ">";
    }

    // atributy suboru, ktore zapisovac nepozna
    private static IEnumerable<XAttribute> Unknown(XElement target, XElement older, XElement desired)
    {
        var known = older.Attributes().Concat(desired.Attributes()).Select(attribute => attribute.Name).ToHashSet();
        return target.Attributes().Where(attribute => !known.Contains(attribute.Name)).Select(attribute => new XAttribute(attribute));
    }

    private string LineIndent(int index)
    {
        var start = index;
        while (start > 0 && _text[start - 1] is ' ' or '\t')
            start--;

        return start == 0 || _text[start - 1] is '\r' or '\n' ? _text[start..index] : "";
    }

    private string Serialize(XElement element, string indent)
    {
        var text = Fragment(element.WriteTo, true);
        return indent.Length == 0 ? text : text.Replace(_newline, _newline + indent, StringComparison.Ordinal);
    }

    private string Fragment(Action<XmlWriter> write, bool indent = false)
    {
        var builder = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = indent,
            IndentChars = IndentStep,
            NewLineChars = _newline
        };

        using (var writer = XmlWriter.Create(builder, settings))
            write(writer);

        return EscapeForCodePage(builder.ToString());
    }

    private static string EscapeForCodePage(string text)
    {
        if (text.All(c => c < 0x80 || Win1250Chars.Contains(c)))
            return text;

        var builder = new StringBuilder(text.Length + 16);
        foreach (var c in text)
        {
            if (c < 0x80 || Win1250Chars.Contains(c))
                builder.Append(c);
            else
                builder.Append("&#x").Append(((int)c).ToString("X", System.Globalization.CultureInfo.InvariantCulture)).Append(';');
        }

        return builder.ToString();
    }

    private static HashSet<char> BuildWin1250Chars()
    {
        var bytes = new byte[128];
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)(0x80 + i);

        return [.. Encodings.Win1250.GetString(bytes)];
    }

    private static string Key(XElement element) => element.ToString(SaveOptions.DisableFormatting);

    private static bool SameAttributes(XElement older, XElement desired)
    {
        var first = older.Attributes().ToDictionary(attribute => attribute.Name, attribute => attribute.Value);
        var second = desired.Attributes().ToList();
        return first.Count == second.Count && second.All(attribute => first.TryGetValue(attribute.Name, out var value) && value == attribute.Value);
    }

    /// <summary>Nahradenie useku textu.</summary>
    /// <param name="Start">zaciatok useku</param>
    /// <param name="End">koniec useku</param>
    /// <param name="Text">novy text</param>
    private readonly record struct Edit(int Start, int End, string Text);
}
