using System.Xml;
using System.Xml.Serialization;
using ToolsCore.Tools;

namespace ToolsCore.XML;

public static class XmlSerialization
{
    /// <summary>
    ///     Nacita data z XML suboru.
    /// </summary>
    /// <param name="fileName">Cesta k suboru.</param>
    /// <returns></returns>
    public static T ReadData<T>(string fileName) where T : new()
    {
        var config = default(T);
        try
        {
            var text = File.ReadAllText(fileName, Encodings.Win1250);
            if (!string.IsNullOrEmpty(text))
                config = (T) Deserialize(text, typeof(T))!;
        }
        catch (FileNotFoundException)
        {
            //ignored, config will be null
        }

        if (config != null) 
            return config;

        //config je null (nevedel precitat subor - napr. neexistuje)
        config = new T();
        WriteData(fileName, config);
        return config;
    }

    /// <summary>
    ///     Zapise data do XML suboru.
    /// </summary>
    /// <param name="file">Cesta k suboru</param>
    /// <param name="obj">Data.</param>
    public static void WriteData<T>(string file, T obj) => SerializeToFile(file, obj!);

    private static string Serialize(object obj, bool indent, XmlSerializer? serializer = null)
    {
        serializer ??= new XmlSerializer(obj.GetType());

        var stringWriter = new StringWriter();
        var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
        {
            Indent = indent,
            OmitXmlDeclaration = true
        });

        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");

        serializer.Serialize(xmlWriter, obj, ns);
        var text = stringWriter.ToString();

        return text.Trim(" \r\n".ToCharArray());
    }

    internal static void SerializeToFile(string fileName, object obj, Encoding? encoding = null, XmlSerializer? serializer = null)
    {
        var text = Serialize(obj, true, serializer);

        encoding ??= Encodings.Win1250;
        text = $"<?xml version=\"1.0\" encoding=\"{encoding.WebName}\"?>\r\n{text}";

        File.WriteAllText(fileName, text, encoding);
    }

    internal static object? Deserialize(string text, Type type)
    {
        if (string.IsNullOrEmpty(text)) 
            throw new ArgumentException("XML text is empty.");

        var xmlSerializer = new XmlSerializer(type);
        var input = new StringReader(text);
        var xmlReader = new XmlTextReader(input);

        return xmlSerializer.Deserialize(xmlReader);
    }
}