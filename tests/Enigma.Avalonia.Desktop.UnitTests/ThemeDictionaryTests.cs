using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests;

/// <summary>
/// The solution's smoke test: proves the three projects build together and that the library's one
/// public XAML entry point is reachable from a consumer.
/// </summary>
public sealed class ThemeDictionaryTests
{
    private static readonly Uri ThemeUri = new("avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml");

    /// <summary>
    /// The theme dictionary resolves and loads from the library assembly at its documented URI.
    /// </summary>
    /// <remarks>
    /// This covers more than it looks: the URI is the library's published contract (plan §2.5), so
    /// the test fails if the assembly name changes, if the <c>AvaloniaResource</c> glob stops
    /// picking the file up, or if the dictionary stops being XAML-compilable.
    /// </remarks>
    [AvaloniaFact]
    public void FluentTheme_LoadsFromTheLibraryAssembly()
    {
        ResourceInclude include = new(ThemeUri) { Source = ThemeUri };

        IResourceDictionary loaded = include.Loaded;

        Assert.NotNull(loaded);
    }
}
