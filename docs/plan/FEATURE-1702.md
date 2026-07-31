# FEATURE-1702 — Release preparation & NuGet publish runbook (1.0.0)

**Status:** TODO · 3 phases
**Branches:** `feature/feature-1702-phaseNN-release`
**Depends on:** FEATURE-22A5, FEATURE-6EB0, FEATURE-57C8, FEATURE-2802
**Solution invariants:** `docs/plan/FEATURE-28E8.md` §2.

## 1. Objective

Take `Enigma.Avalonia.Desktop` to a publishable **1.0.0**: package metadata, third-party licence
audit, release notes, `docs/RELEASE.md`, a Release-configuration pre-flight, a local pack-verify, and
the **printed** pack/tag/push runbook.

This is a **first release** (nothing published under this package id). The `dotnet-release` first-release
shape is four phases; here it is three, because its guides phase is `FEATURE-2802` PHASE01. That
mapping is deliberate — do not re-write the guides here.

## 2. Execution boundary — read before starting

**The agent makes in-repo edits and prints the runbook. The user runs everything outward-facing.**

| Agent may run | Agent prints, never runs |
|---|---|
| `dotnet build` / `dotnet test` (pre-flight) | `git tag`, `git push` |
| `dotnet pack` into a **throwaway** verify directory, then deletes it | the publish `dotnet pack -o ./artifacts` |
| — | `dotnet nuget push` |
| — | the merge/push to the default branch |

The NuGet API key is a secret: never stored, committed or echoed. There is currently **no git
remote** — say so in the completion doc; the publish path cannot run until one is added
(`https://github.com/enigmalibs/Enigma.Avalonia`).

No MSI profile: profiles are apps-only and this item releases a library. The showcase is a
non-packable sample and is not released.

## 3. PHASE01 — Package metadata & third-party licence audit — TODO

### Metadata — all 12 properties in `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj`

| # | Property | Value |
|---|---|---|
| 1 | `PackageId` | `Enigma.Avalonia.Desktop` |
| 2 | `Version` | `1.0.0` |
| 3 | `Title` | `Enigma.Avalonia.Desktop — Avalonia desktop controls` |
| 4 | `Description` | One paragraph naming what ships: navigation, docking, ribbon, typed editors, dialog/overlay/infobar, settings cards, file & folder pickers, the CollectionView data subsystem, and the Fluent Dark/Light theme. |
| 5 | `PackageTags` | `avalonia avaloniaui controls desktop navigation docking ribbon editors mvvm fluent ui dialog settings collectionview overlay infobar dotnet` |
| 6 | `PackageReadmeFile` | `README.md` |
| 7 | `PackageLicenseFile` | `LICENSE.md` |
| 8 | `RepositoryUrl` | `https://github.com/enigmalibs/Enigma.Avalonia` |
| 9 | `RepositoryType` | `git` |
| 10 | `PackageProjectUrl` | `https://github.com/enigmalibs/Enigma.Avalonia` |
| 11 | `PackageReleaseNotes` | written in PHASE02 |
| 12 | `GenerateDocumentationFile` | `true` (already set in FEATURE-28E8) |

Plus the packing `ItemGroup`:

```xml
<ItemGroup>
  <None Include="..\..\README.md" Pack="true" PackagePath="\" />
  <None Include="..\..\LICENSE.md" Pack="true" PackagePath="\" />
</ItemGroup>
```

**Must be absent:** `GeneratePackageOnBuild`, `IncludeSymbols`, `SymbolPackageFormat`,
`PublishRepositoryUrl`, `EmbedUntrackedSources`. House rule: a release ships exactly one file, the
`.nupkg` — no symbol package, not even as an option.

### Third-party licence audit (first release only)

Audit what **ships**. Compile-only and test-only packages are not redistributed and are out of scope.

| Dependency | Kind | Ships? | Licence |
|---|---|---|---|
| `Avalonia` | runtime, both TFMs | yes — audit | MIT |
| `Avalonia.Themes.Fluent` | runtime, both TFMs | yes — audit | MIT |
| `CommunityToolkit.Mvvm` | runtime, both TFMs | yes — audit | MIT |
| `Enigma.Core` | runtime, both TFMs | yes — audit | MIT |
| `BouncyCastle.Cryptography` | runtime, transitive via `Enigma.Core` | yes — audit | MIT |
| `Enigma.Icons.Avalonia`, `LiveChartsCore.*`, `Avalonia.Desktop/Fonts.Inter`, `AvaloniaUI.DiagnosticsSupport`, `Microsoft.Extensions.Hosting` | showcase only — non-packable | no | — |
| `xunit.v3`, `Avalonia.Headless.XUnit` | test-only | no | — |

Confirm each shipped dependency's licence permits redistribution, confirm `LICENSE.md` is present,
correct and referenced by `PackageLicenseFile`, and **record the findings table in the completion
doc**. Decide and record whether a root `THIRD-PARTY-NOTICES.md` is warranted (the sibling
`Enigma.Icons` repo ships one because it redistributes artwork; this package redistributes no
third-party code or assets, so the expected answer is *no* — but state the reasoning).

