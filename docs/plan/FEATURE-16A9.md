# FEATURE-16A9 — `enigma-avalonia-desktop` house skill

**Status:** TODO · single phase
**Branch:** `feature/feature-16a9-house-skill`
**Depends on:** FEATURE-1702 (the published API and its documented surface)

## 1. Objective

Author a house skill at `~/.claude/skills/enigma-avalonia-desktop/` so that agents working in *other*
solutions (Enigma.Msi, Draw, …) can adopt and use `Enigma.Avalonia.Desktop` correctly. Without it,
the only agent-facing documentation of this control family describes a package this solution replaced —
wrong package id, wrong resource keys, and two control families that no longer exist.

## 2. Scope note — the target is outside this repository

Nothing in this item lands under `/home/jo/Dev/Enigma.Avalonia/` except its roadmap status and its
completion doc. The build flow still applies: branch, implement, update `docs/roadmap.md`, write
`docs/done/FEATURE-16A9.md`, print a commit message. The commit will therefore touch only
`docs/roadmap.md` and `docs/done/FEATURE-16A9.md` — say so explicitly, and state in the completion
doc which files were written outside the repo and that they are **not** covered by this repo's git
history.

## 3. Reference source

```
~/.claude/skills/carbon-avalonia-desktop/          SKILL.md + reference/ (12 files)
```

Port it, then correct it against the real shipped API (`src/`) and the guides
(`docs/guides/`, FEATURE-2802). Where the port source's skill and this solution's code disagree, **the
code wins**.

## 4. Deliverables — `~/.claude/skills/enigma-avalonia-desktop/`

- `SKILL.md` — name, description (trigger phrasing that matches how the library is actually asked
  for), the control/service split, the install block, the 5-step bootstrap summary, the quick-reference
  table of controls → XAML `xmlns` → purpose → reference file, cross-cutting conventions, "also use it
  when", and "when NOT to use".
- `reference/` — **10 files**, i.e. the source's 12 minus `calendar-schedule.md` and `displayer2d.md`:
  `setup.md`, `navigation.md`, `docking.md`, `ribbon.md`, `editors.md`,
  `dialogs-overlay-infobar.md`, `settings-cards.md`, `file-folder-dialogs.md`,
  `data-collectionview.md`, `theming.md`.

## 5. Required corrections during the port

1. **Package identity:** `Carbon.Avalonia.Desktop` → `Enigma.Avalonia.Desktop`, version `1.0.0`,
   repo `enigmalibs/Enigma.Avalonia`.
2. **Namespaces:** every `using:Carbon.Avalonia.Desktop.*` XAML declaration and every C# `using`.
3. **Resource keys:** every `Carbon*` brush/colour key → `Enigma*`.
4. **Theme URI:** `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml`, merged as a
   **`ResourceInclude`** in `Application.Resources` (not a `StyleInclude` — it has a
   `ResourceDictionary` root).
5. **Avalonia version:** 12.1.1 throughout the install and coupled-set guidance.
6. **Transitive dependencies:** `Avalonia`, `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm`,
   `Enigma.Core` (which brings `BouncyCastle.Cryptography`) — **not** `PhosphorIconsAvalonia`,
   **not** `Microsoft.Extensions.DependencyInjection`, **not** `Enigma.Cryptography`.
7. **Icons:** replace the `PhosphorIconsAvalonia` guidance with `Enigma.Icons.Avalonia` —
   `xmlns:ei="https://github.com/josueclement/Enigma.Icons"`, `<ei:Icon Kind="Gear"/>` for anything
   themed/bound, `{ei:IconGeometry Gear}` only for static path data, and
   `PhosphorIconSet.Instance.GetGlyph(PhosphorIcon.Gear, PhosphorWeight.Regular).ToGeometry()` from
   C#. Keep the reminder that any `Geometry` works, since the controls take `Geometry`, not an icon
   type. Note that `Enigma.Icons.Avalonia` is **not** a dependency of this library — a consumer adds
   it themselves if they want it.
8. **Remove every trace** of `CalendarSchedule` and `Displayer2D` from `SKILL.md`'s table, the
   feature lists, and any cross-reference in the remaining reference files.
9. **Cross-check every code snippet** against `src/` and the guides — same verification gate as
   FEATURE-2802 PHASE01. Record the coverage table in the completion doc.
10. Document the four gotchas an agent will otherwise hit: the `Enigma.Avalonia.*` ↔ `Avalonia.*`
    namespace collision, `x:DataType`/`AVLN2100`, `{TemplateBinding}` being one-way only, and hit
    testing needing a non-null `Background`.
11. **Do not carry the port source's name into the new skill** — it documents this library on its own
    terms.

## 6. Retiring the old skill

Ask the user before touching `~/.claude/skills/carbon-avalonia-desktop/`: delete it, leave it as-is,
or narrow its description so it stops matching new work. Do not decide this unilaterally — other
projects on disk may still reference the old package.

## 7. Acceptance criteria

- `~/.claude/skills/enigma-avalonia-desktop/SKILL.md` plus 10 `reference/*.md` files exist and are
  internally consistent.
- `grep -ri carbon ~/.claude/skills/enigma-avalonia-desktop/` returns nothing.
- No snippet references a type, member, key or package that does not exist in the shipped 1.0.0.
- The quick-reference table lists every shipped control family and no removed one.
- The old skill's fate has been decided by the user and recorded.
- Nothing to build or test — state that, and state that the acceptance criteria were verified by
  inspection against `src/` and `docs/guides/`.
