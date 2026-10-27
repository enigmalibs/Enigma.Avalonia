# FEATURE-7555 — Enigma.Avalonia.Mobile theme library

**Status:** TODO
**Type:** FEATURE
**Branches:** `feature/feature-7555-phase01-mobile-library`, `feature/feature-7555-phase02-mobile-guide`, `feature/feature-7555-phase03-mobile-packaging`
**Run:** feature/2026-10-27-avalonia-mobile-release
**Depends on:** FEATURE-8763

## 1. Objective

A new packable library, **`Enigma.Avalonia.Mobile`**, for Avalonia mobile applications (Android
first), that ships the **same palette and the same restyling of the standard controls** as
`Enigma.Avalonia.Desktop` — so a mobile app and a desktop app built on these packages look the same.

## 2. Context & constraints

- Desktop's look is three layers merged by `Themes/Fluent.axaml`: `Colors.axaml` (31 colour keys,
  Dark and Light `ThemeDictionaries`), `Brushes.axaml` (28 `Enigma*` brushes plus the 23 FluentTheme
  `TextControl*` / `ComboBox*` override keys that re-colour the framework `TextBox` and
  `ComboBox`), and one `ControlTheme` per Desktop control. The theme declares no application-level
  `Style` — the first two files **are** the library's colours, brushes and standard-control styling.
- The control templates (Ribbon, Docking, NavigationView, Editors, ContentDialog, InfoBar, Overlay,
  SettingsCard…) style Desktop's own controls and are out of scope (§6).
- Avalonia 12 core packages are platform-neutral: a resource-only library needs no
  `-android`/`-ios` TFM and no mobile workload; a `net10.0-android` app project consumes `net10.0`
  assets as-is.
- Solution rules apply: CPM (no `Version=`), shared props never repeated, `.slnx` folders, LF,
  zero warnings with `AVLN*` read by hand, XML docs on public members.
- The Avalonia XAML compiler makes `.axaml` unreadable from the built assembly, which is why the
  Desktop tests embed the theme sources as text (`ThemeSource`).

## 3. Design

- **One palette source.** `src/Enigma.Avalonia.Desktop/Themes/Colors.axaml` and `Brushes.axaml`
  stay the single source of truth. The mobile csproj compiles them into its own assembly as **linked**
  `AvaloniaResource` items (`Link="Themes\Colors.axaml"` / `Themes\Brushes.axaml`), so both packages
  ship identical dictionaries and a palette edit is made once. Desktop's files, URIs and package
  graph do not change; their header comments gain one sentence saying they also compile into
  `Enigma.Avalonia.Mobile`.
  *Fallback, if the XAML compiler does not honour `Link`:* byte-identical copies under
  `src/Enigma.Avalonia.Mobile/Themes/`, guarded by the parity tests below (record the deviation).
- **Entry point** `avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml` — the mobile assembly's own
  `ResourceDictionary` merging `avares://Enigma.Avalonia.Mobile/Themes/Colors.axaml` then
  `Brushes.axaml`, and nothing else. Same file name and the same `ResourceInclude` usage as
  Desktop, so a consumer swaps only the assembly name.
- **Project** `src/Enigma.Avalonia.Mobile/Enigma.Avalonia.Mobile.csproj` — `net10.0`,
  `OutputType` Library, `GenerateDocumentationFile`, `AvaloniaResource` for its own `Themes\**` plus
  the two links; references `Avalonia` and `Avalonia.Themes.Fluent` only (no CommunityToolkit.Mvvm,
  no Enigma.Core — nothing in it needs them). No C# source.
- **Tests** `tests/Enigma.Avalonia.Mobile.UnitTests` — xunit.v3 + Avalonia.Headless.XUnit,
  `net10.0`, a headless `TestApp` merging FluentTheme and the **mobile** entry point (Dark pinned).
  The canonical Desktop `Colors.axaml` / `Brushes.axaml` are embedded as text (as the Desktop
  tests do) so the expectations are read from the source, never hand-copied.

## 4. PHASE01 — Project, linked palette & tests

