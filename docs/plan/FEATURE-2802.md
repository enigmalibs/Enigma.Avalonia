# FEATURE-2802 — Documentation (guides, packed README, SECURITY.md, CLAUDE.md)

**Status:** DONE · 2 phases
**Branches:** `feature/feature-2802-phase01-guides`, `feature/feature-2802-phase02-readme-community`
**Depends on:** FEATURE-22A5 (the API being documented), FEATURE-57C8 (working snippets to draw on)
**Solution invariants:** `docs/plan/FEATURE-28E8.md` §2.

## 1. Objective

Write the documentation the 1.0.0 release ships: nine per-family guides with an index, the packed
root `README.md` that becomes the nuget.org landing page, `SECURITY.md`, and the internal
`CLAUDE.md` agent guide.

**Guides before README** — the README's *Documentation* section summarises what the guides cover, so
writing it first means writing it twice. `RELEASENOTES.md` and `PackageReleaseNotes` belong to
FEATURE-1702, not here.

## 2. Clean-slate constraint

`README.md`, `RELEASENOTES.md`, every guide and `CLAUDE.md` are **repository artifacts** and therefore
subject to FEATURE-28E8 §2.4.10: **no reference to the port source, in any form** — not a
"supersedes", not a migration note, not a lineage aside. The library is documented purely on its own
terms. (`docs/plan/*.md` remains the sole exception.)

## 3. PHASE01 — Per-family guides + index — DONE

### Deliverables — `docs/guides/`

| File | Covers |
|---|---|
| `navigation.md` | `NavigationView`, `NavigationItem`, `NavigationOrientation`, `PaneSize`/`LabelMaxWidth`, `INavigationService`, `PageFactory`, the `INavigationViewModel` lifecycle (`OnAppearingAsync`/`OnDisappearingAsync`, cancelling navigation), serialized navigation, `NavigationFailedEventArgs` |
| `docking.md` | `DockingHost`, `DockPane`, `DockTabGroup`, `DockSplitContainer`, `DockLayoutNode`, `DockPosition` |
| `ribbon.md` | `Ribbon`, `RibbonTab`, `RibbonGroup`, `RibbonButton`, `RibbonToggleButton`, `RibbonDropDownButton`, `RibbonMenuItem` |
| `editors.md` | `BaseEditor`/`BaseEditor<T>` and all 14 typed editors, leading content, validation states and the `:error` pseudo-class, the Base64/hex byte-array editors |
| `dialogs-overlay-infobar.md` | `ContentDialog` (+ `DefaultButton`, `DialogResult`, the six `Dialog*` size properties), `Overlay`, `InfoBar` (+ `InfoBarSeverity`), their three services and the **`RegisterHost` requirement** |
| `settings-cards.md` | `SettingsCard`, `SettingsCardExpander` |
| `file-folder-dialogs.md` | `IFileDialogService`, `IFolderDialogService`, `SetStorageProvider`, the filter-helper extension methods |
| `data-collectionview.md` | `CollectionViewSource`, `CollectionView`, `SortDescription`/`SortDirection`, `FilterEventArgs`, `PropertyGroupDescription`, `CollectionViewGroup` |
| `theming.md` | Merging `Fluent.axaml` as a **`ResourceInclude`** in `Application.Resources`, the `Enigma*` colour/brush key reference, `ThemeDictionaries` Dark/Light, why `DynamicResource` (never `StaticResource`), and how standard Avalonia controls get restyled |
| `README.md` | The index: themed groups of relative links to the nine guides |

### Shape

Each guide follows `dotnet-release/templates/guide.md`: intro → operations/controls table → key types
table → `###`-per-scenario copy-pasteable usage → notes. Relative links **between** guides are correct
here (the guides are not packed).

Cross-cutting content every guide assumes and does not re-explain (it lives in the README quick start
and `theming.md`): the package install, the `ResourceInclude` line, DI registration of the six
services, and the host-control placement.

### Delegation

The nine guides are independent and split cleanly — write them **one per sub-agent**, giving each the
target path, the exact public API surface from `src/`, and the shape template. The owner integrates,
writes the index, and re-verifies every snippet before the phase is done.

### The snippet-verification gate — required

There is no compile harness for doc snippets, so drift is caught only here. Cross-check **every** API
reference in **every** code fence against the real public surface in `src/`: namespaces, type names,
member signatures, `async`/`await` shapes, enum members, default values, resource keys, and XAML
`xmlns` declarations. Fix mismatches in place. **Record coverage in the completion doc as a table** —
per file: snippets · symbols · mismatches · uncertain, with totals.

### Acceptance criteria

- All ten files exist under `docs/guides/`; every guide documents its whole family with no gaps
  against `src/`.
