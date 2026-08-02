# Docking

`Enigma.Avalonia.Desktop` provides an IDE-style docking workspace through one templated host,
`DockingHost`, which owns a mutable tree of three control types: `DockSplitContainer` (two children
and a draggable splitter), `DockTabGroup` (a tab strip over a set of panes) and `DockPane` (a header
plus one content object). The host supplies the drag-and-drop: a user pulls a pane's tab out of one
group and drops it onto another to tab it, or onto a group's edge to split that group.

The idiom is to describe the *initial* layout as data: build a tree of `DockLayoutNode` objects —
`DockSplitModel`, `DockTabGroupModel`, `DockPaneModel` — and bind it to `DockingHost.LayoutRoot`. Two
alternatives cover the simple and the fully imperative cases: declare `DockPane` children directly on
the host in XAML (they become a single tab group), or build controls yourself and pass the root to
`SetRootLayout`.

The model tree is a one-way seed. `DockingHost` reads `LayoutRoot` once per assignment, builds
controls from it, and from then on the live layout *is* the control tree — nothing is written back,
and re-assigning `LayoutRoot` rebuilds from scratch, discarding whatever the user dragged. Pane
content is typed `object?`, so anything that is not a `Control` is rendered by whichever
`DataTemplate` in your application matches it.

## Layout operations

| Operation | How it is triggered | Result |
|-----------|--------------------|--------|
| Tab a pane into a group | Drop on the target group's centre band | Pane is appended to the target's `Panes` and selected |
| Split a group | Drop within the outer 25 % of a target group's edge | Target is wrapped in a new `DockSplitContainer` beside a new one-pane `DockTabGroup` |
| Resize | Drag the splitter between two children of a `DockSplitContainer` | Grid column/row sizes change; the model is untouched |
| Close a pane | Click the tab's close button, or call `DockingHost.ClosePane` | Pane is removed from its group and dropped |
| Collapse a group | Automatic, when a group's last pane leaves | The group's parent split is replaced by the surviving sibling |
| Pin a pane | `CanMove="False"` / `CanClose="False"` on the pane | Drag start is refused / the close button is hidden |

Drop zones are computed per target group: the outer 25 % of each edge is a directional zone, the rest
is `DockPosition.Center`. Horizontal edges are tested first, so a corner resolves to `Left` or
`Right`, never `Top` or `Bottom`. A directional drop creates a `DockSplitContainer` oriented
`Horizontal` for `Left`/`Right` and `Vertical` for `Top`/`Bottom`, with the incoming group as `First`
for `Left`/`Top` and `Second` for `Right`/`Bottom`, always at the default `1*` / `1*` sizes. Two
gestures are deliberate no-ops: dropping a pane on its own group's centre, and dropping the only pane
of a group onto that same group's edge. A drag begins after 5 px of left-button movement over a tab,
and one released over nothing simply ends; nothing ever floats out into its own window.

`DockPosition` is the vocabulary of that behaviour — the host computes one per pointer move — but no
public member accepts one: docking a pane from code means building the tree yourself.

## Key types

| Type | Namespace | Role |
|------|-----------|------|
| `DockingHost` | `Enigma.Avalonia.Desktop.Controls.Docking` | Templated control owning the layout tree and the drag/drop machinery. |
| `DockPane` | `Enigma.Avalonia.Desktop.Controls.Docking` | One dockable unit: header, content, close/move flags. |
| `DockTabGroup` | `Enigma.Avalonia.Desktop.Controls.Docking` | Tab strip over a set of panes; raises the drag and close events. |
| `DockSplitContainer` | `Enigma.Avalonia.Desktop.Controls.Docking` | Two children separated by a `GridSplitter`. |
| `DockLayoutNode` | `Enigma.Avalonia.Desktop.Controls.Docking` | Abstract base of the layout model; no members of its own. |
| `DockPaneModel` | `Enigma.Avalonia.Desktop.Controls.Docking` | Node for a pane: `Header` (`string`), `Content` (`object?`), `CanClose`/`CanMove` (`bool`, `true`). |
| `DockTabGroupModel` | `Enigma.Avalonia.Desktop.Controls.Docking` | Node for a group: `Panes` (`AvaloniaList<DockPaneModel>`), `SelectedPane` (`DockPaneModel?`). |
| `DockSplitModel` | `Enigma.Avalonia.Desktop.Controls.Docking` | Node for a split: `Orientation`, `First`/`Second` (`DockLayoutNode?`), `FirstSize`/`SecondSize` (`GridLength`, `1*`). |
| `DockPosition` | `Enigma.Avalonia.Desktop.Controls.Docking` | Enum: `Center`, `Left`, `Right`, `Top`, `Bottom`. |
| `DockTabGroupEventArgs` | `Enigma.Avalonia.Desktop.Controls.Docking` | Event payload: `Pane` (`DockPane`), `SourceGroup` (`DockTabGroup`), `Pointer` (`IPointer?`). |

