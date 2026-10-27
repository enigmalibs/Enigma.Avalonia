# Mobile theme

`Enigma.Avalonia.Mobile` gives an Avalonia mobile application the look of the Enigma desktop
applications. It ships the palette of `Enigma.Avalonia.Desktop` — every colour, every brush, and the
re-colouring of the framework's `TextBox` and `ComboBox` — and nothing else: no controls, no services,
no control templates. A view painted with the `Enigma*` keys looks the same on a phone as it does in a
desktop window, because it resolves the same keys to the same values.

The package is one compiled resource dictionary, merged once into `Application.Resources` from
`avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml` — the same file name, and the same
`ResourceInclude`, as the desktop library's entry point. It is built for Android first, but it carries
no platform code and targets plain `net10.0`, so an iOS or browser head consumes it exactly the same
way.

The palette is not a copy. The two dictionaries inside it are the desktop library's own `Colors.axaml`
and `Brushes.axaml`, compiled into this assembly, and the two packages are released together under one
version number — `Enigma.Avalonia.Mobile` X.Y.Z resolves exactly what `Enigma.Avalonia.Desktop` X.Y.Z
does. The full key reference therefore lives in one place: the [theming](theming.md) guide.

## What the package contains

| Resource | Address | Contents |
|---|---|---|
| Entry point | `avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml` | Merges the two dictionaries below, colours first. The only file you reference. |
| Colours | `avares://Enigma.Avalonia.Mobile/Themes/Colors.axaml` | The 31 `Enigma*Color` keys, once under `Dark` and once under `Light` in `ThemeDictionaries`. |
| Brushes | `avares://Enigma.Avalonia.Mobile/Themes/Brushes.axaml` | 28 `Enigma*Brush` keys plus the 23 FluentTheme `TextControl*` / `ComboBox*` keys it overrides — one brush instance per key, re-coloured in place when the variant changes. |

What it deliberately does **not** contain: the desktop library's controls (`NavigationView`, `Ribbon`,
`DockingHost`, the editors, `ContentDialog`, `Overlay`, `InfoBar`, the settings cards), its six
services, and their control templates. Every control on a mobile screen takes its template from
FluentTheme; the palette decides its colours wherever a template resolves one of the keys.

## Key resources

| Key family | Count | Role |
|---|---|---|
| `Enigma*Color` | 31 per variant | The palette. Theme-scoped: resolve them with `DynamicResource`, and override them to retheme. |
| `Enigma*Brush` | 28 | One brush per colour that templates and views paint with. Not theme-scoped: each brush follows the active variant. |
| `TextControl*`, `ComboBox*` | 13 + 10 | FluentTheme's own brush keys, redefined so the framework `TextBox` and `ComboBox` match. |

