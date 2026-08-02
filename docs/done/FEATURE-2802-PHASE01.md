# FEATURE-2802 PHASE01 — Per-family guides + index

**Branch:** `feature/feature-2802-phase01-guides`
**Status:** DONE

## 1. Summary

Wrote the nine per-family guides the 1.0.0 release ships, plus the index that groups them. Each
guide follows `dotnet-release/templates/guide.md` — intro → controls/operations table → key types
table → `###`-per-scenario copy-pasteable usage → notes — and documents its whole family against the
real public surface in `src/`.

The nine guides were written one per sub-agent (as the plan directs); the index, the integration and
the snippet-verification gate are the owner's work. The gate was run as an actual **compile
harness**, not a reading pass: every code fence was extracted from the finished markdown by script
and compiled against the real `Enigma.Avalonia.Desktop` assembly. See §4.

## 2. Files created

All under `docs/guides/` (new directory), 10 files, LF with a final newline:

| File | Lines | Covers |
|---|---|---|
| `README.md` | 55 | The index: themed groups (Foundation · Application shell · Input and content · Services) |
| `theming.md` | 356 | `Fluent.axaml` as a `ResourceInclude`, 29 colour + 26 brush + 23 FluentTheme override keys, `ThemeDictionaries`, `DynamicResource`, runtime variant switching |
| `navigation.md` | 365 | `NavigationView`, `NavigationItem`, `NavigationOrientation`, `INavigationService`, `PageFactory`, `INavigationViewModel` lifecycle, serialized navigation, `NavigationFailedEventArgs` |
| `ribbon.md` | 330 | `Ribbon`, `RibbonTab`, `RibbonGroup`, `RibbonButton`, `RibbonToggleButton`, `RibbonDropDownButton`, `RibbonMenuItem` |
| `docking.md` | 321 | `DockingHost`, `DockPane`, `DockTabGroup`, `DockSplitContainer`, `DockLayoutNode` (+ `DockPaneModel`/`DockSplitModel`/`DockTabGroupModel`), `DockPosition`, `DockTabGroupEventArgs` |
| `editors.md` | 455 | `BaseEditor`/`BaseEditor<T>`, `ByteArrayEditor` + all 14 typed editors, leading/action content, validation and `:error`, Base64/hex encoding via `Enigma.Core` |
| `settings-cards.md` | 211 | `SettingsCard`, `SettingsCardExpander` |
| `data-collectionview.md` | 358 | `CollectionViewSource`, `CollectionView`, `SortDescription`/`SortDirection`, `FilterEventArgs`, `PropertyGroupDescription`, `CollectionViewGroup` |
| `dialogs-overlay-infobar.md` | 385 | `ContentDialog` (+ `DefaultButton`, `DialogResult`, the six `Dialog*` size properties), `Overlay`, `InfoBar` (+ `InfoBarSeverity`), the three services and the `RegisterHost` requirement |
| `file-folder-dialogs.md` | 311 | `IFileDialogService`, `IFolderDialogService`, `SetStorageProvider`, the `*ServiceExtensions` overloads |

Modified: `docs/roadmap.md`, `docs/plan/FEATURE-2802.md` (status flips only).

No source, test or sample file was touched.

## 3. Snippet-verification coverage

Extraction and compilation are scripted, so these counts are measured, not estimated.

| File | C# fences | XAML fences | Other | Total | Symbols | Mismatches | Uncertain |
|---|---|---|---|---|---|---|---|
| `theming.md` | 2 | 5 | 1 | 8 | ~52 | 1 | 6 |
| `navigation.md` | 5 | 2 | 0 | 7 | 49 | 6 | 5 |
| `ribbon.md` | 2 | 3 | 0 | 5 | ~57 | 3 | 6 |
| `docking.md` | 2 | 3 | 0 | 5 | 57 | 2 | 4 |
| `editors.md` | 7 | 6 | 0 | 13 | 93 | 1 | 4 |
| `settings-cards.md` | 1 | 4 | 0 | 5 | 50 | 0 | 3 |
| `data-collectionview.md` | 4 | 1 | 0 | 5 | 56 | 3 | 4 |
| `dialogs-overlay-infobar.md` | 3 | 1 | 0 | 4 | ~75 | 3 | 3 |
| `file-folder-dialogs.md` | 5 | 1 | 0 | 6 | ~37 | 2 (+1 owner) | 5 |
| `README.md` | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| **Totals** | **31** | **26** | **1** | **58** | **~526** | **24** | **40** |

