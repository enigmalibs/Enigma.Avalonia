# Enigma.Avalonia.Desktop

[![NuGet](https://img.shields.io/nuget/v/Enigma.Avalonia.Desktop.svg)](https://www.nuget.org/packages/Enigma.Avalonia.Desktop)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)

Enigma.Avalonia.Desktop is a control library for Avalonia 12 desktop applications: a navigation
shell, a ribbon, a docking host, typed editors, dialogs, settings cards and a sortable, filterable
collection view, all under one dark-first theme. It ships two kinds of thing, and every part of the
surface is one of them — **controls** are `TemplatedControl`s styled by a single theme dictionary you
merge once into `Application.Resources`, and **services** are interfaces in
`Enigma.Avalonia.Desktop.Services` that you register as singletons and inject into ViewModels. Three
of the services drive a host control you place in your window and hand over at startup, and two need
the window's storage provider; past that, nothing in the library asks a ViewModel to know about a
`Window`.

> **What's new in 1.0** — first release. See [RELEASENOTES.md](RELEASENOTES.md).

## Features

- **Navigation shell** — `NavigationView` and `NavigationItem` with vertical or horizontal
  `Orientation`, a configurable `PaneSize`, a logo slot and footer items; `INavigationService` drives
  it from a ViewModel through a `PageFactory` hook, with an `INavigationViewModel`
  appearing/disappearing lifecycle that can cancel a navigation, and `NavigationFailedEventArgs` for
  diagnosing the ones that fail.
- **Ribbon** — `Ribbon`, `RibbonTab` and `RibbonGroup`, with `RibbonButton`, `RibbonToggleButton`,
  `RibbonDropDownButton` and `RibbonMenuItem`.
- **Docking** — `DockingHost` and the layout tree it builds from `DockSplitContainer`,
  `DockTabGroup` and `DockPane`, plus `DockLayoutNode` model types for describing a layout in code.
- **Editors** — `BaseEditor<T>` and fourteen typed editors covering text, multi-line text, every
  built-in numeric type, and byte arrays as raw, Base64 or hexadecimal text; each with a title,
  watermark and unit label, leading and action content slots, and a validation state surfaced as the
  `:error` pseudo-class.
- **Dialogs, overlay and info bar** — `ContentDialog` (three configurable buttons, an icon, six size
  properties and a `DialogResult`), `Overlay` for hosting arbitrary blocking content, and `InfoBar`
  with four severities — each driven by its own service.
- **Settings cards** — `SettingsCard` and `SettingsCardExpander` for building a settings page out of
  labelled rows with inline controls.
- **Collection views** — `CollectionViewSource` and `CollectionView` with `SortDescription` sorting,
  event-based filtering, and `PropertyGroupDescription` grouping into `CollectionViewGroup`s.
- **File and folder pickers** — `IFileDialogService` and `IFolderDialogService` put Avalonia's
  storage provider behind an injectable interface, with overloads that hand back plain paths.
- **Theming** — one merged `ResourceDictionary` with Dark and Light `ThemeDictionaries`, a documented
  `Enigma*` colour and brush key reference, and overrides that restyle the standard Avalonia controls
  to match.

## Installation

```bash
dotnet add package Enigma.Avalonia.Desktop
```

Targets **.NET 8.0** and **.NET 10.0**; built on Avalonia 12.1.1.

The package brings `Avalonia`, `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm` and `Enigma.Core`
transitively. It deliberately does **not** bring the packages that turn those into a running
application — your app still references `Avalonia.Desktop` for the desktop backend, and
`Avalonia.Fonts.Inter` if you want the font the theme is designed around.

**One transitive dependency is worth knowing about before you install.** `Enigma.Core` brings
`BouncyCastle.Cryptography`, a ~4.7 MB assembly. It is there for exactly two controls —
`Base64Editor` and `HexadecimalEditor`, which encode and decode through `Enigma.Core`'s encoding
services — so a project that never places either one still carries it, and there is no way to opt
out short of not referencing the package.

## Quick start

Five steps take an empty Avalonia app to one that renders in this theme and can raise a dialog from
a ViewModel.

**1. Merge the theme dictionary** into `Application.Resources`. It is a `ResourceInclude`, not a
`StyleInclude`: a `StyleInclude` compiles and then resolves none of the keys.

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
        <ResourceInclude Source="avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

**2. Register the six services** as singletons.

```csharp
using Enigma.Avalonia.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

services.AddSingleton<INavigationService, NavigationService>();
services.AddSingleton<IContentDialogService, ContentDialogService>();
services.AddSingleton<IOverlayService, OverlayService>();
services.AddSingleton<IInfoBarService, InfoBarService>();
services.AddSingleton<IFileDialogService, FileDialogService>();
services.AddSingleton<IFolderDialogService, FolderDialogService>();
```

**3. Place the three host controls** as the last children of the window's root `Panel`, so they
overlay everything the application draws.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="using:Enigma.Avalonia.Desktop.Controls"
        xmlns:contentDialog="using:Enigma.Avalonia.Desktop.Controls.ContentDialog"
        xmlns:infoBar="using:Enigma.Avalonia.Desktop.Controls.InfoBar"
        x:Class="MyApp.Views.MainWindow"
        Background="{DynamicResource EnigmaBackgroundBrush}">
  <Panel>
    <!-- your application content -->

    <contentDialog:ContentDialog x:Name="HostDialog" />
    <controls:Overlay x:Name="HostOverlay" />
    <infoBar:InfoBar x:Name="HostInfoBar" />
  </Panel>
</Window>
```

**4. Hand the hosts and the storage provider over at startup**, before the window is shown. A
service whose host is still unregistered throws the moment a ViewModel asks it for anything.

```csharp
services.GetRequiredService<IContentDialogService>().RegisterHost(mainWindow.HostDialog);
services.GetRequiredService<IOverlayService>().RegisterHost(mainWindow.HostOverlay);
services.GetRequiredService<IInfoBarService>().RegisterHost(mainWindow.HostInfoBar);
services.GetRequiredService<IFileDialogService>().SetStorageProvider(mainWindow.StorageProvider);
services.GetRequiredService<IFolderDialogService>().SetStorageProvider(mainWindow.StorageProvider);
```

**5. Inject a service and await it.** The ViewModel never sees a `Window`.

```csharp
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Services;

public class MainWindowViewModel : ObservableObject
{
    private readonly IContentDialogService _dialogService;

    public MainWindowViewModel(IContentDialogService dialogService)
    {
        _dialogService = dialogService;
        DeleteCommand = new AsyncRelayCommand(OnDeleteAsync);
    }

    public AsyncRelayCommand DeleteCommand { get; }

    private async Task OnDeleteAsync()
    {
        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Delete the project?";
            dialog.PrimaryButtonText = "Delete";
            dialog.CloseButtonText = "Cancel";
        });

        if (result == DialogResult.Primary)
        {
            // delete it
        }
    }
}
```

## Documentation

Per-category guides — each with the controls or operations the family offers, its key types, and
copy-pasteable samples verified against the public API — live under `docs/guides/` in the
repository, indexed by `docs/guides/README.md`. They cover theming, navigation, the ribbon, docking,
the editors, settings cards, collection views, the dialog/overlay/info-bar trio, and the file and
folder pickers.

Start with the theming guide: nothing in this library renders correctly until its theme dictionary
is merged, so every other guide assumes you have done that and does not repeat it.

## License

Enigma.Avalonia.Desktop is released under the [MIT License](LICENSE.md).
