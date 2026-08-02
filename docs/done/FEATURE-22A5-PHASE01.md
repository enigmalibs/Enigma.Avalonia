# FEATURE-22A5 PHASE01 — Theme foundation & `Enigma*` resource keys

**Branch:** `feature/feature-22a5-phase01-theme-foundation`
**Status:** DONE

## Summary

Filled `src/Enigma.Avalonia.Desktop/Themes/` with the library's theme foundation — the layer every
control template in PHASE02–08 resolves against.

Three dictionaries, in resolution order:

1. **`Colors.axaml`** — the palette, and the only place a literal colour value appears in this library.
   A `ResourceDictionary.ThemeDictionaries` with `x:Key="Dark"` and `x:Key="Light"`, **29 keys in each,
   identical key sets in identical order**. A key present in one variant but not the other would
   resolve to nothing after a theme switch, so the symmetry is load-bearing, not cosmetic.
2. **`Brushes.axaml`** — 26 `Enigma*` brushes plus 23 FluentTheme override keys. Deliberately *not*
   theme-scoped: one brush instance per key whose `Color` tracks the active variant through
   `DynamicResource`. That indirection is what makes runtime theme switching work — a
   `StaticResource` would bake in whichever variant was active at load time and never update.
3. **`Fluent.axaml`** — the composition root, and the library's single public XAML entry point at
   `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml`. Merges `Colors.axaml` then `Brushes.axaml`;
   later phases append control templates below them.

**Consumers merge this as a `ResourceInclude` in `Application.Resources`, not a `StyleInclude`** — the
root element is a `ResourceDictionary`, so a `StyleInclude` is the wrong mechanism and fails. FEATURE-2802
documents this for consumers; FEATURE-57C8 PHASE01 is the first in-repo consumer.

## Resource-key inventory (55 library keys + 23 framework overrides)

The old → new mapping table lives in `docs/plan/FEATURE-22A5.md` under *PHASE01 recorded output*, not
here — see *Deviations* below for why. This is the equivalent forward-looking inventory: the keys that
now exist and that PHASE02–08 and the showcase may reference.

**Colors — 29, defined in both variants** (`EnigmaBackgroundColor`, `EnigmaSurfaceColor`,
`EnigmaSurfaceHighColor`, `EnigmaSurfaceLowColor`, `EnigmaBorderColor`, `EnigmaBorderSubtleColor`,
`EnigmaForegroundColor`, `EnigmaForegroundSecondaryColor`, `EnigmaForegroundTertiaryColor`,
`EnigmaAccentColor`, `EnigmaAccentHoverColor`, `EnigmaSelectionColor`, `EnigmaInputBackgroundColor`,
`EnigmaInputBackgroundFocusedColor`, `EnigmaInputBackgroundHoverColor`, `EnigmaHoverColor`,
`EnigmaPressedColor`, `EnigmaOverlayColor`, `EnigmaSuccessColor`, `EnigmaWarningColor`,
`EnigmaErrorColor`, `EnigmaInfoBackgroundColor`, `EnigmaInfoBorderColor`,
`EnigmaSuccessBackgroundColor`, `EnigmaSuccessBorderColor`, `EnigmaWarningBackgroundColor`,
`EnigmaWarningBorderColor`, `EnigmaErrorBackgroundColor`, `EnigmaErrorBorderColor`).

**Brushes — 26**: the same tokens with a `Brush` suffix, except the three `EnigmaInputBackground*`
colours, which have no brush of their own and feed the framework overrides directly.

**FluentTheme override keys — 23, names verbatim**: 13 `TextControl*` and 10 `ComboBox*`. These are
FluentTheme's contract, not this library's — renaming one silently stops the override applying, which
is why they keep their original names while pointing at `Enigma*` colours.

Two details worth carrying forward:

- `EnigmaOverlayColor` is alpha-premultiplied by design (`#80000000` dark / `#40000000` light) — it is
  a scrim, and PHASE05's `Overlay` depends on that alpha.
