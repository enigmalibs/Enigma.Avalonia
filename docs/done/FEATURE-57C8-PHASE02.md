# FEATURE-57C8 PHASE02 — Pages: Base controls, Editors, Dialogs, Services

**Branch:** `feature/feature-57c8-phase02-pages`
**Status:** DONE

## Summary

Four page pairs ported from the source tester onto the PHASE01 shell, taking the rail from one item to
five. Nothing under `src/` or `tests/` changed — the phase only consumes the library, and every page is
a worked example of one part of its surface:

| Page | What it demonstrates |
|---|---|
| Base Controls | Stock Avalonia controls restyled purely by the theme's overrides of Fluent's `TextControl*`/`ComboBox*` keys — no Enigma control on screen |
| Editors | All 13 concrete editors, each bound to a validated `ObservableValidator` value, with a live readout beside it |
| Dialogs | `IFileDialogService` / `IFolderDialogService` driven from a ViewModel that never sees the window |
| Services | `IContentDialogService` (5 variants), `IOverlayService` (hosting the showcase's own `ProgressOverlayCard`), `IInfoBarService` (all four severities) |

Each page's View is registered transient and its ViewModel singleton in `AddPagesAndViewModels()`, and
each gets one `AddPage(...)` line in `MainWindowViewModel` — the two-step registration PHASE01 left for
later phases to follow.

### The one thing worth copying from this phase

**Dialog content built in C# resolves its brushes as dynamic resources.** The port source hard-coded
`#F59E0B`, `#888888`, `#4CAF50` and `#22808080` into dialog and overlay content. Those would be the only
surfaces in the app that ignore a theme switch, which PHASE03's own acceptance criterion ("charts …
follow a runtime Dark↔Light switch **along with every control on screen**") forbids. The pattern used
instead, already established by PHASE01's `CreateLogo`:

```csharp
block[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("EnigmaForegroundSecondaryBrush");
```

`ThemedText(text, brushKey)` in `ServicesTestingPageViewModel` wraps it for the common case.

## Files/modules touched

**Created**

- `samples/…/ViewModels/BaseControlsPageViewModel.cs`
- `samples/…/ViewModels/EditorsTestingPageViewModel.cs`
- `samples/…/ViewModels/DialogsTestingPageViewModel.cs`
- `samples/…/ViewModels/ServicesTestingPageViewModel.cs`
- `samples/…/Views/BaseControlsPageView.axaml` + `.axaml.cs`
- `samples/…/Views/EditorsTestingPageView.axaml` + `.axaml.cs`
- `samples/…/Views/DialogsTestingPageView.axaml` + `.axaml.cs`
- `samples/…/Views/ServicesTestingPageView.axaml` + `.axaml.cs`

**Modified**

- `samples/…/ServiceCollectionExtensions.cs` — four Views transient, four ViewModels singleton.
- `samples/…/ViewModels/MainWindowViewModel.cs` — four `AddPage(...)` lines.
- `docs/roadmap.md`, `docs/plan/FEATURE-57C8.md` — statuses, plus an *As built* note for PHASE03.

## Deviations & follow-ups

1. **The plan's page descriptions are swapped.** §5 labels `DialogsTestingPage*` as "ContentDialog
   variants, sizing, `DefaultButton`, `DialogResult`" and `ServicesTestingPage*` as "… file/folder
   pickers"; in the port source it is the other way round. The source's page identities were kept (they
   are the named deliverables) — the union of the two pages covers everything §5 lists either way, so
   nothing was dropped. Recorded in the plan's *As built* note so PHASE03 doesn't re-derive it.
2. **`DefaultButton` added, not ported.** §5 asks for it and the library has it, but the source never
   sets it. The Confirm and Password dialogs now nominate `DefaultButton.Primary`; the other three
   leave it `None`, so the difference is visible side by side.
3. **Concrete command types, not interfaces.** The source types every command property as
   `IAsyncRelayCommand`; the `communitytoolkit-mvvm` house skill types them as the concrete
   `AsyncRelayCommand`. The skill wins — it is the loaded convention for new code here — and handlers
   are renamed to its `On…Async` form. No behavioural difference: XAML binds `ICommand` either way, and
   `NotifyCanExecuteChanged` is available on both.
4. **Repeated chrome moved into page-local style classes.** The source repeats five attributes on
   nineteen readout `TextBlock`s (and on the picker result `Border`s). Those became
   `<Style Selector="TextBlock.readout">` etc. in `UserControl.Styles` — identical rendering, and a
   reference app should not teach copy-paste.
5. **Hardcoded colours replaced by theme brushes** — see the summary above. `#F59E0B` →
   `EnigmaWarningBrush`, `#888888` → `EnigmaForegroundSecondaryBrush`, `#4CAF50` →
   `EnigmaSuccessBrush`, `#22808080` → `EnigmaSurfaceBrush` + `EnigmaBorderBrush`.
