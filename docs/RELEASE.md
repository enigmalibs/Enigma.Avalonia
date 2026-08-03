# Release runbook

Reusable checklist for publishing a new **Enigma.Avalonia.Desktop** version to NuGet. Only the
packable library project (`src/Enigma.Avalonia.Desktop`) is published; the showcase app under
`samples/` and the test project under `tests/` are deliberately non-packable and ship as source only.
There is no MSI profile — profiles are for applications, and this repository releases a library.

**This file is printed and followed by hand — it is never executed as a script.** An agent may run
the pre-release checks in §1 and a throwaway `dotnet pack` to verify the artifact; everything
outward-facing — the merge, the tag, the push, the publishing `pack` and `dotnet nuget push` — is
run by a human. The NuGet API key is a secret: never stored in the repository, never committed,
never echoed.

Replace `X.Y.Z` with the version being released (e.g. `1.0.0`) throughout. The version lives in
`src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj` (`<Version>`) and nowhere else — no
other project in the solution carries a package version.

## 1. Pre-release checks

Run from the repository root, on the branch that will be merged:

- [ ] `<Version>X.Y.Z</Version>` set in `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj`.
- [ ] `RELEASENOTES.md` has a top `X.Y.Z` section describing the release (newest-first; any `(unreleased)`
      heading renamed to `X.Y.Z`).
- [ ] `<PackageReleaseNotes>` in the library csproj summarizes the release and ends with
      `See RELEASENOTES.md for the full details.`
- [ ] README badges and the "what's new" callout reflect `X.Y.Z`.
- [ ] `SECURITY.md`'s supported-versions row still covers the version being released.
- [ ] `<TargetFrameworks>` reads `net8.0;net10.0`. There is deliberately **no** `netstandard2.0`:
      Avalonia 12 ships `lib/net8.0` and `lib/net10.0` assets only, so a `netstandard2.0` target
      could not resolve the dependency. Any change here is a compatibility decision — propose it,
      confirm it, and log it in `RELEASENOTES.md` *Compatibility*.
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
tagged (bare `X.Y.Z` vs. `vX.Y.Z`). The repository has **no tags yet**, so the house default
applies: a **bare** `X.Y.Z` tag, and that becomes the convention every later release matches. Tag
the merge commit and push the tag:

```bash
git tag X.Y.Z
git push origin X.Y.Z
```

## 4. Pack

`GeneratePackageOnBuild` is **off** for this library, so no `.nupkg` is produced on an ordinary
build — pack explicitly in Release to get the artifact you publish:

```bash
dotnet pack src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj -c Release -o ./artifacts
```

This writes `./artifacts/Enigma.Avalonia.Desktop.X.Y.Z.nupkg` — and nothing else. `artifacts/` is
already covered by `.gitignore`; the artifact is never committed.

Confirm the version in the filename matches the tag, then inspect the package before pushing it
(`unzip -l` the `.nupkg`, or open it in any zip tool — a `.nupkg` is a zip):

- [ ] The output directory holds **the `.nupkg` and nothing else**. A `.snupkg` beside it means the
      symbol opt-in (`IncludeSymbols` / `SymbolPackageFormat`, or a stray `--include-symbols`) crept
      back in. A release ships exactly one file.
- [ ] `README.md` is present **and non-empty** — an empty packed README is a silent nuget.org
      landing-page failure, not a build error.
- [ ] `LICENSE.md` is present.
- [ ] In the nuspec: `<version>` is `X.Y.Z`, `<title>`, `<license type="file">LICENSE.md</license>`,
      `<readme>README.md</readme>` and `<releaseNotes>` are all correct.
- [ ] The dependency groups are exactly `net8.0` and `net10.0`, each listing `Avalonia`,
      `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm` and `Enigma.Core` at the versions pinned in
      `Directory.Packages.props` — and **no** showcase-only package (`Enigma.Icons.Avalonia`,
      `LiveChartsCore.*`, `Avalonia.Desktop`, `Avalonia.Fonts.Inter`,
      `AvaloniaUI.DiagnosticsSupport`, `Microsoft.Extensions.Hosting`) and no test package.
- [ ] `lib/net8.0/` and `lib/net10.0/` each carry both `Enigma.Avalonia.Desktop.dll` and
      `Enigma.Avalonia.Desktop.xml` — the XML documentation file is part of the public surface.

## 5. Push to NuGet

Publish with a NuGet API key that has push rights for the `Enigma.Avalonia.Desktop` package:

```bash
dotnet nuget push ./artifacts/Enigma.Avalonia.Desktop.X.Y.Z.nupkg \
  --api-key <NUGET_API_KEY> \
  --source https://api.nuget.org/v3/index.json
```

`dotnet pack` produces **only** the `.nupkg` — that single file is what gets pushed. This project
does not ship a `.snupkg` symbols package: the symbol opt-in properties (`IncludeSymbols`,
`SymbolPackageFormat`) are deliberately absent from the csproj and must stay that way, and `pack` is
never run with `--include-symbols`. The API key is a secret — never commit or echo it.

## 6. Post-publish verification

- [ ] The package page shows the new version: <https://www.nuget.org/packages/Enigma.Avalonia.Desktop>
      (indexing can take a few minutes).
- [ ] The README NuGet badge resolves to `X.Y.Z` (shields.io caches briefly).
- [ ] A scratch project can restore the new version:
      ```bash
      dotnet add package Enigma.Avalonia.Desktop --version X.Y.Z
      ```
- [ ] The GitHub release/tag is present and its notes match `RELEASENOTES.md`.
