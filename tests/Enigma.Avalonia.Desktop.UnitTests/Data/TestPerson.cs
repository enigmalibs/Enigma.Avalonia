using System.Collections;
using System.Collections.Generic;
using Enigma.Avalonia.Desktop.Data;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// The mutable model the <c>CollectionView</c> tests sort, filter and group.
/// </summary>
/// <remarks>
/// Deliberately a plain class with settable properties and no change notification:
/// <see cref="CollectionView"/> subscribes to the <em>source collection</em>, never to the items,
/// so a model that raised <c>PropertyChanged</c> would suggest a refresh trigger that does not
/// exist. Mutating an item and calling <see cref="CollectionView.Refresh"/> is the real contract.
/// </remarks>
internal sealed class TestPerson
{
    /// <summary>Initializes a new person.</summary>
    /// <param name="name">The person's name, used as the identity in assertions.</param>
    /// <param name="age">The person's age, the numeric sort key.</param>
    /// <param name="department">The person's department, the group key. May be <see langword="null"/>.</param>
    public TestPerson(string name, int age, string? department = null)
    {
        Name = name;
        Age = age;
        Department = department;
    }

    /// <summary>Gets or sets the person's name.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the person's age.</summary>
    public int Age { get; set; }

    /// <summary>Gets or sets the person's department, which may be <see langword="null"/>.</summary>
    public string? Department { get; set; }

    /// <summary>Returns the name, so a failing collection assertion prints something readable.</summary>
    /// <returns>The person's name.</returns>
    public override string ToString() => Name;
}

/// <summary>
/// Projection helpers that turn a view or a group into the plain string sequences the assertions
/// compare, keeping the tests about ordering and membership rather than about casting.
/// </summary>
internal static class ViewProjection
{
    /// <summary>Projects the current contents of a view to the names of its people, in view order.</summary>
    /// <param name="view">The view to enumerate.</param>
    /// <returns>The names, in the order the view yields them.</returns>
    public static List<string> Names(IEnumerable view)
    {
        List<string> names = [];
        foreach (TestPerson person in view)
            names.Add(person.Name);

        return names;
    }

    /// <summary>Projects the items of a single group to their names, in group order.</summary>
    /// <param name="group">The group to project.</param>
    /// <returns>The names of the group's members.</returns>
    public static List<string> Names(CollectionViewGroup group)
    {
        List<string> names = [];
        foreach (TestPerson person in group.Items)
            names.Add(person.Name);

        return names;
    }

    /// <summary>Projects a view's groups to their keys, in group order.</summary>
    /// <param name="view">The grouped view.</param>
    /// <returns>The group keys, in the order the view produced the groups.</returns>
    public static List<object> GroupKeys(CollectionView view)
    {
        List<object> keys = [];
        foreach (var group in view.Groups ?? [])
            keys.Add(group.Key);

        return keys;
    }
}
