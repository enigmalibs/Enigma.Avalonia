# Release runbook

Reusable checklist for publishing a new version to NuGet. Two packages are published, **always
together and at the same version**: **Enigma.Avalonia.Desktop** (`src/Enigma.Avalonia.Desktop`) and
**Enigma.Avalonia.Mobile** (`src/Enigma.Avalonia.Mobile`), which compiles the desktop library's palette
files, so a release of one is a release of both. The showcase app under `samples/` and the test
projects under `tests/` are deliberately non-packable and ship as source only. There is no MSI
profile — profiles are for applications, and this repository releases libraries.

**This file is printed and followed by hand — it is never executed as a script.** An agent may run
the pre-release checks in §1 and a throwaway `dotnet pack` to verify the artifact; everything
outward-facing — the merge, the tag, the push, the publishing `pack` and `dotnet nuget push` — is
run by a human. The NuGet API key is a secret: never stored in the repository, never committed,
never echoed.

Replace `X.Y.Z` with the version being released (e.g. `1.3.0`) throughout. The version lives in the
two library csproj files (`<Version>`) and nowhere else — no other project in the solution carries a
package version — and both carry the same value.

## 1. Pre-release checks

Run from the repository root, on the branch that will be merged:

- [ ] `<Version>X.Y.Z</Version>` set in **both** `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj`
      and `src/Enigma.Avalonia.Mobile/Enigma.Avalonia.Mobile.csproj`.
- [ ] `RELEASENOTES.md` has a top `X.Y.Z` section describing the release (newest-first; any `(unreleased)`
      heading renamed to `X.Y.Z`).
- [ ] `<PackageReleaseNotes>` in **each** library csproj summarizes the release for that package and
      ends with `See RELEASENOTES.md for the full details.`
