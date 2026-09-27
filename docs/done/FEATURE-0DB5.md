# FEATURE-0DB5 — Release 1.1.0 (routine)

**Completed:** 2026-09-27 · single phase
**Branch:** `feature/feature-0db5-release-1-1-0`
**Run:** feature/2026-09-27-infobar-dialog-release

## Summary

Prepared `Enigma.Avalonia.Desktop` **1.1.0** for publishing, shipping FEATURE-726D (timed info bars) and
FEATURE-5ED6 (secondary dialog background). The version follows Semantic Versioning: `1.0.0` is the
version on nuget.org (confirmed against the NuGet flat-container index), and this release only
**adds** backward-compatible API (a styled property, an extension overload, a style class, two theme
keys, a template part, and a `Background` that now paints the card, defaulting to the same brush),
so it is a **MINOR** bump, **1.0.0 → 1.1.0**.

In-repo edits only. The run did the Release pre-flight and a throwaway pack-verify; everything
outward-facing (merge, tag, publishing pack, `dotnet nuget push`) is **printed** for the user and
never run.

## Files touched

- **Modified** `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj` — `<Version>` 1.0.0 → 1.1.0;
  `<PackageReleaseNotes>` rewritten for 1.1.0, ending with the fixed sentence.
- **Modified** `RELEASENOTES.md` — new `v1.1.0` section prepended (*New Features · Dependencies ·
  Version*); the 1.0.0 section is intact below it.
- **Modified** `README.md` — the what's-new callout for 1.1, and the *Features* dialogs bullet names
  the `secondary` style and `DisplayDuration`. Badges and the TFM line unchanged.
- **Modified** `SECURITY.md` — supported-versions row `1.0.x` → `1.1.x` (the policy supports the latest
  release only).
- **Modified** `Directory.Packages.props` — `Microsoft.Extensions.Hosting` 10.0.10 → 10.0.12
  (showcase only).
- **Modified** `docs/roadmap.md`, `docs/plan/FEATURE-0DB5.md` — status `DONE`.
- **Created** `docs/done/FEATURE-0DB5.md` — this file.

## Dependency refresh

`dotnet list Enigma.Avalonia.slnx package --outdated`, 2026-09-27:

| Package | Scope | Current | Latest | Action |
|---|---|---|---|---|
| `Avalonia`, `Avalonia.Themes.Fluent`, `Avalonia.Desktop`, `Avalonia.Fonts.Inter`, `Avalonia.Headless.XUnit` | coupled set; the first two ship | 12.1.1 | 12.1.3 | **Held back** — the set moves as a whole, on opt-in |
| `Enigma.Core` | runtime, ships | 1.0.0 | 2.0.0 | **Held back** — a major version of a dependency every consumer receives |
| `xunit.v3` | tests | 3.2.2 | 4.0.1 | **Held back** — 4.x is a test-suite migration (Analyzers 2.0, MTP v2 only), its own work item |
| `Microsoft.Extensions.Hosting` | showcase | 10.0.10 | 10.0.12 | **Bumped** — patch, not coupled, never shipped |

`CommunityToolkit.Mvvm` 8.4.2 and `Enigma.Icons.Avalonia` 1.0.0 are current; `LiveChartsCore` is
pinned to its prerelease on purpose. Every transition and hold-back is logged in the release notes'
*Dependencies* section. The package's dependency floors are unchanged from 1.0.0.

## Snippet-verification gate

The guides changed in this release (FEATURE-726D, FEATURE-5ED6), so every code fence in the two touched
guides was checked against `src/`. The README quick-start was not touched.

| File | Snippets | Symbols | Mismatches | Uncertain |
|---|---|---|---|---|
| `docs/guides/dialogs-overlay-infobar.md` | 4 | 25 | 0 | 0 |
| `docs/guides/theming.md` | 8 | 8 | 0 | 0 |
| **Total** | **12** | **33** | **0** | **0** |

