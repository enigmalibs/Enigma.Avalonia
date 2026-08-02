# Settings cards

`Enigma.Avalonia.Desktop` renders settings rows through two templated controls: `SettingsCard`, one
row with an icon, a title, a description and a slot on the right; and `SettingsCardExpander`, the
same row with a body that unfolds beneath it. Both are declared in XAML and stacked in a
`StackPanel` — there is no page or group control to wrap them in.

Both expose `Content` as their `[Content]` property, so the child element written inside the tags
*is* the slot: a `ToggleSwitch`, a `ComboBox`, a `Button`, one of the library's typed
[editors](editors.md), or a whole panel. Header, description and icon are attributes on the card.

The one thing to internalise is that `SettingsCard` has two mutually exclusive modes and `Content`
alone decides which: set it and the card hosts your control and is otherwise inert; leave it null
and the whole row becomes a button — chevron, hover highlight, and `Command` fired on press. A card
is never both, so hosting a `Button` is how you get a clickable control on a card that also shows
state.

## Controls

| Control | Condition | Behaviour |
|---|---|---|
| `SettingsCard` | `Content` set | Icon, header and description on the left; the content control right-aligned and vertically centred. Pointer input goes to the hosted control — `Command` is ignored and no hover highlight is drawn. |
| `SettingsCard` | `Content` null | The same row with a chevron on the right. The whole row is the button: it highlights on hover and invokes `Command` with `CommandParameter` on press. |
| `SettingsCardExpander` | — | The same header row, always clickable, plus `Content` as a body revealed below the header while `IsExpanded` is true. The chevron rotates 90°, a separator appears, and the body sits on `EnigmaSurfaceLowBrush`. |

Every property on the two controls:

| Property | Type | Declared on | Behaviour |
|---|---|---|---|
| `Header` | `string?` | both | The title line. Always rendered — set it. |
| `Description` | `string?` | both | Wrapping subtitle below the header. The whole text block is collapsed when the value is null or empty. |
| `IconData` | `Geometry?` | both | Drawn as a 20×20 `PathIcon` in a 40×40 leading slot; the slot is collapsed when null. Any SVG path-markup string converts to `Geometry` in XAML; from C# use `Geometry.Parse`. |
| `Content` | `object?` | both | The `[Content]` property. Right-hand slot on `SettingsCard`, expandable body on `SettingsCardExpander`. |
| `Command` | `ICommand?` | `SettingsCard` | Invoked on pointer press, and only while `Content` is null. |
| `CommandParameter` | `object?` | `SettingsCard` | Passed to both `CanExecute` and `Execute`. |
| `IsExpanded` | `bool` | `SettingsCardExpander` | Defaults to `false`; clicking the header toggles it. Bind it `Mode=TwoWay` — the property's default binding mode is one-way. |

## Key types

| Type | Namespace | Role |
|---|---|---|
| `SettingsCard` | `Enigma.Avalonia.Desktop.Controls` | One settings row. A `TemplatedControl`, so `Width`, `Margin` and the rest apply normally. |
| `SettingsCardExpander` | `Enigma.Avalonia.Desktop.Controls` | Settings row with a collapsible body. Also a `TemplatedControl`. |
| `Geometry` | `Avalonia.Media` | The type of `IconData`. The package ships no icon geometries — supply your own path markup. |
| `ICommand` | `System.Windows.Input` | The type of `SettingsCard.Command`. |

## Usage

### A card that hosts a control

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Enigma.Avalonia.Desktop.Controls"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.AppearanceView"
             x:DataType="vm:SettingsViewModel">
  <StackPanel Margin="24" Spacing="12">
    <controls:SettingsCard Header="Theme"
                           Description="Switch the application between its dark and light variants"
                           IconData="M12 2 L15 9 L22 12 L15 15 L12 22 L9 15 L2 12 L9 9 Z">
      <ToggleSwitch IsChecked="{Binding IsDarkTheme, Mode=TwoWay}"
                    OnContent="Dark" OffContent="Light" />
    </controls:SettingsCard>
    <controls:SettingsCard Header="Region" Description="Where your data is stored">
      <ComboBox ItemsSource="{Binding Regions}"
                SelectedItem="{Binding SelectedRegion, Mode=TwoWay}"
                PlaceholderText="Select a region"
                MinWidth="170" />
    </controls:SettingsCard>
  </StackPanel>
</UserControl>
```

### A clickable card that runs a command

Omit `Content` entirely. The card draws a chevron, takes the pointer itself, and passes
`CommandParameter` through — so one command can back every row on the page.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Enigma.Avalonia.Desktop.Controls"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.ShortcutsView"
             x:DataType="vm:SettingsViewModel">
  <StackPanel Margin="24" Spacing="12">
    <controls:SettingsCard Header="Language"
                           Description="Set your preferred language"
                           Command="{Binding OpenPageCommand}"
                           CommandParameter="Language" />
    <controls:SettingsCard Header="Keyboard shortcuts"
                           Command="{Binding OpenPageCommand}"
                           CommandParameter="Shortcuts" />
  </StackPanel>
</UserControl>
```