- Every snippet verified; the coverage table is in the completion doc with **0 mismatches**.
- Zero occurrences of the port source's name anywhere in `docs/guides/`.
- Nothing to build or test in this phase beyond the solution staying green — state that explicitly.

## 4. PHASE02 — Packed README, SECURITY.md, CLAUDE.md — DONE

### `README.md` (root, packed into the nupkg)

From `dotnet-release/templates/package-README.md`, in this section order: title → badges →
one-paragraph intro → what's-new callout (added in FEATURE-1702 PHASE02) → *Features* →
*Installation* → *Quick start* → *Documentation* → *License*.

- **Badges — exactly two, in this order:**
  `[![NuGet](https://img.shields.io/nuget/v/Enigma.Avalonia.Desktop.svg)](https://www.nuget.org/packages/Enigma.Avalonia.Desktop)`
  then `[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)`.
  No downloads badge.
- **Installation** must state the supported target frameworks and the fact that the package does
  **not** bring the Avalonia *app* packages transitively — consumers add `Avalonia.Desktop`,
  `Avalonia.Themes.Fluent` and `Avalonia.Fonts.Inter` themselves — and list what *is* transitive:
  `Avalonia`, `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm`, `Enigma.Core`.
  **Say plainly that `Enigma.Core` brings `BouncyCastle.Cryptography`**, and which two controls
  (`Base64Editor`, `HexadecimalEditor`) that dependency exists for. A consumer discovering a ~27 MB
  crypto dependency after installing a control library should have read it here first.
- **Quick start** carries the five bootstrap steps end to end: the `ResourceInclude` in
  `Application.Resources`; registering the six services in DI; placing `ContentDialog`, `Overlay` and
  `InfoBar` as siblings in the window's root `Panel`; the three `RegisterHost` + two
  `SetStorageProvider` calls at startup; injecting a service into a ViewModel and awaiting it.
- **Link rule (correctness, not style):** the README renders on nuget.org where the repo tree does not
  exist. Link only to packed or repo-root files (`LICENSE.md`, `RELEASENOTES.md`); point at
  `docs/guides/` **in prose** with no clickable per-guide links; no absolute GitHub URLs.
- Keep it summary-length — the guides carry the detail.

### `SECURITY.md` (root)

From `dotnet-release/templates/SECURITY.md`: supported versions, GitHub private vulnerability
reporting, what to expect, scope. Note in *Scope* that cryptographic operations reached through the
Base64/hex editors are implemented by `Enigma.Core`, and that vulnerabilities there belong to that
project's process.

### `CLAUDE.md` (root, internal — never packed)

Agent instructions for this solution: build/run/test/pack commands
(`dotnet build Enigma.Avalonia.slnx`, `dotnet test --solution Enigma.Avalonia.slnx`,
`dotnet run --project samples/Enigma.Avalonia.Desktop.Showcase`,
`dotnet pack src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj -c Release`), the
architecture (library / showcase / tests and their dependency direction), and the hard rules and
gotchas from FEATURE-28E8 §2.4 — with these called out explicitly because they cost real time when
rediscovered:

1. The `Enigma.Avalonia.*` namespace collision with `Avalonia.*` and the file-scoped-using fix.
2. `AVLN*` XAML warnings are **not** promoted by `TreatWarningsAsErrors` — read the warning count.
3. Compiled bindings need `x:DataType` (`AVLN2100`).
4. Custom controls are `TemplatedControl`; classes in `Controls/`, templates in `Themes/`; a new
   control also needs its `ResourceInclude` in `Fluent.axaml`.
5. `{TemplateBinding}` is one-way only — two-way template bindings use
   `{Binding …, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}`.
6. Hit testing needs a non-null `Background`.
7. Detach old handlers before attaching new ones in `OnApplyTemplate` (it can run more than once).
8. Property/command style: semi-auto properties with `SetProperty`, `IRelayCommand` properties
   initialised in the constructor; **never** `[ObservableProperty]` / `[RelayCommand]`; classes not
   `partial`.
9. Releasing is the user's job — `docs/RELEASE.md` is printed and followed, never executed; the NuGet
   API key is never stored, committed or echoed.
10. Where the truth lives: `docs/roadmap.md` + `docs/plan/<ID>.md` + `docs/done/<ID>.md`.

It must **not** mention the port source (§2).

### Acceptance criteria

- `README.md` renders correctly as markdown, is non-empty, follows the section order, carries exactly
  the two badges, contains no `docs/` link and no absolute GitHub URL, and states the BouncyCastle
  consequence.
- Every snippet in the README quick start passes the same verification gate as the guides.
- `SECURITY.md` and `CLAUDE.md` exist at the root.
- Zero occurrences of the port source's name in any repository file outside `docs/plan/`
  (`grep -ril carbon . --exclude-dir=docs/plan --exclude-dir=.git` returns nothing).
- Solution still builds and tests green.