Symbols counted are the library's own: namespaces, control/service/enum types, `RegisterHost`,
`ShowAsync` (including the new `TimeSpan` overload), `ShowMessageAsync`, `HideAsync`,
`IconBrushProperty`, `OverlayBrush`, and every `Enigma*` resource key (each defined in both variants
where it is a colour).

## Pack-verify

`dotnet pack … -c Release -o <scratch>/artifacts-verify`, inspected, then the directory was deleted:

- [x] `Enigma.Avalonia.Desktop.1.1.0.nupkg` — and nothing else (no `.snupkg`).
- [x] `README.md` embedded and non-empty (8 848 bytes, carries the 1.1 callout); `LICENSE.md` embedded.
- [x] nuspec: `<version>1.1.0</version>`, `<title>`, `<license type="file">LICENSE.md</license>`,
      `<readme>README.md</readme>`, `<releaseNotes>` = the new `PackageReleaseNotes`.
- [x] Dependency groups exactly `net8.0` and `net10.0`, each `Avalonia` 12.1.1,
      `Avalonia.Themes.Fluent` 12.1.1, `CommunityToolkit.Mvvm` 8.4.2, `Enigma.Core` 1.0.0 — no
      showcase or test package.
- [x] `lib/net8.0/` and `lib/net10.0/` each carry the `.dll` and the `.xml` docs; the XML documents
      `DisplayDuration` and `InfoBarServiceExtensions`.

## Decisions taken at build time

| Question | Chosen | Why |
|---|---|---|
| Is 1.0.0 really published? | Yes — the NuGet index lists `1.0.0` only, and Enigma.GitClient consumes it | The repo has no tags, so the published state had to be checked rather than assumed |
| Tag format | Bare `1.1.0` | No tag exists locally or on `origin`; `docs/RELEASE.md` fixes the bare house default |
| `Enigma.Core` 2.0.0 | Held back | A major bump of a shipped dependency would make this minor release a compatibility event for consumers |
| `xunit.v3` 4.0.1 | Held back | The house skill calls the 3.x → 4.x move its own work item, not a drive-by |
| README beyond the callout | The *Features* dialogs bullet names the two additions | The packed README is the nuget.org landing page; one clause each, no restructuring |

## Deviations & follow-ups

- **1.0.0 was never tagged.** `origin` has no tags. The runbook tags 1.1.0; tagging 1.0.0 after the fact
  (on the commit it was packed from, `repository commit` in its nuspec) is the user's call.
- **Run branch vs `develop`.** `docs/RELEASE.md` §2 has day-to-day work land on `develop` and a release
  merge `develop` into `main`. This run was cut from `main` (where the session started), so the run
  branch should go into `develop` first, then `develop` → `main`, per the runbook.
- Follow-ups outside this repo: update the `enigma-avalonia-desktop` house skill (it says 1.0.0, 29
  colours, no auto-close, no `secondary` class); optionally switch Enigma.GitClient's `ToolDialog` host
  to `Classes="secondary"` once it references 1.1.0.
- Follow-up work items worth planning: the coupled Avalonia 12.1.3 bump, `Enigma.Core` 2.0.0, and the
  `xunit.v3` 4.x migration.
- Line endings: every touched file is LF with a final newline; nothing to recommend.

## Documentation sweep

The release edits themselves are the documentation of this dev: `RELEASENOTES.md`, `README.md`,
`SECURITY.md`. `docs/RELEASE.md` stays correct ("no tags yet" is still true; its steps are
version-agnostic). CLAUDE.md is unaffected.

## Build/test evidence

- `dotnet clean` + `dotnet build Enigma.Avalonia.slnx -c Release` — **0 Warning(s), 0 Error(s)**.
- `dotnet test --solution Enigma.Avalonia.slnx -c Release` — **311 passed, 0 failed, 0 skipped**.
- Pack-verify as above; verify directory deleted.
- Fix cycles used: 0 of 3.
