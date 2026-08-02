using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enigma.Avalonia.Desktop.Data;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// Covers the bindable wrapper: building the view from <c>Source</c>, the <c>Filter</c> event, and
/// the description-change subscriptions that make the view refresh without an explicit call.
/// </summary>
public sealed class CollectionViewSourceTests
{
    private static List<TestPerson> People() =>
    [
        new("Alice", 30, "Engineering"),
        new("Bob", 25, "Sales"),
        new("Carol", 35, "Engineering"),
    ];

    /// <summary>Asserts the wrapper built a view and hands it back non-null for the assertions.</summary>
    /// <param name="source">The wrapper whose view is under test.</param>
    /// <returns>The wrapper's current view.</returns>
    private static CollectionView ViewOf(CollectionViewSource source)
    {
        Assert.NotNull(source.View);
        return source.View;
    }

    [Fact]
    public void BeforeASourceIsSet_TheViewIsNull()
    {
        CollectionViewSource source = new();

        Assert.Null(source.View);
    }

    /// <summary>Unlike a hand-built view, the wrapper refreshes for you when the source is assigned.</summary>
    [Fact]
    public void SettingTheSource_BuildsAndRefreshesTheView()
    {
        CollectionViewSource source = new() { Source = People() };

        Assert.Equal(["Alice", "Bob", "Carol"], ViewProjection.Names(ViewOf(source)));
    }

    [Fact]
    public void SettingTheSourceBackToNull_ClearsTheView()
    {
        CollectionViewSource source = new() { Source = People() };
        Assert.NotNull(source.View);

        source.Source = null;

        Assert.Null(source.View);
    }

    [Fact]
    public void ReplacingTheSource_RebuildsTheView()
    {
        CollectionViewSource source = new() { Source = People() };
        CollectionView first = ViewOf(source);

        source.Source = new List<TestPerson> { new("Zoe", 44, "Support") };

        CollectionView second = ViewOf(source);
        Assert.NotSame(first, second);
        Assert.Equal(["Zoe"], ViewProjection.Names(second));
    }

    /// <summary>
    /// The replaced view is detached from its old source, so the discarded view stops tracking it —
    /// the guard against a leaked subscription keeping stale views alive.
    /// </summary>
    [Fact]
    public void ReplacingTheSource_DetachesTheOldViewFromItsCollectionChanged()
    {
        ObservableCollection<TestPerson> firstSource = [new("Alice", 30, "Engineering")];
        CollectionViewSource source = new() { Source = firstSource };
        CollectionView oldView = ViewOf(source);
        Assert.Equal(1, oldView.Count);

        source.Source = new List<TestPerson> { new("Zoe", 44, "Support") };
        firstSource.Add(new TestPerson("Bob", 25, "Sales"));

        Assert.Equal(1, oldView.Count);
    }

    [Fact]
    public void TheViewProperty_RaisesAChangeNotificationWhenTheSourceIsSet()
    {
        CollectionViewSource source = new();
        List<string> changed = [];
        source.PropertyChanged += (_, e) => changed.Add(e.Property.Name);

        source.Source = People();

        Assert.Contains(nameof(CollectionViewSource.View), changed);
    }

    [Fact]
    public void FilterEventArgs_DefaultToAccepted()
    {
        FilterEventArgs args = new(new TestPerson("Alice", 30, "Engineering"));

        Assert.True(args.Accepted);
    }

    [Fact]
    public void TheFilterEvent_ExcludesTheItemsAHandlerRejects()
    {
        CollectionViewSource source = new();
        source.Filter += (_, e) => e.Accepted = ((TestPerson)e.Item).Department == "Engineering";

        source.Source = People();

        Assert.Equal(["Alice", "Carol"], ViewProjection.Names(ViewOf(source)));
    }

    /// <summary>The handler is raised once per item on every refresh, not cached from the first pass.</summary>
    [Fact]
    public void TheFilterEvent_IsRaisedOncePerItemPerRefresh()
    {
        CollectionViewSource source = new();
        int calls = 0;
        source.Filter += (_, _) => calls++;
        source.Source = People();
        Assert.Equal(3, calls);

        ViewOf(source).Refresh();

        Assert.Equal(6, calls);
    }

