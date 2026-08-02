using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;

namespace Enigma.Avalonia.Desktop.Data;

/// <summary>
/// An <see cref="AvaloniaObject"/> that wraps a source collection in a <see cref="CollectionView"/>,
/// exposing bindable <see cref="SortDescriptions"/>, <see cref="GroupDescriptions"/>, and a
/// <see cref="Filter"/> event. The <see cref="View"/> property provides the live view for binding.
/// </summary>
public class CollectionViewSource : AvaloniaObject
{
    /// <summary>Defines the <see cref="Source"/> styled property.</summary>
    public static readonly StyledProperty<IEnumerable?> SourceProperty =
        AvaloniaProperty.Register<CollectionViewSource, IEnumerable?>(nameof(Source));

    /// <summary>Defines the <see cref="View"/> direct property.</summary>
    public static readonly DirectProperty<CollectionViewSource, CollectionView?> ViewProperty =
        AvaloniaProperty.RegisterDirect<CollectionViewSource, CollectionView?>(
            nameof(View),
            o => o.View);

    private CollectionView? _view;

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionViewSource"/> and subscribes to
    /// changes on <see cref="SortDescriptions"/> and <see cref="GroupDescriptions"/>.
    /// </summary>
    public CollectionViewSource()
    {
        SortDescriptions.CollectionChanged += OnSortDescriptionsCollectionChanged;
        GroupDescriptions.CollectionChanged += OnGroupDescriptionsCollectionChanged;
    }

    static CollectionViewSource()
    {
        SourceProperty.Changed.AddClassHandler<CollectionViewSource>((s, _) => s.OnSourceChanged());
    }

    /// <summary>
    /// Raised for each item when the view is refreshed, allowing handlers to accept or reject
    /// individual items by setting <see cref="FilterEventArgs.Accepted"/>.
    /// </summary>
    public event EventHandler<FilterEventArgs>? Filter;

    /// <summary>Gets or sets the source collection to wrap. Changing this property rebuilds the <see cref="View"/>.</summary>
    public IEnumerable? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets the current <see cref="CollectionView"/> built from <see cref="Source"/>,
    /// or <see langword="null"/> if <see cref="Source"/> is <see langword="null"/>.
    /// </summary>
    public CollectionView? View => _view;

    /// <summary>Gets the list of sort criteria applied to the view.</summary>
    public AvaloniaList<SortDescription> SortDescriptions { get; } = new();

    /// <summary>Gets the list of group descriptions applied to the view.</summary>
    public AvaloniaList<PropertyGroupDescription> GroupDescriptions { get; } = new();

    private void OnSourceChanged()
    {
        // Detach old view
        if (_view is not null)
        {
            _view.Detach();
            DetachDescriptionChangedHandlers(_view.SortDescriptions);
            DetachGroupDescriptionChangedHandlers(_view.GroupDescriptions);
        }

        var source = Source;
        if (source is null)
        {
            SetAndRaise(ViewProperty, ref _view, null);
            return;
        }

        var view = new CollectionView(source)
        {
            SortDescriptions = SortDescriptions,
            GroupDescriptions = GroupDescriptions,
        };

        if (Filter is not null)
        {
            view.Filter = item =>
            {
                var args = new FilterEventArgs(item);
                Filter.Invoke(this, args);
                return args.Accepted;
            };
        }

        AttachDescriptionChangedHandlers(SortDescriptions);
        AttachGroupDescriptionChangedHandlers(GroupDescriptions);

        SetAndRaise(ViewProperty, ref _view, view);

        // SetAndRaise takes _view by ref, which resets the compiler's null-state for the field to its
        // declared CollectionView?. The value just assigned is the non-null `view` local, so the
        // suppression is safe. (Solution invariant §2.4.3 requires a justification for every `!`.)
        _view!.Refresh();
    }

    private void OnSortDescriptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (SortDescription desc in e.OldItems)
                desc.DescriptionChanged -= OnDescriptionChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (SortDescription desc in e.NewItems)
                desc.DescriptionChanged += OnDescriptionChanged;
        }

        _view?.Refresh();
    }

    private void OnGroupDescriptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (PropertyGroupDescription desc in e.OldItems)
                desc.DescriptionChanged -= OnDescriptionChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (PropertyGroupDescription desc in e.NewItems)
                desc.DescriptionChanged += OnDescriptionChanged;
        }

        _view?.Refresh();
    }

    private void OnDescriptionChanged(object? sender, EventArgs e) => _view?.Refresh();

    private void AttachDescriptionChangedHandlers(AvaloniaList<SortDescription> descriptions)
    {
        foreach (var desc in descriptions)
            desc.DescriptionChanged += OnDescriptionChanged;
    }

    private void DetachDescriptionChangedHandlers(AvaloniaList<SortDescription> descriptions)
    {
        foreach (var desc in descriptions)
            desc.DescriptionChanged -= OnDescriptionChanged;
    }

    private void AttachGroupDescriptionChangedHandlers(AvaloniaList<PropertyGroupDescription> descriptions)
    {
        foreach (var desc in descriptions)
            desc.DescriptionChanged += OnDescriptionChanged;
    }

    private void DetachGroupDescriptionChangedHandlers(AvaloniaList<PropertyGroupDescription> descriptions)
    {
        foreach (var desc in descriptions)
            desc.DescriptionChanged -= OnDescriptionChanged;
    }
}
