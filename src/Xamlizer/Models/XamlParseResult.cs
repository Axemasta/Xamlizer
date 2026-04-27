using System.Collections.Generic;

namespace Xamlizer.Models;

/// <summary>
/// Holds the parsed contents of a single XAML file.
/// </summary>
internal sealed class XamlParseResult(string fileName, List<XamlEntry> entries)
{
    /// <summary>File name without extension (used as the generated class name).</summary>
    public string FileName { get; } = fileName;

    /// <summary>All resource entries in document order.</summary>
    public IReadOnlyList<XamlEntry> Entries { get; } = entries;
}
