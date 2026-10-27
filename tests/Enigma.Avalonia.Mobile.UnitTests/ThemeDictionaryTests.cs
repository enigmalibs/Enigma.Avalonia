using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Xunit;

namespace Enigma.Avalonia.Mobile.UnitTests;

/// <summary>
/// The mobile library's smoke tests: its one public XAML entry point is reachable from a consumer,
/// and it is made of the palette and nothing else.
/// </summary>
public sealed class ThemeDictionaryTests
{
    /// <summary>
    /// The theme dictionary resolves and loads from the mobile assembly at its documented URI.
    /// </summary>
    /// <remarks>
    /// The URI is the package's published contract, so this fails if the assembly name changes, if the
    /// <c>AvaloniaResource</c> glob stops picking the file up, or if the dictionary stops compiling.
    /// </remarks>
    [AvaloniaFact]
    public void FluentTheme_LoadsFromTheMobileAssembly()
    {
        ResourceInclude include = new(TestApp.ThemeUri) { Source = TestApp.ThemeUri };

        IResourceDictionary loaded = include.Loaded;

        Assert.NotNull(loaded);
    }

    /// <summary>
    /// The entry point declares no key of its own and merges exactly two dictionaries — the colours,
    /// then the brushes that resolve them. A control template merged here by mistake would show up as
    /// a third dictionary.
    /// </summary>
    [AvaloniaFact]
    public void FluentTheme_MergesTheColoursThenTheBrushesAndNothingElse()
    {
        ResourceInclude include = new(TestApp.ThemeUri) { Source = TestApp.ThemeUri };
        IResourceDictionary loaded = include.Loaded;

        Assert.Empty(loaded.Keys);
        Assert.Empty(loaded.ThemeDictionaries);
        Assert.Equal(2, loaded.MergedDictionaries.Count);

        var colours = Assert.IsAssignableFrom<IResourceProvider>(loaded.MergedDictionaries[0]);
        var brushes = Assert.IsAssignableFrom<IResourceProvider>(loaded.MergedDictionaries[1]);
        Assert.True(colours.TryGetResource("EnigmaBackgroundColor", ThemeVariant.Dark, out _));
        Assert.False(colours.TryGetResource("EnigmaBackgroundBrush", ThemeVariant.Dark, out _));
        Assert.True(brushes.TryGetResource("EnigmaBackgroundBrush", ThemeVariant.Dark, out _));
        Assert.False(brushes.TryGetResource("EnigmaBackgroundColor", ThemeVariant.Dark, out _));
    }

    /// <summary>
    /// The two palette dictionaries are addressable on their own inside the mobile assembly, at the
    /// paths the entry point merges — which is what the linked <c>AvaloniaResource</c> items promise.
    /// </summary>
    /// <param name="file">The palette file.</param>
    [AvaloniaTheory]
    [InlineData("Colors.axaml")]
    [InlineData("Brushes.axaml")]
    public void ThePaletteFiles_LoadFromTheMobileAssembly(string file)
    {
        Uri uri = new($"avares://Enigma.Avalonia.Mobile/Themes/{file}");
        ResourceInclude include = new(uri) { Source = uri };

        IResourceDictionary loaded = include.Loaded;

        Assert.True(loaded.Keys.Any() || loaded.ThemeDictionaries.Count > 0, $"'{file}' loaded empty.");
    }
}
