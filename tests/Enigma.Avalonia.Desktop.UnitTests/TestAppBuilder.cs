using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Enigma.Avalonia.Desktop.UnitTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Enigma.Avalonia.Desktop.UnitTests;

/// <summary>
/// The headless Avalonia application every test in this assembly runs inside.
/// </summary>
/// <remarks>
/// The fixture is required even by tests that never touch a control: resolving an
/// <c>avares://</c> asset or parsing a <c>Geometry</c> needs the platform services, and a test
/// method missing <c>[AvaloniaFact]</c> fails with an obscure "Unable to locate
/// IPlatformRenderInterface" rather than an assertion failure.
/// </remarks>
public static class TestAppBuilder
{
    /// <summary>Builds the headless Avalonia application.</summary>
    /// <returns>The configured builder.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<TestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

/// <summary>
/// The application the headless session runs: FluentTheme plus this library's real theme
/// dictionary, merged exactly the way a consumer is documented to merge it.
/// </summary>
/// <remarks>
/// <para>
/// Merging <c>Themes/Fluent.axaml</c> is what makes the resource-key and control-smoke tests mean
/// anything — with a stub dictionary every <c>DynamicResource</c> would fall back silently and the
/// tests would pass against a library whose templates resolve nothing.
/// </para>
/// <para>
/// <see cref="FluentTheme"/> is needed as well, not for this library's own controls (each ships a
/// full <c>ControlTheme</c>) but for the framework controls their templates host — <c>ListBox</c>,
/// <c>Button</c>, <c>Popup</c>, <c>GridSplitter</c>, <c>PathIcon</c>. Without it those have no
/// template, and "the template applied" would be a much weaker claim.
/// </para>
/// </remarks>
public sealed class TestApp : Application
{
    /// <summary>The library's single public XAML entry point, as a consumer addresses it.</summary>
    public static readonly Uri ThemeUri = new("avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml");

    /// <inheritdoc />
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Resources.MergedDictionaries.Add(new ResourceInclude(ThemeUri) { Source = ThemeUri });

        // Pinned, not left at Default: a headless session has no OS preference to follow, and the
        // theme-variant tests assert against a known starting point.
        RequestedThemeVariant = ThemeVariant.Dark;
    }
}
