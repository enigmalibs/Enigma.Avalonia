using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Enigma.Avalonia.Desktop.UnitTests.Themes;

/// <summary>
/// Reads the library's theme XAML as text, so a test can assert on what the templates actually
/// reference rather than on a list hand-copied into the test.
/// </summary>
/// <remarks>
/// The sources are embedded into this assembly by the test project's <c>EmbeddedResource</c> glob
/// over <c>src/Enigma.Avalonia.Desktop/Themes/**</c>. They cannot be read back out of the library
/// assembly: the Avalonia XAML compiler replaces every <c>AvaloniaResource</c> <c>.axaml</c> with
/// compiled IL, and <c>AssetLoader.Open</c> on one throws <see cref="FileNotFoundException"/>.
/// </remarks>
internal static class ThemeSource
{
    /// <summary>The manifest-name prefix the test project's glob assigns to the theme sources.</summary>
    private const string Prefix = "ThemeSource/";

    /// <summary>The Avalonia default XAML namespace.</summary>
    private static readonly XNamespace Avalonia = "https://github.com/avaloniaui";

    /// <summary>The XAML directive namespace, which carries <c>x:Key</c>.</summary>
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>Matches a <c>{DynamicResource Key}</c> or <c>{StaticResource Key}</c> markup extension.</summary>
    private static readonly Regex ResourceReference =
        new(@"\{(?:Dynamic|Static)Resource\s+(?<key>[A-Za-z0-9_]+)\s*\}", RegexOptions.Compiled);

    /// <summary>The assembly the theme sources are embedded in.</summary>
    private static readonly Assembly Assembly = typeof(ThemeSource).Assembly;

    /// <summary>
    /// Gets every embedded theme file, as a path relative to <c>Themes/</c> — e.g.
    /// <c>Fluent.axaml</c>, <c>Controls/Ribbon/Ribbon.axaml</c>.
    /// </summary>
    public static IReadOnlyList<string> AllFiles { get; } = Assembly.GetManifestResourceNames()
        .Where(name => name.StartsWith(Prefix, StringComparison.Ordinal))
        .Select(name => name[Prefix.Length..].Replace('\\', '/'))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();

    /// <summary>Gets the paths <c>Fluent.axaml</c> merges, in merge order.</summary>
    public static IReadOnlyList<string> MergedFiles { get; } = Document("Fluent.axaml")
        .Descendants(Avalonia + "ResourceInclude")
        .Select(include => (string?)include.Attribute("Source"))
        .Where(source => source is not null)
        .Select(source => source![(source!.IndexOf("/Themes/", StringComparison.Ordinal) + "/Themes/".Length)..])
        .ToArray();

    /// <summary>Gets the merged control-template dictionaries — everything under <c>Controls/</c>.</summary>
    public static IReadOnlyList<string> TemplateFiles { get; } = MergedFiles
        .Where(path => path.StartsWith("Controls/", StringComparison.Ordinal))
        .ToArray();

    /// <summary>
    /// Gets every distinct resource key referenced by a control template, sorted.
    /// </summary>
    public static IReadOnlyList<string> KeysReferencedByTemplates { get; } = TemplateFiles
        .SelectMany(ReferencedKeys)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(key => key, StringComparer.Ordinal)
        .ToArray();

    /// <summary>Gets the keys defined in <c>Brushes.axaml</c>.</summary>
    public static IReadOnlyList<string> BrushKeys { get; } = Document("Brushes.axaml")
        .Root!
        .Elements()
        .Select(element => (string?)element.Attribute(Xaml + "Key"))
        .Where(key => key is not null)
        .Select(key => key!)
        .ToArray();

    /// <summary>
    /// Gets each brush key in <c>Brushes.axaml</c> paired with the colour key its <c>Color</c> binds
    /// to — the indirection that makes runtime theme switching work.
    /// </summary>
    public static IReadOnlyDictionary<string, string> BrushColorBindings { get; } = Document("Brushes.axaml")
        .Root!
        .Elements()
        .Where(element => element.Attribute(Xaml + "Key") is not null && element.Attribute("Color") is not null)
        .ToDictionary(
            element => (string)element.Attribute(Xaml + "Key")!,
            element => ResourceReference.Match((string)element.Attribute("Color")!).Groups["key"].Value,
            StringComparer.Ordinal);

    /// <summary>Gets the literal colour values one theme variant declares, keyed by colour key.</summary>
    /// <param name="variant">The variant's <c>x:Key</c> — <c>Dark</c> or <c>Light</c>.</param>
    /// <returns>The colour key to literal value map.</returns>
    public static IReadOnlyDictionary<string, string> ColorValues(string variant) => Document("Colors.axaml")
        .Descendants(Avalonia + "ResourceDictionary")
        .Single(dictionary => (string?)dictionary.Attribute(Xaml + "Key") == variant)
        .Elements()
        .ToDictionary(
            element => (string)element.Attribute(Xaml + "Key")!,
            element => element.Value.Trim(),
            StringComparer.Ordinal);

    /// <summary>Reads an embedded theme file as text.</summary>
    /// <param name="path">The path relative to <c>Themes/</c>.</param>
    /// <returns>The file's contents.</returns>
    public static string Read(string path)
    {
        using var stream = Assembly.GetManifestResourceStream(Prefix + path)
            ?? throw new InvalidOperationException(
                $"'{path}' is not embedded. Embedded: {string.Join(", ", AllFiles)}");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>Parses an embedded theme file.</summary>
    /// <param name="path">The path relative to <c>Themes/</c>.</param>
    /// <returns>The parsed document.</returns>
    public static XDocument Document(string path) => XDocument.Parse(Read(path));

    /// <summary>Gets the distinct resource keys a single theme file references.</summary>
    /// <param name="path">The path relative to <c>Themes/</c>.</param>
    /// <returns>The referenced keys.</returns>
    public static IReadOnlyList<string> ReferencedKeys(string path) => ResourceReference
        .Matches(Read(path))
        .Select(match => match.Groups["key"].Value)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(key => key, StringComparer.Ordinal)
        .ToArray();

    /// <summary>Gets the colour keys <c>Colors.axaml</c> defines for one theme variant.</summary>
    /// <param name="variant">The variant's <c>x:Key</c> — <c>Dark</c> or <c>Light</c>.</param>
    /// <returns>The colour keys, in declaration order.</returns>
    public static IReadOnlyList<string> ColorKeys(string variant) => Document("Colors.axaml")
        .Descendants(Avalonia + "ResourceDictionary")
        .Single(dictionary => (string?)dictionary.Attribute(Xaml + "Key") == variant)
        .Elements()
        .Select(element => (string)element.Attribute(Xaml + "Key")!)
        .ToArray();

    /// <summary>Gets the <c>ControlTheme</c> target types declared across the control templates.</summary>
    /// <returns>One entry per <c>ControlTheme</c>, as the unqualified target type name.</returns>
    public static IReadOnlyList<string> ControlThemeTargets() => TemplateFiles
        .SelectMany(path => Document(path).Descendants(Avalonia + "ControlTheme"))
        .Select(theme => (string)theme.Attribute("TargetType")!)
        .Select(target => target[(target.IndexOf(':') + 1)..])
        .OrderBy(target => target, StringComparer.Ordinal)
        .ToArray();
}
