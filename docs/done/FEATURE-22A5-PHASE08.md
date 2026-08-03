# FEATURE-22A5 PHASE08 — Docking

**Branch:** `feature/feature-22a5-phase08-docking`
**Plan:** `docs/plan/FEATURE-22A5.md` § PHASE08

## Summary

Ported the Docking control family into `Enigma.Avalonia.Desktop` — six control classes and their
four `ControlTheme` dictionaries — and appended the four new includes to `Themes/Fluent.axaml`.
**This is the final phase of FEATURE-22A5; the item's roadmap row closes with it.**

The family splits cleanly into three layers, which is why the plan's "port file by file, building
after each" instruction was worth following literally:

- **The layout model** — `DockPosition` (the five drop zones) and `DockLayoutNode` with its three
  concrete nodes (`DockPaneModel`, `DockTabGroupModel`, `DockSplitModel`). Plain POCOs, no Avalonia
  control involvement; they let a consumer declare a docking layout up front and hand it to
  `DockingHost.LayoutRoot`.
- **The visual leaves** — `DockPane` (header + `[Content]` `PaneContent` + `CanClose`/`CanMove`),
  `DockTabGroup` (a tab strip over an `AvaloniaList<DockPane>`, raising `PaneDragStarted` and
  `PaneCloseRequested`), and `DockSplitContainer` (two children either side of a `GridSplitter`).
- **The orchestrator** — `DockingHost`, which builds the visual tree from the model, wires every
  group's events, and owns the whole drag-to-dock interaction: hit-test → drop zone → overlay →
  drop → split or re-tab → collapse whatever emptied out.

This is a 1:1 port, not a reimplementation. Only the mechanical transformations of plan §3 were
applied; the file bodies are otherwise identical to the port source, verified mechanically (see
*Build/test evidence*). Behaviour carried over deliberately unchanged, including the parts that look
odd:

- **`Background="Transparent"` on `PART_RootPanel`.** Load-bearing exactly as in the other families:
  a null background is invisible to hit testing, and this panel is what captures the pointer for the
  entire drag. Removing it would silently kill drag-to-dock.
- **`IsHitTestVisible="False"` on `PART_DropOverlay`.** The overlay sits *above* the layout while the
  pointer is being tracked through it; without this the overlay would eat the moves that position it.
- **`DockTabGroup` binds the content area to `SelectedPane.PaneContent`, not to `SelectedPane`.** The
  source comments this and it is worth repeating: a `DockPane` is already the `ListBoxItem`'s content
  in the tab strip, so presenting the pane itself in the content area would give it two logical
  parents and crash when the tree is restructured mid-drop.
- **`ExecuteDrop` clears the selection *before* re-parenting.** Same root cause — the source group's
  `ContentPresenter` has to release the pane from the logical tree first.
- **The two no-op guards in `ExecuteDrop`** (drop on own group at centre; drop on own group at an
  edge when it holds a single pane) — without the second, a lone pane would split its own group into
  an empty half.
- **`DockSplitContainer.ConfigureLayout` resets *both* spans in both branches**, not just the axis it
  is switching to. Redundant on first layout, necessary when orientation changes at runtime.
- **`DetermineDropZone`'s 25 % edge band** and `ShowDropOverlay`'s 50 % preview: the overlay shows the
  space the pane *will* occupy, which is deliberately not the same as the band that triggered it.

§3.6 does not apply to this phase — the port source's docking files carry no `global::` qualifier.
No `global::` remains anywhere under `src/`.

`Fluent.axaml` now merges 21 dictionaries (2 foundation + 19 control templates). The four docking
includes are ordered innermost-first (Pane → TabGroup → SplitContainer → Host), the reverse of the
Ribbon block, because that mirrors how the family composes at runtime; as with every earlier family
the `ControlTheme` keys are per-type, so the order is for readability only.

## Files/modules touched

**Created — controls (6)**

