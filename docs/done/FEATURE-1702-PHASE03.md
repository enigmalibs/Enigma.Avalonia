# FEATURE-1702 PHASE03 — docs/RELEASE.md, pre-flight, pack-verify, printed runbook

**Branch:** `feature/feature-1702-phase03-release`
**Plan:** `docs/plan/FEATURE-1702.md` §5

## 1. Summary

Closed out the 1.0.0 release preparation: wrote the release runbook, ran the Release pre-flight, and
verified the real packed artifact against every assertion the plan lists — then deleted the artifact.

- **`docs/RELEASE.md`** — created (it did not exist) from `dotnet-release/templates/RELEASE.md` with
  all five placeholders filled and the template's instruction comment dropped. Six sections, same
  order and numbering as the template: pre-release checks → merge to `main` → tag → pack → push →
  post-publish verification. `CLAUDE.md` gotcha 9 already promised this file existed and was followed
  by hand; it now does.
- **Pre-flight** — `dotnet clean` + `dotnet build -c Release` + `dotnet test --solution -c Release`,
  all clean and green. The `clean` was deliberate: an incremental build skips the Avalonia XAML
  compiler entirely, so `0 Warning(s)` on an up-to-date build proves nothing about `AVLN*`.
- **Pack-verify** — packed into a throwaway `./artifacts-verify`, checked all eight of the plan's
  assertions against the real `.nupkg` (§4 below), then deleted the directory. Nothing was tagged,
  pushed or published.
- **Runbook printed** to the console for the user to run.
- **`INavigationViewModel`'s summary corrected** — added on the user's instruction after the
  verification above surfaced it (deviation 5). The pack-verify proved the XML documentation file
  ships in both TFM folders, so a doc comment describing a design the code does not implement was a
  1.0.0 defect visible in every consumer's IntelliSense, not a cosmetic one.

No theme, showcase, test or csproj file was touched. This phase is one new document, one XML doc
comment, and verification of what the previous two phases wrote.

## 2. Files touched

| File | Change |
|---|---|
| `docs/RELEASE.md` | **Created** — 134 lines, the filled release runbook |
| `CLAUDE.md` | Modified — line 18's pack comment pointed at `docs/RELEASE.md` instead of the forward reference `see "Releasing" below`, now that the file it implied actually exists (documentation freshness sweep, accepted) |
| `src/Enigma.Avalonia.Desktop/Services/INavigationViewModel.cs` | Modified — the `<summary>` block rewritten to describe the dispatch the code actually performs (deviation 5). Doc comment only; no signature, no behaviour change |
| `docs/roadmap.md` | PHASE03 row → `IN PROGRESS`, then → `DONE`; the `FEATURE-1702` item row → `DONE` (final phase) |
| `docs/plan/FEATURE-1702.md` | §5 heading → `IN PROGRESS`, then → `DONE` |
| `docs/done/FEATURE-1702-PHASE03.md` | Created (this file) |

Created and deleted within the phase, never committed: `./artifacts-verify/` (one `.nupkg`).

## 3. Acceptance criteria

| Criterion | Status |
|---|---|
| `docs/RELEASE.md` exists with no placeholders left | Met — `grep '{{'` returns nothing; all five placeholders filled |
| Release build clean | Met — `0 Warning(s)`, `0 Error(s)` across all four outputs |
| Release test suite green | Met — 281 passed, 0 failed, 0 skipped |
| Every pack-verify assertion checked and recorded | Met — all eight, §4 below |
| `artifacts-verify/` deleted and absent from the tree | Met — removed; `git status --porcelain` shows only the tracked doc edits |
| Runbook printed to the console | Met — printed as the phase's final output |
| Nothing tagged, pushed or published by the agent | Met — `git tag` still returns nothing; no `push`, no `nuget push`; the only repository-state change was `git switch -c` |

## 4. Pack-verify record

`dotnet pack src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj -c Release -o ./artifacts-verify`
→ `Enigma.Avalonia.Desktop.1.0.0.nupkg` (173 069 bytes).

