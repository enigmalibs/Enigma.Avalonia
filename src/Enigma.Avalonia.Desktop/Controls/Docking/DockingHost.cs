using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Enigma.Avalonia.Desktop.Controls.Docking;

/// <summary>
/// A templated control that hosts a tree of <see cref="DockTabGroup"/> and
/// <see cref="DockSplitContainer"/> controls, and manages drag-and-drop re-docking
/// of <see cref="DockPane"/> instances within the layout.
/// </summary>
public class DockingHost : TemplatedControl
{
    /// <summary>The <c>PART_RootHost</c> ContentControl that holds the root of the docking layout tree.</summary>
    private ContentControl? _rootHost;

    /// <summary>The <c>PART_DropOverlay</c> border displayed as a visual hint for the current drop target zone.</summary>
    private Border? _dropOverlay;

    /// <summary>The <c>PART_RootPanel</c> Panel used to capture global pointer events during a drag operation.</summary>
    private Panel? _rootPanel;

    /// <summary>The active drag session, or <see langword="null"/> when no drag is in progress.</summary>
    private DockDragSession? _dragSession;

    /// <summary>Gets the flat list of <see cref="DockPane"/> instances declared as content children.</summary>
    [Content]
    public AvaloniaList<DockPane> Panes { get; } = new();

    /// <summary>Defines the <see cref="LayoutRoot"/> property.</summary>
    public static readonly StyledProperty<DockLayoutNode?> LayoutRootProperty =
        AvaloniaProperty.Register<DockingHost, DockLayoutNode?>(nameof(LayoutRoot));

    /// <summary>Gets or sets the root layout model node used to build the docking UI declaratively.</summary>
    public DockLayoutNode? LayoutRoot
    {
        get => GetValue(LayoutRootProperty);
        set => SetValue(LayoutRootProperty, value);
    }

    /// <summary>
    /// Finds template parts <c>PART_RootHost</c>, <c>PART_DropOverlay</c>, and <c>PART_RootPanel</c>,
    /// wires global pointer events for drag-and-drop, then initializes the layout.
    /// </summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_rootPanel is not null)
        {
            _rootPanel.RemoveHandler(PointerMovedEvent, OnRootPointerMoved);
            _rootPanel.RemoveHandler(PointerReleasedEvent, OnRootPointerReleased);
        }

        _rootHost = e.NameScope.Find<ContentControl>("PART_RootHost");
        _dropOverlay = e.NameScope.Find<Border>("PART_DropOverlay");
        _rootPanel = e.NameScope.Find<Panel>("PART_RootPanel");

        if (_rootPanel is not null)
        {
            _rootPanel.AddHandler(PointerMovedEvent, OnRootPointerMoved, RoutingStrategies.Bubble, true);
            _rootPanel.AddHandler(PointerReleasedEvent, OnRootPointerReleased, RoutingStrategies.Bubble, true);
        }

