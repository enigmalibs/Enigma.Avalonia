# FEATURE-D792 — Release 1.3.0 (Desktop + Mobile)

**Completed:** 2026-10-27 · single phase
**Branch:** `feature/feature-d792-release-1-3-0`
**Run:** feature/2026-10-27-avalonia-mobile-release

## Summary

**1.3.0** is prepared for publishing, under one bare tag `1.3.0`:

- a routine release of `Enigma.Avalonia.Desktop`, which ships `FEATURE-8763` (Avalonia 12.1.4);
- the first release of `Enigma.Avalonia.Mobile` (`FEATURE-7555`).

The in-repo edits are made, the Release pre-flight and a throwaway pack-verify of both packages
passed, and the tag/pack/push runbook is printed for the user and never run.

**Version (SemVer): MINOR.** Desktop goes 1.2.0 → 1.3.0: a compatible dependency move and no API
change, while the package line gains a companion package. Mobile starts at 1.3.0 in lockstep.

## Files touched

- **Modified** `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj` — `<Version>` 1.2.0 →
  1.3.0; `<PackageReleaseNotes>` for 1.3.0.
- **Modified** `src/Enigma.Avalonia.Mobile/Enigma.Avalonia.Mobile.csproj` — `<PackageReleaseNotes>`,
  the twelfth house property (version already 1.3.0); the packaging comment no longer says it is
  pending.
- **Modified** `RELEASENOTES.md` — a `v1.3.0` section prepended for both packages (*New Features ·
  Dependencies · Compatibility · Version*); the 1.2.0 section is intact below.
- **Modified** `README.md` — the what's-new callout now reads 1.3.
- **Modified** `SECURITY.md` — supported versions `1.2.x` → `1.3.x`; *Scope* names the mobile
  package.
- **Modified** `Directory.Packages.props` — `Microsoft.Extensions.Hosting` 10.0.12 → 10.0.14.
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-D792.md` — status `DONE`.
- **Created** `docs/done/FEATURE-D792.md` — this file.

## Dependency refresh

`dotnet list Enigma.Avalonia.slnx package --outdated`:

| Package | Current | Latest | Decision |
|---|---|---|---|
| `Microsoft.Extensions.Hosting` (showcase only) | 10.0.12 | 10.0.14 | **Bumped**: patch, not coupled, not packed |
| `Enigma.Core` (Desktop runtime dependency) | 1.0.0 | 2.0.0 | Held back: a major version that flows to every consumer |
| `xunit.v3` (both test projects) | 3.2.2 | 4.0.2 | Held back: a test-suite migration of its own |

The coupled Avalonia set was moved to 12.1.4 by `FEATURE-8763` and is current. All of it is logged
in the notes' *Dependencies* section.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| Desktop TFMs against the house set (`net10.0` alone) | Kept `net8.0;net10.0`, with the planned drop recorded in *Compatibility* | Planned decision: dropping a TFM is breaking in a minor release, and .NET 8 is supported until 2026-11-10 |
| Release-notes heading for a two-package release | `# Enigma.Avalonia.Desktop and Enigma.Avalonia.Mobile v1.3.0 Release Notes` | Names both packages released under the tag, in the file's existing `<Name> vX.Y.Z Release Notes` form |
| What's-new callout in the mobile README | None | First release. The callout format compares against a previous version, and the README's intro already says what the package is |
| Mention the Windows test-portability fixes from FEATURE-8763 in the notes | No | Repository-internal and invisible to consumers; recorded in `docs/done/FEATURE-8763.md` |
| Pack-verify location | Session scratchpad, deleted afterwards | Same check, nothing written inside the repository |

## Deviations & follow-ups

- None from the plan.
- **Follow-ups:**
  - drop `net8.0` from Desktop at 2.0.0;
  - the out-of-repo `enigma-avalonia-desktop` house skill still describes 1.0–1.2 and knows nothing
    of the mobile package;
  - an Android sample head, if the visual check on a device is wanted;
  - moving `xunit.v3` to 4.x and `Enigma.Core` to 2.x, each as its own item.
- Line endings: every touched file is LF with a final newline.

## Documentation sweep

- README, both packed READMEs, `SECURITY.md`, `RELEASENOTES.md`: edited as planned.
- `docs/RELEASE.md`: already covers two packages (FEATURE-7555-PHASE03). Its §3 bare-tag sentence is
  still true; `1.2.0` was tagged bare as well.
- `CLAUDE.md`, `docs/guides/`: nothing became stale. No version is stated there that changed here.
- Snippet gate: no code fence was touched in this dev, so 0 snippets.

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx -c Release`: **0 Warning(s), 0 Error(s)**.
- `dotnet test --solution Enigma.Avalonia.slnx -c Release`: **340 passed, 0 failed, 0 skipped**
  (Desktop 327, Mobile 13).
- Pack-verify of both packages into one scratch directory, then deleted. The directory held exactly
  `Enigma.Avalonia.Desktop.1.3.0.nupkg` and `Enigma.Avalonia.Mobile.1.3.0.nupkg`, with no `.snupkg`.
  - **Desktop:**
    - nuspec: `<version>1.3.0</version>`, `<title>`, `<license type="file">LICENSE.md</license>`,
      `<readme>README.md</readme>`, and `<releaseNotes>` equal to the new `PackageReleaseNotes`,
      ending with the fixed sentence;
    - `README.md` (9,211 bytes) carries the 1.3 callout; `LICENSE.md` is embedded;
    - dependency groups are exactly `net8.0` and `net10.0`, each with `Avalonia` 12.1.4,
      `Avalonia.Themes.Fluent` 12.1.4, `CommunityToolkit.Mvvm` 8.4.2 and `Enigma.Core` 1.0.0;
    - `lib/net8.0/` and `lib/net10.0/` each hold the DLL and its XML doc.
  - **Mobile:**
    - nuspec: `<version>1.3.0</version>`, `<title>`, license, readme, and its `<releaseNotes>`,
      ending with the fixed sentence;
    - `README.md` (4,242 bytes) is the mobile README; `LICENSE.md` is embedded;
    - one `net10.0` group, with `Avalonia` 12.1.4 and `Avalonia.Themes.Fluent` 12.1.4 only;
    - `lib/net10.0/` holds the DLL and its XML doc.
- Runbook: printed in the end-of-run report and never run.
- Fix cycles used: 0 of 3.
