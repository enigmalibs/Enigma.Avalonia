# Theming

`Enigma.Avalonia.Desktop` ships one public XAML entry point: the compiled resource dictionary at
`avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml`. It merges three layers in order — a palette of
`Color` values defined once per theme variant, a set of `SolidColorBrush` instances bound to those
colours, and a `ControlTheme` per control the library ships. Merging that one URI is the whole setup.

The dictionary is a `ResourceDictionary`, so it goes into `Application.Resources` →
`ResourceDictionary.MergedDictionaries` as a `ResourceInclude`. It is not a `Styles` collection and a
`StyleInclude` will not compile. `<FluentTheme />` stays where it was, in `Application.Styles` — this
library themes its own controls and re-colours the framework's `TextBox` and `ComboBox`, but every
other standard Avalonia control still takes its template from FluentTheme.

The layering exists for one reason: runtime theme switching. The colours are theme-scoped, the brushes
are not — there is one brush instance per key, and its `Color` follows the active variant through a
`DynamicResource`. Assigning `Application.Current.RequestedThemeVariant` therefore re-colours every
control on screen in one step, with no reload and no rebuilt visual tree. The price is that your own
XAML must resolve these keys with `DynamicResource` too.

## Setting up the theme

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App"
             RequestedThemeVariant="Dark">

  <Application.Styles>
    <FluentTheme />
  </Application.Styles>

  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>

