# FEATURE-7555-PHASE02 — Mobile guide & guides index

**Completed:** 2026-10-27 · phase 2 of 3
**Branch:** `feature/feature-7555-phase02-mobile-guide`
**Run:** feature/2026-10-27-avalonia-mobile-release

## Summary

`docs/guides/mobile.md` documents `Enigma.Avalonia.Mobile` in the house guide shape (intro → what
the package contains → key resources → usage scenarios → notes). It covers:

- installing it in the shared project of a cross-platform solution;
- merging the entry point in `App.axaml`, and why it must be merged from XAML;
- painting a single-view `MainView` from the palette;
- following the device's dark mode, or pinning or switching the variant at runtime;
- sharing views with a desktop app;
- retheming, and what the package deliberately leaves out.

The key reference is linked to `theming.md`, not copied, so the palette has one table, just as it
has one source.

The guides index gains a *Mobile* group and an intro sentence naming the companion package.
`theming.md` states that its key reference is exactly what the mobile package ships.

## Files touched

- **Created** `docs/guides/mobile.md`.
- **Modified** `docs/guides/README.md` — intro sentence and the *Mobile* group.
- **Modified** `docs/guides/theming.md` — one paragraph in *Colour and brush reference*.
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-7555.md`; **created** this file.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| Show the single-view lifetime wiring in the guide | No — only the root view's XAML | The house convention resolves views from DI, so a `new MainView()` snippet would teach the wrong pattern, and the lifetime is not the package's concern |
| Snippet gate method | Inspection against the source, **plus** compiling every fence in a scratch project (outside the repo) against the real mobile project | The house gate is inspection. Compiling also proves the XAML (`PlaceholderText`, the `AVLN2000` claim) at no cost to the repo |
| Duplicate the key tables | No — link `theming.md`'s anchors | A second copy of 31 + 51 rows would drift |

## Snippet-verification gate

| File | Snippets | Symbols | Mismatches | Uncertain |
|---|---|---|---|---|
| `docs/guides/mobile.md` | 5 (1 bash, 3 XAML, 1 C#) | 35 | 0 | 0 |
| `docs/guides/README.md` | 0 | 0 | 0 | 0 |
| `docs/guides/theming.md` (edited paragraph) | 0 | 0 | 0 | 0 |
| **Total** | **5** | **35** | **0** | **0** |

The scratch compile was a `net10.0` project named `MyApp` with a `ProjectReference` to
`src/Enigma.Avalonia.Mobile` and Avalonia 12.1.4:

- `App.axaml` with `App.axaml.cs`, `Views/MainView.axaml` and `Services/AppearanceService.cs` were
  taken verbatim from the fences;
- the retheming fragment was wrapped in a dictionary beside a two-key `Themes/Palette.axaml`;
- result: **0 Warning(s), 0 Error(s)**, `AVLN*` included.

The guide's `StyleInclude` claim was checked by adding a `StyleInclude` of the mobile URI, which
failed with `AVLN2000: Resource "avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml" is defined as
"Avalonia.Controls.ResourceDictionary" type …`.

These facts were verified against the source: 31 colour keys per variant, 28 `Enigma*` brushes,
13 `TextControl*` and 10 `ComboBox*` override keys, and `WithInterFont()`, which the showcase's
`Program.cs` uses.

## Deviations & follow-ups

- None from the plan.
- Unverifiable here: the guide's statement that `ThemeVariant.Default` follows the Android dark-theme
  setting is Avalonia's documented platform behaviour. It was not exercised on a device; there is no
  Android head in the solution.
- Line endings: every touched file is LF with a final newline.

## Documentation sweep

- This phase is documentation. `README.md` (the packed Desktop README) gets its mobile pointer in
  PHASE03 with the packaging. `CLAUDE.md` was already updated in PHASE01.

## Build/test evidence

- `dotnet build Enigma.Avalonia.slnx`: **0 Warning(s), 0 Error(s)**. No compiled file changed.
- `dotnet test --solution Enigma.Avalonia.slnx`:
  - **340 passed, 0 failed** (`--output detailed`, with 340 individual pass lines);
  - per project: Desktop 327, Mobile 13;
  - one non-detailed solution run summarized `total: 339`, an intermittent miscount in the
    aggregate summary, since the same build's per-project and detailed runs all report 340.
- Fix cycles used: 0 of 3.
