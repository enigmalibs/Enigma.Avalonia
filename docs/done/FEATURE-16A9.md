# FEATURE-16A9 — `enigma-avalonia-desktop` house skill

**Completed:** 2026-08-03 · single phase
**Branch:** `feature/feature-16a9-house-skill`

## Summary

Authored the house skill that teaches an agent working in *another* solution (Enigma.Msi, Draw, …) how
to adopt and use `Enigma.Avalonia.Desktop` 1.0.0. It replaces a retired skill that documented the
package this solution superseded — wrong package id, wrong resource keys, and two control families that
no longer exist.

The skill is `SKILL.md` plus **10** `reference/*.md` files. It was ported from the retired
`carbon-avalonia-desktop` skill and then corrected against the shipped API in `src/` and the guides in
`docs/guides/`; where the port source and this solution's code disagreed, the code won.

## Files touched

### In this repository (2 modified, 1 created)

- **Modified** `docs/roadmap.md` — `FEATURE-16A9` status `TODO` → `DONE`.
- **Modified** `docs/plan/FEATURE-16A9.md` — status line `TODO` → `DONE`.
- **Created** `docs/done/FEATURE-16A9.md` — this file.

No source, project, theme or test file was touched. The library, showcase and test projects are
byte-identical to their state at branch point.

### Outside this repository — **not covered by this repo's git history**

Real files, written to the dotfiles repository:

```
~/dotfiles2/.claude/skills/enigma-avalonia-desktop/
  SKILL.md
  reference/setup.md
  reference/theming.md
  reference/navigation.md
  reference/ribbon.md
  reference/docking.md
  reference/editors.md
  reference/dialogs-overlay-infobar.md
  reference/settings-cards.md
  reference/file-folder-dialogs.md
  reference/data-collectionview.md
```

Plus one symlink activating it for Claude Code:

```
~/.claude/skills/enigma-avalonia-desktop -> ../../dotfiles2/.claude/skills/enigma-avalonia-desktop
```

**These 11 files live in `~/dotfiles2` (a separate git repository, clean on `master` at branch point)
and need their own commit there.** The commit in *this* repository touches only the three `docs/` files
listed above.

## Deviations & follow-ups

### Deviations from the plan

1. **Write target moved from `~/.claude/skills/` to `~/dotfiles2/.claude/skills/` + symlink.**
   Plan §4 names `~/.claude/skills/enigma-avalonia-desktop/` as the deliverable path. That directory
   holds *only* symlinks into `~/dotfiles2/.claude/skills/` — all 13 pre-existing skills, including the
   retired port source, are stored that way. Writing a real directory there would have produced the only
   untracked, machine-local skill on the box. Confirmed with the user before writing; the literal plan
   path is now the symlink, so the skill loads exactly as §4 intended.

2. **The port source no longer existed and had to be recovered.**
   Plan §3 points at `~/.claude/skills/carbon-avalonia-desktop/`. It was deleted in `dotfiles2` commit
   `22f3ca4` — *"chore(skills): drop carbon-avalonia-desktop and phosphor-icons-avalonia"*. All 13 source
   files (SKILL.md + 12 reference) were recovered read-only from `22f3ca4^` and used as the port base.
   Nothing in `dotfiles2` was modified to obtain them.

3. **Plan §6 — "retiring the old skill" — was already settled, so the user was not asked.**
   §6 requires a user decision on the old skill's fate: delete, leave, or narrow its description. It is
   already gone from both `~/.claude/skills/` (no symlink) and the `dotfiles2` source tree (commit
   `22f3ca4` above). There was nothing left to delete, keep or narrow. Recorded here as the outcome:
   **already retired, by prior deliberate commit.** No action taken.

### Corrections applied beyond the plan's §5 list

Cross-checking every snippet against `src/` surfaced eleven facts the port source stated wrongly or
omitted. All are now correct in the skill:

- `NavigationView.PaneSize` (`double`, default `90`) and `NavigationItem.LabelMaxWidth` (`double`,
  default `72`) — both absent from the port source.
- `ContentDialog`'s six size properties (`DialogWidth/Height`, `DialogMin/MaxWidth`,
  `DialogMin/MaxHeight`), plus `IsOpen`, `OverlayBrush`, `DialogResult` and the `Closed` event. The
  size properties are **not** reset between dialogs — documented.
- `DockPane`'s content property is `PaneContent`, not `Content`; `DockTabGroup.Panes` is **not** a
  content property.
- `ContentDialog`, `Overlay` and `InfoBar` derive from `ContentControl`, not `TemplatedControl`.
- No `Enigma*` brush exists for the three input-background colours — they are colour keys only, feeding
  the FluentTheme overrides. The port source listed three non-existent `Input*` brushes.
- Exact counts: **29** colour keys, **26** brushes, **13** framework `TextBox` keys and **10**
  `ComboBox` keys.
- Editors: **thirteen** ready-to-use controls; `ByteArrayEditor` and `BaseEditor<T>` are abstract, and
  `ByteArrayEditor.Value` is registered *without* data validation. The port source omitted
  `ByteArrayEditor` entirely.
- The library ships **no** `AddEnigmaServices()` — the consumer writes it. The port source implied the
  package provides it.
