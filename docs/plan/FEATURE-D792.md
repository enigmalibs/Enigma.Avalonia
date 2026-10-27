# FEATURE-D792 — Release 1.3.0 (Desktop + Mobile)

**Status:** DONE
**Type:** FEATURE
**Branch:** `feature/feature-d792-release-1-3-0`
**Run:** feature/2026-10-27-avalonia-mobile-release
**Depends on:** FEATURE-8763, FEATURE-7555

## 1. Objective

Prepare **1.3.0** for publishing: a routine release of `Enigma.Avalonia.Desktop` (Avalonia
12.1.4) and the first release of `Enigma.Avalonia.Mobile`, in lockstep under one bare tag — the
in-repo edits, the Release pre-flight, a throwaway pack-verify of both packages, and the **printed**
tag/pack/push runbook.

## 2. Version choice (SemVer)

`1.2.0` is published (tagged `1.2.0`). The draft fixes the number at 1.3.0, and SemVer agrees:

- Desktop: the Avalonia floor moves 12.1.1 → 12.1.4 (a compatible dependency move); no API change.
- The release adds a new package to the product line — new, backward-compatible functionality.

Nothing is removed or changed incompatibly → **MINOR**: Desktop 1.2.0 → 1.3.0; Mobile starts at
1.3.0 (lockstep, `FEATURE-7555`).

## 3. Execution boundary

Per `docs/RELEASE.md`: in-repo edits, the Release build/test pre-flight and a throwaway pack-verify
(then deleted) are run; everything outward-facing — merge, `git tag`, `git push`, the publishing
`dotnet pack`, `dotnet nuget push` — is **printed**. The NuGet API key is never stored, committed
or echoed.

## 4. Steps

1. Desktop `<Version>` → `1.3.0`; confirm Mobile's is `1.3.0`. TFMs: Desktop stays
   `net8.0;net10.0`, Mobile `net10.0` (see decisions).
2. `RELEASENOTES.md` — prepend a `v1.3.0` section covering both packages: summary · *New Features*
   (the mobile package — what it contains, the shared palette, its URI) · *Dependencies* · 
   *Compatibility* (TFM sets, the Avalonia floor, .NET 8's end of support) · *Version*.
3. `<PackageReleaseNotes>` in **both** csproj — prose mirroring the new section from that package's
   point of view, each ending `See RELEASENOTES.md for the full details.`
4. Root `README.md` — the what's-new callout for 1.3. Mobile README — confirm it reads as a first
   release (no stale callout). Badges untouched.
5. `SECURITY.md` — supported versions `1.2.x` → `1.3.x`; the scope names both packages.
6. Dependency refresh — `dotnet list package --outdated`; apply the non-coupled, non-major bumps
   (expected: `Microsoft.Extensions.Hosting` 10.0.12 → 10.0.14, showcase only); hold back majors
   (`Enigma.Core` 2.0.0, `xunit.v3` 4.x); log every `old → new` in the notes.
7. Pre-flight — `dotnet clean` + `dotnet build Enigma.Avalonia.slnx -c Release` (0 warnings) and
   `dotnet test --solution Enigma.Avalonia.slnx -c Release`.
8. Pack-verify both packages into a scratch directory: versions 1.3.0, `.nupkg` only, non-empty
   README and LICENSE embedded, nuspec fields, dependency groups (Desktop `net8.0` + `net10.0` with
   Avalonia/Fluent 12.1.4, CommunityToolkit.Mvvm 8.4.2, Enigma.Core 1.0.0; Mobile `net10.0` with
   Avalonia/Fluent 12.1.4) — then delete it.
9. Snippet gate only on fences touched by this dev.
10. Print the runbook (bare tag `1.3.0`, two packs, two pushes).

## 5. Acceptance criteria

- [x] Both csproj `<Version>1.3.0</Version>`; both `PackageReleaseNotes` end with the fixed sentence.
- [x] `RELEASENOTES.md` top section is v1.3.0; the 1.2.0 section intact below.
- [x] README callout reads 1.3; `SECURITY.md` supports `1.3.x`.
- [x] Dependency transitions and held-back packages logged.
- [x] Release build 0 warnings; Release test suite green.
- [x] Pack-verify of both packages passed; scratch directory deleted.
- [x] Runbook printed, never run.

## 6. Out of scope

- Tagging, pushing, packing into `./artifacts`, publishing — the user's steps.
- Dropping `net8.0` from Desktop (a breaking change — next major).
- Major dependency bumps; the out-of-repo `enigma-avalonia-desktop` house skill (follow-up).

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| Version | 1.3.0 for both | Fixed by the draft; SemVer MINOR (new package, compatible dependency move) | — |
| Desktop TFMs (house set is now `net10.0` alone) | Keep `net8.0;net10.0`, log the plan to drop net8.0 at 2.0.0 | A dropped TFM is breaking and 1.3.0 is a minor; .NET 8 is still supported on the release date (EOL 2026-11-10) | Dropping net8.0 now in a minor release |
| Release notes layout | One `v1.3.0` section in the root `RELEASENOTES.md` covering both packages | One tag, one release, one chronology; the house forbids a second notes file | A per-package notes file |
| Release shape | Single phase; Mobile's first-release metadata, guide and README were done in `FEATURE-7555` | The first-release phases already exist as FEATURE-7555's phases | A four-phase first-release item |
| Major dependency bumps | Held back | A minor release should not pull a major onto consumers or migrate the test suite | Taking them now |
