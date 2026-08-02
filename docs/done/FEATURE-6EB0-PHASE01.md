# FEATURE-6EB0 PHASE01 — CollectionView & service-contract unit tests

**Branch:** `feature/feature-6eb0-phase01-unit-tests`
**Status:** DONE

## Summary

Gave `tests/Enigma.Avalonia.Desktop.UnitTests/` its first real coverage: **131 new tests** across
twelve classes, taking the suite from 1 test (the PHASE02 theme smoke test) to **132**. Nothing in
`src/` changed — this phase only adds tests.

Two areas, matching the plan's split:

| Area | Class | Tests |
|---|---|---|
| `Data/` | `CollectionViewSortingTests` | 10 |
| | `CollectionViewFilteringTests` | 6 |
| | `CollectionViewGroupingTests` | 12 |
| | `CollectionViewNotificationTests` | 11 |
| | `CollectionViewEdgeCaseTests` | 8 |
| | `CollectionViewSourceTests` | 17 |
| `Services/` | `ContentDialogServiceTests` | 9 |
| | `OverlayServiceTests` | 8 |
| | `InfoBarServiceTests` | 7 |
| | `FileDialogServiceTests` | 16 |
| | `FolderDialogServiceTests` | 10 |
| | `NavigationServiceTests` | 17 |

The `Data/` tests are plain `[Fact]` — verified by probe that an `AvaloniaObject`'s styled
properties can be set off the dispatcher thread in this assembly, so `SortDescription` and
`PropertyGroupDescription` need no headless fixture. The `Services/` tests are `[AvaloniaFact]`
wherever they instantiate a `Control` (all three host services, `NavigationService`, and anything
needing a storage provider); the pure "throws before initialisation" cases stay `[Fact]`.

`TestAppBuilder` is unchanged. Merging `Themes/Fluent.axaml` into `Application.Resources`
(plan §2) is what makes PHASE02's resource-key tests meaningful; no PHASE01 test resolves a brush,
so the change belongs with the tests that need it.

### Behaviours pinned, not assumed

Several tests exist to lock in what the port actually does, where a reader might expect otherwise.
Each is documented in the test's XML comment:

- `CollectionView`'s constructor does **not** project the source — the view is empty until the first
  `Refresh()`. `CollectionViewSource` hides this by refreshing on assignment.
- Only `GroupDescriptions[0]` is applied; there is no nested-group model, so a second description is
  silently inert.
- The sort runs through `List<T>.Sort`, which is not stable. The duplicate-key test asserts the
  **key sequence**, deliberately not the relative order of tied items.
- `CollectionViewSource` wires the `Filter` event into the view only while `Source` is being
  assigned — a handler attached afterwards never runs. Subscribe before setting `Source`.
- All three host services replace their host on a second `RegisterHost` (one host, not a stack);
  their `HideAsync` is a no-op with no host, while `ShowAsync` throws.
- `OverlayService.ShowAsync` checks the host **before** the null argument, so an unregistered
  service reports the missing host even when handed `null`.
- A throwing `OnDisappearingAsync` is reported on `NavigationFailed` but treated as consent — a
  broken guard must not trap the user on the current page.

### Mutation-verified, not just green

The plan's two flagship navigation criteria were checked by breaking the library and confirming the
tests catch it, then reverting (`src/` is byte-identical to its state at branch point —
`git diff --stat src/` is empty):

| Mutation | Result |
|---|---|
| `SemaphoreSlim _navigationLock = new(1, 1)` → `new(2, 2)` | 3 tests fail, incl. `ANavigationRaisedWhileOneIsInFlight_IsDropped` |
| `if (!allowed) return;` → result discarded | 1 test fails: `OnDisappearingAsyncReturningFalse_CancelsTheNavigation` |

## Files/modules touched

**Created** — all under `tests/Enigma.Avalonia.Desktop.UnitTests/`:

| File | Purpose |
|---|---|
| `Data/TestPerson.cs` | The mutable model plus `ViewProjection`, the name/key projection helpers |
| `Data/CollectionViewSortingTests.cs` | Description priority, direction, inert descriptions, null keys |
| `Data/CollectionViewFilteringTests.cs` | Predicate accept/reject, filter swap, filter × sort |
| `Data/CollectionViewGroupingTests.cs` | Key derivation, group order, converter, grouping × sort × filter |
| `Data/CollectionViewNotificationTests.cs` | Source add/remove/replace/reset, the raised events, `DeferRefresh` |
| `Data/CollectionViewEdgeCaseTests.cs` | Null/empty/single source, everything filtered, enumerator |
| `Data/CollectionViewSourceTests.cs` | `Source`/`View` lifecycle, `Filter` event, description subscriptions |
| `Services/RecordingDialogServices.cs` | `HeadlessStorage` plus the recording file/folder dialog services |
| `Services/ContentDialogServiceTests.cs` | Host guard, reset between dialogs, `DialogResult` resolution |
| `Services/OverlayServiceTests.cs` | Host guard, argument guard, content clearing on hide |
| `Services/InfoBarServiceTests.cs` | Host guard, reset between messages, pending-task completion |
| `Services/FileDialogServiceTests.cs` | Provider guard, forwarding, options built by both extensions |
| `Services/FolderDialogServiceTests.cs` | Provider guard, forwarding, options built by the extension |
| `Services/NavigationServiceTests.cs` | Page factory, lifecycle callbacks, the lock, the failure channel |

