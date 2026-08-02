# Ribbon

`Enigma.Avalonia.Desktop` provides a tabbed command surface through seven controls that nest in a
fixed order: a `Ribbon` holds `RibbonTab` instances, each tab holds `RibbonGroup` instances, and each
group holds the controls the user clicks — `RibbonButton`, `RibbonToggleButton` and
`RibbonDropDownButton`, the last of which carries `RibbonMenuItem` entries in its popup. All seven
live in `Enigma.Avalonia.Desktop.Controls.Ribbon`, reached from XAML with
`xmlns:ribbon="using:Enigma.Avalonia.Desktop.Controls.Ribbon"`.

Every container exposes its children as a get-only `AvaloniaList<T>` marked `[Content]`, so a ribbon
is written as one nested XAML literal. There is no `ItemsSource` anywhere in the family — a ribbon
whose tabs come from data is built by adding to those collections from code, not by binding a source
to them.

Nothing in the family raises a `Click` event. `RibbonButton`, `RibbonToggleButton` and
`RibbonMenuItem` each expose `Command` and `CommandParameter`, and the buttons execute on pointer
*press* rather than release, so an `ICommand` — guarded by `CanExecute` where it matters — is the
only way to respond to a ribbon control.

## Controls

| Control | Contains | Purpose |
|---------|----------|---------|
| `Ribbon` | `Tabs` — `RibbonTab` | The whole surface: a tab strip over the selected tab's content. Dock it to the top of a `DockPanel`. |
| `RibbonTab` | `Groups` — `RibbonGroup` | One page of commands. `Header` is its text in the tab strip. |
| `RibbonGroup` | `Items` — any `Control` | A cluster laid out in a horizontal `StackPanel`, with `Header` rendered beneath it. |
| `RibbonButton` | — | One-shot action. Executes `Command` with `CommandParameter` on pointer press. |
| `RibbonToggleButton` | — | Two-state action. Flips `IsChecked`, then executes `Command`. Gains `:checked`. |
| `RibbonDropDownButton` | `Items` — `RibbonMenuItem` | Opens a light-dismiss popup of menu items. Toggles `IsDropDownOpen` on press. |
| `RibbonMenuItem` | — | One popup entry. Not a `Control` — the drop-down renders it from its own item template. |

## Key types

| Type | Namespace | Role |
|------|-----------|------|
| `Ribbon` | `Enigma.Avalonia.Desktop.Controls.Ribbon` | `TemplatedControl`. Root of the family; owns tab selection. |
| `RibbonTab` | `Enigma.Avalonia.Desktop.Controls.Ribbon` | `TemplatedControl`. A tab page. |
| `RibbonGroup` | `Enigma.Avalonia.Desktop.Controls.Ribbon` | `TemplatedControl`. A labelled cluster of arbitrary controls. |
| `RibbonButton` | `Enigma.Avalonia.Desktop.Controls.Ribbon` | `TemplatedControl`. Icon + label + `ICommand`. |
| `RibbonToggleButton` | `Enigma.Avalonia.Desktop.Controls.Ribbon` | `TemplatedControl`. As above, plus two-way `IsChecked`. |
| `RibbonDropDownButton` | `Enigma.Avalonia.Desktop.Controls.Ribbon` | `TemplatedControl`. Icon + label + popup menu. |
| `RibbonMenuItem` | `Enigma.Avalonia.Desktop.Controls.Ribbon` | `AvaloniaObject`. The data for one popup entry. |

Every member below is a styled property, so all of them bind:

