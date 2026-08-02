# FEATURE-22A5 — Control library: Enigma.Avalonia.Desktop

**Status:** IN PROGRESS · 8 phases
**Branches:** `feature/feature-22a5-phaseNN-<area>` (one per phase)
**Solution invariants:** `docs/plan/FEATURE-28E8.md` §2 — that section wins over anything here.

## 1. Objective

Populate `src/Enigma.Avalonia.Desktop/` with the full control library: 40 control classes,
the 7-file `CollectionView` data subsystem, 15 service files and 22 theme dictionaries — a
**faithful 1:1 port** with no behavioural or public-API change beyond the mechanical
transformations in §3.

## 2. Port source (internal reference — see FEATURE-28E8 §2.4.10)

```
/home/jo/Dev/Carbon.Avalonia.Desktop/src/Carbon.Avalonia.Desktop/
```

Read that tree as the authoritative source of behaviour. **Port it — do not reimplement it.** When a
detail looks odd (a magic number, an unusual event-detach order, a `Background="Transparent"` that
seems pointless), carry it over unchanged: it is almost certainly load-bearing. Hit testing in
particular depends on it — an element with a null `Background` is invisible to the pointer.

### Deliberately excluded

| Excluded | Files |
|---|---|
| `Controls/CalendarSchedule/` | `CalendarSchedule.cs`, `CalendarScheduleItem.cs`, `CalendarScheduleItemChangedEventArgs.cs`, `CalendarViewMode.cs`, `ScheduleDragSession.cs`, `ScheduleInteractionMode.cs` |
| `Controls/Displayer2D/` | `Displayer2D.cs`, `Displayer2DCanvas.cs`, `DragInteraction.cs`, `DrawingObject.cs`, `DrawingObjectGroup.cs`, `ISelectableDrawingObject.cs`, `UserInteraction.cs`, `Groups/` (2), `Shapes/` (7) |
| Their templates | `Themes/Controls/CalendarSchedule/CalendarSchedule.axaml`, `Themes/Controls/Displayer2D/Displayer2D.axaml` |
| Stale references | `Microsoft.Extensions.DependencyInjection` (zero usages), `PhosphorIconsAvalonia` (zero usages) |

No excluded type may be referenced from any ported file. After each phase, grep the ported tree for
`CalendarSchedule`, `Displayer2D`, `DrawingObject` and `Shape` and confirm zero hits.

## 3. Mechanical transformations (apply to every ported file)

1. **Namespace:** `Carbon.Avalonia.Desktop` → `Enigma.Avalonia.Desktop` (and every sub-namespace).
2. **Resource keys:** `Carbon<Token>` → `Enigma<Token>` in both the definitions
   (`Colors.axaml`, `Brushes.axaml`) and every consumer (`{DynamicResource …}` in 22 templates).
   Never convert a `DynamicResource` to `StaticResource` — the dynamic lookup is what makes runtime
   theme switching work.
3. **Resource URIs:** `avares://Carbon.Avalonia.Desktop/…` → `avares://Enigma.Avalonia.Desktop/…`.
4. **XAML namespace declarations:** `using:Carbon.Avalonia.Desktop.*` → `using:Enigma.Avalonia.Desktop.*`.
5. **Encoding:** `using Enigma.Cryptography.DataEncoding;` → `using Enigma.Core.Encoding;`
   (`Base64Service` and `HexService` keep the same type names and the same
   `string Encode(byte[])` / `byte[] Decode(string)` members — nothing else in those two editors
   changes).
6. **`global::Avalonia.…`:** replace each of the 29 occurrences with a file-scoped `using` above the
   `namespace` declaration (FEATURE-28E8 §2.4.11). Keep `global::` only where a using genuinely
   cannot express it, with a one-line comment saying why.
7. **Clean-slate sweep:** grep every ported file for the literal string `Carbon` — including XML doc
   comments, `<see cref>` targets and `PART_` names — and leave **zero** hits.