        InitializeLayout();
    }

    /// <summary>
    /// Rebuilds the visual layout tree from the model when <see cref="LayoutRoot"/> changes.
    /// </summary>
    /// <param name="change">Details about the property that changed.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LayoutRootProperty)
        {
            BuildLayoutFromModel();
        }
    }

    /// <summary>
    /// Sets the root of the docking layout tree to the given control and wires all
    /// contained <see cref="DockTabGroup"/> instances.
    /// </summary>
    /// <param name="root">The control to use as the root of the layout.</param>
    /// <exception cref="InvalidOperationException">Thrown if called before the control template has been applied.</exception>
    public void SetRootLayout(Control root)
    {
        if (_rootHost == null)
            throw new InvalidOperationException("Cannot set layout before OnApplyTemplate");

        _rootHost.Content = root;
        WireAllGroups(root);
    }

    /// <summary>Builds the visual layout tree from the current <see cref="LayoutRoot"/> model and sets it as the root content.</summary>
    private void BuildLayoutFromModel()
    {
        if (_rootHost == null || LayoutRoot == null)
            return;

        var visualRoot = BuildVisualTree(LayoutRoot);
        if (visualRoot != null)
        {
            _rootHost.Content = visualRoot;
            WireAllGroups(visualRoot);
        }
    }

    /// <summary>Recursively converts a <see cref="DockLayoutNode"/> model into the corresponding visual control.</summary>
    /// <param name="node">The model node to convert.</param>
    /// <returns>The visual control, or <see langword="null"/> for unrecognised node types.</returns>
    private Control? BuildVisualTree(DockLayoutNode node)
    {
        return node switch
        {
            DockPaneModel paneModel => BuildPane(paneModel),
            DockTabGroupModel groupModel => BuildTabGroup(groupModel),
            DockSplitModel splitModel => BuildSplit(splitModel),
            _ => null
        };
    }

    /// <summary>Creates a <see cref="DockPane"/> from the given <see cref="DockPaneModel"/>.</summary>
    /// <param name="model">The pane model to convert.</param>
    /// <returns>A new <see cref="DockPane"/> populated with model data.</returns>
    private DockPane BuildPane(DockPaneModel model)
    {
        return new DockPane
        {
            Header = model.Header,
            PaneContent = model.Content,
            CanClose = model.CanClose,
            CanMove = model.CanMove
        };
    }

    /// <summary>Creates a <see cref="DockTabGroup"/> from the given <see cref="DockTabGroupModel"/>, building each pane and setting the initial selection.</summary>
    /// <param name="model">The tab group model to convert.</param>
    /// <returns>A new <see cref="DockTabGroup"/> populated with model data.</returns>
    private DockTabGroup BuildTabGroup(DockTabGroupModel model)
    {
        var group = new DockTabGroup();

        foreach (var paneModel in model.Panes)
        {
            var pane = BuildPane(paneModel);
            group.Panes.Add(pane);

            if (paneModel == model.SelectedPane)
                group.SelectedPane = pane;
        }

        return group;
    }

    /// <summary>Creates a <see cref="DockSplitContainer"/> from the given <see cref="DockSplitModel"/>, recursively building each child.</summary>
    /// <param name="model">The split model to convert.</param>
    /// <returns>A new <see cref="DockSplitContainer"/> populated with model data.</returns>
    private DockSplitContainer BuildSplit(DockSplitModel model)
    {
        var split = new DockSplitContainer
        {
            Orientation = model.Orientation,
            FirstSize = model.FirstSize,
            SecondSize = model.SecondSize
        };

        if (model.First != null)
            split.First = BuildVisualTree(model.First);

        if (model.Second != null)
            split.Second = BuildVisualTree(model.Second);

        return split;
    }

    /// <summary>
    /// Initializes the root layout from the model (if set), falls back to a single tab group containing all <see cref="Panes"/>,
    /// or wires existing content when layout was set programmatically before the template was applied.
    /// </summary>
    private void InitializeLayout()
    {
        if (_rootHost == null)
            return;

        // If LayoutRoot model is set, build from model
        if (LayoutRoot != null)
        {
            BuildLayoutFromModel();
            return;
        }

        // If no panes, nothing to do
        if (Panes.Count == 0)
            return;

        // Skip if layout already set programmatically
        if (_rootHost.Content != null)
        {
            WireAllGroups(_rootHost.Content as Control);
            return;
        }

        // Default: single group with all panes
        var group = new DockTabGroup();
        foreach (var pane in Panes)
            group.Panes.Add(pane);

        WireGroup(group);
        _rootHost.Content = group;
    }

    /// <summary>Recursively traverses the layout tree and wires drag and close events on every <see cref="DockTabGroup"/>.</summary>
    /// <param name="control">The root control to start traversal from.</param>
    private void WireAllGroups(Control? control)
    {
        if (control is DockTabGroup group)
        {
            WireGroup(group);
            return;
        }

        if (control is DockSplitContainer split)
        {
            if (split.First != null) WireAllGroups(split.First);
            if (split.Second != null) WireAllGroups(split.Second);
        }
    }

    /// <summary>Subscribes to drag and close events on the given <see cref="DockTabGroup"/>.</summary>
    /// <param name="group">The group to wire.</param>
    private void WireGroup(DockTabGroup group)
    {
        group.PaneDragStarted += OnPaneDragStarted;
        group.PaneCloseRequested += OnPaneCloseRequested;
    }

    /// <summary>Unsubscribes drag and close events from the given <see cref="DockTabGroup"/>.</summary>
    /// <param name="group">The group to unwire.</param>
    private void UnwireGroup(DockTabGroup group)
    {
        group.PaneDragStarted -= OnPaneDragStarted;
        group.PaneCloseRequested -= OnPaneCloseRequested;
    }

    /// <summary>Starts a drag session when a pane tab is dragged beyond the drag threshold.</summary>
    private void OnPaneDragStarted(object? sender, DockTabGroupEventArgs e)
    {
        if (_rootPanel == null || _dropOverlay == null)
            return;

        _dragSession = new DockDragSession(e.Pane, e.SourceGroup);
        _dropOverlay.IsVisible = false;

        e.Pointer?.Capture(_rootPanel);
    }

    /// <summary>
    /// Handles pointer movement during a drag session. Hit-tests tab groups, determines the drop zone,
    /// and repositions the drop overlay indicator.
    /// </summary>
    private void OnRootPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSession == null || _dropOverlay == null || _rootPanel == null)
            return;

        var position = e.GetPosition(_rootPanel);
        var targetGroup = HitTestTabGroup(position);

        if (targetGroup == null)
        {
            _dropOverlay.IsVisible = false;
            _dragSession.TargetGroup = null;
            _dragSession.TargetPosition = DockPosition.Center;
            return;
        }

        _dragSession.TargetGroup = targetGroup;

        // Determine drop zone based on pointer position relative to target
        var groupBounds = targetGroup.Bounds;
        var groupTopLeft = targetGroup.TranslatePoint(new Point(0, 0), _rootPanel);
        if (groupTopLeft == null)
        {
            _dropOverlay.IsVisible = false;
            return;
        }

        var relativePos = position - groupTopLeft.Value;
        var zone = DetermineDropZone(relativePos, groupBounds.Width, groupBounds.Height);
        _dragSession.TargetPosition = zone;

        // Position and show overlay
        ShowDropOverlay(groupTopLeft.Value, groupBounds.Width, groupBounds.Height, zone);
    }

    /// <summary>Finalises a drag session on pointer release, executing the drop if a valid target group is present.</summary>
    private void OnRootPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragSession == null)
            return;

        // Release pointer capture
        e.Pointer.Capture(null);

        var session = _dragSession;
        _dragSession = null;

        if (_dropOverlay != null)
            _dropOverlay.IsVisible = false;

        if (session.TargetGroup == null)
            return;

        ExecuteDrop(session);
    }

    /// <summary>
    /// Determines the <see cref="DockPosition"/> based on how far <paramref name="relativePos"/> is from the
    /// edges of the target group. The outer 25 % of each edge maps to a directional zone; the centre maps to <see cref="DockPosition.Center"/>.
    /// </summary>
    /// <param name="relativePos">The pointer position relative to the target group's top-left corner.</param>
    /// <param name="width">The width of the target group in canvas pixels.</param>
    /// <param name="height">The height of the target group in canvas pixels.</param>
    /// <returns>The <see cref="DockPosition"/> for the current pointer location.</returns>
    private DockPosition DetermineDropZone(Point relativePos, double width, double height)
    {
        double edgeBand = 0.25;

        double leftBand = width * edgeBand;
        double rightBand = width * (1 - edgeBand);
        double topBand = height * edgeBand;
        double bottomBand = height * (1 - edgeBand);

        if (relativePos.X < leftBand)
            return DockPosition.Left;
        if (relativePos.X > rightBand)
            return DockPosition.Right;
        if (relativePos.Y < topBand)
            return DockPosition.Top;
        if (relativePos.Y > bottomBand)
            return DockPosition.Bottom;

        return DockPosition.Center;
    }

    /// <summary>Positions and shows the drop overlay to cover the half of the target group corresponding to <paramref name="zone"/>.</summary>
    /// <param name="groupOrigin">The top-left corner of the target group in root-panel space.</param>
    /// <param name="groupWidth">The width of the target group.</param>
    /// <param name="groupHeight">The height of the target group.</param>
    /// <param name="zone">The current drop zone that determines which half is highlighted.</param>
    private void ShowDropOverlay(Point groupOrigin, double groupWidth, double groupHeight, DockPosition zone)
    {
        if (_dropOverlay == null)
            return;

        double x = groupOrigin.X;
        double y = groupOrigin.Y;
        double w = groupWidth;
        double h = groupHeight;

        switch (zone)
        {
            case DockPosition.Left:
                w = groupWidth * 0.5;
                break;
            case DockPosition.Right:
                x += groupWidth * 0.5;
                w = groupWidth * 0.5;
                break;
            case DockPosition.Top:
                h = groupHeight * 0.5;
                break;
            case DockPosition.Bottom:
                y += groupHeight * 0.5;
                h = groupHeight * 0.5;
                break;
        }

        _dropOverlay.Width = w;
        _dropOverlay.Height = h;
        _dropOverlay.RenderTransform = new TranslateTransform(x, y);
        _dropOverlay.IsVisible = true;
    }

    /// <summary>
    /// Executes the pending drop: moves <paramref name="session"/>.Pane from its source group to the target,
    /// either adding it as a tab (center drop) or splitting the target group (directional drop).
    /// </summary>
    /// <param name="session">The completed drag session containing pane, source, target, and position.</param>
    private void ExecuteDrop(DockDragSession session)
    {
        var pane = session.Pane;
        var sourceGroup = session.SourceGroup;
        var targetGroup = session.TargetGroup!;
        var position = session.TargetPosition;

        // Drop on own group at center → no-op
        if (sourceGroup == targetGroup && position == DockPosition.Center)
            return;

        // Drop on own group at edge with single pane → no-op
        if (sourceGroup == targetGroup && sourceGroup.Panes.Count <= 1)
            return;

        // Switch selection away first so the ContentPresenter releases the
        // pane from the logical tree before we re-parent it.
        if (sourceGroup.SelectedPane == pane)
        {
            var idx = sourceGroup.Panes.IndexOf(pane);
            sourceGroup.SelectedPane = sourceGroup.Panes.Count > 1
                ? sourceGroup.Panes[idx == 0 ? 1 : idx - 1]
                : null;
        }

        sourceGroup.Panes.Remove(pane);

        if (position == DockPosition.Center)
        {
            // Move to target group as new tab
            targetGroup.Panes.Add(pane);
            targetGroup.SelectedPane = pane;
        }
        else
        {
            // Split target group
            SplitGroup(targetGroup, pane, position);
        }

        // Collapse empty groups
        if (sourceGroup.Panes.Count == 0)
            CollapseEmptyGroup(sourceGroup);
    }

    /// <summary>
    /// Splits <paramref name="targetGroup"/> by inserting a new <see cref="DockTabGroup"/> containing <paramref name="pane"/>
    /// on the side indicated by <paramref name="position"/>, wrapping both in a new <see cref="DockSplitContainer"/>.
    /// </summary>
    private void SplitGroup(DockTabGroup targetGroup, DockPane pane, DockPosition position)
    {
        if (_rootHost == null)
            return;

        var newGroup = new DockTabGroup();
        newGroup.Panes.Add(pane);
        newGroup.SelectedPane = pane;
        WireGroup(newGroup);

        var orientation = position is DockPosition.Left or DockPosition.Right
            ? Orientation.Horizontal
            : Orientation.Vertical;

        var split = new DockSplitContainer { Orientation = orientation };

        bool newIsFirst = position is DockPosition.Left or DockPosition.Top;

        if (newIsFirst)
        {
            split.First = newGroup;
            split.Second = targetGroup;
        }
        else
        {
            split.First = targetGroup;
            split.Second = newGroup;
        }

        ReplaceInParent(targetGroup, split);
    }

    /// <summary>
    /// Removes an empty <see cref="DockTabGroup"/> from the layout tree. If it was the root content it is simply cleared;
    /// otherwise the parent <see cref="DockSplitContainer"/> is replaced by its surviving child.
    /// </summary>
    private void CollapseEmptyGroup(DockTabGroup emptyGroup)
    {
        if (_rootHost == null)
            return;

        UnwireGroup(emptyGroup);

        // If the empty group is the root content
        if (_rootHost.Content == emptyGroup)
        {
            _rootHost.Content = null;
            return;
        }

        // Find parent DockSplitContainer
        var parent = FindParentSplit(emptyGroup);
        if (parent == null)
            return;

        // Get the surviving child
        Control? survivor = null;
        if (parent.First == emptyGroup)
            survivor = parent.Second;
        else if (parent.Second == emptyGroup)
            survivor = parent.First;

        if (survivor == null)
            return;

        // Detach survivor from the split
        parent.First = null;
        parent.Second = null;

        // Replace the split with the survivor
        ReplaceInParent(parent, survivor);
    }

    /// <summary>Replaces <paramref name="target"/> with <paramref name="replacement"/> in the layout tree, either at the root or inside its parent split container.</summary>
    private void ReplaceInParent(Control target, Control replacement)
    {
        if (_rootHost == null)
            return;

        if (_rootHost.Content == target)
        {
            _rootHost.Content = replacement;
            return;
        }

        var parent = FindParentSplit(target);
        if (parent == null)
            return;

        if (parent.First == target)
            parent.First = replacement;
        else if (parent.Second == target)
            parent.Second = replacement;
    }

    /// <summary>Finds the <see cref="DockSplitContainer"/> that directly contains <paramref name="child"/> in the layout tree.</summary>
    /// <param name="child">The control whose parent split container is sought.</param>
    /// <returns>The parent <see cref="DockSplitContainer"/>, or <see langword="null"/> if not found.</returns>
    private DockSplitContainer? FindParentSplit(Control child)
    {
        if (_rootHost?.Content is not Control root)
            return null;

        return FindParentSplitRecursive(root, child);
    }

    /// <summary>Recursively searches the layout tree for the <see cref="DockSplitContainer"/> that directly contains <paramref name="target"/>.</summary>
    /// <param name="current">The current node being examined.</param>
    /// <param name="target">The control to find a parent for.</param>
    /// <returns>The containing split container, or <see langword="null"/> if not found.</returns>
    private DockSplitContainer? FindParentSplitRecursive(Control current, Control target)
    {
        if (current is DockSplitContainer split)
        {
            if (split.First == target || split.Second == target)
                return split;

            if (split.First != null)
            {
                var result = FindParentSplitRecursive(split.First, target);
                if (result != null) return result;
            }

            if (split.Second != null)
            {
                var result = FindParentSplitRecursive(split.Second, target);
                if (result != null) return result;
            }
        }

        return null;
    }

    /// <summary>Returns the <see cref="DockTabGroup"/> whose bounds contain <paramref name="position"/> in root-panel space, or <see langword="null"/> if none.</summary>
    /// <param name="position">The position in root-panel space to test.</param>
    private DockTabGroup? HitTestTabGroup(Point position)
    {
        if (_rootPanel == null)
            return null;

        var groups = new List<DockTabGroup>();
        CollectTabGroups(_rootHost?.Content as Control, groups);

        foreach (var group in groups)
        {
            var topLeft = group.TranslatePoint(new Point(0, 0), _rootPanel);
            if (topLeft == null) continue;

            var bounds = new Rect(topLeft.Value, group.Bounds.Size);
            if (bounds.Contains(position))
                return group;
        }

        return null;
    }

    /// <summary>Recursively collects all <see cref="DockTabGroup"/> instances in the layout tree into <paramref name="groups"/>.</summary>
    private void CollectTabGroups(Control? control, List<DockTabGroup> groups)
    {
        if (control is DockTabGroup group)
        {
            groups.Add(group);
            return;
        }

        if (control is DockSplitContainer split)
        {
            if (split.First != null) CollectTabGroups(split.First, groups);
            if (split.Second != null) CollectTabGroups(split.Second, groups);
        }
    }

    /// <summary>
    /// Finds the group containing the given pane, removes it, and collapses the group if it becomes empty.
    /// </summary>
    /// <param name="pane">The pane to close.</param>
    public void ClosePane(DockPane pane)
    {
        if (_rootHost?.Content == null)
            return;

        var groups = new List<DockTabGroup>();
        CollectTabGroups(_rootHost.Content as Control, groups);

        foreach (var group in groups)
        {
            if (group.Panes.Contains(pane))
            {
                if (group.SelectedPane == pane)
                {
                    var idx = group.Panes.IndexOf(pane);
                    group.SelectedPane = group.Panes.Count > 1
                        ? group.Panes[idx == 0 ? 1 : idx - 1]
                        : null;
                }

                group.Panes.Remove(pane);

                if (group.Panes.Count == 0)
                    CollapseEmptyGroup(group);

                return;
            }
        }
    }

    /// <summary>Handles a close request from a tab group by forwarding to <see cref="ClosePane"/>.</summary>
    private void OnPaneCloseRequested(object? sender, DockTabGroupEventArgs e)
    {
        ClosePane(e.Pane);
    }

    /// <summary>Encapsulates the state of an in-progress pane drag operation within the docking host.</summary>
    private class DockDragSession
    {
        /// <summary>Gets the pane being dragged.</summary>
        public DockPane Pane { get; }

        /// <summary>Gets the group from which the pane originated.</summary>
        public DockTabGroup SourceGroup { get; }

        /// <summary>Gets or sets the group currently under the pointer, or <see langword="null"/> if none.</summary>
        public DockTabGroup? TargetGroup { get; set; }

        /// <summary>Gets or sets the drop position relative to the target group.</summary>
        public DockPosition TargetPosition { get; set; } = DockPosition.Center;

        /// <summary>
        /// Initializes a new drag session for the given pane and source group.
        /// </summary>
        /// <param name="pane">The pane being dragged.</param>
        /// <param name="sourceGroup">The group that owns the pane at the start of the drag.</param>
        public DockDragSession(DockPane pane, DockTabGroup sourceGroup)
        {
            Pane = pane;
            SourceGroup = sourceGroup;
        }
    }
}
