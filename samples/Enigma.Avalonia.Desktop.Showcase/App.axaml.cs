using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Enigma.Avalonia.Desktop.Showcase.Views;

namespace Enigma.Avalonia.Desktop.Showcase;

/// <summary>The Avalonia application.</summary>
/// <remarks>
/// A skeleton: it shows an empty main window and nothing else. FEATURE-57C8 PHASE01 replaces the
/// body of <see cref="OnFrameworkInitializationCompleted"/> with the generic-host wiring — the host
/// is started, never run, and the main window is resolved from the container rather than
/// constructed here.
/// </remarks>
public partial class App : Application
{
    /// <summary>Loads the application XAML.</summary>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <summary>Hands Avalonia its main window.</summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
