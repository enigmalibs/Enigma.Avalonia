# FEATURE-22A5 PHASE07 — Ribbon

**Branch:** `feature/feature-22a5-phase07-ribbon`
**Plan:** `docs/plan/FEATURE-22A5.md` § PHASE07

## Summary

Ported the Ribbon control family into `Enigma.Avalonia.Desktop` — seven control classes and their
six `ControlTheme` dictionaries — and appended the six new includes to `Themes/Fluent.axaml`.

The family is a small composition tree: a `Ribbon` owns `RibbonTab`s (tab strip + content area),
each tab owns `RibbonGroup`s (a labelled, right-bordered column of controls), and each group owns
any mix of `RibbonButton`, `RibbonToggleButton` and `RibbonDropDownButton`. `RibbonMenuItem` is the
odd one out: it derives from `AvaloniaObject` rather than a control, and has no theme file of its
own because `RibbonDropDownButton.axaml` renders the popup's items with an inline `DataTemplate`.
That is why this phase is 7 `.cs` but only 6 `.axaml`.

This is a 1:1 port, not a reimplementation. Only the mechanical transformations of plan §3 were
applied; the file bodies are otherwise identical to the port source, verified mechanically (see
*Build/test evidence*). Behaviour carried over deliberately unchanged, including the parts that look
odd:

- **`Ribbon`'s three-way selection sync.** `SelectedTab` ↔ `PART_TabStrip.SelectedItem` ↔
  `SelectedIndex` are kept in step from all three directions, and `OnApplyTemplate` re-seeds the
  strip's selection because the template is reapplied on navigation. The
  `AttachedToVisualTree` → `Dispatcher.UIThread.Post(…, DispatcherPriority.Loaded)` re-sync on top
  of that is load-bearing: it lets bindings evaluate before the selection is forced, which is what
  makes the correct tab appear after navigating back to a page.
- **`Background="Transparent"` on every `PART_Root`.** A null background is invisible to hit
  testing, so removing it would silently kill `:pointerover` and the press handlers.
- **`RibbonDropDownButton._popup`** is assigned in `OnApplyTemplate` and never read. It is dead in
  the source too; kept because §2 says to carry oddities over, and it compiles warning-free.
- **`e.Handled`**, which differs per button by design: `RibbonButton` sets it only when the command
  actually executes, while `RibbonToggleButton` and `RibbonDropDownButton` always set it.

The §3.6 `global::` unwind applies to `Ribbon.cs`, the phase's one such file: its two
`global::Avalonia.Threading.*` references became a file-scoped `using Avalonia.Threading;`. No
`global::` qualifier remains anywhere under `src/`.

`Fluent.axaml` now merges 17 dictionaries (2 foundation + 15 control templates). The six Ribbon
includes are ordered outermost-first (Ribbon → Tab → Group → the three buttons); as with the earlier
families the `ControlTheme` keys are per-type, so the order is for readability only.

## Files/modules touched

**Created — controls (7)**

- `src/Enigma.Avalonia.Desktop/Controls/Ribbon/Ribbon.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Ribbon/RibbonTab.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Ribbon/RibbonGroup.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Ribbon/RibbonButton.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Ribbon/RibbonToggleButton.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Ribbon/RibbonDropDownButton.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Ribbon/RibbonMenuItem.cs`

**Created — templates (6)**

- `src/Enigma.Avalonia.Desktop/Themes/Controls/Ribbon/Ribbon.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Ribbon/RibbonTab.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Ribbon/RibbonGroup.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Ribbon/RibbonButton.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Ribbon/RibbonToggleButton.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Ribbon/RibbonDropDownButton.axaml`

**Modified (3)**

