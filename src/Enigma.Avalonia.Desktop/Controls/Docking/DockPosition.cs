namespace Enigma.Avalonia.Desktop.Controls.Docking;

/// <summary>
/// Specifies the target drop zone when docking a pane relative to another group.
/// </summary>
public enum DockPosition
{
    /// <summary>Drop onto the center of the target group, adding the pane as a new tab.</summary>
    Center,

    /// <summary>Dock to the left of the target group, splitting it horizontally.</summary>
    Left,

    /// <summary>Dock to the right of the target group, splitting it horizontally.</summary>
    Right,

    /// <summary>Dock above the target group, splitting it vertically.</summary>
    Top,

    /// <summary>Dock below the target group, splitting it vertically.</summary>
    Bottom
}