**Mismatches remaining after the gate: 0.** The 24 counted above were all found and fixed before the
file shipped — 23 by the authoring sub-agents, 1 by the owner's integration gate (below).

The single "other" fence is `theming.md`'s quoted `AVLN2000` build-error text, which is output, not
code.

### The one mismatch the integration gate caught

`file-folder-dialogs.md` fence 1 declared `namespace MyApp;` and used `MainWindow` with no `using`
for it, violating the "every snippet carries its full using block" rule. Fixed by adding
`using MyApp.Views;`.

## 4. How the gate was run

There is no compile harness for doc snippets in this repo, so one was built in the scratchpad:

1. A script extracts every fence from the finished markdown verbatim, C# into `.cs` files and
   XAML into `.axaml` files.
2. They compile in a project that `ProjectReference`s `src/Enigma.Avalonia.Desktop`, with
   `ImplicitUsings=disable`, `Nullable=enable`, `LangVersion=14` and
   `AvaloniaUseCompiledBindingsByDefault=true` — matching the solution's own settings, so a missing
   `using`, a wrong property name or an absent `x:DataType` is a build error.
3. Reader-supplied placeholder types (`MyApp.Views.*`, `MyApp.ViewModels.*`) are stubbed with
   exactly the members the surrounding prose says the reader provides.

**Result: 0 errors, 0 warnings, on both `net8.0` and `net10.0`.**

**Negative controls** confirm the gate is real rather than vacuously passing:

| Injected defect | Detected as |
|---|---|
| `HasValidationError` → `HasValidationErrorXX` in a C# fence | `error CS1061` |
| `FooterItems` → `FooterItemsXX` in a XAML fence | `error AVLN2000` |

Both restored; the build returns to 0/0.

Three sub-agents independently built their own equivalent harnesses while drafting
(`navigation.md`, `docking.md`, `data-collectionview.md`), and `theming.md` and
`data-collectionview.md` additionally *ran* their snippets headless — the inline output comments in
those two guides are observed values, not asserted ones.

### What the gate does not prove

- **Path geometry content.** The Avalonia XAML compiler accepts any string for `Geometry`/`IconData`
  without validating the path mini-language, so a compiling `IconData` may still render nothing.
  The `navigation.md` author caught this and runtime-parsed all six of that guide's geometries
  through `Geometry.Parse` under `Avalonia.Headless`; geometries in other guides are compile-checked
  only.
- **Runtime behaviour**, except where a guide's author ran it (noted above).

## 5. Acceptance criteria

| Criterion | Status |
|---|---|
| All ten files exist under `docs/guides/`; every guide documents its whole family with no gaps against `src/` | Met — 10 files; `DockPaneModel`/`DockSplitModel`/`DockTabGroupModel`/`DockTabGroupEventArgs` added to docking beyond the plan's list to close the gap |
| Every snippet verified; coverage table in the completion doc with 0 mismatches | Met — §3, 0 mismatches remaining, verified by compile |
| Zero occurrences of the port source's name anywhere in `docs/guides/` | Met — `grep -ril carbon docs/guides/` returns nothing |
| Nothing to build or test beyond the solution staying green — state explicitly | Met — see below |

**Definition of Done criteria 1–2.** This phase adds documentation only; no source, test or project
file was touched, so there was nothing new to build and no new tests to write. The solution was
verified green before and after: `dotnet build Enigma.Avalonia.slnx` → **0 warnings, 0 errors**;
`dotnet test --solution Enigma.Avalonia.slnx` → **281 passed, 0 failed**. Separately, the doc
snippets themselves were compiled (§4).

