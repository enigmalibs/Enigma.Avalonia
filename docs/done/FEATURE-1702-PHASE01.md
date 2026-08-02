# FEATURE-1702 PHASE01 — Package metadata & third-party licence audit

**Branch:** `feature/feature-1702-phase01-release`
**Plan:** `docs/plan/FEATURE-1702.md` §3

## 1. Summary

Made `Enigma.Avalonia.Desktop` packable at 1.0.0 and audited what the package redistributes.

- **Packaging metadata** — the twelve house-required properties now live in the library csproj, in
  their own `PropertyGroup` beneath the build settings. Eleven are new; `GenerateDocumentationFile`
  (property 12) was already set by FEATURE-28E8 and stays where it belongs, with the build settings
  rather than the packaging block.
- **Packing `ItemGroup`** — `README.md` and `LICENSE.md` from the solution root, `Pack="true"`,
  `PackagePath="\"`. `PackageReadmeFile`/`PackageLicenseFile` only *name* the files; this is what
  puts them in the `.nupkg`.
- **The five forbidden properties stay absent** — `GeneratePackageOnBuild`, `IncludeSymbols`,
  `SymbolPackageFormat`, `PublishRepositoryUrl`, `EmbedUntrackedSources`. The comment above the
  block now says so explicitly, replacing the "deliberately absent — added by FEATURE-1702 PHASE01"
  note that this phase made obsolete.
- **Third-party licence audit** — §3. Every shipped dependency, direct and transitive, is MIT.

Nothing under `src/**/*.cs`, `Themes/`, `samples/` or `tests/` was touched: this phase is csproj
metadata and documentation only.

## 2. Files touched

| File | Change |
|---|---|
| `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj` | Modified — packaging `PropertyGroup` (11 properties) + packing `ItemGroup` added; obsolete "deliberately absent" comment removed |
| `docs/roadmap.md` | `FEATURE-1702` item row → `IN PROGRESS`; PHASE01 row → `DONE` |
| `docs/plan/FEATURE-1702.md` | Item status → `IN PROGRESS`; §3 heading → `DONE` |
| `CLAUDE.md` | Modified — doc-freshness sweep: CPM rule scoped to *dependency* versions, new "Packaging metadata lives in the library csproj" rule carrying the no-symbol-package house rule |
| `docs/done/FEATURE-1702-PHASE01.md` | Created (this file) |

## 3. Third-party licence audit

Scope is **what ships**. A consumer installing `Enigma.Avalonia.Desktop` receives the library
assembly plus every runtime dependency it resolves; compile-only and test-only packages are not
redistributed and are out of scope. Licences were read from each package's `.nuspec` in the local
NuGet cache, not from memory or a web page.

| Dependency | Version | Kind | Ships? | Licence | Source of finding |
|---|---|---|---|---|---|
| `Avalonia` | 12.1.1 | direct, both TFMs | **yes** | MIT | nuspec `<license type="expression">MIT` |
| `Avalonia.Themes.Fluent` | 12.1.1 | direct, both TFMs | **yes** | MIT | nuspec `<license type="expression">MIT` |
| `CommunityToolkit.Mvvm` | 8.4.2 | direct, both TFMs | **yes** | MIT | nuspec `<license type="expression">MIT` |
| `Enigma.Core` | 1.0.0 | direct, both TFMs | **yes** | MIT | nuspec `<license type="file">LICENSE.md`; packed file read — MIT text, © 2026 Josué Clément |
| `BouncyCastle.Cryptography` | 2.6.2 | transitive via `Enigma.Core`, both TFMs | **yes** | MIT | nuspec `<license type="expression">MIT` |
| `Avalonia.BuildServices` | 11.3.2 | transitive via `Avalonia`, both TFMs | **yes** | MIT | nuspec `<license type="expression">MIT` |
| `Avalonia.Remote.Protocol` | 12.1.1 | transitive via `Avalonia`, both TFMs | **yes** | MIT | nuspec `<license type="expression">MIT` |
| `MicroCom.Runtime` | 0.11.6 | transitive via `Avalonia`, both TFMs | **yes** | MIT | nuspec `<license type="expression">MIT` |
| `Enigma.Icons.Avalonia`, `LiveChartsCore.SkiaSharpView.Avalonia`, `Avalonia.Desktop`, `Avalonia.Fonts.Inter`, `AvaloniaUI.DiagnosticsSupport`, `Microsoft.Extensions.Hosting` | — | showcase only — non-packable sample | no | — | not referenced by the library csproj |
| `xunit.v3`, `Avalonia.Headless.XUnit` | — | test-only | no | — | not referenced by the library csproj |

**The last three rows are additions to the plan's table.** Plan §3 lists `BouncyCastle.Cryptography`
as the only transitive dependency; `Avalonia` 12.1.1 in fact brings three more of its own. They are
redistributed to consumers exactly like BouncyCastle, so they belong in the audit. All three are MIT
— the finding does not change the conclusion, only its completeness.

`CommunityToolkit.Mvvm`'s `net8.0` dependency group is empty, and it has no `net10.0` group (the
`net8.0` assets apply), so it contributes no further transitive packages on either of our TFMs. Its
four `netstandard2.0` dependencies never resolve here.

**Redistribution:** MIT permits redistribution without restriction, subject only to preserving the
copyright and permission notice. NuGet satisfies that structurally — each dependency ships as its
own package carrying its own licence, and `Enigma.Avalonia.Desktop` redistributes no third-party
code or assets inside its own `.nupkg`. No copyleft, no source-availability obligation, and no
attribution requirement that the package's own `LICENSE.md` does not already meet.