- `EnigmaAccentColor` is `#3574F0` in **both** variants. That is intentional, not a copy-paste slip;
  any future test asserting "every key differs per variant" must exempt it.

## Files touched

**Created**

- `src/Enigma.Avalonia.Desktop/Themes/Colors.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Brushes.axaml`

**Modified**

- `src/Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — was an empty placeholder; now merges the two
  foundation dictionaries.
- `docs/roadmap.md` — item → `IN PROGRESS`, PHASE01 → `DONE`. The `Status` column was re-padded to
  fit `IN PROGRESS`; whitespace only, every other row unchanged.
- `docs/plan/FEATURE-22A5.md` — statuses, plus the *PHASE01 recorded output* key-map section.
- `samples/Enigma.Avalonia.Desktop.Showcase/App.axaml` — documentation freshness sweep. Its comment
  claimed "Nothing to merge yet — the dictionary is still empty", which this phase made false.
  Comment text only; FEATURE-57C8 PHASE01 still owns adding the actual `ResourceInclude`.

No `.cs` files and no csproj changes: the existing `<AvaloniaResource Include="Themes\**" />` glob
picked both new dictionaries up with no edit, which is the arrangement PHASE02–08 relies on.

## Deviations & follow-ups

**1. Twelve `Calendar*` keys dropped (decision taken during this phase, user-approved).** The port
source defines six calendar colours and six matching brushes whose sole consumer is the schedule
control that plan §2 excludes from this port. Porting them would have shipped 12 theme keys that no
shipped control consumes and that FEATURE-2802 would then document. PHASE01's "rename every key"
wording read as port-all, so this was raised before building and approved. Verified by grep against the
port source that **no** template ported in PHASE02–08 references any of them. Recorded in the plan file
alongside the key map. **If the schedule control is ever revived, reverse this first.**

**2. Key map recorded in the plan file, not here.** PHASE01 asks for the old → new map in this
completion doc, but §2.4.10 makes `docs/plan/*.md` the single artifact permitted to name the port
source, and an old → new table necessarily names the old keys. §2 wins over a later plan by its own
terms, so the table went to `docs/plan/FEATURE-22A5.md` — which is also where PHASE02–08 will look for
a porting contract. The forward-looking inventory above is the clean-slate equivalent.

**3. PHASE08's merge count is off by one — worth fixing before PHASE08 is built.** Its acceptance says
`Fluent.axaml` will merge "exactly 22 dictionaries (2 foundation + 20 control templates)". The
reachable figure is **21** (2 foundation + 19 control templates): the port source merges 23, and two
are excluded. The 22 in item-level §5 is correct but counts `.axaml` *files* — Colors + Brushes +
Fluent + 19 templates — and `Fluent.axaml` does not merge itself. Left unedited; PHASE08 should
correct the criterion rather than chase an unreachable number.

**4. Re-indented from 4-space to 2-space.** The port source's theme files use 4-space indentation;
`.editorconfig` line 25 sets `indent_size = 2` for `*.{xml,axaml,xaml}` and the repo's existing
`.axaml` files follow it. Whitespace only — no key, value or structure differs.

**5. No tests added, by design.** Plan §4 assigns the automated suite to FEATURE-6EB0, whose PHASE02
is specifically "headless control smoke & resource-key tests". Adding resource-key tests here would
poach that phase's scope. Runtime verification was done in a throwaway harness instead (below).
**Follow-up for FEATURE-6EB0 PHASE02:** the two highest-value assertions are (a) both variants define
the identical 29-key set, and (b) a brush's `Color` changes when `RequestedThemeVariant` flips — the
harness below is a ready-made basis, and note the `EnigmaAccentColor` exemption.

No line-ending issues observed: all three files are LF with a final newline, consistent with the
repository's `.gitattributes`.

## Build / test evidence

**Build — clean, zero warnings.** `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental`
(full rebuild, `-v:n`): `Build succeeded. 0 Warning(s) 0 Error(s)`, with all four outputs produced
(library `net8.0` + `net10.0`, showcase, tests). Per plan §2.4.1 the exit code alone is not
sufficient — the XAML compiler emits `AVLN*` diagnostics from an MSBuild task that
`TreatWarningsAsErrors` does not promote — so the log was searched directly for `AVLN` and for
`: warning` / `: error` diagnostic lines: **zero hits of each.** Debug build likewise clean.

**Tests — pass.** `dotnet test --solution Enigma.Avalonia.slnx` in both Debug and Release: `Passed!
total: 1, failed: 0`. The existing `ThemeDictionaryTests.FluentTheme_LoadsFromTheLibraryAssembly`
became a materially stronger test at this phase without being modified: it loads `Fluent.axaml`, which
now resolves two `avares://` merged dictionaries, so a bad URI or non-compilable dictionary fails it.

**Static integrity checks** over the three files:

| Check | Result |
|---|---|
| `Dark` vs `Light` key sets | identical, 29 each, same order |
| Duplicate keys in either variant | none |
| `{DynamicResource …}` refs in `Brushes.axaml` resolving to a defined colour | 49/49 |
| Colours defined but consumed by no brush | none — confirms the prune left nothing dangling |
| `StaticResource` in `Brushes.axaml` | none (one mention, in a comment explaining why) |
| Brush keys | 49 = 26 `Enigma*` + 23 Fluent overrides, no duplicates |
| `Fluent.axaml` merges | 2, both foundation |

**Grep gates.** §3.7 clean-slate sweep — a case-insensitive grep for the port source's name over
`src/` (excluding `bin`/`obj`): **0 hits**; over every file in the repository outside `docs/plan/`,
tracked and untracked alike, this document included: **0 hits**. §2 excluded-type gate —
`grep -rnE 'CalendarSchedule|Displayer2D|DrawingObject|Shape' src/`: **0 hits**. (An early draft of
the `Fluent.axaml` comment named the two excluded families to explain why they never appear; it
tripped this gate and was reworded to state the invariant without naming them.)

**Runtime verification — throwaway harness, outside the repo.** The one claim static checks cannot
settle is the architectural one: that a brush in a *non*-theme-scoped dictionary resolves a colour out
of a *theme-scoped* one, and re-resolves on a variant switch. A headless probe
(`Avalonia.Headless` 12.1.1, `SetupWithoutStarting`, project-referencing the library) merged
`Fluent.axaml` into `Application.Resources` and read 10 representative brushes — one per semantic
family plus `TextControlBackground` and `ComboBoxBackground` for the framework hand-off — under each
variant:

- all 10 resolved in both variants; none sat at an unset default;
- all 10 changed colour across the switch except `EnigmaAccentBrush`, which is `#3574F0` in both by
  design and was exempted;
- `EnigmaOverlayBrush` reported `#80000000` → `#40000000`, confirming the premultiplied alpha survives
  the port intact;
- `TextControlBackground` and `ComboBoxBackground` both tracked `EnigmaInputBackgroundColor`
  (`#ff1f2123` → `#ffe8eaed`), confirming the FluentTheme overrides are wired to the palette.

Result: `PASS — 10 keys resolved in both variants and tracked the switch`. The harness was
intentionally not added to the repository — see *Deviations* item 5.

## Acceptance criteria

| Criterion | Status |
|---|---|
| Build clean | ✔ zero warnings, zero `AVLN*`, Debug + Release full rebuild |
| `Colors.axaml` / `Brushes.axaml` / `Fluent.axaml` present under `Themes/` | ✔ |
| Zero port-source-name hits | ✔ 0 under `src/`, 0 anywhere outside `docs/plan/` |
| Both theme variants defined | ✔ 29 identical keys each; both resolve at runtime |
| Key map recorded | ✔ in `docs/plan/FEATURE-22A5.md` — see *Deviations* item 2 |
| `Fluent.axaml` starts with `Colors` + `Brushes` only; no excluded includes | ✔ merges exactly 2 |
| `ResourceDictionary` root, merged as `ResourceInclude` not `StyleInclude` | ✔ noted above and in the file |
