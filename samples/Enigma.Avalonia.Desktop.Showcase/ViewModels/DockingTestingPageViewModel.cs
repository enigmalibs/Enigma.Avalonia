using Avalonia.Controls;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Enigma.Avalonia.Desktop.Controls.Docking;
using Enigma.Icons.Phosphor;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the docking page: an IDE-shaped layout described entirely as a model.</summary>
/// <remarks>
/// <para>
/// The ViewModel builds a tree of <see cref="DockLayoutNode"/>s and hands the root to
/// <c>DockingHost.LayoutRoot</c>; the host turns it into <c>DockSplitContainer</c>,
/// <c>DockTabGroup</c> and <c>DockPane</c> controls. Nothing here touches a control, which is the
/// point — the initial layout is data, so it can come from a settings file just as easily.
/// </para>
/// <para>
/// The tree this builds:
/// </para>
/// <code>
/// Root — vertical split
/// ├─ Top — horizontal split
/// │  ├─ Left — horizontal split
/// │  │  ├─ [Solution]              200px, fixed pane: neither closable nor movable
/// │  │  └─ [Doc 1 | Doc 2 | Doc 3] star-sized tab group
/// │  └─ [Properties]               200px
/// └─ [Output | Debug]              200px
/// </code>
/// </remarks>
public class DockingTestingPageViewModel : ObservableObject
{
    /// <summary>Initializes a new instance of the <see cref="DockingTestingPageViewModel"/> class.</summary>
    public DockingTestingPageViewModel() => RootLayout = BuildLayout();

    /// <summary>Gets the root of the layout model the docking host builds its controls from.</summary>
    public DockLayoutNode RootLayout { get; }

    /// <summary>Builds the initial layout tree.</summary>
    /// <returns>The root split of the layout.</returns>
    private static DockLayoutNode BuildLayout()
    {
        // The Solution pane is the one that cannot be closed or dragged: with every other pane movable,
        // it shows that the two flags are per-pane rather than a host-wide switch.
        var solution = Pane(
            PhosphorIcon.TreeStructure,
            "Solution",
            "This pane sets CanClose and CanMove to false — it has no close button and refuses to drag.",
            canClose: false,
            canMove: false);

        var properties = Pane(
            PhosphorIcon.Sliders,
            "Properties",
            "CanClose is false here too, but the pane still moves: the two flags are independent.",
            canClose: false);

        var output = Pane(PhosphorIcon.Terminal, "Output", "Build output would go here.");
        var debug = Pane(PhosphorIcon.Bug, "Debug", "A debug console would go here.");

        var doc1 = Pane(PhosphorIcon.FileText, "Doc 1", "Drag this tab onto another pane's edge to split it.");
        var doc2 = Pane(PhosphorIcon.FileText, "Doc 2", "Drag it onto a pane's centre to join that tab group.");
        var doc3 = Pane(PhosphorIcon.FileText, "Doc 3", "Drag the splitters between groups to resize them.");

        var documents = Group(doc1, doc2, doc3);
        var bottom = Group(output, debug);

        var leftAndCentre = new DockSplitModel
        {
            Orientation = Orientation.Horizontal,
            First = Group(solution),
            Second = documents,
            FirstSize = new GridLength(200, GridUnitType.Pixel),
            SecondSize = new GridLength(1, GridUnitType.Star),
        };

        var top = new DockSplitModel
        {
            Orientation = Orientation.Horizontal,
            First = leftAndCentre,
            Second = Group(properties),
            FirstSize = new GridLength(1, GridUnitType.Star),
            SecondSize = new GridLength(200, GridUnitType.Pixel),
        };

        return new DockSplitModel
        {
            Orientation = Orientation.Vertical,
            First = top,
            Second = bottom,
            FirstSize = new GridLength(1, GridUnitType.Star),
            SecondSize = new GridLength(200, GridUnitType.Pixel),
        };
    }

    /// <summary>Creates a pane model carrying a <see cref="PaneContent"/>.</summary>
    /// <param name="icon">The glyph shown in the pane's content.</param>
    /// <param name="header">The pane's tab label.</param>
    /// <param name="description">The sentence shown inside the pane.</param>
    /// <param name="canClose">Whether the pane shows a close button.</param>
    /// <param name="canMove">Whether the pane can be dragged to another group.</param>
    /// <returns>The pane model.</returns>
    private static DockPaneModel Pane(
        PhosphorIcon icon,
        string header,
        string description,
        bool canClose = true,
        bool canMove = true) =>
        new()
        {
            Header = header,
            Content = new PaneContent(icon, header, description),
            CanClose = canClose,
            CanMove = canMove,
        };

    /// <summary>Creates a tab group holding the given panes, with the first one selected.</summary>
    /// <param name="panes">The panes to put in the group, in tab order.</param>
    /// <returns>The tab group model.</returns>
    private static DockTabGroupModel Group(params DockPaneModel[] panes)
    {
        var group = new DockTabGroupModel { SelectedPane = panes[0] };

        foreach (var pane in panes)
            group.Panes.Add(pane);

        return group;
    }
}