### An expander holding a form

The body is a single `Content`, not an items collection, so wrap several children in a panel.
Controls inside the body take pointer input normally — only the header row toggles.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Enigma.Avalonia.Desktop.Controls"
             xmlns:editors="using:Enigma.Avalonia.Desktop.Controls.Editors"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.AdvancedView"
             x:DataType="vm:SettingsViewModel">
  <controls:SettingsCardExpander Margin="24"
                                 Header="Advanced"
                                 Description="Session behaviour"
                                 IsExpanded="{Binding IsAdvancedExpanded, Mode=TwoWay}">
    <StackPanel Spacing="12">
      <editors:IntEditor Title="Session timeout" Unit="min" Value="{Binding SessionTimeout}" />
      <CheckBox Content="Restore the last page at startup" />
    </StackPanel>
  </controls:SettingsCardExpander>
</UserControl>
```

### A settings page and its ViewModel

Sections are plain `TextBlock` headings; the `StackPanel` supplies the rhythm and a `MaxWidth` keeps
rows from stretching across a wide window. Mix the three shapes freely.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Enigma.Avalonia.Desktop.Controls"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.SettingsPageView"
             x:DataType="vm:SettingsViewModel">
  <ScrollViewer>
    <StackPanel Margin="24" Spacing="12" MaxWidth="840" HorizontalAlignment="Left">
      <TextBlock Text="Appearance" FontSize="16" FontWeight="SemiBold"
                 Foreground="{DynamicResource EnigmaForegroundBrush}" />
      <controls:SettingsCard Header="Theme" Description="Dark or light">
        <ToggleSwitch IsChecked="{Binding IsDarkTheme, Mode=TwoWay}"
                      OnContent="Dark" OffContent="Light" />
      </controls:SettingsCard>
      <TextBlock Text="General" FontSize="16" FontWeight="SemiBold" Margin="0,8,0,0"
                 Foreground="{DynamicResource EnigmaForegroundBrush}" />
      <controls:SettingsCard Header="Language"
                             Command="{Binding OpenPageCommand}"
                             CommandParameter="Language" />
      <controls:SettingsCardExpander Header="Advanced"
                                     IsExpanded="{Binding IsAdvancedExpanded, Mode=TwoWay}">
        <CheckBox Content="Restore the last page at startup" />
      </controls:SettingsCardExpander>
      <TextBlock Text="{Binding LastAction}" TextWrapping="Wrap"
                 Foreground="{DynamicResource EnigmaForegroundSecondaryBrush}" />
    </StackPanel>
  </ScrollViewer>
</UserControl>
```

Nothing about the family is special on the ViewModel side — the cards bind to ordinary observable
properties and commands.

```csharp
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModels;

public class SettingsViewModel : ObservableObject
{
    public SettingsViewModel()
    {
        OpenPageCommand = new RelayCommand<string?>(OnOpenPage);
    }

    public bool IsDarkTheme { get; set => SetProperty(ref field, value); }

    public IReadOnlyList<string> Regions { get; } = ["North America", "Europe", "Asia Pacific"];

    public string? SelectedRegion { get; set => SetProperty(ref field, value); }

    public bool IsAdvancedExpanded { get; set => SetProperty(ref field, value); }

    public int? SessionTimeout { get; set => SetProperty(ref field, value); } = 30;

    public string? LastAction { get; set => SetProperty(ref field, value); }

    public IRelayCommand<string?> OpenPageCommand { get; }

    private void OnOpenPage(string? page) => LastAction = $"Opened: {page}";
}
```

## Notes

- `Content` is the only switch between the two `SettingsCard` modes. Setting it adds the
  `:hasContent` pseudo-class, which hides the chevron and suppresses the hover highlight, and
  `Command` is then never invoked — setting both is a silent no-op on the command.
- A card with neither `Content` nor `Command` still shows a chevron and still highlights on hover:
  it looks actionable and does nothing. Give every chevron row a command.
- `CanExecute` is consulted at the moment of the press and never re-queried. The card does not
  disable or grey itself when the command cannot run — the press is simply ignored. Bind `IsEnabled`
  on the card when that state has to be visible.
- Both controls act on `PointerPressed`, not on click, so there is no press-and-drag-off to cancel:
  the command fires, or the expander toggles, the instant the pointer goes down.
- Restyling hooks — `SettingsCard` exposes `PART_Root`, `PART_ContentPresenter` and `PART_Chevron`
  with the `:hasContent` and `:pressed` pseudo-classes; `SettingsCardExpander` exposes `PART_Root`,
  `PART_Header`, `PART_Separator`, `PART_Content` and `PART_Chevron` with `:expanded` and
  `:pressed`. Every colour resolves from an `Enigma*` brush as a `DynamicResource`, so a card
  follows a theme-variant switch without being rebuilt — see [theming](theming.md).
