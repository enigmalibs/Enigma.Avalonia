using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Enigma.Avalonia.Desktop.Controls.Navigation;

/// <summary>
/// A control that provides navigation capabilities, typically used for a side navigation bar.
/// </summary>
public class NavigationView : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<NavigationItem>?> ItemsProperty =
        AvaloniaProperty.Register<NavigationView, IReadOnlyList<NavigationItem>?>(nameof(Items));

    /// <summary>
    /// Defines the <see cref="FooterItems"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<NavigationItem>?> FooterItemsProperty =
        AvaloniaProperty.Register<NavigationView, IReadOnlyList<NavigationItem>?>(nameof(FooterItems));

    /// <summary>
    /// Defines the <see cref="SelectedItem"/> property.
    /// </summary>
    public static readonly StyledProperty<NavigationItem?> SelectedItemProperty =
        AvaloniaProperty.Register<NavigationView, NavigationItem?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Defines the <see cref="Logo"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> LogoProperty =
        AvaloniaProperty.Register<NavigationView, object?>(nameof(Logo));

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<NavigationOrientation> OrientationProperty =
        AvaloniaProperty.Register<NavigationView, NavigationOrientation>(nameof(Orientation), NavigationOrientation.Vertical);

    /// <summary>
    /// Defines the <see cref="PaneSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PaneSizeProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(PaneSize), 90d);

    /// <summary>
    /// The <see cref="ListBox"/> used for main navigation items.
    /// </summary>
    private ListBox? _itemsListBox;

    /// <summary>
    /// The <see cref="ListBox"/> used for footer navigation items.
    /// </summary>
    private ListBox? _footerListBox;

    /// <summary>
    /// Indicates whether a synchronization operation is currently in progress to avoid recursive calls.
    /// </summary>
    private bool _isSyncing;

    /// <summary>
    /// Gets or sets the main navigation items.
    /// </summary>
    public IReadOnlyList<NavigationItem>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the footer navigation items.
    /// </summary>
    public IReadOnlyList<NavigationItem>? FooterItems
    {
        get => GetValue(FooterItemsProperty);
        set => SetValue(FooterItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected navigation item.
    /// </summary>
    public NavigationItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the logo content to be displayed in the navigation control.
    /// </summary>
    public object? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the navigation control.
    /// </summary>
    public NavigationOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the cross-axis extent of the navigation rail — its width when
    /// <see cref="Orientation"/> is vertical, and its maximum height when horizontal. Defaults to 90.
    /// </summary>
    public double PaneSize
    {
        get => GetValue(PaneSizeProperty);
        set => SetValue(PaneSizeProperty, value);
    }

    /// <summary>
    /// Finds and wires <c>PART_ItemsListBox</c> and <c>PART_FooterListBox</c>,
    /// synchronizes selection state, and applies orientation layout.
    /// </summary>
    /// <param name="e">The template applied event data containing the name scope.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_itemsListBox != null)
            _itemsListBox.SelectionChanged -= OnItemsSelectionChanged;
        if (_footerListBox != null)
            _footerListBox.SelectionChanged -= OnFooterSelectionChanged;

        _itemsListBox = e.NameScope.Find<ListBox>("PART_ItemsListBox");
        _footerListBox = e.NameScope.Find<ListBox>("PART_FooterListBox");
        if (_itemsListBox != null)
            _itemsListBox.SelectionChanged += OnItemsSelectionChanged;
        if (_footerListBox != null)
            _footerListBox.SelectionChanged += OnFooterSelectionChanged;

        SyncListBoxSelection();
        ApplyOrientationLayout();
    }

    /// <summary>
    /// Responds to property changes by synchronizing list box selection when
    /// <see cref="SelectedItem"/> changes, or updating layout when <see cref="Orientation"/> changes.
    /// </summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
            SyncListBoxSelection();
        else if (change.Property == OrientationProperty)
            ApplyOrientationLayout();
    }

    /// <summary>
    /// Handles the selection change event for the main items ListBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnItemsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isSyncing)
            return;

        _isSyncing = true;
        try
        {
            if (_itemsListBox?.SelectedItem is NavigationItem item)
            {
                if (_footerListBox != null)
                    _footerListBox.SelectedItem = null;
                SelectedItem = item;
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// Handles the selection change event for the footer items ListBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnFooterSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isSyncing)
            return;

        _isSyncing = true;
        try
        {
            if (_footerListBox?.SelectedItem is NavigationItem item)
            {
                if (_itemsListBox != null)
                    _itemsListBox.SelectedItem = null;
                SelectedItem = item;
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// Updates the pseudo-classes of the navigation control and its items based on the current <see cref="Orientation"/>.
    /// </summary>
    private void ApplyOrientationLayout()
    {
        bool horizontal = Orientation == NavigationOrientation.Horizontal;

        PseudoClasses.Set(":vertical", !horizontal);
        PseudoClasses.Set(":horizontal", horizontal);

        ApplyItemOrientationClasses(Items, horizontal);
        ApplyItemOrientationClasses(FooterItems, horizontal);
    }

    /// <summary>
    /// Applies <c>:horizontal</c> and <c>:vertical</c> pseudo-classes to a collection of
    /// <see cref="NavigationItem"/> instances based on the current orientation.
    /// </summary>
    /// <param name="items">The items to update, or <see langword="null"/> to skip.</param>
    /// <param name="horizontal"><see langword="true"/> when the orientation is horizontal.</param>
    private static void ApplyItemOrientationClasses(IReadOnlyList<NavigationItem>? items, bool horizontal)
    {
        if (items == null) return;

        foreach (var item in items)
        {
            ((IPseudoClasses)item.Classes).Set(":horizontal", horizontal);
            ((IPseudoClasses)item.Classes).Set(":vertical", !horizontal);
        }
    }

    /// <summary>
    /// Synchronizes the selection state between the <see cref="SelectedItem"/> property and the ListBoxes.
    /// </summary>
    private void SyncListBoxSelection()
    {
        if (_isSyncing)
            return;

        _isSyncing = true;
        try
        {
            var selected = SelectedItem;

            if (selected != null && Items != null && Items.Contains(selected))
            {
                if (_itemsListBox != null)
                    _itemsListBox.SelectedItem = selected;
                if (_footerListBox != null)
                    _footerListBox.SelectedItem = null;
            }
            else if (selected != null && FooterItems != null && FooterItems.Contains(selected))
            {
                if (_itemsListBox != null)
                    _itemsListBox.SelectedItem = null;
                if (_footerListBox != null)
                    _footerListBox.SelectedItem = selected;
            }
            else
            {
                if (_itemsListBox != null)
                    _itemsListBox.SelectedItem = null;
                if (_footerListBox != null)
                    _footerListBox.SelectedItem = null;
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }
}
