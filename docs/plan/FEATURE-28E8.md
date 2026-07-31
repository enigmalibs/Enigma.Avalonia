# FEATURE-28E8 — Repository & solution scaffolding

**Status:** TODO · 2 phases
**Branches:** `feature/feature-28e8-phase01-root-config`, `feature/feature-28e8-phase02-project-skeletons`

## 1. Objective

Bring the empty `/home/jo/Dev/Enigma.Avalonia` directory to a committed, zero-warning,
`dotnet build`-clean solution skeleton: a git repository on `main`, the four root configuration
files, the MIT licence, placeholder release documents, an `.slnx` with three projects, and one
passing smoke test. Everything after this item fills the skeleton in.

## 2. Solution invariants

Every later work item in this solution defers to this section. Where a later plan and this section
disagree, **this section wins** — fix the plan.

### 2.1 Layout

```
Enigma.Avalonia/
├── Enigma.Avalonia.slnx
├── .gitignore .gitattributes .editorconfig
├── Directory.Build.props  Directory.Packages.props  global.json
├── LICENSE.md  README.md  RELEASENOTES.md
├── docs/            roadmap.md · plan/ · done/   (guides/ arrives in FEATURE-2802)
├── src/Enigma.Avalonia.Desktop/                  (packable control library)
├── samples/Enigma.Avalonia.Desktop.Showcase/     (non-packable desktop app)
└── tests/Enigma.Avalonia.Desktop.UnitTests/      (xUnit v3, MTP)
```

### 2.2 Target frameworks

| Project | TFM(s) | Why |
|---|---|---|
| `Enigma.Avalonia.Desktop` | `net8.0;net10.0` | The current LTS pair. **No `netstandard2.0`** — Avalonia 12 ships `lib/net8.0` and `lib/net10.0` assets only, so a `netstandard2.0` target could not resolve the dependency at all. |
| `Enigma.Avalonia.Desktop.Showcase` | `net10.0` | An app ships a single runtime. |
| `Enigma.Avalonia.Desktop.UnitTests` | `net10.0` | Deliberate deviation from the xunit-v3 "mirror the library's TFMs" convention: headless Avalonia tests are the slow kind, and the library still *compiles* for `net8.0` on every build, so a net8-only compile break is still caught. Matches `Enigma.Icons.Avalonia.UnitTests`. |

### 2.3 Dependencies (Central Package Management — no `Version=` on any `PackageReference`)

The Avalonia set is **version-coupled**: bumped together, never individually.

| Package | Version | Referenced by |
|---|---|---|
| `Avalonia` | 12.1.1 | library, showcase, (tests transitively) |
| `Avalonia.Themes.Fluent` | 12.1.1 | library, showcase |
| `Avalonia.Desktop` | 12.1.1 | showcase |
| `Avalonia.Fonts.Inter` | 12.1.1 | showcase |
| `Avalonia.Headless.XUnit` | 12.1.1 | tests |
| `AvaloniaUI.DiagnosticsSupport` | 2.2.3 | showcase (Debug assets only; versioned independently of Avalonia but part of the coupled set) |
| `CommunityToolkit.Mvvm` | 8.4.2 | library, showcase |
| `Enigma.Core` | 1.0.0 | library |
| `Enigma.Icons.Avalonia` | 1.0.0 | **showcase only** |
| `LiveChartsCore.SkiaSharpView.Avalonia` | 2.1.0-dev-365 | showcase (Charts page) |
| `Microsoft.Extensions.Hosting` | 10.0.10 | showcase |
| `xunit.v3` | 3.2.2 | tests |

There is no `Avalonia.Diagnostics` package for Avalonia 12 — never reintroduce that id.
`Enigma.Icons.Avalonia` 1.0.0 declares a floor of `Avalonia` 12.1.0, which 12.1.1 satisfies.

### 2.4 Hard rules

1. **Zero-warning builds.** `TreatWarningsAsErrors` + `EnforceCodeStyleInBuild` solution-wide.
   *Watch out:* Avalonia's XAML compiler emits `AVLN*` diagnostics from an MSBuild task and
   `TreatWarningsAsErrors` does **not** promote them — the build can report `Build succeeded` with
   warnings. Read the warning count on any project that compiles `.axaml`; never trust the exit code
   alone.