**Modified:** `docs/roadmap.md`, `docs/plan/FEATURE-6EB0.md` (item + PHASE01 status `TODO` →
`IN PROGRESS` → `DONE`).

**Not modified:** anything under `src/`, the test `.csproj` (no new package needed —
`xunit.v3` and `Avalonia.Headless.XUnit` cover everything), and `TestAppBuilder.cs`.

## Deviations & follow-ups

Three of the plan's §3 steps describe surface that does not exist in the ported code. Per the
phase's own acceptance criterion 5 — *"if a test would fail against the port, the **test** is wrong,
not the port"* — the tests were written against the shipped behaviour and the gaps recorded here.
**No library code was changed.**

1. **"the filter-building extension methods produce the expected `FilePickerFileType` values."**
   There are no filter-building extensions. `FileDialogServiceExtensions` *accepts* an
   `IReadOnlyList<FilePickerFileType>` (`fileTypeFilter` / `fileTypeChoices`) and passes it into the
   options. Covered as written: `TheOpenExtension_BuildsTheExpectedOptions` asserts the list reaches
   `FilePickerOpenOptions.FileTypeFilter` with its `Name`, `Patterns` and `MimeTypes` intact, and
   the save equivalent covers `FileTypeChoices`.

2. **"`OnAppearingAsync` receives the item's parameter."** `NavigationItem` carries no parameter.
   A parameter only reaches the callback through `NavigateToAsync(page, parameter)`, which
   `NavigateToAsync_PassesItsParameterToOnAppearingAsync` asserts. The item-driven path always
   passes `null`, pinned by `ItemDrivenNavigation_CallsOnAppearingAsyncWithoutAParameter`.

3. **"All six services have a 'used before initialisation throws' test."** Five do, literally.
   `NavigationService` has no initialisation step — `PageFactory` is assigned in its constructor and
   is never null. Its equivalent failure is a `NavigationItem` whose `PageType` was never
   configured: the default factory's `Activator.CreateInstance(null)` throws and the service reports
   it on `NavigationFailed` with phase `PageFactory`, asserted by
   `AnUnconfiguredNavigationItem_SurfacesThroughNavigationFailed`. **This criterion is met in
   substance, not to the letter** — there is no sixth literal guard to test.

Two constraints found while building, worth knowing before PHASE02:

4. **`IStorageProvider` cannot be faked.** Avalonia 12 marks `IStorageProvider`, `IStorageItem`,
   `IStorageFile` and `IStorageFolder` as not client-implementable (the interfaces carry a member
   whose name is unspeakable in C#), so a hand-written stub does not compile. The workaround —
   `HeadlessStorage.Provider()` — takes the real BCL-backed provider off a headless `Window`; its
   pickers cancel and its path lookups hit the real file system. Options recording is done one level
   up, through recording fakes of *our* `IFileDialogService` / `IFolderDialogService`, which are not
   closed. Consequence: the exact `FilePickerOpenOptions` instance handed to the *platform* provider
   cannot be inspected, only that `FileDialogService` forwards to it without throwing.

5. **`CollectionView.Refresh()` reads `SortDescriptions` and `GroupDescriptions` as properties, not
   snapshots.** `CollectionViewSource` assigns its own lists into the view by reference, which is
   why mutating `source.SortDescriptions` refreshes the view. Asserted by
   `TheWrapperDescriptions_AreTheOnesTheViewUses`; worth remembering if either list is ever made
   read-only.

**Line endings:** every file added by this phase is LF with a final newline; no CRLF churn was
observed anywhere in the working tree, so there is nothing to recommend.

## Build/test evidence

- `dotnet build Enigma.Avalonia.slnx` — **Build succeeded, 0 Warning(s), 0 Error(s)**, both library
  TFMs (`net8.0`, `net10.0`) plus the showcase and the test project. Warning count read from the
  build output, not inferred from the exit code (solution invariant §2.4.1).
- `dotnet test --solution Enigma.Avalonia.slnx` — **132 passed, 0 failed, 0 skipped**, under
  Microsoft.Testing.Platform. Run three consecutive times with identical results, to rule out
  ordering flakiness from mixing `[Fact]` and `[AvaloniaFact]` in one assembly.
- Mutation checks: see the table above. `src/` restored and verified byte-identical afterwards.

### Acceptance criteria

| Criterion | Status |
|---|---|
| `dotnet test --solution Enigma.Avalonia.slnx` passes with zero warnings | Met |
| Every branch of the sort / filter / group pipeline exercised, including combinations | Met — filter × sort, sort × group, and filter × sort × group each have a dedicated test |
| All six services have a "used before initialisation throws" test | Met in substance — five literal guards; `NavigationService` has no initialisation step (deviation 3) |
| Drop-on-concurrent and cancel-on-false both asserted | Met, and mutation-verified |
| No test asserts behaviour differing from the port | Met — no library change; the three plan/code gaps are recorded above |
