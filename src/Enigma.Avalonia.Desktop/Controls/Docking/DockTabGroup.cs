using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Enigma.Avalonia.Desktop.Controls.Docking;

/// <summary>
/// Provides data for <see cref="DockTabGroup"/> pane events such as drag-start and close-request.
/// </summary>
public class DockTabGroupEventArgs : EventArgs
{
    /// <summary>Gets the pane that is the subject of the event.</summary>
    public DockPane Pane { get; }

    /// <summary>Gets the <see cref="DockTabGroup"/> from which the pane originated.</summary>
    public DockTabGroup SourceGroup { get; }

    /// <summary>Gets the pointer associated with the event, if any.</summary>
    public IPointer? Pointer { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DockTabGroupEventArgs"/>.
    /// </summary>
    /// <param name="pane">The pane that triggered the event.</param>
    /// <param name="sourceGroup">The group that owns the pane.</param>
    /// <param name="pointer">The pointer involved in the interaction, or <see langword="null"/>.</param>
    public DockTabGroupEventArgs(DockPane pane, DockTabGroup sourceGroup, IPointer? pointer = null)
    {
        Pane = pane;
        SourceGroup = sourceGroup;
        Pointer = pointer;
    }
}

/// <summary>
/// A templated control that displays a collection of <see cref="DockPane"/> instances as tabs,
/// and raises events when a pane is dragged or a close is requested.
/// </summary>
public class DockTabGroup : TemplatedControl
{
    /// <summary>The <c>PART_TabStrip</c> ListBox used to display the tab headers.</summary>
    private ListBox? _tabStrip;

    /// <summary>The canvas-space position where the pointer was pressed at the start of a potential drag.</summary>
    private Point _dragStartPoint;

    /// <summary>The pane identified as a drag candidate when the pointer is pressed but has not yet exceeded the drag threshold.</summary>
    private DockPane? _dragCandidate;

    /// <summary>Indicates whether a drag operation is currently in progress.</summary>
    private bool _isDragging;

    /// <summary>Minimum pointer displacement in pixels required to trigger a drag operation.</summary>
    private const double DragThreshold = 5.0;

    /// <summary>Gets the collection of panes displayed as tabs in this group.</summary>
    public AvaloniaList<DockPane> Panes { get; } = new();

    /// <summary>Defines the <see cref="SelectedPane"/> property.</summary>
    public static readonly StyledProperty<DockPane?> SelectedPaneProperty =
        AvaloniaProperty.Register<DockTabGroup, DockPane?>(
            nameof(SelectedPane),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Gets or sets the currently selected pane.</summary>
    public DockPane? SelectedPane
    {
        get => GetValue(SelectedPaneProperty);
        set => SetValue(SelectedPaneProperty, value);
    }

    /// <summary>Raised when the user begins dragging a pane tab beyond the drag threshold.</summary>
    public event EventHandler<DockTabGroupEventArgs>? PaneDragStarted;

    /// <summary>Raised when the user clicks the close button on a pane tab.</summary>
    public event EventHandler<DockTabGroupEventArgs>? PaneCloseRequested;

    /// <summary>
    /// Finds the <c>PART_TabStrip</c> template part, wires selection and pointer events,
    /// and ensures an initial pane selection.
    /// </summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_tabStrip is not null)
        {
            _tabStrip.SelectionChanged -= OnTabStripSelectionChanged;
            _tabStrip.RemoveHandler(PointerPressedEvent, OnTabStripPointerPressed);
            _tabStrip.RemoveHandler(PointerMovedEvent, OnTabStripPointerMoved);
            _tabStrip.RemoveHandler(PointerReleasedEvent, OnTabStripPointerReleased);
        }

        _tabStrip = e.NameScope.Find<ListBox>("PART_TabStrip");

        if (_tabStrip is not null)
        {
            _tabStrip.SelectionChanged += OnTabStripSelectionChanged;
            _tabStrip.AddHandler(PointerPressedEvent, OnTabStripPointerPressed, RoutingStrategies.Bubble, true);
            _tabStrip.AddHandler(PointerMovedEvent, OnTabStripPointerMoved, RoutingStrategies.Bubble, true);
            _tabStrip.AddHandler(PointerReleasedEvent, OnTabStripPointerReleased, RoutingStrategies.Bubble, true);
        }

        if (SelectedPane is null && Panes.Count > 0)
            SelectedPane = Panes[0];
    }

