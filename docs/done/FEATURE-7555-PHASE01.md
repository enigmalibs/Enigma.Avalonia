# FEATURE-7555-PHASE01 — Project, linked palette & tests

**Completed:** 2026-10-27 · phase 1 of 3
**Branch:** `feature/feature-7555-phase01-mobile-library`
**Run:** feature/2026-10-27-avalonia-mobile-release

## Summary

`Enigma.Avalonia.Mobile` exists. It is a `net10.0` library with no C# code. Its single entry point,
`avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml`, merges Desktop's own `Colors.axaml` and
`Brushes.axaml`. Those two files are compiled into the mobile assembly as **linked**
`AvaloniaResource` items, so a mobile app resolves exactly the 31 colour keys and the 51 brush keys a
desktop app does: the 28 `Enigma*` brushes plus the 23 FluentTheme `TextBox`/`ComboBox` overrides.
There is one palette source, and an edit to it ships in both packages.

The build log confirms that the XAML compiler honours `Link`:
`XAMLIL: /Themes/Colors.axaml -> avares://Enigma.Avalonia.Mobile/Themes/Colors.axaml`. The
copy-and-guard fallback in the plan was not needed.

A new test project, `Enigma.Avalonia.Mobile.UnitTests`, checks the mobile assembly against the
canonical source files, which are embedded as text:

- every colour value in both variants;
- every brush's painted colour in both variants;
- exact key sets, with nothing missing and nothing extra;
- the shape of the entry point;
- in-place re-colouring of brushes on a variant switch;
- a negative lookup.

## Files touched

- **Created** `src/Enigma.Avalonia.Mobile/Enigma.Avalonia.Mobile.csproj`,
  `src/Enigma.Avalonia.Mobile/Themes/Fluent.axaml`.
- **Created** `tests/Enigma.Avalonia.Mobile.UnitTests/Enigma.Avalonia.Mobile.UnitTests.csproj`,
  `TestApp.axaml`, `TestAppBuilder.cs`, `ThemeDictionaryTests.cs`, `Themes/PaletteSource.cs`,
  `Themes/PaletteTests.cs`.
- **Modified** `Enigma.Avalonia.slnx` — both projects added under `/src/` and `/tests/`.
- **Modified** `src/Enigma.Avalonia.Desktop/Themes/Colors.axaml`, `Brushes.axaml` — a header
  sentence saying the file also ships in the mobile package (comment only).
- **Modified** `CLAUDE.md` — documentation sweep (see below).
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-7555.md`; **created** this file.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| How the test app merges the dictionary | `TestApp.axaml` (`x:Class`) merging it with a `ResourceInclude`, exactly as a mobile app's `App.axaml` does | The mobile assembly has no C# type. A `ResourceInclude` built in C# found no loaded assembly and threw "No precompiled XAML found"; confirmed by force-loading the assembly, after which all 13 tests passed. The compiled XAML include creates a real reference to the assembly, which is the consumer path, so the fixture now proves the documented setup |
| Adding a public C# "anchor" type to the library so C# includes work | No | It would add API surface to work around a code-only setup the docs don't recommend. The constraint is recorded in `CLAUDE.md` gotcha 11 and goes into the guide in PHASE02 |
| `AvaloniaUseCompiledBindingsByDefault` in a library with no bindings | Set, matching Desktop | It is harmless now, and it guards any `{Binding}` added later |
| Coverage package in the new test project | None | Stays consistent with the Desktop test project. The house default would add one; that is a solution-wide decision |
| Test expectations | Read from the canonical files embedded as text, never from the Desktop assembly | Keeps the mobile tests free of Desktop's dependency set (Enigma.Core, BouncyCastle) |

## Deviations & follow-ups

- **Deviation:** the plan described a C# `TestApp` like Desktop's; it is a XAML `TestApp` for the
  reason above.
- **Follow-up:** the out-of-repo `enigma-avalonia-desktop` house skill knows nothing of the mobile
  package.
- Line endings: every created or touched file is LF with a final newline.

## Documentation sweep

- `CLAUDE.md`:
  - the build command comment names both libraries;
  - *Architecture* now reads five projects, with the mobile block in the diagram;
  - new gotcha 11: one palette in two packages, never copy it, merge it from XAML.
- `README.md`, `docs/guides/`, `docs/RELEASE.md`: no statement became false in this phase. The
  mobile package is not documented or packable yet; that is PHASE02 and PHASE03.

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx`: **0 Warning(s), 0 Error(s)**, with every
  `.axaml` project rebuilt from clean.
- `dotnet test --solution Enigma.Avalonia.slnx`: **340 passed, 0 failed, 0 skipped**
  (327 Desktop + 13 Mobile).
- Mutation check: dropping the `Brushes.axaml` merge from the mobile `Fluent.axaml` failed 4 tests
  (both brush-colour theories, the entry-point shape, the in-place re-colour). The file was
  restored and the suite re-run green.
- New tests:
  - `ThemeDictionaryTests`: loads at its URI; merges colours then brushes and nothing else; both
    palette files load at their linked paths (×2).
  - `PaletteTests`: every colour equals the canonical value (×2 variants); every brush paints the
    canonical colour of its key (×2); exact colour key sets (×2); exact brush key set; in-place
    re-colour on a variant switch; negative lookup.
- Fix cycles used: 0 of 3 for the build-or-test loop. The first test run ran zero tests, because the
  fixture could not load the assembly; that was diagnosed and fixed before any suite run counted.
