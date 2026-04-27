using System;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Xamlizer.Models;

namespace Xamlizer;

/// <summary>
/// Roslyn incremental source generator that reads XAML resource dictionaries added as
/// AdditionalFiles and emits a typed static class containing <c>public const string</c>
/// members for every <c>x:Key</c> attribute found.
/// </summary>
[Generator]
public sealed class XamlizerGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Registers the incremental pipeline stages with the generator host.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Stage 1: Read content of every .xaml AdditionalFile.
        var xamlContents = context.AdditionalTextsProvider
            .Where(static f => f.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, ct) =>
            {
                var content = file.GetText(ct)?.ToString() ?? string.Empty;
                var fileName = Path.GetFileNameWithoutExtension(file.Path);
                return (file.Path, FileName: fileName, Content: content);
            });

        // Stage 2: Parse each XAML file into a result/error value.
        var parsed = xamlContents.Select(static (item, _) =>
        {
            if (string.IsNullOrEmpty(item.Content))
                return (item.Path, item.FileName, ParseResult: null, Error: null);

            try
            {
                var parseResult = XamlParser.Parse(item.FileName, item.Content);
                return (item.Path, item.FileName, ParseResult: (XamlParseResult?)parseResult, Error: null);
            }
            catch (XmlException ex)
            {
                return (item.Path, item.FileName, ParseResult: (XamlParseResult?)null, Error: (string?)ex.Message);
            }
        });

        // Stage 3: Get the consuming project's root namespace and project directory.
        var buildProps = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) =>
            {
                options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var ns);
                options.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir);
                return (
                    RootNamespace: string.IsNullOrWhiteSpace(ns) ? "Xamlizer" : ns!,
                    ProjectDir: projectDir ?? string.Empty
                );
            });

        // Stage 4: Generate the C# source string for each parsed file + namespace pair.
        var generated = parsed.Combine(buildProps).Select(static (pair, _) =>
        {
            var ((path, fileName, parseResult, error), props) = pair;

            if (error is not null || parseResult is null || parseResult.Entries.Count == 0)
                return (HintName: BuildHintName(path, props.ProjectDir), Source: (string?)null, Error: error, Path: path);

            var source = CSharpCodeGenerator.Generate(parseResult, props.RootNamespace);
            return (HintName: BuildHintName(path, props.ProjectDir), Source: (string?)source, Error: (string?)null, Path: path);
        });

        // Stage 5: Register source output — only I/O happens here.
        context.RegisterSourceOutput(generated, static (ctx, item) =>
        {
            if (item.Error is not null)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    XamlizerDiagnostics.InvalidXaml,
                    location: null,
                    item.Path,
                    item.Error));
                return;
            }

            if (item.Source is not null)
                ctx.AddSource(item.HintName, item.Source);
        });
    }

    private static string BuildHintName(string fullPath, string projectDir)
    {
        string relativePath;
        if (!string.IsNullOrEmpty(projectDir))
        {
            relativePath = MakeRelativePath(projectDir, fullPath);
        }
        else
        {
            relativePath = Path.GetFileName(fullPath);
        }

        // Strip the .xaml extension.
        var withoutExtension = Path.ChangeExtension(relativePath, null);

        // Replace path separators and any character that is not a letter, digit, or underscore with _.
        var sb = new StringBuilder(withoutExtension.Length);
        foreach (var c in withoutExtension)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
            else
                sb.Append('_');
        }

        return $"{sb}Keys.g.cs";
    }

    private static string MakeRelativePath(string projectDir, string fullPath)
    {
        // Normalise the base so it always ends with a separator, then strip it.
        var baseDir = projectDir.TrimEnd('/', '\\') + Path.DirectorySeparatorChar;

        if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            return fullPath.Substring(baseDir.Length);

        // Also try the alt separator variant in case paths mix separators.
        var altBase = projectDir.TrimEnd('/', '\\') + Path.AltDirectorySeparatorChar;
        if (fullPath.StartsWith(altBase, StringComparison.OrdinalIgnoreCase))
            return fullPath.Substring(altBase.Length);

        return Path.GetFileName(fullPath);
    }
}

