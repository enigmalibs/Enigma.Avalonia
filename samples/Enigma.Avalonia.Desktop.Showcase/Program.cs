using System;
using Avalonia;

namespace Enigma.Avalonia.Desktop.Showcase;

/// <summary>The showcase application's entry point.</summary>
public static class Program
{
    /// <summary>Starts Avalonia's classic desktop lifetime.</summary>
    /// <param name="args">The command-line arguments, passed through to Avalonia.</param>
    /// <remarks>
    /// Synchronous and <c>[STAThread]</c> on purpose: Avalonia's lifetime runs the app, and the
    /// generic host that FEATURE-57C8 adds only supplies services. <c>await host.RunAsync()</c> on
    /// its own never shows a window.
    /// </remarks>
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>Builds the Avalonia application.</summary>
    /// <returns>The configured builder.</returns>
    /// <remarks>
    /// Public because the XAML previewer calls it by convention. <c>.WithDeveloperTools()</c> comes
    /// from <c>AvaloniaUI.DiagnosticsSupport</c> and replaces Avalonia 11's window-level
    /// <c>this.AttachDevTools()</c>; there is no <c>Avalonia.Diagnostics</c> release for Avalonia 12.
    /// </remarks>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
