# FEATURE-8763 — Avalonia 12.1.4 (coupled set bump)

**Completed:** 2026-10-27 · single phase
**Branch:** `feature/feature-8763-avalonia-12-1-4`
**Run:** feature/2026-10-27-avalonia-mobile-release

## Summary

The version-coupled Avalonia set moves from **12.1.1** to **12.1.4**, the latest stable release
on nuget.org (no newer prerelease exists). The five members move together: `Avalonia`,
`Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter` and `Avalonia.Headless.XUnit`.
`AvaloniaUI.DiagnosticsSupport` stays at 2.2.3, which is already its latest version. No code needed
to change.

Getting the suite green on this Windows host exposed **two pre-existing test bugs that only show up
on Windows**. Both are unrelated to the bump: rolling `Directory.Packages.props` back to 12.1.1
reproduced the same 26 failures, and only 277 of the 327 tests were discovered. They were fixed
here, because the Definition of Done needs the whole suite green.

## Files touched

- **Modified** `Directory.Packages.props` — the five coupled `PackageVersion`s 12.1.1 → 12.1.4; the
  group comment now names 12.1.4.
- **Modified** `README.md` — "built on Avalonia 12.1.1" → 12.1.4 (documentation sweep).
- **Modified** `tests/Enigma.Avalonia.Desktop.UnitTests/Themes/ThemeSource.cs` — lookups go through
  a `/`-path → manifest-name map.
- **Modified** `tests/Enigma.Avalonia.Desktop.UnitTests/Services/FileDialogServiceTests.cs`,
  `…/Services/FolderDialogServiceTests.cs` — the suggested-start-location assertion normalizes the
  path with `Path.GetFullPath` before trimming.
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-8763.md` — status `DONE`.
- **Created** `docs/done/FEATURE-8763.md` — this file.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| The suite was red before the bump, on this host only | Fix the two test bugs in this dev, with the rollback evidence recorded | The DoD needs a green suite; both fixes are test-only, and neither weakens an assertion |
| `ThemeSource` fix: rewrite the manifest name in MSBuild or map it in C# | Map in C# | An MSBuild property function on `%(RecursiveDir)` inside item metadata is fragile; the map is three lines, and it also makes `Read` agree with `AllFiles`, which already normalized |
| Dialog tests: how to compare paths | `TrimEndingDirectorySeparator(GetFullPath(…))` | The headless storage provider returns `Temp\\` with a doubled separator on Windows. `GetFullPath` collapses it, and the test still asserts the same directory |

## Deviations & follow-ups

- **Deviation:** two test-only fixes beyond the plan (see *Decisions*).
  - Root cause 1: the `EmbeddedResource` glob's `%(RecursiveDir)` carries `\` on Windows. The
    `ThemeSource` type initializer threw for nested template files, so 21 theme tests failed and the
    theory rows were never discovered.
  - Root cause 2: in `FileDialogServiceTests` and `FolderDialogServiceTests`, the start-location
    path came back with a doubled trailing separator.
- Line endings: every touched file is LF with a final newline; nothing to recommend.

## Documentation sweep

- `README.md` — the Avalonia version line was factually wrong after the bump and is updated.
- Left alone: `CLAUDE.md`, `docs/guides/`, `docs/RELEASE.md` and `SECURITY.md` name no Avalonia
  patch version. `RELEASENOTES.md` and `docs/done/` are history.

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx`: **0 Warning(s), 0 Error(s)**. The library
  DLLs were regenerated after the clean, so the XAML compiler re-ran.
- `dotnet list package`: every coupled package resolves to 12.1.4; DiagnosticsSupport resolves to
  2.2.3.
- `dotnet test --solution Enigma.Avalonia.slnx`:
  - first run: 277 discovered, 26 failed; the same result reproduced on 12.1.1;
  - after the fix: **327 passed, 0 failed, 0 skipped**.
- Fix cycles used: 1 of 3.
