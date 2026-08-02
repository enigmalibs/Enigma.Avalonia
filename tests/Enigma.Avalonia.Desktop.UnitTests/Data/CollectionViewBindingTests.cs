using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Enigma.Avalonia.Desktop.Data;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// Covers the one thing a <see cref="CollectionView"/> exists to do: be the <c>ItemsSource</c> of a
/// control.
/// </summary>
/// <remarks>
/// Every other test in this folder exercises the view in isolation, as an <see cref="IEnumerable"/>.
/// That left a gap wide enough to hide a total failure: Avalonia's <c>ItemsSourceView</c> refuses any
/// collection that raises <see cref="INotifyCollectionChanged"/> without also implementing
/// <see cref="IList"/>, so a view that satisfied every isolated test still could not be bound to
/// anything. These tests bind a real control to a real view, which is the only way that gap closes.
/// </remarks>
public sealed class CollectionViewBindingTests
{
    private static ObservableCollection<TestPerson> People() =>
    [
        new("Alice", 30, "Engineering"),
        new("Bob", 25, "Sales"),
        new("Carol", 35, "Engineering"),
    ];

    private static CollectionView ViewOf(IEnumerable source, params SortDescription[] sorts)
    {
        CollectionView view = new(source);

        foreach (SortDescription sort in sorts)
            view.SortDescriptions.Add(sort);

        view.Refresh();
        return view;
    }

    /// <summary>The contract Avalonia checks before it will accept a collection as an items source.</summary>
    [Fact]
    public void AView_IsBothAListAndObservable()
    {
        CollectionView view = new(People());

        Assert.IsAssignableFrom<IList>(view);
        Assert.IsAssignableFrom<INotifyCollectionChanged>(view);
    }

    [AvaloniaFact]
    public void AView_CanBeAnItemsSource()
    {
        CollectionView view = ViewOf(People());

        ItemsControl control = new() { ItemsSource = view };

        Assert.Equal(3, control.ItemCount);
    }

    [AvaloniaFact]
    public void ASortedView_ReachesTheControlInViewOrder()
    {
        CollectionView view = ViewOf(
            People(),
            new SortDescription { PropertyName = "Age", Direction = SortDirection.Descending });

        ItemsControl control = new() { ItemsSource = view };

        Assert.Equal(["Carol", "Alice", "Bob"], ViewProjection.Names(control.ItemsView));
    }

    [AvaloniaFact]
    public void AFilterChange_ReachesTheControl()
    {
        CollectionView view = new(People()) { Filter = item => ((TestPerson)item).Department == "Engineering" };
        view.Refresh();

        ItemsControl control = new() { ItemsSource = view };
        Assert.Equal(2, control.ItemCount);

        view.Filter = null;
        view.Refresh();

        Assert.Equal(3, control.ItemCount);
    }

    [AvaloniaFact]
    public void AnAddToTheSource_ReachesTheControl()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = ViewOf(source);

        ItemsControl control = new() { ItemsSource = view };

        source.Add(new TestPerson("Dan", 40, "Sales"));

        Assert.Equal(4, control.ItemCount);
    }

    /// <summary>Indexed access is what a control uses to realise the container for one row.</summary>
    [Fact]
    public void AView_IsIndexableInViewOrder()
    {
        CollectionView view = ViewOf(
            People(),
            new SortDescription { PropertyName = "Age", Direction = SortDirection.Ascending });

        Assert.Equal("Bob", ((TestPerson)view[0]!).Name);
        Assert.Equal(1, view.IndexOf(view[1]));
        Assert.True(view.Contains(view[2]));
        Assert.Equal(-1, view.IndexOf(new TestPerson("Nobody", 1)));
        Assert.False(view.Contains(null));
    }

    [Fact]
    public void AView_CopiesItselfIntoAnArray()
    {
        CollectionView view = ViewOf(People());

        object[] target = new object[3];
        view.CopyTo(target, 0);

        Assert.Equal(["Alice", "Bob", "Carol"], ViewProjection.Names(target));
    }

    /// <summary>
    /// The view is a projection, so every mutating member of <see cref="IList"/> refuses. A control
    /// never calls these, but a consumer who typed the view as <see cref="IList"/> might.
    /// </summary>
    [Fact]
    public void EveryMutationOfTheView_IsRefused()
    {
        IList list = ViewOf(People());

        Assert.True(list.IsReadOnly);
        Assert.False(list.IsFixedSize);
        Assert.Throws<NotSupportedException>(() => list.Add(new TestPerson("Dan", 40)));
        Assert.Throws<NotSupportedException>(() => list.Insert(0, new TestPerson("Dan", 40)));
        Assert.Throws<NotSupportedException>(() => list.Remove(list[0]));
        Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
        Assert.Throws<NotSupportedException>(list.Clear);
        Assert.Throws<NotSupportedException>(() => list[0] = new TestPerson("Dan", 40));
    }
}
