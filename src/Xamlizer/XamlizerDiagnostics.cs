using Microsoft.CodeAnalysis;

namespace Xamlizer;

internal static class XamlizerDiagnostics
{
    public static readonly DiagnosticDescriptor InvalidXaml = new DiagnosticDescriptor(
        id: "XAM001",
        title: "Invalid XAML file",
        messageFormat: "Failed to parse XAML file '{0}': {1}",
        category: "Xamlizer",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
