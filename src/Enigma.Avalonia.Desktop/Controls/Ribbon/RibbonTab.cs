using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Enigma.Avalonia.Desktop.Controls.Ribbon;

/// <summary>
/// A tab within a <see cref="Ribbon"/> that contains a collection of <see cref="RibbonGroup"/> instances.
/// </summary>
public class RibbonTab : TemplatedControl
{
    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<RibbonTab, string?>(nameof(Header));

    /// <summary>Gets the collection of groups displayed in this tab.</summary>
    [Content]
    public AvaloniaList<RibbonGroup> Groups { get; } = new();

    /// <summary>Gets or sets the text displayed on the tab header.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