| Control | Member | Type | Notes |
|---------|--------|------|-------|
| `Ribbon` | `Tabs` | `AvaloniaList<RibbonTab>` | Content property, get-only. The strip binds it, so mutations show at once. |
| `Ribbon` | `SelectedTab` | `RibbonTab?` | Two-way by default. `null` until the template is applied. |
| `Ribbon` | `SelectedIndex` | `int` | Default `0`, two-way by default. Out-of-range writes are ignored. |
| `RibbonTab` | `Header` | `string?` | Text shown in the tab strip. |
| `RibbonTab` | `Groups` | `AvaloniaList<RibbonGroup>` | Content property, get-only. |
| `RibbonGroup` | `Header` | `string?` | Caption under the group. Hidden when null or empty. |
| `RibbonGroup` | `Items` | `AvaloniaList<Control>` | Content property, get-only. Accepts any `Avalonia.Controls.Control`. |
| `RibbonButton` | `Header` | `string?` | Label under the icon. Hidden when null or empty. |
| `RibbonButton` | `IconData` | `Geometry?` | Drawn by a 24×24 `PathIcon`. Hidden when null. |
| `RibbonButton` | `Command`, `CommandParameter` | `ICommand?`, `object?` | Executed on pointer press when `CanExecute(CommandParameter)` is true. |
| `RibbonToggleButton` | `Header`, `IconData` | `string?`, `Geometry?` | As `RibbonButton`. |
| `RibbonToggleButton` | `IsChecked` | `bool` | Two-way by default. Drives the `:checked` pseudo-class. |
| `RibbonToggleButton` | `Command`, `CommandParameter` | `ICommand?`, `object?` | Executed *after* `IsChecked` has been flipped. |
| `RibbonDropDownButton` | `Header`, `IconData` | `string?`, `Geometry?` | As `RibbonButton`. |
| `RibbonDropDownButton` | `IsDropDownOpen` | `bool` | Two-way by default. Toggled on pointer press. |
| `RibbonDropDownButton` | `Items` | `AvaloniaList<RibbonMenuItem>` | Content property, get-only. |
| `RibbonMenuItem` | `Header`, `IconData` | `string?`, `Geometry?` | Entry label and its 14×14 `PathIcon`; the icon is hidden when null. |
| `RibbonMenuItem` | `Command`, `CommandParameter` | `ICommand?`, `object?` | Invoked by the entry's button. |

Template parts and pseudo-classes are the styling hooks; see [theming](theming.md) for the brush keys
the default templates resolve.

| Control | Template parts | Pseudo-classes |
|---------|----------------|----------------|
| `Ribbon` | `PART_TabStrip` (`ListBox`) | — |
| `RibbonButton` | `PART_Root` (`Border`) | `:pointerover`, `:pressed` |
| `RibbonToggleButton` | `PART_Root` (`Border`) | `:pointerover`, `:pressed`, `:checked` |
| `RibbonDropDownButton` | `PART_Root` (`Border`), `PART_Popup` (`Popup`) | `:pointerover`, `:pressed` |

## Usage

### A ribbon with tabs and groups

`IconData` is a `Geometry`, and XAML converts SVG path syntax to one, so no icon package is required —
any source of `Geometry` works. `RibbonGroup.Items` takes any `Control`, so standard Avalonia controls
sit beside the ribbon buttons.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ribbon="using:Enigma.Avalonia.Desktop.Controls.Ribbon"
        x:Class="MyApp.Views.MainWindow">

  <DockPanel>
    <ribbon:Ribbon DockPanel.Dock="Top">

      <ribbon:RibbonTab Header="Home">
        <ribbon:RibbonGroup Header="File">
          <ribbon:RibbonButton Header="New" IconData="M6 2h8l4 4v16H6z" />
          <ribbon:RibbonButton Header="Open" IconData="M3 6h6l2 2h10v11H3z" />
        </ribbon:RibbonGroup>
        <ribbon:RibbonGroup Header="Font">
          <ComboBox Width="140" SelectedIndex="0" VerticalAlignment="Center">
            <ComboBoxItem>Inter</ComboBoxItem>
            <ComboBoxItem>Cascadia Mono</ComboBoxItem>
          </ComboBox>
          <NumericUpDown Width="90" Value="12" Minimum="6" Maximum="72"
                         VerticalAlignment="Center" />
        </ribbon:RibbonGroup>
      </ribbon:RibbonTab>

      <ribbon:RibbonTab Header="Insert">
        <ribbon:RibbonGroup Header="Elements">
          <ribbon:RibbonButton Header="Table" IconData="M3 4h18v16H3z" />
        </ribbon:RibbonGroup>
      </ribbon:RibbonTab>

    </ribbon:Ribbon>

    <TextBlock Margin="24" Text="Document content" />
  </DockPanel>