**Status:** TODO

### Steps

1. Create `src/Enigma.Avalonia.Mobile/Enigma.Avalonia.Mobile.csproj` and
   `src/Enigma.Avalonia.Mobile/Themes/Fluent.axaml` per §3; add the project to
   `Enigma.Avalonia.slnx` under `/src/`.
2. Create `tests/Enigma.Avalonia.Mobile.UnitTests/` (csproj, `TestAppBuilder.cs`, a palette-source
   reader, tests); add it under `/tests/`.
3. Tests:
   - the entry point loads from the mobile assembly at its documented URI;
   - every canonical colour key resolves, in both variants, to the canonical literal value;
   - every canonical brush key (Enigma and FluentTheme override) resolves to a solid brush whose
     colour, in both variants, is the canonical value of the colour key it binds;
   - the mobile `Colors.axaml` / `Brushes.axaml` define exactly the canonical key sets (nothing
     missing, nothing extra), and the mobile `Fluent.axaml` merges exactly those two dictionaries;
   - a runtime variant switch re-colours a brush instance in place (the shared-brush contract).
4. One sentence in the header comment of Desktop's `Colors.axaml` and `Brushes.axaml`: the file is
   also compiled into `Enigma.Avalonia.Mobile`.
5. Documentation sweep: `CLAUDE.md` (architecture diagram, project count, the linked-palette rule).

### Acceptance criteria

- [ ] `dotnet build Enigma.Avalonia.slnx` (after clean) — 0 warnings including `AVLN*`, 0 errors.
- [ ] `dotnet test --solution Enigma.Avalonia.slnx` — the whole suite green, the new tests included.
- [ ] The mobile assembly carries no copy of the palette in source (linked), or the recorded fallback.
- [ ] No `PackageReference` with `Version=`; no shared property repeated in a csproj.
- [ ] Desktop's public surface and theme URIs unchanged (its existing tests pass untouched).

## 5. PHASE02 — Mobile guide & guides index

**Status:** TODO

### Steps

