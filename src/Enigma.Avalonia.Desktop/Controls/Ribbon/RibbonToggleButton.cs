using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Enigma.Avalonia.Desktop.Controls.Ribbon;

/// <summary>
/// A ribbon button that toggles between checked and unchecked states and optionally executes a command.
/// Applies the <c>:checked</c> pseudo-class when <see cref="IsChecked"/> is <see langword="true"/>.
/// </summary>
public class RibbonToggleButton : TemplatedControl
{
    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<RibbonToggleButton, string?>(nameof(Header));

    /// <summary>Defines the <see cref="IconData"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<RibbonToggleButton, Geometry?>(nameof(IconData));

    /// <summary>Defines the <see cref="IsChecked"/> property.</summary>
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<RibbonToggleButton, bool>(
            nameof(IsChecked),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="Command"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<RibbonToggleButton, ICommand?>(nameof(Command));

    /// <summary>Defines the <see cref="CommandParameter"/> property.</summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<RibbonToggleButton, object?>(nameof(CommandParameter));

    /// <summary>Gets or sets the text label displayed beneath the button icon.</summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets the icon geometry displayed on the button.</summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the button is in the checked state.</summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>Gets or sets the command executed when the button is toggled.</summary>
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

    /// <summary>Updates the <c>:checked</c> pseudo-class when <see cref="IsChecked"/> changes.</summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsCheckedProperty)
        {
            if (change.GetNewValue<bool>())
                PseudoClasses.Add(":checked");
            else
                PseudoClasses.Remove(":checked");
        }
    }

    /// <summary>Applies the <c>:pressed</c> pseudo-class, toggles <see cref="IsChecked"/>, and executes <see cref="Command"/> on pointer press.</summary>
    /// <param name="e">The pointer pressed event data.</param>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Add(":pressed");

        IsChecked = !IsChecked;

        if (Command is { } command && command.CanExecute(CommandParameter))
        {
            command.Execute(CommandParameter);
        }

        e.Handled = true;
    }

    /// <summary>Removes the <c>:pressed</c> pseudo-class when the pointer is released.</summary>
    /// <param name="e">The pointer released event data.</param>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Remove(":pressed");
    }

    /// <summary>Removes the <c>:pressed</c> pseudo-class when pointer capture is lost.</summary>
    /// <param name="e">The pointer capture lost event data.</param>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        PseudoClasses.Remove(":pressed");
    }
}
