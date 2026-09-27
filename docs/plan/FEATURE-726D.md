# FEATURE-726D — InfoBar auto-close after a duration

**Status:** DONE · single phase
**Type:** FEATURE
**Branch:** `feature/feature-726d-infobar-auto-close`
**Run:** feature/2026-09-27-infobar-dialog-release

## 1. Objective

Let an `InfoBar` close itself after a caller-chosen `TimeSpan`, from the control and from
`IInfoBarService`, while keeping today's behaviour as the default: **an info bar never closes on its
own unless a duration is asked for**.

## 2. Context & constraints

- `InfoBar` (`src/Enigma.Avalonia.Desktop/Controls/InfoBar/InfoBar.cs`) has no timer today. It closes
  through its `PART_CloseButton`, `Close()`, `CloseAsync()` or `IInfoBarService.HideAsync()`, and
  `ShowAsync()` completes on `Closed`.
- `InfoBarService.ShowAsync(configure)` resets `Title`, `Message` and `Severity` on the single host
  before applying `configure` — "every message starts from a clean host" is its contract.
- The guide `docs/guides/dialogs-overlay-infobar.md` currently works around the missing feature with
  `Task.WhenAny(dismissed, Task.Delay(5000))` and states "There is no auto-dismiss timer on `InfoBar`".
- `1.0.0` is published. Everything here must be additive: `IInfoBarService` is a public interface
  consumers may implement (test fakes), so **no new interface member**.
- Solution rules (CLAUDE.md): zero warnings, XML docs on every public member, file-scope `using`s
  above the namespace (the `Enigma.Avalonia` / `Avalonia` collision), C# 14 extension blocks.

## 3. Design

### Control — `InfoBar.DisplayDuration`

