# FEATURE-6EB0 — Test suite (headless + unit)

**Status:** IN PROGRESS · 2 phases
**Branches:** `feature/feature-6eb0-phase01-unit-tests`, `feature/feature-6eb0-phase02-headless-tests`
**Depends on:** FEATURE-22A5 (all 8 phases)
**Solution invariants:** `docs/plan/FEATURE-28E8.md` §2.

## 1. Objective

Give `tests/Enigma.Avalonia.Desktop.UnitTests/` real coverage where it pays: the logic-dense
`CollectionView` subsystem, the six services' contracts, and a headless smoke test per control that
catches the single most likely fallout of the `Carbon*` → `Enigma*` resource-key rename — a template
that no longer resolves a brush.

The port source shipped an **empty test project with zero tests**. Closing that gap is the point of
this item; it is not a port.

## 2. Design

- One project, `net10.0`, xUnit v3 + `Avalonia.Headless.XUnit` under Microsoft.Testing.Platform
  (created in FEATURE-28E8 PHASE02).
- `TestAppBuilder.cs` carries the `[AvaloniaTestApplication]` attribute and builds a headless app
  whose `Application.Resources` merges
  `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — **this is what makes the resource-key
  tests meaningful**, so it must merge the real dictionary, not a stub.
- Headless UI tests use `[AvaloniaFact]` / `[AvaloniaTheory]`; pure-logic tests use `[Fact]` /
  `[Theory]`.
- Namespaces mirror the project: `Enigma.Avalonia.Desktop.UnitTests`, with `.Data`, `.Services` and
  `.Controls` sub-namespaces matching the folders.
- Pass `TestContext.Current.CancellationToken` into anything async; never write `async void` tests.
- `ITestOutputHelper` comes from `Xunit` (there is no `Xunit.Abstractions` in v3).

## 3. PHASE01 — CollectionView & service-contract unit tests — DONE

### Steps

1. **`Data/` tests** — the bulk of the phase, no UI thread needed:
   - `CollectionViewSource` projects its source; enumeration order matches insertion when no sort is
     applied.
   - Sorting: single `SortDescription`, multiple descriptions applied in order, `Ascending` vs
     `Descending`, re-sort on a property change.
   - Filtering: `FilterEventArgs` accept/reject, filter changed → view refreshed, filter combined with
     sort.
   - Grouping: `PropertyGroupDescription` produces the expected `CollectionViewGroup` set; group
       membership follows a re-assigned property; grouping combined with sort and filter.
   - Change notification: `INotifyCollectionChanged` add / remove / replace / reset on the source each
     propagate to the view; an item added under an active filter appears only if it passes.
   - Edge cases: empty source, single item, all items filtered out, duplicate sort keys, `null`
     property values in a group key.
2. **Service-contract tests** — one class per service:
   - `ContentDialogService`, `OverlayService`, `InfoBarService`: calling the show/hide API **before**
     `RegisterHost` throws `InvalidOperationException`; after `RegisterHost` it does not; a second
     `RegisterHost` call behaves as the ported implementation does (assert the actual behaviour, do
     not change it).
   - `FileDialogService`, `FolderDialogService`: calling a picker before `SetStorageProvider` throws;
     the filter-building extension methods produce the expected `FilePickerFileType` values.
   - `NavigationService`: `PageFactory` is invoked for the selected item; `OnAppearingAsync` receives
     the item's parameter; `OnDisappearingAsync` returning `false` cancels the navigation and leaves
     `CurrentPage` unchanged; a navigation raised while one is in flight is **dropped** (the
     `SemaphoreSlim` contract), asserted with a deliberately slow fake `INavigationViewModel`;
     a factory that throws surfaces through `NavigationFailedEventArgs`.

### Acceptance criteria

- `dotnet test --solution Enigma.Avalonia.slnx` passes with zero warnings.
- Every branch of `CollectionView`'s sort / filter / group pipeline is exercised, including the
  combinations, not just each feature alone.
- All six services have a "used before initialisation throws" test.
- The navigation drop-on-concurrent and cancel-on-false behaviours are both asserted.
- No test asserts a behaviour that differs from FEATURE-22A5's ported code — if a test would fail
  against the port, the **test** is wrong, not the port. Record any such discovery in the completion
  doc instead of changing library code.

## 4. PHASE02 — Headless control smoke & resource-key tests — TODO

### Steps

1. **Resource-key existence test** — the rename guard. Assert that every key the 20 control templates
   reference resolves from the merged `Fluent.axaml`, in **both** theme variants
   (`ThemeVariant.Dark` and `ThemeVariant.Light`). Build the expected key list from the
   `Colors.axaml`/`Brushes.axaml` map recorded in FEATURE-22A5 PHASE01's completion doc, and assert
   no key still starts with `Carbon`.
2. **Per-control smoke tests** — for each of the 22 templated controls (`NavigationView`,
   `NavigationItem`, `ContentDialog`, `Overlay`, `InfoBar`, `SettingsCard`, `SettingsCardExpander`,
   `Ribbon`, `RibbonTab`, `RibbonGroup`, `RibbonButton`, `RibbonToggleButton`,
   `RibbonDropDownButton`, `DockingHost`, `DockPane`, `DockTabGroup`, `DockSplitContainer`, and the
   editors via `BaseEditor`/`MultiLineTextEditor`): instantiate it in a headless window, force a
   layout pass, and assert the template applied (`ApplyTemplate()` returns true / the expected
   `PART_*` parts are found) and nothing threw.
3. **Editor round-trip tests** (headless, since editors are controls): each typed editor formats and
   parses its own type; `Base64Editor` and `HexadecimalEditor` round-trip a byte array through
   `Enigma.Core`'s services; invalid text sets the `:error` pseudo-class and leaves `Value`
   unchanged.
4. **Theme-switch test:** switch `RequestedThemeVariant` at runtime with controls loaded and assert no
   exception and that a sampled brush actually changed — proof the `DynamicResource` lookups survived
   the rename.

### Acceptance criteria

- `dotnet test --solution Enigma.Avalonia.slnx` passes; total suite in the 40–60 test range or above.
- Every templated control has at least one applied-template assertion.
- The resource-key test fails loudly if any template references a key the dictionary does not define
  (verify by temporarily renaming one key, then reverting — record that check in the completion doc).
- Zero warnings; no `Microsoft.NET.Test.Sdk` or `xunit.runner.visualstudio` anywhere.
