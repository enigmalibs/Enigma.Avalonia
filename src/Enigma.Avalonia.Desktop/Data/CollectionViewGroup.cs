using System.Collections.Generic;

namespace Enigma.Avalonia.Desktop.Data;

/// <summary>
/// Represents a group of items within a <see cref="CollectionView"/> that share a common group key.
/// </summary>
public class CollectionViewGroup
{
    /// <summary>
    /// Initializes a new instance of <see cref="CollectionViewGroup"/> with the specified key and items.
    /// </summary>
    /// <param name="key">The value that identifies this group.</param>
    /// <param name="items">The read-only list of items belonging to this group.</param>
    public CollectionViewGroup(object key, IReadOnlyList<object> items)
    {
        Key = key;
        Items = items;
    }

    /// <summary>Gets the value that identifies this group, derived from the grouped property of its items.</summary>
    public object Key { get; }

    /// <summary>Gets the read-only list of items that belong to this group.</summary>
    public IReadOnlyList<object> Items { get; }

    /// <summary>Gets the number of items in this group.</summary>
    public int ItemCount => Items.Count;
}