</Application>
```

`RequestedThemeVariant` sets the starting variant; `Dark` and `Light` are the two the library defines.
Omit it and Avalonia follows the operating system.

**`ResourceInclude`, not `StyleInclude`.** Putting the URI in `Application.Styles` fails the build, and
says why — so the mistake never reaches runtime:

```text
Avalonia error AVLN2000: Resource "avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml" is
defined as "Avalonia.Controls.ResourceDictionary" type in the "Enigma.Avalonia.Desktop"
assembly, but expected "Avalonia.Styling.IStyle".
```

**Keep `<FluentTheme />`.** The library declares a `ControlTheme` only for its own controls. Drop
FluentTheme and every framework control — `Button`, `CheckBox`, `ToggleSwitch`, `ScrollViewer`,
`ListBox` — loses its template, including the ones nested inside this library's templates. The package
brings `Avalonia.Themes.Fluent` transitively, so no extra `PackageReference` is needed.

**Order.** `Application.Styles` and `Application.Resources` are independent collections; their order in
the file is irrelevant. Order *inside* `MergedDictionaries` is not: on a duplicate key the last
dictionary that defines it wins, so a dictionary of yours that overrides an `Enigma*` key must be merged
**after** `Fluent.axaml`. Merged before, it is silently ignored.

**The window background is yours.** The theme paints controls, not the window — set
`Background="{DynamicResource EnigmaBackgroundBrush}"` on it or it keeps Avalonia's default.

That is the whole theming bootstrap. Registering the library's services and placing the dialog, overlay
and info-bar host controls are separate steps — see the [quick start](../../README.md) and
[dialogs, overlay and info bar](dialogs-overlay-infobar.md).

## Colour and brush reference

`Colors.axaml` defines **29** colour keys, once under `x:Key="Dark"` and once under `x:Key="Light"`
inside `ResourceDictionary.ThemeDictionaries`. `Brushes.axaml` turns **26** of them into brushes, one
per key, outside any theme dictionary; the three input-background colours have no `Enigma*` brush of
their own and feed the FluentTheme override keys in the next section.

Both variants define the same key set, which is what makes a variant switch total — a key present in one
variant only would resolve to nothing after the switch, and the brush bound to it would silently stop
updating. The table is in declaration order, so it diffs directly against the source.

| Colour key | Dark | Light | Brush key | Role |
|---|---|---|---|---|
| `EnigmaBackgroundColor` | `#1E1F22` | `#F7F8FA` | `EnigmaBackgroundBrush` | The lowest layer: window background, ribbon body, dock tab strips. |
| `EnigmaSurfaceColor` | `#2B2D30` | `#EBEDF0` | `EnigmaSurfaceBrush` | Default panel fill: settings cards, navigation pane, editor bodies. |
| `EnigmaSurfaceHighColor` | `#313335` | `#DFE1E5` | `EnigmaSurfaceHighBrush` | Raised fill: dialog body, editor under the pointer. |
| `EnigmaBorderColor` | `#393B40` | `#D1D3D8` | `EnigmaBorderBrush` | Emphasised border: dialog frame, hovered card, drop-down flyout. |
| `EnigmaBorderSubtleColor` | `#43454A` | `#E0E2E6` | `EnigmaBorderSubtleBrush` | Default 1px border and separator on cards, panes, groups, editors, splitters. |
| `EnigmaForegroundColor` | `#BCBEC4` | `#1E1F22` | `EnigmaForegroundBrush` | Primary text, icons, caret. |
| `EnigmaForegroundSecondaryColor` | `#6F737A` | `#6F737A` | `EnigmaForegroundSecondaryBrush` | Descriptions, captions, inactive tab labels, editor titles. |
| `EnigmaForegroundTertiaryColor` | `#4E5157` | `#A0A3AA` | `EnigmaForegroundTertiaryBrush` | Placeholder and watermark text, ribbon group labels. |
| `EnigmaAccentColor` | `#3574F0` | `#3574F0` | `EnigmaAccentBrush` | Focus ring, checked-state border. Deliberately the same blue in both variants. |
| `EnigmaAccentHoverColor` | `#4A88F7` | `#2D64D4` | `EnigmaAccentHoverBrush` | The accent under the pointer. |
| `EnigmaSelectionColor` | `#214283` | `#C4D8F8` | `EnigmaSelectionBrush` | Selected navigation item, checked ribbon toggle, text selection. |
| `EnigmaSurfaceLowColor` | `#252628` | `#F2F3F5` | `EnigmaSurfaceLowBrush` | Recessed fill: focused editor, expanded card content area. |
| `EnigmaInputBackgroundColor` | `#1F2123` | `#E8EAED` | — | Resting fill of the framework `TextBox` and `ComboBox`. |
| `EnigmaInputBackgroundFocusedColor` | `#222325` | `#ECEEF1` | — | Focused fill of the framework `TextBox`. |
| `EnigmaInputBackgroundHoverColor` | `#242628` | `#E4E6EA` | — | Pointer-over fill of the framework `TextBox`. |
| `EnigmaHoverColor` | `#2E3035` | `#E8EAED` | `EnigmaHoverBrush` | Pointer-over fill for clickable library surfaces. |
| `EnigmaPressedColor` | `#3C3E42` | `#D2D4D8` | `EnigmaPressedBrush` | Pressed fill for the same surfaces. |
| `EnigmaOverlayColor` | `#80000000` | `#40000000` | `EnigmaOverlayBrush` | Modal scrim. The alpha channel is meaningful — see Notes. |
| `EnigmaSuccessColor` | `#59A869` | `#3B8C4B` | `EnigmaSuccessBrush` | Success accent for your own content; no shipped template consumes it. |
| `EnigmaWarningColor` | `#E8A33D` | `#C48832` | `EnigmaWarningBrush` | Warning accent for your own content; no shipped template consumes it. |
| `EnigmaErrorColor` | `#F75464` | `#DB3B3B` | `EnigmaErrorBrush` | Validation error border and message text on the editors. |
| `EnigmaInfoBackgroundColor` | `#1C2940` | `#DAE6FA` | `EnigmaInfoBackgroundBrush` | Info-severity notification fill. |
| `EnigmaInfoBorderColor` | `#28406A` | `#A0BEF0` | `EnigmaInfoBorderBrush` | Info-severity notification border. |
| `EnigmaSuccessBackgroundColor` | `#1C3028` | `#DAEEDA` | `EnigmaSuccessBackgroundBrush` | Success-severity notification fill. |
| `EnigmaSuccessBorderColor` | `#28503A` | `#A0D4A0` | `EnigmaSuccessBorderBrush` | Success-severity notification border. |
| `EnigmaWarningBackgroundColor` | `#302718` | `#F4E6D0` | `EnigmaWarningBackgroundBrush` | Warning-severity notification fill. |
| `EnigmaWarningBorderColor` | `#504020` | `#DCC098` | `EnigmaWarningBorderBrush` | Warning-severity notification border. |
| `EnigmaErrorBackgroundColor` | `#301C20` | `#F4DADA` | `EnigmaErrorBackgroundBrush` | Error-severity notification fill. |
| `EnigmaErrorBorderColor` | `#502830` | `#DCA0A0` | `EnigmaErrorBorderBrush` | Error-severity notification border. |

## Standard Avalonia controls

The dictionary restyles exactly two framework controls, and does it by overriding FluentTheme's own
brush keys rather than by replacing its templates. Those names are FluentTheme's contract, so they
appear verbatim — 13 for `TextBox`, 10 for `ComboBox` — and each resolves an `Enigma*` colour, which is
why they follow a variant switch like everything else.

Every `TextControl*` key below belongs to `TextBox`, every `ComboBox*` key to `ComboBox`. They are
grouped by the palette colour each one resolves, because that is the axis you retheme along.

