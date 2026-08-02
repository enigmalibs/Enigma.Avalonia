# FEATURE-57C8 PHASE01 — App shell & host wiring

**Branch:** `feature/feature-57c8-phase01-app-shell`
**Status:** DONE

## Summary

The showcase went from an empty window to a running application: a generic host that supplies
configuration, logging and services; a navigation shell with the library's three overlay hosts wired
to their services; and one navigable page. Everything under `src/` is untouched — this phase only
consumes the library, which is the point of it.

The host is **started, never run**. Avalonia's classic desktop lifetime runs the application and
`Host.CreateApplicationBuilder()` only supplies the container, so `Program.Main` stays synchronous and
`[STAThread]`. `App.OnFrameworkInitializationCompleted` is the composition root: build, start, resolve
the window, perform the five startup wirings, hand the window over, and stop the host on
`desktop.Exit`.

| Concern | Where |
|---|---|
| Entry point | `Program.cs` (unchanged from the skeleton — it was already right) |
| Composition root | `App.axaml.cs` |
| Service registration | `ServiceCollectionExtensions.cs` — `AddEnigmaServices()`, `AddPagesAndViewModels()` |
| Configuration | `ShowcaseOptions` + the `Showcase` section of `appsettings.json` |
| Shell | `Views/MainWindow.axaml(.cs)` + `ViewModels/MainWindowViewModel.cs` |
| Landing page | `Views/HomePageView.axaml(.cs)` + `ViewModels/HomePageViewModel.cs` |
| Consumer control demo | `Controls/ProgressOverlayCard.cs` + `.axaml` |

### The ordering that matters

The three host controls are the **last** children of the window's root `Panel`, after the `DockPanel`
that holds the rail and the content area — that is what puts a dialog, an overlay or an info bar above
everything the app draws. All five wirings (`RegisterHost` ×3, `SetStorageProvider` ×2) run **before**
`desktop.MainWindow` is assigned, so no page can reach a service whose host is still unregistered.

### Views transient, ViewModels singleton

`AddPagesAndViewModels()` registers each page View as transient and each page ViewModel as a singleton.
Navigating back to a page therefore builds a fresh visual tree but re-attaches the ViewModel it had
before: page state survives leaving the page, the controls do not leak. `MainWindowViewModel.CreatePage`
is the navigation service's `PageFactory` and resolves both halves from the container, so a page is
never constructed by hand.

## Files/modules touched

**Created**

- `samples/Enigma.Avalonia.Desktop.Showcase/ShowcaseOptions.cs`
- `samples/Enigma.Avalonia.Desktop.Showcase/ServiceCollectionExtensions.cs`
- `samples/Enigma.Avalonia.Desktop.Showcase/Controls/ProgressOverlayCard.cs`
- `samples/Enigma.Avalonia.Desktop.Showcase/Controls/ProgressOverlayCard.axaml`
- `samples/Enigma.Avalonia.Desktop.Showcase/ViewModels/MainWindowViewModel.cs`
- `samples/Enigma.Avalonia.Desktop.Showcase/ViewModels/HomePageViewModel.cs`
- `samples/Enigma.Avalonia.Desktop.Showcase/ViewModels/HomeHighlight.cs`
- `samples/Enigma.Avalonia.Desktop.Showcase/Views/HomePageView.axaml`
- `samples/Enigma.Avalonia.Desktop.Showcase/Views/HomePageView.axaml.cs`
- `samples/Enigma.Avalonia.Desktop.Showcase/Assets/app.ico`

**Modified**

- `samples/Enigma.Avalonia.Desktop.Showcase/App.axaml` — merged the library's `Themes/Fluent.axaml`
  and the showcase-local `ProgressOverlayCard.axaml` into `Application.Resources` as `ResourceInclude`s.
- `samples/Enigma.Avalonia.Desktop.Showcase/App.axaml.cs` — host wiring; `LiveCharts.Configure` in `Initialize()`.
- `samples/Enigma.Avalonia.Desktop.Showcase/Views/MainWindow.axaml` — the shell layout.
- `samples/Enigma.Avalonia.Desktop.Showcase/Views/MainWindow.axaml.cs` — the close confirmation.
- `samples/Enigma.Avalonia.Desktop.Showcase/appsettings.json` — added the `Showcase` section.
- `samples/Enigma.Avalonia.Desktop.Showcase/Enigma.Avalonia.Desktop.Showcase.csproj` — `<ApplicationIcon>`;
  refreshed the comments that described the skeleton's placeholders.
- `docs/roadmap.md`, `docs/plan/FEATURE-57C8.md` — statuses.

Nothing under `src/` or `tests/` changed.

## Deviations & follow-ups

1. **One navigation item, not ten** (plan §4 step 6 vs. its own acceptance criteria). Step 6 says the
   ViewModel populates `Items` with 10 pages, but every one of those View/ViewModel types lands in
   PHASE02 and PHASE03 — the two statements cannot both hold at PHASE01. Resolved with the user:
   PHASE01 ships a **new** showcase-local `HomePageView` / `HomePageViewModel` (written, not ported) as
   the permanent landing page, and later phases append their items. This matches PHASE01's own
   criterion, "at least one navigable page", and leaves PHASE02/03 scopes untouched.