6. **Icons.** Per plan §3.5: `{ei:IconGeometry …}` for `SettingsCardExpander.IconData` (a `Geometry`
   consumed by a themed `PathIcon` in the control's own template) and `<ei:Icon>` for the editors'
   `LeadingContent`, which sits in interactive chrome. The Editors rail item uses
   `PhosphorIcon.PencilSimple`, replacing the source's hand-written `Geometry.Parse` path. The three
   expanders on the Editors page gained icons the source did not have (`Hash`, `Sigma`, `TextAa`,
   `Binary`) so all four sections read consistently.
7. **The Services rail item is "Services", not the source's "Services with a very long header".** That
   header was a rail-truncation probe; the headless control tests cover the rail, and a reference app
   should not ship a joke label.
8. **`d:DesignWidth`/`mc:Ignorable` dropped**, consistent with PHASE01's views: the pages' ViewModels
   are resolved from the container, so the XAML previewer cannot build them anyway.
9. **Library defect found — `ContentDialogService.ResetDialog` does not reset the sizing properties.**
   It resets 14 properties and clears `IconBrush`, but not `DialogWidth`/`Height`/`MinWidth`/`MaxWidth`/
   `MinHeight`/`MaxHeight`. Confirmed by measurement: after the Wide (900) and Tall (400) dialogs, a
   later simple dialog still reports `DialogMaxWidth=900, DialogMaxHeight=400`. The showcase does **not**
   work around it — hiding it would put the workaround in the code consumers copy. *Follow-up: worth a
   `BUG-` item against the library; the fix is six lines in `ResetDialog`.* Out of scope here
   (FEATURE-22A5 is `DONE`, and this phase touches no library code).
10. **Line endings:** nothing to report. Every file added or modified is LF with a final newline.

## Build/test evidence

- **Build:** `dotnet build --no-incremental` → `Build succeeded. 0 Warning(s), 0 Error(s)`. Checked for
  `AVLN*` explicitly on a full rebuild per solution invariant §2.4.1 — none. Every view root and every
  `DataTemplate` using `{Binding}` declares `x:DataType`.
- **Tests:** `dotnet test` → **273 passed, 0 failed** (unchanged: this phase adds no tests, and its
  acceptance criteria are interactive — they call for none).
- **Clean-slate rule:** zero occurrences of `Carbon` anywhere under `samples/`.
- **Rendering, real app, real desktop session** (Wayland/KDE): launched four times with
  `Showcase__InitialPage=<key>` and screenshotted each page. All four render with the library's
  `Enigma*` brushes and `Enigma.Icons` glyphs, and the rail highlights the right entry. Launching by
  config key exercises the same path a click does — `NavigateToInitialPage` → `Navigation.SelectedItem`
  → the page factory → DI — so this also verifies navigation resolution for all four pages.
- **Interaction, headless with real input events.** The compositor blocks synthetic input on this
  session (PHASE01 hit the same wall: no `xdotool`/`wtype`, `/dev/uinput` is root-only), so the
  interactive criteria were driven through Avalonia's headless platform with `MouseDown`/`MouseUp` and
  `KeyTextInput` against the **real** `MainWindow`, the **real** `AddEnigmaServices()` /
  `AddPagesAndViewModels()` registrations and the real page Views. A throwaway harness in the session
  scratchpad (not committed) reported **86/86 checks passing**, including:
  - each of the five rail items resolves its expected View, attaches its expected ViewModel and lays out;
  - typing `777` into the `IntEditor` reaches the ViewModel; unparseable text raises `:error` with a
    message; `50000` raises `:error` from `[Range(0, 10000)]` (the DataAnnotations →
    `INotifyDataErrorInfo` → binding path, which no existing test covered); clearing the `[Required]`
    `TextEditor` raises `:error`; typing hex re-renders the same bytes in the Base64 editor; all 13
    concrete editor types are present;
  - the simple dialog opens and returns `Primary`; the complex dialog reports `DefaultButton.Primary`,
    shows three buttons and returns `Secondary`; the password dialog's typed value (`s3cret`) is read
    back after it closes; the wide/tall dialogs carry their raised caps;
  - all four InfoBar severities open with the right severity and title and report back on close;
  - the overlay opens hosting a `ProgressOverlayCard`, both task commands disable while it runs, and the
    page reports the elapsed time when it closes.
  - Frames captured from the headless window confirm by eye: the complex dialog's warning icon in the
    theme's amber, the accent-styled default button, the wide dialog's four themed columns, the error
    InfoBar, the overlay card, and the `IntEditor` with a red border and *"The field IntValue must be
    between 0 and 10000."* beneath it.
- **The one criterion not verified by machine:** that the **native** file/folder picker window appears
  on screen. Headless supplies a no-op storage provider, so what is proven there is the whole path up to
  it — the command reaches the service, the service has the window's storage provider, and the cancelled
  result renders. The platform dialog itself needs a human click; worth a glance on the next run.