8. **Doc comments:** every public member keeps (or gains) its XML documentation; CS1591 is a build
   error.

## 4. Phase order and rationale

The theme dictionary comes first because every control template resolves `Enigma*` keys from it;
`CollectionView` comes second because it has no dependency on any control and gives the test suite
something substantial to target. After that, one control family per phase, cheapest first, with the
two heaviest (Ribbon, Docking) last.

Every phase ends with `dotnet build Enigma.Avalonia.slnx` clean at zero warnings, and adds its new
template include to `Themes/Fluent.axaml`.

**Test note:** the automated suite is `FEATURE-6EB0`, deliberately a separate item. Phases here are
*not* expected to add tests; Definition-of-Done criterion 2 is satisfied by the existing suite
continuing to pass. Verification per phase is: zero-warning build + the grep gates in §3.7 and §2.

---

## PHASE01 — Theme foundation & `Enigma*` resource keys — DONE

Port `Themes/Colors.axaml` (`ResourceDictionary.ThemeDictionaries` with `x:Key="Dark"` and
`x:Key="Light"`), `Themes/Brushes.axaml` (brushes referencing colors via `DynamicResource`) and
`Themes/Fluent.axaml` (the merged-dictionary composition root).

- Rename every key per §3.2 and record the **complete old → new key map** in the completion doc — it
  is the reference the remaining seven phases and the showcase depend on.
- `Fluent.axaml` starts with `Colors.axaml` + `Brushes.axaml` only; each later phase appends its own
  `ResourceInclude` lines. The two excluded controls' includes never appear.
- Root element is a `ResourceDictionary`, so consumers merge it as a `ResourceInclude` in
  `Application.Resources` — **not** a `StyleInclude`. Note this in the completion doc; FEATURE-2802
  documents it for consumers.

**Acceptance:** build clean; `Colors.axaml`/`Brushes.axaml`/`Fluent.axaml` present under
`Themes/`; zero `Carbon` hits; both theme variants defined; the key map is recorded.

### PHASE01 recorded output — the resource-key map (PHASE02–08 porting contract)

Recorded here rather than in `docs/done/FEATURE-22A5-PHASE01.md` because §2.4.10 makes `docs/plan/*.md`
the **only** artifact permitted to name the port source, and an old → new table necessarily names the
old keys. Per §2 that rule wins over PHASE01's "record it in the completion doc" wording. The
completion doc carries the equivalent new-key inventory.

**The rule for every template ported in PHASE02–08:** a `{DynamicResource Carbon<Token>}` becomes
`{DynamicResource Enigma<Token>}`. It stays a `DynamicResource` — never a `StaticResource`.

**Colors** (`Themes/Colors.axaml`, same 29 keys in both `Dark` and `Light`) — `CarbonBackgroundColor`,
`CarbonSurfaceColor`, `CarbonSurfaceHighColor`, `CarbonSurfaceLowColor`, `CarbonBorderColor`,
`CarbonBorderSubtleColor`, `CarbonForegroundColor`, `CarbonForegroundSecondaryColor`,
`CarbonForegroundTertiaryColor`, `CarbonAccentColor`, `CarbonAccentHoverColor`, `CarbonSelectionColor`,
`CarbonInputBackgroundColor`, `CarbonInputBackgroundFocusedColor`, `CarbonInputBackgroundHoverColor`,
`CarbonHoverColor`, `CarbonPressedColor`, `CarbonOverlayColor`, `CarbonSuccessColor`,
`CarbonWarningColor`, `CarbonErrorColor`, `CarbonInfoBackgroundColor`, `CarbonInfoBorderColor`,
`CarbonSuccessBackgroundColor`, `CarbonSuccessBorderColor`, `CarbonWarningBackgroundColor`,
`CarbonWarningBorderColor`, `CarbonErrorBackgroundColor`, `CarbonErrorBorderColor` → each with
`Carbon` replaced by `Enigma`.

