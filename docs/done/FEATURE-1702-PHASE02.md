# FEATURE-1702 PHASE02 — RELEASENOTES, PackageReleaseNotes, README what's-new callout

**Branch:** `feature/feature-1702-phase02-releasenotes`
**Plan:** `docs/plan/FEATURE-1702.md` §4

## 1. Summary

Wrote the 1.0.0 release notes and pointed the two things that reference them at real content.

- **`RELEASENOTES.md`** — was a 0-byte tracked file at the root; now the first-release variant of
  `dotnet-release/templates/RELEASENOTES.md`, headed `Enigma.Avalonia.Desktop v1.0.0 Release Notes`,
  with *Feature overview* → *Dependencies* → *Compatibility* → *Version* in the plan's order. The
  feature overview is grouped by the nine families the plan names, one line per public control and
  service.
- **`PackageReleaseNotes`** (csproj property 11) — refined from PHASE01's provisional value to
  mirror the new opening paragraph: the control/service split, then the families, then the TFMs. The
  closing sentence is unchanged and exact.
- **README what's-new callout** — the `<!-- The what's-new callout slots in here at release time. -->`
  placeholder FEATURE-2802 PHASE02 left after the intro is now the blockquote, verbatim from the
  plan.

No source, theme, showcase or test file was touched: this phase is release documentation plus one
csproj property.

## 2. Files touched

| File | Change |
|---|---|
| `RELEASENOTES.md` | Written — 0 bytes → the full v1.0.0 first-release section |
| `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj` | Modified — `PackageReleaseNotes` refined; its comment updated now that PHASE02 has done the refining it promised |
| `README.md` | Modified — placeholder comment on line 16 replaced by the what's-new blockquote |
| `docs/roadmap.md` | PHASE02 row → `IN PROGRESS`, then → `DONE` |
| `docs/plan/FEATURE-1702.md` | §4 heading → `IN PROGRESS`, then → `DONE` |
| `docs/done/FEATURE-1702-PHASE02.md` | Created (this file) |

## 3. Acceptance criteria

| Criterion | Status |
|---|---|
| `RELEASENOTES.md` non-empty and complete | Met — 133 lines, all four sub-sections present in the plan's order |
| Every shipped control and service listed | Met — see the coverage check below |
| `PackageReleaseNotes` present and ending with the required sentence | Met — the element ends `See RELEASENOTES.md for the full details.</PackageReleaseNotes>`, matched literally |
| README callout in place | Met — `README.md:16`, immediately after the intro paragraph |
| No `CHANGELOG.md` exists | Met — absent; `RELEASENOTES.md` is the single release-notes source |
| Clean-slate rule holds — no port-source reference in either document | Met — case-insensitive grep for the port source over both files returns nothing |

**Coverage check — every public type appears in the notes.** Rather than reading the notes against
the source by eye, the 66 public types declared under `src/Enigma.Avalonia.Desktop/**/*.cs`
(excluding `obj/`) were extracted and each name searched for in `RELEASENOTES.md`:

```
public types: 66
MISSING from RELEASENOTES.md: none
```

That covers the 40 controls and control-support types, the 6 service interfaces with their 6
implementations, the 2 extension classes, the 7 `Data` types and the 5 enums. The theme system has
no public C# type, so it is described by its resource URI, its dictionary counts (29 `Enigma*`
colours per variant, 26 `Enigma*` brushes, 19 control templates — all read from
`Themes/Colors.axaml`, `Themes/Brushes.axaml` and `Themes/Fluent.axaml`) and the Avalonia Fluent key
families it overrides.

The README's supported-target-frameworks line was re-checked as the plan requires: `README.md:52`
reads *"Targets **.NET 8.0** and **.NET 10.0**; built on Avalonia 12.1.1."* — correct, unchanged.

## 4. Build/test evidence

**Definition of Done criteria 1–2.** `dotnet build Enigma.Avalonia.slnx -c Release` →
**0 warnings, 0 errors** across all four outputs (library `net8.0` + `net10.0`, showcase, tests). The
warning **count** was read rather than the exit code, per `CLAUDE.md` gotcha 2 — the XAML compiler
emits `AVLN*` from an MSBuild task that `TreatWarningsAsErrors` does not promote; none were reported.
Editing the csproj forces the library to recompile on both TFMs, so this was a real rebuild of the
XAML, not an up-to-date skip.

`dotnet test --solution Enigma.Avalonia.slnx -c Release` → **281 passed, 0 failed, 0 skipped.**

**No test was added, and none is warranted.** Plan §4's acceptance criteria are all statements about
document content, and each was verified mechanically above (a type-coverage script, a literal match
on the `PackageReleaseNotes` closing sentence, greps for the callout, the absent `CHANGELOG.md` and
the clean-slate rule). The assertions that need a test harness are made against the *packed
artifact*, and PHASE03's pack-verify is where the plan places them — including the check that the
`<releaseNotes>` this phase wrote actually reaches the nuspec.

## 5. Deviations & follow-ups

### Deviations from the plan

1. **`RELEASENOTES.md` was filled, not created.** Plan §4 says "`RELEASENOTES.md` (root) —
   first-release variant", implying creation; the file already existed as a 0-byte tracked file from
   FEATURE-28E8 PHASE01. Writing into it is the same outcome and respects the template's
   *create only if missing — never clobber* rule (there was nothing to clobber).
2. **`PackageReleaseNotes` was refined rather than written from nothing.** PHASE01 wrote a genuine
   provisional value (recorded as its deviation 1) because its own acceptance criterion demanded all
   twelve properties. This phase replaced it with prose mirroring the finished notes' opening
   paragraph and updated the csproj comment, which had promised exactly this refinement.
3. **The feature overview uses nested bullets, not `###` headings per family.** The template's
   *Feature overview* is a flat bullet list with bold category leaders; the plan asks for the nine
   families *and* one line per public control/service. Nested bullets under each bold family
   satisfy both without introducing a heading level the template does not have.

### Follow-ups

1. **PHASE01 follow-up 2 is resolved.** `SECURITY.md`'s supported-versions row reads `1.0.x`, and
   `RELEASENOTES.md` now declares the release as `1.0.0` — the row is correct as written and needs
   no edit. Re-confirm only if the published version differs from `1.0.0`.
2. **PHASE01 follow-ups 1 and 3 stand as recorded.** Follow-up 1 (the dangling 0-byte
   `RELEASENOTES.md`) is closed by this phase. Follow-up 3 remains PHASE03's to act on: plan §2's
   "there is currently no git remote" is stale — `origin` exists and matches `RepositoryUrl` — and
   `git tag` is still empty, so the bare `X.Y.Z` house default applies.
3. **`INavigationViewModel`'s XML doc names a type that does not exist.**
   `src/Enigma.Avalonia.Desktop/Services/INavigationViewModel.cs:8` reads *"Takes precedence over
   INavigationLifecycle when both are implemented"*; there is no `INavigationLifecycle` in the
   solution. FEATURE-2802 PHASE01 found the same thing and documented the real behaviour in the
   navigation guide rather than editing the source. The stale sentence ships in the XML
   documentation file, so it is visible in consumers' IntelliSense — worth a one-line fix, but it is
   a source edit outside this phase's scope. Not addressed here.
4. **Line endings:** every file this phase created or modified is LF with a final newline; no CRLF
   was observed anywhere in the tree. No action taken or recommended.
