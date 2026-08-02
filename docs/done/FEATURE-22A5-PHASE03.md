# FEATURE-22A5 PHASE03 — Editors (16 controls, Enigma.Core encoding)

**Branch:** `feature/feature-22a5-phase03-editors`
**Status:** DONE

## Summary

Added the first control family to `Enigma.Avalonia.Desktop`: the 16-class `Controls/Editors/`
namespace and its two `ControlTheme` dictionaries, now merged into `Themes/Fluent.axaml`.

The family is a three-level hierarchy on top of Avalonia's `TextBox`:

| Level | Type(s) | Adds |
|---|---|---|
| 1 | `BaseEditor : TextBox` | `Title`, `Unit`, `LeadingContent`, `ActionContent`, `HasValidationError`, `ValidationErrorMessage`, `SelectAllTextOnFocus` (default `true`), the `:error` pseudo-class, and suppression of Avalonia's own `DataValidationErrors` adorner |
| 2 | `TextEditor` | nothing — a named single-line editor |
| 2 | `MultiLineTextEditor` | `AcceptsReturn`, `TextWrapping.Wrap`, `SelectAllTextOnFocus` off |
| 2 | `BaseEditor<T> : BaseEditor where T : struct` | typed `Value`, `FormatString`, `NullWhenEmpty`; text↔value sync with a re-entrancy guard, and reformat-on-lost-focus |
| 3 | `IntEditor`, `ShortEditor`, `LongEditor`, `UIntEditor`, `UShortEditor`, `ULongEditor`, `SingleEditor`, `DoubleEditor`, `DecimalEditor` | invariant-culture `TryParse`/`FormatValue` per numeric type |
| 3 | `ByteArrayEditor : MultiLineTextEditor` | `byte[]? Value` and byte-array↔encoded-text sync |
| 4 | `Base64Editor`, `HexadecimalEditor` | the concrete encodings, via `Enigma.Core.Encoding` |

Two behaviours are worth naming because they look like oversights and are not:

- `ByteArrayEditor.StyleKeyOverride` returns `typeof(MultiLineTextEditor)`, not its own type. That is
  deliberate — the byte-array editors deliberately reuse the multi-line template rather than shipping
  one of their own, which is why this phase has 16 classes but only 2 dictionaries.
- `ByteArrayEditor` writes its validation state directly (`HasValidationError` /
  `ValidationErrorMessage`) instead of going through `BaseEditor.SetParseError`, so the
  `"Invalid value"` literal in `CommitValue()` is a plain default a consumer can overwrite. Per the
  plan it stays as-is; no localisation infrastructure is implied.

## Files/modules touched

**Created** — 16 files under `src/Enigma.Avalonia.Desktop/Controls/Editors/`:

| File | Lines |
|---|---|
| `BaseEditor.cs` | 196 |
| `BaseEditorOfT.cs` | 207 |
| `ByteArrayEditor.cs` | 166 |
| `DecimalEditor.cs` | 36 |
| `DoubleEditor.cs` | 36 |
| `SingleEditor.cs` | 36 |
| `IntEditor.cs` | 35 |
| `ShortEditor.cs` | 35 |
| `LongEditor.cs` | 35 |
| `UIntEditor.cs` | 35 |
| `UShortEditor.cs` | 35 |
| `ULongEditor.cs` | 35 |
| `Base64Editor.cs` | 47 |
| `HexadecimalEditor.cs` | 47 |
| `MultiLineTextEditor.cs` | 27 |
| `TextEditor.cs` | 9 |

**Created** — 2 files under `src/Enigma.Avalonia.Desktop/Themes/Controls/Editors/`:
`BaseEditor.axaml` (157 lines), `MultiLineTextEditor.axaml` (150 lines).

**Modified:** `src/Enigma.Avalonia.Desktop/Themes/Fluent.axaml` — the two `ResourceInclude` lines,
plus a one-clause fix to the header comment, which claimed the two foundation dictionaries were "all
there is for now".

**Modified:** `docs/roadmap.md`, `docs/plan/FEATURE-22A5.md` (PHASE03 `TODO` → `IN PROGRESS` → `DONE`).