- `src/Enigma.Avalonia.Desktop/Controls/Docking/DockPosition.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Docking/DockLayoutNode.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Docking/DockPane.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Docking/DockSplitContainer.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Docking/DockTabGroup.cs`
- `src/Enigma.Avalonia.Desktop/Controls/Docking/DockingHost.cs`

**Created — templates (4)**

- `src/Enigma.Avalonia.Desktop/Themes/Controls/Docking/DockPane.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Docking/DockTabGroup.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Docking/DockSplitContainer.axaml`
- `src/Enigma.Avalonia.Desktop/Themes/Controls/Docking/DockingHost.axaml`

**Modified (3)**

- `src/Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — four `ResourceInclude` lines appended.
- `docs/roadmap.md` — PHASE08 → `DONE`, **and the `FEATURE-22A5` item row → `DONE`** (last phase).
- `docs/plan/FEATURE-22A5.md` — item status → `DONE`, PHASE08 heading → `DONE`, plus the two count
  corrections described under *Deviations & follow-ups*.

**Deleted** — none.

### Theme keys consumed

No new keys. The four templates resolve four existing PHASE01 brushes, all as `DynamicResource`:
`EnigmaBackgroundBrush`, `EnigmaSurfaceBrush`, `EnigmaBorderSubtleBrush`,
`EnigmaForegroundSecondaryBrush`.

`DockingHost.axaml`'s drop overlay is the family's one deviation from the token palette: it uses the
literal `#40007ACC` fill and `#80007ACC` border rather than a theme brush. Carried over unchanged
per §2 — it is a transient drag affordance, not chrome, and inventing an `Enigma*` key for it would
have been a redesign. Worth revisiting in **FEATURE-66EB** (accessibility baseline), where a
hard-coded translucent blue against an arbitrary user background is the kind of thing a contrast
audit flags.

## Deviations & follow-ups

- **No deviations from the plan's scope.** All 10 files landed, the four includes were appended, and
  every applicable §3 transformation was applied.
- **Two plan counts corrected — arithmetic only, no scope change.** Both were predicted in
  `docs/done/FEATURE-22A5-PHASE07.md`, and both were confirmed against the tree here before editing.
  The corrections are recorded inline in `docs/plan/FEATURE-22A5.md` as quoted notes rather than
  silent overwrites:
  - **§5 file count:** was "62 `.cs` (40 controls + 7 data + 15 services)", now **63** (40 + 7 +
    **16**). The port source ships 16 service files and always did; the plan's parenthetical was one
    short. No file was added or dropped to reach the corrected number.
  - **PHASE08 acceptance:** was "merges exactly 22 dictionaries (2 foundation + 20 control
    templates)", now **21** (2 + **19**). 22 is the `.axaml` *file* count — correct, and inclusive of
    `Fluent.axaml` itself — but a file cannot merge itself, so the merged count is one lower.
- **Verified in a scratch harness, not by eye.** PHASE08's acceptance does not ask for a visual
  check, but this is the most intricate family in the library and a template that silently fails to
  apply would still build clean, so the PHASE07 precedent was followed: a temporary
  `ScratchDockingHarness.cs` in the unit-test project, run under the existing `Avalonia.Headless`
  fixture with `FluentTheme` + `Fluent.axaml` merged into the headless `Application`. It was
  **deleted before this commit**, in line with the plan's test note that these phases add no tests
  (FEATURE-6EB0 owns the suite). It passed 8/8 (1 existing + 7 docking). What it asserted, worth
  re-creating in **FEATURE-6EB0 PHASE02**:
  1. `DockingHost` templates (`PART_RootPanel`, `PART_RootHost`, `PART_DropOverlay` all found) and
     falls back to a single `DockTabGroup` holding every declared pane.
  2. `LayoutRoot` builds the visual tree from the model — a `DockSplitModel` of two
     `DockTabGroupModel`s becomes a `DockSplitContainer` of two `DockTabGroup`s, honouring
     orientation and the model's initial `SelectedPane`.
  3. `DockSplitContainer` reconfigures grid definitions and the splitter's `ResizeDirection` on an
     orientation change, and toggles `:horizontal` / `:vertical`.
  4. `DockTabGroup` auto-selects its first pane on template applied and round-trips selection in
     both directions against `PART_TabStrip`.
  5. The tab strip's item template realizes the `dock-pane-close` button, visible and laid out —
     which is what `IsCloseButton`'s class check hit-tests for.
  6. `ClosePane` removes the pane, collapses the emptied group, and promotes the surviving sibling
     to the root — i.e. `CollapseEmptyGroup` → `ReplaceInParent` works.
  7. **The full drag-to-dock interaction, driven through real headless pointer input:** press on the
     second tab, move past the 5 px threshold, drag into the target's left quarter — the drop
     overlay becomes visible — release. The group splits horizontally with the dragged pane's new
     group first, the original keeps the remaining pane, and the overlay hides. This exercises
     `PaneDragStarted` → `HitTestTabGroup` → `DetermineDropZone` → `ShowDropOverlay` →
     `ExecuteDrop` → `SplitGroup` end to end.
