using System;
using Avalonia;
using Avalonia.Data.Converters;

namespace Enigma.Avalonia.Desktop.Data;

/// <summary>
/// Describes how items in a <see cref="CollectionView"/> are grouped, specifying
/// the property whose value determines group membership and an optional converter
/// to transform that value into a group key.
/// </summary>
public class PropertyGroupDescription : AvaloniaObject
{
    /// <summary>Defines the <see cref="PropertyName"/> styled property.</summary>
    public static readonly StyledProperty<string?> PropertyNameProperty =
        AvaloniaProperty.Register<PropertyGroupDescription, string?>(nameof(PropertyName));

    /// <summary>Defines the <see cref="ValueConverter"/> styled property.</summary>
    public static readonly StyledProperty<IValueConverter?> ValueConverterProperty =
        AvaloniaProperty.Register<PropertyGroupDescription, IValueConverter?>(nameof(ValueConverter));

    /// <summary>Raised when <see cref="PropertyName"/> or <see cref="ValueConverter"/> changes.</summary>
    public event EventHandler? DescriptionChanged;

    static PropertyGroupDescription()
    {
        PropertyNameProperty.Changed.AddClassHandler<PropertyGroupDescription>((s, _) => s.OnDescriptionChanged());
        ValueConverterProperty.Changed.AddClassHandler<PropertyGroupDescription>((s, _) => s.OnDescriptionChanged());
    }

    /// <summary>
    /// Gets or sets the name of the property on source items used to determine the group key.
    /// When <see langword="null"/> or empty, the item itself is used as the group key.
    /// </summary>
    public string? PropertyName
    {
        get => GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    /// <summary>Gets or sets an optional converter applied to the raw property value to produce the group key.</summary>
    public IValueConverter? ValueConverter
    {
        get => GetValue(ValueConverterProperty);
        set => SetValue(ValueConverterProperty, value);
    }

    private void OnDescriptionChanged() => DescriptionChanged?.Invoke(this, EventArgs.Empty);
}
