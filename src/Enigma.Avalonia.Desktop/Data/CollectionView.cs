using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Collections;

namespace Enigma.Avalonia.Desktop.Data;

/// <summary>
/// Provides a filterable, sortable, and groupable view over an <see cref="IEnumerable"/> source.
/// Implements <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/> so
/// that bound controls update automatically when the view is refreshed.
/// </summary>
/// <remarks>
/// The view is also a read-only <see cref="IList"/>. That is not decoration: Avalonia's
/// <c>ItemsSourceView</c> rejects any collection that raises <see cref="INotifyCollectionChanged"/>
/// without implementing <see cref="IList"/>, so without it a view could never be assigned to
/// <c>ItemsControl.ItemsSource</c> — the one thing it exists for. Every mutating member throws
/// <see cref="NotSupportedException"/>: a view is projected from its source, and the way to change
/// it is to change the source or the filter, sort and group descriptions.
/// </remarks>
public class CollectionView : IList, INotifyCollectionChanged, INotifyPropertyChanged
{
    private static readonly ConcurrentDictionary<(Type, string), Func<object, object?>> _accessorCache = new();

    private readonly IEnumerable _source;
    private List<object> _view = [];
    private IReadOnlyList<CollectionViewGroup>? _groups;
    private int _deferLevel;

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionView"/> over the specified source collection.
    /// If the source implements <see cref="INotifyCollectionChanged"/>, the view refreshes automatically
    /// when the source changes.
    /// </summary>
    /// <param name="source">The source collection to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    public CollectionView(IEnumerable source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));