| Palette colour | FluentTheme keys that resolve it |
|---|---|
| `EnigmaInputBackgroundColor` | `TextControlBackground`, `ComboBoxBackground` |
| `EnigmaInputBackgroundHoverColor` | `TextControlBackgroundPointerOver` |
| `EnigmaInputBackgroundFocusedColor` | `TextControlBackgroundFocused` |
| `EnigmaSurfaceHighColor` | `ComboBoxBackgroundPointerOver` |
| `EnigmaSurfaceLowColor` | `ComboBoxBackgroundPressed`, `ComboBoxBackgroundDisabled` |
| `EnigmaBorderSubtleColor` | `TextControlBorderBrush`, `TextControlBorderBrushPointerOver`, `ComboBoxBorderBrush`, `ComboBoxBorderBrushPointerOver` |
| `EnigmaAccentColor` | `TextControlBorderBrushFocused`, `ComboBoxBorderBrushPressed` |
| `EnigmaForegroundColor` | `TextControlForeground`, `TextControlForegroundPointerOver`, `TextControlForegroundFocused`, `ComboBoxForeground`, `ComboBoxForegroundPointerOver` |
| `EnigmaForegroundTertiaryColor` | `TextControlPlaceholderForeground`, `TextControlPlaceholderForegroundPointerOver`, `TextControlPlaceholderForegroundFocused`, `ComboBoxPlaceholderTextForeground` |
| `EnigmaSelectionColor` | `TextControlSelectionHighlightColor` — a brush despite the name; that is FluentTheme's naming |

Everything else — `Button`, `CheckBox`, `RadioButton`, `ToggleSwitch`, `ListBox`, `ScrollViewer`,
`Expander`, `ToolTip`, `TabControl`, `Menu`, `Slider` and the rest — is left entirely to FluentTheme:
the dictionary declares no application-level `Style` and no `ControlTheme` for any framework type. The
`ListBoxItem` styles in the library's templates sit inside a `<ListBox.Styles>` block within those
templates, so they never reach a `ListBox` of yours.

## Usage

### Painting your own views with the palette

Resolve brush keys with `DynamicResource` and your views change variant along with the library's
controls. No `xmlns` beyond the two defaults is needed — the keys are plain resource lookups.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="MyApp.MainWindow"
        Title="My application"
        Background="{DynamicResource EnigmaBackgroundBrush}">

  <Border Margin="24"
          Padding="16"
          CornerRadius="8"
          BorderThickness="1"
          Background="{DynamicResource EnigmaSurfaceBrush}"
          BorderBrush="{DynamicResource EnigmaBorderSubtleBrush}">
    <StackPanel Spacing="4">
      <TextBlock Text="A panel painted from the palette"
                 FontWeight="SemiBold"
                 Foreground="{DynamicResource EnigmaForegroundBrush}" />
      <TextBlock Text="Secondary text sits on the same surface."
                 Foreground="{DynamicResource EnigmaForegroundSecondaryBrush}" />
    </StackPanel>
  </Border>

</Window>
```

### Switching the variant at runtime

`RequestedThemeVariant` is a property on `Avalonia.Application`; `ThemeVariant` lives in
`Avalonia.Styling`. Assign it and every `DynamicResource` in the application re-resolves. Read
`ActualThemeVariant` when initialising — it reports the variant in force, which a view model created
mid-session cannot assume.

```csharp
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyApp.ViewModels;

public class AppearanceViewModel : ObservableObject
{
    private bool _isDarkTheme = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (!SetProperty(ref _isDarkTheme, value)) return;