| # | Plan assertion | Result |
|---|---|---|
| 1 | `.nupkg` version is `1.0.0` | **Pass** — filename `Enigma.Avalonia.Desktop.1.0.0.nupkg` |
| 2 | The directory holds the `.nupkg` and nothing else | **Pass** — 1 file, extension count `1 nupkg`; no `.snupkg`, so no symbol opt-in leaked back in |
| 3 | `README.md` embedded and non-empty | **Pass** — 8 666 bytes, byte-for-byte the size of the root `README.md`, opens on the `# Enigma.Avalonia.Desktop` heading and the two badges |
| 4 | `LICENSE.md` embedded | **Pass** — 1 063 bytes, matches the root file, opens on `Copyright (c) 2026 Josué Clément` |
| 5 | nuspec `<version>` | **Pass** — `1.0.0` |
| 6 | nuspec `<title>`, `<license type="file">`, `<readme>`, `<releaseNotes>` | **Pass** — `Enigma.Avalonia.Desktop — Avalonia desktop controls`; `<license type="file">LICENSE.md</license>`; `<readme>README.md</readme>`; `<releaseNotes>` is PHASE02's prose, ending exactly `See RELEASENOTES.md for the full details.` |
| 7 | Dependency groups exactly `net8.0` + `net10.0`, four packages each, no showcase or test package | **Pass** — two groups, nothing else; each lists `Avalonia` 12.1.1, `Avalonia.Themes.Fluent` 12.1.1, `CommunityToolkit.Mvvm` 8.4.2, `Enigma.Core` 1.0.0. No `Enigma.Icons.Avalonia`, no `LiveChartsCore.*`, no `Avalonia.Desktop`/`Fonts.Inter`/`DiagnosticsSupport`, no `Microsoft.Extensions.Hosting`, no `xunit.v3`, no `Avalonia.Headless.XUnit` |
| 8 | Both TFMs carry the XML documentation file | **Pass** — `lib/net8.0/Enigma.Avalonia.Desktop.xml` and `lib/net10.0/Enigma.Avalonia.Desktop.xml`, 195 008 bytes each |

Full entry list — ten entries, no `.pdb`, no stray content:

```
Enigma.Avalonia.Desktop.nuspec                      3 010
LICENSE.md                                          1 063
README.md                                           8 666
[Content_Types].xml                                   582
_rels/.rels                                           513
lib/net10.0/Enigma.Avalonia.Desktop.dll           205 824
lib/net10.0/Enigma.Avalonia.Desktop.xml           195 008
lib/net8.0/Enigma.Avalonia.Desktop.dll            205 824
lib/net8.0/Enigma.Avalonia.Desktop.xml            195 008
package/services/metadata/core-properties/….psmdcp  1 286
```

The nuspec also carries `<repository type="git" url="https://github.com/enigmalibs/Enigma.Avalonia"
commit="aaf5006…" />` and `<licenseUrl>https://aka.ms/deprecateLicenseUrl</licenseUrl>` — see
follow-ups 2 and 3.

## 5. Build/test evidence

**Definition of Done criterion 1.** `dotnet clean Enigma.Avalonia.slnx -c Release` followed by
`dotnet build Enigma.Avalonia.slnx -c Release` → **`0 Warning(s)`, `0 Error(s)`**, with all four
outputs relinked from scratch (library `net8.0` + `net10.0`, showcase, tests). The warning **count**
was read rather than the exit code, per `CLAUDE.md` gotcha 2, and a `grep` for `AVLN`/`CS####`/`warn`
over the whole log matched only the two summary lines. Because the `clean` forced the Avalonia XAML
compiler to re-run on both TFM passes of the library and on the showcase, the zero is a real zero and
not an up-to-date skip.

**Definition of Done criterion 2.** `dotnet test --solution Enigma.Avalonia.slnx -c Release` →
**281 passed, 0 failed, 0 skipped** (`Test run summary: Passed!`). Same count as PHASE02 — expected,
since this phase adds no executable code.

**Both criteria were re-established after the `INavigationViewModel` edit.** A doc comment recompiles
the library on both TFM passes and regenerates the shipped XML documentation file, so the first run's
evidence no longer covered the tree. `clean` → `build` → `test` was repeated in full: again
**`0 Warning(s)`, `0 Error(s)`** with no `AVLN*`/`CS####` line anywhere in the log, and again
**281 passed, 0 failed, 0 skipped**. The new `<see cref="INavigationService"/>` resolved — it appears
in the generated XML as `T:Enigma.Avalonia.Desktop.Services.INavigationService`, so CS1574 (a build
*error* here, under `TreatWarningsAsErrors`) did not fire.

