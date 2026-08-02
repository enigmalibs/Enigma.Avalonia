# FEATURE-57C8 — Showcase app: Enigma.Avalonia.Desktop.Showcase

**Status:** IN PROGRESS · 3 phases
**Branches:** `feature/feature-57c8-phaseNN-showcase`
**Depends on:** FEATURE-22A5 (all 8 phases)
**Solution invariants:** `docs/plan/FEATURE-28E8.md` §2.

## 1. Objective

A non-packable Avalonia desktop app under `samples/Enigma.Avalonia.Desktop.Showcase/` that exercises
every control the library ships, across 11 navigable pages — the visual verification the headless
tests cannot provide, and the reference consumers copy from.

## 2. Port source (internal reference — see FEATURE-28E8 §2.4.10)

```
/home/jo/Dev/Carbon.Avalonia.Desktop/src/CarbonAvaloniaDesktopTester/
```

Page views and ViewModels are ported near-verbatim (with the §3 transformations). The **host wiring
is deliberately not ported** — see PHASE01.

### Deliberately excluded

| Excluded | Why |
|---|---|
| `ViewModels/SchedulePageViewModel.cs` + `Views/SchedulePageView.axaml(.cs)` | `CalendarSchedule` is not in the library |
| `Displayer2DPage*` and `Displayer2DImagePage*` (2 VMs + 2 views) | `Displayer2D` is not in the library |
| `ApiHostedService.cs`, `ApiOptions.cs`, `<FrameworkReference Include="Microsoft.AspNetCore.App" />` | An embedded ASP.NET Core minimal API has nothing to do with an Avalonia control library |
| `NLog.config`, `NLog`, `NLog.Extensions.Logging` | `Host.CreateApplicationBuilder` already provides console logging with no extra package |
| `ViewLocator.cs` | Dead code — it is commented out in the source's `App.axaml`; navigation resolves views through `PageFactory` + DI |
| `Documentation/*.md` | Generic notes, not part of the sample |

## 3. Mechanical transformations

1. Namespace `CarbonAvaloniaDesktopTester` → `Enigma.Avalonia.Desktop.Showcase`.
2. `using Carbon.Avalonia.Desktop.*` → `using Enigma.Avalonia.Desktop.*`; XAML
   `using:Carbon.Avalonia.Desktop.*` → `using:Enigma.Avalonia.Desktop.*`.
3. `{DynamicResource Carbon*}` → `{DynamicResource Enigma*}` per the key map from
   FEATURE-22A5 PHASE01.
4. `avares://Carbon.Avalonia.Desktop/Themes/Fluent.axaml` →
   `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml` (merged as a `ResourceInclude` in
   `Application.Resources` — it is a `ResourceDictionary`, so a `StyleInclude` would be wrong).
5. **Icons:** replace every `PhosphorIconsAvalonia` usage with `Enigma.Icons.Avalonia`:
   - C#: `IconService.CreateGeometry(Icon.gear, IconType.regular)` →
     `PhosphorIconSet.Instance.GetGlyph(PhosphorIcon.Gear, PhosphorWeight.Regular).ToGeometry()`
     (`using Enigma.Icons.Avalonia;` + `using Enigma.Icons.Phosphor;`). Icon names move from
     `snake_case` to `PascalCase` (`chat_circle_text` → `ChatCircleText`,
     `square_split_horizontal` → `SquareSplitHorizontal`, `pencil_ruler` → `PencilRuler`).
   - XAML: one namespace declaration reaches both the control and the markup extensions —
     `xmlns:ei="https://github.com/josueclement/Enigma.Icons"` — then `<ei:Icon Kind="Gear"/>` for
     anything themed or bound, and `{ei:IconGeometry Gear}` only for static `Path.Data`.
   - **Prefer `<ei:Icon>` over the markup extension** wherever the icon sits in a themed or
     interactive surface: a markup extension is evaluated once at load time and cannot follow a
     theme switch, while `Icon` inherits `Foreground` from its text scope and re-resolves at render
     time. The Editors nav item in the source uses a hand-written `Geometry.Parse` path — replace it
     with a real icon (`PhosphorIcon.PencilSimple` or similar).
   - Nothing goes into `App.axaml` for icons — the package ships no XAML.
6. **Clean-slate sweep:** zero occurrences of the literal string `Carbon` anywhere under `samples/`.
7. Every view root and `DataTemplate` using `{Binding}` declares `x:DataType` (`AVLN2100` otherwise).

## 4. PHASE01 — App shell & host wiring — DONE

New code, written to the house pattern rather than ported.

### Steps

