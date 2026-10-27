# Enigma.Avalonia.Mobile

[![NuGet](https://img.shields.io/nuget/v/Enigma.Avalonia.Mobile.svg)](https://www.nuget.org/packages/Enigma.Avalonia.Mobile)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)

Enigma.Avalonia.Mobile gives an Avalonia 12 mobile application — Android first, iOS alike — the look
of the applications built on **Enigma.Avalonia.Desktop**. It is one resource dictionary you merge once
into `Application.Resources`, and everything in it is a resource key: a palette of colours defined
once per theme variant, brushes that follow the active variant at runtime, and the re-colouring of
the framework's `TextBox` and `ComboBox`. It ships no controls and no services — every control on the
screen keeps its FluentTheme template — and it is compiled from the very files the desktop library
uses, so a view painted with these keys looks the same on a phone as in a desktop window.

## Features

- **The Enigma palette** — 31 `Enigma*Color` keys, defined once under `Dark` and once under `Light`,
  from the background and surface scale to text tones, accent, selection and the four severity fills.
- **Brushes that follow the variant** — 28 `Enigma*Brush` keys, one shared instance each, re-coloured
  in place when the application's theme variant changes, so a runtime switch or the device's own dark
  mode repaints every view that resolves them with `DynamicResource`.
- **Standard controls in the same skin** — FluentTheme's own `TextBox` and `ComboBox` brush keys,
  redefined on top of the palette.
- **One palette, two packages** — the same keys and values as `Enigma.Avalonia.Desktop` at the same
  version: both packages are compiled from one set of source files and released together.

## Installation

```bash
dotnet add package Enigma.Avalonia.Mobile
```

Targets **.NET 10.0**; built on Avalonia 12.1.4.

The package brings `Avalonia` and `Avalonia.Themes.Fluent` transitively. It is platform-neutral, so
reference it from the project that holds your `App.axaml` — the shared project of a cross-platform
solution — while each head keeps its own platform package (`Avalonia.Android`, `Avalonia.iOS`).

## Quick start

**1. Merge the theme dictionary** into `Application.Resources`, from XAML. It is a
`ResourceInclude`, not a `StyleInclude`, and `<FluentTheme />` stays in `Application.Styles`.

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

Leave `RequestedThemeVariant` unset and the app follows the device's dark-theme setting; set it to
`Dark` or `Light` to pin one variant.

**2. Paint your views from the palette** with `DynamicResource` — starting with the root view's
background, which the theme does not set for you.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.Views.MainView"
             Background="{DynamicResource EnigmaBackgroundBrush}">
  <Border Margin="16"
          Padding="16"
          CornerRadius="8"
          Background="{DynamicResource EnigmaSurfaceBrush}">
    <TextBlock Text="Hello from the Enigma palette"
               Foreground="{DynamicResource EnigmaForegroundBrush}" />
  </Border>
</UserControl>
```

## Documentation

The mobile guide — installing the package, merging the theme, following the device's dark mode,
sharing views with a desktop app, and retheming — lives in the repository at
`docs/guides/mobile.md`, indexed by `docs/guides/README.md`. The full colour and brush reference,
with every key's Dark and Light value and its role, is the theming guide in the same folder: the keys
and values are identical in both packages.

## License

Enigma.Avalonia.Mobile is released under the [MIT License](LICENSE.md).
