using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;
using Enigma.Avalonia.Mobile.UnitTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Enigma.Avalonia.Mobile.UnitTests;

/// <summary>
/// The headless Avalonia application every test in this assembly runs inside.
/// </summary>
/// <remarks>
/// The fixture is required even by tests that never touch a control: loading an
/// <c>avares://</c> dictionary needs the platform services.
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
/// The application the headless session runs: FluentTheme plus the mobile library's theme
/// dictionary, merged in <c>TestApp.axaml</c> exactly the way a mobile app is documented to merge
/// it — see that file for why it has to be XAML.
/// </summary>
public partial class TestApp : Application
{
    /// <summary>The mobile library's single public XAML entry point, as a consumer addresses it.</summary>
    public static readonly Uri ThemeUri = new("avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml");

    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
