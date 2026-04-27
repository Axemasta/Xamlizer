using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Xamlizer.Models;

namespace Xamlizer;

/// <summary>
/// Parses a XAML file's content and extracts all resource entries that have an <c>x:Key</c> attribute.
/// </summary>
internal static class XamlParser
{
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

    /// <summary>
    /// Parses the given XAML content and returns the resource entries grouped by type.
    /// </summary>
    /// <param name="fileName">The file name without extension, used as the generated class name.</param>
    /// <param name="xmlContent">The raw XAML XML content to parse.</param>
    /// <returns>A <see cref="XamlParseResult"/> containing all entries in document order.</returns>
    /// <exception cref="XmlException">Thrown when the XML content is malformed.</exception>
    public static XamlParseResult Parse(string fileName, string xmlContent)
    {
        var entries = new List<XamlEntry>();

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
        };

        using var stringReader = new StringReader(xmlContent);
        using var reader = XmlReader.Create(stringReader, settings);

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element)
                continue;

            var typeName = reader.LocalName;
            var key = reader.GetAttribute("Key", XamlNamespace);

            if (!string.IsNullOrEmpty(key))
                entries.Add(new XamlEntry(typeName, key!));
        }

        return new XamlParseResult(fileName, entries);
    }
}