**The pack-verify was re-run for the one assertion the edit could move.** Rather than trust the
source, the corrected summary was read back out of a freshly packed `.nupkg`: both
`lib/net8.0/Enigma.Avalonia.Desktop.xml` and `lib/net10.0/Enigma.Avalonia.Desktop.xml` carry the new
text, and a search for `INavigationLifecycle` across both returns nothing. The verify directory was
deleted again.

**A sweep confirmed this was the only instance of the defect class.** The phantom survived because it
was a bare type name in prose rather than a `<see cref="…"/>`, which the compiler never validates. So
every bare interface- or PascalCase-looking identifier in the library's doc comments (22 distinct,
`cref` attributes stripped first) was checked against the declarations in `src/`: the remainder are
all real BCL types (`ArgumentNullException`, `InvalidOperationException`, `NotSupportedException`),
real Avalonia types (`ContentControl`, `GridSplitter`, `ItemsControl`, `ItemsSourceView`, `ListBox`,
`TextBlock`), members rather than types (`OnAppearingAsync`, `PageFactory`, `CurrentPage`,
`DataContext`, …) or plain words (`ViewModel`, `ViewModels`). No second phantom type exists.

**No test was added, and none is warranted.** Plan §5's acceptance criteria are assertions about a
*packed artifact* and about a document's content, neither of which a unit test in this solution can
reach: the pack-verify inspects a `.nupkg` produced by `dotnet pack`, which is a release step, not a
build step. Each assertion was instead verified mechanically against the real artifact — the zip
entry table and the nuspec were read out of the package itself, not inferred from the csproj — and
recorded in §4. `docs/RELEASE.md` was verified by inspection plus a placeholder `grep`, an encoding
check (UTF-8, LF, 0 CR bytes) and a final-newline check.

## 6. Deviations & follow-ups

### Deviations from the plan

1. **`docs/RELEASE.md` adapts the template in six places rather than only filling placeholders.**
   The plan says "from `dotnet-release/templates/RELEASE.md` with placeholders filled"; a literal
   fill would have shipped instructions that are wrong for this solution. The changes:
   - **Test command** → `dotnet test --solution Enigma.Avalonia.slnx -c Release`. The template's
     `dotnet test {{SOLUTION}} -c Release` is not this repository's invocation — `global.json`
     selects Microsoft.Testing.Platform, which needs `--solution`. Plan §5 step 2 already writes the
     command in the `--solution` form, so the runbook now agrees with the plan.
   - **`AVLN*` warning-count check** added to the build item, with the `dotnet clean` precondition.
     Plan §5 step 2 requires this check of the agent; the runbook is what a human follows for *every*
     future release, so the check belongs there too — otherwise it is a fact only this phase knew.
   - **`<TargetFrameworks>` item** → the template's "`netstandard*` preserved" is inverted here: this
     package deliberately has no `netstandard2.0`, because Avalonia 12 ships `net8.0`/`net10.0`
     assets only. Left as the template wrote it, the checklist would have read as an instruction to
     preserve a target that must not exist.
   - **§4's "optionally inspect the package contents"** → the concrete eight-point assertion
     checklist this phase actually ran. The plan makes these checks mandatory for 1.0.0; making them
     an optional aside in the runbook would lose them at 1.1.0.
   - **Non-packable projects named explicitly** — the showcase under `samples/` and the tests under
     `tests/`, in place of the template's generic "Tools / CLI / Desktop projects".
   - **The template's leading HTML instruction comment was dropped**, since it is placeholder text
     and the acceptance criterion is that no placeholder survives.
2. **§2 gained a note about the embedded commit hash — a finding from the pack-verify, not from the
   plan.** The nuspec carries `<repository … commit="…">`, populated from `HEAD` at pack time; the
   verify pack recorded `aaf5006`, the PHASE02 commit, because that is where this phase branched
   from. Harmless for a throwaway verify, but it means the *publishing* pack must run on the merged
   `main` commit or the shipped package points at a commit that is not the released one. That is a
   correctness constraint on the release, so it is stated in the runbook rather than left implicit.
