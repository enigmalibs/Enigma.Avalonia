# FEATURE-22A5 PHASE05 — ContentDialog, Overlay, InfoBar + their services

**Branch:** `feature/feature-22a5-phase05-dialogs`
**Status:** DONE

## Summary

Added the library's three **modal / notification surfaces** and the three services that drive them —
six control types, three `ControlTheme` dictionaries and six service files.

All three follow the same shape, and that shape is the phase's real deliverable: a control is a
**passive host** that a service owns. The host is placed once, as a sibling in the window's root
`Panel`, and handed to its service via `RegisterHost(...)` at startup; from then on view-models talk
only to the service interface and never touch the control.

| Layer | Type | Role |
|---|---|---|
| Control | `ContentDialog : ContentControl` | Modal card over a dimming overlay. Title, icon, scrolling content, up to three buttons. `ShowAsync()` → `Task<DialogResult>`. |
| Control | `DefaultButton` | `None` (default) / `Primary` / `Secondary` / `Close` — which button is styled as the default action. |
| Control | `DialogResult` | `None` / `Primary` / `Secondary` / `Close` — what closed the dialog. |
| Control | `Overlay : ContentControl` | Full-window dimming layer hosting arbitrary centred content. `IsOpen` only — no result, no buttons. |
| Control | `InfoBar : ContentControl` | Inline notification: title, message, severity icon, close button. `ShowAsync()` completes on dismissal. |
| Control | `InfoBarSeverity` | `Info` (default) / `Success` / `Warning` / `Error` — selects card background, border and glyph. |
| Service | `IContentDialogService` / `ContentDialogService` | `RegisterHost`, `ShowMessageAsync(title, message, closeButtonText = "OK")`, `ShowAsync(Action<ContentDialog>)`, `HideAsync`. |
| Service | `IOverlayService` / `OverlayService` | `RegisterHost`, `ShowAsync(Control)`, `HideAsync`. |
| Service | `IInfoBarService` / `InfoBarService` | `RegisterHost`, `ShowAsync(Action<InfoBar>? = null)`, `HideAsync`. |

Five behaviours look like rough edges and are deliberate. All five are preserved verbatim and all
five are covered by the verification harness below:

- **The pre-`RegisterHost` throw is asymmetric — `Show` throws, `Hide` does not.** All three services
  throw `InvalidOperationException` from their `Show*` path with a host-specific message, but
  `HideAsync` is a silent no-op when `_host is null`. That is the right asymmetry for teardown code:
  hiding something that was never registered is not an error, showing into nothing is.
- **`OverlayService.ShowAsync` checks the host *before* the argument.** A `null` control passed
  before `RegisterHost` reports the host error, not `ArgumentNullException`. Order is load-bearing
  for the message a consumer sees.
- **Reset-then-configure, not configure-onto-stale.** `ContentDialogService` and `InfoBarService`
  each reset every property of the shared host before invoking the caller's configuration action, so
  a field left set by a previous dialog cannot bleed into the next one. `ResetDialog` uses
  `ClearValue(ContentDialog.IconBrushProperty)` rather than assigning `null` — assigning would pin a
  local `null` over the `ControlTheme` setter and permanently kill the themed icon brush.
- **`ShowAsync` unsubscribes its own completion handler.** Both `ContentDialog.ShowAsync` and
  `InfoBar.ShowAsync` capture a local handler that removes itself inside the callback, so a host
  shown many times accumulates no subscriptions and each `Task` resolves exactly once.
- **`Overlay.HideAsync` clears `Content`.** The comment in the source says "prevent memory leaks" and
  it is accurate: the host outlives every control it displays, so not clearing would root the last
  one for the lifetime of the window.

## Files/modules touched

**Created** — 6 files under `src/Enigma.Avalonia.Desktop/Controls/`:

| File | Lines |
|---|---|
| `ContentDialog/ContentDialog.cs` | 391 |
| `ContentDialog/DefaultButton.cs` | 19 |
| `ContentDialog/DialogResult.cs` | 19 |
| `InfoBar/InfoBar.cs` | 113 |
| `InfoBar/InfoBarSeverity.cs` | 19 |
| `Overlay.cs` | 36 |