**`LICENSE.md`:** present at the solution root, MIT, © 2026 Josué Clément — the same author as the
package. Referenced by `<PackageLicenseFile>LICENSE.md</PackageLicenseFile>` and packed by the new
`ItemGroup`. PHASE03's pack-verify confirms it is actually embedded.

### `THIRD-PARTY-NOTICES.md` — decided: **not warranted**

The sibling `Enigma.Icons` repository ships one because it **redistributes third-party artwork**
inside its own package — the notice is what carries the upstream attribution alongside the copied
asset. `Enigma.Avalonia.Desktop` redistributes nothing: every dependency arrives as its own NuGet
package with its own licence metadata, which nuget.org displays and consumers' tooling
(`dotnet list package`, licence scanners, SBOM generators) reads directly. A notices file here would
restate metadata that is already machine-readable and authoritative, and would need hand-updating
on every dependency bump — a new way to be wrong, with no obligation it discharges. All eight
shipped dependencies are permissive MIT, none of which imposes a notice requirement beyond the
licence text each package already carries. Revisit if a future dependency is copied into the package
rather than referenced, or carries a non-MIT licence.

## 4. Acceptance criteria

| Criterion | Status |
|---|---|
| All 12 properties present and correct | Met — `grep -c` over the twelve element names returns 12; values match plan §3's table |
| Packing `ItemGroup` present | Met — `README.md` and `LICENSE.md`, `Pack="true"`, `PackagePath="\"` |
| `GeneratePackageOnBuild`, `IncludeSymbols`, `SymbolPackageFormat`, `PublishRepositoryUrl`, `EmbedUntrackedSources` absent | Met — repo-wide grep over `*.csproj`/`*.props`/`*.slnx` returns only the comment naming them |
| Build clean in Release | Met — see below |
| Audit table recorded | Met — §3, extended by three transitive Avalonia packages |
| `THIRD-PARTY-NOTICES.md` decision recorded with reasoning | Met — §3, decided *no* |

**Definition of Done criteria 1–2.** `dotnet build Enigma.Avalonia.slnx -c Release` →
**0 warnings, 0 errors** across all four build outputs (library `net8.0` + `net10.0`, showcase,
tests). The warning **count** was read rather than the exit code, per `CLAUDE.md` gotcha 2 — the
XAML compiler emits `AVLN*` from an MSBuild task and `TreatWarningsAsErrors` does not promote those;
none were reported. `dotnet test --solution Enigma.Avalonia.slnx -c Release` → **281 passed,
0 failed, 0 skipped**. The count is unchanged from FEATURE-2802 PHASE02: this phase adds no test,
and none is warranted — the assertions that matter here are made against the *packed artifact*, and
PHASE03's pack-verify is where the plan places them.

## 5. Deviations & follow-ups

### Deviations from the plan

1. **`PackageReleaseNotes` was written now, not left empty for PHASE02.** Plan §3's property table
   says property 11 is "written in PHASE02", but the same section's acceptance criterion requires
   "all 12 properties present and correct" at the end of *this* phase. The two cannot both hold with
   the element absent. Resolved by writing a genuine first-release value now — accurate as it
   stands, and already ending with the mandated `See RELEASENOTES.md for the full details.` — with a
   csproj comment recording that PHASE02 refines it to mirror the top of `RELEASENOTES.md`. PHASE02
   re-checks the property in its own acceptance criteria, so nothing is lost by having it correct
   early, and the csproj is never in a state that fails PHASE01's stated criterion.
2. **The audit table gained three rows.** `Avalonia.BuildServices` 11.3.2,
   `Avalonia.Remote.Protocol` 12.1.1 and `MicroCom.Runtime` 0.11.6 are transitive runtime
   dependencies of `Avalonia` that plan §3 does not list. All MIT; conclusion unchanged (§3).
3. **`Description` says "fourteen typed editors", not sixteen.** `docs/roadmap.md`'s
   `FEATURE-22A5` PHASE03 row calls it "Editors (16 controls)", which counts `BaseEditor` and
   `BaseEditor<T>`. Those are the base classes, not typed editors — `Controls/Editors/` holds
   16 files, 14 of which are concrete typed editors. The README already says fourteen; the package
   `Description` now matches it.

### Follow-ups

1. **`RELEASENOTES.md` is a 0-byte file at the root.** `PackageReleaseNotes` and the README callout
   both point at it. PHASE02 fills it; until then the pointer is dangling.
2. **`SECURITY.md`'s supported-versions row (`1.0.x`)** still wants confirming against the version
   actually published — carried forward from FEATURE-2802 PHASE02 §6, unresolved and out of scope
   here.
3. **Plan §2's "there is currently no git remote" is stale — a remote now exists.** `git remote -v`
   reports `origin https://github.com/enigmalibs/Enigma.Avalonia.git`, which matches the
   `RepositoryUrl` and `PackageProjectUrl` this phase set. §2 instructs the completion doc to state
   that the publish path is blocked on a missing remote; it is not. PHASE03 should drop that caveat
   from its runbook framing rather than repeat it. Related: `git tag` is empty, so PHASE03's
   house default of a bare `X.Y.Z` tag (no `v` prefix) applies with nothing to match against.
4. **Line endings:** every file this phase created or modified is LF with a final newline; no CRLF
   was observed. No action taken or recommended.