Control surface:

| Owner | Member | Type | Default | Notes |
|-------|--------|------|---------|-------|
| `DockingHost` | `Panes` | `AvaloniaList<DockPane>` | empty | XAML content property. Read only when `LayoutRoot` is null and no root has been set. |
| `DockingHost` | `LayoutRoot` | `DockLayoutNode?` | `null` | Styled property; assigning it rebuilds the whole tree. |
| `DockingHost` | `SetRootLayout(Control)` | `void` | — | Replaces the tree with a control you built. Throws `InvalidOperationException` before the template is applied. |
| `DockingHost` | `ClosePane(DockPane)` | `void` | — | Finds the pane's group, removes it, collapses the group if it empties. |
| `DockPane` | `Header` | `string?` | `null` | Tab label. |
| `DockPane` | `PaneContent` | `object?` | `null` | XAML content property. |
| `DockPane` | `CanClose` | `bool` | `true` | `false` hides the tab's close button. |
| `DockPane` | `CanMove` | `bool` | `true` | `false` refuses to start a drag. |
| `DockTabGroup` | `Panes` | `AvaloniaList<DockPane>` | empty | *Not* a content property — use property-element syntax in XAML. |
| `DockTabGroup` | `SelectedPane` | `DockPane?` | first pane, on template applied | Styled property, `BindingMode.TwoWay` by default. |
| `DockTabGroup` | `PaneDragStarted` | `EventHandler<DockTabGroupEventArgs>` | — | Raised once the 5 px threshold is crossed. |
| `DockTabGroup` | `PaneCloseRequested` | `EventHandler<DockTabGroupEventArgs>` | — | Raised on close-button click, only for panes with `CanClose`. |
| `DockSplitContainer` | `First`, `Second` | `Control?` | `null` | Left/top and right/bottom children. |
| `DockSplitContainer` | `Orientation` | `Orientation` | `Horizontal` | Also drives the `:horizontal` / `:vertical` pseudo-classes. |
| `DockSplitContainer` | `FirstSize`, `SecondSize` | `GridLength` | `1*` | Initial sizes; the splitter overwrites them as the user drags. |

## Usage

### A single group of tabbed panes

`DockingHost.Panes` is the XAML content property, so panes written as children of the host end up in
one tab group — the whole layout API for a workspace that starts as a plain set of tabs. The user can
still split it apart by dragging.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:docking="using:Enigma.Avalonia.Desktop.Controls.Docking"
             x:Class="MyApp.Views.QuickWorkspaceView">

  <docking:DockingHost>
    <docking:DockPane Header="Explorer" CanClose="False">
      <TextBlock Margin="12" Text="Project tree" />
    </docking:DockPane>
    <docking:DockPane Header="Document">
      <TextBlock Margin="12" Text="Editor surface" />
    </docking:DockPane>
  </docking:DockingHost>

