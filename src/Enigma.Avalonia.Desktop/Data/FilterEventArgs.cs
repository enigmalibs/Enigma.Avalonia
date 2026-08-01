using System;

namespace Enigma.Avalonia.Desktop.Data;

/// <summary>
/// Provides data for the <see cref="CollectionViewSource.Filter"/> event,
/// allowing a handler to inspect an item and decide whether it should be included in the view.
/// </summary>
public class FilterEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of <see cref="FilterEventArgs"/> for the specified item.
    /// </summary>
    /// <param name="item">The item being evaluated by the filter.</param>
    public FilterEventArgs(object item)
    {
        Item = item;
    }

    /// <summary>Gets the item being evaluated by the filter.</summary>
    public object Item { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the item passes the filter and should appear in the view.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool Accepted { get; set; } = true;
}
