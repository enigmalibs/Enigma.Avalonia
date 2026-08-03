# FEATURE-28E8 PHASE02 — Three buildable project skeletons + smoke test

**Status:** DONE
**Branch:** `feature/feature-28e8-phase02-project-skeletons`
**Plan:** `docs/plan/FEATURE-28E8.md` §4

## Summary

Turned the configured-but-empty solution root into a solution that builds and tests. The three
projects the `.slnx` declared in PHASE01 now exist, each carrying only the properties that are
genuinely its own — everything shared comes from `Directory.Build.props` and every package version
from `Directory.Packages.props`.

Placeholder content is deliberately the minimum that makes each project compile and proves the wiring
works: an empty `Themes/Fluent.axaml` resource dictionary in the library, an empty window in the
showcase, and one smoke test. The showcase's **host wiring is intentionally absent** — FEATURE-57C8
PHASE01 owns it and writes it new rather than porting it, so pre-empting it here would only create
something to unpick.

With this phase the `FEATURE-28E8` item is complete: the skeleton is in place and every later item
fills it in.

## Files/modules touched

### Created — library (`src/Enigma.Avalonia.Desktop/`)

| File | What it carries |
|---|---|
| `Enigma.Avalonia.Desktop.csproj` | `OutputType` Library · `net8.0;net10.0` · `AvaloniaUseCompiledBindingsByDefault` · `GenerateDocumentationFile` · `<AvaloniaResource Include="Themes\**" />` · 4 `PackageReference`s (`Avalonia`, `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm`, `Enigma.Core`) |
| `Themes/Fluent.axaml` | Empty `ResourceDictionary` — the library's single public XAML entry point, `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml` |

### Created — showcase (`samples/Enigma.Avalonia.Desktop.Showcase/`)

| File | What it carries |
|---|---|
| `Enigma.Avalonia.Desktop.Showcase.csproj` | `OutputType` WinExe · `net10.0` · `IsPackable` false · compiled bindings · `Assets\**` glob · `appsettings.json` copy · `ProjectReference` to the library · 8 `PackageReference`s + `AvaloniaUI.DiagnosticsSupport` with Debug-only assets |
| `Program.cs` | `[STAThread]`, sync `Main`, `StartWithClassicDesktopLifetime`; `UsePlatformDetect()` + `#if DEBUG .WithDeveloperTools()` + `.WithInterFont().LogToTrace()` |
| `App.axaml` | `RequestedThemeVariant="Dark"`, `<FluentTheme/>` |
| `App.axaml.cs` | `Initialize` + `OnFrameworkInitializationCompleted` assigning `desktop.MainWindow` |
| `Views/MainWindow.axaml(.cs)` | Empty 1280×800 centred window |
| `appsettings.json` | Minimal `Logging` section — see deviation 1 |

### Created — tests (`tests/Enigma.Avalonia.Desktop.UnitTests/`)

| File | What it carries |
|---|---|
| `Enigma.Avalonia.Desktop.UnitTests.csproj` | `net10.0` · `OutputType` Exe · `IsPackable` false · `xunit.v3` + `Avalonia.Headless.XUnit` · `ProjectReference` to the library. No `Microsoft.NET.Test.Sdk`, no `xunit.runner.visualstudio`, no `GenerateDocumentationFile` |
| `TestAppBuilder.cs` | `[assembly: AvaloniaTestApplication]` + headless `AppBuilder` |
| `ThemeDictionaryTests.cs` | The smoke test |

### Modified

- `docs/roadmap.md` — `PHASE02` to `IN PROGRESS`, then `PHASE02` **and the `FEATURE-28E8` item row**
  to `DONE` (final phase).
- `docs/plan/FEATURE-28E8.md` — item status `IN PROGRESS` → `DONE`; the `PHASE02` heading marker
  likewise.

### Deleted

None.

## The smoke test

