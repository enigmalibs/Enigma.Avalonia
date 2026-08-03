using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Enigma.Avalonia.Desktop.Data;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Data;

/// <summary>
/// Covers how a <see cref="CollectionView"/> reacts to its source changing, what it raises when it
/// refreshes, and how <see cref="CollectionView.DeferRefresh"/> suspends that.
/// </summary>
public sealed class CollectionViewNotificationTests
{
    private static ObservableCollection<TestPerson> People() =>
    [
        new("Alice", 30, "Engineering"),
        new("Bob", 25, "Sales"),
    ];

    [Fact]
    public void AnAddToTheSource_PropagatesToTheView()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source);
        view.Refresh();

        source.Add(new TestPerson("Carol", 35, "Support"));

        Assert.Equal(["Alice", "Bob", "Carol"], ViewProjection.Names(view));
    }

    [Fact]
    public void ARemoveFromTheSource_PropagatesToTheView()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source);
        view.Refresh();

        source.RemoveAt(0);

        Assert.Equal(["Bob"], ViewProjection.Names(view));
    }

    [Fact]
    public void AReplaceInTheSource_PropagatesToTheView()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source);
        view.Refresh();

        source[0] = new TestPerson("Zoe", 44, "Support");

        Assert.Equal(["Zoe", "Bob"], ViewProjection.Names(view));
    }

    [Fact]
    public void AResetOfTheSource_PropagatesToTheView()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source);
        view.Refresh();

        source.Clear();

        Assert.True(view.IsEmpty);
        Assert.Empty(view);
    }

    /// <summary>
    /// The source change triggers a full refresh, which re-runs the filter — so a newly added item
    /// reaches the view only when it passes.
    /// </summary>
    [Fact]
    public void AnItemAddedUnderAnActiveFilter_AppearsOnlyIfItPasses()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source)
        {
            Filter = item => ((TestPerson)item).Department == "Engineering",
        };
        view.Refresh();
        Assert.Equal(["Alice"], ViewProjection.Names(view));

        source.Add(new TestPerson("Bill", 50, "Sales"));

        Assert.Equal(["Alice"], ViewProjection.Names(view));

        source.Add(new TestPerson("Erin", 41, "Engineering"));

        Assert.Equal(["Alice", "Erin"], ViewProjection.Names(view));
    }

    /// <summary>
    /// The view reports every source change as a single <c>Reset</c> rather than a granular add or
    /// remove, because it rebuilds the whole projection each time.
    /// </summary>
    [Fact]
    public void ARefresh_RaisesResetAndThePropertyChangedTrio()
    {
        CollectionView view = new(People());
        List<NotifyCollectionChangedAction> actions = [];
        List<string?> properties = [];
        view.CollectionChanged += (_, e) => actions.Add(e.Action);
        view.PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        view.Refresh();

        Assert.Equal([NotifyCollectionChangedAction.Reset], actions);
        Assert.Equal(
            [nameof(CollectionView.Count), nameof(CollectionView.IsEmpty), nameof(CollectionView.Groups)],
            properties);
    }

    /// <summary>A source that does not notify cannot drive the view; only an explicit refresh can.</summary>
    [Fact]
    public void ANonObservableSource_DoesNotRefreshTheViewByItself()
    {
        List<TestPerson> source = [new("Alice", 30, "Engineering")];
        CollectionView view = new(source);
        view.Refresh();

        source.Add(new TestPerson("Bob", 25, "Sales"));

        Assert.Equal(["Alice"], ViewProjection.Names(view));

        view.Refresh();

        Assert.Equal(["Alice", "Bob"], ViewProjection.Names(view));
    }

    [Fact]
    public void DeferRefresh_SuspendsRefreshUntilTheTokenIsDisposed()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source);
        view.Refresh();
        int refreshes = 0;
        view.CollectionChanged += (_, _) => refreshes++;

        using (view.DeferRefresh())
        {
            source.Add(new TestPerson("Carol", 35, "Support"));
            source.Add(new TestPerson("Dave", 28, "Sales"));

            Assert.Equal(0, refreshes);
            Assert.Equal(2, view.Count);
        }

        Assert.Equal(1, refreshes);
        Assert.Equal(4, view.Count);
    }

    /// <summary>Nested tokens refresh once, on the outermost dispose.</summary>
    [Fact]
    public void NestedDeferRefresh_RefreshesOnceOnTheOutermostDispose()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source);
        view.Refresh();
        int refreshes = 0;
        view.CollectionChanged += (_, _) => refreshes++;

        IDisposable outer = view.DeferRefresh();
        IDisposable inner = view.DeferRefresh();
        source.Add(new TestPerson("Carol", 35, "Support"));

        inner.Dispose();

        Assert.Equal(0, refreshes);

        outer.Dispose();

        Assert.Equal(1, refreshes);
        Assert.Equal(3, view.Count);
    }

    /// <summary>Disposing the same token twice is a no-op rather than an unbalanced decrement.</summary>
    [Fact]
    public void DisposingADeferTokenTwice_DoesNotUnbalanceTheDeferLevel()
    {
        ObservableCollection<TestPerson> source = People();
        CollectionView view = new(source);
        view.Refresh();
        int refreshes = 0;
        view.CollectionChanged += (_, _) => refreshes++;

        IDisposable token = view.DeferRefresh();
        token.Dispose();
        token.Dispose();

        Assert.Equal(1, refreshes);

        source.Add(new TestPerson("Carol", 35, "Support"));

        Assert.Equal(2, refreshes);
    }

    /// <summary>
    /// <see cref="INotifyPropertyChanged"/> is implemented explicitly enough for a bound control to
    /// observe <c>Count</c> flipping the view from empty to populated.
    /// </summary>
    [Fact]
    public void TheViewReportsIsEmptyChanging_WhenTheFirstItemArrives()
    {
        ObservableCollection<TestPerson> source = [];
        CollectionView view = new(source);
        view.Refresh();
        Assert.True(view.IsEmpty);
        List<string?> properties = [];
        view.PropertyChanged += (_, e) => properties.Add(e.PropertyName);

        source.Add(new TestPerson("Alice", 30, "Engineering"));

        Assert.Contains(nameof(CollectionView.IsEmpty), properties);
        Assert.False(view.IsEmpty);
    }
}