**Brushes** (`Themes/Brushes.axaml`, 26 keys) — the same tokens with a `Brush` suffix, minus the three
`CarbonInputBackground*` colors (plain, `…Focused`, `…Hover`), which have no brush of their own and are
consumed directly by the Fluent overrides below. Full list: `CarbonBackgroundBrush`,
`CarbonSurfaceBrush`, `CarbonSurfaceLowBrush`, `CarbonSurfaceHighBrush`, `CarbonBorderBrush`,
`CarbonBorderSubtleBrush`, `CarbonForegroundBrush`, `CarbonForegroundSecondaryBrush`,
`CarbonForegroundTertiaryBrush`, `CarbonAccentBrush`, `CarbonAccentHoverBrush`, `CarbonSelectionBrush`,
`CarbonHoverBrush`, `CarbonPressedBrush`, `CarbonOverlayBrush`, `CarbonSuccessBrush`,
`CarbonWarningBrush`, `CarbonErrorBrush`, `CarbonInfoBackgroundBrush`, `CarbonInfoBorderBrush`,
`CarbonSuccessBackgroundBrush`, `CarbonSuccessBorderBrush`, `CarbonWarningBackgroundBrush`,
`CarbonWarningBorderBrush`, `CarbonErrorBackgroundBrush`, `CarbonErrorBorderBrush` → `Enigma…`.

**Fluent override keys — 23, names unchanged.** `TextControlBackground`,
`TextControlBackgroundPointerOver`, `TextControlBackgroundFocused`, `TextControlBorderBrush`,
`TextControlBorderBrushPointerOver`, `TextControlBorderBrushFocused`, `TextControlForeground`,
`TextControlForegroundPointerOver`, `TextControlForegroundFocused`,
`TextControlPlaceholderForeground`, `TextControlPlaceholderForegroundPointerOver`,
`TextControlPlaceholderForegroundFocused`, `TextControlSelectionHighlightColor`, `ComboBoxBackground`,
`ComboBoxBackgroundPointerOver`, `ComboBoxBackgroundPressed`, `ComboBoxBackgroundDisabled`,
`ComboBoxBorderBrush`, `ComboBoxBorderBrushPointerOver`, `ComboBoxBorderBrushPressed`,
`ComboBoxForeground`, `ComboBoxForegroundPointerOver`, `ComboBoxPlaceholderTextForeground`. These are
FluentTheme's own contract — renaming one silently stops the override applying.

**Dropped — 12 keys, by decision taken during this phase.** The six `CarbonCalendar*Color`
(`Today`, `Selected`, `OutOfMonth`, `Appointment`, `GridLine`, `CurrentTime`) and their six
`CarbonCalendar*Brush` counterparts were **not** ported. Their sole consumer in the port source is
`Controls/CalendarSchedule/CalendarSchedule.cs`, which §2 excludes — porting them would ship 12 theme
keys no shipped control consumes, and FEATURE-2802 would have to document them. No template ported in
PHASE02–08 references them (verified by grep against the port source). **If `CalendarSchedule` is ever
revived, this decision must be reversed first.**

## PHASE02 — CollectionView data subsystem — DONE

Port `Data/` verbatim: `CollectionView.cs`, `CollectionViewGroup.cs`, `CollectionViewSource.cs`,
`FilterEventArgs.cs`, `PropertyGroupDescription.cs`, `SortDescription.cs`, `SortDirection.cs`.

`PropertyGroupDescription.cs` is one of the `global::Avalonia` files — apply §3.6.

**Acceptance:** build clean; all 7 files present under `Data/`; the public surface
(`CollectionViewSource`, filtering, `SortDescription`/`SortDirection`, grouping) is byte-for-byte
equivalent in shape to the source; zero `Carbon` hits.

