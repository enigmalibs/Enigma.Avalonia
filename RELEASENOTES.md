# Enigma.Avalonia.Desktop v1.0.0 Release Notes

The first public release of **Enigma.Avalonia.Desktop** — a control library for Avalonia 12 desktop
applications. Everything it ships is one of exactly two things, and the split runs through the whole
surface: a **control** is a `TemplatedControl` styled by the one theme dictionary you merge into
`Application.Resources`, and a **service** is an interface under `Enigma.Avalonia.Desktop.Services`
that you register as a singleton and inject. The constraint that shapes it is that the services
needing a visual surface take it **once, at startup** — three through `RegisterHost`, two through
`SetStorageProvider` — so past that handover no ViewModel ever holds a `Window`.

## Feature overview

- **Navigation** — a navigation shell and the service that drives it from a ViewModel.
  - `NavigationView` — the shell: `Items` and `FooterItems` lists, a `SelectedItem`, a `Logo` slot,
    vertical or horizontal `Orientation`, and a configurable `PaneSize`.
  - `NavigationItem` — one entry: `Header`, `IconData`, `LabelMaxWidth`, and the `PageType` /
    `PageViewModelType` pair the page factory resolves against.
  - `NavigationOrientation` — `Vertical` or `Horizontal`, selecting the pane layout.
  - `INavigationService` / `NavigationService` — owns `Items`, `FooterItems`, `SelectedItem` and
    `CurrentPage`, resolves pages through a replaceable `PageFactory` hook, navigates with
    `NavigateToAsync(page, parameter)`, and reports failures on `NavigationFailed`.
  - `INavigationViewModel` — the page lifecycle: `OnAppearingAsync(parameter)` on arrival, and
    `OnDisappearingAsync()`, which **cancels the navigation** by returning `false`.
  - `NavigationFailedEventArgs` — the exception and the lifecycle phase it was thrown in.
- **Docking** — a drag-and-drop docking host and the layout model that describes it in code.
  - `DockingHost` — hosts the layout tree, exposes its `Panes`, rebuilds from a model with
    `SetRootLayout`, closes with `ClosePane`, and handles re-docking by drag and drop.
  - `DockTabGroup` — a tabbed group of panes with a `SelectedPane`, raising `PaneDragStarted` and
    `PaneCloseRequested`.
  - `DockSplitContainer` — two children (`First`, `Second`) split by a resizable `GridSplitter`,
    with an `Orientation` and proportional `FirstSize` / `SecondSize`.
  - `DockPane` — one dockable pane: `Header`, `PaneContent`, `CanClose`, `CanMove`.
  - `DockLayoutNode` and its three subclasses `DockPaneModel`, `DockTabGroupModel` and
    `DockSplitModel` — the serializable layout description `SetRootLayout` consumes.
  - `DockPosition` — the drop zone a drag resolves to; `DockTabGroupEventArgs` carries the pane,
    its source group and the pointer.
- **Ribbon** — a tabbed command surface, composed outermost-in.
  - `Ribbon` — the tab strip: a `Tabs` collection with `SelectedTab` and `SelectedIndex`.
  - `RibbonTab` — one tab: a `Header` and its `Groups`.
  - `RibbonGroup` — a labelled group of controls inside a tab.
  - `RibbonButton` — icon-and-label button bound to a `Command` / `CommandParameter`.
  - `RibbonToggleButton` — the same, with an `IsChecked` state surfaced as the `:checked`
    pseudo-class.
  - `RibbonDropDownButton` — opens a popup of `RibbonMenuItem`s, with a bindable `IsDropDownOpen`.
  - `RibbonMenuItem` — one popup entry: `Header`, `IconData`, `Command`, `CommandParameter`.
- **Editors** — one base class and **fourteen** typed editors, all `TextBox`-derived.
  - `BaseEditor` — the shared surface every editor inherits: `Title`, `Unit`, `LeadingContent` and
    `ActionContent` slots, `SelectAllTextOnFocus`, and a validation state
    (`HasValidationError` / `ValidationErrorMessage`) surfaced as the `:error` pseudo-class.
  - `BaseEditor<T>` — the abstract typed layer: a strongly-typed `Value` kept in sync with the text,
    parsed on change and reformatted on focus loss, with `FormatString` and `NullWhenEmpty`.
  - `TextEditor` and `MultiLineTextEditor` — single-line, and multi-line with return-key acceptance
    and word wrapping.
  - `ShortEditor`, `IntEditor`, `LongEditor`, `UShortEditor`, `UIntEditor`, `ULongEditor`,
    `SingleEditor`, `DoubleEditor`, `DecimalEditor` — one per built-in numeric type, parsing and
    formatting in the invariant culture.
  - `ByteArrayEditor` — the abstract byte-array layer, with `Base64Editor` and `HexadecimalEditor`
    encoding and decoding through `Enigma.Core`'s `Base64Service` and `HexService`.