    /// <summary>
    /// Synchronizes the tab strip's selected item when <see cref="SelectedPane"/> changes.
    /// </summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedPaneProperty && _tabStrip is not null)
        {
            var pane = change.GetNewValue<DockPane?>();
            if (_tabStrip.SelectedItem != pane)
                _tabStrip.SelectedItem = pane;
        }
    }

    /// <summary>Updates <see cref="SelectedPane"/> when the tab strip selection changes.</summary>
    private void OnTabStripSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_tabStrip?.SelectedItem is DockPane pane)
            SelectedPane = pane;
    }

    /// <summary>
    /// Handles pointer press on the tab strip. Detects close-button clicks and starts tracking
    /// a potential drag operation for movable panes.
    /// </summary>
    private void OnTabStripPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDragging = false;
        _dragCandidate = null;

        if (_tabStrip == null || !e.GetCurrentPoint(_tabStrip).Properties.IsLeftButtonPressed)
            return;

        var hitVisual = _tabStrip.InputHitTest(e.GetPosition(_tabStrip)) as Visual;
        if (hitVisual == null)
            return;

        if (IsCloseButton(hitVisual))
        {
            var pane = FindPaneFromVisual(hitVisual);
            if (pane != null && pane.CanClose)
            {
                PaneCloseRequested?.Invoke(this, new DockTabGroupEventArgs(pane, this));
                e.Handled = true;
            }
            return;
        }

        var paneForDrag = FindPaneFromVisual(hitVisual);
        if (paneForDrag != null && paneForDrag.CanMove)
        {
            _dragCandidate = paneForDrag;
            _dragStartPoint = e.GetPosition(this);
        }
    }

    /// <summary>
    /// Handles pointer movement over the tab strip. Fires <see cref="PaneDragStarted"/> once the
    /// pointer has moved beyond <see cref="DragThreshold"/> pixels from the press position.
    /// </summary>
    private void OnTabStripPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragCandidate == null || _isDragging)
            return;

        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _dragStartPoint;

        if (Math.Abs(delta.X) > DragThreshold || Math.Abs(delta.Y) > DragThreshold)
        {
            _isDragging = true;
            var pane = _dragCandidate;
            _dragCandidate = null;
            PaneDragStarted?.Invoke(this, new DockTabGroupEventArgs(pane!, this, e.Pointer));
        }
    }

    /// <summary>Clears the drag candidate and drag state when the pointer is released.</summary>
    private void OnTabStripPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragCandidate = null;
        _isDragging = false;
    }

    /// <summary>
    /// Determines whether the given visual or any of its ancestors is the pane close button.
    /// </summary>
    /// <param name="visual">The visual element to test.</param>
    /// <returns><see langword="true"/> if the visual is part of a close button; otherwise <see langword="false"/>.</returns>
    private static bool IsCloseButton(Visual? visual)
    {
        var current = visual;
        while (current != null)
        {
            if (current is Button button && button.Classes.Contains("dock-pane-close"))
                return true;
            current = current.GetVisualParent() as Visual;
        }
        return false;
    }

    /// <summary>
    /// Walks the visual tree upward to find the <see cref="DockPane"/> associated with a visual element.
    /// </summary>
    /// <param name="visual">The visual element to start from.</param>
    /// <returns>The owning <see cref="DockPane"/>, or <see langword="null"/> if not found.</returns>
    private static DockPane? FindPaneFromVisual(Visual? visual)
    {
        var current = visual;
        while (current != null)
        {
            if (current is ListBoxItem item && item.Content is DockPane pane)
                return pane;
            current = current.GetVisualParent() as Visual;
        }
        return null;
    }
}