## PHASE03 — Editors (16 controls, Enigma.Core encoding) — DONE

Port `Controls/Editors/` (16 files): `BaseEditor.cs`, `BaseEditorOfT.cs`, `ByteArrayEditor.cs`,
`TextEditor.cs`, `MultiLineTextEditor.cs`, `IntEditor.cs`, `ShortEditor.cs`, `LongEditor.cs`,
`UIntEditor.cs`, `UShortEditor.cs`, `ULongEditor.cs`, `SingleEditor.cs`, `DoubleEditor.cs`,
`DecimalEditor.cs`, `Base64Editor.cs`, `HexadecimalEditor.cs` — plus
`Themes/Controls/Editors/BaseEditor.axaml` and `MultiLineTextEditor.axaml`.

- `Base64Editor` and `HexadecimalEditor` keep their `private static readonly` service instance and
  their swallow-and-return-false `TryParse`; only the `using` changes (§3.5).
- `ByteArrayEditor`, `BaseEditorOfT` and `MultiLineTextEditor` are `global::Avalonia` files (§3.6).
- The `"Invalid value"` default `ValidationErrorMessage` in `ByteArrayEditor` stays as-is — it is a
  consumer-overridable property, which is why no localisation infrastructure is needed.
- Append both editor includes to `Fluent.axaml`.

**Acceptance:** build clean for `net8.0` and `net10.0`; a byte array round-trips through
`Base64Editor` and `HexadecimalEditor` (verified by eye in a scratch harness or deferred to
FEATURE-6EB0 — say which in the completion doc); zero `Carbon` hits; the `:error` pseudo-class
styling still resolves.

## PHASE04 — Navigation controls & navigation service — DONE

Port `Controls/Navigation/` (`NavigationView.cs`, `NavigationItem.cs`, `NavigationOrientation.cs`),
`Themes/Controls/Navigation/NavigationView.axaml` + `NavigationItem.axaml`, and the navigation
services: `INavigationService.cs`, `NavigationService.cs`, `INavigationViewModel.cs`,
`NavigationFailedEventArgs.cs`.

- `NavigationService` stays `: ObservableObject` (CommunityToolkit.Mvvm is kept by decision).
- Preserve exactly: the `SemaphoreSlim(1,1)` serialization with concurrent navigations **dropped**;
  the `PageFactory(NavigationItem)` indirection with its `Activator.CreateInstance` default; the
  `OnAppearingAsync(object?)` / `OnDisappearingAsync()` lifecycle with `false` cancelling navigation;
  `Items` and `FooterItems`; `PaneSize` (default 90) and `LabelMaxWidth` (default 72).
- `NavigationView.cs` is a `global::Avalonia` file (§3.6).
- Append both navigation includes to `Fluent.axaml`.

**Acceptance:** build clean; the 7 files present; `PaneSize`/`LabelMaxWidth` defaults unchanged;
`OnApplyTemplate` still detaches old handlers before attaching new ones; zero `Carbon` hits.

## PHASE05 — ContentDialog, Overlay, InfoBar + their services — DONE

Port `Controls/ContentDialog/` (`ContentDialog.cs`, `DefaultButton.cs`, `DialogResult.cs`),
`Controls/Overlay.cs`, `Controls/InfoBar/` (`InfoBar.cs`, `InfoBarSeverity.cs`), their three
templates (`Themes/Controls/ContentDialog.axaml`, `Overlay.axaml`, `InfoBar.axaml`) and the three
services + interfaces (`IContentDialogService`/`ContentDialogService`,
`IOverlayService`/`OverlayService`, `IInfoBarService`/`InfoBarService`).

Preserve exactly:

- The **host pattern**: each service needs `RegisterHost(...)` once at startup and throws
  `InvalidOperationException` when used before that. Hosts are siblings in the window's root `Panel`.
