using Avalonia;
using Avalonia.Controls;

namespace Enigma.Avalonia.Desktop.Showcase.Controls;

/// <summary>A card showing a title, a progress bar and a message, sized to sit in the middle of an
/// <c>Overlay</c>.</summary>
/// <remarks>
/// This control belongs to the showcase, not to the library — that is the point of it. It is the
/// demonstration that <c>IOverlayService</c> hosts arbitrary consumer content: the service takes any
/// <see cref="Control"/>, so a consumer's own templated control drops straight in. Its theme lives in
/// <c>Controls/ProgressOverlayCard.axaml</c> and is merged from <c>App.axaml</c>.
/// </remarks>
public class ProgressOverlayCard : ContentControl
{
    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ProgressOverlayCard, string?>(nameof(Title));

    /// <summary>Defines the <see cref="Message"/> property.</summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<ProgressOverlayCard, string?>(nameof(Message));

    /// <summary>Defines the <see cref="IsIndeterminate"/> property.</summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<ProgressOverlayCard, bool>(nameof(IsIndeterminate), true);

    /// <summary>Defines the <see cref="Progress"/> property.</summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<ProgressOverlayCard, double>(nameof(Progress));

    /// <summary>Defines the <see cref="Minimum"/> property.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<ProgressOverlayCard, double>(nameof(Minimum));

    /// <summary>Defines the <see cref="Maximum"/> property.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<ProgressOverlayCard, double>(nameof(Maximum), 100d);

    /// <summary>Gets or sets the card's heading. Hidden when null or empty.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the message below the progress bar. Hidden when null or empty.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Gets or sets whether the progress bar is indeterminate. Defaults to <c>true</c>.</summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Gets or sets the current progress value. Defaults to 0.</summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>Gets or sets the lowest progress value. Defaults to 0.</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Gets or sets the highest progress value. Defaults to 100.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
}