</UserControl>
```

### Describing a layout as a model

Anything more structured than one tab group is described with `DockLayoutNode`s. The ViewModel below
builds a fixed-width tree on the left, documents in the centre and a tool group along the bottom
without touching a control, which is what lets the initial layout come from settings. Assigning a
fresh tree to the settable property is a "reset window layout" operation.

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Enigma.Avalonia.Desktop.Controls.Docking;

namespace MyApp.ViewModels;

// Whatever a pane displays; the view supplies the DataTemplate that renders it.
public sealed record PaneContent(string Title, string Description);

public class WorkspaceViewModel : ObservableObject
{
    private DockLayoutNode _rootLayout;

    public WorkspaceViewModel() => _rootLayout = BuildLayout();

    public DockLayoutNode RootLayout
    {
        get => _rootLayout;
        set => SetProperty(ref _rootLayout, value);
    }

    private static DockLayoutNode BuildLayout() =>
        new DockSplitModel
        {
            Orientation = Orientation.Vertical,
            SecondSize = new GridLength(180, GridUnitType.Pixel),
            First = new DockSplitModel
            {
                Orientation = Orientation.Horizontal,
                FirstSize = new GridLength(200, GridUnitType.Pixel),
                First = Group(Pane("Explorer", "The project tree.", canClose: false, canMove: false)),
                Second = Group(
                    Pane("Doc 1", "Drag this tab onto a group's edge to split that group."),
                    Pane("Doc 2", "Drag it onto a group's centre to join that group's tabs.")),
            },
            Second = Group(Pane("Output", "Build output.")),
        };

    private static DockPaneModel Pane(string header, string text, bool canClose = true, bool canMove = true) =>
        new() { Header = header, Content = new PaneContent(header, text), CanClose = canClose, CanMove = canMove };

    private static DockTabGroupModel Group(params DockPaneModel[] panes)
    {
        var group = new DockTabGroupModel { SelectedPane = panes[0] };
        foreach (var pane in panes)
            group.Panes.Add(pane);
        return group;
    }
}
```

The view binds the root and supplies the `DataTemplate` for the content type; without a matching
template a pane renders as `ToString()`.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:docking="using:Enigma.Avalonia.Desktop.Controls.Docking"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.WorkspaceView"
             x:DataType="vm:WorkspaceViewModel">

  <UserControl.DataTemplates>
    <DataTemplate DataType="vm:PaneContent" x:DataType="vm:PaneContent">
      <StackPanel Margin="16" Spacing="8">
        <TextBlock Text="{Binding Title}" FontWeight="SemiBold" />
        <TextBlock Text="{Binding Description}" TextWrapping="Wrap" />
      </StackPanel>
    </DataTemplate>
  </UserControl.DataTemplates>

  <docking:DockingHost LayoutRoot="{Binding RootLayout}" />

</UserControl>
```

### Building the tree from code, closing panes, observing drags

`SetRootLayout` takes a control you assembled yourself and makes it the root of the tree, wiring every
`DockTabGroup` it can reach. Use it when you need references to the live `DockPane` instances — that
is the only way to address a pane from code once the user has dragged it elsewhere. Groups you build
can also be observed: the host subscribes to `PaneDragStarted` and `PaneCloseRequested` on them and
extra handlers are additive, but groups the host creates while splitting are wired by the host alone.

```csharp
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Enigma.Avalonia.Desktop.Controls.Docking;

namespace MyApp.Views;

public class CodeBuiltWorkspaceView : UserControl
{
    private readonly DockingHost _host = new();
    private readonly DockPane _output = Pane("Output", "Build output", canClose: false);

    public CodeBuiltWorkspaceView()
    {
        Content = _host;

        // SetRootLayout needs the host's template parts, so it cannot run any earlier than Loaded.
        _host.Loaded += OnHostLoaded;
    }

    // Closes the pane wherever the user has since dragged it to; CanClose does not apply here.
    public void CloseOutput() => _host.ClosePane(_output);

    private static DockPane Pane(string header, string text, bool canClose = true) =>
        new() { Header = header, CanClose = canClose, PaneContent = new TextBlock { Text = text } };

    private static DockTabGroup Group(params DockPane[] panes)
    {
        var group = new DockTabGroup { SelectedPane = panes[0] };
        foreach (var pane in panes)
            group.Panes.Add(pane);
        return group;
    }

    private void OnHostLoaded(object? sender, RoutedEventArgs e)
    {
        _host.Loaded -= OnHostLoaded;
        var documents = Group(Pane("Doc 1", "First document"), Pane("Doc 2", "Second document"));
        documents.PaneDragStarted += OnPaneDragStarted;
        documents.PaneCloseRequested += OnPaneCloseRequested;

        _host.SetRootLayout(new DockSplitContainer
        {
            Orientation = Orientation.Vertical,
            First = documents,
            Second = Group(_output),
            SecondSize = new GridLength(180, GridUnitType.Pixel),
        });
    }

    private static void OnPaneDragStarted(object? sender, DockTabGroupEventArgs e) =>
        Console.WriteLine($"{e.Pane.Header} dragged out of {e.SourceGroup.Panes.Count} tabs, pointer {e.Pointer?.Id}");