## 6. Deviations & follow-ups

### Deviations from the plan

1. **"Filter-helper extension methods" do not exist.** The plan's `file-folder-dialogs.md` row names
   them, but `FileDialogServiceExtensions` / `FolderDialogServiceExtensions` contain no filter
   builders — they are same-named overloads that assemble the options object and project the result
   to local path strings. Filters are Avalonia's own `FilePickerFileType` / `FilePickerFileTypes`.
   The guide documents what exists, with a full signature table mapping every parameter to the
   option property it sets.
2. **Docking covers four types the plan's table omits** (`DockPaneModel`, `DockSplitModel`,
   `DockTabGroupModel`, `DockTabGroupEventArgs`), because the acceptance criterion requires no gaps
   against `src/`.
3. **Guide length.** The plan quotes the template's "roughly 100–300 lines"; six guides run
   211–456. The overage is reference-table coverage demanded by the no-gaps criterion. Every author
   trimmed at least once before stopping.
4. **`DockPosition` has no public member that accepts it** — the host computes it internally. The
   docking guide states this rather than implying a surface that is not there.

### Follow-ups for later phases or items

1. **PHASE02's repo-wide clean-slate criterion is not yet satisfiable as written.** It requires
   `grep -ril carbon . --exclude-dir=docs/plan --exclude-dir=.git` to return nothing, but the name
   currently survives in:
   - `tests/Enigma.Avalonia.Desktop.UnitTests/Themes/ResourceKeyTests.cs` — two test method names
     (`NoControlTemplate_ReferencesACarbonPrefixedKey`, `NoThemeDictionary_DefinesACarbonPrefixedKey`)
     and three `StartsWith("Carbon", …)` guard strings. These are deliberate rename-regression
     guards; renaming the methods is easy, but the guard *strings* are the test's whole point, so
     PHASE02 needs a decision, not a blind `sed`.
   - Nine `docs/done/*.md` completion records, which are historical and arguably should not be
     rewritten.

   Worth resolving explicitly in PHASE02 rather than discovering at its acceptance check.
2. **Stale XML doc on `INavigationViewModel`** (`src/.../Services/INavigationViewModel.cs:7-8`): it
   says the interface "can be implemented by either page views (Controls) or their ViewModels" and
   references an `INavigationLifecycle` type. `NavigationService` only ever inspects
   `page.DataContext`, and no `INavigationLifecycle` exists. The guide documents the real behaviour.
3. **Behavioural findings from the dialogs family**, read from source, not fixed here — each is a
   candidate bug, not a documentation problem:
   - `DefaultButton` has no effect in the shipped theme: no `[DefaultButton=…]` selector in
     `ContentDialog.axaml`, and `ContentDialog.cs` never calls `Focus()`. There is no Enter-to-commit.
   - `Escape` closes the dialog only while focus is already inside it; nothing moves focus into the
     card on open.
   - `ContentDialogService.ResetDialog` does not reset the six `Dialog*` size properties,
     `OverlayBrush` or `DialogResult`, so a size set in one `configure` action leaks into every later
     dialog on the shared host.
   - `InfoBar` is a `ContentControl` whose template has no `ContentPresenter`, so `Content` is
     silently invisible.
4. **`CollectionView` refresh asymmetry**, measured while writing the data guide: a hand-built
   `CollectionView` does not refresh when sort/group descriptions are added or edited (only
   `CollectionViewSource` subscribes to `DescriptionChanged`), and a description already present when
   `Source` is assigned gets its handler attached twice, so later edits refresh twice. Documented;
   worth confirming it is intended.
5. **Per-window / `ThemeVariantScope` `RequestedThemeVariant` does not re-colour the library's
   brushes** — they are single app-level instances following `Application.ActualThemeVariant`.
   Documented in `theming.md` with a workaround.

### Line endings

No CRLF issues observed; every file written in this phase is LF with a final newline, matching the
repository's `.gitattributes`. No action taken or recommended.
