using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Enigma.Avalonia.Mobile.UnitTests.Themes;

/// <summary>
/// Reads the canonical palette — <c>src/Enigma.Avalonia.Desktop/Themes/Colors.axaml</c> and
/// <c>Brushes.axaml</c> — as text, so a test asserts the mobile assembly against what the source
/// declares rather than against a list hand-copied into the test.
/// </summary>
/// <remarks>
/// The two files are embedded into this assembly by the test project's <c>EmbeddedResource</c> item.
/// The mobile library compiles the very same files as linked resources; reading them back out of it is
/// impossible, because the Avalonia XAML compiler replaces every <c>.axaml</c> with compiled IL.
/// </remarks>
internal static class PaletteSource
{
    /// <summary>The manifest-name prefix the test project assigns to the palette sources.</summary>
    private const string Prefix = "PaletteSource/";

    /// <summary>The Avalonia default XAML namespace.</summary>
    private static readonly XNamespace Avalonia = "https://github.com/avaloniaui";

    /// <summary>The XAML directive namespace, which carries <c>x:Key</c>.</summary>
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>Matches a <c>{DynamicResource Key}</c> or <c>{StaticResource Key}</c> markup extension.</summary>
    private static readonly Regex ResourceReference =
        new(@"\{(?:Dynamic|Static)Resource\s+(?<key>[A-Za-z0-9_]+)\s*\}", RegexOptions.Compiled);

    /// <summary>Gets the keys defined in <c>Brushes.axaml</c>, in declaration order.</summary>
    public static IReadOnlyList<string> BrushKeys { get; } = Document("Brushes.axaml")
        .Root!
        .Elements()
        .Select(element => (string?)element.Attribute(Xaml + "Key"))
        .Where(key => key is not null)
        .Select(key => key!)
        .ToArray();

    /// <summary>Gets each brush key in <c>Brushes.axaml</c> paired with the colour key its <c>Color</c> binds to.</summary>
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

    /// <summary>Parses an embedded palette file.</summary>
    /// <param name="file">The file name — <c>Colors.axaml</c> or <c>Brushes.axaml</c>.</param>
    /// <returns>The parsed document.</returns>
    private static XDocument Document(string file)
    {
        var assembly = typeof(PaletteSource).Assembly;
        using var stream = assembly.GetManifestResourceStream(Prefix + file)
            ?? throw new InvalidOperationException(
                $"'{file}' is not embedded. Embedded: {string.Join(", ", assembly.GetManifestResourceNames())}");
        using StreamReader reader = new(stream);
        return XDocument.Parse(reader.ReadToEnd());
    }
}
