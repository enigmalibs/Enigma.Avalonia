using System.Collections.Generic;
using Enigma.Avalonia.Desktop.Data;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// Covers the filter stage of <see cref="CollectionView.Refresh"/>, including its interaction with
/// the sort stage that runs after it.
/// </summary>
public sealed class CollectionViewFilteringTests
{
    private static List<TestPerson> People() =>
    [
        new("Alice", 30, "Engineering"),
        new("Bob", 25, "Sales"),
        new("Carol", 35, "Engineering"),
        new("Dave", 28, "Sales"),
    ];

    [Fact]
    public void ANullFilter_IncludesEveryItem()
    {
        CollectionView view = new(People()) { Filter = null };

        view.Refresh();

        Assert.Equal(4, view.Count);
        Assert.False(view.IsEmpty);
    }

    [Fact]
    public void AFilter_ExcludesTheItemsItRejects()
    {
        CollectionView view = new(People())
        {
            Filter = item => ((TestPerson)item).Department == "Engineering",
        };

        view.Refresh();

        Assert.Equal(["Alice", "Carol"], ViewProjection.Names(view));
    }

    /// <summary>
    /// The filter is read on every refresh rather than captured once, so swapping the predicate and
    /// refreshing re-evaluates the whole source.
    /// </summary>
    [Fact]
    public void ChangingTheFilter_TakesEffectOnTheNextRefresh()
    {
        CollectionView view = new(People())
        {
            Filter = item => ((TestPerson)item).Department == "Engineering",
        };
        view.Refresh();
        Assert.Equal(2, view.Count);

        view.Filter = item => ((TestPerson)item).Age < 29;

        Assert.Equal(2, view.Count);
        Assert.Equal(["Alice", "Carol"], ViewProjection.Names(view));

        view.Refresh();

        Assert.Equal(["Bob", "Dave"], ViewProjection.Names(view));
    }

    /// <summary>
    /// Filtering runs before sorting, so the sort only ever sees the surviving items — the two
    /// stages compose rather than compete.
    /// </summary>
    [Fact]
    public void AFilterCombinedWithASort_FiltersFirstThenSortsTheSurvivors()
    {
        CollectionView view = new(People())
        {
            Filter = item => ((TestPerson)item).Age >= 28,
        };
        view.SortDescriptions.Add(new SortDescription
        {
            PropertyName = nameof(TestPerson.Age),
            Direction = SortDirection.Descending,
        });

        view.Refresh();

        Assert.Equal(["Carol", "Alice", "Dave"], ViewProjection.Names(view));
    }

    [Fact]
    public void AFilterRejectingEverything_LeavesTheViewEmpty()
    {
        CollectionView view = new(People()) { Filter = _ => false };

        view.Refresh();

        Assert.Empty(view);
        Assert.True(view.IsEmpty);
        Assert.Empty(ViewProjection.Names(view));
    }

    [Fact]
    public void ClearingTheFilter_RestoresTheFullView()
    {
        CollectionView view = new(People()) { Filter = _ => false };
        view.Refresh();
        Assert.True(view.IsEmpty);

        view.Filter = null;
        view.Refresh();

        Assert.Equal(4, view.Count);
    }
}
