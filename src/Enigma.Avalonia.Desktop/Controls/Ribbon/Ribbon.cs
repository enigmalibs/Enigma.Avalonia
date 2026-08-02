using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace Enigma.Avalonia.Desktop.Controls.Ribbon;

/// <summary>
/// A ribbon control that displays a set of <see cref="RibbonTab"/> instances in a tab strip,
/// each containing groups of commands.
/// </summary>
public class Ribbon : TemplatedControl
{
    /// <summary>The <c>PART_TabStrip</c> ListBox used to display the tab headers.</summary>
    private ListBox? _tabStrip;

    /// <summary>Initializes a new instance of <see cref="Ribbon"/> and wires the visual-tree attachment handler.</summary>
    public Ribbon()
    {
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    /// <summary>Gets the collection of tabs displayed in this ribbon.</summary>
    [Content]
    public AvaloniaList<RibbonTab> Tabs { get; } = new();

    /// <summary>Defines the <see cref="SelectedTab"/> property.</summary>
    public static readonly StyledProperty<RibbonTab?> SelectedTabProperty =
        AvaloniaProperty.Register<Ribbon, RibbonTab?>(
            nameof(SelectedTab),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<Ribbon, int>(
            nameof(SelectedIndex),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Gets or sets the currently selected tab.</summary>
    public RibbonTab? SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    /// <summary>Gets or sets the zero-based index of the currently selected tab.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Finds <c>PART_TabStrip</c>, wires selection change, and selects the first tab if none is selected.
    /// </summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_tabStrip is not null)
            _tabStrip.SelectionChanged -= OnTabStripSelectionChanged;

        _tabStrip = e.NameScope.Find<ListBox>("PART_TabStrip");

        if (_tabStrip is not null)
        {
            _tabStrip.SelectionChanged += OnTabStripSelectionChanged;

            // Sync the ListBox selection with the current SelectedTab
            // This is important when the template is reapplied (e.g., after navigation)
            if (SelectedTab is not null)
                _tabStrip.SelectedItem = SelectedTab;
        }

        if (SelectedTab is null && Tabs.Count > 0)
            SelectedTab = Tabs[0];
    }

    /// <summary>
    /// Synchronizes <see cref="SelectedTab"/> with the tab strip and keeps <see cref="SelectedIndex"/> in sync when either property changes.
    /// </summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedTabProperty && _tabStrip is not null)
        {
            var tab = change.GetNewValue<RibbonTab?>();
            if (_tabStrip.SelectedItem != tab)
                _tabStrip.SelectedItem = tab;

            // Sync SelectedIndex with SelectedTab
            var newIndex = tab != null ? Tabs.IndexOf(tab) : -1;
            if (newIndex != SelectedIndex)
                SelectedIndex = newIndex;
        }
        else if (change.Property == SelectedIndexProperty)
        {
            var index = change.GetNewValue<int>();
            if (index >= 0 && index < Tabs.Count)
            {
                var tab = Tabs[index];
                if (SelectedTab != tab)
                    SelectedTab = tab;
            }
        }
    }

    /// <summary>Updates <see cref="SelectedTab"/> when the tab strip selection changes.</summary>
    private void OnTabStripSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_tabStrip?.SelectedItem is RibbonTab tab)
            SelectedTab = tab;
    }

    /// <summary>
    /// Forces the tab strip selection to re-sync after the control is attached to the visual tree,
    /// ensuring bindings have been evaluated before updating the selection.
    /// </summary>
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // Force sync after being attached to visual tree
        // This ensures the ListBox selection is correct after navigation
        if (_tabStrip is not null && SelectedTab is not null)
        {
            // Use dispatcher to ensure bindings have been evaluated
            Dispatcher.UIThread.Post(() =>
            {
                if (_tabStrip.SelectedItem != SelectedTab)
                    _tabStrip.SelectedItem = SelectedTab;
            }, DispatcherPriority.Loaded);
        }
    }
}