- **Dialogs, Overlay and InfoBar** — three host controls, each driven by its own service after a
  single `RegisterHost` call.
  - `ContentDialog` — a modal card with a title, an icon, up to three independently enabled and
    commanded buttons, a `DefaultButton`, six sizing properties, and an awaited `DialogResult`.
  - `IContentDialogService` / `ContentDialogService` — `ShowMessageAsync(title, message, …)` for the
    common case, `ShowAsync(configure)` for full control, and `HideAsync()`; the host is reset
    between uses.
  - `Overlay` — a full-window surface hosting arbitrary blocking content, gated by `IsOpen`.
  - `IOverlayService` / `OverlayService` — `ShowAsync(control)` and `HideAsync()`.
  - `InfoBar` — an inline notification with a `Title`, `Message` and one of four severities.
  - `IInfoBarService` / `InfoBarService` — `ShowAsync(configure)` and `HideAsync()`.
  - `DialogResult`, `DefaultButton` and `InfoBarSeverity` — the three enums the trio is configured
    and answered with.
- **Settings controls** — labelled rows for building a settings page.
  - `SettingsCard` — `Header`, `Description` and `IconData`; hosts arbitrary `Content` on the right,
    or acts as a clickable card with a chevron and a `Command` when `Content` is null.
  - `SettingsCardExpander` — the same header, revealing its `Content` on click and applying the
    `:expanded` pseudo-class while open.
- **File and folder dialogs** — Avalonia's storage provider behind an injectable interface, set once
  with `SetStorageProvider`.
  - `IFileDialogService` / `FileDialogService` — `ShowOpenFileDialogAsync(FilePickerOpenOptions)` and
    `ShowSaveFileDialogAsync(FilePickerSaveOptions)`, returning `IStorageFile`s.
  - `IFolderDialogService` / `FolderDialogService` —
    `ShowOpenFolderDialogAsync(FolderPickerOpenOptions)`, returning `IStorageFolder`s.
  - `FileDialogServiceExtensions` and `FolderDialogServiceExtensions` — overloads that build the
    options for you and hand back plain `string` paths.
- **Data — the CollectionView subsystem** — sorting, filtering and grouping over any `IEnumerable`.
  - `CollectionView` — the live view: `SortDescriptions`, `GroupDescriptions`, a `Filter` predicate,
    `Groups`, `Refresh()` and a `DeferRefresh()` batching scope. It raises
    `INotifyCollectionChanged` **and** implements `IList`, which Avalonia's `ItemsSourceView`
    requires of any observable source.
  - `CollectionViewSource` — the bindable XAML entry point, exposing `View`, `SortDescriptions`,
    `GroupDescriptions` and a `Filter` event.
  - `SortDescription` (`PropertyName` + `Direction`), `PropertyGroupDescription`
    (`PropertyName` + optional `ValueConverter`), `CollectionViewGroup` (`Key`, `Items`,
    `ItemCount`), `SortDirection` and `FilterEventArgs`.
- **Theme system** — one dictionary, merged once.
  - `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — a `ResourceDictionary` (merge it with a
    `ResourceInclude`, never a `StyleInclude`) that pulls in the colour and brush foundations and
    all nineteen control templates.
  - **29 `Enigma*` colours in `Dark` and `Light` theme dictionaries**, and **26 `Enigma*` brushes**
    resolved from them — background, surface, border, foreground, accent, selection, hover, pressed,
    overlay and the four severity families.
  - **Overrides of Avalonia's own Fluent keys** — the `TextControl*` and `ComboBox*` families — so
    the built-in `TextBox` and `ComboBox` match the library's controls without any work from you.
- **Cross-cutting** — every operation that can block the user is `Task`-returning and awaited from
  the ViewModel; the library targets the same two TFMs throughout, ships XML documentation for
  IntelliSense on both, and has no public API that exposes a `Window`.

## Dependencies

The package brings four runtime dependencies, identical on both target frameworks:

- `Avalonia` **12.1.1**
- `Avalonia.Themes.Fluent` **12.1.1**
- `CommunityToolkit.Mvvm` **8.4.2**
- `Enigma.Core` **1.0.0** — which brings `BouncyCastle.Cryptography` transitively; it backs the
  `Base64Editor` and `HexadecimalEditor` encoding services, and a project that places neither
  control still carries it.

The **Avalonia set is version-coupled** and is always bumped as a whole, never one package at a
time. The package deliberately does **not** bring the packages that turn Avalonia into a running
application — an app still references `Avalonia.Desktop` for the desktop backend, and
`Avalonia.Fonts.Inter` for the font the theme is designed around.

## Compatibility

- Targets **`net8.0`** and **`net10.0`** — the current LTS pair.
- **No `netstandard2.0`.** Avalonia 12 ships `lib/net8.0` and `lib/net10.0` assets only, so a
  `netstandard2.0` target could not resolve the dependency it is built on.
- Built on **Avalonia 12.1.1**; consuming applications must be on the same coupled Avalonia set.

## Version

- Initial release: **1.0.0**.
