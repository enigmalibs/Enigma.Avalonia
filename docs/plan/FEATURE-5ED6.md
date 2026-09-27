# FEATURE-5ED6 — ContentDialog secondary background

**Status:** TODO · single phase
**Type:** FEATURE
**Branch:** `feature/feature-5ed6-dialog-secondary-bg`
**Run:** feature/2026-09-27-infobar-dialog-release

## 1. Objective

Give `ContentDialog` a second, darker card background as a theme style, for dialogs whose content is
laid out for the window's own background rather than for the raised dialog surface.

## 2. Context & constraints

- The card in `Themes/Controls/ContentDialog.axaml` paints a hard-coded
  `{DynamicResource EnigmaSurfaceHighBrush}` and ignores the control's `Background`.
- The motivating consumer, Enigma.GitClient (`src/Enigma.GitClient.App/Views/MainWindow.axaml`),
  hosts its Branches / Tags / Remotes tool dialogs on a dedicated `ToolDialog` host and works around
  the hard-coded brush by redefining `EnigmaSurfaceHighBrush` in that host's `Resources` as
  `EnigmaBackgroundColor`. That override must keep working.
- The palette contract (`Colors.axaml`): every literal colour lives there, both variants define the
  same keys, every brush in `Brushes.axaml` resolves one colour key, every template resolves a brush.
  `ResourceKeyTests` and `ThemeVariantTests` enforce it automatically for new keys.
- `ControlCatalog` lists each template's `PART_*` names exactly; naming the card adds one.
- `1.0.0` is published; the change must be additive.

## 3. Design

- **Palette** — new colour key `EnigmaDialogSecondaryBackgroundColor` in both variants:
  Dark `#1E1F22`, Light `#F7F8FA` — the window-background tone (identical to
  `EnigmaBackgroundColor` today, but its own key so it can be re-themed independently). In Dark it is
  the darkest tone of the scale, below `EnigmaSurfaceHighColor` `#313335`; in Light the elevation
  scale inverts, so the window tone is the lightest.
- **Brush** — `EnigmaDialogSecondaryBackgroundBrush` bound to that colour with `DynamicResource`.
- **Template** — the card `Border` is named `PART_Card` and paints `{TemplateBinding Background}`;
  the `ControlTheme` sets `Background` to `{DynamicResource EnigmaSurfaceHighBrush}` by default, so
  the default look is unchanged and GitClient's resource override still resolves.
- **Style** — `^.secondary` sets `Background` to `{DynamicResource EnigmaDialogSecondaryBackgroundBrush}`:
  `<ContentDialog Classes="secondary" />`.
- **Service** — `ContentDialogService` does not reset `Background` or classes: the look is a host
  setting, like the six `Dialog*` size properties. Documented.
- **Counts** — `Colors.axaml` comment and `docs/guides/theming.md` move from 29 colours / 26 brushes
  to 30 / 27.
- **Showcase** — Services page, Content Dialogs section: a "Secondary Background" button showing a
  dialog with the `secondary` class, removed again in a `finally` because the showcase shares one
  host.

## 4. Steps

1. `Colors.axaml`, `Brushes.axaml` — the key pair and the updated count comment.
2. `ContentDialog.axaml` — the `Background` setter, `PART_Card`, the template binding, the
   `.secondary` style.
3. Tests — `ControlCatalog` part list; new `tests/…/Controls/ContentDialogBackgroundTests.cs`.
4. Showcase — the command and the button.
5. Documentation sweep — `docs/guides/dialogs-overlay-infobar.md`, `docs/guides/theming.md`.

## 5. Acceptance criteria

- [ ] By default the dialog card paints `EnigmaSurfaceHighBrush` (unchanged look).
- [ ] `Classes="secondary"` paints the card with `EnigmaDialogSecondaryBackgroundBrush`, whose Dark
      colour is darker than `EnigmaSurfaceHighColor`.
- [ ] An explicit `Background` on the dialog paints the card.
- [ ] Redefining `EnigmaSurfaceHighBrush` in the dialog's own `Resources` (the GitClient workaround)
      still re-paints the card.
- [ ] Both variants define the new colour; the new brush tracks a theme switch (existing theme tests
      stay green with the new keys).
- [ ] `PART_Card` is in the ContentDialog catalogue entry.
- [ ] The showcase demonstrates the secondary background.
- [ ] Full build 0 warnings (including `AVLN*`), whole suite green.
- [ ] The theming and dialogs guides document the class, the key pair and the `Background` hook.

## 6. Out of scope

- Changing Enigma.GitClient to use the new class (a follow-up in that repository).
- Secondary variants for `InfoBar`, `Overlay` or other controls.
- A per-call reset of the dialog's look in `ContentDialogService`.

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| API shape | A `secondary` style class + card bound to `Background` | "Add in the styles" asks for a style; binding `Background` makes the existing property meaningful and keeps the class a one-setter style | Class only (Background stays dead); a new enum property |
| Class name | `secondary` | The spec's own word for it | `dark` (collides with the theme-variant vocabulary), `dim`, `recessed` |
| Colour value | Window-background tone in both variants (Dark `#1E1F22`, Light `#F7F8FA`) | Matches what GitClient validated in Dark, and the purpose — content laid out for the window background — holds in Light too | A literal "darker" Light value (would clash with page-laid content) |
| New colour key or reuse `EnigmaBackgroundColor` | New `EnigmaDialogSecondaryBackgroundColor` | The palette's contract is that consumers re-theme through colour keys; its own key can diverge from the window background | Brush bound straight to `EnigmaBackgroundColor` |
| Does the service reset it? | No | It is a host setting, like the `Dialog*` sizes and `OverlayBrush`; resetting classes would also wipe a host's XAML `Classes` | Reset in `ResetDialog` |
