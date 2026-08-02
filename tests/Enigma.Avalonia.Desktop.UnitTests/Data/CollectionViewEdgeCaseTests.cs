using System;
using System.Collections;
using System.Collections.Generic;
using Enigma.Avalonia.Desktop.Data;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// The degenerate inputs — no source, no items, one item, nothing surviving the filter — plus the
/// two contract details a caller is most likely to get wrong.
/// </summary>
public sealed class CollectionViewEdgeCaseTests
{
    [Fact]
    public void ANullSource_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new CollectionView(null!));

        Assert.Equal("source", exception.ParamName);
    }

    /// <summary>
    /// The constructor does not project the source — the view is empty until the first refresh.
    /// <see cref="CollectionViewSource"/> hides this by refreshing for you; a hand-built
    /// <see cref="CollectionView"/> does not.
    /// </summary>
    [Fact]
    public void AFreshlyConstructedView_IsEmptyUntilTheFirstRefresh()
    {
        CollectionView view = new(new List<TestPerson> { new("Alice", 30, "Engineering") });

        Assert.Equal(0, view.Count);
        Assert.True(view.IsEmpty);

        view.Refresh();

        Assert.Equal(1, view.Count);
        Assert.False(view.IsEmpty);
    }

    [Fact]
    public void AnEmptySource_ProducesAnEmptyView()
    {
        CollectionView view = new(new List<TestPerson>());

        view.Refresh();

        Assert.Equal(0, view.Count);
        Assert.True(view.IsEmpty);
        Assert.Empty(ViewProjection.Names(view));
    }

    [Fact]
    public void ASingleItemSource_SurvivesSortingAndGrouping()
    {
        CollectionView view = new(new List<TestPerson> { new("Alice", 30, "Engineering") });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.Equal(["Alice"], ViewProjection.Names(view));
        Assert.NotNull(view.Groups);
        Assert.Single(view.Groups);
        Assert.Equal(1, view.Groups[0].ItemCount);
    }

    /// <summary>
    /// With every item filtered out, grouping still runs: <c>Groups</c> is an empty list rather
    /// than <see langword="null"/>, because a group description is configured.
    /// </summary>
    [Fact]
    public void AllItemsFilteredOut_LeaveAnEmptyGroupList()
    {
        CollectionView view = new(new List<TestPerson>
        {
            new("Alice", 30, "Engineering"),
            new("Bob", 25, "Sales"),
        })
        {
            Filter = _ => false,
        };
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.True(view.IsEmpty);
        Assert.NotNull(view.Groups);
        Assert.Empty(view.Groups);
    }

    /// <summary>Removing the group descriptions drops <c>Groups</c> back to <see langword="null"/>.</summary>
    [Fact]
    public void ClearingTheGroupDescriptions_ResetsGroupsToNull()
    {
        CollectionView view = new(new List<TestPerson> { new("Alice", 30, "Engineering") });
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });
        view.Refresh();
        Assert.NotNull(view.Groups);

        view.GroupDescriptions.Clear();
        view.Refresh();

        Assert.Null(view.Groups);
    }

    [Fact]
    public void SourceCollection_ExposesTheOriginalInstance()
    {
        List<TestPerson> source = [new("Alice", 30, "Engineering")];
        CollectionView view = new(source);

        Assert.Same(source, view.SourceCollection);
    }

    /// <summary>
    /// The enumerator walks the projected view, not the source — a filtered-out item is invisible
    /// to <c>foreach</c> even though it is still in the source collection.
    /// </summary>
    [Fact]
    public void GetEnumerator_WalksTheProjectedViewRatherThanTheSource()
    {
        List<TestPerson> source =
        [
            new("Alice", 30, "Engineering"),
            new("Bob", 25, "Sales"),
        ];
        CollectionView view = new(source)
        {
            Filter = item => ((TestPerson)item).Name == "Bob",
        };
        view.Refresh();

        IEnumerator enumerator = view.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal("Bob", Assert.IsType<TestPerson>(enumerator.Current).Name);
        Assert.False(enumerator.MoveNext());
        Assert.Equal(2, source.Count);
    }
}
