using System.Xml;
using Xamlizer;
using Xamlizer.Models;
using Xunit;

namespace Xamlizer.Tests;

public class XamlParserTests
{
    // --- Colors.xaml integration ---

    [Fact]
    public void Parse_ColorsXaml_ReturnsCorrectFileName()
    {
        var result = ParseColors();
        Assert.Equal("Colors", result.FileName);
    }

    [Fact]
    public void Parse_ColorsXaml_ReturnsTwoDistinctTypes()
    {
        var result = ParseColors();
        var types = GetDistinctTypes(result);
        Assert.Equal(2, types.Count);
    }

    [Fact]
    public void Parse_ColorsXaml_TypesAreColorAndSolidColorBrush()
    {
        var result = ParseColors();
        var types = GetDistinctTypes(result);
        Assert.Contains("Color", types);
        Assert.Contains("SolidColorBrush", types);
    }

    [Fact]
    public void Parse_ColorsXaml_ColorTypeAppearsFirst()
    {
        var result = ParseColors();
        var types = GetDistinctTypesOrdered(result);
        Assert.Equal("Color", types[0]);
    }

    [Fact]
    public void Parse_ColorsXaml_ColorHas22Entries()
    {
        var result = ParseColors();
        var colorEntries = GetEntriesForType(result, "Color");
        Assert.Equal(22, colorEntries.Count);
    }

    [Fact]
    public void Parse_ColorsXaml_SolidColorBrushHas13Entries()
    {
        var result = ParseColors();
        var brushEntries = GetEntriesForType(result, "SolidColorBrush");
        Assert.Equal(13, brushEntries.Count);
    }

    [Fact]
    public void Parse_ColorsXaml_ColorEntriesAreInDocumentOrder()
    {
        var result = ParseColors();
        var colorEntries = GetEntriesForType(result, "Color");

        Assert.Equal("Primary", colorEntries[0].Key);
        Assert.Equal("Secondary", colorEntries[1].Key);
        Assert.Equal("Tertiary", colorEntries[2].Key);
        Assert.Equal("White", colorEntries[3].Key);
        Assert.Equal("Black", colorEntries[4].Key);
        Assert.Equal("Gray100", colorEntries[5].Key);
        Assert.Equal("Gray950", colorEntries[12].Key);
        Assert.Equal("Yellow100Accent", colorEntries[13].Key);
        Assert.Equal("Blue300Accent", colorEntries[21].Key);
    }

    [Fact]
    public void Parse_ColorsXaml_SolidColorBrushEntriesAreInDocumentOrder()
    {
        var result = ParseColors();
        var brushEntries = GetEntriesForType(result, "SolidColorBrush");

        Assert.Equal("PrimaryBrush", brushEntries[0].Key);
        Assert.Equal("SecondaryBrush", brushEntries[1].Key);
        Assert.Equal("Gray950Brush", brushEntries[12].Key);
    }

    // --- Identifier edge cases ---

    [Fact]
    public void Parse_XamlWithNoKeys_ReturnsEmptyEntries()
    {
        const string xaml = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                                xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <Color>Red</Color>
            </ResourceDictionary>
            """;

        var result = XamlParser.Parse("Test", xaml);
        Assert.Empty(result.Entries);
    }

    [Fact]
    public void Parse_XamlWithSingleEntry_ReturnsSingleEntry()
    {
        const string xaml = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                                xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <Color x:Key="Primary">#FECA51</Color>
            </ResourceDictionary>
            """;

        var result = XamlParser.Parse("Test", xaml);
        Assert.Single(result.Entries);
        Assert.Equal("Color", result.Entries[0].Type);
        Assert.Equal("Primary", result.Entries[0].Key);
    }

    [Fact]
    public void Parse_MalformedXml_ThrowsXmlException()
    {
        Assert.Throws<XmlException>(() => XamlParser.Parse("Bad", "<unclosed"));
    }

    [Fact]
    public void Parse_RootElementWithNoKey_IsNotIncluded()
    {
        const string xaml = """
            <?xml version="1.0" encoding="UTF-8" ?>
            <ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                                xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
                <Color x:Key="A">Red</Color>
            </ResourceDictionary>
            """;

        var result = XamlParser.Parse("Test", xaml);
        Assert.DoesNotContain(result.Entries, e => e.Type == "ResourceDictionary");
    }

    // --- Helpers ---

    private static XamlParseResult ParseColors()
        => XamlParser.Parse("Colors", TestResources.GetColorsXaml());

    private static HashSet<string> GetDistinctTypes(XamlParseResult result)
    {
        var set = new HashSet<string>();
        foreach (var e in result.Entries)
            set.Add(e.Type);
        return set;
    }

    private static List<string> GetDistinctTypesOrdered(XamlParseResult result)
    {
        var seen = new HashSet<string>();
        var list = new List<string>();
        foreach (var e in result.Entries)
        {
            if (seen.Add(e.Type))
                list.Add(e.Type);
        }
        return list;
    }

    private static List<XamlEntry> GetEntriesForType(XamlParseResult result, string type)
    {
        var entries = new List<XamlEntry>();
        foreach (var e in result.Entries)
        {
            if (e.Type == type)
                entries.Add(e);
        }
        return entries;
    }
}
