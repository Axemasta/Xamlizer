using System.IO;
using System.Reflection;

namespace Xamlizer.Tests;

internal static class TestResources
{
    public static string GetColorsXaml()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Xamlizer.Tests.Resources.Colors.xaml")
            ?? throw new InvalidOperationException("Embedded resource 'Colors.xaml' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