`FluentTheme_LoadsFromTheLibraryAssembly` loads the theme dictionary through a `ResourceInclude` at
`avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — the exact URI plan §2.5 publishes as the
library's contract. It is one assertion, but it is not a tautology: it fails if the assembly name
changes, if the `AvaloniaResource` glob stops picking the dictionary up, or if the dictionary stops
being XAML-compilable.

That was checked rather than assumed. Repointing the URI at a non-existent `Themes/DoesNotExist.axaml`
turns the run red (`failed: 1`); restoring it turns it green again. A smoke test that cannot fail is
worse than no smoke test, because it reports confidence it hasn't earned.

## Deviations & follow-ups

1. **`appsettings.json` was created, not just referenced.** Plan §4 step 2 requires the csproj to copy
   `appsettings.json` to the output, but step 4 does not list the file among the placeholder content,
   and FEATURE-57C8 PHASE01 is where `ShowcaseOptions` binds it. A `<None Update>` on a file that does
   not exist is a silent no-op, so the csproj would have carried a dangling instruction. A minimal
   `Logging` section was written instead — FEATURE-57C8 extends it.
2. **`<AvaloniaResource Include="Assets\**" />` currently globs an empty directory** — the showcase
   tracks no art assets yet (git does not track empty directories, so `Assets/` itself is absent). The
   glob is declared per plan §4 step 2 and is a harmless no-op until FEATURE-57C8 adds assets. Not a
   defect; noted so it isn't later mistaken for one.
3. **No `ApplicationIcon` / `app.manifest`** on the showcase. The plan does not call for either and the
   repo tracks no `.ico`. The sibling `Enigma.Icons.Avalonia.Gallery` does set `ApplicationManifest`;
   the difference is deliberate, not an oversight.
4. **PHASE01's heads-up (its deviation 4) was applied.** Plan §4 step 3 lists `ImplicitUsings` disable
   on the test csproj, but `Directory.Build.props` already sets it solution-wide, and §4's own
   acceptance criteria forbid repeating it. All three csproj files therefore omit `ImplicitUsings`,
   `LangVersion`, `Nullable` and `TreatWarningsAsErrors`. Where the plan's step text and its acceptance
   criteria disagreed, the acceptance criteria won.
5. **Release configuration was verified too**, though the plan only asks for the default. `Release` is
   where `#if DEBUG .WithDeveloperTools()` compiles out and the `AvaloniaUI.DiagnosticsSupport` asset
   conditions take effect — a real breakage surface that a Debug-only check would miss. It builds with
   0 warnings.
6. **Line endings: nothing to report.** All 12 new files are LF with a final newline; no CRLF byte
   anywhere in the working tree.

## Build/test evidence

All commands run from the solution root, on a clean `obj/`+`bin/` (deleted before the audited build,
so nothing was skipped as up to date).

| Check | Result |
|---|---|
| `dotnet build Enigma.Avalonia.slnx` (clean, `-v normal`) | **Build succeeded · 0 Warning(s) · 0 Error(s)** |
| Per-project warning audit (plan §2.4.1) | `grep ': warning'` over the full normal-verbosity log: **0 matches**; **no `AVLN*` diagnostic** on any project. The exit code alone was not trusted — Avalonia's XAML diagnostics come from an MSBuild task that `TreatWarningsAsErrors` does not promote |
| Both library TFMs produced | `bin/Debug/net8.0/Enigma.Avalonia.Desktop.dll` **and** `bin/Debug/net10.0/Enigma.Avalonia.Desktop.dll` |
| `dotnet build Enigma.Avalonia.slnx -c Release` | 0 Warning(s) · 0 Error(s) |
| `dotnet test --solution Enigma.Avalonia.slnx` | **Passed!** total: 1 · failed: 0 · succeeded: 1 · skipped: 0 (xUnit v3 3.2.2 under Microsoft.Testing.Platform, runner from `global.json`) |
| Smoke-test negative control | URI repointed at a non-existent asset → **failed: 1**; restored → passed. The test can fail |
| `dotnet run --project samples/Enigma.Avalonia.Desktop.Showcase` | **Verified by eye.** A 1280×800 centred window titled *Enigma.Avalonia.Desktop Showcase* opened on the desktop session with an empty dark client area, stayed up, and logged nothing to stdout/stderr |
| Working tree | 12 new source files; `bin/`/`obj/` correctly ignored (`.gitignore:30,31`) — no build artifact is stageable |

## Acceptance criteria

| Criterion (plan §4) | Met |
|---|---|
| `dotnet build` succeeds for both library TFMs with zero warnings, verified per project rather than by exit code | **Yes** |
| `dotnet test --solution` runs under Microsoft.Testing.Platform and the single smoke test passes | **Yes** |
| `dotnet run --project samples/…Showcase` opens an empty window | **Yes** — verified by eye on a real desktop session |
| No csproj repeats `LangVersion`, `Nullable`, `ImplicitUsings` or `TreatWarningsAsErrors` | **Yes** — `grep` over all three: 0 matches |
| No `PackageReference` carries a `Version=` attribute | **Yes** — `grep` over all three: 0 matches |
