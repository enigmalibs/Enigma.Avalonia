# FEATURE-2501 — InfoBar pastel fills in the Dark variant

**Completed:** 2026-10-01 · single phase
**Branch:** `feature/feature-2501-infobar-dark-pastel`
**Run:** feature/2026-10-01-infobar-pastel-release

## Summary

The four InfoBar severity fills and their borders are lifted to pastel tints in the **Dark** variant;
the Light variant is untouched.

- **The rule.** Every Dark fill is now OKLCH **L 0.40 / C 0.05** and every border **L 0.52 / C 0.075**,
  on the Light variant's own hue per severity (Info 261°, Success 145°, Warning 79°, Error 18°). The
  1.1.0 fills sat at L 0.25–0.29, level with `EnigmaSurfaceColor` and below `EnigmaSurfaceHighColor`,
  so they sank into the panels around them; the new ones stand off the surface scale.

  | Key | Dark 1.1.0 | Dark now | `EnigmaForegroundColor` contrast |
  |---|---|---|---|
  | `EnigmaInfoBackgroundColor` | `#1C2940` | `#384863` | 4.97:1 |
  | `EnigmaInfoBorderColor` | `#28406A` | `#506994` | — |
  | `EnigmaSuccessBackgroundColor` | `#1C3028` | `#364F37` | 4.85:1 |
  | `EnigmaSuccessBorderColor` | `#28503A` | `#4D744E` | — |
  | `EnigmaWarningBackgroundColor` | `#302718` | `#564527` | 4.96:1 |
  | `EnigmaWarningBorderColor` | `#504020` | `#806434` | — |
  | `EnigmaErrorBackgroundColor` | `#301C20` | `#603D3D` | 5.06:1 |
  | `EnigmaErrorBorderColor` | `#502830` | `#8F5758` | — |

- **The message line.** New key pair **`EnigmaInfoBarMessageForegroundColor`** (Dark `#BCBEC4`,
  Light `#6F737A`) / **`EnigmaInfoBarMessageForegroundBrush`**, painted by the template's message
  `TextBlock`. On the old Dark fills the secondary tone `#6F737A` managed about 3:1; on the new ones it
  would be about 2:1. Dark now uses the primary tone (≥ 4.85:1 on every fill); Light keeps exactly the
  secondary tone it had.
- The fills remain the shared severity keys, so consumer content on them — the showcase's NavigationView
  warning banner, the editors guide's error-fill example — keeps AA contrast for
  `EnigmaForegroundColor` text, and every existing override of these keys still applies.

## Files touched

- **Modified** `src/Enigma.Avalonia.Desktop/Themes/Colors.axaml` — the eight Dark values and the rule
  comment above them; `EnigmaInfoBarMessageForegroundColor` last in each variant; the key count 30 → 31.
- **Modified** `src/Enigma.Avalonia.Desktop/Themes/Brushes.axaml` — `EnigmaInfoBarMessageForegroundBrush`.
- **Modified** `src/Enigma.Avalonia.Desktop/Themes/Controls/InfoBar.axaml` — the message `TextBlock`
  paints the new brush.
- **Created** `tests/Enigma.Avalonia.Desktop.UnitTests/Controls/InfoBarPaletteTests.cs` — 14 cases.
- **Modified** `docs/guides/theming.md` — documentation sweep (below).
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-2501.md` — status `DONE`.
- **Created** `docs/done/FEATURE-2501.md` — this file.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| Name the message `TextBlock` as a template part | No — the test finds it by its text | A `PART_Message` name is new template surface the spec does not need; the card was already a named part |
| Where the contrast rule is checked | A test computing WCAG 2 contrast from the declared `Colors.axaml` literals | It pins the property that matters (readable text on any future re-tune of the values), not the hex values themselves |
| "Not too dark" as a test | Each Dark fill's luminance above `EnigmaSurfaceHighColor`'s | The measurable form of the complaint: the 1.1.0 fills sat below the raised surface |
| Success hue | 145°, the Light variant's | The 1.1.0 Dark success fill leaned teal (158–167°); one hue per severity makes both variants read as the same four colours |

## Deviations & follow-ups

- None from the plan.
- Follow-up: the out-of-repo `enigma-avalonia-desktop` house skill (`reference/theming.md`) still lists
  29 colours and the 1.0 Dark severity values.
- Restoring the 1.1.0 look is an ordinary override of the eight keys, merged after `Fluent.axaml`; the
  release notes say so.
- Line endings: every touched file is LF with a final newline; nothing to recommend.

## Documentation sweep

- `docs/guides/theming.md` — the counts (30 → 31 colours, 27 → 28 brushes), the eight Dark values in
  the declaration-order table, the new `EnigmaInfoBarMessageForegroundColor` row at its end, the Notes'
  "identical 30-key set" → 31, and one Notes bullet stating the Dark fill rule and the AA contract for
  overrides.
- `docs/guides/dialogs-overlay-infobar.md` — checked: it says the severity "selects the glyph and the
  background/border pair", still true; it never names the message colour. Unchanged.
- README, CLAUDE.md, `docs/guides/README.md`, `docs/guides/editors.md` checked: nothing contradicted
  (the editors guide's error-fill example still reads well — see Summary).
- No code fence was touched, so the snippet-verification gate has nothing to cover (0 snippets).

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx` — **0 Warning(s), 0 Error(s)** (XAML compiler
  re-run by the clean, so no hidden `AVLN*` warnings).
- `dotnet test --solution Enigma.Avalonia.slnx` — **327 passed, 0 failed, 0 skipped** (16 new: the 14
  `InfoBarPaletteTests`, and the 2 resource-key theory cases the new brush adds automatically;
  `ThemeVariantTests` and `ResourceKeyTests` also cover the new colour in both variants).
- Fix cycles used: 0 of 3.