1. **`Program.cs`** — `[STAThread]`, sync `Main`, `BuildAvaloniaApp().StartWithClassicDesktopLifetime(args)`;
   `AppBuilder.Configure<App>().UsePlatformDetect()` + `#if DEBUG .WithDeveloperTools() #endif`
   + `.WithInterFont().LogToTrace()`.
2. **`App.axaml`** — `RequestedThemeVariant="Dark"`, `<FluentTheme/>` in `Application.Styles`, and
   `Application.Resources` merging the library's `Fluent.axaml` plus the showcase-local
   `ProgressOverlayCard.axaml`.
3. **`App.axaml.cs`** — `OnFrameworkInitializationCompleted` builds a `HostApplicationBuilder`
   (`Host.CreateApplicationBuilder()`), registers services (§4.4), starts the host, resolves
   `MainWindow` + `MainWindowViewModel`, performs the five startup wirings, assigns
   `desktop.MainWindow`, and stops the host on `desktop.Exit`.
   The five wirings, in this order: `IContentDialogService.RegisterHost(mainWindow.HostDialog)`,
   `IOverlayService.RegisterHost(mainWindow.HostOverlay)`,
   `IInfoBarService.RegisterHost(mainWindow.HostInfoBar)`,
   `IFileDialogService.SetStorageProvider(mainWindow.StorageProvider)`,
   `IFolderDialogService.SetStorageProvider(mainWindow.StorageProvider)`.
   `LiveCharts.Configure(...)` (SkiaSharp + default mappers + dark theme) stays in `Initialize()`.
4. **`ServiceCollectionExtensions.cs`** — C# 14 `extension(IServiceCollection)` blocks, as in the
   source: `AddEnigmaServices()` registers the six library services as singletons;
   `AddPagesAndViewModels()` registers `MainWindow` + each page **View as Transient** (a new instance
   per navigation) and each **ViewModel as Singleton** (state survives navigation).
5. **`Views/MainWindow.axaml(.cs)`** — ported layout: a root `Panel` whose children are the app
   content, then `ContentDialog` (`x:Name="HostDialog"`), `Overlay` (`HostOverlay"`) and `InfoBar`
   (`HostInfoBar`) as the last siblings; `NavigationView` bound to the VM's navigation service;
   `Icon="/Assets/app.ico"` on the window.
6. **`ViewModels/MainWindowViewModel.cs`** — ported: sets `Navigation.PageFactory` to resolve
   `navItem.PageType` / `navItem.PageViewModelType` from `IServiceProvider`, populates `Items` (10
   pages) and `FooterItems` (Settings), builds the logo, and navigates to the initial page. Icons per
   §3.5. Keep the ported property/command style: semi-auto properties with `SetProperty` and
   `IRelayCommand` properties initialised in the constructor — **no `[ObservableProperty]`, no
   `[RelayCommand]`**, classes not `partial`.
7. **`appsettings.json` + `ShowcaseOptions`** — replaces the excluded `ApiOptions` as the
   configuration demo: a `Showcase` section bound via
   `builder.Services.Configure<ShowcaseOptions>(builder.Configuration.GetSection("Showcase"))`,
   carrying at least the window title and the initial page key; `MainWindowViewModel` consumes it
   through `IOptions<ShowcaseOptions>`. `appsettings.json` is `CopyToOutputDirectory=PreserveNewest`.
8. **`Controls/ProgressOverlayCard.cs` + `.axaml`** — ported showcase-local `TemplatedControl`, the
   demonstration that `IOverlayService` hosts arbitrary consumer content.
9. **`Assets/`** — a multi-resolution `.ico` (16/32/48/256 in one file) plus `<ApplicationIcon>` in the
   csproj and `Icon="/Assets/app.ico"` on the window. If no artwork exists yet, carry over the
   source's `avalonia-logo.ico` and note it as placeholder art in the completion doc.
10. Two platform workarounds from the source are **not** carried over by default (the house pattern
    supersedes the hosting shape they were written against). If either symptom appears, add it back
    with a comment explaining why: a `TaskCanceledException` from `Tmds.DBus` on Linux shutdown, and
    the XAML previewer having no host (a designer-time service fallback).

### Acceptance criteria

- `dotnet build` clean, zero warnings including `AVLN*`.
- `dotnet run --project samples/Enigma.Avalonia.Desktop.Showcase` opens the shell with a working
  `NavigationView` and at least one navigable page; the window closes without an unhandled exception
  (state explicitly if a desktop session was unavailable and this could not be verified by eye).
- The three host controls are the last children of the root `Panel` and all five startup wirings run
  before `desktop.MainWindow` is assigned.
