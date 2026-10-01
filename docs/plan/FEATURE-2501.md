# FEATURE-2501 — InfoBar pastel fills in the Dark variant

**Status:** TODO · single phase
**Type:** FEATURE
**Branch:** `feature/feature-2501-infobar-dark-pastel`
**Run:** feature/2026-10-01-infobar-pastel-release

## 1. Objective

Lift the four InfoBar severity fills — Info, Success, Warning, Error — and their borders in the
**Dark** variant to lighter, softer, pastel-leaning tints. The Light variant is liked as it is and
stays untouched.

## 2. Context & constraints

- The eight keys `Enigma{Info,Success,Warning,Error}{Background,Border}Color` live in
  `Themes/Colors.axaml`; `InfoBar.axaml` paints `PART_Card` with the matching brushes per
  `Severity`.
- Measured in OKLCH, today's Dark fills sit at L ≈ 0.25–0.29 — level with `EnigmaSurfaceColor`
  (0.30) and *below* `EnigmaSurfaceHighColor` (0.32). They barely separate from the panels around
  them, which is the "too dark" the spec describes.
- The fills are **shared** keys, documented as the severity notification fills. Outside the InfoBar
  they carry primary text: the showcase's NavigationView warning banner sets
  `EnigmaForegroundBrush` on `EnigmaWarningBackgroundBrush`, and `docs/guides/editors.md` shows
  `EnigmaErrorBackgroundBrush` behind editor text. Whatever the Dark fills become, primary text on
  them must stay readable — WCAG AA, 4.5:1 against `EnigmaForegroundColor` `#BCBEC4`.
- The InfoBar message line paints `EnigmaForegroundSecondaryBrush` (`#6F737A`): about 3:1 on
  today's Dark fills, and about 2:1 on any fill lifted to a pastel tone. The message colour must
  change in Dark — and only in Dark.
- The palette contract: literals only in `Colors.axaml`, both variants define the same keys, one
  brush per colour key in `Brushes.axaml`; `ResourceKeyTests` and `ThemeVariantTests` enforce it for
  new keys automatically.
- `1.1.0` is published; the change must be additive (no key renamed or removed).

## 3. Design

- **Colour rule (Dark only)** — every fill at OKLCH **L 0.40 / C 0.05**, every border at
  **L 0.52 / C 0.075**, on the Light variant's own hue per severity (Info 261°, Success 145°,
  Warning 79°, Error 18°), so both variants read as the same four colours:

  | Key | Dark 1.1.0 | Dark new |
  |---|---|---|
  | `EnigmaInfoBackgroundColor` | `#1C2940` | `#384863` |
  | `EnigmaInfoBorderColor` | `#28406A` | `#506994` |
  | `EnigmaSuccessBackgroundColor` | `#1C3028` | `#364F37` |
  | `EnigmaSuccessBorderColor` | `#28503A` | `#4D744E` |
  | `EnigmaWarningBackgroundColor` | `#302718` | `#564527` |
  | `EnigmaWarningBorderColor` | `#504020` | `#806434` |
  | `EnigmaErrorBackgroundColor` | `#301C20` | `#603D3D` |
  | `EnigmaErrorBorderColor` | `#502830` | `#8F5758` |

  Each new fill keeps 4.85–5.06:1 against `EnigmaForegroundColor` — above AA with margin — and
  rises above `EnigmaSurfaceHighColor`, so the card stands off the surface scale.
- **Message foreground** — a new key pair `EnigmaInfoBarMessageForegroundColor` (Dark `#BCBEC4`,
  Light `#6F737A`) and `EnigmaInfoBarMessageForegroundBrush`; the template's message `TextBlock`
  paints it. Light keeps today's exact message colour; Dark gains ≥ 4.5:1 on every fill.
- **Comment** — `Colors.axaml` records the rule above the Dark severity block, so a re-themer can
  derive a matching set; the key-count comment moves 30 → 31.
- **Template** — `InfoBar.axaml`: the message `TextBlock` resolves the new brush. Nothing else moves.

## 4. Steps

1. `Colors.axaml` — the eight Dark values, the rule comment, the new colour key in both variants
   (last, as declaration order is the guide's table order), the count comment.
2. `Brushes.axaml` — `EnigmaInfoBarMessageForegroundBrush`.
3. `InfoBar.axaml` — the message `TextBlock` foreground.
4. Tests — new `tests/…/Controls/InfoBarPaletteTests.cs`: per-severity card brushes; the message
   brush; Dark fills keep AA against the primary and message foregrounds; Dark fills sit above
   `EnigmaSurfaceHighColor`; the Light message colour equals `EnigmaForegroundSecondaryColor`.
5. Documentation sweep — `docs/guides/theming.md` (table rows, the new row, counts 30 → 31 and
   27 → 28), `docs/guides/dialogs-overlay-infobar.md` where it describes the severity look.

## 5. Acceptance criteria

- [ ] The eight Dark values are the table's; every Light value is byte-for-byte unchanged.
- [ ] Every Dark severity fill keeps ≥ 4.5:1 against `EnigmaForegroundColor` and against
      `EnigmaInfoBarMessageForegroundColor`, and is lighter than `EnigmaSurfaceHighColor`.
- [ ] The InfoBar card paints each severity's background/border brush pair; the message line paints
      `EnigmaInfoBarMessageForegroundBrush`.
- [ ] In Light the message colour equals `EnigmaForegroundSecondaryColor` (look unchanged).
- [ ] Both variants define the new colour; the new brush tracks a theme switch.
- [ ] Full build 0 warnings (including `AVLN*`), whole suite green.
- [ ] The theming guide documents the new values, the new key pair and the counts.

## 6. Out of scope

- Light pastel cards with dark text in the Dark variant (see decisions).
- InfoBar-only fill keys, severity-tinted icons, or any change to the Light variant.
- The release itself — `FEATURE-39BB`.

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| What "pastel" means in Dark | Lighter, desaturated tints that keep light text | The fills are shared keys carrying primary text in consumer content; light pastel cards would silently make that text unreadable | Light pastel cards with dark text (breaks the shared-key contract) |
| Shared keys or InfoBar-only keys | Change the shared keys' Dark values | Re-themers' overrides of the existing keys keep applying; the keys are documented as the notification fills | New `EnigmaInfoBar*` fill keys (a re-themer's existing override would silently stop applying) |
| Light variant | Untouched | The spec likes it as it is | Re-deriving it from the same rule |
| Borders | Lifted with the fills | A border darker than its fill reads as inverted; Dark borders are lighter than fills today too | Fills only |
| How light | OKLCH L 0.40 / C 0.05 fills, L 0.52 / C 0.075 borders, Light hues | The lightest systematic step that still keeps AA (≈ 5:1) for primary text, with margin; one rule for all four | The 4.5:1 ceiling exactly (no margin); per-severity hand-picked values |
| Message readability | New `EnigmaInfoBarMessageForeground` key pair (Dark `#BCBEC4`, Light `#6F737A`) | `#6F737A` would fall to ≈ 2:1 on the new fills; a key keeps Light identical and is re-themable | `EnigmaForegroundBrush` in both variants (changes the liked Light look) |
