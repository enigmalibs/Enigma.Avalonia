# FEATURE-28E8 PHASE01 — Root configuration & solution file

**Status:** DONE
**Branch:** `feature/feature-28e8-phase01-root-config`
**Plan:** `docs/plan/FEATURE-28E8.md` §3

## Summary

Brought the repository from "one `docs/` directory" to a fully configured solution root: git hygiene,
the authoritative C# code-style file, solution-wide build defaults, the Central Package Management
manifest carrying all 12 dependencies, the SDK/test-runner pin, the MIT licence, the release-document
placeholders, and the `.slnx` declaring the three projects PHASE02 will create.

No code and no projects yet — by design. The `.slnx` intentionally references csproj files that do not
exist, so the solution does not build until PHASE02 (see *Build/test evidence*).

`Directory.Build.props` and `Directory.Packages.props` are the two files every later phase depends on
and neither should be edited casually: the props file's properties must never be repeated in a csproj,
and the Avalonia group in the packages file is version-coupled — bumping one member alone breaks the
build.

## Files/modules touched

### Created

| File | Bytes | Source |
|---|---|---|
| `.gitignore` | 6239 | `git-repo-hygiene/templates/gitignore`, verbatim + one appended LF (see deviations) |
| `.gitattributes` | 653 | `git-repo-hygiene/templates/gitattributes`, byte-identical |
| `.editorconfig` | 9465 | `dotnet-solution-config/templates/editorconfig`, byte-identical — the **full C# variant** (124 `dotnet_`/`csharp_` rules), not the minimal line-endings file |
| `Directory.Build.props` | 1243 | Template + `Authors`/`Copyright` filled; comment added recording that `TreatWarningsAsErrors` does **not** promote Avalonia's `AVLN*` diagnostics |
| `Directory.Packages.props` | 2969 | Written from plan §2.3 — 12 `PackageVersion` entries in 4 commented groups |
| `global.json` | 141 | SDK `10.0.100` + `rollForward: latestFeature` + `test.runner: Microsoft.Testing.Platform` |
| `LICENSE.md` | 1063 | `dotnet-solution-setup/templates/LICENSE.md`, MIT, 2026, Josué Clément |
| `RELEASENOTES.md` | 0 | Placeholder — filled by FEATURE-1702 PHASE02 |
| `Enigma.Avalonia.slnx` | 427 | `/src/`, `/samples/`, `/tests/` folders, one project each |

### Modified

- `docs/roadmap.md` — `FEATURE-28E8` and its `PHASE01` row to `IN PROGRESS`, then `PHASE01` to `DONE`.
- `docs/plan/FEATURE-28E8.md` — item status to `IN PROGRESS`; `PHASE01`/`PHASE02` headings carry an
  explicit status marker.

### Deliberately not touched

- `README.md` — see deviation 1.

### Deleted

None.

## Package inventory (verified against plan §2.3 — 12 entries, no extras)

| Group | Packages |
|---|---|
| Avalonia (coupled) | `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Headless.XUnit` — all **12.1.1**; `AvaloniaUI.DiagnosticsSupport` **2.2.3** |
| Control library | `CommunityToolkit.Mvvm` **8.4.2**, `Enigma.Core` **1.0.0** |
| Showcase only | `Enigma.Icons.Avalonia` **1.0.0**, `LiveChartsCore.SkiaSharpView.Avalonia` **2.1.0-dev-365**, `Microsoft.Extensions.Hosting` **10.0.10** |
| Tests | `xunit.v3` **3.2.2** |

Confirmed absent, as required: `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`,
`Avalonia.Diagnostics` (does not exist for Avalonia 12), `coverlet.collector` (not in §2.3),
`PhosphorIconsAvalonia`, `Enigma.Cryptography`, `Microsoft.Extensions.DependencyInjection`.

## Deviations & follow-ups

1. **`README.md` is not zero-byte — acceptance criterion partially unmet.** The plan calls for a
   0-byte placeholder; the file already existed as tracked content (`# Enigma.Avalonia`, 18 bytes)
   from the user's own `0496175 Initial commit`. `touch` does not truncate, and truncating
   user-authored committed content to satisfy a placeholder rule was not worth doing silently, so the
   line was left intact and raised with the user instead. `RELEASENOTES.md` **is** 0 bytes as
   specified. FEATURE-2802 PHASE02 rewrites `README.md` wholesale, so the practical impact is nil.
