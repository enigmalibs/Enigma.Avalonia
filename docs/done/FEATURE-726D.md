# FEATURE-726D — InfoBar auto-close after a duration

**Completed:** 2026-09-27 · single phase
**Branch:** `feature/feature-726d-infobar-auto-close`
**Run:** feature/2026-09-27-infobar-dialog-release

## Summary

`InfoBar` can now close itself after a caller-chosen period, and still never does unless asked:

- **`InfoBar.DisplayDuration`** — a new `TimeSpan?` styled property, default `null` (stays open until
  dismissed, as before). While the bar is open and has a duration, a one-shot
  `DispatcherTimer.RunOnce` countdown runs; when it elapses the bar calls `Close()`, so `IsOpen` goes
  `false`, `Closed` is raised once, and a pending `ShowAsync()` completes exactly as for a manual
  dismissal. Any change to `IsOpen` or `DisplayDuration` restarts or stops the countdown, and
  `ShowAsync()` on an already-open bar restarts it, so a new message gets its whole duration.
  Invalid values (zero, negative, over `int.MaxValue` ms — the `DispatcherTimer` ceiling) are refused
  by the property's `validate` callback at the point they are set.
- **`InfoBarServiceExtensions.ShowAsync(TimeSpan displayDuration, Action<InfoBar>? configure = null)`**
  — a C# 14 extension block on `IInfoBarService`, beside the file/folder dialog extensions. It
  validates the duration before touching the host (`ArgumentOutOfRangeException`, `ParamName`
  `"displayDuration"`), then applies `configure` and the duration after it, so the argument wins.
  `IInfoBarService` itself is unchanged, so existing implementers keep compiling.
- **`InfoBarService` reset** — `ClearValue(InfoBar.DisplayDurationProperty)` joins the reset, so a timed
  message never makes the next one timed, and an untimed message shown over a timed one stops its
  countdown.

## Files touched

- **Modified** `src/Enigma.Avalonia.Desktop/Controls/InfoBar/InfoBar.cs` — property, validation helper,
  countdown, `OnPropertyChanged`, `ShowAsync` restart, docs.
- **Created** `src/Enigma.Avalonia.Desktop/Services/InfoBarServiceExtensions.cs` — the timed overload.
- **Modified** `src/Enigma.Avalonia.Desktop/Services/InfoBarService.cs` — the reset line.
- **Created** `tests/Enigma.Avalonia.Desktop.UnitTests/Controls/InfoBarDisplayDurationTests.cs` — 15
  cases.
- **Created** `tests/Enigma.Avalonia.Desktop.UnitTests/TaskAssert.cs` — `CompletesAsync`, a bounded
  "this task completes on its own" assertion shared by both test classes.
- **Modified** `tests/Enigma.Avalonia.Desktop.UnitTests/Services/InfoBarServiceTests.cs` — 7 new cases.
- **Modified** `samples/Enigma.Avalonia.Desktop.Showcase/ViewModels/ServicesTestingPageViewModel.cs`
  and `Views/ServicesTestingPageView.axaml` — `ShowTimedCommand` and an "Auto-close (5 s)" button.
- **Modified** `docs/guides/dialogs-overlay-infobar.md` — documentation sweep (below).
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-726D.md` — status `DONE`.
- **Created** `docs/done/FEATURE-726D.md` — this file.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| Exception type from an invalid `DisplayDuration` | Whatever Avalonia's `validate` path throws, asserted as `ArgumentException` or a subclass | The property system owns that exception; the docs promise `ArgumentException` (which it is, or derives from) |
| Does the extension throw synchronously? | Yes — a non-`async` `Task`-returning method, so argument validation fails at the call | The .NET convention for argument errors; the tests await it with `Assert.ThrowsAsync`, which also sees synchronous throws |
| Shared test helper | `TaskAssert.CompletesAsync` with a 5 s bound | Both test classes wait for a real timer; the bound keeps a regression from hanging the run, and a passing test returns as soon as its task completes |
| Proving "closing early cancels the countdown" | Count `Closed` events past the original deadline | Re-showing the bar (the plan's wording) restarts the countdown anyway, so it proves nothing; an uncancelled countdown would call `Close()` again and raise a second `Closed` |

## Deviations & follow-ups

- **Acceptance criterion 3** was verified in the stronger form above (no second `Closed` after an early
  dismissal) instead of "re-show without a duration and stay open", which cannot tell a cancelled
  countdown from a restarted one. The re-show scenario is covered separately by
  `AnUntimedMessageShownOverATimedOne_StaysOpen` and `ClearingTheDurationWhileOpen_StopsTheCountdown`.
- The timing tests run a real dispatcher timer (the headless platform fires it while a test awaits).
  Durations are 50 ms against 400 ms waits, plus one 1 s / 0.6 s restart case, so a heavily loaded
  machine has wide margins.
- Follow-up: the out-of-repo `enigma-avalonia-desktop` house skill (`reference/dialogs-overlay-infobar.md`)
  still says there is no auto-dismiss timer; update it with the release.
- Line endings: every touched file is LF with a final newline; nothing to recommend.

## Documentation sweep

`docs/guides/dialogs-overlay-infobar.md`, the sections the change made factually wrong:

- *Surfaces* table — the InfoBar row's "Opens with" / "Closes with".
- Service-member table — the timed overload row; the `ArgumentOutOfRangeException` sentence.
- *Key types* — `InfoBarServiceExtensions`.
- The `InfoBar` property sentence — `DisplayDuration`, its default and its valid range.
- Control-member table — `ShowAsync()` and `Closed` now mention the countdown.
- *An overlay around a long operation…* example — the `Task.WhenAny(dismissed, Task.Delay(5000))` +
  `HideAsync` workaround replaced by `ShowAsync(TimeSpan.FromSeconds(5), …)`; `using System;` added.
- *Notes* — the reset list gains `DisplayDuration`; "There is no auto-dismiss timer" replaced by the
  default-never / countdown rules.

Snippet check of the one touched code fence: `IInfoBarService.ShowAsync(TimeSpan, Action<InfoBar>?)`
(extension, `Enigma.Avalonia.Desktop.Services` — imported), `TimeSpan.FromSeconds` (`System` — added),
`InfoBarSeverity.Success` (`Enigma.Avalonia.Desktop.Controls.InfoBar` — imported): 1 snippet ·
3 new symbols · 0 mismatches · 0 uncertain.

README, CLAUDE.md and `docs/guides/README.md` were checked: nothing in them is contradicted.

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx` — **0 Warning(s), 0 Error(s)** (library for
  net8.0 + net10.0, showcase, tests; XAML compiler re-run by the clean).
- `dotnet test --solution Enigma.Avalonia.slnx` — **303 passed, 0 failed, 0 skipped** (22 new: 15 in
  `InfoBarDisplayDurationTests`, 7 in `InfoBarServiceTests`).
- Fix cycles used: 1 of 3 — the first build tripped xUnit2014/CS0619 (`Assert.Throws` on a
  `Task`-returning call); switched to `Assert.ThrowsAsync`.