            if (Application.Current is { } app)
                app.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
```

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Enigma.Avalonia.Desktop.Controls"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.AppearanceView"
             x:DataType="vm:AppearanceViewModel">

  <controls:SettingsCard Header="Theme"
                         Description="Switch the application between its dark and light variants">
    <ToggleSwitch IsChecked="{Binding IsDarkTheme, Mode=TwoWay}"
                  OnContent="Dark"
                  OffContent="Light" />
  </controls:SettingsCard>

</UserControl>
```

### Following the variant from code

Content built in C# — chart paints, generated geometry, anything that takes a `Color` rather than a
brush — cannot rely on `DynamicResource`. Subscribe to `Application.ActualThemeVariantChanged` and read
the current value back through `TryFindResource`, the extension method in `Avalonia.Controls`.

```csharp
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace MyApp.Services;

public sealed class ChartPalette
{
    public ChartPalette()
    {
        if (Application.Current is { } app)
            app.ActualThemeVariantChanged += OnThemeChanged;

        Refresh();
    }

    public Color Accent { get; private set; }

    public bool IsDark { get; private set; }

    private void OnThemeChanged(object? sender, EventArgs e) => Refresh();

    private void Refresh()
    {
        if (Application.Current is not { } app) return;

        IsDark = app.ActualThemeVariant == ThemeVariant.Dark;

        if (app.TryFindResource("EnigmaAccentBrush", out var value) && value is ISolidColorBrush accent)
            Accent = accent.Color;
    }
}
```

`Application.Current` outlives every view model, so detach the handler when the subscriber's life is
shorter than the application's.

### Retheming with your own palette

Override **colours**, not brushes: every brush binds its `Color` to a colour key and every template
binds to a brush, so replacing one colour re-tints everything that layer feeds. Define both variants — a
key present in only one resolves to nothing after a switch.

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <ResourceDictionary.ThemeDictionaries>
    <ResourceDictionary x:Key="Dark">
      <Color x:Key="EnigmaAccentColor">#7A5AF8</Color>
      <Color x:Key="EnigmaSurfaceColor">#102030</Color>
    </ResourceDictionary>
    <ResourceDictionary x:Key="Light">
      <Color x:Key="EnigmaAccentColor">#5B3FD0</Color>
      <Color x:Key="EnigmaSurfaceColor">#E0F0FF</Color>
    </ResourceDictionary>
  </ResourceDictionary.ThemeDictionaries>

</ResourceDictionary>
```

Add it to the `MergedDictionaries` block of the `App.axaml` above, on the line **after** the library's —
`<ResourceInclude Source="avares://MyApp/Themes/Palette.axaml" />`. That position is what makes it win;
above the library's include it is silently ignored.

A `ResourceDictionary.ThemeDictionaries` block written directly inside `Application.Resources` works
identically and needs no second file; use it for one or two keys, and the merged file when the palette
is worth versioning on its own.

### Scoping a variant to one window or one subtree

`RequestedThemeVariant` also exists on `TopLevel` (so on `Window`) and on `ThemeVariantScope`, and sets
`ActualThemeVariant` for that subtree. What it does **not** do is re-colour the library's brushes: those
are single instances defined once at application level, so their colour always follows
`Application.Current.ActualThemeVariant`. Inside a locally scoped subtree, bind the **colour** keys and
build the brush there — a `DynamicResource` on a colour key resolves in the subtree's own context and
does honour the local variant.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="MyApp.PreviewWindow"
        Title="Preview"
        Background="{DynamicResource EnigmaBackgroundBrush}">

  <ThemeVariantScope RequestedThemeVariant="Light">
    <Border Padding="16" CornerRadius="8">
      <Border.Background>
        <SolidColorBrush Color="{DynamicResource EnigmaSurfaceColor}" />
      </Border.Background>
      <TextBlock Text="Always rendered against the light surface." />
    </Border>
  </ThemeVariantScope>

</Window>
```

For anything beyond a preview pane, prefer one variant per application: set it on `Application` and let
the whole tree follow.

## Notes

- **Never `StaticResource` for these keys.** On a colour key it is a hard failure — colours live in
  `ThemeDictionaries`, `StaticResource` is variant-blind, and the view throws
  `KeyNotFoundException: Static resource 'EnigmaBackgroundColor' not found.` while loading. On a brush
  key it appears to work, because the shared instance mutates, but it resolves once and keeps the object
  it captured: replace a key later (a dictionary merged at runtime, or
  `Application.Current.Resources["EnigmaSurfaceBrush"] = …`) and every `StaticResource` reference stays
  on the stale brush while every `DynamicResource` reference updates.
- `EnigmaAccentColor` is the same `#3574F0` in both variants by design; a brush bound to it does not
  change on a variant switch, and that is not a bug.
- `EnigmaOverlayColor` is the one colour whose alpha carries meaning — `#80000000` dark, `#40000000`
  light — because the scrim dims what is behind it. Preserve the alpha when overriding it or the modal
  background turns opaque.
- `EnigmaOverlayBrush`, `EnigmaSuccessBrush` and `EnigmaWarningBrush` are consumed by no shipped
  template; they exist for your content. `Overlay.OverlayBrush` and `ContentDialog.OverlayBrush` default
  to a literal semi-transparent black, so assign `{DynamicResource EnigmaOverlayBrush}` explicitly if
  you want the scrim to track the palette.
- The theme sets no application-level `Foreground` or `FontFamily`. Text outside the library's controls
  keeps Avalonia's defaults until you give it an `Enigma*` brush.
- A colour key added to one variant and not the other compiles and ships, then fails at runtime after a
  switch by rendering nothing — which is why both variants carry the identical 29-key set.
