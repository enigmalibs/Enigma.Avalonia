# FEATURE-22A5 PHASE02 — CollectionView data subsystem

**Branch:** `feature/feature-22a5-phase02-collectionview`
**Status:** DONE

## Summary

Added the `Data/` namespace to `Enigma.Avalonia.Desktop` — the seven-file `CollectionView` data
subsystem that gives the library a filterable, sortable and groupable view over any `IEnumerable`,
plus the bindable `AvaloniaObject` wrapper (`CollectionViewSource`) that exposes it to XAML.

The subsystem has no dependency on any control, which is why it lands before the control families:
it is the one part of the library that `FEATURE-6EB0` can unit-test without a headless UI, and every
later phase can ignore it.

Public surface, as shipped:

| Type | Kind | Surface |
|---|---|---|
| `CollectionView` | class : `IEnumerable`, `INotifyCollectionChanged`, `INotifyPropertyChanged` | `SourceCollection`, `Filter`, `SortDescriptions`, `GroupDescriptions`, `Groups`, `Count`, `IsEmpty`, `Refresh()`, `DeferRefresh()`, `GetEnumerator()`, `CollectionChanged`, `PropertyChanged` |
| `CollectionViewSource` | class : `AvaloniaObject` | `SourceProperty` (styled), `ViewProperty` (direct), `Source`, `View`, `SortDescriptions`, `GroupDescriptions`, `Filter` event |
| `CollectionViewGroup` | class | `Key`, `Items`, `ItemCount` |
| `SortDescription` | class : `AvaloniaObject` | `PropertyNameProperty`, `DirectionProperty`, `PropertyName`, `Direction`, `DescriptionChanged` |
| `PropertyGroupDescription` | class : `AvaloniaObject` | `PropertyNameProperty`, `ValueConverterProperty`, `PropertyName`, `ValueConverter`, `DescriptionChanged` |
| `SortDirection` | enum | `Ascending`, `Descending` |
| `FilterEventArgs` | class : `EventArgs` | `Item`, `Accepted` (defaults `true`) |

`CollectionView.Detach()` is deliberately `internal` — `CollectionViewSource` is its only caller, and
promoting it would put source-subscription lifetime into the public contract.

## Files/modules touched

**Created** — all under `src/Enigma.Avalonia.Desktop/Data/` (555 lines total):

| File | Lines |
|---|---|
| `CollectionView.cs` | 221 |
| `CollectionViewSource.cs` | 171 |
| `PropertyGroupDescription.cs` | 49 |
| `SortDescription.cs` | 44 |
| `CollectionViewGroup.cs` | 29 |
| `FilterEventArgs.cs` | 28 |
| `SortDirection.cs` | 13 |

**Modified:** `docs/roadmap.md`, `docs/plan/FEATURE-22A5.md` (PHASE02 status `TODO` → `IN PROGRESS`
→ `DONE`).

**Not modified:** `Themes/Fluent.axaml`. Plan §4 says every phase appends its template include, but
this phase ships no `.axaml` — the data subsystem is code only. PHASE08's "exactly 22 dictionaries
(2 foundation + 20 control templates)" arithmetic already assumes PHASE02 contributes nothing, so
the two statements agree; §4's wording is the loose one.

**No csproj change was needed:** `.cs` files under the project directory are compiled by the SDK's
default glob, and the subsystem introduces no new package dependency — `AvaloniaList<T>`,
`AvaloniaObject`, `StyledProperty<T>`, `DirectProperty<,>` and `IValueConverter` all come from the
existing `Avalonia` reference. The library's four `PackageReference`s are unchanged (plan §5).

## Deviations & follow-ups

Three mechanical transformations beyond the plan's §3 list were required, each forced by a
FEATURE-28E8 §2 solution invariant — which §2 itself declares wins over any later plan. None of them
changes a declaration, a default value, or observable behaviour.

1. **Explicit `using` directives added (§2.4.2).** The port source compiles with
   `ImplicitUsings=enable`; this solution sets it `disable` solution-wide, so the implicitly-imported
   namespaces had to become real directives. Added: `using System;` to five files
   (`CollectionView`, `CollectionViewSource`, `FilterEventArgs`, `PropertyGroupDescription`,
   `SortDescription`) and `using System.Collections.Generic;` to two (`CollectionView`,
   `CollectionViewGroup`). `SortDirection.cs` needed none. Ordering follows the repo's
   `dotnet_sort_system_directives_first = true` with no group separator, and all directives sit
   above the file-scoped `namespace` — which §2.4.11 requires anyway and `.editorconfig` enforces at
   warning level (`csharp_using_directive_placement = outside_namespace:warning`).
