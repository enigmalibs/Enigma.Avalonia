using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Enigma.Avalonia.Desktop.Controls;

/// <summary>
/// A full-screen overlay control that can display arbitrary content on top of the application window.
/// Visibility is controlled via <see cref="IsOpen"/>.
/// </summary>
public class Overlay : ContentControl
{
    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Overlay, bool>(nameof(IsOpen), false);

    /// <summary>Defines the <see cref="OverlayBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<Overlay, IBrush?>(
            nameof(OverlayBrush),
            new SolidColorBrush(Color.FromArgb(77, 0, 0, 0)));

    /// <summary>Gets or sets a value indicating whether the overlay is currently visible.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets the brush used to tint the background behind the overlay content.</summary>
    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }
}