- **Item-level §5 re-verified at item close** (below, under *Build/test evidence*) — this is the
  phase that closes `FEATURE-22A5`, so its item-level criteria were checked, not just PHASE08's.
- **Line endings:** all 10 new files are LF, consistent with the rest of the tree. No CRLF churn
  observed; no action taken (recommendation-only per the workflow).

## Build/test evidence

- **Build:** `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental` →
  **Build succeeded, 0 Warning(s), 0 Error(s)** for `net8.0` and `net10.0`, plus the showcase and
  test projects. Per the `Directory.Build.props` caveat that `TreatWarningsAsErrors` does not
  promote Avalonia's XAML diagnostics, the full normal-verbosity log was grepped separately:
  **0 `AVLN*` hits**. Intermediate builds were run after each of the three porting steps (leaves,
  split/tab group, host) as the plan instructs, all clean.
- **Tests:** `dotnet test --solution Enigma.Avalonia.slnx -c Release` → **1/1 passed**
  (`ThemeDictionaryTests.FluentTheme_LoadsFromTheLibraryAssembly`), re-run after the scratch harness
  was deleted. This pre-existing test is a real signal here rather than a formality: it loads
  `Fluent.axaml` and would fail if any of the four new `ResourceInclude` URIs were unresolvable.
  With the harness in place the run was 8/8.
- **PHASE08 acceptance gates:**
  - 10 files present under `Controls/Docking/` and `Themes/Controls/Docking/` — confirmed.
  - `Fluent.axaml` merges **21** dictionaries (2 foundation + 19 control templates) — counted 21
    `<ResourceInclude>` elements; none is a `CalendarSchedule` or `Displayer2D` entry.
  - `grep -ri carbon src/` (excluding `obj/`, `bin/`) → **zero hits**.
  - `grep -rniE "CalendarSchedule|Displayer2D|DrawingObject|Shape" src/` → **zero hits**.
  - `grep -rn "global::" src/ --include=*.cs` → **zero hits**.
- **Item-level §5 acceptance (FEATURE-22A5 closes here):**
  - **63 `.cs`** files — 40 controls (34 + this phase's 6) + 7 data + 16 services. ✔ (corrected count)
  - **22 `.axaml`** files. ✔
  - `dotnet build … -c Release` clean for `net8.0` and `net10.0`, `AVLN*` included. ✔
  - `grep -ri carbon src/` → nothing. ✔
  - `PackageReference`s are exactly `Avalonia`, `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm`,
    `Enigma.Core`. ✔
  - No public type, member, default value or pseudo-class differs from the port source beyond the
    namespace and the `Enigma*` key rename. ✔ (mechanical diff, below)
- **Port fidelity:** each of the 10 files was diffed against its port-source counterpart after
  applying the §3 transformations and normalising whitespace and `using` ordering. **All 10 are
  body-identical** — no behavioural drift was introduced. The only non-mechanical edits are the
  explicit `using System;` / `using System.Collections.Generic;` added to `DockTabGroup.cs` and
  `DockingHost.cs`, required because this solution sets `ImplicitUsings=disable` where the port
  source had it enabled.
