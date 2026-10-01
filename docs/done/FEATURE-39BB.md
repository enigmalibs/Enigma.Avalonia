# FEATURE-39BB — Release 1.2.0 (routine)

**Completed:** 2026-10-01 · single phase
**Branch:** `feature/feature-39bb-release-1-2-0`
**Run:** feature/2026-10-01-infobar-pastel-release

## Summary

`Enigma.Avalonia.Desktop` **1.2.0** is prepared for publishing, shipping FEATURE-2501 — the pastel
InfoBar severity fills in the Dark variant and the new `EnigmaInfoBarMessageForeground` key pair. The
in-repo edits are made, the Release pre-flight and a throwaway pack-verify passed, and the
tag/pack/push runbook is printed for the user, never run.

**Version (SemVer): 1.1.0 → 1.2.0, MINOR.** The release adds public theming surface (a new colour key
and brush key consumers can resolve and override) and changes the default Dark values of eight
existing keys — a look change that renames or removes nothing and leaves every override applying.
New, backward-compatible functionality is a minor bump.

## Files touched

- **Modified** `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj` — `<Version>` 1.1.0 →
  1.2.0; `<PackageReleaseNotes>` mirroring the new top section, ending with the fixed sentence.
- **Modified** `RELEASENOTES.md` — a `v1.2.0` section prepended (*New Features · Dependencies ·
  Version*), with the 1.1.0 → 1.2.0 Dark value table and how to keep the 1.1 look; the 1.1.0 section
  intact below.
- **Modified** `README.md` — the what's-new callout for 1.2. Badges, the TFM line and the quick start
  untouched.
- **Modified** `SECURITY.md` — supported versions `1.1.x` → `1.2.x`.
- **Modified** `docs/RELEASE.md` — §3's stale "no tags yet" sentence now states the established bare
  `X.Y.Z` convention.
- **Modified** `docs/roadmap.md` — status `DONE`; the footer paragraph listing the single-phase items
  re-wrapped (the planning commit had left one over-long line).
- **Modified** `docs/plan/FEATURE-39BB.md` — status `DONE`.
- **Created** `docs/done/FEATURE-39BB.md` — this file.

## Dependency refresh

`dotnet list Enigma.Avalonia.slnx package --outdated` — no bump applied; every available update is
either part of the coupled set or a major version:

| Package | Current | Latest | Decision |
|---|---|---|---|
| `Avalonia`, `Avalonia.Themes.Fluent`, `Avalonia.Desktop`, `Avalonia.Fonts.Inter`, `Avalonia.Headless.XUnit` | 12.1.1 | 12.1.3 | Held back — the coupled set, bumped as a whole on its own decision |
| `Enigma.Core` | 1.0.0 | 2.0.0 | Held back — a major version of a runtime dependency |
| `xunit.v3` (tests only) | 3.2.2 | 4.0.1 | Held back — a test-suite migration of its own |

Every other package is current. Logged in the release notes' *Dependencies* section.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| How the notes explain restoring the 1.1 look | Point at the theming guide's *Retheming with your own palette* recipe with the 1.1.0 values in a table | The recipe already exists and covers merge order; restating it in the notes would be a second copy to drift |
| The over-long roadmap line from the planning commit | Re-wrapped here | The roadmap is touched by this dev anyway; a separate fix commit would be noise |

## Deviations & follow-ups

- None from the plan.
- Follow-up: the out-of-repo `enigma-avalonia-desktop` house skill still describes 1.0/1.1 (29
  colours, the 1.0 Dark severity values).
- Line endings: every touched file is LF with a final newline; nothing to recommend.

## Documentation sweep

- README, CLAUDE.md, `docs/guides/`, `docs/RELEASE.md`, `SECURITY.md` scanned for version references:
  only the planned edits above were needed; the README's "built on Avalonia 12.1.1" line is still true.
- Snippet-verification gate: no code fence was touched in the README quick start or the guides —
  0 snippets to cover.

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx -c Release` — **0 Warning(s), 0 Error(s)**; the
  library DLL was regenerated after the clean, so the XAML compiler re-ran and no `AVLN*` warning is
  hidden.
- `dotnet test --solution Enigma.Avalonia.slnx -c Release` — **327 passed, 0 failed, 0 skipped**.
- Pack-verify (`dotnet pack … -c Release` into a scratch directory, then deleted):
  - the directory held `Enigma.Avalonia.Desktop.1.2.0.nupkg` and nothing else — no `.snupkg`;
  - `README.md` embedded and non-empty (8,841 bytes, carrying the 1.2 callout), `LICENSE.md` embedded;
  - nuspec `<version>1.2.0</version>`, `<title>`, `<license type="file">LICENSE.md</license>`,
    `<readme>README.md</readme>`, `<releaseNotes>` = the new `PackageReleaseNotes`;
  - dependency groups exactly `net8.0` and `net10.0`, each `Avalonia` 12.1.1,
    `Avalonia.Themes.Fluent` 12.1.1, `CommunityToolkit.Mvvm` 8.4.2, `Enigma.Core` 1.0.0 — no
    showcase or test package;
  - `lib/net8.0/` and `lib/net10.0/` each carry the DLL and its XML documentation file.
- Fix cycles used: 0 of 3.
