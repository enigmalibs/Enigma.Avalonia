# FEATURE-8763 — Avalonia 12.1.4 (coupled set bump)

**Status:** DONE
**Type:** FEATURE
**Branch:** `feature/feature-8763-avalonia-12-1-4`
**Run:** feature/2026-10-27-avalonia-mobile-release

## 1. Objective

Move the solution's version-coupled Avalonia set from **12.1.1** to **12.1.4**, the latest stable
release on nuget.org (2026-10-27; no newer prerelease exists), so the 1.3.0 release
(`FEATURE-D792`) and the new mobile library (`FEATURE-7555`) are built on it.

## 2. Context & constraints

- `Directory.Packages.props` pins the coupled set — `Avalonia`, `Avalonia.Desktop`,
  `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Headless.XUnit` — and its comment
  states the rule: the set always moves as a unit. `AvaloniaUI.DiagnosticsSupport` belongs to the
  set but is versioned independently; 2.2.3 is already its latest release.
- 12.1.4 is a patch line on 12.1: no API break is expected. The library's `Avalonia` and
  `Avalonia.Themes.Fluent` floors flow to every consumer, so the change is recorded in the 1.3.0
  release notes (`FEATURE-D792`), not here.
- `Enigma.Icons.Avalonia` 1.0.0 (showcase only) has a published floor of Avalonia 12.1.0 —
  satisfied.
- Gotcha 2: `AVLN*` warnings are not promoted by `TreatWarningsAsErrors`; the build must be cleaned
  first and its warning count read.

## 3. Steps

1. `Directory.Packages.props` — the five coupled `PackageVersion` entries `12.1.1` → `12.1.4`;
   rewrite the group comment's version sentence to name 12.1.4.
2. `dotnet clean` + `dotnet build Enigma.Avalonia.slnx` — read the warning count (0 expected).
3. `dotnet test --solution Enigma.Avalonia.slnx` — whole suite green (327 tests at 1.2.0).
4. Documentation sweep — `README.md`'s "built on Avalonia 12.1.1" line becomes factually wrong and is
   updated; history in `RELEASENOTES.md` and `docs/done/` is left as written.

## 4. Acceptance criteria

- [x] All five coupled packages resolve to 12.1.4 (`dotnet list package` shows no 12.1.1).
- [x] No `PackageReference` gains a `Version=`; `AvaloniaUI.DiagnosticsSupport` stays 2.2.3.
- [x] Clean build: 0 warnings, 0 errors, `AVLN*` included.
- [x] Whole test suite green.
- [x] README's Avalonia version line reads 12.1.4.

## 5. Out of scope

- Any other dependency (handled by the release's refresh in `FEATURE-D792`).
- Release notes, version numbers, `PackageReleaseNotes` — `FEATURE-D792`.

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| Which "latest" version | 12.1.4, latest stable | The draft asks for the latest; a library release does not ship on a prerelease (none newer exists anyway) | A prerelease feed |
| Which packages move | The whole coupled set, including the showcase-only and test-only members | House rule in `Directory.Packages.props`: mixing versions within the set breaks the build | Bumping only the two library dependencies |
| `AvaloniaUI.DiagnosticsSupport` | Unchanged at 2.2.3 | Already its latest release; versioned independently | — |
| Separate item vs inside the release | Its own item and commit | A dependency move is reviewable on its own and the mobile library should be born on it | Folding it into `FEATURE-D792` |
