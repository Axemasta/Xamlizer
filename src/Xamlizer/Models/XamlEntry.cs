namespace Xamlizer.Models;

/// <summary>
/// Represents a single resource entry parsed from a XAML file.
/// </summary>
internal sealed class XamlEntry
{
    public string Type { get; }
    public string Key { get; }

    public XamlEntry(string type, string key)
    {
        Type = type;
        Key = key;
    }
}
