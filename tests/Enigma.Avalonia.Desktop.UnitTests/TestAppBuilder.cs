using Avalonia;
using Avalonia.Headless;
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
        => AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