- [ ] The packed READMEs reflect `X.Y.Z` — the root `README.md` (Desktop's: badges and the "what's
      new" callout) and `src/Enigma.Avalonia.Mobile/README.md` (Mobile's: its Avalonia version line).
- [ ] `SECURITY.md`'s supported-versions row still covers the version being released.
- [ ] Desktop's `<TargetFrameworks>` reads `net8.0;net10.0`, Mobile's `<TargetFramework>` reads
      `net10.0`. There is deliberately **no** `netstandard2.0`: Avalonia 12 ships `lib/net8.0` and
      `lib/net10.0` assets only, so a `netstandard2.0` target could not resolve the dependency. Any
      change here is a compatibility decision — propose it, confirm it, and log it in
      `RELEASENOTES.md` *Compatibility*.
- [ ] Clean, warning-free build across all TFMs:
      ```bash
      dotnet build Enigma.Avalonia.slnx -c Release
      ```
      **Read the warning count, not just the exit code.** Avalonia's XAML compiler emits `AVLN*`
      warnings from an MSBuild task that `TreatWarningsAsErrors` does not promote, so the build can
      print `Build succeeded` while carrying XAML warnings. The summary must read `0 Warning(s)`.
      If the build was incremental, the XAML compiler may not have re-run at all — precede it with
      `dotnet clean Enigma.Avalonia.slnx -c Release` so the count means something.
- [ ] Full test suite green:
      ```bash
      dotnet test --solution Enigma.Avalonia.slnx -c Release
      # global.json selects Microsoft.Testing.Platform as the runner, hence --solution;
      # a bare `dotnet test <solution>` is not the invocation this repository uses.
      # If the test apphost can't find the runtime, prefix: DOTNET_ROOT=~/.dotnet
      ```
- [ ] README samples verified against the built version, and `docs/guides/` still matches the public
      API.

## 2. Merge to the default branch

`main` is the published branch. Day-to-day work lands on `develop` (one branch and one commit per
dev); a release merges `develop` into `main` via a pull request (or fast-forward), then you check it
out locally:

```bash
git switch main
git pull
```

Pack and tag from this commit, not from the feature or `develop` branch — the nuspec embeds the
`HEAD` commit hash as `<repository commit="…">`, so packing anywhere else ships a package that
points at a commit which is not the released one.

## 3. Tag the release

Match the repository's existing tag convention — run `git tag` to see how prior releases were
tagged (bare `X.Y.Z` vs. `vX.Y.Z`). The convention is a **bare** `X.Y.Z` tag — `1.0.0` and `1.1.0`
were tagged that way — and every later release matches it. One tag releases both packages. Tag the merge
commit and push the tag:

```bash
git tag X.Y.Z
git push origin X.Y.Z
```

## 4. Pack

`GeneratePackageOnBuild` is **off** for both libraries, so no `.nupkg` is produced on an ordinary
build — pack each explicitly in Release to get the artifacts you publish:

```bash
dotnet pack src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj -c Release -o ./artifacts
dotnet pack src/Enigma.Avalonia.Mobile/Enigma.Avalonia.Mobile.csproj -c Release -o ./artifacts
```

This writes `./artifacts/Enigma.Avalonia.Desktop.X.Y.Z.nupkg` and
`./artifacts/Enigma.Avalonia.Mobile.X.Y.Z.nupkg` — and nothing else. `artifacts/` is already covered
by `.gitignore`; the artifacts are never committed.

Confirm the version in both filenames matches the tag, then inspect each package before pushing it
(`unzip -l` the `.nupkg`, or open it in any zip tool — a `.nupkg` is a zip):

- [ ] The output directory holds **the two `.nupkg` files and nothing else**. A `.snupkg` beside one
      means the symbol opt-in (`IncludeSymbols` / `SymbolPackageFormat`, or a stray
      `--include-symbols`) crept back in. A release ships exactly one file per package.
- [ ] `README.md` is present **and non-empty** in each — an empty packed README is a silent nuget.org
      landing-page failure, not a build error. Desktop's is the root `README.md`, Mobile's its own
      `src/Enigma.Avalonia.Mobile/README.md`; a swapped one shows the wrong landing page.
- [ ] `LICENSE.md` is present.
- [ ] In the nuspec: `<version>` is `X.Y.Z`, `<title>`, `<license type="file">LICENSE.md</license>`,
      `<readme>README.md</readme>` and `<releaseNotes>` are all correct.
- [ ] Desktop's dependency groups are exactly `net8.0` and `net10.0`, each listing `Avalonia`,
      `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm` and `Enigma.Core` at the versions pinned in
      `Directory.Packages.props`. Mobile's is exactly one group, `net10.0`, listing `Avalonia` and
      `Avalonia.Themes.Fluent` only. Neither lists a showcase-only package (`Enigma.Icons.Avalonia`,
      `LiveChartsCore.*`, `Avalonia.Desktop`, `Avalonia.Fonts.Inter`,
      `AvaloniaUI.DiagnosticsSupport`, `Microsoft.Extensions.Hosting`) or a test package, and Mobile
      does not depend on Desktop.
- [ ] Desktop's `lib/net8.0/` and `lib/net10.0/` each carry both `Enigma.Avalonia.Desktop.dll` and
      `Enigma.Avalonia.Desktop.xml` — the XML documentation file is part of the public surface.
      Mobile's `lib/net10.0/` carries `Enigma.Avalonia.Mobile.dll` and `Enigma.Avalonia.Mobile.xml`.

## 5. Push to NuGet

Publish with a NuGet API key that has push rights for both packages:

```bash
dotnet nuget push ./artifacts/Enigma.Avalonia.Desktop.X.Y.Z.nupkg \
  --api-key <NUGET_API_KEY> \
  --source https://api.nuget.org/v3/index.json
dotnet nuget push ./artifacts/Enigma.Avalonia.Mobile.X.Y.Z.nupkg \
  --api-key <NUGET_API_KEY> \
  --source https://api.nuget.org/v3/index.json
```

`dotnet pack` produces **only** a `.nupkg` per package — those two files are what get pushed. This project
does not ship a `.snupkg` symbols package: the symbol opt-in properties (`IncludeSymbols`,
`SymbolPackageFormat`) are deliberately absent from the csproj and must stay that way, and `pack` is
never run with `--include-symbols`. The API key is a secret — never commit or echo it.

## 6. Post-publish verification

- [ ] Both package pages show the new version (indexing can take a few minutes):
      <https://www.nuget.org/packages/Enigma.Avalonia.Desktop> and
      <https://www.nuget.org/packages/Enigma.Avalonia.Mobile>.
- [ ] Each README's NuGet badge resolves to `X.Y.Z` (shields.io caches briefly).
- [ ] A scratch project can restore both new versions:
      ```bash
      dotnet add package Enigma.Avalonia.Desktop --version X.Y.Z
      dotnet add package Enigma.Avalonia.Mobile --version X.Y.Z
      ```
- [ ] The GitHub release/tag is present and its notes match `RELEASENOTES.md`.