2. **`global::` removed (§3.6).** `PropertyGroupDescription.cs` carried the phase's single
   `global::Avalonia.…` occurrence — `using global::Avalonia.Data.Converters;`. It is now
   `using Avalonia.Data.Converters;`. A compilation-unit-level `using` resolves against the global
   namespace, so the `Enigma.Avalonia` / `Avalonia` collision §2.4.11 warns about cannot arise here;
   the qualifier was redundant. Verified by the clean build, and no `global::` remains under `Data/`.
   That leaves 28 of the 29 §3.6 occurrences for PHASE03–08.
3. **`!` justified (§2.4.3).** `CollectionViewSource.OnSourceChanged` ends with `_view!.Refresh()`.
   The suppression is load-bearing, not cosmetic: `SetAndRaise(ViewProperty, ref _view, view)` takes
   the field by `ref`, which resets the compiler's null-state for it to the declared
   `CollectionView?`, so the call would otherwise be CS8602 — an error under
   `TreatWarningsAsErrors`. §2.4.3 forbids an uncommented `!`, so a three-line comment above the call
   records why it is safe. The `!` itself is the port source's, unchanged.

**Line endings:** no CRLF recommendation to make. All seven new files are LF with a final newline;
the repository's `.gitattributes` LF rule (FEATURE-28E8 PHASE01) is doing its job.

**Follow-ups for `FEATURE-6EB0` PHASE01** — behaviours worth pinning down with tests, all inherited
verbatim from the port source and none of them a defect to fix here:

- `CollectionView` groups by `GroupDescriptions[0]` only; additional descriptions are accepted and
  silently ignored. Test the documented behaviour, not multi-level grouping.
- `DeferRefresh()` nesting is counted (`_deferLevel`), and a `DeferToken` disposed twice is a no-op
  — it nulls its back-reference on first disposal.
- `GetPropertyValue` caches accessors per `(Type, PropertyName)` in a `static` dictionary that lives
  for the process; a missing property caches a `_ => null` accessor rather than throwing.
- `SortDescriptions`/`GroupDescriptions` are settable on `CollectionView` but get-only on
  `CollectionViewSource`, which hands its own instances to the view it builds.
- `CollectionViewSource.Filter` is captured once, when the view is built in `OnSourceChanged` — a
  handler subscribed after `Source` is set is not picked up until `Source` changes again.

## Build/test evidence

- **Build:** `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental` → **Build succeeded,
  0 Warning(s), 0 Error(s)**, for `net8.0` and `net10.0`. Per §2.4.1 the warning count was read
  from the log rather than trusting the exit code; no `AVLN*` diagnostic appears.
- **Tests:** `dotnet test --solution Enigma.Avalonia.slnx -c Release` → **1 total, 1 succeeded,
  0 failed, 0 skipped**. This phase adds no tests by design — plan §4's test note assigns the
  automated suite to `FEATURE-6EB0`, so Definition-of-Done criterion 2 is met by the existing suite
  continuing to pass.
- **1:1 port fidelity:** each ported file was diffed against its source with only the namespace
  rename applied. The complete diff is the three transformations above and nothing else — every
  type, member, default value, comment and blank line is byte-identical. That is the evidence for
  the "byte-for-byte equivalent in shape" acceptance criterion.
- **`grep -ri carbon src/`** → zero hits (§3.7).
- **Excluded-type gate** — `CalendarSchedule|Displayer2D|DrawingObject|Shape` across `src/` → zero
  hits (§2).
- **File count:** 7 of 7 present under `Data/`.

## Acceptance criteria

| Criterion | Status |
|---|---|
| Build clean | Met — 0 warnings, both TFMs, Release, non-incremental |
| All 7 files present under `Data/` | Met |
| Public surface equivalent in shape to the source | Met — diff shows only the three sanctioned transformations |
| Zero `Carbon` hits | Met |
