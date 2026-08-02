using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Enigma.Icons.Phosphor;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the landing page.</summary>
/// <remarks>
/// Read-only content: the page's job is to prove the shell works end to end — a View resolved from
/// DI, a ViewModel attached by the navigation page factory, compiled bindings against a typed
/// <c>x:DataType</c>, and the library's theme brushes applied to a page the library knows nothing
/// about.
/// </remarks>
public class HomePageViewModel : ObservableObject
{
    /// <summary>The page's heading.</summary>
    public string Heading => "Enigma.Avalonia.Desktop";

    /// <summary>A one-line description of the library, shown under the heading.</summary>
    public string Tagline => "A control library for Avalonia 12 desktop applications.";

    /// <summary>The cards listing what the library provides.</summary>
    public IReadOnlyList<HomeHighlight> Highlights { get; } =
    [
        new(
            PhosphorIcon.SquaresFour,
            "A full control family set",
            "Typed editors, a navigation rail, a ribbon, dockable panes, settings cards, content "
            + "dialogs, overlays and info bars — each with a control theme that ships with the package."),
        new(
            PhosphorIcon.Sliders,
            "One dictionary, two variants",
            "Merge avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml into Application.Resources and "
            + "every control resolves the same Enigma* brushes, in Dark and in Light."),
        new(
            PhosphorIcon.Compass,
            "Services you resolve, not statics you call",
            "Navigation, dialogs, overlays, info bars and the file and folder pickers are interfaces "
            + "registered in the container and injected into ViewModels."),
        new(
            PhosphorIcon.Funnel,
            "A data layer for lists",
            "CollectionView adds live sorting, filtering and grouping over any collection, without "
            + "the control that displays it having to know about any of it."),
    ];

    /// <summary>The closing note under the highlight cards.</summary>
    public string Footnote =>
        "Pick a page from the rail on the left. Every page here is a working example — the source of "
        + "this application is the reference for wiring the library into your own.";
}
