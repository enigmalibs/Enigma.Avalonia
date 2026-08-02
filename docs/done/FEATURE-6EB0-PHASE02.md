# FEATURE-6EB0 PHASE02 — Headless control smoke & resource-key tests

**Branch:** `feature/feature-6eb0-phase02-headless-tests`
**Status:** DONE

## Summary

Closed the item: **141 new tests** across five classes, taking the suite from 132 to **273**.
Nothing under `src/` changed — this phase, like PHASE01, only adds tests.

| Area | Class | Tests |
|---|---|---|
| `Themes/` | `ResourceKeyTests` | 57 |
| | `ThemeVariantTests` | 7 |
| `Controls/` | `ControlTemplateTests` | 52 |
| | `EditorRoundTripTests` | 21 |
| | `TemplatePartWiringTests` | 4 |

`TestAppBuilder` now builds a real `TestApp`: FluentTheme in `Application.Styles`, the library's
`Themes/Fluent.axaml` merged into `Application.Resources` as a `ResourceInclude`, and
`RequestedThemeVariant` pinned to `Dark`. Plan §2 made that this phase's job, and it is what makes
everything below mean anything — against a stub dictionary every `DynamicResource` would fall back
silently and the whole class would pass on a broken library. FluentTheme is there for the framework
controls the templates *host* (`ListBox`, `Button`, `Popup`, `GridSplitter`, `PathIcon`); without it,
"the template applied" would be a much weaker claim.

### The resource-key tests read the real XAML

The rename guard cannot work off a key list typed into the test — that list would be maintained by
the same hand that renames the keys. `ThemeSource` reads the theme sources as text and derives
everything: the merge list from `Fluent.axaml`, the referenced keys from each template, the per-variant
colour keys and values from `Colors.axaml`, the brush → colour bindings from `Brushes.axaml`.

The sources are **embedded into the test assembly** by a new `EmbeddedResource` glob over
`src/…/Themes/**`, with an explicit `LogicalName`. They cannot come from the library assembly:
the Avalonia XAML compiler replaces every `AvaloniaResource` `.axaml` with compiled IL, and
`AssetLoader.Open` on one throws `FileNotFoundException` (verified by probe before choosing this
route). Embedding keeps the tests free of any filesystem lookup at run time while still reading the
real, current files — add a template and the tests see it on the next build.

That is what lets `FluentAxaml_MergesEveryControlTemplateTheLibraryShips` and
`EveryShippedControlTheme_HasACatalogueEntry` exist at all: both compare the shipped files against
what is actually wired up, so a template that exists but is never merged, or a control family added
without a test, fails rather than looking like coverage.

### Behaviours pinned, not assumed

- **A brush lookup is variant-independent; only colours are theme-scoped.**
  `TryFindResource("EnigmaSurfaceBrush", ThemeVariant.Light)` returns the *same instance* with the
  *Dark* colour while Dark is active — `Brushes.axaml` is deliberately not theme-scoped. So
  "resolves in both variants" (plan step 1) and "the colour actually changed" (plan step 4) are two
  different tests against two different mechanisms, and the switch test flips
  `Application.RequestedThemeVariant` rather than passing a variant to a lookup.
- **`ContentDialog`, `InfoBar` and `Overlay` are `IsVisible="False"` until `IsOpen`.** Avalonia never
  applies a template to a control it does not measure, so a closed dialog would "pass" a template
  test by never being templated. All three are created open in the catalogue.
- **`DecimalEditor` accepts invariant group separators** (`"1,999.99"` parses to `1999.99m`) because
  it parses with `NumberStyles.Number`, while its sibling editors are stricter. Pinned, since a
  reader would reasonably expect them to agree.
- **`HexService` encodes lower case** (`010203fa`) and decodes either case.
- **On unparseable text every editor keeps its last good value** and only raises `:error` — asserted
  for all nine typed editors and both byte-array editors.