3. **§1 gained a `SECURITY.md` supported-versions item.** PHASE02 follow-up 1 had to hand-check that
   `SECURITY.md`'s `1.0.x` row matched the release; a per-release checklist is where that check
   belongs so the next version does not have to rediscover it.
4. **The four extra sections.** The plan's step 4 lists the runbook as five commands plus the
   post-publish checks; the template's shape is six numbered sections. The document follows the
   template's six sections, which cover exactly the plan's steps in the plan's order — nothing added
   to the release procedure itself, and nothing dropped.
5. **One source edit, outside the plan's scope, made on the user's explicit instruction.** Plan §5
   scopes this phase to `docs/RELEASE.md` plus verification, and the phase first recorded
   `INavigationViewModel`'s doc comment as a recommendation only. The user then asked for it to be
   fixed, so it was — after the phase's own commit `21f165d` had already landed, which makes the fix
   a second commit on `feature/feature-1702-phase03-release` (this file's update rides with it).
   Squashing it into `21f165d` keeps the workflow's one-commit-per-dev shape; landing it as its own
   commit keeps a source change visibly separate from a documentation phase. Either is defensible and
   the choice is the user's. If it should be tracked as its own work item instead, revert the two
   files and re-land them as a `BUG-HHHH` — nothing else in the phase depends on the fix.

   **Two sentences were wrong, not the one originally flagged.** The whole `<summary>` described a
   dual-dispatch design the code never had:

   - *"Takes precedence over INavigationLifecycle when both are implemented"* — no
     `INavigationLifecycle` exists anywhere in the solution.
   - *"Can be implemented by either page views (Controls) or their ViewModels"* — false, and the more
     harmful of the two. `NavigationService.cs:201` and `:221` both test
     `page.DataContext is INavigationViewModel`; the `Control` itself is never inspected. A consumer
     following the comment and implementing the interface on their page Control gets **no callbacks
     and no error** — `OnDisappearingAsync` never runs, so a navigation guard written that way
     silently fails to guard.

   The replacement states the dispatch the code performs and matches `docs/guides/navigation.md`,
   which FEATURE-2802 PHASE01 had already written against the source rather than against the comment
   (`navigation.md:28`, `:226`–`:230`). `DataContext` and `Control` are marked up as `<c>…</c>` rather
   than `cref`-ed on purpose: a `cref` to an Avalonia type from inside namespace
   `Enigma.Avalonia.Desktop.*` runs straight into `CLAUDE.md` gotcha 1, where `Avalonia.…` resolves
   against `Enigma.Avalonia`. The one `cref` added, `<see cref="INavigationService"/>`, is in the same
   namespace and resolved cleanly.

### Follow-ups

1. **Plan §2's "there is currently no git remote" is stale, and the correction is in the release's
   favour.** `origin` exists and is exactly `https://github.com/enigmalibs/Enigma.Avalonia.git`,
   matching `RepositoryUrl`, with `origin/HEAD → main`. The publish path is therefore **not** blocked
   on adding a remote, contrary to what the plan says the completion doc should record. PHASE01
   follow-up 3 and PHASE02 follow-up 2 both flagged this; recording it here closes it. `git tag`
   still returns nothing, so the bare `X.Y.Z` house default stands and the runbook says so.
2. **`<licenseUrl>https://aka.ms/deprecateLicenseUrl</licenseUrl>` in the nuspec is expected — no
   action.** NuGet emits that shim automatically whenever `<license type="file">` is used, so older
   clients still find a licence URL. It is not a stray property and must not be "fixed".
3. **`INavigationViewModel`'s XML doc is fixed — this closes the follow-up FEATURE-2802 PHASE01
   opened.** See deviation 5. Nothing is left outstanding on it: the phantom type is gone from the
   source and from the packed XML on both TFMs, and the sweep in §5 shows no second instance.
4. **`FEATURE-1702` is complete — the 1.0.0 line is prepared but not published.** Every in-repo
   release artifact now exists; what remains is the human-run runbook. `FEATURE-66EB`
   (accessibility baseline) stays deferred until 1.0.0 is actually on nuget.org, per the roadmap's
   note.
5. **Line endings:** `docs/RELEASE.md` is UTF-8, LF, 0 CR bytes, with a final newline; no CRLF was
   observed anywhere in the tree. No action taken or recommended.
