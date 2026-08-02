using System.Collections.Generic;
using Enigma.Avalonia.Desktop.Data;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// Covers the sort stage of <see cref="CollectionView.Refresh"/>: description priority, direction,
/// and the two ways a description can be inert.
/// </summary>
public sealed class CollectionViewSortingTests
{
    private static readonly TestPerson Alice = new("Alice", 30, "Engineering");
    private static readonly TestPerson Bob = new("Bob", 25, "Sales");
    private static readonly TestPerson Carol = new("Carol", 35, "Engineering");

    [Fact]
    public void NoSortDescriptions_PreserveTheSourceOrder()
    {
        CollectionView view = new(new List<TestPerson> { Carol, Alice, Bob });

        view.Refresh();

        Assert.Equal(["Carol", "Alice", "Bob"], ViewProjection.Names(view));
    }

    [Fact]
    public void ASingleAscendingDescription_OrdersByThatProperty()
    {
        CollectionView view = new(new List<TestPerson> { Carol, Alice, Bob });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });

        view.Refresh();

        Assert.Equal(["Bob", "Alice", "Carol"], ViewProjection.Names(view));
    }

    [Fact]
    public void ASingleDescendingDescription_ReversesThatOrder()
    {
        CollectionView view = new(new List<TestPerson> { Bob, Alice, Carol });
        view.SortDescriptions.Add(new SortDescription
        {
            PropertyName = nameof(TestPerson.Age),
            Direction = SortDirection.Descending,
        });

        view.Refresh();

        Assert.Equal(["Carol", "Alice", "Bob"], ViewProjection.Names(view));
    }

    /// <summary>
    /// Descriptions are a priority chain, not a set: the second only breaks ties left by the first,
    /// and each carries its own direction.
    /// </summary>
    [Fact]
    public void MultipleDescriptions_AreAppliedInPriorityOrder()
    {
        CollectionView view = new(MixedDepartments());
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Department) });
        view.SortDescriptions.Add(new SortDescription
        {
            PropertyName = nameof(TestPerson.Age),
            Direction = SortDirection.Descending,
        });

        view.Refresh();

        // Engineering before Sales (ascending), then oldest first within Engineering.
        Assert.Equal(["Carol", "Alice", "Dave", "Bob"], ViewProjection.Names(view));
    }

    /// <summary>
    /// Reversing the priority of the same two descriptions reorders the view — proof the chain is
    /// ordered rather than combined. Bob is the oldest but sorts last by department, so the two
    /// priorities cannot agree.
    /// </summary>
    [Fact]
    public void ReversingTheDescriptionPriority_ChangesTheResult()
    {
        CollectionView view = new(MixedDepartments());
        view.SortDescriptions.Add(new SortDescription
        {
            PropertyName = nameof(TestPerson.Age),
            Direction = SortDirection.Descending,
        });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Department) });

        view.Refresh();

        Assert.Equal(["Bob", "Carol", "Alice", "Dave"], ViewProjection.Names(view));
    }

    /// <summary>Four people whose age order and department order deliberately disagree.</summary>
    /// <returns>The source list, in an order that matches neither sort.</returns>
    private static List<TestPerson> MixedDepartments() =>
    [
        new("Bob", 40, "Sales"),
        new("Carol", 35, "Engineering"),
        new("Alice", 30, "Engineering"),
        new("Dave", 28, "Engineering"),
    ];

    /// <summary>A description with no property name is skipped, letting the next one decide.</summary>
    [Fact]
    public void ADescriptionWithoutAPropertyName_IsSkipped()
    {
        CollectionView view = new(new List<TestPerson> { Carol, Alice, Bob });
        view.SortDescriptions.Add(new SortDescription { PropertyName = null });
        view.SortDescriptions.Add(new SortDescription { PropertyName = string.Empty });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });

        view.Refresh();

        Assert.Equal(["Bob", "Alice", "Carol"], ViewProjection.Names(view));
    }

    /// <summary>
    /// An unknown property name resolves to <see langword="null"/> for every item rather than
    /// throwing, so the description contributes no ordering and the next one decides.
    /// </summary>
    [Fact]
    public void ADescriptionNamingAnUnknownProperty_ContributesNoOrdering()
    {
        CollectionView view = new(new List<TestPerson> { Carol, Alice, Bob });
        view.SortDescriptions.Add(new SortDescription { PropertyName = "NoSuchProperty" });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });

        view.Refresh();

        Assert.Equal(["Bob", "Alice", "Carol"], ViewProjection.Names(view));
    }

    /// <summary>
    /// The view sorts on demand, not on mutation: an item whose sort key changed keeps its place
    /// until the next <see cref="CollectionView.Refresh"/>.
    /// </summary>
    [Fact]
    public void AnItemPropertyChange_ReordersOnTheNextRefresh()
    {
        TestPerson alice = new("Alice", 30);
        TestPerson bob = new("Bob", 25);
        TestPerson carol = new("Carol", 35);
        CollectionView view = new(new List<TestPerson> { alice, bob, carol });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });
        view.Refresh();
        Assert.Equal(["Bob", "Alice", "Carol"], ViewProjection.Names(view));

        bob.Age = 99;

        Assert.Equal(["Bob", "Alice", "Carol"], ViewProjection.Names(view));

        view.Refresh();

        Assert.Equal(["Alice", "Carol", "Bob"], ViewProjection.Names(view));
    }

    /// <summary>
    /// Duplicate keys are ordered by key only. The sort runs through <see cref="List{T}.Sort"/>,
    /// which is not a stable sort, so the assertion pins the key sequence — the part of the
    /// contract that holds — and deliberately not the relative order of the tied items.
    /// </summary>
    [Fact]
    public void DuplicateSortKeys_ProduceANonDecreasingKeySequence()
    {
        CollectionView view = new(new List<TestPerson>
        {
            new("Alice", 30),
            new("Bob", 25),
            new("Carol", 30),
            new("Dave", 25),
            new("Erin", 30),
        });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });

        view.Refresh();

        List<int> ages = [];
        foreach (TestPerson person in view)
            ages.Add(person.Age);

        Assert.Equal([25, 25, 30, 30, 30], ages);
        Assert.Equal(5, view.Count);
    }

    /// <summary>
    /// A <see langword="null"/> key sorts before every non-null one: the comparison runs through
    /// <c>Comparer.Default</c>, which treats <see langword="null"/> as the smallest value.
    /// </summary>
    [Fact]
    public void NullKeys_SortFirstInAscendingOrder()
    {
        CollectionView view = new(new List<TestPerson>
        {
            new("Alice", 30, "Sales"),
            new("Bob", 25, null),
            new("Carol", 35, "Engineering"),
        });
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Department) });

        view.Refresh();

        Assert.Equal(["Bob", "Carol", "Alice"], ViewProjection.Names(view));
    }
}