2. **`ImplicitUsings` is `disable`** everywhere; every file declares its own `using` directives.
3. **`Nullable` enable**; no `!` without a commented justification.
4. **`LangVersion 14`**, set once in `Directory.Build.props` — never repeated in a csproj.
5. **XML doc comments on every public member** in the library (`GenerateDocumentationFile` makes
   CS1591 a build error).
6. **`.slnx`, never `.sln`.** Solution folders `/src/`, `/samples/`, `/tests/`.
7. **Every text file is LF with a final newline.**
8. **Central Package Management** — versions live only in `Directory.Packages.props`.
9. **Never commit.** The user owns every commit, tag and push.
10. **Clean-slate rule.** No *shipped or repository* artifact may reference the port source —
    that includes code, code comments, `README.md`, `RELEASENOTES.md`, guides, `CLAUDE.md` and
    commit messages. **`docs/plan/*.md` files are the single exception** (internal build contracts;
    a 1:1 port needs to name its source to be buildable).
11. **Namespace collision.** Inside namespace `Enigma.Avalonia.Desktop.*`, an inline reference to
    `Avalonia.Something` resolves against `Enigma.Avalonia` and fails to compile. Put `using`
    directives at **file scope, above the `namespace` declaration** — that resolves in the global
    namespace and is the fix. Reach for `global::Avalonia.…` only where a using cannot express it.
12. **Compiled bindings** are on by default; every `UserControl` root and every `DataTemplate` using
    `{Binding}` needs an explicit `x:DataType` or the build fails with `AVLN2100`.

### 2.5 Naming

- Assembly / root namespace / PackageId of the library: `Enigma.Avalonia.Desktop`.
- Theme dictionary URI: `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml`.
- Theme resource keys are prefixed **`Enigma`**: `EnigmaBackground*`, `EnigmaSurface*`,
  `EnigmaBorder*`, `EnigmaForeground*`, `EnigmaAccent*`, `EnigmaSuccess`, `EnigmaWarning`,
  `EnigmaError`.
- Showcase pages: `{Name}PageView` in `Views/`, `{Name}PageViewModel` in `ViewModels/`.

## 3. PHASE01 — Root configuration & solution file

**Prerequisite, user-run (not an agent action):** `git init -b main` in `/home/jo/Dev/Enigma.Avalonia`,
then commit the planning documents (`docs/roadmap.md`, `docs/plan/`, `docs/done/.gitkeep`). This phase
branches from that commit. If the directory is still not a git repository, **stop and ask** rather
than initialising it.

### Steps

1. `.gitignore` and `.gitattributes` from the `git-repo-hygiene` templates (`templates/gitignore`,
   `templates/gitattributes`). The `.gitattributes` LF rule is what keeps this repo free of
   line-ending churn for its whole life — no renormalize pass will ever be needed.