- No `Host.CreateDefaultBuilder`, no `ConfigureServices` callback anywhere.
- Zero `Carbon` hits under `samples/`.

### As built — what PHASE02 and PHASE03 inherit

Step 6's "populates `Items` (10 pages)" could not hold at PHASE01: those View and ViewModel types are
PHASE02/PHASE03 deliverables. The shell was built to grow instead, and the two later phases must
account for it (full record in `docs/done/FEATURE-57C8-PHASE01.md`):

- The rail ships **one** item, a new showcase-local `HomePageView` / `HomePageViewModel` landing page —
  written for this library, not ported. It stays; later phases **append** their items rather than
  replacing it, which makes the total 12 pages (Home + the 10 nav pages + `DummyPage`).
- `MainWindowViewModel.AddPage(key, header, icon, viewType, viewModelType, footer)` is the single place
  a page is registered on the rail. `SettingsPage` passes `footer: true`.
- Each page also needs its View registered **transient** and its ViewModel **singleton** in
  `AddPagesAndViewModels()`.
- `ShowcaseOptions.InitialPage` names a page by the `key` passed to `AddPage`; unknown keys fall back to
  the first item. `appsettings.json` ships `"InitialPage": "home"`.
- `Assets/app.ico` is generated placeholder art (a white "E" on the accent) — replace it with real
  artwork before FEATURE-1702.

## 5. PHASE02 — Pages: Base controls, Editors, Dialogs, Services — DONE

Port 4 page pairs: `BaseControlsPage*` (standard Avalonia controls restyled by the theme),
`EditorsTestingPage*` (all 14 typed editors incl. Base64/Hex), `DialogsTestingPage*` (ContentDialog
variants, sizing, `DefaultButton`, `DialogResult`), `ServicesTestingPage*` (Overlay + InfoBar
severities + file/folder pickers + `ProgressOverlayCard`).

Register each View (Transient) and ViewModel (Singleton), and add its `NavigationItem` with an
`Enigma.Icons` glyph.

**Acceptance:** build clean; all four pages navigate, render and interact correctly by eye — a dialog
opens and returns a result, an InfoBar of each severity appears, a file picker opens, every editor
accepts and rejects input with the `:error` state visible.

### As built — what PHASE03 inherits

Full record in `docs/done/FEATURE-57C8-PHASE02.md`. The conventions PHASE03's seven pages follow:

- The rail reads Home · Base Controls · Editors · Dialogs · Services. PHASE03 appends its six main
  items and the `SettingsPage` footer item; keys are lowercase-hyphenated (`base-controls`).
- §5's parenthetical page descriptions have `DialogsTestingPage*` and `ServicesTestingPage*` the wrong
  way round versus the port source. The source's names were kept: `DialogsTestingPage*` is the
  file/folder pickers, `ServicesTestingPage*` is ContentDialog + Overlay + InfoBar.
- Repeated per-row chrome inside a page goes in a `UserControl.Styles` class (`.readout`, `.result`,
  `.hint`), not on every element.
- **Content built in C# takes theme brushes as dynamic resources, never literal colours** — the port
  source's hardcoded hex would not survive PHASE03's own runtime Dark↔Light criterion. The pattern is
  `control[!SomeProperty] = new DynamicResourceExtension("EnigmaX")`.
- Command properties are the concrete `AsyncRelayCommand`/`RelayCommand` with `On…Async` handlers, per
  the `communitytoolkit-mvvm` skill.

## 6. PHASE03 — Pages: Ribbon, Docking, Navigation, CollectionView, Charts, Settings — TODO

Port the remaining 7 page pairs: `RibbonTestingPage*`, `DockingTestingPage*`, `NavigationDemoPage*`,
`DummyPage*` (the navigation target stub), `CollectionViewPage*` (sort/filter/group over a sample
collection), `ChartsPage*` (LiveCharts line/column/pie with theme-reactive axes) and
`SettingsPage*` (`SettingsCard`/`SettingsCardExpander` + the theme-variant switch).

- `ChartsPageViewModel` subscribes to `Application.Current.ActualThemeVariantChanged` and rebuilds its
  series — keep that, it is the theme-reactivity demo. `SettingsPageView` contains an app-description
  paragraph in the source: **rewrite it for this library, with no reference to the port source**
  (clean-slate rule).
- `SettingsPage` is the `FooterItems` entry, not a main nav item.

**Acceptance:** build clean; all 11 pages reachable; ribbon tabs switch and its drop-down opens;
panes dock, split and tab; the CollectionView page sorts, filters and groups live; charts render and
follow a runtime Dark↔Light switch along with every control on screen; zero `Carbon` hits.