- `ContentDialog`'s sizing surface — `DialogWidth`, `DialogHeight`, `DialogMinWidth`,
  `DialogMaxWidth` (defaults 320/600), `DialogMinHeight`, `DialogMaxHeight` — and the content
  `ScrollViewer` that keeps title and buttons fixed.
- `ContentDialogService.ShowMessageAsync(string title, string message, string closeButtonText = "OK")`
  — signature and default value unchanged.
- Two-way template bindings stay `{Binding …, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}`;
  `{TemplateBinding}` is one-way only in Avalonia and must not be substituted.

Append the three includes to `Fluent.axaml`.

**Acceptance:** build clean; 11 files present; the pre-`RegisterHost` throw is intact on all three
services; dialog size defaults unchanged; zero `Carbon` hits.

## PHASE06 — Settings cards + file/folder dialog services — DONE

Port `Controls/SettingsCard.cs`, `Controls/SettingsCardExpander.cs`, their two templates, and the
picker services: `IFileDialogService.cs`, `FileDialogService.cs`, `FileDialogServiceExtensions.cs`,
`IFolderDialogService.cs`, `FolderDialogService.cs`, `FolderDialogServiceExtensions.cs`.

- The picker services are initialised with `SetStorageProvider(window.StorageProvider)` at startup —
  same contract, same failure mode as the host services.
- Keep the extension-method filter helpers as-is (they are the ergonomic surface consumers use).
- `SettingsCardExpander`'s `:expanded` / `:hasContent` pseudo-classes and their styling stay.

Append both includes to `Fluent.axaml`.

**Acceptance:** build clean; 8 files present; zero `Carbon` hits.

## PHASE07 — Ribbon — TODO

Port `Controls/Ribbon/` (7 files: `Ribbon.cs`, `RibbonTab.cs`, `RibbonGroup.cs`, `RibbonButton.cs`,
`RibbonToggleButton.cs`, `RibbonDropDownButton.cs`, `RibbonMenuItem.cs`) and all 6
`Themes/Controls/Ribbon/*.axaml`.

`Ribbon.cs` is a `global::Avalonia` file (§3.6). Append all six includes to `Fluent.axaml`.

**Acceptance:** build clean; 13 files present; tab selection, group layout and the drop-down
popup all still template correctly (verified by eye once FEATURE-57C8 PHASE03 exists, or in a scratch
harness — say which); zero `Carbon` hits.

## PHASE08 — Docking — TODO

Port `Controls/Docking/` (6 files: `DockingHost.cs`, `DockPane.cs`, `DockTabGroup.cs`,
`DockSplitContainer.cs`, `DockLayoutNode.cs`, `DockPosition.cs`) and all 4
`Themes/Controls/Docking/*.axaml`.

This is the most intricate family — the layout tree (`DockLayoutNode`/`DockPosition`), the splitter
container and the drag-to-dock interaction. Port file by file, building after each, rather than in
one sweep.

Append all four includes to `Fluent.axaml`.

**Acceptance:** build clean; 10 files present; `Fluent.axaml` now merges exactly 22 dictionaries
(2 foundation + 20 control templates) and none of them is a `CalendarSchedule` or `Displayer2D`
entry; zero `Carbon` hits anywhere under `src/`.

---

## 5. Item-level acceptance criteria

- `src/Enigma.Avalonia.Desktop/` contains **62 `.cs`** files (40 controls + 7 data + 15 services) and
  **22 `.axaml`** files.
- `dotnet build Enigma.Avalonia.slnx -c Release` succeeds for `net8.0` and `net10.0` with zero
  warnings, XAML `AVLN*` diagnostics included.
- `grep -ri carbon src/` returns nothing.
- The library's only `PackageReference`s are `Avalonia`, `Avalonia.Themes.Fluent`,
  `CommunityToolkit.Mvvm` and `Enigma.Core`.
- No public type, member, default value or pseudo-class name differs from the port source, apart from
  the namespace and the `Enigma*` resource-key rename.