2. **Page keys are showcase-local.** `ShowcaseOptions.InitialPage` needs to name a page, but
   `NavigationItem` is a control and has no key. `MainWindowViewModel` keeps a
   `Dictionary<string, NavigationItem>` alongside the rail; an unknown key falls back to the first item,
   so a typo costs a wrong landing page rather than an empty shell. *Follow-up:* if this turns out to be
   a recurring consumer need, `NavigationItem` could gain a `Key` property — a library change, not this item.
3. **Initial navigation assigns `Navigation.SelectedItem`.** The port source resolved the page by hand
   and called `NavigateToAsync(...).GetAwaiter().GetResult()`. Assigning `SelectedItem` runs the same page
   factory, highlights the rail entry in the same step, and drops the sync-over-async. It completes
   synchronously here because nothing is on screen yet to veto the navigation.
4. **Content root pinned to `AppContext.BaseDirectory`.** A desktop app is launched with whatever
   working directory the shell or shortcut had, so the default content root would make `appsettings.json`
   resolution depend on it. Verified: launched from an unrelated directory, the host still logged the
   output directory as its content root.
5. **The application icon is generated placeholder art.** Plan step 9's fallback was to carry over the
   port source's `avalonia-logo.ico`; that would put the Avalonia project's own logo on this library's
   showcase. Per the user's decision, `Assets/app.ico` is instead a generated neutral mark — a white "E"
   on the theme accent `#3574F0`, with 16/32/48/256 px in the single file the plan asks for. **Replace it
   with real artwork before the 1.0.0 release.**
6. **The close confirmation is ported, not just the layout.** Plan step 5 describes only
   `MainWindow.axaml`'s layout, but the source's `Closing` handler came with it: the window vetoes the
   close, asks through `IContentDialogService`, and closes for real on `DialogResult.Primary`. Keeping it
   is what made the dialog host verifiable at PHASE01 rather than at PHASE02.
7. **Neither platform workaround from step 10 was needed.** No `Tmds.DBus` `TaskCanceledException` on
   Linux shutdown, and no designer-time service fallback (the window is resolved from the container only
   at run time). The related risk in the house pattern — `host.StopAsync().GetAwaiter().GetResult()` on
   the UI thread inside `desktop.Exit` — also did not bite: nothing is registered as an `IHostedService`,
   so `StopAsync` has no continuation to post back to a shut-down dispatcher.
8. **`AddEnigmaServices` / `AddPagesAndViewModels` drop the `_ =` discards** the port source wrote on
   every registration. `IDE0058` is `silent` in this solution's `.editorconfig`, so they were pure noise.
9. **`Logo` is a Control built inside a ViewModel**, per plan step 6. It works — the icon binds its
   `Foreground` to `EnigmaAccentBrush` as a dynamic resource, so the mark follows a theme switch — but
   XAML on the `NavigationView` would be the more idiomatic home for it. Left as the plan specifies.
10. **One defect found and fixed during visual verification.** The home page's card descriptions were
    clipped instead of wrapped: the icon and the text sat in a horizontal `StackPanel`, which offers its
    children unlimited width along its orientation, so `TextWrapping="Wrap"` never fired. Replaced with a
    `Grid ColumnDefinitions="Auto,*"`.
11. **Line endings:** nothing to report. Every file added or modified is LF with a final newline.

## Build/test evidence

- **Build:** `dotnet build --no-incremental` → `Build succeeded. 0 Warning(s), 0 Error(s)`. Checked for
  `AVLN*` explicitly on a full rebuild, per solution invariant §2.4.1 — none. Every view root and the one
  `DataTemplate` using `{Binding}` declares `x:DataType`.
- **Tests:** `dotnet test` → **273 passed, 0 failed** (unchanged: this phase adds no tests, and its
  acceptance criteria are visual — they call for none).
- **Ran the app** on a real desktop session (Wayland with XWayland), verified by eye from screenshots:
  - the shell opens with the window title taken from `ShowcaseOptions.Title`, the generated icon in the
    titlebar, the navigation rail with its accent logo, and Home selected;
  - the Home page renders with the library's `Enigma*` brushes and `Enigma.Icons` glyphs in Dark;
  - requesting a close (a real `WM_DELETE_WINDOW`) is vetoed and the `ContentDialog` host renders the
    confirmation — proof that `RegisterHost` ran and that the host controls sit above the page content;
  - clicking **Leave** closes the window and the process exits cleanly, with `Application is shutting
    down...` in the log and no exception. The final click was made by the user: the compositor blocks
    synthetic X11 input and `/dev/uinput` is not writable, so it could not be driven from the session.
- **Configuration:** the host logs `Content root path: …/bin/Debug/net10.0/`, confirming
  `appsettings.json` binds regardless of the working directory.
- **Clean-slate rule:** zero occurrences of `Carbon` anywhere under `samples/`.
- **Forbidden host APIs:** no `Host.CreateDefaultBuilder` and no `ConfigureServices` callback in the
  solution.
