# FEATURE-2802 PHASE02 — Packed README, SECURITY.md, CLAUDE.md

**Branch:** `feature/feature-2802-phase02-readme-community`
**Plan:** `docs/plan/FEATURE-2802.md` §4

## 1. Summary

Wrote the three remaining 1.0.0 documentation artifacts and closed the repo-wide clean-slate
criterion that PHASE01 flagged as unsatisfiable.

- **`README.md`** — the packed nuget.org landing page. Replaces the one-line placeholder from
  FEATURE-28E8 PHASE01. Title → two badges → intro → (what's-new slot) → *Features* →
  *Installation* → *Quick start* → *Documentation* → *License*, per the plan's section order and
  `dotnet-release/templates/package-README.md`.
- **`SECURITY.md`** — supported versions, GitHub private vulnerability reporting, what to expect,
  and a *Scope* section that names two boundaries: `Enigma.Core`/BouncyCastle for the encoding the
  Base64 and hexadecimal editors reach, and Avalonia for rendering, input and the storage provider.
- **`CLAUDE.md`** — the internal agent guide (never packed): the four commands, the three-project
  architecture and its dependency direction, the solution rules, and the ten gotchas from
  FEATURE-28E8 §2.4 written out with the reasons they cost time.
- **`ResourceKeyTests.cs`** — the two port-source-named guards replaced by two equivalent
  prefix guards, on the user's decision (see §4).

## 2. Files touched

| File | Change |
|---|---|
| `README.md` | Modified — 1-line placeholder → full packed README (189 lines) |
| `SECURITY.md` | Created |
| `CLAUDE.md` | Created |
| `tests/Enigma.Avalonia.Desktop.UnitTests/Themes/ResourceKeyTests.cs` | Modified — two guards reformulated, `AvaloniaOverrideKeys` allowlist + `IsUnexpected` helper added, class doc de-historicised |
| `docs/roadmap.md` | PHASE02 and the `FEATURE-2802` item row → `DONE` |
| `docs/plan/FEATURE-2802.md` | Item status and §4 heading → `DONE` |
| `docs/done/FEATURE-2802-PHASE02.md` | Created (this file) |

Nothing under `src/` or `samples/` was touched.

## 3. Snippet verification

Both README code languages were verified by **compilation against the real library**, not by
reading — two throwaway projects in the session scratchpad, each with a `ProjectReference` to
`src/Enigma.Avalonia.Desktop`.

| Snippet | Language | Symbols exercised | Mismatches |
|---|---|---|---|
| Step 1 — theme merge | XAML | `ResourceInclude`, `avares://Enigma.Avalonia.Desktop/Themes/Fluent.axaml` | 0 |
| Step 2 — DI registration | C# | 6 interfaces + 6 implementations, `AddSingleton` | 0 |
| Step 3 — host placement | XAML | 3 `xmlns` declarations, `ContentDialog`, `Overlay`, `InfoBar`, `EnigmaBackgroundBrush` | 0 |
| Step 4 — startup wiring | C# | `RegisterHost` ×3, `SetStorageProvider` ×2, `Window.StorageProvider` | 0 |
| Step 5 — ViewModel | C# | `ObservableObject`, `AsyncRelayCommand`, `IContentDialogService.ShowAsync`, `ContentDialog.Title`/`PrimaryButtonText`/`CloseButtonText`, `DialogResult.Primary` | 0 |
| **Totals** | | **5 snippets · 34 symbols** | **0** |

Both harness projects built with **0 warnings, 0 errors**. The XAML harness was checked on warning
count, not exit code (FEATURE-28E8 §2.4.1) — Avalonia's XAML compiler reported no `AVLN*`
diagnostics.

Feature-list claims were separately checked against `src/`, which corrected two drafted statements
before they shipped: `NavigationView` has no collapsible-pane property (it has `PaneSize`;
`LabelMaxWidth` is on `NavigationItem`), and `InfoBarSeverity` has four members, not three.

A third correction came out of the documentation freshness sweep rather than this pass, and is worth
recording as a method note: reading only the declared members of `BaseEditor.cs` suggested the
editors have no watermark, and the *Features* bullet was drafted without one. `BaseEditor` derives
from `TextBox`, so `PlaceholderText` is inherited and bound by the template — `editors.md:74` already
documented it. **Grepping a control's own file is not the same as reading its surface**; the base
type has to be followed. The bullet now names the watermark.

## 4. The clean-slate criterion — resolved

PHASE01's completion doc §6 recorded that criterion 4 (`grep -ril carbon . --exclude-dir=docs/plan
--exclude-dir=.git` returns nothing) could not be met as written. Both surviving sites were put to
the user, who decided:

**Test guards — reformulated (not deleted, not left alone).** `NoControlTemplate_ReferencesA…Key`
and `NoThemeDictionary_DefinesA…Key` asserted the *absence* of an old prefix, so the old prefix had
to appear as a string literal. They are replaced by the positive form, which carries no such
literal and is strictly stronger — it catches *any* foreign prefix, not one named one:

- `EveryKeyAControlTemplateReferences_IsEnigmaPrefixed` — all 23 keys the 19 control templates
  reference must be `Enigma*`.
- `EveryThemeDictionaryKey_IsEnigmaPrefixedOrAKnownAvaloniaOverride` — every key in `Brushes.axaml`
  (49) and both `Colors.axaml` variants (29 each) must be `Enigma*` or listed in the new
  `AvaloniaOverrideKeys` allowlist.

The allowlist holds the 23 standard Avalonia keys `Brushes.axaml` deliberately redefines
(10 `ComboBox*`, 13 `TextControl*`) so the built-in `TextBox` and `ComboBox` match the library's
controls. It is explicit rather than a `ComboBox*`/`TextControl*` wildcard so that widening it is a
visible decision.

Both replacements were **negative-tested**: injecting a stray `LegacySurfaceBrush` key into
`Brushes.axaml` failed the dictionary guard, and repointing one `SettingsCard.axaml` reference at a
`Legacy*` key failed the reference guard. Both probes were reverted and the suite re-run green.

**Completion records — left as they are, criterion narrowed.** The nine `docs/done/*.md` hits are
historical records, and eight of them are hits only because they *quote the verification command*.
Rewriting them to hide how verification was performed damages the record for no gain, so
`docs/done/` is treated like `docs/plan/`: an internal workflow artifact, neither shipped nor
user-facing, and outside the clean-slate rule. The criterion in force is therefore:

```
git grep -il carbon -- . ':!docs/plan' ':!docs/done'    →    (none)
```

which passes. One genuine lineage aside remains at `docs/done/FEATURE-57C8-PHASE03.md:34`; it is
inside the narrowed exclusion and was left untouched.

## 5. Acceptance criteria

| Criterion | Status |
|---|---|
| `README.md` renders as markdown, non-empty, follows the section order | Met — 189 lines; `#` headings in the planned order, what's-new slot marked by an HTML comment for FEATURE-1702 PHASE02 |
| Exactly the two badges, in order | Met — NuGet then License: MIT; no downloads badge |
| No `docs/` link and no absolute GitHub URL | Met — the only markdown links are the two badges and `LICENSE.md`; `docs/guides/` appears in prose only |
| States the BouncyCastle consequence | Met — *Installation*, with the two controls that cause it named |
| Every quick-start snippet passes the verification gate | Met — §3, 5 snippets, 0 mismatches, verified by compile |
| `SECURITY.md` and `CLAUDE.md` exist at the root | Met |
| Zero port-source occurrences outside `docs/plan/` | Met under the narrowed criterion — §4 |
| Solution builds and tests green | Met — see below |

**Definition of Done criteria 1–2.** `dotnet build Enigma.Avalonia.slnx` → **0 warnings, 0 errors**
(library for `net8.0` and `net10.0`, showcase, tests). `dotnet test --solution Enigma.Avalonia.slnx`
→ **281 passed, 0 failed, 0 skipped**. The count is unchanged from PHASE01 because this phase
replaced two tests with two tests; both new ones were negative-tested (§4).

## 6. Deviations & follow-ups

### Deviations from the plan

1. **The plan's *Installation* wording is self-contradictory on `Avalonia.Themes.Fluent`.** §4 says
   consumers "add `Avalonia.Desktop`, `Avalonia.Themes.Fluent` and `Avalonia.Fonts.Inter`
   themselves", then lists `Avalonia.Themes.Fluent` among what *is* transitive. The library's csproj
   references it, so it **is** transitive. The README states the accurate version: `Avalonia`,
   `Avalonia.Themes.Fluent`, `CommunityToolkit.Mvvm` and `Enigma.Core` come transitively;
   `Avalonia.Desktop` and `Avalonia.Fonts.Inter` do not.
2. **"~27 MB" would have been wrong.** The plan's parenthetical describes the extracted
   global-packages folder for all TFM assets. What a consumer actually ships is one
   `BouncyCastle.Cryptography` assembly of **~4.7 MB** (the nupkg is 7.9 MB). The README states
   ~4.7 MB, which is the number that matters and still makes the plan's point.
3. **The what's-new callout is a slot, not text.** The plan assigns it to FEATURE-1702 PHASE02, so
   the README carries a one-line HTML comment where it goes rather than a placeholder heading —
   invisible when rendered, unambiguous for the phase that fills it.
4. **`SECURITY.md` uses one absolute GitHub URL** (the `Enigma.Core` repository, taken from that
   package's own nuspec `projectUrl`). The no-absolute-URL rule is a correctness rule for the
   *packed* README rendered on nuget.org; `SECURITY.md` is not packed and is rendered on GitHub,
   where the link works.
5. **The `ResourceKeyTests` class summary was reworded** from "The rename guard … the theme-key
   rename" to "The resource-key guard … any theme-key edit". No behaviour change; it removes a
   historical framing that no longer described what the class asserts.

### Follow-ups

1. **`docs/RELEASE.md` does not exist yet.** `CLAUDE.md` gotcha 9 refers to it as the release
   runbook; FEATURE-1702 PHASE03 creates it. The reference is deliberate and forward-looking, not
   stale — but it is a dangling pointer until that phase lands.
2. **`SECURITY.md`'s supported-versions table says `1.0.x`** before 1.0.0 is published. FEATURE-1702
   should confirm the row matches the version actually released.
3. **GitHub private vulnerability reporting must be enabled** in the repository's
   Settings → Security, or `SECURITY.md` step 2 points at a control the reporter cannot use. This is
   a repository-settings action for the user, not a code change.
4. **The behavioural findings from PHASE01 §6.3 are still open** (`DefaultButton` has no effect,
   `ContentDialogService.ResetDialog` leaks size properties, `InfoBar.Content` is invisible,
   `INavigationViewModel`'s stale XML doc). None is a documentation problem, and none was touched
   here; they remain candidates for a `BUG-*` item.
5. **Line endings:** no CRLF was observed in any file this phase created or modified — all LF with a
   final newline. No action taken or recommended.