**Created** — 6 files under `src/Enigma.Avalonia.Desktop/Services/`:

| File | Lines |
|---|---|
| `IContentDialogService.cs` | 42 |
| `ContentDialogService.cs` | 97 |
| `IOverlayService.cs` | 33 |
| `OverlayService.cs` | 64 |
| `IInfoBarService.cs` | 33 |
| `InfoBarService.cs` | 66 |

**Created** — 3 files under `src/Enigma.Avalonia.Desktop/Themes/Controls/`: `ContentDialog.axaml`
(104 lines), `Overlay.axaml` (40), `InfoBar.axaml` (105). These three sit **directly** under
`Themes/Controls/`, not in a per-family subfolder — the port source places them there and the plan's
file list repeats those paths, so the flat placement is carried over rather than normalised to match
`Editors/` and `Navigation/`.

**Modified:** `src/Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — the three `ResourceInclude` lines
under a short comment. It now merges **9** dictionaries (2 foundation + 7 control templates).

**Modified:** `docs/roadmap.md`, `docs/plan/FEATURE-22A5.md` (PHASE05 `TODO` → `IN PROGRESS` → `DONE`).

**No csproj change was needed** — as in PHASE03 and PHASE04, the SDK's default glob picks up the
`.cs` files and the existing recursive `<AvaloniaResource Include="Themes\**" />` picks up the
`.axaml` files.

## Deviations & follow-ups

Two transformations were applied beyond the namespace rename. Every ported `.cs` file was diffed
against its source normalised for §3.1 with the `using` block excluded; **all twelve diffs are
empty** — every declaration, default value, comment and blank line is otherwise byte-identical. All
three `.axaml` files were verified **token-identical** to their source after whitespace
normalisation, and each keeps its original line count.

1. **§2.4.2 explicit `using` directives.** `ImplicitUsings` is `disable` solution-wide, so the
   directives the port source inherited implicitly are now written out: `System` (`Action`,
   `EventArgs`, `EventHandler`, `InvalidOperationException`, `ArgumentNullException`) and
   `System.Threading.Tasks` (`Task`, `TaskCompletionSource`). Ordering follows
   `dotnet_sort_system_directives_first = true` with no group separator, which also alphabetised the
   pre-existing lines in `ContentDialog.cs` and `InfoBar.cs` (both listed `Avalonia` last).
   `System.Windows.Input` was already present in `ContentDialog.cs` and only moved position.
2. **`.axaml` reindented from 4-space to 2-space**, exactly as PHASE01, PHASE03 and PHASE04 did, on
   the same `.editorconfig` (`[*.{xml,axaml,xaml}] indent_size = 2`) grounds. Presentation only: no
   element, attribute, `PART_` name, selector or resource key differs. Several
   continuation-alignment columns were off by one in the source (`<Border Name="PART_Overlay">` and
   the three `<Button Name="PART_*Button">` blocks in `ContentDialog.axaml`) and are now aligned.

**§3.6 — nothing to do this phase.** None of the 15 ported files contains a `global::Avalonia`
qualification, so the running count is unchanged: **11 of the original 29** remain for PHASE06–08
(PHASE02 cleared 1, PHASE03 13, PHASE04 4).

**Line endings:** no CRLF recommendation to make. All 15 new files are LF with a final newline and
no trailing whitespace.

**Two arithmetic slips in the plan, surfaced before building and not acted on.** Neither is
ambiguous — in both cases the enumerated file list is unmistakable and the count is simply wrong.

- **PHASE05's acceptance line says "11 files present"; the phase enumerates 15** (6 control `.cs` +
  3 templates + 6 service files). All 15 were ported. Read as "11", the criterion cannot be
  satisfied by any subset that also satisfies the enumeration.
- **§5 says 62 `.cs` (40 controls + 7 data + 15 services); the true total is 63.** The port source's
  `Services/` holds 16 files, not 15 — the 15 excludes `NavigationFailedEventArgs.cs`, which is an
  `EventArgs` type living in `Services/` rather than a service. `40 + 7 + 16 = 63`. This is an
  item-level criterion checked at PHASE08, so it is recorded here rather than corrected: **the
  PHASE08 dev should expect 63 `.cs` files, not 62.** The 22 `.axaml` figure is unaffected and still
  correct (2 foundation + 20 control templates); after this phase the tree holds 42 `.cs` and 10
  `.axaml`.

**Follow-ups for `FEATURE-6EB0`** — behaviours inherited verbatim from the port source, none of them
a defect to fix here, all worth pinning down with tests. The 27 checks below were written against the
real controls and services and should be promoted into the suite largely as-is:

- **The three hosts are single-instance by construction.** `RegisterHost` overwrites `_host` with no
  guard, so a second call silently repoints the service and any dialog awaiting on the previous host
  never completes. That is acceptable for the documented once-at-startup contract, but nothing
  enforces it.
- **`ContentDialog.ShowAsync` is re-entrant and will strand a task.** Calling it twice without an
  intervening close leaves two handlers subscribed; the next close resolves both, but a *third* show
  before any close leaves the earlier `TaskCompletionSource`s to be completed out of order relative
  to the caller's expectations. `ContentDialogService` never does this (it resets and shows a single
  host), but the control's public API permits it.
- **`InfoBarService.ShowAsync` awaits the info bar's dismissal.** Unlike `OverlayService.ShowAsync`,
  which returns `Task.CompletedTask` immediately, this one does not complete until the bar is closed.
  Both are correct for their control, and the asymmetry is easy to trip over in a view-model that
  `await`s the call.
- **`ContentDialog.DefaultButton` is a data-only property.** It is stored and reset, but no template
  selector or code path consumes it — no button is actually styled or focused as the default. Faithful
  to the source; worth a note when FEATURE-2802 documents the control.
- **`ContentDialog`'s `Escape` handling requires keyboard focus.** `OnKeyDown` only fires when the
  dialog or a descendant has focus; nothing in the control or the template focuses it on open, so
  Escape works only after the user has tabbed into the card.
- **The `Classes="accent"` / `Classes="transparent"` styles are not defined by this library.**
  `ContentDialog`'s primary button and `InfoBar`'s close button both reference them; `accent` comes
  from FluentTheme, `transparent` does not resolve to anything shipped here and is inert.

## Build/test evidence

- **Build:** `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental` → **Build succeeded,
  0 Warning(s), 0 Error(s)**, for `net8.0` and `net10.0` (both library outputs confirmed present in
  the log). Per §2.4.1 the log was re-read at `-v n` rather than trusting the exit code:
  **zero `AVLN*` occurrences** and zero `: warning` / `: error` diagnostic lines in all 763 lines.
- **Tests:** `dotnet test --solution Enigma.Avalonia.slnx -c Release` → **1 total, 1 succeeded,
  0 failed, 0 skipped**. This phase adds no tests by design (plan §4 assigns the suite to
  `FEATURE-6EB0`), so Definition-of-Done criterion 2 is met by the existing suite continuing to
  pass. That suite is not inert here: its smoke test loads `Themes/Fluent.axaml`, which now merges
  all three new dictionaries, so a template that failed to compile or resolve would fail it.
- **Behavioural verification — a throwaway headless harness** (xUnit v3 + `Avalonia.Headless`, built
  outside the repository against the Release `net10.0` output, then discarded) exercised the real
  controls and the real services. **27 of 27 checks passed:**

  | Area | Check | Result |
  |---|---|---|
  | Defaults | `DialogWidth`/`DialogHeight` are `NaN`; `DialogMinWidth` 320, `DialogMaxWidth` 600, `DialogMinHeight` 0, `DialogMaxHeight` `+∞` | pass |
  | Defaults | The three `Is*ButtonEnabled` are `true`; `DefaultButton`/`DialogResult` are `None`; `IsOpen` false; all four text properties null | pass |
  | Defaults | `OverlayBrush` is a `SolidColorBrush` of `#4D000000` on **both** `ContentDialog` and `Overlay` | pass |
  | Defaults | `Overlay.IsOpen` false with null content; `InfoBar` opens closed at `Severity=Info` with null title/message | pass |
  | Defaults | All three enums keep their member names **and declaration order** | pass |
  | Theme | `Fluent.axaml` resolves a `ControlTheme` for `ContentDialog`, `Overlay` and `InfoBar` | pass |
  | Theme | The dialog template exposes `PART_Overlay`, `PART_PrimaryButton`, `PART_SecondaryButton`, `PART_CloseButton` | pass |
  | Theme | **The sizing surface reaches the card** — 320/600/0/+∞ land on the card `Border`, and changing `DialogMinWidth`/`DialogMaxWidth` re-flows it live | pass |
  | Theme | **Title and buttons stay fixed** — the `ScrollViewer` is row 1 only (`Disabled`/`Auto`), with the title in row 0 and all three buttons in a row-2 `StackPanel` | pass |
  | Theme | The info bar template exposes `PART_Card`, `PART_Icon`, `PART_CloseButton`, and `Severity` restyles both card and glyph | pass |
  | Theme | `IsOpen` drives `IsVisible` on all three controls | pass |
  | Dialog | Each of the three buttons closes with **its own** `DialogResult` and executes **its own** command first | pass |
  | Dialog | `Escape` closes an open dialog with `None` | pass |
  | Dialog | `HideAsync` closes an open dialog with `None` and completes immediately when already closed | pass |
  | Dialog | A second `ShowAsync` is independent of the first — the handler unsubscribed itself | pass |
  | InfoBar | The close button raises `Closed` once and completes the `ShowAsync` task; `CloseAsync` raises it again | pass |
  | Services | **All three throw `InvalidOperationException` before `RegisterHost`**, each with its own message (`ShowMessageAsync` included) | pass |
  | Services | `HideAsync` is a **no-op** before `RegisterHost` on all three | pass |
  | Services | Each concrete service implements its interface | pass |
  | Dialog svc | `ShowMessageAsync`'s reflected signature is `(string title, string message, string closeButtonText = "OK") → Task<DialogResult>` | pass |
  | Dialog svc | It wraps the message in a `TextBlock` with `TextWrapping.Wrap` and applies the `"OK"` default | pass |
  | Dialog svc | `ShowAsync` resets **all 14** properties before configuring, and `ClearValue` restores the themed `IconBrush` rather than nulling it | pass |
  | Dialog svc | `HideAsync` closes the registered host and resolves the pending task with `None` | pass |
  | Overlay svc | `ShowAsync` sets `Content` and opens; `HideAsync` closes **and clears `Content`** | pass |
  | Overlay svc | A null control reports the *host* error before registration, and `ArgumentNullException("control")` after | pass |
  | InfoBar svc | Reset-then-configure: a stale message is cleared while the caller's title and severity are applied | pass |
  | InfoBar svc | `ShowAsync()` with no configuration action resets to `Info` with null title and message | pass |

