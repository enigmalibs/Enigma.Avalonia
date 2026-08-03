using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Enigma.Avalonia.Desktop.Controls.Ribbon;

/// <summary>
/// A labeled group of ribbon controls displayed within a <see cref="RibbonTab"/>.
/// </summary>
public class RibbonGroup : TemplatedControl
{
    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<RibbonGroup, string?>(nameof(Header));

    /// <summary>Gets the collection of controls contained in this group.</summary>
    [Content]
    public AvaloniaList<Control> Items { get; } = new();

    /// <summary>Gets or sets the label displayed beneath the group's controls.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
