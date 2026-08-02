# CLAUDE.md

Agent instructions for the Enigma.Avalonia solution. This file is internal — it is never packed into
the NuGet package.

## Commands

```bash
# Build the whole solution (library for net8.0 + net10.0, showcase, tests)
dotnet build Enigma.Avalonia.slnx

# Run the whole test suite (xUnit v3 under Microsoft.Testing.Platform)
dotnet test --solution Enigma.Avalonia.slnx

# Run the showcase app — needs a real desktop session; a headless shell cannot show it
dotnet run --project samples/Enigma.Avalonia.Desktop.Showcase

# Pack the library (release time only — see "Releasing" below)
dotnet pack src/Enigma.Avalonia.Desktop/Enigma.Avalonia.Desktop.csproj -c Release
```

The SDK is pinned in `global.json` (10.0.100, `rollForward: latestFeature`), which also selects
Microsoft.Testing.Platform as the test runner — hence `dotnet test --solution`, not a bare
`dotnet test`.

## Architecture

Three projects, one direction of dependency:

```
src/Enigma.Avalonia.Desktop/            packable control library   (net8.0;net10.0)
  Controls/    ContentDialog · Docking · Editors · InfoBar · Navigation · Ribbon
               + Overlay, SettingsCard, SettingsCardExpander
  Services/    the six interfaces + implementations, and their extension methods
  Data/        CollectionView, CollectionViewSource, sorting/filtering/grouping types
  Themes/      Colors.axaml · Brushes.axaml · Controls/**  — merged by Fluent.axaml
        ▲                            ▲
        │ ProjectReference           │ ProjectReference
samples/Enigma.Avalonia.Desktop.Showcase/    non-packable desktop app (net10.0)
tests/Enigma.Avalonia.Desktop.UnitTests/     xUnit v3 + Avalonia.Headless (net10.0)
```

Neither the showcase nor the tests are referenced by the library. The showcase is the place to see
every control working; the tests embed the library's theme XAML as `EmbeddedResource` (the XAML
compiler makes it unreadable from the library assembly) and assert on it as text.

The library ships two kinds of thing, and the distinction runs through everything: **controls** are
`TemplatedControl`s styled by `Themes/Fluent.axaml`, and **services** are DI interfaces in
`Enigma.Avalonia.Desktop.Services`. Three services (`IContentDialogService`, `IOverlayService`,
`IInfoBarService`) require a `RegisterHost` call against a host control placed in the window; two
(`IFileDialogService`, `IFolderDialogService`) require `SetStorageProvider`. All five calls happen at
startup, before the window is shown — `samples/…/App.axaml.cs` is the reference wiring.

## Solution rules

- **Zero-warning builds.** `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild` are on solution-wide.
- **`ImplicitUsings` is `disable`** everywhere; every file declares its own `using` directives.
- **`Nullable` is enable**; no `!` without a commented justification.
- **XML doc comments on every public member** of the library — `GenerateDocumentationFile` makes
  CS1591 a build error.
- **Central Package Management.** Versions live only in `Directory.Packages.props`; no
  `PackageReference` carries a `Version=` attribute. The Avalonia set is version-coupled — bump the
  whole set together, never one package. There is no `Avalonia.Diagnostics` package for Avalonia 12;
  never reintroduce that id.
- **`LangVersion` 14**, set once in `Directory.Build.props` — never repeated in a csproj. The same
  goes for `Nullable`, `ImplicitUsings` and `TreatWarningsAsErrors`.
- **`.slnx`, never `.sln`.** Solution folders `/src/`, `/samples/`, `/tests/`.
- **Every text file is LF with a final newline.**
- **Never commit.** The user owns every commit, tag and push.

## Gotchas

These cost real time when rediscovered. Read them before touching the corresponding area.

1. **The `Enigma.Avalonia.*` / `Avalonia.*` namespace collision.** Inside namespace
   `Enigma.Avalonia.Desktop.*`, an inline reference to `Avalonia.Something` resolves against
   `Enigma.Avalonia` and fails to compile. The fix is to put `using` directives at **file scope,
   above the `namespace` declaration**, where they resolve in the global namespace. Reach for
   `global::Avalonia.…` only where a using cannot express it.

2. **`AVLN*` XAML warnings are not promoted by `TreatWarningsAsErrors`.** Avalonia's XAML compiler
   emits them from an MSBuild task, so the build can print `Build succeeded` while carrying XAML
   warnings. **Read the warning count** on every project that compiles `.axaml`; never trust the exit
   code alone.

3. **Compiled bindings need `x:DataType`.** `AvaloniaUseCompiledBindingsByDefault` is on, so every
   `UserControl` root and every `DataTemplate` that uses `{Binding}` needs an explicit `x:DataType`
   or the build fails with `AVLN2100`.

4. **A new control is three files, not one.** Custom controls are `TemplatedControl`s: the class goes
   in `Controls/`, its template goes in `Themes/Controls/`, and the template dictionary must be added
   as a `ResourceInclude` in `Themes/Fluent.axaml`. A template file that exists but is never merged
   compiles, ships, and applies to nothing. (`ResourceKeyTests` enforces this.)

5. **`{TemplateBinding}` is one-way only.** A two-way template binding must be written
   `{Binding …, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}`.

6. **Hit testing needs a non-null `Background`.** A control with `Background="{x:Null}"` — or none at
   all — does not receive pointer events over its empty area.

7. **`OnApplyTemplate` can run more than once.** Detach the previous handlers before attaching new
   ones, or events fire two, three, *n* times.

8. **Property and command style.** Semi-auto properties with `SetProperty`
   (`public string? Foo { get; set => SetProperty(ref field, value); }`) and `IRelayCommand`
   properties initialised in the constructor. **Never** `[ObservableProperty]` or `[RelayCommand]`;
   classes are not `partial`.

9. **Releasing is the user's job.** `docs/RELEASE.md` is printed and followed by hand, never
   executed. The NuGet API key is never stored, committed or echoed.

10. **Where the truth lives.** `docs/roadmap.md` is the registry of every work item;
    `docs/plan/<ID>.md` holds the full plan for one item; `docs/done/<ID>.md` records what a finished
    dev actually did. Read the plan before implementing, and update all three as the workflow
    requires. `docs/guides/` is the user-facing documentation and must stay in step with the public
    API.