- **§3.7 clean-slate sweep:** a case-insensitive `grep` for the port source's name across `src/`
  → **zero hits**.
- **§2 excluded-type gate:** `CalendarSchedule|Displayer2D|DrawingObject|Shape` across `src/`
  → **zero hits**.
- **§3.6 gate:** no `global::` remains in any hand-written file under `src/`.
- **File count:** 15 of 15 (6 controls + 6 services + 3 templates). The tree now holds 42 `.cs` and
  10 `.axaml` under `src/`.

## Acceptance criteria

| Criterion | Status |
|---|---|
| Build clean | Met — Release, non-incremental, `net8.0` + `net10.0`, 0 warnings, 0 `AVLN*` |
| Files present | Met — **15**, the phase's full enumeration (the "11" in the acceptance line is a miscount; see *Deviations*) |
| The pre-`RegisterHost` throw is intact on all three services | Met — asserted on `ContentDialogService.ShowAsync`/`ShowMessageAsync`, `OverlayService.ShowAsync`, `InfoBarService.ShowAsync`, each with its own message |
| Dialog size defaults unchanged | Met — `NaN`/`NaN`/320/600/0/`+∞`, asserted on a live instance **and** through the template onto the card |
| `ShowMessageAsync` signature and `"OK"` default unchanged | Met — verified by reflection against the interface |
| Two-way template bindings not substituted with `{TemplateBinding}` | Met vacuously — none of the three templates contains a two-way binding; all bindings are one-way `{TemplateBinding}` in the source and remain so |
| Zero port-source-name hits (§3.7) | Met |
| The three includes appended to `Fluent.axaml` | Met — it now merges 9 dictionaries |