    /// <summary>
    /// Pins a real ordering constraint in the shipped code: the wrapper wires the filter into the
    /// view only while the source is being assigned, so a handler attached afterwards never runs.
    /// Subscribe to <c>Filter</c> before setting <c>Source</c>.
    /// </summary>
    [Fact]
    public void AFilterHandlerAttachedAfterTheSource_IsNotApplied()
    {
        CollectionViewSource source = new() { Source = People() };
        CollectionView view = ViewOf(source);

        source.Filter += (_, e) => e.Accepted = false;
        view.Refresh();

        Assert.Equal(3, view.Count);
    }

    [Fact]
    public void AddingASortDescription_RefreshesTheViewWithoutAnExplicitCall()
    {
        CollectionViewSource source = new() { Source = People() };

        source.SortDescriptions.Add(new SortDescription { PropertyName = nameof(TestPerson.Age) });

        Assert.Equal(["Bob", "Alice", "Carol"], ViewProjection.Names(ViewOf(source)));
    }

    [Fact]
    public void ChangingASortDescriptionDirection_RefreshesTheView()
    {
        CollectionViewSource source = new() { Source = People() };
        CollectionView view = ViewOf(source);
        SortDescription byAge = new() { PropertyName = nameof(TestPerson.Age) };
        source.SortDescriptions.Add(byAge);
        Assert.Equal(["Bob", "Alice", "Carol"], ViewProjection.Names(view));

        byAge.Direction = SortDirection.Descending;

        Assert.Equal(["Carol", "Alice", "Bob"], ViewProjection.Names(view));
    }

    /// <summary>A removed description is unsubscribed, so changing it no longer refreshes the view.</summary>
    [Fact]
    public void ARemovedSortDescription_NoLongerRefreshesTheView()
    {
        CollectionViewSource source = new() { Source = People() };
        CollectionView view = ViewOf(source);
        SortDescription byAge = new() { PropertyName = nameof(TestPerson.Age) };
        source.SortDescriptions.Add(byAge);
        source.SortDescriptions.Remove(byAge);
        int refreshes = 0;
        view.CollectionChanged += (_, _) => refreshes++;

        byAge.Direction = SortDirection.Descending;

        Assert.Equal(0, refreshes);
        Assert.Equal(["Alice", "Bob", "Carol"], ViewProjection.Names(view));
    }

    [Fact]
    public void AddingAGroupDescription_RefreshesTheViewWithoutAnExplicitCall()
    {
        CollectionViewSource source = new() { Source = People() };

        source.GroupDescriptions.Add(new PropertyGroupDescription
        {
            PropertyName = nameof(TestPerson.Department),
        });

        Assert.Equal(["Engineering", "Sales"], ViewProjection.GroupKeys(ViewOf(source)));
    }

    [Fact]
    public void ChangingAGroupDescriptionPropertyName_RefreshesTheView()
    {
        CollectionViewSource source = new() { Source = People() };
        CollectionView view = ViewOf(source);
        PropertyGroupDescription byDepartment = new() { PropertyName = nameof(TestPerson.Department) };
        source.GroupDescriptions.Add(byDepartment);
        Assert.Equal(["Engineering", "Sales"], ViewProjection.GroupKeys(view));

        byDepartment.PropertyName = nameof(TestPerson.Age);

        Assert.Equal([30, 25, 35], ViewProjection.GroupKeys(view));
    }

    /// <summary>The descriptions configured on the wrapper are the ones the view sorts and groups by.</summary>
    [Fact]
    public void TheWrapperDescriptions_AreTheOnesTheViewUses()
    {
        CollectionViewSource source = new() { Source = People() };
        CollectionView view = ViewOf(source);

        Assert.Same(source.SortDescriptions, view.SortDescriptions);
        Assert.Same(source.GroupDescriptions, view.GroupDescriptions);
    }

    [Fact]
    public void AnObservableSource_KeepsDrivingTheViewThroughTheWrapper()
    {
        ObservableCollection<TestPerson> people = [new("Alice", 30, "Engineering")];
        CollectionViewSource source = new() { Source = people };

        people.Add(new TestPerson("Bob", 25, "Sales"));

        Assert.Equal(["Alice", "Bob"], ViewProjection.Names(ViewOf(source)));
    }
}