**No csproj change was needed.** `.cs` files are picked up by the SDK's default glob and the two
`.axaml` files by the existing `<AvaloniaResource Include="Themes\**" />`, which is recursive. The
library's four `PackageReference`s are unchanged — `Enigma.Core` was already referenced (plan §2.3),
and this is the first phase to actually consume it.

## Deviations & follow-ups

Five transformations were applied beyond a pure namespace rename. The complete `diff` of every
ported `.cs` file against its source consists of exactly these and nothing else — every declaration,
default value, `#pragma`, comment and blank line is otherwise byte-identical.

1. **§3.5 encoding namespace.** `Base64Editor.cs` and `HexadecimalEditor.cs`: the `using` now names
   `Enigma.Core.Encoding`. Both `Base64Service` and `HexService` keep their type names and their
   `string Encode(byte[])` / `byte[] Decode(string)` members, verified against the package's XML
   documentation before porting, so nothing else in either file changed — the `private static
   readonly` instance and the swallow-and-return-`false` `TryParse` are untouched.
2. **§3.6 `global::` removed — all 13 occurrences in this phase.** Two were in code
   (`ByteArrayEditor`'s `global::Avalonia.Data.BindingMode.TwoWay`, `MultiLineTextEditor`'s
   `global::Avalonia.Media.TextWrapping.Wrap`) and eleven in `<see cref>` targets. Each became a
   file-scoped `using` above the `namespace`, and **no `global::` remains under `Controls/`**. Two
   points worth recording for PHASE04–08, which face the same pattern:
   - `TextWrapping = TextWrapping.Wrap;` compiles because of C#'s *Color Color* rule — the inherited
     `TextBox.TextWrapping` property has the same name as its own enum type, so the qualifier is
     unambiguous. It is not a latent ambiguity.
   - A `<see cref="TextBox.Text"/>` binds its qualifier as a *type*, never as the inherited property,
     so the crefs resolve with only `using Avalonia.Controls;` added. That using is otherwise unused
     in `BaseEditorOfT.cs` and `ByteArrayEditor.cs`, which is harmless: `.editorconfig` pins IDE0005
     to `suggestion`, so it is not promoted by `TreatWarningsAsErrors`.
   That leaves **15** of the original 29 §3.6 occurrences for PHASE04–08 (PHASE02 cleared 1, this
   phase 13).
3. **§2.4.2 explicit `using` directives.** `ImplicitUsings` is `disable` solution-wide, so
   `using System;` was added to the three files that name `Type`, `Exception` or `AggregateException`
   — `BaseEditor.cs`, `ByteArrayEditor.cs`, `MultiLineTextEditor.cs`. `BaseEditorOfT.cs` needs none:
   it only uses keyword aliases. Ordering follows `dotnet_sort_system_directives_first = true` with
   no group separator.
4. **§3.7 clean-slate sweep.** One prose hit: `BaseEditor`'s class summary read "Base class for all
   Carbon editor controls" and now reads "Enigma".
5. **`.axaml` reindented from 4-space to 2-space.** The port source indents these two templates with
   4 spaces; `.editorconfig` sets `indent_size = 2` for `[*.{xml,axaml,xaml}]`, and PHASE01 already
   reindented `Colors.axaml`/`Brushes.axaml` on the same grounds. Both files were verified
   token-identical to their source after whitespace normalisation, so this is presentation only —
   no element, attribute, `PART_` name, selector or resource key differs, and both files keep their
   original line count; only the leading whitespace and the continuation-line alignment columns
   changed.

**Not a deviation, but worth flagging for PHASE08.** Plan §5 asks for "22 `.axaml` files" while
PHASE08 asks that `Fluent.axaml` "merges exactly 22 dictionaries (2 foundation + 20 control
templates)". These agree only if `Fluent.axaml` itself is excluded from the count — i.e. 23 files on
disk. After this phase the tree holds 5 (`Colors`, `Brushes`, `Fluent`, and the 2 editor templates),
and `Fluent.axaml` merges 4.

**Line endings:** no CRLF recommendation to make. All 18 new files are LF with a final newline; the
`.gitattributes` LF rule from FEATURE-28E8 PHASE01 continues to hold.

**Follow-ups for `FEATURE-6EB0`** — behaviours inherited verbatim from the port source, none of them
a defect to fix here, all worth pinning down with tests:

- `BaseEditor<T>` and `ByteArrayEditor` handle the empty-text case differently: the former honours
  `NullWhenEmpty` (falling back to `default(T)`), the latter always nulls `Value`. `ByteArrayEditor`
  has no `NullWhenEmpty` of its own — `byte[]?` is already nullable.
- `ByteArrayEditor` has no `_textModifiedByUser` guard, so its `CommitValue()` re-parses on every
  lost-focus even when the user typed nothing. `BaseEditor<T>` short-circuits that case.
- `TryParse` in both encoding editors returns `true` for `null` text while leaving `result` as the
  empty array — a null-vs-empty distinction the callers never exercise, because
  `string.IsNullOrEmpty` is checked first on every path.
- `SetParseError`/`ClearParseError` give the parse error priority over the binding error; a cleared
  parse error re-exposes an outstanding binding error rather than clearing the `:error` state.
- `BaseEditor.OnGotFocus` posts `SelectAll` to the dispatcher rather than calling it inline —
  headless tests need to pump the dispatcher to observe it.

## Build/test evidence

- **Build:** `dotnet build Enigma.Avalonia.slnx -c Release --no-incremental` → **Build succeeded,
  0 Warning(s), 0 Error(s)**, for `net8.0` and `net10.0`. Per §2.4.1 the log was re-read at
  `-v n` verbosity rather than trusting the exit code: **zero `AVLN*` occurrences** and zero
  `: warning`/`: error` diagnostic lines anywhere in it.
- **Tests:** `dotnet test --solution Enigma.Avalonia.slnx -c Release` → **1 total, 1 succeeded,
  0 failed, 0 skipped**. This phase adds no tests by design (plan §4 assigns the suite to
  `FEATURE-6EB0`), so Definition-of-Done criterion 2 is met by the existing suite continuing to
  pass. Note that the existing smoke test now covers more than it did: it loads
  `Themes/Fluent.axaml`, which merges the two new dictionaries, so a template that failed to compile
  or resolve would fail that test.
- **Byte-array round-trip — verified, not deferred.** A throwaway headless harness (built outside
  the repository against the Release `net10.0` output, then discarded) exercised the real controls.
  All 15 checks passed:

  | Check | Result |
  |---|---|
  | `Base64Editor.Value = payload` → `Text` equals `Base64Service.Encode(payload)` | pass |
  | `Base64Editor.Text = encoded` → `Value` equals the original bytes | pass |
  | `HexadecimalEditor.Value = payload` → `Text` equals `HexService.Encode(payload)` | pass |
  | `HexadecimalEditor.Text = encoded` → `Value` equals the original bytes | pass |
  | Neither editor raises a validation error on valid input | pass |
  | Both set `HasValidationError` on undecodable input | pass |
  | `IntEditor` round-trips `42` / `"1234"` and errors on `"not a number"` | pass |
  | `:error` pseudo-class is set on failure and absent on success | pass |
  | `Fluent.axaml` resolves a `ControlTheme` for `typeof(BaseEditor)` and `typeof(MultiLineTextEditor)` | pass |

  Payload `00 01 7F 80 FE FF 2A 42` → Base64 `AAF/gP7/KkI=` → hex `00017f80feff2a42`, both decoding
  back byte-for-byte.
- **`:error` styling resolves:** covered by the last two rows above — the pseudo-class is set by
  `BaseEditor`'s static class handler, and the dictionary that carries the `^:error` selectors is
  reachable from the theme's public URI.
- **`grep -ri carbon src/`** → zero hits (§3.7).
- **Excluded-type gate** — `CalendarSchedule|Displayer2D|DrawingObject|Shape` across `src/` → zero
  hits (§2).
- **File count:** 16 of 16 `.cs` under `Controls/Editors/`, 2 of 2 `.axaml` under
  `Themes/Controls/Editors/`.

## Acceptance criteria

| Criterion | Status |
|---|---|
| Build clean for `net8.0` and `net10.0` | Met — 0 warnings, 0 `AVLN*`, Release, non-incremental |
| Byte array round-trips through `Base64Editor` and `HexadecimalEditor` | Met — verified in a scratch headless harness (not deferred); evidence above |
| Zero `Carbon` hits | Met |
| The `:error` pseudo-class styling still resolves | Met |
| Both editor includes appended to `Fluent.axaml` | Met — it now merges 4 dictionaries |