- `CollectionView` sharp edges the port source omitted wholesale: a fresh view is empty until its first
  `Refresh()`; a filter change never self-refreshes; only the **first** group description is applied;
  and a `CollectionViewSource.Filter` handler must be attached **before** `Source` is assigned.
  `PropertyGroupDescription.ValueConverter` and the full refresh-trigger matrix are now documented.
- Navigation's semaphore uses a **zero** timeout, so a concurrent navigation is dropped rather than
  queued; and nothing on `INavigationService` throws — failures surface on `NavigationFailed` with a
  `Phase` string.
- Ribbon buttons execute on pointer **press**, not release; `RibbonToggleButton` writes `IsChecked`
  before invoking `Command`; invoking a menu item does not close the drop-down.

### Follow-ups

- **Editor count is inconsistent in the shipped release copy.** `docs/guides/editors.md` says "thirteen
  ready-to-use input controls" (correct — `ByteArrayEditor` is abstract), while the packed `README.md`
  and `docs/guides/README.md` both say "fourteen typed editors", and the csproj `<Description>` and
  `<PackageReleaseNotes>` say "fourteen typed editors" too. The skill follows the per-family guide and
  says thirteen. Worth reconciling in the next release pass — it is package-visible copy, so it is out
  of scope for this dev.
- **Line endings:** no CRLF inconsistency observed in any file touched by this dev; all 11 new files are
  LF with a final newline. No action taken or recommended.

## Build/test evidence

**Nothing to build or test.** This dev adds no code and changes no project, source, theme or test file
in this repository — its product is 11 markdown files in a different repository, plus three `docs/`
status/record files here. Definition-of-Done criteria 1 and 2 are met by the applicable equivalent, and
the acceptance criteria were verified by inspection against `src/` and `docs/guides/`:

| Acceptance criterion (plan §7) | How it was verified | Result |
|---|---|---|
| `SKILL.md` + 10 `reference/*.md` exist and are internally consistent | File inventory; every `reference` filename cited in `SKILL.md`'s quick-reference table resolves to a shipped file | **Pass** — 1 + 10 files |
| `grep -ri carbon ~/.claude/skills/enigma-avalonia-desktop/` returns nothing | Ran verbatim | **Pass** — no matches |
| No snippet references a type, member, key or package absent from 1.0.0 | See coverage table below | **Pass** |
| The quick-reference table lists every shipped control family and no removed one | Table rows diffed against `Themes/Fluent.axaml`'s `ResourceInclude` set and the `Controls/` tree | **Pass** — 10 rows, all shipped |
| The old skill's fate decided and recorded | Already retired by `dotfiles2` commit `22f3ca4`; recorded above | **Pass** |
| State that there was nothing to build or test | This section | **Pass** |

### Verification coverage

| What was checked | Method | Result |
|---|---|---|
| 68 library type names (controls, enums, services, data types, extension classes) | Each matched against a `class`/`interface`/`enum` declaration in `src/` | 68/68 exist |
| 21 spot-checked members (`PaneSize`, `LabelMaxWidth`, `PaneContent`, `DialogMinWidth`, `DialogMaxHeight`, `IsDropDownOpen`, `SelectAllTextOnFocus`, `NullWhenEmpty`, `FormatString`, `ValueConverter`, `DeferRefresh`, `SourceCollection`, `IsEmpty`, `PaneDragStarted`, `PaneCloseRequested`, `ClosePane`, `SetRootLayout`, `SetStorageProvider`, `RegisterHost`, `PageFactory`, `NavigationFailed`) | Grepped `src/**/*.cs` | 21/21 exist |
| Every `Enigma*Brush` key referenced | Diffed against `x:Key`s in `Themes/Brushes.axaml` | All exist. One deliberate negative: `EnigmaInputBackgroundBrush` appears only in a "this key does not exist" warning |
| Every `Enigma*Color` key referenced | Diffed against `x:Key`s in `Themes/Colors.axaml` | All exist |
| Every `TextControl*` / `ComboBox*` override key referenced | Diffed against `Themes/Brushes.axaml` | All exist |
| Removed families absent | Grepped for `CalendarSchedule`, `Displayer2D` | No matches |
| Wrong dependencies absent | Grepped for `PhosphorIconsAvalonia`, `Enigma.Cryptography` | No matches |
| Package/version facts | `Enigma.Avalonia.Desktop` `1.0.0`, `net8.0;net10.0`, repo `enigmalibs/Enigma.Avalonia`, Avalonia `12.1.1` | Match the csproj and `Directory.Packages.props` |
| Transitive dependency set | `Avalonia`, `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm`, `Enigma.Core` (→ `BouncyCastle.Cryptography`) | Match the csproj `PackageReference` set |
| `Enigma.Icons.Avalonia` guidance | 12 `PhosphorIcon` members, `PhosphorWeight.Regular`, the positional `IconGeometryExtension(PhosphorIcon)` ctor, and `Icon`'s `Kind`/`Weight`/`Size` checked against `~/Dev/Enigma.Icons` | All exist; `xmlns` URI matches the `XmlnsDefinition` attributes |
| Reference wiring | `setup.md` matches `samples/…/App.axaml.cs` and `ServiceCollectionExtensions.cs` | Faithful |
| File hygiene | All 11 files LF, final newline present | Clean |

A confirmatory `dotnet build Enigma.Avalonia.slnx` was run to show the tree is unchanged from its state
at branch point; it was not required by this dev, since no compilable file was modified.