    private static void OnPaneCloseRequested(object? sender, DockTabGroupEventArgs e) =>
        Console.WriteLine($"{e.Pane.Header} close requested from {e.SourceGroup.Panes.Count} tabs");
}
```

### Restyling the docking chrome

Each of the four controls has a `ControlTheme` keyed by `{x:Type}`, so any of them can be retemplated
by declaring a theme with the same key. Short of that, the chrome is driven by dynamic brush
resources, which can be overridden anywhere above the host — including on the host itself, where the
override reaches every split and group in the tree. See [theming](theming.md) for the resource set.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:docking="using:Enigma.Avalonia.Desktop.Controls.Docking"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.ThemedWorkspaceView"
             x:DataType="vm:WorkspaceViewModel">

  <docking:DockingHost LayoutRoot="{Binding RootLayout}">
    <docking:DockingHost.Resources>
      <!-- The splitter brush; EnigmaSurfaceBrush recolours the tab strip the same way. -->
      <SolidColorBrush x:Key="EnigmaBorderSubtleBrush" Color="#FF6E6E6E" />
    </docking:DockingHost.Resources>
  </docking:DockingHost>

</UserControl>
```

Retemplating targets these parts:

| Control | Part | Type | Role |
|---------|------|------|------|
| `DockingHost` | `PART_RootPanel` | `Panel` | Captures the pointer for the duration of a drag. |
| `DockingHost` | `PART_RootHost` | `ContentControl` | Holds the root of the layout tree. |
| `DockingHost` | `PART_DropOverlay` | `Border` | Drop hint; sized and placed by the host through a `TranslateTransform`. |
| `DockTabGroup` | `PART_TabStrip` | `ListBox` | Tab headers. Its item template carries the `Button` with class `dock-pane-close`. |
| `DockSplitContainer` | `PART_Grid` | `Grid` | Column/row definitions are rebuilt on every orientation or size change. |
| `DockSplitContainer` | `PART_First`, `PART_Second` | `ContentControl` | Hosts for `First` and `Second`. |
| `DockSplitContainer` | `PART_Splitter` | `GridSplitter` | Resize handle; its `ResizeDirection` follows `Orientation`. |

`DockPane` has no named parts — its template is a single `ContentPresenter` over `PaneContent`.

## Notes

- There is no layout persistence API and no floating windows. To save a layout you serialise your own
  representation and rebuild a `DockLayoutNode` tree from it on startup; the live control tree is
  never projected back. `GridLength` is not JSON-friendly, so persist a number plus a unit and
  reconstruct with `new GridLength(value, GridUnitType.Star)`.
- The layout tree must consist solely of `DockSplitContainer`, `DockTabGroup` and `DockPane`.
  Hit-testing, event wiring and empty-group collapse traverse `DockSplitContainer.First` / `Second`
  only, so a group wrapped in a `Border` or `Grid` is invisible to the docking machinery — its tabs
  still render, but drops and closes are ignored.
- `DockingHost.Panes` is consulted once, when the template is applied; adding to it later has no
  effect. Collapsing a group discards the sizes of the split it removes — the surviving sibling
  inherits the slot, not the proportions — and a directional drop always splits 50/50.
- `CanClose` and `CanMove` govern user gestures only. `ClosePane` closes a pane whose `CanClose` is
  `false`, and `SetRootLayout` places a pane whose `CanMove` is `false`. Closing drops the pane:
  nothing retains it and there is no reopen surface, so keep a reference to any `DockPane` you intend
  to bring back and rebuild the tree with it.
- A `DockTabGroup` also works outside a host: it renders its tabs and raises both events, but nothing
  re-docks and removing the pane is left to you — move `SelectedPane` off it first, so the tab strip
  never points at a pane the collection no longer holds.
- A `DockTabGroup` presents `SelectedPane.PaneContent`, not the `DockPane` control — the pane is only
  ever data for the tab strip. That keeps it out of two logical trees at once while it is dragged,
  and it is why a non-`Control` content object needs a `DataTemplate`.
- The drop overlay's colours are baked into the `DockingHost` template rather than exposed as
  resources; changing them means supplying your own `ControlTheme` for `DockingHost`.
