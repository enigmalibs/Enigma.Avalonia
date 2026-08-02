# FEATURE-22A5 PHASE04 — Navigation controls & navigation service

**Branch:** `feature/feature-22a5-phase04-navigation`
**Status:** DONE

## Summary

Added the second control family to `Enigma.Avalonia.Desktop` — the three-type `Controls/Navigation/`
namespace and its two `ControlTheme` dictionaries — plus the first four files of `Services/`, the
navigation service and its contracts.

The family splits cleanly in two, and the split is the point: the **controls** own presentation and
selection, the **service** owns page lifetime. Neither references the other; `NavigationService`
depends on `NavigationItem` only as a data record.

| Layer | Type | Role |
|---|---|---|
| Control | `NavigationView : TemplatedControl` | The rail. `Items`/`FooterItems`, `SelectedItem` (two-way by default), `Logo`, `Orientation`, `PaneSize` (90). Owns the two-way sync between `SelectedItem` and the two templated `ListBox`es. |
| Control | `NavigationItem : TemplatedControl` | One entry: `Header`, `IconData`, `PageType`, `PageViewModelType`, `LabelMaxWidth` (72). |
| Control | `NavigationOrientation` | `Vertical` (default) / `Horizontal`. |
| Service | `INavigationService` / `NavigationService` | `CurrentPage`, `Items`/`FooterItems`, `SelectedItem`, `PageFactory`, `NavigateToAsync`, `NavigationFailed`. |
| Service | `INavigationViewModel` | The page-lifecycle contract: `OnAppearingAsync(object?)`, `OnDisappearingAsync()`. |
| Service | `NavigationFailedEventArgs` | `Exception` + `Phase` (`"PageFactory"`, `"OnAppearingAsync"`, `"OnDisappearingAsync"`, `"TryNavigateToItemAsync"`). |

Four behaviours look like rough edges and are deliberate — all four are preserved verbatim and all
four are now covered by the verification harness below:

- **Concurrent navigations are dropped, not queued.** `_navigationLock.WaitAsync(0)` returns `false`
  immediately and both `NavigateToAsync` and `TryNavigateToItemAsync` simply `return`. A second
  navigation raised while the first is awaiting `OnDisappearingAsync` is silently discarded — the
  correct behaviour for a nav rail, where the user's last click should not queue up a burst of page
  constructions.
- **Cancellation restores the previous selection.** `SelectedItem`'s setter captures `field` before
  `SetProperty`, and `TryNavigateToItemAsync` writes it back when `OnDisappearingAsync` returns
  `false`. The write-back re-enters the setter, but the semaphore is still held, so the re-entrant
  navigation is dropped by the same mechanism above — that is what makes the restore safe.
- **`OnApplyTemplate` detaches before it attaches.** Both `SelectionChanged` handlers are removed
  from the *previous* `ListBox`es before the name scope is re-queried. Without it a re-templated
  rail would keep a stale `ListBox` driving `SelectedItem`.
- **Lifecycle exceptions never abort navigation.** `InvokeDisappearingAsync` catches, reports through
  `NavigationFailed`, and returns `true` — a throwing view-model is reported, not allowed to trap the
  user on a page.

## Files/modules touched

**Created** — 3 files under `src/Enigma.Avalonia.Desktop/Controls/Navigation/`:

| File | Lines |
|---|---|
| `NavigationView.cs` | 286 |
| `NavigationItem.cs` | 87 |
| `NavigationOrientation.cs` | 13 |

**Created** — 4 files under `src/Enigma.Avalonia.Desktop/Services/` (the directory is new):

| File | Lines |
|---|---|
| `NavigationService.cs` | 234 |
| `INavigationService.cs` | 53 |
| `INavigationViewModel.cs` | 25 |
| `NavigationFailedEventArgs.cs` | 19 |

**Created** — 2 files under `src/Enigma.Avalonia.Desktop/Themes/Controls/Navigation/`:
`NavigationView.axaml` (105 lines), `NavigationItem.axaml` (37 lines).

