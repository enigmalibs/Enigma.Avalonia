# Enigma.Avalonia.Desktop and Enigma.Avalonia.Mobile v1.3.0 Release Notes

A feature release that moves to Avalonia 12.1.4 and adds a second package. **Enigma.Avalonia.Mobile**
is new: it brings the Enigma palette — the same colours, the same brushes, the same re-colouring of
the framework's `TextBox` and `ComboBox` — to Avalonia mobile applications, so a phone screen and a
desktop window built on these packages look the same. **Enigma.Avalonia.Desktop** is backward
compatible with 1.2.0: no API, key or value changes, only its Avalonia floor.

## New Features

- **Enigma.Avalonia.Mobile — the Enigma theme for mobile apps (first release).**
  - One resource dictionary, `avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml`, merged into
    `Application.Resources` from `App.axaml` with a `ResourceInclude` — the same idiom, and the same
    file name, as the desktop library's entry point.
  - The desktop palette, exactly: the 31 `Enigma*Color` keys in both the `Dark` and `Light` variants,
    the 28 `Enigma*Brush` keys that follow the active variant, and the 23 FluentTheme `TextControl*` /
    `ComboBox*` keys the desktop theme redefines. The package is compiled from the desktop library's
    own `Colors.axaml` and `Brushes.axaml`, so the two packages resolve identical keys and values at
    the same version.
  - No controls, no services and no control templates: every control on a mobile screen keeps its
    FluentTheme template, painted from the palette.
  - Platform-neutral `net10.0`: an Android or iOS head on `net10.0-android` / `net10.0-ios` consumes
    it with no extra setup. It depends on `Avalonia` and `Avalonia.Themes.Fluent` only — not on
    Enigma.Avalonia.Desktop, `CommunityToolkit.Mvvm` or `Enigma.Core`.
  - Merge it from XAML. The package holds no C# type, so its assembly is loaded through the compiled
    `App.axaml` that references it; a `ResourceInclude` constructed in C# throws
    `XamlLoadException: No precompiled XAML found` when nothing else has loaded the assembly yet.
  - New guide: `docs/guides/mobile.md` — installing, merging the theme, painting the main view,
    following the device's dark mode, sharing views with a desktop app, and retheming.

## Dependencies

- The coupled Avalonia set **12.1.1 → 12.1.4**: `Avalonia` and `Avalonia.Themes.Fluent` (dependencies
  of both packages), and, repository-only, `Avalonia.Desktop`, `Avalonia.Fonts.Inter` and
  `Avalonia.Headless.XUnit`. `AvaloniaUI.DiagnosticsSupport` stays at **2.2.3**, its latest release.
- Enigma.Avalonia.Desktop now brings `Avalonia` and `Avalonia.Themes.Fluent` **12.1.4**,
  `CommunityToolkit.Mvvm` **8.4.2** and `Enigma.Core` **1.0.0**; Enigma.Avalonia.Mobile brings
  `Avalonia` and `Avalonia.Themes.Fluent` **12.1.4**.
- `Enigma.Core` is held back at **1.0.0** — 2.0.0 is a major version of a dependency that flows to
  every consumer, which a minor release should not pull in.
- Repository-only, not in either package: `Microsoft.Extensions.Hosting` (showcase) **10.0.12 →
  10.0.14**; `xunit.v3` (tests) held back at **3.2.2** — 4.0.2 is out, and moving to 4.x is a
  test-suite migration of its own.

## Compatibility

- Enigma.Avalonia.Desktop targets **.NET 8.0** and **.NET 10.0** — unchanged. .NET 8 reaches the end
  of its support on 2026-11-10; the `net8.0` target stays for the 1.x line, and dropping it is
  planned for the next major version.
- Enigma.Avalonia.Desktop's Avalonia floor is now **12.1.4**. An application that pins `Avalonia` or
  `Avalonia.Themes.Fluent` to an earlier 12.1.x directly must raise that pin, or NuGet reports a
  package downgrade (`NU1605`).