</Window>
```

### The ViewModel behind a ribbon

One command shared by every one-shot control and identified through `CommandParameter`; a dedicated
command per toggle, because a toggle also owns state the ribbon binds back to.

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public class DocumentRibbonViewModel : ObservableObject
{
    public DocumentRibbonViewModel()
    {
        RunActionCommand = new RelayCommand<string?>(OnRunAction);
        ToggleBoldCommand = new RelayCommand(OnToggleBold);
    }

    /// <summary>Bound to <c>Ribbon.SelectedIndex</c>.</summary>
    public int SelectedTabIndex { get; set => SetProperty(ref field, value); }

    public string StatusText { get; set => SetProperty(ref field, value); } = "Ready";

    /// <summary>Bound to the Bold toggle's <c>IsChecked</c>.</summary>
    public bool IsBold { get; set => SetProperty(ref field, value); }

    /// <summary>Bound to <c>IsDropDownOpen</c> so a selection can close the menu.</summary>
    public bool IsExportMenuOpen { get; set => SetProperty(ref field, value); }

    public IRelayCommand<string?> RunActionCommand { get; }

    public IRelayCommand ToggleBoldCommand { get; }

    private void OnRunAction(string? action)
    {
        StatusText = action ?? "Unnamed action";
        IsExportMenuOpen = false;
    }

    // IsChecked is written before the command runs, so read it rather than flipping it here.
    private void OnToggleBold() => StatusText = IsBold ? "Bold on" : "Bold off";
}
```

### Commands, toggles and drop-down menus

`SelectedIndex`, `IsChecked` and `IsDropDownOpen` all register `BindingMode.TwoWay` as their default
binding mode, so a plain `{Binding …}` on them already writes back.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ribbon="using:Enigma.Avalonia.Desktop.Controls.Ribbon"
        xmlns:vm="using:MyApp.ViewModels"
        x:Class="MyApp.Views.MainWindow"
        x:DataType="vm:DocumentRibbonViewModel">

  <Window.DataContext>
    <vm:DocumentRibbonViewModel />
  </Window.DataContext>

  <DockPanel>
    <ribbon:Ribbon DockPanel.Dock="Top" SelectedIndex="{Binding SelectedTabIndex}">
      <ribbon:RibbonTab Header="Home">

        <ribbon:RibbonGroup Header="File">
          <ribbon:RibbonButton Header="Save"
                               IconData="M5 3h11l3 3v15H5z"
                               Command="{Binding RunActionCommand}"
                               CommandParameter="Saved" />
        </ribbon:RibbonGroup>

        <ribbon:RibbonGroup Header="Format">
          <ribbon:RibbonToggleButton Header="Bold"
                                     IconData="M7 4h6a4 4 0 0 1 0 8H7z"
                                     IsChecked="{Binding IsBold}"
                                     Command="{Binding ToggleBoldCommand}" />
        </ribbon:RibbonGroup>

        <ribbon:RibbonGroup Header="Export">
          <!-- RibbonMenuItem is not a Control and has no DataContext of its own: its bindings
               resolve against the DataContext of the scope that declares it — this window. -->
          <ribbon:RibbonDropDownButton Header="Export As"
                                       IconData="M11 3h2v9h4l-5 5-5-5h4zM4 19h16v2H4z"
                                       IsDropDownOpen="{Binding IsExportMenuOpen}">
            <ribbon:RibbonMenuItem Header="PDF"
                                   IconData="M6 2h8l4 4v16H6z"
                                   Command="{Binding RunActionCommand}"
                                   CommandParameter="Exported as PDF" />
            <ribbon:RibbonMenuItem Header="CSV"
                                   Command="{Binding RunActionCommand}"
                                   CommandParameter="Exported as CSV" />
          </ribbon:RibbonDropDownButton>
        </ribbon:RibbonGroup>

      </ribbon:RibbonTab>
    </ribbon:Ribbon>

    <TextBlock Margin="24" Text="{Binding StatusText}" />
  </DockPanel>

