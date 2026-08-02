using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Enigma.Avalonia.Desktop.Controls.Docking;

/// <summary>
/// Base class for docking layout model nodes
/// </summary>
public abstract class DockLayoutNode
{
}

/// <summary>
/// Model representing a single dock pane
/// </summary>
public class DockPaneModel : DockLayoutNode
{
    /// <summary>Gets or sets the header text for the pane's tab.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>Gets or sets the content displayed inside the pane.</summary>
    public object? Content { get; set; }

    /// <summary>Gets or sets a value indicating whether the pane can be closed by the user.</summary>
    public bool CanClose { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the pane can be moved via drag-and-drop.</summary>
    public bool CanMove { get; set; } = true;
}

/// <summary>
/// Model representing a group of tabbed panes
/// </summary>
public class DockTabGroupModel : DockLayoutNode
{
    /// <summary>Gets the ordered collection of pane models in this tab group.</summary>
    public AvaloniaList<DockPaneModel> Panes { get; } = new();

    /// <summary>Gets or sets the pane model that should be initially selected.</summary>
    public DockPaneModel? SelectedPane { get; set; }
}

/// <summary>
/// Model representing a split container with two children
/// </summary>
public class DockSplitModel : DockLayoutNode
{
    /// <summary>Gets or sets the axis along which <see cref="First"/> and <see cref="Second"/> are arranged.</summary>
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>Gets or sets the first (left or top) child layout node.</summary>
    public DockLayoutNode? First { get; set; }

    /// <summary>Gets or sets the second (right or bottom) child layout node.</summary>
    public DockLayoutNode? Second { get; set; }

    /// <summary>Gets or sets the initial grid size allocated to the first child.</summary>
    public GridLength FirstSize { get; set; } = new GridLength(1, GridUnitType.Star);

    /// <summary>Gets or sets the initial grid size allocated to the second child.</summary>
    public GridLength SecondSize { get; set; } = new GridLength(1, GridUnitType.Star);
}
