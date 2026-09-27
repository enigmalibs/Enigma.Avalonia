# FEATURE-0DB5 — Release 1.1.0 (routine)

**Status:** TODO · single phase
**Type:** FEATURE
**Branch:** `feature/feature-0db5-release-1-1-0`
**Run:** feature/2026-09-27-infobar-dialog-release
**Depends on:** FEATURE-726D, FEATURE-5ED6

## 1. Objective

Prepare `Enigma.Avalonia.Desktop` **1.1.0** for publishing — the in-repo edits, a Release pre-flight,
a local pack-verify, and the **printed** tag/pack/push runbook — shipping FEATURE-726D and
FEATURE-5ED6.

## 2. Version choice (SemVer)

`1.0.0` is the published version. This release adds public API — `InfoBar.DisplayDuration`,
`InfoBarServiceExtensions`, the `secondary` `ContentDialog` class, the
`EnigmaDialogSecondaryBackground*` keys, and a `ContentDialog.Background` that now paints the card —
all backward compatible: no member removed or changed, no default look changed, no interface member
added. New backward-compatible functionality is a **MINOR** bump: **1.0.0 → 1.1.0**.

## 3. Execution boundary

Per `docs/RELEASE.md` and the house release standard: the run makes in-repo edits, runs the Release
build/test pre-flight and a throwaway pack-verify (then deletes it), and **prints** everything
outward-facing — merge, `git tag`, `git push`, the publishing `dotnet pack`, `dotnet nuget push`. The
NuGet API key is never stored, committed or echoed.

## 4. Steps (routine release — one phase)

1. `<Version>` → `1.1.0` in `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj`. Target
   frameworks already `net8.0;net10.0` — no normalization.
2. `RELEASENOTES.md` — prepend a `v1.1.0` section (*New Features · Dependencies · Compatibility ·
   Version*, only where non-empty).
3. `<PackageReleaseNotes>` — prose mirroring the new top section, ending
   `See RELEASENOTES.md for the full details.`
4. `README.md` — the what's-new callout for 1.1, and the *Features* dialogs bullet mentions the two
   additions. Badges untouched; the TFM line unchanged.
5. `SECURITY.md` — supported-versions row `1.0.x` → `1.1.x` (latest release only, per its policy).
6. Dependency refresh — `dotnet list package --outdated`; apply non-coupled bumps in
   `Directory.Packages.props`; hold back the version-coupled Avalonia set; log every `old → new`
   (or "none") in the release notes.
7. Pre-flight — `dotnet clean` + `dotnet build Enigma.Avalonia.slnx -c Release` (0 warnings) and
   `dotnet test --solution Enigma.Avalonia.slnx -c Release`.
8. Pack-verify into a scratch directory: version `1.1.0`, `.nupkg` only (no `.snupkg`), non-empty
   `README.md` and `LICENSE.md` embedded, nuspec `<version>`/`<title>`/`<license>`/`<readme>`/
   `<releaseNotes>`, dependency floors per TFM — then delete the directory.
9. Snippet-verification gate on the touched README quick-start/guides (only if touched this phase).
10. Print the runbook (tag format: bare `X.Y.Z`, as `docs/RELEASE.md` prescribes for a repo without
    tags).

## 5. Acceptance criteria

- [ ] `<Version>1.1.0</Version>`; `PackageReleaseNotes` mirrors the new section and ends with the
      fixed sentence.
- [ ] `RELEASENOTES.md` top section is `v1.1.0`, newest-first, with the 1.0.0 section intact below.
- [ ] README callout reads 1.1; `SECURITY.md` supports `1.1.x`.
- [ ] Dependency transitions (or their absence) and the held-back Avalonia set are logged.
- [ ] Release build 0 warnings, Release test suite green.
- [ ] Pack-verify passed and its directory deleted.
- [ ] Runbook printed, never run.

## 6. Out of scope

- Tagging, pushing, packing into `./artifacts`, publishing — the user's steps.
- Bumping the coupled Avalonia set.
- Updating the out-of-repo `enigma-avalonia-desktop` house skill to 1.1.0 (follow-up).

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| Version number | 1.1.0 | Additive, backward-compatible API only → SemVer MINOR | 1.0.1 (patch is for fixes only); 2.0.0 (nothing breaks) |
| Release shape | Routine, single phase | A published version exists; the first-release phases (metadata, licence audit, guides) are done | Re-running the first-release shape |
| Coupled Avalonia set | Held back | House rule: bumped as a whole and only on opt-in; an Avalonia bump is its own compatibility decision | Bumping it inside this release |
| `SECURITY.md` | Row becomes `1.1.x` only | Its policy supports the latest release only | Listing both `1.0.x` and `1.1.x` |