**Modified:** `src/Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — the two `ResourceInclude` lines
under a short comment. It now merges **6** dictionaries (2 foundation + 4 control templates).

**Modified:** `docs/roadmap.md`, `docs/plan/FEATURE-22A5.md` (PHASE04 `TODO` → `IN PROGRESS` → `DONE`).

**No csproj change was needed** — same reasoning as PHASE03: the SDK's default glob picks up the
`.cs` files and the existing recursive `<AvaloniaResource Include="Themes\**" />` picks up the
`.axaml` files. This phase is the first to consume `CommunityToolkit.Mvvm`, which was already
referenced (plan §2.3).

## Deviations & follow-ups

Three transformations were applied beyond the namespace rename. Every ported `.cs` file was diffed
against its source normalised for §3.1 and §3.6; the complete diff consists of exactly these and
nothing else — every declaration, default value, comment and blank line is otherwise byte-identical.
Both `.axaml` files were verified **token-identical** to their source after whitespace normalisation.

1. **§2.4.2 explicit `using` directives.** `ImplicitUsings` is `disable` solution-wide, so the
   directives the port source inherited implicitly are now written out: `System` (`Type`,
   `Exception`, `EventArgs`, `EventHandler`, `Func`, `Activator`, `InvalidOperationException`),
   `System.Collections.Generic` (`IReadOnlyList`), `System.Collections.ObjectModel`,
   `System.Linq` (`Items.Contains` on an `IReadOnlyList<T>` is `Enumerable.Contains`),
   `System.Threading` (`SemaphoreSlim`) and `System.Threading.Tasks`. Ordering follows
   `dotnet_sort_system_directives_first = true` with no group separator, which also alphabetised
   three pre-existing lines (`Avalonia` in `NavigationItem.cs`, `CommunityToolkit.Mvvm` in
   `NavigationService.cs`).
2. **§3.6 `global::` removed — 4 occurrences, all in `NavigationView.cs`.** Each became a plain
   file-scoped `using` above the `namespace`. `NavigationView.cs` was the phase's only §3.6 file, as
   the plan predicted. That leaves **11** of the original 29 for PHASE05–08 (PHASE02 cleared 1,
   PHASE03 13, this phase 4).
3. **One comment added — `NavigationService.cs`, §2.4.3.** The solution invariant forbids `!`
   without a commented justification, and `t.Exception!.InnerException ?? t.Exception!` inside the
   `ContinueWith` is this port's first null-forgiving operator. A two-line comment records why it is
   sound (`TaskContinuationOptions.OnlyOnFaulted` runs the continuation only for a faulted task, so
   `Exception` cannot be null). Per plan §5 this changes no type, member, default value or
   pseudo-class — it is a comment, and FEATURE-28E8 §2 wins over a literal reading of "1:1".
4. **`.axaml` reindented from 4-space to 2-space**, exactly as PHASE01 and PHASE03 did, on the same
   `.editorconfig` `indent_size = 2` grounds. Presentation only: both files keep their original line
   count, and no element, attribute, `PART_` name, selector or resource key differs. One
   continuation-alignment column in `NavigationView.axaml` was off by one in the source
   (`<Border x:Name=…>`'s attributes) and is now aligned.

**Line endings:** no CRLF recommendation to make. All 9 new files are LF with a final newline.

**Observation for a future phase, not acted on here.** The §3.7 sweep is clean under `src/`, but
`docs/done/FEATURE-22A5-PHASE02.md` and `-PHASE03.md` each quote the old name when describing a
prose rename they performed. FEATURE-28E8 §2.4.10 lists the artifacts the
clean-slate rule binds (code, code comments, `README.md`, `RELEASENOTES.md`, guides, `CLAUDE.md`,
commit messages) and exempts `docs/plan/*.md`; `docs/done/` is named in neither list, so this is an
ambiguity in the rule rather than a violation to fix. Worth settling explicitly when FEATURE-2802
writes the consumer documentation — this document deliberately avoids the old name entirely.

**Follow-ups for `FEATURE-6EB0`** — behaviours inherited verbatim from the port source, none of them
a defect to fix here, all worth pinning down with tests. The 20 checks described below were written
against the real controls and should be promoted into the suite largely as-is:

- `NavigationView.ApplyOrientationLayout` writes pseudo-classes onto the `NavigationItem`s held in
  `Items`/`FooterItems`, but nothing re-runs when those *collections* are replaced or mutated — only
  when `Orientation` changes or the template is applied. Setting `Items` after `Orientation` leaves
  the new items without an orientation class.
- `NavigationService.SelectedItem`'s navigation is fire-and-forget (`_ = …ContinueWith(…)`). It
  completes synchronously whenever the factory and both lifecycle hooks are synchronous — which is
  what makes it testable without pumping — but an asynchronous `OnAppearingAsync` completes after
  the setter returns, with no handle for a caller to await.
- `NavigateToAsync` sets `CurrentPage` first and `SelectedItem` second; the `SelectedItem` write
  re-enters the setter and starts a *second* navigation attempt, which the still-held semaphore
  drops. Correct, but only by that indirection.
- `FindItemForPage` matches on `PageType` alone, so two items sharing a `PageType` resolve to
  whichever appears first in `Items`, and `Items` always wins over `FooterItems`.
- The default `PageFactory` throws `InvalidOperationException` when `PageType` is not a `Control`,
  but `Activator.CreateInstance` on a `PageViewModelType` without a parameterless constructor throws
  `MissingMethodException` — both surface through `NavigationFailed` with phase `"PageFactory"`.

## Build/test evidence

- **Build:** `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental` → **Build succeeded,
  0 Warning(s), 0 Error(s)**, for `net8.0` and `net10.0` (both library outputs confirmed present in
  the log). Per §2.4.1 the log was re-read at `-v n` rather than trusting the exit code:
  **zero `AVLN*` occurrences** and zero `: warning` / `: error` diagnostic lines in all 779 lines.
- **Tests:** `dotnet test --solution Enigma.Avalonia.slnx -c Release` → **1 total, 1 succeeded,
  0 failed, 0 skipped**. This phase adds no tests by design (plan §4 assigns the suite to
  `FEATURE-6EB0`), so Definition-of-Done criterion 2 is met by the existing suite continuing to
  pass. That suite is not inert here: its smoke test loads `Themes/Fluent.axaml`, which now merges
  both new dictionaries, so a template that failed to compile or resolve would fail it.
- **Behavioural verification — a throwaway headless harness** (xUnit v3 + `Avalonia.Headless`, built
  outside the repository against the Release `net10.0` output, then discarded) exercised the real
  controls and the real service. **20 of 20 checks passed:**

  | Area | Check | Result |
  |---|---|---|
  | Defaults | `NavigationView.PaneSize` is `90` | pass |
  | Defaults | `NavigationItem.LabelMaxWidth` is `72` | pass |
  | Defaults | `NavigationView.Orientation` is `Vertical` | pass |
  | Defaults | `SelectedItemProperty` default binding mode is `TwoWay` | pass |
  | Theme | `Fluent.axaml` resolves a `ControlTheme` for `typeof(NavigationView)` and `typeof(NavigationItem)` | pass |
  | Theme | The applied template exposes `PART_Border`, `PART_Logo`, `PART_ItemsListBox`, `PART_FooterListBox` | pass |
  | Theme | `PaneSize` reaches `PART_Border.Width` through the template binding (90) | pass |
  | Theme | `NavigationItem`'s template renders its `PathIcon` and its `Header` text | pass |
  | Orientation | `:horizontal`/`:vertical` set on the view **and** on both item collections, both directions | pass |
  | Wiring | **`OnApplyTemplate` detaches old handlers before attaching new ones** — after a re-template the stale `ListBox` no longer drives `SelectedItem`, and the fresh one does | pass |
  | Wiring | Selection is mutually exclusive between `Items` and `FooterItems`, and an unknown/`null` selection clears both | pass |
  | Service | Default `PageFactory` creates the page and sets its ViewModel as `DataContext` | pass |
  | Service | `NavigateToAsync` sets `CurrentPage` and calls `OnAppearingAsync` with the parameter | pass |
  | Service | `NavigateToAsync` matches the page back to its item, including one in `FooterItems` | pass |
  | Service | `OnDisappearingAsync` returning `false` cancels the navigation | pass |
  | Service | The `SelectedItem` setter navigates through `PageFactory` | pass |
  | Service | A cancelled item navigation restores the previous selection | pass |
  | Service | A throwing `PageFactory` raises `NavigationFailed` with phase `"PageFactory"` and leaves `CurrentPage` null | pass |
  | Service | A concurrent navigation is **dropped, not queued**; the in-flight one still completes | pass |
  | Service | It is an `ObservableObject`, raises `PropertyChanged`, implements `INavigationService`, and both collections start empty | pass |

- **§3.7 clean-slate sweep:** a case-insensitive `grep` for the port source's name across `src/`
  → **zero hits**.
- **§2 excluded-type gate:** `CalendarSchedule|Displayer2D|DrawingObject|Shape` across `src/`
  → **zero hits**.
- **§3.6 gate:** no `global::` remains in any hand-written file under `src/` (the only two hits are
  SDK-generated `obj/**/AssemblyAttributes.cs`).
- **File count:** 7 of 7 `.cs` (3 controls + 4 services), 2 of 2 `.axaml`. The tree now holds 30
  `.cs` and 7 `.axaml` under `src/`.

## Acceptance criteria

| Criterion | Status |
|---|---|
| Build clean | Met — Release, non-incremental, `net8.0` + `net10.0`, 0 warnings, 0 `AVLN*` |
| The 7 files present | Met — 3 under `Controls/Navigation/`, 4 under `Services/` |
| `PaneSize` / `LabelMaxWidth` defaults unchanged (90 / 72) | Met — asserted against live instances |
| `OnApplyTemplate` still detaches old handlers before attaching new ones | Met — asserted by re-templating and proving the stale `ListBox` is inert |
| Zero port-source-name hits (§3.7) | Met |
| Both navigation includes appended to `Fluent.axaml` | Met — it now merges 6 dictionaries |
