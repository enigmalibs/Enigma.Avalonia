using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Enigma.Avalonia.Desktop.Controls.Navigation;

/// <summary>
/// Represents an item in a navigation control.
/// </summary>
public class NavigationItem : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Header"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<NavigationItem, string?>(nameof(Header));

    /// <summary>
    /// Defines the <see cref="IconData"/> property.
    /// </summary>
    public static readonly StyledProperty<Geometry?> IconDataProperty =
        AvaloniaProperty.Register<NavigationItem, Geometry?>(nameof(IconData));

    /// <summary>
    /// Defines the <see cref="PageType"/> property.
    /// </summary>
    public static readonly StyledProperty<Type> PageTypeProperty =
        AvaloniaProperty.Register<NavigationItem, Type>(nameof(PageType));

    /// <summary>
    /// Defines the <see cref="PageViewModelType"/> property.
    /// </summary>
    public static readonly StyledProperty<Type> PageViewModelTypeProperty =
        AvaloniaProperty.Register<NavigationItem, Type>(nameof(PageViewModelType));

    /// <summary>
    /// Defines the <see cref="LabelMaxWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> LabelMaxWidthProperty =
        AvaloniaProperty.Register<NavigationItem, double>(nameof(LabelMaxWidth), 72d);

    /// <summary>
    /// Gets or sets the header text for the navigation item.
    /// </summary>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon geometry data for the navigation item.
    /// </summary>
    public Geometry? IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// <summary>
    /// Gets or sets the View type for this navigation item's page, used to match pages to items.
    /// </summary>
    public Type PageType
    {
        get => GetValue(PageTypeProperty);
        set => SetValue(PageTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the ViewModel type for this navigation item's page
    /// </summary>
    public Type PageViewModelType
    {
        get => GetValue(PageViewModelTypeProperty);
        set => SetValue(PageViewModelTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of the item's label in horizontal orientation. Defaults to 72.
    /// </summary>
    public double LabelMaxWidth
    {
        get => GetValue(LabelMaxWidthProperty);
        set => SetValue(LabelMaxWidthProperty, value);
    }
}