1. `docs/guides/mobile.md` (house guide shape: intro → what the package contains → key types/URIs
   → `###` usage scenarios → notes): installing next to an Avalonia mobile head; merging the entry
   point in `App.axaml`; painting a single-view `MainView` from the palette; following the OS
   dark/light setting and switching at runtime; sharing views between a desktop and a mobile head;
   retheming (pointer to the theming guide's recipe — same keys); what the package deliberately
   does not contain. The full key reference is **linked**, not duplicated: `theming.md` is the one
   table.
2. `docs/guides/README.md` — a *Mobile* group linking the new guide; the intro names both packages.
3. `docs/guides/theming.md` — one sentence that the same palette ships in `Enigma.Avalonia.Mobile`.
4. Snippet-verification gate on every code fence of `mobile.md` (record the coverage table).

### Acceptance criteria

- [ ] Guide written in the house shape; snippets verified against the real URIs, types and APIs.
- [ ] Index links it; `theming.md` names the mobile package.
- [ ] Build and the whole suite still green (nothing compiled changes).

## 6. PHASE03 — Packaging, packed README & runbook

**Status:** TODO

### Steps

1. Mobile csproj — a packaging `PropertyGroup` with the house properties: `PackageId`
   `Enigma.Avalonia.Mobile`, `Version` **1.3.0** (lockstep with Desktop), `Title`, `Description`,
   `PackageTags`, `PackageReadmeFile`, `PackageLicenseFile`, `RepositoryUrl`, `RepositoryType`,
   `PackageProjectUrl` (`GenerateDocumentationFile` landed in PHASE01; `PackageReleaseNotes` lands
   with the release notes in `FEATURE-D792`). Packing `ItemGroup`: its own `README.md` and the root
   `LICENSE.md`. No `GeneratePackageOnBuild`, no symbol properties.
2. `src/Enigma.Avalonia.Mobile/README.md` — the packed README (house template: title → two badges →
   intro → *Features* → *Installation* → *Quick start* → *Documentation* → *License*), links only
   to packed files, guides named in prose.
3. Root `README.md` (Desktop's packed README) — a short pointer to the mobile package.
4. `docs/RELEASE.md` — the runbook covers two packages released in lockstep under one bare tag:
   both `<Version>`s, pack both, inspect both (Mobile: `lib/net10.0/` only, dependencies `Avalonia`
   + `Avalonia.Themes.Fluent`), push both, verify both.
5. Third-party license audit (runtime dependencies: `Avalonia`, `Avalonia.Themes.Fluent` — MIT) in
   the completion doc.
6. Throwaway pack-verify of the mobile package into a scratch directory, then delete it.
7. Snippet-verification gate on the mobile README quick start.

### Acceptance criteria

- [ ] Pack-verify: `Enigma.Avalonia.Mobile.1.3.0.nupkg` only (no `.snupkg`), non-empty `README.md`,
      `LICENSE.md`, nuspec version/title/license/readme correct, one `net10.0` dependency group with
      `Avalonia` and `Avalonia.Themes.Fluent` at 12.1.4, `lib/net10.0/` with DLL and XML doc.
- [ ] `docs/RELEASE.md` describes both packages; nothing in it is run by the agent beyond pack-verify.
- [ ] Release build 0 warnings; whole suite green.

## 7. Out of scope

- Mobile versions of Desktop's controls and services (navigation shell, dialogs, info bar, editors,
  docking, ribbon) — a separate product decision.
- An Android (or iOS) sample/showcase head — it would put the android workload and SDK on every
  solution build; follow-up.
- Touch-specific sizing, density or typography — the draft asks for the same appearance.
- Release notes, `PackageReleaseNotes`, Desktop's version — `FEATURE-D792`.

## Decisions taken autonomously

| Question | Chosen | Why | Alternatives rejected |
|---|---|---|---|
| What "styles/colors/brushes" covers | `Colors.axaml` + `Brushes.axaml` (incl. the FluentTheme `TextBox`/`ComboBox` overrides) | That is the whole of Desktop's palette and standard-control styling; the rest is templates for Desktop-only controls | Porting the control templates (a mobile control library, not a theme) |
| How the palette is shared | Link Desktop's two files into the mobile assembly | One source, zero drift, no change to Desktop's URIs or package graph | Copies (drift, guarded only by tests); a third shared package both depend on (new package, moves Desktop's files and dependency graph in a minor release); Mobile depending on Desktop (drags desktop controls and BouncyCastle onto phones) |
| Target frameworks | `net10.0` | House set for new projects; a resource-only library needs no platform TFM; consumed as-is by `net10.0-android` / `-ios` heads; no workload needed to build the solution | `net8.0;net10.0` (nothing new targets net8.0, out of support 2026-11-10); `net10.0-android` (needs the workload everywhere, excludes iOS) |
| Entry point | `avares://Enigma.Avalonia.Mobile/Themes/Fluent.axaml` | Mirrors Desktop exactly | A differently named file |
| Package dependencies | `Avalonia`, `Avalonia.Themes.Fluent` | The only things the dictionaries need; FluentTheme keys are overridden | Carrying CommunityToolkit.Mvvm / Enigma.Core |
| Version | 1.3.0 in lockstep with Desktop | One bare tag per release covers both packages; the palette is shared, so versions mean the same thing | An independent 1.0.0 (needs per-package tags, a second version history for the same palette) |
| Test project | New `Enigma.Avalonia.Mobile.UnitTests` on the solution's xunit.v3 3.2.2, no coverage package | One test project per library; stay consistent with the existing test project | Extending the Desktop tests; xunit 4.x (its own migration item) |
| Test expectations | Read from the canonical source files embedded as text | Hand-copied lists drift; the Desktop tests already do this | Comparing against the Desktop assembly at runtime (couples the mobile tests to Desktop's dependency set) |
| Sample app | None in this item | Android head = workload + SDK on every build | An Android showcase now |
| Phase order | Library → guide → packaging/README | House first-release order: guides before the README, metadata before pack-verify | Packaging first |
