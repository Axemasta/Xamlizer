using System;
using System.IO;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xamlizer;

[Generator]
public sealed class XamlizerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect all .xaml AdditionalFiles and their text content.
        var xamlContents = context.AdditionalTextsProvider
            .Where(static f => f.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, ct) =>
            {
                var content = file.GetText(ct)?.ToString() ?? string.Empty;
                var fileName = Path.GetFileNameWithoutExtension(file.Path);
                return (Path: file.Path, FileName: fileName, Content: content);
            });

        // Get the consuming project's root namespace.
        var rootNamespace = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) =>
            {
                options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var ns);
                return string.IsNullOrWhiteSpace(ns) ? "Xamlizer" : ns!;
            });

        var combined = xamlContents.Combine(rootNamespace);

        context.RegisterSourceOutput(combined, static (ctx, pair) =>
        {
            var ((path, fileName, content), ns) = pair;

            if (string.IsNullOrEmpty(content))
                return;

            try
            {
                var parseResult = XamlParser.Parse(fileName, content);

                if (parseResult.Entries.Count == 0)
                    return;

                var source = CSharpCodeGenerator.Generate(parseResult, ns);
                var hintName = BuildHintName(path, fileName);
                ctx.AddSource(hintName, source);
            }
            catch (XmlException ex)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    XamlizerDiagnostics.InvalidXaml,
                    location: null,
                    path,
                    ex.Message));
            }
        });
    }

    private static string BuildHintName(string fullPath, string fileName)
    {
        // Include a stable numeric hash of the full path to avoid hint-name collisions
        // when two XAML files in different folders share the same base name.
        var pathHash = ComputeStableHash(fullPath);
        var sanitized = CSharpCodeGenerator.SanitizeIdentifier(fileName);
        return $"{sanitized}_{pathHash:X8}.g.cs";
    }

    private static uint ComputeStableHash(string value)
    {
        // FNV-1a 32-bit: deterministic, not subject to runtime hash randomisation.
        unchecked
        {
            uint hash = 2166136261u;
            foreach (char c in value)
            {
                hash ^= c;
                hash *= 16777619u;
            }
            return hash;
        }
    }
}