- `ThemeVariantTests` derives its expectation from `Colors.axaml` rather than hard-coding "every
  brush changes": `EnigmaAccentColor` is `#3574F0` in both variants by design, and the test asserts
  each brush changes *iff* the source says its colour differs. `EnigmaOverlayBrush`'s premultiplied
  alpha (`0x80` dark / `0x40` light) is pinned separately — both were flagged as follow-ups by
  FEATURE-22A5 PHASE01.

### Mutation-verified, not just green

Plan §4 acceptance criterion 3 asks for the rename check explicitly. Four mutations were applied to
`src/`, the suite run against each, and the mutation reverted. `git diff src/` is empty and
`git status --porcelain src/` is clean afterwards.

| Mutation | Tests that failed |
|---|---|
| `Brushes.axaml`: `EnigmaSurfaceBrush` → `EnigmaSurfaceBrushRenamed` | 4 — both theory forms for that key, the aggregate `NoControlTemplate_ReferencesAKeyTheDictionaryDoesNotDefine`, and the live-repaint test |
| `SettingsCard.axaml`: a template left pointing at `CarbonSurfaceBrush` | 5 — the above plus `NoControlTemplate_ReferencesACarbonPrefixedKey` |
| `Fluent.axaml`: `SettingsCard.axaml` dropped from the merge list | 7 — the merge-completeness test, both `SettingsCard` template cases, the catalogue test, the all-hosted-together test, and both theme-switch tests |
| `SettingsCard.axaml`: `PART_Chevron` → `PART_Arrow` | 1 — `EveryTemplatedControl_ExposesItsTemplateParts(SettingsCard)` |

`TheKeyLookup_ReportsAKeyThatIsNotDefined` covers the same ground permanently: it asserts the lookup
returns `false` for a key that does not exist, so the guard can never go vacuous.

## Files/modules touched

**Created** — all under `tests/Enigma.Avalonia.Desktop.UnitTests/`:

| File | Purpose |
|---|---|
| `Themes/ThemeSource.cs` | Reads the embedded theme XAML; derives merge list, referenced keys, colour values, brush → colour bindings |
| `Themes/ResourceKeyTests.cs` | The rename guard: resolution in both variants, no `Carbon*` survivors, variant symmetry, merge completeness |
| `Themes/ThemeVariantTests.cs` | Runtime variant switching, the accent exemption, the overlay alpha, live-template repaint |
| `Controls/ControlCatalog.cs` | One entry per templated control — factory plus expected `PART_*` set — and the realise/parts helpers |
| `Controls/ControlTemplateTests.cs` | Applied-template and template-part assertions per control, plus the all-hosted-together case |
| `Controls/EditorRoundTripTests.cs` | Format/parse round trips per typed editor, Base64 and hex through `Enigma.Core`, `:error` reaching the template |
| `Controls/TemplatePartWiringTests.cs` | The code side of the part contract: the dialog's three buttons and the info bar's close button |

**Modified**

- `tests/…/TestAppBuilder.cs` — `AppBuilder.Configure<TestApp>()`; the new `TestApp` merges
  `Themes/Fluent.axaml`, adds FluentTheme, and pins the variant.
- `tests/…/Enigma.Avalonia.Desktop.UnitTests.csproj` — the `EmbeddedResource` glob over the
  library's `Themes/**`. No new package: `xunit.v3` and `Avalonia.Headless.XUnit` still cover
  everything, and `Avalonia.Themes.Fluent` arrives transitively through the project reference.
- `docs/roadmap.md`, `docs/plan/FEATURE-6EB0.md` — PHASE02 and the item flipped to `DONE`.

**Not modified:** anything under `src/`, `samples/`, or the root config.

## Deviations & follow-ups

