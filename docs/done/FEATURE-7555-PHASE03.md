# FEATURE-7555-PHASE03 — Packaging, packed README & runbook

**Completed:** 2026-10-27 · phase 3 of 3 (FEATURE-7555 is now `DONE`)
**Branch:** `feature/feature-7555-phase03-mobile-packaging`
**Run:** feature/2026-10-27-avalonia-mobile-release

## Summary

`Enigma.Avalonia.Mobile` is packable as its own NuGet package at **1.3.0**, in lockstep with Desktop.
The mobile csproj now carries the house packaging set, all of it except `PackageReleaseNotes`, which
arrives with the release notes in `FEATURE-D792`. It also gets a packing `ItemGroup` for its own
`README.md` and the root `LICENSE.md`.

The package has its own landing-page README at `src/Enigma.Avalonia.Mobile/README.md`. The root
README, which is Desktop's packed README, points mobile developers to the companion package.
`docs/RELEASE.md` now describes one release of two packages: both versions, two packs, two
inspections, two pushes, two post-publish checks, and one bare tag.

## Files touched

- **Modified** `src/Enigma.Avalonia.Mobile/Enigma.Avalonia.Mobile.csproj` — packaging
  `PropertyGroup` (`PackageId`, `Version` 1.3.0, `Title`, `Description`, `PackageTags`,
  `PackageReadmeFile`, `PackageLicenseFile`, `RepositoryUrl`, `RepositoryType`,
  `PackageProjectUrl`) and the packing `ItemGroup`.
- **Created** `src/Enigma.Avalonia.Mobile/README.md` — the packed README.
- **Modified** `README.md` — a *Building a mobile app?* paragraph under *Installation*; the
  *Documentation* paragraph names the mobile guide.
- **Modified** `docs/RELEASE.md` — the runbook covers both packages.
- **Modified** `CLAUDE.md` — documentation sweep (see below).
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-7555.md`; **created** this file.

## Third-party license audit (first release of the package)

| Dependency | Kind | Ships? | License |
|---|---|---|---|
| `Avalonia` 12.1.4 | runtime, `net10.0` | yes, as a NuGet dependency | MIT |
| `Avalonia.Themes.Fluent` 12.1.4 | runtime, `net10.0` | yes, as a NuGet dependency | MIT |
| `xunit.v3`, `Avalonia.Headless.XUnit` | test-only | no | — |

Both runtime dependencies were already audited for Desktop 1.0.0 and permit redistribution. The
package embeds no third-party code: its DLL holds only compiled XAML. `LICENSE.md` (MIT) is packed.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| Where the mobile README lives | `src/Enigma.Avalonia.Mobile/README.md`, packed as `README.md` | The root README is Desktop's landing page. A project-local README is what GitHub shows when browsing the folder, and it packs without renaming |
| `None Include` vs `Update` for that README | `Update` | The file sits in the project folder, so the SDK's default glob already made it a `None` item; `Include` would duplicate it |
| `LICENSE.md` link in the mobile README | Relative `LICENSE.md` | House link rule: link only packed files. It resolves on nuget.org, where the README is rendered beside the packed license; it is dead when browsing the folder on GitHub, which is the accepted cost |
| What's-new callout in the mobile README | None | A first release has nothing new relative to a previous version. `FEATURE-D792` revisits the callouts |
| Pack-verify location | The session scratchpad, not `./artifacts/verify` | Same check, with no writes inside the repository; deleted afterwards |

## Snippet-verification gate

| File | Snippets | Symbols | Mismatches | Uncertain |
|---|---|---|---|---|
| `src/Enigma.Avalonia.Mobile/README.md` | 3 (1 bash, 2 XAML) | 21 | 0 | 0 |
| `README.md` (edited paragraphs) | 0 fences (1 inline command) | 1 | 0 | 0 |
| **Total** | **3** | **22** | **0** | **0** |

Both XAML fences of the mobile README were compiled verbatim in the scratch `MyApp` project from
PHASE02, against the real mobile project: **0 Warning(s), 0 Error(s)**.

## Deviations & follow-ups

- None from the plan.
- Line endings: every touched or created file is LF with a final newline.

## Documentation sweep

- `CLAUDE.md`:
  - *Commands* packs both libraries;
  - *Solution rules*: packaging metadata lives in **each** library csproj, Mobile packs its own
    README, and the two are versioned in lockstep.
- `docs/RELEASE.md` was in the plan (step 4), not a sweep find.
- `SECURITY.md` still names only Desktop and `1.2.x`. That is the release's edit (`FEATURE-D792`
  step 5), because its supported-versions row must name the released version.

## Build/test evidence

- Pack-verify (`dotnet pack src/Enigma.Avalonia.Mobile/… -c Release -o <scratch>`, then deleted):
  - the directory held `Enigma.Avalonia.Mobile.1.3.0.nupkg` and nothing else; no `.snupkg`;
  - `README.md` is embedded and non-empty (4,242 bytes, starting with `# Enigma.Avalonia.Mobile`);
  - `LICENSE.md` is embedded (1,063 bytes);
  - nuspec: `<version>1.3.0</version>`, `<title>`, `<license type="file">LICENSE.md</license>`,
    `<readme>README.md</readme>`, description, tags and repository are all present.
    `<releaseNotes>` is absent until `FEATURE-D792`;
  - one dependency group, `net10.0`: `Avalonia` 12.1.4 and `Avalonia.Themes.Fluent` 12.1.4. No
    Desktop, showcase or test package;
  - `lib/net10.0/` holds `Enigma.Avalonia.Mobile.dll` (32,256 bytes) and `Enigma.Avalonia.Mobile.xml`.
- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx -c Release`: **0 Warning(s), 0 Error(s)**.
- `dotnet test --solution Enigma.Avalonia.slnx -c Release`: **340 passed, 0 failed**.
- Fix cycles used: 0 of 3.