- Enigma.Avalonia.Mobile targets **.NET 10.0** and is built on Avalonia **12.1.4**.
- In a solution that uses both packages, keep them on the same version: a palette change always ships
  in both, in the same release.

## Version

- Released: Enigma.Avalonia.Desktop **1.3.0** — a minor release under Semantic Versioning: a
  compatible dependency move, and the package line gains a companion package; nothing removed,
  renamed or changed incompatibly.
- Initial release: Enigma.Avalonia.Mobile **1.3.0** — numbered in lockstep with Enigma.Avalonia.Desktop
  rather than from 1.0.0, because both packages ship the same palette files under one tag.

# Enigma.Avalonia.Desktop v1.2.0 Release Notes

A feature release for the Dark theme, backward compatible with 1.1.0. The `InfoBar` severity colours
— Info, Success, Warning and Error — are lifted to softer, pastel tints in the Dark variant, so an
info bar stands out from the panels around it instead of sinking into them, and its message line
stays readable on the new fills. The Light variant is unchanged.

## New Features

- **Pastel severity fills in the Dark variant.**
  - The eight severity keys take new Dark defaults. Every fill sits at OKLCH L 0.40 / C 0.05 and
    every border at L 0.52 / C 0.075, on the Light variant's own hue per severity, so both variants
    read as the same four colours. The 1.1 fills sat level with `EnigmaSurfaceColor`.

    | Key | Dark 1.1.0 | Dark 1.2.0 |
    |---|---|---|
    | `EnigmaInfoBackgroundColor` | `#1C2940` | `#384863` |
    | `EnigmaInfoBorderColor` | `#28406A` | `#506994` |
    | `EnigmaSuccessBackgroundColor` | `#1C3028` | `#364F37` |
    | `EnigmaSuccessBorderColor` | `#28503A` | `#4D744E` |
    | `EnigmaWarningBackgroundColor` | `#302718` | `#564527` |
    | `EnigmaWarningBorderColor` | `#504020` | `#806434` |
    | `EnigmaErrorBackgroundColor` | `#301C20` | `#603D3D` |
    | `EnigmaErrorBorderColor` | `#502830` | `#8F5758` |

  - `EnigmaForegroundColor` text on every new fill keeps WCAG AA contrast (4.85–5.06:1), so your own
    content on the `Enigma*BackgroundBrush` severity brushes stays readable too.
  - No key is renamed or removed, and an override of any of the eight keys still applies. To keep the
    1.1 look, override them with the 1.1.0 Dark values above, in a dictionary merged after
    `Fluent.axaml` — the *Retheming with your own palette* recipe in `docs/guides/theming.md`.
- **A readable info bar message line.**
  - New theme keys `EnigmaInfoBarMessageForegroundColor` (Dark `#BCBEC4`, Light `#6F737A`) and
    `EnigmaInfoBarMessageForegroundBrush`; the `InfoBar` message line paints the brush. In Dark it
    is the primary text tone, which the secondary tone it replaces could not match on the new fills;
    in Light it is exactly the secondary tone it was. The palette is now 31 colours and 28 brushes.

## Dependencies

No runtime dependency of the package changes; it still brings `Avalonia` and
`Avalonia.Themes.Fluent` **12.1.1**, `CommunityToolkit.Mvvm` **8.4.2** and `Enigma.Core` **1.0.0**.

- The coupled Avalonia set (`Avalonia`, `Avalonia.Themes.Fluent`, `Avalonia.Desktop`,
  `Avalonia.Fonts.Inter`, `Avalonia.Headless.XUnit`) is held back at **12.1.1** — 12.1.3 is out, and
  the set is bumped as a whole, as its own decision.
- `Enigma.Core` is held back at **1.0.0** — 2.0.0 is a major version of a dependency that flows to
  every consumer, which a minor release of this package should not pull in.