- New styled property `DisplayDurationProperty` / `TimeSpan? DisplayDuration`, default `null`.
  `null` means **stays open until dismissed** (today's behaviour).
- Validated at set time through the property's `validate` callback: a value must be `null` or
  strictly positive and at most `int.MaxValue` milliseconds (the `DispatcherTimer` interval ceiling).
  An invalid value throws `ArgumentException` from `SetValue` — never later, when a timer starts.
  The rule lives in one `internal static bool IsValidDisplayDuration(TimeSpan?)` helper.
- The countdown is a one-shot `DispatcherTimer.RunOnce(...)` held as an `IDisposable`: it runs on the
  UI thread (no marshalling, no `async void`) and is cancelled synchronously by disposing it.
- `OnPropertyChanged`: a change to `IsOpen` or `DisplayDuration` restarts the countdown — started
  when the bar is open and has a duration, stopped otherwise. So closing early (button, `Close()`,
  `HideAsync()`) cancels it, and changing the duration while open restarts it with the new value.
- `ShowAsync()` also restarts the countdown when the bar was already open, so a new message shown on
  an open bar gets its full duration.
- When the countdown elapses it calls `Close()` — `IsOpen` goes false, `Closed` is raised, and the
  pending `ShowAsync()` task completes exactly as with a manual dismissal.

### Service — `InfoBarServiceExtensions`

- New `public static class InfoBarServiceExtensions` in `Enigma.Avalonia.Desktop.Services`, one C# 14
  `extension(IInfoBarService service)` block, mirroring `FileDialogServiceExtensions` /
  `FolderDialogServiceExtensions`:
  `Task ShowAsync(TimeSpan displayDuration, Action<InfoBar>? configure = null)`.
- Validates `displayDuration` up front (`ArgumentOutOfRangeException`, `ParamName`
  `"displayDuration"`) so an invalid value never leaves the host reset-but-unopened.
- Applies `configure` first, then `DisplayDuration = displayDuration` — the explicit argument wins.
- `InfoBarService.ResetInfoBar` gains `ClearValue(InfoBar.DisplayDurationProperty)`: a timed
  message never makes the next message timed.

### Showcase

- Services page, InfoBar section: one extra button, "Auto-close (5 s)", driven by a new
  `ShowTimedCommand` that uses the extension and records `LastInfoBarResult`.

## 4. Steps

1. `InfoBar.cs` — the property, the validation helper, the countdown, `OnPropertyChanged`, and the
   `ShowAsync` restart; XML docs on every new public member.
2. `InfoBarServiceExtensions.cs` — the extension block.
3. `InfoBarService.cs` — the reset line; its XML doc lists the reset properties.
4. Tests — `tests/…/Controls/InfoBarDisplayDurationTests.cs` and additions to
   `tests/…/Services/InfoBarServiceTests.cs`.
5. Showcase — the command and the button.
6. Documentation sweep — `docs/guides/dialogs-overlay-infobar.md`.

## 5. Acceptance criteria

- [x] `InfoBar.DisplayDuration` defaults to `null`, and a bar shown without a duration is still open
      after its would-be deadline has passed.
- [x] A bar shown with a short duration closes on its own: `IsOpen` becomes `false`, `Closed` is
      raised once, and the `ShowAsync()` task completes.
- [x] Closing a timed bar early cancels its countdown: re-showing it without a duration keeps it open
      past the original deadline.
- [x] Setting `DisplayDuration` to zero, a negative value, or more than `int.MaxValue` ms throws
      `ArgumentException`; `null` and positive values are accepted.
- [x] `IInfoBarService.ShowAsync(TimeSpan, Action<InfoBar>?)` opens a timed bar, the explicit
      duration wins over one set in `configure`, and an invalid duration throws
      `ArgumentOutOfRangeException` with `ParamName` `"displayDuration"` without opening the host.
- [x] `InfoBarService.ShowAsync` resets `DisplayDuration`: a message after a timed one is untimed.
- [x] The showcase has an auto-closing info bar demo.
- [x] Full build 0 warnings (including `AVLN*`), whole suite green.
- [x] The guide documents the property, the extension, the reset and replaces the `Task.WhenAny`
      workaround.

## 6. Out of scope

- Pausing the countdown while the pointer is over the bar (see Decisions).
- A close-reason on `Closed` (timer vs user).
- Any change to `IInfoBarService` itself.
- A progress indicator for the remaining time.

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| Where does the duration live? | A styled property on `InfoBar` | Works with and without the service; bindable and settable from XAML on a page-local bar | Service-only timer (a hand-driven bar could not use it) |
| Property name and type | `DisplayDuration`, `TimeSpan?`, `null` = never | The spec asks for a `TimeSpan` period and "never" by default; `null` says "no duration" without a magic value; `Duration` alone is ambiguous next to animations | `TimeSpan` with `Zero`/`InfiniteTimeSpan` sentinels; `AutoCloseDelay`; `int` milliseconds |
| How does the service expose it? | Extension `ShowAsync(TimeSpan, Action<InfoBar>?)` on `IInfoBarService` | Additive — a new interface member breaks every implementer and would force a major version; the house already extends its service interfaces this way | New `IInfoBarService` member; default interface method |
| Does the service reset it? | Yes, `ClearValue` | "Every message starts from a clean host" is the service's contract; a leaked duration would silently auto-close a later error message | Leave it (host-level default) |
| Explicit argument vs `configure` | The argument is applied after `configure` and wins | The overload's name promises the duration; `configure` is for content | `configure` wins |
| Timer mechanism | `DispatcherTimer.RunOnce`, disposed to cancel | UI-thread affinity, synchronous cancellation, no `async void`, no `ConfigureAwait` question | `Task.Delay` + `CancellationTokenSource`; `System.Threading.Timer` (needs marshalling) |
| Invalid values | Rejected by the property's `validate` callback | Fails at the call site that made the mistake, not at the moment the timer starts | Coercion to `null`; throwing when the timer starts |
| Pause on hover | Not included | Timed closing is opt-in and the default never closes; accessibility-driven timing is FEATURE-66EB's remit (deferred) | Pause while `IsPointerOver` |
