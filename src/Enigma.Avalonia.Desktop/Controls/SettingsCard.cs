using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Enigma.Avalonia.Desktop.Controls;

/// <summary>
/// A settings card that either hosts arbitrary content on the right (when Content is set)
/// or acts as a clickable card with a chevron and Command support (when Content is null).
/// </summary>
public class SettingsCard : TemplatedControl
{
    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SettingsCard, string?>(nameof(Header));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<SettingsCard, string?>(nameof(Description));

    /// <summary>Defines the <see cref="IconData"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<SettingsCard, Geometry?>(nameof(IconData));

    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<SettingsCard, object?>(nameof(Content));

    /// <summary>Defines the <see cref="Command"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SettingsCard, ICommand?>(nameof(Command));

    /// <summary>Defines the <see cref="CommandParameter"/> property.</summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<SettingsCard, object?>(nameof(CommandParameter));

    /// <summary>Gets or sets the title text displayed in the card header.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets the subtitle text displayed beneath the header.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Gets or sets the icon geometry displayed in the card.</summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>
    /// Gets or sets the content control on the right side of the card.
    /// When set, the card is not clickable; when <see langword="null"/>, the card acts as a button.
    /// </summary>
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Gets or sets the command executed when the card is clicked (only when <see cref="Content"/> is <see langword="null"/>).</summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="Command"/> when it is executed.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Toggles the <c>:hasContent</c> pseudo-class when <see cref="Content"/> changes.</summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
        {
            if (change.NewValue is not null)
                PseudoClasses.Add(":hasContent");
            else
                PseudoClasses.Remove(":hasContent");
        }
    }


    /// <summary>Handles the pointer pressed event. Executes the command if applicable and updates the visual state.</summary>
    /// <param name="e">The event data for the pointer press.</param>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (Content is not null)
            return;

        PseudoClasses.Add(":pressed");

        if (Command is { } command && command.CanExecute(CommandParameter))
        {
            command.Execute(CommandParameter);
            e.Handled = true;
        }
    }

    /// <summary>Removes the <c>:pressed</c> pseudo-class when the pointer is released, if the card is acting as a button.</summary>
    /// <param name="e">The pointer released event data.</param>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (Content is null)
            PseudoClasses.Remove(":pressed");
    }

    /// <summary>Removes the <c>:pressed</c> pseudo-class when pointer capture is lost, if the card is acting as a button.</summary>
    /// <param name="e">The pointer capture lost event data.</param>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if (Content is null)
            PseudoClasses.Remove(":pressed");
    }
}