        if (source is INotifyCollectionChanged incc)
            incc.CollectionChanged += OnSourceCollectionChanged;
    }

    /// <summary>Raised when the contents of the view change, for example after a call to <see cref="Refresh"/>.</summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>Raised when <see cref="Count"/>, <see cref="IsEmpty"/>, or <see cref="Groups"/> changes.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the underlying source collection this view is built from.</summary>
    public IEnumerable SourceCollection => _source;

    /// <summary>
    /// Gets or sets a predicate used to filter items. Items for which the predicate returns
    /// <see langword="false"/> are excluded from the view. When <see langword="null"/>, all items are included.
    /// </summary>
    public Predicate<object>? Filter { get; set; }

    /// <summary>Gets or sets the list of sort criteria applied to the view in order of priority.</summary>
    public AvaloniaList<SortDescription> SortDescriptions { get; set; } = new();

    /// <summary>Gets or sets the list of group descriptions that define how items are grouped.</summary>
    public AvaloniaList<PropertyGroupDescription> GroupDescriptions { get; set; } = new();

    /// <summary>
    /// Gets the current groups after grouping has been applied, or <see langword="null"/> if
    /// no <see cref="GroupDescriptions"/> are set.
    /// </summary>
    public IReadOnlyList<CollectionViewGroup>? Groups => _groups;

    /// <summary>Gets the number of items in the current view after filtering.</summary>
    public int Count => _view.Count;

    /// <summary>Gets a value indicating whether the view contains no items.</summary>
    public bool IsEmpty => _view.Count == 0;

    /// <summary>Gets a value indicating whether the view can be modified. Always <see langword="true"/>.</summary>
    public bool IsReadOnly => true;

    /// <summary>Gets a value indicating whether the view has a fixed size. Always <see langword="false"/>.</summary>
    /// <remarks>
    /// A view's length follows its source and its filter, so it is not fixed — but it is still not
    /// the caller's to change, which <see cref="IsReadOnly"/> is what states.
    /// </remarks>
    public bool IsFixedSize => false;

    /// <summary>Gets a value indicating whether access to the view is synchronized. Always <see langword="false"/>.</summary>
    public bool IsSynchronized => false;

    /// <summary>Gets an object that can be used to synchronize access to the view.</summary>
    public object SyncRoot { get; } = new();

    /// <summary>Gets the item at the given position in the view.</summary>
    /// <param name="index">The zero-based position, in the view's filtered and sorted order.</param>
    /// <returns>The item at that position.</returns>
    /// <exception cref="NotSupportedException">Thrown on any attempt to set an item.</exception>
    public object? this[int index]
    {
        get => _view[index];
        set => throw new NotSupportedException("A CollectionView is a read-only projection of its source.");
    }

    /// <summary>
    /// Reapplies filtering, sorting, and grouping to the source collection and raises
    /// change notifications. Has no effect while a <see cref="DeferRefresh"/> token is active.
    /// </summary>
    public void Refresh()
    {
        if (_deferLevel > 0)
            return;

        // 1. Filter
        var filtered = new List<object>();
        foreach (var item in _source)
        {
            if (Filter is null || Filter(item))
                filtered.Add(item);
        }

        // 2. Sort
        if (SortDescriptions.Count > 0)
        {
            filtered.Sort((a, b) =>
            {
                foreach (var desc in SortDescriptions)
                {
                    if (string.IsNullOrEmpty(desc.PropertyName))
                        continue;

                    var valA = GetPropertyValue(a, desc.PropertyName);
                    var valB = GetPropertyValue(b, desc.PropertyName);

                    int cmp = Comparer.Default.Compare(valA, valB);
                    if (cmp != 0)
                        return desc.Direction == SortDirection.Descending ? -cmp : cmp;
                }

                return 0;
            });
        }

        // 3. Group
        if (GroupDescriptions.Count > 0)
        {
            var groupDesc = GroupDescriptions[0];
            var groupDict = new Dictionary<object, List<object>>();
            var groupOrder = new List<object>();

            foreach (var item in filtered)
            {
                var key = GetGroupKey(item, groupDesc);
                if (!groupDict.TryGetValue(key, out var list))
                {
                    list = [];
                    groupDict[key] = list;
                    groupOrder.Add(key);
                }

                list.Add(item);
            }

            var groups = new List<CollectionViewGroup>(groupOrder.Count);
            foreach (var key in groupOrder)
                groups.Add(new CollectionViewGroup(key, groupDict[key].AsReadOnly()));

            _groups = groups.AsReadOnly();
        }
        else
        {
            _groups = null;
        }

        // 4. Store flat view and raise events
        _view = filtered;

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmpty)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Groups)));
    }

    /// <summary>
    /// Suspends automatic refresh until the returned token is disposed, at which point a single
    /// <see cref="Refresh"/> is performed. Use this when making multiple changes to avoid redundant refreshes.
    /// </summary>
    /// <returns>A disposable token whose disposal triggers a deferred refresh.</returns>
    public IDisposable DeferRefresh() => new DeferToken(this);

    /// <summary>Unsubscribes this view from source collection change notifications.</summary>
    internal void Detach()
    {
        if (_source is INotifyCollectionChanged incc)
            incc.CollectionChanged -= OnSourceCollectionChanged;
    }

    /// <summary>Returns an enumerator that iterates over the filtered and sorted items in the view.</summary>
    /// <returns>An enumerator for the current view.</returns>
    public IEnumerator GetEnumerator() => _view.GetEnumerator();

    /// <summary>Determines whether the given item is currently in the view.</summary>
    /// <param name="value">The item to look for.</param>
    /// <returns><see langword="true"/> when the item survived the current filter.</returns>
    public bool Contains(object? value) => value is not null && _view.Contains(value);

    /// <summary>Returns the position of the given item in the view.</summary>
    /// <param name="value">The item to look for.</param>
    /// <returns>The zero-based position, or -1 when the item is not in the view.</returns>
    public int IndexOf(object? value) => value is null ? -1 : _view.IndexOf(value);

    /// <summary>Copies the view's items into an array.</summary>
    /// <param name="array">The destination array.</param>
    /// <param name="index">The position in <paramref name="array"/> to start writing at.</param>
    public void CopyTo(Array array, int index) => ((ICollection)_view).CopyTo(array, index);

    /// <summary>Not supported: a view is projected from its source.</summary>
    /// <param name="value">Ignored.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="NotSupportedException">Always thrown. Add to the source collection instead.</exception>
    public int Add(object? value) =>
        throw new NotSupportedException("Add to the source collection; the view follows it.");

    /// <summary>Not supported: a view is projected from its source.</summary>
    /// <exception cref="NotSupportedException">Always thrown. Clear the source collection instead.</exception>
    public void Clear() =>
        throw new NotSupportedException("Clear the source collection; the view follows it.");

    /// <summary>Not supported: the view's order comes from its sort descriptions.</summary>
    /// <param name="index">Ignored.</param>
    /// <param name="value">Ignored.</param>
    /// <exception cref="NotSupportedException">Always thrown. Insert into the source collection instead.</exception>
    public void Insert(int index, object? value) =>
        throw new NotSupportedException("Insert into the source collection; the view follows it.");

    /// <summary>Not supported: a view is projected from its source.</summary>
    /// <param name="value">Ignored.</param>
    /// <exception cref="NotSupportedException">Always thrown. Remove from the source collection, or filter it out.</exception>
    public void Remove(object? value) =>
        throw new NotSupportedException("Remove from the source collection, or exclude the item with Filter.");

    /// <summary>Not supported: a view is projected from its source.</summary>
    /// <param name="index">Ignored.</param>
    /// <exception cref="NotSupportedException">Always thrown. Remove from the source collection, or filter it out.</exception>
    public void RemoveAt(int index) =>
        throw new NotSupportedException("Remove from the source collection, or exclude the item with Filter.");

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        var type = obj.GetType();
        var accessor = _accessorCache.GetOrAdd((type, propertyName), static key =>
        {
            var prop = key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance);
            if (prop is null)
                return _ => null;

            return o => prop.GetValue(o);
        });

        return accessor(obj);
    }

    private static object GetGroupKey(object item, PropertyGroupDescription desc)
    {
        object? rawKey = string.IsNullOrEmpty(desc.PropertyName) ? item : GetPropertyValue(item, desc.PropertyName);

        if (desc.ValueConverter is { } converter)
            rawKey = converter.Convert(rawKey, typeof(object), null!, CultureInfo.CurrentCulture);

        return rawKey ?? string.Empty;
    }

    private sealed class DeferToken : IDisposable
    {
        private CollectionView? _view;

        public DeferToken(CollectionView view)
        {
            _view = view;
            _view._deferLevel++;
        }

        public void Dispose()
        {
            if (_view is null)
                return;

            _view._deferLevel--;
            if (_view._deferLevel == 0)
                _view.Refresh();

            _view = null;
        }
    }
}