2. **`.gitignore` needed one appended newline.** The upstream `git-repo-hygiene` template ends without
   a trailing newline, which violates hard rule §2.4.7. Fixed here by appending a single LF
   (6238 → 6239 bytes); everything before that byte is byte-identical to the template.
   **Follow-up:** the template itself should be fixed, so the next repo bootstrapped from it doesn't
   need the same patch. The two sibling Enigma repos both carry the un-terminated 6238-byte copy.
3. **`git init -b main` was run by the agent, not the user.** The plan's §3 prerequisite assigns it to
   the user; the user explicitly chose that split when the pre-flight blocked. Both commits on `main`
   are the user's — no commit was made by the agent.
4. **Heads-up for PHASE02:** the plan's PHASE02 step 3 lists `ImplicitUsings` disable on the test
   csproj. `Directory.Build.props` now sets it solution-wide, so repeating it in any csproj would
   violate §2.4.2/§2.4.4 and the dotnet-solution-config convention. **PHASE02 must omit
   `ImplicitUsings`, `LangVersion`, `Nullable` and `TreatWarningsAsErrors` from all three csproj
   files.** (The sibling `Enigma.Icons` repo does set `ImplicitUsings` per-project — its
   `Directory.Build.props` does not carry it. Ours does; don't copy the sibling here.)
5. **`.slnx` folder order** is `/src/` → `/samples/` → `/tests/`, following plan §2.1. The sibling
   `Enigma.Icons.slnx` orders them `/src/` → `/tests/` → `/samples/`. Purely cosmetic; noted only so
   the difference isn't later mistaken for a mistake.
6. **Line endings: nothing to report.** `.gitattributes` (`* text=auto eol=lf`) lands with the first
   content commit, so this repository starts LF-clean and no `git add --renormalize` pass will ever be
   needed. A tree-wide scan found zero CRLF bytes.

## Build/test evidence

**There was nothing to build and nothing to test in this phase**, and that is the planned outcome — the
three csproj files arrive in PHASE02. Definition-of-Done criteria 1–2 are satisfied by the applicable
equivalent (well-formed artifacts, criteria verified by inspection and dry run):

| Check | Result |
|---|---|
| `dotnet build Enigma.Avalonia.slnx` | Fails as designed: **3 × MSB3202** "project file was not found", one per declared project — and **0 Warning(s)**. This is the useful part: the `.slnx` itself parses and resolves, and nothing other than the intentionally-absent projects is wrong. |
| XML well-formedness | `Directory.Build.props`, `Directory.Packages.props`, `Enigma.Avalonia.slnx` — all parse (`xml.dom.minidom`) |
| JSON validity | `global.json` parses; both `sdk` and `test` keys present |
| SDK pin resolves | Installed SDK **10.0.103** satisfies the `10.0.100` pin via `rollForward: latestFeature` |
| Template fidelity | `.gitattributes` and `.editorconfig` byte-identical to their templates; `.gitignore` identical for its first 6238 bytes |
| CPM inventory | 12 `PackageVersion` entries, exactly matching plan §2.3, with the forbidden packages confirmed absent |
| Encoding & newlines | Every text file LF-terminated with a final newline; zero CRLF bytes anywhere outside `.git/` |
| Test suite | None exists yet (created in PHASE02) — nothing to run |

## Acceptance criteria

| Criterion (plan §3) | Met |
|---|---|
| All eight artifacts exist at the solution root with the exact names | **Yes** (10 files across the 8 steps) |
| Every file is LF with a final newline | **Yes** (after deviation 2's one-byte fix) |
| `Directory.Packages.props` lists every package from §2.3 and no others; versions match | **Yes** |
| `README.md` and `RELEASENOTES.md` are zero-byte | **Partially** — `RELEASENOTES.md` yes; `README.md` see deviation 1 |
| No build expected to succeed; state so explicitly and verify config by inspection | **Yes** — stated above, with the 0-warning MSB3202 result as evidence |
