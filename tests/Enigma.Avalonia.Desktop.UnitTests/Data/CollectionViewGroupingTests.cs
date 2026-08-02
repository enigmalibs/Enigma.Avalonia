using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Enigma.Avalonia.Desktop.Data;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// Covers the group stage of <see cref="CollectionView.Refresh"/> — key derivation, group order,
/// and how grouping composes with the filter and sort stages that run before it.
/// </summary>
public sealed class CollectionViewGroupingTests
{
    private static List<TestPerson> People() =>
    [
        new("Alice", 30, "Engineering"),
        new("Bob", 25, "Sales"),
        new("Carol", 35, "Engineering"),
        new("Dave", 28, "Sales"),
        new("Erin", 41, "Support"),
    ];

    [Fact]
    public void NoGroupDescriptions_LeaveGroupsNull()
    {
        CollectionView view = new(People());

        view.Refresh();

        Assert.Null(view.Groups);
    }

    [Fact]
    public void APropertyGroupDescription_ProducesOneGroupPerDistinctKey()
    {
        CollectionView view = new(People());
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.NotNull(view.Groups);
        Assert.Equal(3, view.Groups.Count);
        Assert.Equal(["Alice", "Carol"], ViewProjection.Names(view.Groups[0]));
        Assert.Equal(["Bob", "Dave"], ViewProjection.Names(view.Groups[1]));
        Assert.Equal(["Erin"], ViewProjection.Names(view.Groups[2]));
    }

    /// <summary>Groups appear in the order their key was first encountered, not in key order.</summary>
    [Fact]
    public void Groups_AreOrderedByFirstAppearance()
    {
        CollectionView view = new(People());
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.Equal(["Engineering", "Sales", "Support"], ViewProjection.GroupKeys(view));
    }

    /// <summary>
    /// Because the sort runs before the grouping, sorting reorders the groups themselves — the
    /// first-appearance order is computed over the already-sorted list.
    /// </summary>
    [Fact]
    public void ASortAheadOfTheGrouping_ReordersTheGroups()
    {
        CollectionView view = new(People());
        view.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        // Youngest first: Bob (Sales, 25), then Dave (Sales), Alice (Engineering, 30), … Erin (41).
        Assert.Equal(["Sales", "Engineering", "Support"], ViewProjection.GroupKeys(view));
        Assert.NotNull(view.Groups);
        Assert.Equal(["Bob", "Dave"], ViewProjection.Names(view.Groups[0]));
        Assert.Equal(["Alice", "Carol"], ViewProjection.Names(view.Groups[1]));
    }

    /// <summary>Filtered-out items never reach the grouping, so a group can disappear entirely.</summary>
    [Fact]
    public void GroupingCombinedWithASortAndAFilter_GroupsOnlyTheSurvivors()
    {
        CollectionView view = new(People())
        {
            Filter = item => ((TestPerson)item).Age < 40,
        };
        view.SortDescriptions.Add(new SortDescription
        {
            PropertyName = nameof(TestPerson.Age),
            Direction = SortDirection.Descending,
        });
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.Equal(["Engineering", "Sales"], ViewProjection.GroupKeys(view));
        Assert.NotNull(view.Groups);
        Assert.Equal(["Carol", "Alice"], ViewProjection.Names(view.Groups[0]));
        Assert.Equal(["Dave", "Bob"], ViewProjection.Names(view.Groups[1]));
    }

    [Fact]
    public void GroupMembership_FollowsAReassignedPropertyOnTheNextRefresh()
    {
        List<TestPerson> people = People();
        CollectionView view = new(people);
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });
        view.Refresh();
        Assert.NotNull(view.Groups);
        Assert.Equal(2, view.Groups[0].ItemCount);

        people[0].Department = "Sales";
        view.Refresh();

        Assert.NotNull(view.Groups);
        Assert.Equal(["Sales", "Engineering", "Support"], ViewProjection.GroupKeys(view));
        Assert.Equal(["Alice", "Bob", "Dave"], ViewProjection.Names(view.Groups[0]));
        Assert.Equal(["Carol"], ViewProjection.Names(view.Groups[1]));
    }

    /// <summary>
    /// A <see langword="null"/> key is coerced to the empty string, because a group key is a
    /// dictionary key and cannot be <see langword="null"/>.
    /// </summary>
    [Fact]
    public void ANullGroupKey_BecomesTheEmptyString()
    {
        CollectionView view = new(new List<TestPerson>
        {
            new("Alice", 30, null),
            new("Bob", 25, "Sales"),
            new("Carol", 35, null),
        });
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.Equal([string.Empty, "Sales"], ViewProjection.GroupKeys(view));
        Assert.NotNull(view.Groups);
        Assert.Equal(["Alice", "Carol"], ViewProjection.Names(view.Groups[0]));
    }

    /// <summary>A description with no property name groups by the item itself.</summary>
    [Fact]
    public void AnEmptyPropertyName_GroupsByTheItemItself()
    {
        TestPerson alice = new("Alice", 30, "Engineering");
        TestPerson bob = new("Bob", 25, "Sales");
        CollectionView view = new(new List<TestPerson> { alice, bob, alice });
        view.GroupDescriptions.Add(new PropertyGroupDescription { PropertyName = null });

        view.Refresh();

        Assert.Equal([alice, bob], ViewProjection.GroupKeys(view));
        Assert.NotNull(view.Groups);
        Assert.Equal(2, view.Groups[0].ItemCount);
    }

    [Fact]
    public void AValueConverter_TransformsTheGroupKey()
    {
        CollectionView view = new(People());
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
            ValueConverter = new FirstLetterConverter(),
        });

        view.Refresh();

        Assert.Equal(["E", "S"], ViewProjection.GroupKeys(view));
        Assert.NotNull(view.Groups);
        Assert.Equal(["Alice", "Carol"], ViewProjection.Names(view.Groups[0]));
        Assert.Equal(["Bob", "Dave", "Erin"], ViewProjection.Names(view.Groups[1]));
    }

    /// <summary>
    /// Pins the shipped behaviour rather than an expectation: only the first group description is
    /// applied. The view has no nested-group model, so a second description is silently inert.
    /// </summary>
    [Fact]
    public void OnlyTheFirstGroupDescription_IsApplied()
    {
        CollectionView view = new(People());
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Age),
        });

        view.Refresh();

        Assert.Equal(["Engineering", "Sales", "Support"], ViewProjection.GroupKeys(view));
    }

    /// <summary>The flat view is unaffected by grouping — both projections stay available.</summary>
    [Fact]
    public void Grouping_LeavesTheFlatViewIntact()
    {
        CollectionView view = new(People());
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.Equal(5, view.Count);
        Assert.Equal(["Alice", "Bob", "Carol", "Dave", "Erin"], ViewProjection.Names(view));
    }

    [Fact]
    public void ACollectionViewGroup_ReportsItsItemCount()
    {
        CollectionView view = new(People());
        view.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        view.Refresh();

        Assert.NotNull(view.Groups);
        foreach (var group in view.Groups)
            Assert.Equal(group.Items.Count, group.ItemCount);
    }

    /// <summary>Projects a department name down to its initial, to prove the converter is consulted.</summary>
    private sealed class FirstLetterConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is string text && text.Length > 0 ? text[..1] : value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