</Window>
```

### Building and driving a ribbon from code

The collections have no `ItemsSource`, so a ribbon whose shape depends on data is assembled by hand;
`Geometry.Parse` turns the same path syntax XAML accepts into an `IconData` value. `Ribbon` raises no
selection event either — `AvaloniaObject.PropertyChanged` is where selection changes surface when a
two-way binding on `SelectedIndex` is not what you want.

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Enigma.Avalonia.Desktop.Controls.Ribbon;

namespace MyApp.Views;

public static class RibbonBuilder
{
    public static Ribbon Build(IReadOnlyList<string> workspaces, ICommand runAction)
    {
        var ribbon = new Ribbon();

        foreach (var workspace in workspaces)
        {
            var group = new RibbonGroup { Header = "Actions" };
            group.Items.Add(new RibbonButton
            {
                Header = "Run",
                IconData = Geometry.Parse("M8 5v14l11-7z"),
                Command = runAction,
                CommandParameter = workspace
            });

            var tab = new RibbonTab { Header = workspace };
            tab.Groups.Add(group);
            ribbon.Tabs.Add(tab);
        }

        ribbon.PropertyChanged += (object? sender, AvaloniaPropertyChangedEventArgs e) =>
        {
            if (e.Property == Ribbon.SelectedTabProperty)
                Console.WriteLine($"Tab: {e.GetNewValue<RibbonTab?>()?.Header}");
            else if (e.Property == Ribbon.SelectedIndexProperty)
                Console.WriteLine($"Index: {e.GetNewValue<int>()}");
        };

        // A write to SelectedIndex always reaches SelectedTab, templated or not.
        ribbon.SelectedIndex = 1;

        return ribbon;
    }
}
```

### Restyling the buttons

A `/template/` selector reaches the template parts and adjusts them without replacing the control
template. Put this in a `Styles` file merged into `Application.Styles`, or inline in `Window.Styles`.

```xml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ribbon="using:Enigma.Avalonia.Desktop.Controls.Ribbon">

  <Style Selector="ribbon|RibbonButton /template/ Border#PART_Root">
    <Setter Property="MinWidth" Value="72" />
    <Setter Property="CornerRadius" Value="2" />
  </Style>

  <Style Selector="ribbon|RibbonToggleButton:checked /template/ Border#PART_Root">
    <Setter Property="BorderThickness" Value="2" />
  </Style>

  <Style Selector="ribbon|RibbonDropDownButton:pressed /template/ Border#PART_Root">
    <Setter Property="Opacity" Value="0.8" />
  </Style>
</Styles>
```

## Notes

- `RibbonButton` and `RibbonToggleButton` execute on pointer press, not release. `RibbonButton` marks
  the press handled only when the command actually ran, so a press blocked by `CanExecute` keeps
  bubbling; `RibbonToggleButton` and `RibbonDropDownButton` always handle it.
- `RibbonToggleButton` writes `IsChecked` before invoking `Command`, and the property binds two-way by
  default. Read the bound state in the handler rather than flipping it — flipping there fights the
  binding.
- Invoking a menu item does not close the drop-down; only a light-dismiss click outside the popup
  does. Bind `IsDropDownOpen` to a ViewModel property and clear it from the command when a selection
  should close the menu.
- `RibbonMenuItem` derives from `AvaloniaObject`, not `Control`: it cannot go into `RibbonGroup.Items`,
  it has no template of its own — the drop-down renders it as an icon-and-label button — and its
  bindings resolve against the scope that declares it, since it has no `DataContext`.
- `SelectedTab` is `null` until the template is applied, at which point the first tab is selected if
  nothing else has been. Writes to `SelectedIndex` always propagate to `SelectedTab`; the reverse sync
  runs once the control is templated. An index outside `0..Tabs.Count - 1` is ignored, and clearing
  `SelectedTab` sets `SelectedIndex` to `-1`. Removing the selected tab at runtime does not move the
  selection — assign one of the two properties yourself afterwards.
- There is no collapsed or minimised state: the tab strip and the selected tab's content are always
  visible. Dock the ribbon to the top of a `DockPanel` so it never scrolls with the content.
- Icons are filled with the theme foreground by `PathIcon` (24×24 on buttons, 14×14 in menus), so
  single-path monochrome geometry reads best; outline-only paths render as slivers.
