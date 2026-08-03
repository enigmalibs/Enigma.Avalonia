namespace Enigma.Avalonia.Desktop.Showcase;

/// <summary>Settings for the showcase application, bound from the <c>Showcase</c> section of
/// <c>appsettings.json</c>.</summary>
/// <remarks>
/// The showcase's demonstration of <c>IConfiguration</c> + <c>IOptions&lt;T&gt;</c>: the section is
/// bound once at startup and consumed through <c>IOptions&lt;ShowcaseOptions&gt;</c>, never by
/// reading the configuration root from a ViewModel. The defaults below are what the app runs on if
/// <c>appsettings.json</c> is missing entirely.
/// </remarks>
public sealed class ShowcaseOptions
{
    /// <summary>The configuration section these options bind to.</summary>
    public const string SectionName = "Showcase";

    /// <summary>The main window's title.</summary>
    public string Title { get; set; } = "Enigma.Avalonia.Desktop — showcase";

    /// <summary>
    /// The key of the page to open at startup. Unknown keys fall back to the first navigation item,
    /// so a typo here costs a wrong landing page rather than an empty shell.
    /// </summary>
    public string InitialPage { get; set; } = "home";
}
