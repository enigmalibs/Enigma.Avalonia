# FEATURE-5ED6 — ContentDialog secondary background

**Completed:** 2026-09-27 · single phase
**Branch:** `feature/feature-5ed6-dialog-secondary-bg`
**Run:** feature/2026-09-27-infobar-dialog-release

## Summary

`ContentDialog` gains a second, darker card background as a theme style, for dialogs whose content is
laid out for the window's own background — the case of Enigma.GitClient's Branches / Tags / Remotes
tool dialogs.

- **`Classes="secondary"`** on a `ContentDialog` paints its card with the new
  **`EnigmaDialogSecondaryBackgroundBrush`**, bound to the new **`EnigmaDialogSecondaryBackgroundColor`**:
  Dark `#1E1F22`, Light `#F7F8FA` — the window-background tone, i.e. exactly what GitClient's
  workaround produced in Dark, and darker than the default `EnigmaSurfaceHighColor` `#313335`.
- **The card now paints `ContentDialog.Background`.** The template's hard-coded
  `EnigmaSurfaceHighBrush` became a `ControlTheme` setter for `Background` (same resource, so the
  default look is unchanged) and the card `Border` — now named `PART_Card` — template-binds it. A
  consumer's own `Background` applies, the `secondary` class is a one-setter style, and GitClient's
  1.0 workaround (redefining `EnigmaSurfaceHighBrush` in the host's `Resources`) still resolves.
- `ContentDialogService` is untouched: `Background` and classes are host settings, like the six
  `Dialog*` sizes, and are documented as such.

## Files touched

- **Modified** `src/Enigma.Avalonia.Desktop/Themes/Colors.axaml` — the colour key in both variants; the
  header comment's key count 29 → 30.
- **Modified** `src/Enigma.Avalonia.Desktop/Themes/Brushes.axaml` — the brush key.
- **Modified** `src/Enigma.Avalonia.Desktop/Themes/Controls/ContentDialog.axaml` — the `Background`
  setter, `PART_Card` with `{TemplateBinding Background}`, the `^.secondary` style.
- **Modified** `tests/Enigma.Avalonia.Desktop.UnitTests/Controls/ControlCatalog.cs` — `PART_Card` in the
  ContentDialog part list.
- **Created** `tests/Enigma.Avalonia.Desktop.UnitTests/Controls/ContentDialogBackgroundTests.cs` — 6 cases.
- **Modified** `samples/Enigma.Avalonia.Desktop.Showcase/ViewModels/ServicesTestingPageViewModel.cs`
  and `Views/ServicesTestingPageView.axaml` — a "Secondary Background" dialog (a branch list with
  lighter group strips) on the shared host, adding the class in `configure` and removing it in a
  `finally`.
- **Modified** `docs/guides/theming.md`, `docs/guides/dialogs-overlay-infobar.md` — documentation sweep
  (below).
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-5ED6.md` — status `DONE`.
- **Created** `docs/done/FEATURE-5ED6.md` — this file.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| Where the new colour sits in `Colors.axaml` | Last in each variant | The theming guide's table is "in declaration order, so it diffs directly against the source"; appending keeps both in step without reordering 29 rows |
| How "darker" is asserted | Sum of RGB channels, Dark variant (the test app's pinned variant) | Enough to prove the direction the spec asks for without encoding a luminance formula in a test |
| Showcase on a shared host | Add the class in `configure`, remove it in `finally` | The showcase has one dialog host; this is also the documented recipe for apps that do not give the secondary look its own host |

## Deviations & follow-ups

- None from the plan.
- Follow-up (Enigma.GitClient, outside this repository): once it moves to 1.1.0, its `ToolDialog` host
  can replace the `<ContentDialog.Resources>` brush override with `Classes="secondary"`. The override
  keeps working, so the move is optional and not urgent.
- Follow-up: the out-of-repo `enigma-avalonia-desktop` house skill (`reference/theming.md`,
  `reference/dialogs-overlay-infobar.md`) lists 29 colours and no `secondary` class.
- Line endings: every touched file is LF with a final newline; nothing to recommend.

## Documentation sweep

- `docs/guides/theming.md` — the colour/brush counts (29 → 30, 26 → 27), the new row at the end of the
  declaration-order table, and the Notes' "identical 29-key set" → 30.
- `docs/guides/dialogs-overlay-infobar.md` — a `Background` row in the `ContentDialog` property table
  (default and the `secondary` class), and the service-reset note now lists `Background` and the host's
  `Classes` as not reset, with the two ways to use both looks.
- No code fence was touched, so the snippet-verification gate has nothing to cover (0 snippets).
- README, CLAUDE.md, `docs/guides/README.md` checked: nothing contradicted. The 1.0.0 section of
  `RELEASENOTES.md` still says 29 colours / 26 brushes, which was true of 1.0.0 — historical, left alone.

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx` — **0 Warning(s), 0 Error(s)** (XAML compiler
  re-run by the clean, so no hidden `AVLN*` warnings).
- `dotnet test --solution Enigma.Avalonia.slnx` — **311 passed, 0 failed, 0 skipped** (8 new: the 6
  `ContentDialogBackgroundTests`, and the 2 resource-key theory cases the new brush adds automatically;
  `ThemeVariantTests` and `ResourceKeyTests` also cover the new colour in both variants).
- Fix cycles used: 0 of 3.
