using System;
using Avalonia;

namespace Enigma.Avalonia.Desktop.Data;

/// <summary>
/// Describes a sort criterion applied to a <see cref="CollectionView"/>, specifying
/// the property to sort on and the sort direction.
/// </summary>
public class SortDescription : AvaloniaObject
{
    /// <summary>Defines the <see cref="PropertyName"/> styled property.</summary>
    public static readonly StyledProperty<string?> PropertyNameProperty =
        AvaloniaProperty.Register<SortDescription, string?>(nameof(PropertyName));

    /// <summary>Defines the <see cref="Direction"/> styled property.</summary>
    public static readonly StyledProperty<SortDirection> DirectionProperty =
        AvaloniaProperty.Register<SortDescription, SortDirection>(nameof(Direction));

    /// <summary>Raised when <see cref="PropertyName"/> or <see cref="Direction"/> changes.</summary>
    public event EventHandler? DescriptionChanged;

    static SortDescription()
    {
        PropertyNameProperty.Changed.AddClassHandler<SortDescription>((s, _) => s.OnDescriptionChanged());
        DirectionProperty.Changed.AddClassHandler<SortDescription>((s, _) => s.OnDescriptionChanged());
    }

    /// <summary>Gets or sets the name of the property on source items to sort by.</summary>
    public string? PropertyName
    {
        get => GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    /// <summary>Gets or sets the direction of the sort.</summary>
    public SortDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    private void OnDescriptionChanged() => DescriptionChanged?.Invoke(this, EventArgs.Empty);
}