The values, the role of each key and the FluentTheme mapping are tabulated in
[theming](theming.md#colour-and-brush-reference) — they are identical here.

## Usage

### Installing the package

Add it to the project that holds your `App.axaml` — in Avalonia's cross-platform template, the shared
project the Android, iOS and desktop heads all reference:

```bash
dotnet add package Enigma.Avalonia.Mobile
```

It brings `Avalonia` and `Avalonia.Themes.Fluent` transitively. The package targets `net10.0`: a shared
project on `net10.0`, or a head on `net10.0-android` / `net10.0-ios`, consumes it as-is.

### Merging the theme in `App.axaml`

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">

  <Application.Styles>
    <FluentTheme />
  </Application.Styles>

  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>

</Application>
```

The rules are the desktop library's: a `ResourceInclude` in `Application.Resources`, never a
`StyleInclude` (the build fails with `AVLN2000` if you try); `<FluentTheme />` stays in
`Application.Styles`, because it templates every control on the screen; and a dictionary of your own
that overrides an `Enigma*` key goes **after** this one in `MergedDictionaries`.

**Merge it from XAML.** The package holds no C# type, so nothing loads its assembly until a compiled
XAML file references it — which the `ResourceInclude` above does, at build time. The same include
constructed in C# (`new ResourceInclude(uri)`) throws `XamlLoadException: No precompiled XAML found`
when nothing else has loaded the assembly yet.

### Painting the main view

The theme paints templates, not your root view. On a phone the root is the view you hand to the
single-view lifetime — give it the background brush, and paint the rest of the screen from the palette
with `DynamicResource`:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.Views.MainView"
             Background="{DynamicResource EnigmaBackgroundBrush}">

  <StackPanel Margin="16" Spacing="12">
    <Border Padding="16"
            CornerRadius="8"
            BorderThickness="1"
            Background="{DynamicResource EnigmaSurfaceBrush}"
            BorderBrush="{DynamicResource EnigmaBorderSubtleBrush}">
      <StackPanel Spacing="4">
        <TextBlock Text="The same palette as the desktop app"
                   FontWeight="SemiBold"
                   Foreground="{DynamicResource EnigmaForegroundBrush}" />
        <TextBlock Text="Secondary text sits on the same surface."
                   TextWrapping="Wrap"
                   Foreground="{DynamicResource EnigmaForegroundSecondaryBrush}" />
      </StackPanel>
    </Border>

    <!-- A framework TextBox: FluentTheme's template, the Enigma input colours. -->
    <TextBox PlaceholderText="Search" />
  </StackPanel>

</UserControl>
```

### Following the device's dark mode

Leave `RequestedThemeVariant` unset on the `Application` and it stays at `Default`: Avalonia follows
the operating system, so the app switches between the `Dark` and `Light` variants with the phone's own
dark-theme setting, re-colouring every brush in place. Set it to `Dark` or `Light` to pin one variant
instead, and assign it at runtime to switch — the code is the desktop library's, unchanged:

```csharp
using Avalonia;
using Avalonia.Styling;

namespace MyApp.Services;

public sealed class AppearanceService
{
    public void UseDark() => Apply(ThemeVariant.Dark);

    public void UseLight() => Apply(ThemeVariant.Light);

    public void FollowTheDevice() => Apply(ThemeVariant.Default);

    private static void Apply(ThemeVariant variant)
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = variant;
    }
}
```

[Theming](theming.md#following-the-variant-from-code) shows how to read a colour back from code when
the variant changes — for chart paints and anything else that takes a `Color` rather than a brush.

### Sharing views with a desktop app

The keys are plain resource lookups, so a view written against them depends on neither package: a
class library of views that only uses `{DynamicResource Enigma…}` resolves them from whichever
dictionary the running application merged.

- **A desktop app on `Enigma.Avalonia.Desktop`** merges the desktop dictionary, which already carries
  the whole palette — it needs nothing from this package.
- **A mobile app** merges this package's dictionary, and the shared views render with the same colours.
- **One application, both packages** gains nothing: the two dictionaries define the same keys with the
  same values. Merge the one whose controls you use.

### Retheming

The keys are the desktop library's, so the
[retheming recipe](theming.md#retheming-with-your-own-palette) applies unchanged: override colour keys
in both variants, in a dictionary merged after this one.

```xml
<ResourceDictionary.MergedDictionaries>
  <ResourceInclude Source="avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml" />
  <ResourceInclude Source="avares://MyApp/Themes/Palette.axaml" />
</ResourceDictionary.MergedDictionaries>
```

## Notes

- **Never `StaticResource` for these keys.** The colours live in `ThemeDictionaries`, so a
  `StaticResource` on a colour key throws while the view loads, and one on a brush key resolves once
  and never sees a dictionary replaced later — see the theming guide's notes.
- **The theme sets no `FontFamily`.** The palette was designed around Inter, as on desktop: reference
  `Avalonia.Fonts.Inter` and call `WithInterFont()` on the app builder of each head if you want it.
- **Touch sizing is FluentTheme's.** The package changes colours only — control sizes, paddings and
  hit targets are the framework's own.
- **Versions move together.** A palette change ships in both packages in the same release; keep the two
  on the same version in a solution that uses both, so desktop and mobile screens stay identical.
