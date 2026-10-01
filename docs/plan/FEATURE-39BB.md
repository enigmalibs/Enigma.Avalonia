# FEATURE-39BB — Release 1.2.0 (routine)

**Status:** TODO · single phase
**Type:** FEATURE
**Branch:** `feature/feature-39bb-release-1-2-0`
**Run:** feature/2026-10-01-infobar-pastel-release
**Depends on:** FEATURE-2501

## 1. Objective

Prepare `Enigma.Avalonia.Desktop` **1.2.0** for publishing — the in-repo edits, a Release pre-flight,
a local pack-verify, and the **printed** tag/pack/push runbook — shipping FEATURE-2501.

## 2. Version choice (SemVer)

`1.1.0` is the published version (tagged `1.1.0`). This release:

- adds public theming surface — the `EnigmaInfoBarMessageForegroundColor` /
  `EnigmaInfoBarMessageForegroundBrush` key pair, which consumers can resolve and override;
- changes the **default Dark values** of eight existing palette keys (the severity fills and
  borders) — a look change, not an API change: no key is renamed or removed, every override a
  consumer already has keeps applying, and primary text on the fills keeps WCAG AA contrast.

Nothing is removed or changed incompatibly, and new backward-compatible functionality is present, so
the bump is **MINOR**: **1.1.0 → 1.2.0**.

## 3. Execution boundary

Per `docs/RELEASE.md` and the house release standard: the run makes in-repo edits, runs the Release
build/test pre-flight and a throwaway pack-verify (then deletes it), and **prints** everything
outward-facing — merge, `git tag`, `git push`, the publishing `dotnet pack`, `dotnet nuget push`. The
NuGet API key is never stored, committed or echoed.

## 4. Steps (routine release — one phase)

1. `<Version>` → `1.2.0` in `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj`. Target
   frameworks already `net8.0;net10.0` — no normalization.
2. `RELEASENOTES.md` — prepend a `v1.2.0` section (*New Features · Dependencies · Version*, only
   where non-empty), calling out the changed Dark defaults and how to restore the 1.1 values by
   override.
3. `<PackageReleaseNotes>` — prose mirroring the new top section, ending
   `See RELEASENOTES.md for the full details.`
4. `README.md` — the what's-new callout for 1.2. Badges untouched; the TFM line unchanged.
5. `SECURITY.md` — supported-versions row `1.1.x` → `1.2.x` (latest release only, per its policy).
6. `docs/RELEASE.md` §3 — the "no tags yet" sentence is stale (`1.0.0` and `1.1.0` exist, bare);
   state the established convention instead.
7. Dependency refresh — `dotnet list package --outdated`; apply non-coupled, non-major bumps in
   `Directory.Packages.props`; hold back the version-coupled Avalonia set; log every `old → new`
   (or "none") in the release notes.
8. Pre-flight — `dotnet clean` + `dotnet build Enigma.Avalonia.slnx -c Release` (0 warnings) and
   `dotnet test --solution Enigma.Avalonia.slnx -c Release`.
9. Pack-verify into a scratch directory: version `1.2.0`, `.nupkg` only (no `.snupkg`), non-empty
   `README.md` and `LICENSE.md` embedded, nuspec `<version>`/`<title>`/`<license>`/`<readme>`/
   `<releaseNotes>`, dependency floors per TFM — then delete the directory.
10. Snippet-verification gate on the touched README quick-start/guides (only if touched this phase).
11. Print the runbook (bare `X.Y.Z` tag, matching `1.0.0` and `1.1.0`).

## 5. Acceptance criteria

- [ ] `<Version>1.2.0</Version>`; `PackageReleaseNotes` mirrors the new section and ends with the
      fixed sentence.
- [ ] `RELEASENOTES.md` top section is `v1.2.0`, newest-first, with the 1.1.0 section intact below.
- [ ] README callout reads 1.2; `SECURITY.md` supports `1.2.x`; `docs/RELEASE.md` §3 states the
      bare-tag convention.
- [ ] Dependency transitions (or their absence) and the held-back sets are logged.
- [ ] Release build 0 warnings, Release test suite green.
- [ ] Pack-verify passed and its directory deleted.
- [ ] Runbook printed, never run.

## 6. Out of scope

- Tagging, pushing, packing into `./artifacts`, publishing — the user's steps.
- Bumping the coupled Avalonia set, or any dependency's major version.
- Updating the out-of-repo `enigma-avalonia-desktop` house skill (follow-up).

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| Version number | 1.2.0 | A new public theme key is backward-compatible functionality → SemVer MINOR; the changed defaults break no API and no override | 1.1.1 (patch is for fixes, and the release adds surface); 2.0.0 (nothing is removed or incompatible) |
| Release shape | Routine, single phase | A published version exists; the first-release phases are done | Re-running the first-release shape |
| Coupled Avalonia set | Held back | House rule: bumped as a whole and only on opt-in | Bumping it inside this release |
| Major bumps of dependencies | Held back | A major dependency move flows to every consumer; not for a minor release | Taking them now |
| `SECURITY.md` | Row becomes `1.2.x` only | Its policy supports the latest release only | Listing `1.1.x` too |
| Stale "no tags yet" in `docs/RELEASE.md` | Corrected in this release | The runbook is followed by hand for this very release | Leaving it as a follow-up |