- Repository-only, not in the package: `xunit.v3` (tests) held back at **3.2.2** — 4.0.1 is out, and
  moving to 4.x is a test-suite migration of its own. Nothing else is outdated.

## Version

- Released: **1.2.0** — a minor release under Semantic Versioning: new, backward-compatible theme
  keys, and new Dark defaults for existing keys; nothing removed or renamed.

# Enigma.Avalonia.Desktop v1.1.0 Release Notes

A feature release for two of the modal and notification surfaces, fully backward compatible with
1.0.0. An `InfoBar` can now close itself after a period you choose — and still never does unless
you ask — and a `ContentDialog` can sit on a darker, secondary background for content laid out for
the window's own background rather than for the raised dialog surface.

## New Features

- **Timed info bars.**
  - `InfoBar.DisplayDuration` — a new `TimeSpan?` styled property. The default, `null`, keeps the 1.0
    behaviour: the bar stays open until it is dismissed. Set it and the bar closes itself once the
    period has elapsed, exactly as `Close()` would — `IsOpen` goes `false`, `Closed` is raised, and
    the pending `ShowAsync()` completes. The countdown starts when the bar opens, restarts when the
    duration changes or `ShowAsync()` is called on the open bar, and is cancelled when the bar closes
    any other way. A value must be `null`, or greater than zero and at most `int.MaxValue`
    milliseconds; anything else throws `ArgumentException` where it is set.
  - `InfoBarServiceExtensions` — `IInfoBarService.ShowAsync(TimeSpan displayDuration,
    Action<InfoBar>? configure = null)`, an extension overload that shows a timed bar. The duration
    is applied after `configure`, so the argument wins, and an invalid one throws
    `ArgumentOutOfRangeException` before the host is touched. `IInfoBarService` itself is unchanged,
    so your own implementations keep compiling.
  - `InfoBarService.ShowAsync` now also resets `DisplayDuration` between messages, so a timed message
    never makes the next one timed.
- **Secondary dialog background.**
  - `Classes="secondary"` on a `ContentDialog` paints its card with the new
    `EnigmaDialogSecondaryBackgroundBrush` — the window-background tone, darker than the default
    `EnigmaSurfaceHighBrush` in the Dark variant.
  - New theme keys `EnigmaDialogSecondaryBackgroundColor` (Dark `#1E1F22`, Light `#F7F8FA`) and
    `EnigmaDialogSecondaryBackgroundBrush`; the palette is now 30 colours and 27 brushes.
  - The dialog card now paints `ContentDialog.Background`, which the theme sets to
    `EnigmaSurfaceHighBrush` — the look is unchanged by default, a `Background` of your own now
    applies, and redefining `EnigmaSurfaceHighBrush` in a dialog's own `Resources` still works. The
    card is exposed as the template part `PART_Card`.
  - `ContentDialogService` does not reset `Background` or the host's classes: like the `Dialog*`
    sizes, the look is a host setting.

## Dependencies

No runtime dependency of the package changes; it still brings `Avalonia` and
`Avalonia.Themes.Fluent` **12.1.1**, `CommunityToolkit.Mvvm` **8.4.2** and `Enigma.Core` **1.0.0**.

- The coupled Avalonia set (`Avalonia`, `Avalonia.Themes.Fluent`, `Avalonia.Desktop`,
  `Avalonia.Fonts.Inter`, `Avalonia.Headless.XUnit`) is held back at **12.1.1** — 12.1.3 is out, and
  the set is bumped as a whole, as its own decision.
- `Enigma.Core` is held back at **1.0.0** — 2.0.0 is a major version of a dependency that flows to
  every consumer, which a minor release of this package should not pull in.
- Repository-only, not in the package: `Microsoft.Extensions.Hosting` (showcase) **10.0.10 →
  10.0.12**; `xunit.v3` (tests) held back at **3.2.2** — moving to 4.x is a test-suite migration of
  its own.

## Version

- Released: **1.1.0** — a minor release under Semantic Versioning: new, backward-compatible
  functionality, nothing removed or changed.

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