2. `.editorconfig` — the **full C# file** from `dotnet-solution-config/templates/editorconfig`
   (not git-repo-hygiene's minimal line-endings variant).
3. `Directory.Build.props` from `dotnet-solution-config/templates/Directory.Build.props`, with
   `Authors` = `Josué Clément`, `Copyright` = `Copyright © 2026 Josué Clément`, `LangVersion` 14,
   `Nullable` enable, `ImplicitUsings` disable, `TreatWarningsAsErrors` true,
   `EnforceCodeStyleInBuild` true.
4. `Directory.Packages.props` with `ManagePackageVersionsCentrally` and the §2.3 table, grouped and
   commented: Avalonia coupled set · library dependencies · showcase · tests.
5. `global.json` — `{"sdk":{"version":"10.0.100","rollForward":"latestFeature"},"test":{"runner":"Microsoft.Testing.Platform"}}`.
6. `LICENSE.md` from `dotnet-solution-setup/templates/LICENSE.md` (MIT, 2026, Josué Clément).
7. `README.md` and `RELEASENOTES.md` created **empty (0 bytes)** — FEATURE-2802 and FEATURE-1702 fill
   them.
8. `Enigma.Avalonia.slnx` with `/src/`, `/samples/` and `/tests/` solution folders, referencing the
   three projects PHASE02 creates. (Write the folders and project paths now; the csproj files land in
   PHASE02, so the solution does not load cleanly until then — that is expected within this item.)

### Acceptance criteria

- All eight artifacts exist at the solution root with the exact names above; every file is LF with a
  final newline.
- `Directory.Packages.props` lists every package from §2.3 and no others; every version matches.
- `README.md` and `RELEASENOTES.md` are zero-byte.
- No build is expected to succeed yet (no projects). State that explicitly in the completion doc as
  the Definition-of-Done equivalent for criteria 1–2, and verify the config by inspection.

## 4. PHASE02 — Three buildable project skeletons + smoke test

### Steps

1. `src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj` — `OutputType` Library,
   `TargetFrameworks` `net8.0;net10.0`, `AvaloniaUseCompiledBindingsByDefault` true,
   `GenerateDocumentationFile` true, `<AvaloniaResource Include="Themes\**" />`, and
   `PackageReference`s (no `Version=`) to `Avalonia`, `Avalonia.Themes.Fluent`,
   `CommunityToolkit.Mvvm`, `Enigma.Core`.
   **No packaging metadata** — the 12 package properties are added in FEATURE-1702 PHASE01, and
   `GeneratePackageOnBuild` is never set.
2. `samples/Enigma.Avalonia.Desktop.Showcase/Enigma.Avalonia.Desktop.Showcase.csproj` —
   `OutputType` WinExe, `net10.0`, `IsPackable` false, `AvaloniaUseCompiledBindingsByDefault` true,
   `<AvaloniaResource Include="Assets\**" />`, `appsettings.json` copied to output,
   `ProjectReference` to the library, `PackageReference`s to `Avalonia`, `Avalonia.Desktop`,
   `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `AvaloniaUI.DiagnosticsSupport` (Debug-only
   assets), `CommunityToolkit.Mvvm`, `Enigma.Icons.Avalonia`,
   `LiveChartsCore.SkiaSharpView.Avalonia`, `Microsoft.Extensions.Hosting`.
3. `tests/Enigma.Avalonia.Desktop.UnitTests/Enigma.Avalonia.Desktop.UnitTests.csproj` —
   `net10.0`, `OutputType` Exe, `IsPackable` false, `ImplicitUsings` disable,
   `PackageReference`s to `xunit.v3` and `Avalonia.Headless.XUnit`, `ProjectReference` to the
   library. **No** `Microsoft.NET.Test.Sdk`, **no** `xunit.runner.visualstudio`, **no**
   `GenerateDocumentationFile` (it would make CS1591 fire on every public test class).
4. Minimal placeholder content so all three compile: an empty `Themes/Fluent.axaml`
   `ResourceDictionary` in the library; `Program.cs` + `App.axaml(.cs)` + `Views/MainWindow.axaml(.cs)`
   showing an empty window in the showcase, wired per the house Avalonia pattern
   (`Main` sync + `StartWithClassicDesktopLifetime`, `.WithInterFont()`, `#if DEBUG
   .WithDeveloperTools()`); `TestAppBuilder.cs` (`[AvaloniaTestApplication]`) plus one smoke test in
   the test project.
5. `dotnet build Enigma.Avalonia.slnx` and `dotnet test --solution Enigma.Avalonia.slnx`.

### Acceptance criteria

- `dotnet build Enigma.Avalonia.slnx` succeeds for both library TFMs with **zero warnings**
  (verify the per-project warning count, not just the exit code — rule §2.4.1).
- `dotnet test --solution Enigma.Avalonia.slnx` runs under Microsoft.Testing.Platform and the single
  smoke test passes.
- `dotnet run --project samples/Enigma.Avalonia.Desktop.Showcase` opens an empty window (needs a real
  desktop session; a headless shell cannot show it — say so in the completion doc if it could not be
  verified by eye).
- No csproj repeats `LangVersion`, `Nullable`, `ImplicitUsings` or `TreatWarningsAsErrors`.
- No `PackageReference` carries a `Version=` attribute.
