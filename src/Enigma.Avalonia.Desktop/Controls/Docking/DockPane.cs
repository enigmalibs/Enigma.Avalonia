using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Enigma.Avalonia.Desktop.Controls.Docking;

/// <summary>
/// Represents a single dockable content pane with a header, content, and optional close/move capabilities.
/// </summary>
public class DockPane : TemplatedControl
{
    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<DockPane, string?>(nameof(Header));

    /// <summary>Defines the <see cref="PaneContent"/> property.</summary>
    public static readonly StyledProperty<object?> PaneContentProperty =
        AvaloniaProperty.Register<DockPane, object?>(nameof(PaneContent));

    /// <summary>Defines the <see cref="CanClose"/> property.</summary>
    public static readonly StyledProperty<bool> CanCloseProperty =
        AvaloniaProperty.Register<DockPane, bool>(nameof(CanClose), true);

    /// <summary>Defines the <see cref="CanMove"/> property.</summary>
    public static readonly StyledProperty<bool> CanMoveProperty =
        AvaloniaProperty.Register<DockPane, bool>(nameof(CanMove), true);

    /// <summary>Gets or sets the header text displayed in the pane's tab.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets the content displayed inside the pane.</summary>
    [Content]
    public object? PaneContent
    {
        get => GetValue(PaneContentProperty);
        set => SetValue(PaneContentProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the pane can be closed by the user.</summary>
    public bool CanClose
    {
        get => GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the pane can be moved via drag-and-drop.</summary>
    public bool CanMove
    {
        get => GetValue(CanMoveProperty);
        set => SetValue(CanMoveProperty, value);
    }
}
