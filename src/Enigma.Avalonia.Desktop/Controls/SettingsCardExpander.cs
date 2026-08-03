using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Enigma.Avalonia.Desktop.Controls;

/// <summary>
/// An expandable settings card that reveals its <see cref="Content"/> when the user clicks the header.
/// Applies the <c>:expanded</c> pseudo-class while expanded.
/// </summary>
public class SettingsCardExpander : TemplatedControl
{
    /// <summary>The <c>PART_Header</c> border used to detect clicks on the card header.</summary>
    private Border? _headerBorder;

    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SettingsCardExpander, string?>(nameof(Header));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<SettingsCardExpander, string?>(nameof(Description));

    /// <summary>Defines the <see cref="IconData"/> property.</summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<SettingsCardExpander, Geometry?>(nameof(IconData));

    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<SettingsCardExpander, object?>(nameof(Content));

    /// <summary>Defines the <see cref="IsExpanded"/> property.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<SettingsCardExpander, bool>(nameof(IsExpanded));

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

    /// <summary>Gets or sets the icon geometry displayed in the card header.</summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>Gets or sets the content revealed when the card is expanded.</summary>
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the content area is currently expanded.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Updates the <c>:expanded</c> pseudo-class when <see cref="IsExpanded"/> changes.</summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsExpandedProperty)
        {
            if (change.GetNewValue<bool>())
                PseudoClasses.Add(":expanded");
            else
                PseudoClasses.Remove(":expanded");
        }
    }

    /// <summary>
    /// Finds the <c>PART_Header</c> template part and wires pointer events to handle expand/collapse.
    /// </summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_headerBorder is not null)
        {
            _headerBorder.PointerPressed -= OnHeaderPointerPressed;
            _headerBorder.PointerReleased -= OnHeaderPointerReleased;
            _headerBorder.PointerCaptureLost -= OnHeaderPointerCaptureLost;
        }

        _headerBorder = e.NameScope.Find<Border>("PART_Header");

        if (_headerBorder is not null)
        {
            _headerBorder.PointerPressed += OnHeaderPointerPressed;
            _headerBorder.PointerReleased += OnHeaderPointerReleased;
            _headerBorder.PointerCaptureLost += OnHeaderPointerCaptureLost;
        }
    }

    /// <summary>Applies the <c>:pressed</c> pseudo-class and toggles <see cref="IsExpanded"/> when the header is pressed.</summary>
    private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PseudoClasses.Add(":pressed");
        IsExpanded = !IsExpanded;
        e.Handled = true;
    }

    /// <summary>Removes the <c>:pressed</c> pseudo-class when the pointer is released over the header.</summary>
    private void OnHeaderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        PseudoClasses.Remove(":pressed");
    }

    /// <summary>Removes the <c>:pressed</c> pseudo-class when pointer capture is lost from the header.</summary>
    private void OnHeaderPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        PseudoClasses.Remove(":pressed");
    }
}