**Acceptance:** all 12 properties present and correct; packing `ItemGroup` present; the five forbidden
properties absent; build clean in Release; audit table recorded.

## 4. PHASE02 — RELEASENOTES, PackageReleaseNotes, README callout — TODO

### `RELEASENOTES.md` (root) — first-release variant

From `dotnet-release/templates/RELEASENOTES.md`, headed `Enigma.Avalonia.Desktop v1.0.0 Release
Notes`, with these sub-sections in order:

- **Feature overview** — grouped by family: Navigation · Docking · Ribbon · Editors · Dialogs,
  Overlay & InfoBar · Settings controls · File & folder dialogs · Data (CollectionView) · Theme
  system. One line per public control/service.
- **Dependencies** — the shipped set and their versions: `Avalonia` 12.1.1,
  `Avalonia.Themes.Fluent` 12.1.1, `CommunityToolkit.Mvvm` 8.4.2, `Enigma.Core` 1.0.0 (which brings
  `BouncyCastle.Cryptography`). State that the Avalonia set is version-coupled and bumped together.
- **Compatibility** — targets `net8.0` and `net10.0`; **no `netstandard2.0`**, because Avalonia 12
  ships `net8.0`/`net10.0` assets only. Note that the package does not bring the Avalonia *app*
  packages transitively.
- **Version** — `1.0.0`.

No `CHANGELOG.md` — `RELEASENOTES.md` is the single release-notes source.

### `PackageReleaseNotes` (csproj property 11)

Short prose mirroring the top of `RELEASENOTES.md`, ending exactly with
`See RELEASENOTES.md for the full details.`

### README what's-new callout

A single blockquote after the intro:
`> **What's new in 1.0** — first release. See [RELEASENOTES.md](RELEASENOTES.md).`

Also confirm the README's supported-target-frameworks line reads `net8.0` + `net10.0`.

**Acceptance:** `RELEASENOTES.md` non-empty and complete, every shipped control and service listed;
`PackageReleaseNotes` present and ending with the required sentence; README callout in place; no
`CHANGELOG.md` exists; clean-slate rule holds (no port-source reference in either document).

## 5. PHASE03 — docs/RELEASE.md, pre-flight, pack-verify, printed runbook — TODO

### Steps

1. **`docs/RELEASE.md`** from `dotnet-release/templates/RELEASE.md` with placeholders filled:
   `{{PACKAGE_ID}}` = `Enigma.Avalonia.Desktop`, `{{SOLUTION}}` = `Enigma.Avalonia.slnx`,
   `{{LIB_CSPROJ}}` = `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj`,
   `{{LIB_DIR}}` = `src/Enigma.Avalonia.Desktop`, `{{DEFAULT_BRANCH}}` = `main`.
   Create only if missing.
2. **Pre-flight (agent runs):**
   `dotnet build Enigma.Avalonia.slnx -c Release` and
   `dotnet test --solution Enigma.Avalonia.slnx -c Release` — both must be clean and green, with the
   `AVLN*` warning-count check on every `.axaml` project.
3. **Pack-verify (agent runs, then deletes):**
   `dotnet pack src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj -c Release -o ./artifacts-verify`
   then inspect the artifact and confirm:
   - the `.nupkg` version is `1.0.0`;
   - the directory holds **the `.nupkg` and nothing else** — a `.snupkg` means a symbol opt-in leaked
     back in;
   - `README.md` is embedded and **non-empty** (an empty packed README is a silent nuget.org
     landing-page failure);
   - `LICENSE.md` is embedded;
   - the nuspec's `<version>`, `<title>`, `<license type="file">`, `<readme>` and `<releaseNotes>` are
     all correct;
   - the dependency groups are exactly `net8.0` and `net10.0`, each listing `Avalonia` 12.1.1,
     `Avalonia.Themes.Fluent` 12.1.1, `CommunityToolkit.Mvvm` 8.4.2 and `Enigma.Core` 1.0.0 — and
     **no** `Enigma.Icons.Avalonia`, `LiveChartsCore.*` or test package;
   - both TFMs carry the XML documentation file.
   Then **delete `./artifacts-verify`** — it is scratch and is never committed.
4. **Print the runbook** (do not run): Release build + test, merge to `main`, `git tag 1.0.0` +
   `git push origin 1.0.0` (bare `X.Y.Z` — the repo has no tags yet, so the house default applies),
   `dotnet pack … -o ./artifacts`, `dotnet nuget push ./artifacts/Enigma.Avalonia.Desktop.1.0.0.nupkg
   --api-key <NUGET_API_KEY> --source https://api.nuget.org/v3/index.json`, then the post-publish
   checks (package page shows 1.0.0, the README badge resolves,
   `dotnet add package Enigma.Avalonia.Desktop --version 1.0.0` restores, the tag exists and matches
   `RELEASENOTES.md`). State explicitly that the agent printed these and the user runs them.

**Acceptance:** `docs/RELEASE.md` exists with no placeholders left; Release build and test both clean
and green (evidence in the completion doc); every pack-verify assertion above checked and recorded;
`artifacts-verify/` deleted and absent from the tree; the runbook printed to the console; nothing
tagged, pushed or published by the agent.
