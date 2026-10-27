# Enigma.Avalonia.Desktop — Guides & Samples

Per-category guides for **Enigma.Avalonia.Desktop**, a control library for Avalonia 12 desktop
applications. The library ships two kinds of thing: **controls** you place in XAML, and **services**
you resolve from dependency injection and call from a ViewModel.

Both follow the same idiom. Every control is a `TemplatedControl` styled by a single theme
dictionary you merge once into `Application.Resources`; every service is an interface in
`Enigma.Avalonia.Desktop.Services` that you register as a singleton and inject. Three of the
services drive a host control you place in your window and hand over at startup, and two need the
window's storage provider — [theming](theming.md) covers the theme merge, and
[dialogs, overlay and info bar](dialogs-overlay-infobar.md) covers the host registration.

Each guide follows the same shape — **the controls or operations the family offers → key types →
copy-pasteable usage samples → notes** — and every snippet targets the real public API.

**Start with [theming](theming.md).** Nothing in this library renders correctly until its theme
dictionary is merged, so every other guide assumes you have done that and does not repeat it.

The companion package **Enigma.Avalonia.Mobile** carries the same palette — colours, brushes and the
`TextBox` / `ComboBox` re-colouring, without the controls — for Avalonia mobile applications; its
guide is under [Mobile](#mobile) below.

## Foundation

- [Theming](theming.md) — merging `Fluent.axaml` as a `ResourceInclude`, the full `Enigma*` colour
  and brush key reference, Dark/Light theme dictionaries, runtime variant switching, and why these
  keys are always reached with `DynamicResource`.

## Application shell

- [Navigation](navigation.md) — `NavigationView` and `NavigationItem`, `INavigationService` and its
  `PageFactory` hook, the `INavigationViewModel` appearing/disappearing lifecycle, cancelling a
  navigation, and diagnosing failures through `NavigationFailedEventArgs`.
- [Ribbon](ribbon.md) — `Ribbon`, `RibbonTab` and `RibbonGroup`, the three button types
  (`RibbonButton`, `RibbonToggleButton`, `RibbonDropDownButton`) and `RibbonMenuItem`.
- [Docking](docking.md) — `DockingHost` and the layout tree it builds from `DockSplitContainer`,
  `DockTabGroup` and `DockPane`, plus the `DockLayoutNode` model types for describing a layout in
  code.

## Input and content

- [Editors](editors.md) — `BaseEditor<T>` and the fourteen typed editors, leading and action
  content, validation states and the `:error` pseudo-class, and the Base64/hexadecimal byte-array
  editors.
- [Settings cards](settings-cards.md) — `SettingsCard` and `SettingsCardExpander` for building a
  settings page out of labelled rows with inline controls.
- [Collection views](data-collectionview.md) — `CollectionViewSource` and `CollectionView`, sorting
  with `SortDescription`, filtering through the `Filter` event, and grouping with
  `PropertyGroupDescription` and `CollectionViewGroup`.

## Services

- [Dialogs, overlay and info bar](dialogs-overlay-infobar.md) — `ContentDialog`, `Overlay` and
  `InfoBar` with the three services that drive them, and the `RegisterHost` wiring they all
  require.
- [File and folder dialogs](file-folder-dialogs.md) — `IFileDialogService` and
  `IFolderDialogService`, the `SetStorageProvider` requirement, and the convenience overloads that
  return plain paths.

## Mobile

- [Mobile theme](mobile.md) — `Enigma.Avalonia.Mobile`: merging its entry point in a mobile app's
  `App.axaml`, painting the main view from the palette, following the device's dark mode, sharing
  views with a desktop app, and why the dictionary is merged from XAML.