1. **The plan's control counts are wrong; the enumerated list is right.** §4 step 1 says "the 20
   control templates" and step 2 says "22 templated controls", but the library ships **19**
   `ControlTheme`s — and the 19 names step 2 actually enumerates are exactly those. This is the same
   off-by-count FEATURE-22A5 PHASE01's completion doc recorded (its deviation 3, about `Fluent.axaml`
   merging 21 dictionaries rather than 22). Raised before building; all 19 are covered, and
   `EveryShippedControlTheme_HasACatalogueEntry` now makes the number self-checking rather than
   copied into prose.

2. **Coverage extends past the 19 to the 12 concrete editor subclasses.** None ships a theme of its
   own — each resolves `BaseEditor`'s or `MultiLineTextEditor`'s through `StyleKeyOverride`. That
   indirection is silent when it breaks, so `EveryConcreteEditor_ResolvesTheSharedEditorTemplate`
   asserts it for all twelve.

3. **`TemplatedControl.ApplyTemplate()` returns `void`.** Step 2's "`ApplyTemplate()` returns true"
   describes an API Avalonia 12 does not have. The equivalent assertions are used instead: the
   control has visual children after a layout pass (no theme found ⇒ no template ⇒ none), and the
   expected `PART_*` set is present.

4. **Template parts are found by `TemplatedParent`, not by name scope.** `NameScope.GetNameScope`
   on the template root returns `null` in Avalonia 12 (probed), and a plain visual-descendant search
   by name would also match FluentTheme's own `PART_ContentPresenter` and `PART_TextPresenter` inside
   nested framework controls. Filtering descendants on `ReferenceEquals(child.TemplatedParent,
   control)` gives exactly the control's own template.

5. **`TemplatePartWiringTests` is beyond the plan's letter, and deliberate.** The parts test compares
   the template against an expected list, so it catches a renamed part in the XAML — but not a
   control whose `OnApplyTemplate` looks up a name the template no longer uses, which wires nothing
   and throws nothing. Four tests close that gap for the parts that actually carry handlers.

6. **Zero `Carbon*` hits were already true before this phase.** Both `Carbon` tests are regression
   guards, not discoveries; mutation 2 above is what proves they bite.

7. **The headless session is shared across the assembly**, so every test that changes
   `RequestedThemeVariant` restores it in a `finally`. The suite was run three consecutive times with
   identical results to rule out ordering effects from that shared state.

**Line endings:** every file added by this phase is LF with a final newline; no CRLF churn observed
anywhere in the working tree, so there is nothing to recommend.

## Build/test evidence

- `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental -v:n` — **Build succeeded,
  0 Warning(s), 0 Error(s)**, all four outputs (library `net8.0` + `net10.0`, showcase, tests). Per
  solution invariant §2.4.1 the exit code alone is not trusted: the log was searched directly for
  `AVLN` (**0 hits**), `: warning` (**0**) and `: error` (**0**). Debug full rebuild likewise clean.
- `dotnet test --solution Enigma.Avalonia.slnx` — **273 passed, 0 failed, 0 skipped** under
  Microsoft.Testing.Platform, in Debug and in Release. Run three consecutive times in Debug with
  identical results.
- Mutation checks: see the table above; `src/` restored and verified clean afterwards.
- Package gate: no `Microsoft.NET.Test.Sdk` and no `xunit.runner.visualstudio` anywhere — the only
  textual hits in the repository are the two comments that explain their absence.

### Acceptance criteria

| Criterion | Status |
|---|---|
| `dotnet test --solution Enigma.Avalonia.slnx` passes; suite in the 40–60 range **or above** | Met — 273 (141 added here) |
| Every templated control has at least one applied-template assertion | Met — all 19 themed controls, plus the 12 concrete editors, plus an all-hosted-together case |
| The resource-key test fails loudly if a template references an undefined key (verify by renaming, then reverting) | Met — four mutations run and reverted, see the table; `TheKeyLookup_ReportsAKeyThatIsNotDefined` keeps the guard honest permanently |
| Zero warnings; no `Microsoft.NET.Test.Sdk` or `xunit.runner.visualstudio` anywhere | Met |