- `src/Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — six `ResourceInclude` lines appended.
- `docs/roadmap.md` — PHASE07 → `DONE`.
- `docs/plan/FEATURE-22A5.md` — PHASE07 heading → `DONE`.

**Deleted** — none.

### Theme keys consumed

No new keys. The six templates resolve nine existing PHASE01 brushes, all as `DynamicResource`:
`EnigmaBackgroundBrush`, `EnigmaSurfaceBrush`, `EnigmaBorderBrush`, `EnigmaBorderSubtleBrush`,
`EnigmaForegroundBrush`, `EnigmaForegroundSecondaryBrush`, `EnigmaForegroundTertiaryBrush`,
`EnigmaHoverBrush`, `EnigmaPressedBrush`, `EnigmaSelectionBrush`, `EnigmaAccentBrush`,
`EnigmaAccentHoverBrush`.

## Deviations & follow-ups

- **No deviations from the plan.** All 13 files landed, the six includes were appended, and every
  §3 transformation was applied.
- **Templating verified in a scratch harness, not by eye.** The plan's PHASE07 acceptance offers a
  choice — "verified by eye once FEATURE-57C8 PHASE03 exists, or in a scratch harness — say which".
  FEATURE-57C8 does not exist yet, so a scratch harness was used: a temporary
  `ScratchRibbonHarness.cs` in the unit-test project, run under the existing `Avalonia.Headless`
  fixture with `FluentTheme` + `Fluent.axaml` merged into the headless `Application`. It was
  **deleted before this commit**, in line with the plan's test note that PHASE07 adds no tests
  (FEATURE-6EB0 owns the suite). What it asserted, worth re-creating in **FEATURE-6EB0 PHASE02**:
  1. `Ribbon` templates, finds `PART_TabStrip`, auto-selects `Tabs[0]`, and round-trips selection
     in all three directions (strip → `SelectedTab`, `SelectedIndex` → `SelectedTab`, and back).
  2. `RibbonTab` realizes its groups and `RibbonGroup` realizes its items, with a visible header
     `TextBlock` and non-zero bounds — i.e. the nested `ItemsControl`s actually lay out.
  3. `RibbonToggleButton` adds/removes the `:checked` pseudo-class as `IsChecked` changes.
  4. `RibbonDropDownButton` templates `PART_Popup`, opens/closes it via `IsDropDownOpen`, and
     realizes one entry per `RibbonMenuItem` (which also proves the popup's compiled `DataTemplate`
     binding against `x:DataType="controls:RibbonMenuItem"` resolves).
- **Item-level §5 counts will need a small correction at PHASE08.** Two arithmetic slips in the
  plan's item-level acceptance, neither affecting this phase — flagged now so PHASE08 does not chase
  a phantom mismatch:
  - §5 says **62 `.cs`** ("40 controls + 7 data + 15 services"). The tree currently holds 34
    controls + 7 data + **16** services = 57; PHASE08's 6 docking controls bring it to **63**. The
    control count (40) and the data count (7) are right — the services count is 15 in the plan and
    16 on disk, so the total should read 63.
  - PHASE08's acceptance says `Fluent.axaml` "merges exactly 22 dictionaries (2 foundation + 20
    control templates)". 22 is the `.axaml` **file** count, which is correct and includes
    `Fluent.axaml` itself; the number of dictionaries it *merges* will be **21** (2 foundation + 19
    control templates). At 17 today, PHASE08's four docking includes land exactly on 21.
- **Line endings:** all 13 new files are LF, consistent with the rest of the tree. No CRLF churn
  observed; no action taken (recommendation-only per the workflow).

## Build/test evidence

- **Build:** `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental` →
  **Build succeeded, 0 Warning(s), 0 Error(s)** for `net8.0` and `net10.0`, plus the showcase and
  test projects. Per the `Directory.Build.props` caveat that `TreatWarningsAsErrors` does not
  promote Avalonia's XAML diagnostics, the full normal-verbosity log was grepped separately:
  **0 `AVLN*` hits**.
- **Tests:** `dotnet test --solution Enigma.Avalonia.slnx -c Release` → **1/1 passed**
  (`ThemeDictionaryTests.FluentTheme_LoadsFromTheLibraryAssembly`). This is the pre-existing suite;
  it is a real signal here rather than a formality, because it loads `Fluent.axaml` and would fail
  if any of the six new `ResourceInclude` URIs were unresolvable. The scratch harness above passed
  5/5 (1 existing + 4 Ribbon) before being removed.
- **Acceptance gates:**
  - 13 files present under `Controls/Ribbon/` and `Themes/Controls/Ribbon/` — confirmed.
  - `grep -ri carbon src/` (excluding `obj/`, `bin/`) → **zero hits**.
  - `grep -rniE "CalendarSchedule|Displayer2D|DrawingObject|Shape" src/` → **zero hits**.
  - `grep -rn "global::" src/ --include=*.cs` → **zero hits**.
  - Tab selection, group layout and the drop-down popup all template correctly — scratch harness,
    detailed above.
- **Port fidelity:** each of the 13 files was diffed against its port-source counterpart after
  applying the §3 transformations (namespace/key rename, the `global::Avalonia.Threading` unwind)
  and normalising whitespace and `using` ordering. **All 13 are body-identical** — no behavioural
  drift was introduced.
