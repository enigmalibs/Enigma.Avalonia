# FEATURE-57C8 PHASE03 — Pages: Ribbon, Docking, Navigation, CollectionView, Charts, Settings

**Branch:** `feature/feature-57c8-phase03-pages`
**Status:** DONE

## Summary

The showcase's last seven page pairs, taking the rail from five items to eleven and completing
FEATURE-57C8. Six join the main list, `SettingsPage` is the footer entry, and `DummyPage` is reachable
only from the Navigation page — twelve pages in all, as PHASE01 predicted.

| Page | What it demonstrates |
|---|---|
| Ribbon | `Ribbon` / `RibbonTab` / `RibbonGroup` / `RibbonButton` / `RibbonToggleButton` / `RibbonDropDownButton` + `RibbonMenuItem`, over a live status readout |
| Docking | A `DockingHost` built from a `DockLayoutNode` tree the ViewModel composes — four groups, three splits, seven panes, per-pane `CanClose`/`CanMove` |
| Navigation | `NavigateToAsync` for a rail page and for an unlisted one, plus `INavigationViewModel.OnDisappearingAsync` vetoing a departure |
| Collections | `CollectionViewSource` sorting, filtering and grouping a live `ObservableCollection`, with the source shown beside the view |
| Charts | LiveCharts2 line/column/pie, rebuilt on `ActualThemeVariantChanged` — the one surface a theme switch does not reach on its own |
| Settings | `SettingsCard` in its three shapes and `SettingsCardExpander`, and the switch that flips the whole application's theme |

### The thing worth knowing from this phase

**A library defect blocked an acceptance criterion, and was fixed rather than worked around.**
`CollectionView` implemented `IEnumerable + INotifyCollectionChanged` but not `IList`. Avalonia 12's
`ItemsSourceView` refuses exactly that pair:

```
[Binding] An error occurred binding 'ItemsSource' to 'PeopleView.View' at 'View':
'Collection implements INotifyCollectionChanged but not IList.' (ListBox)
```

So the library's data-layer headline type could not be an `ItemsSource` — its only purpose. All 273
tests passed because every one of them exercised the view in isolation, as an `IEnumerable`; none ever
bound it to a control. The declaration was ported faithfully (the Carbon original is identical), so
this is an upstream defect FEATURE-22A5 inherited correctly.

PHASE02's rule is that the showcase never works around a library defect, because that puts the
workaround in the code consumers copy. Here the defect blocked *"the CollectionView page sorts, filters
and groups live"* outright, so on the user's decision the fix landed in this phase.

## Files/modules touched

**Created — showcase (`samples/Enigma.Avalonia.Desktop.Showcase/`)**

- `ViewModels/RibbonTestingPageViewModel.cs` + `Views/RibbonTestingPageView.axaml(.cs)`
- `ViewModels/DockingTestingPageViewModel.cs` + `Views/DockingTestingPageView.axaml(.cs)`
- `ViewModels/NavigationDemoPageViewModel.cs` + `Views/NavigationDemoPageView.axaml(.cs)`
- `ViewModels/CollectionViewPageViewModel.cs` + `Views/CollectionViewPageView.axaml(.cs)`
- `ViewModels/ChartsPageViewModel.cs` + `Views/ChartsPageView.axaml(.cs)`
- `ViewModels/SettingsPageViewModel.cs` + `Views/SettingsPageView.axaml(.cs)`
- `ViewModels/DummyPageViewModel.cs` + `Views/DummyPageView.axaml(.cs)`
- `ViewModels/PaneContent.cs`, `ViewModels/PersonItem.cs` — one type per file, as `HomeHighlight` set

**Modified — showcase**

- `ServiceCollectionExtensions.cs` — seven Views transient, seven ViewModels singleton.
- `ViewModels/MainWindowViewModel.cs` — six `AddPage(...)` lines plus the `footer: true` Settings entry.

**Modified — library (`src/Enigma.Avalonia.Desktop/`)**

- `Data/CollectionView.cs` — now `IList`. `Count`/`IsEmpty` unchanged; added `this[int]` (get),
  `Contains`, `IndexOf`, `CopyTo`, `IsReadOnly`, `IsFixedSize`, `IsSynchronized`, `SyncRoot`. Every
  mutator (`Add`, `Insert`, `Remove`, `RemoveAt`, `Clear`, the indexer setter) throws
  `NotSupportedException` naming the source collection as the thing to change. No behaviour change to
  filtering, sorting, grouping or notification.

**Modified — tests (`tests/Enigma.Avalonia.Desktop.UnitTests/`)**

- `Data/CollectionViewBindingTests.cs` — **new**, 8 tests. Binds a real `ItemsControl` to a real view:
  the view reaches the control, in view order, and a filter change or a source add reaches it too.
  Plus the `IList` surface itself, including that every mutation is refused.
- `Data/CollectionViewEdgeCaseTests.cs`, `CollectionViewFilteringTests.cs`,
  `CollectionViewNotificationTests.cs`, `CollectionViewSourceTests.cs` — seven
  `Assert.Equal(0|1, view.Count)` became `Assert.Empty(view)` / `Assert.Single(view)`. Not a choice:
  once the view is an `ICollection`, xUnit's xUnit2013 analyzer flags the old form, and
  `TreatWarningsAsErrors` turns that into a build error. Identical assertions.

**Modified — docs**

- `docs/roadmap.md`, `docs/plan/FEATURE-57C8.md` — PHASE03 and the item itself to `DONE`, plus the
  *As built* note recording the scope change.

## Deviations & follow-ups

1. **The library fix, above.** The one departure from "this phase touches `samples/` only", taken with
   the user's explicit agreement after the alternatives (separate `BUG-` item first, or shipping the
   page with a dead list) were put to them. FEATURE-22A5 stays `DONE` — this corrects one line of its
   output rather than reopening it.
2. **The port source's filter was inert.** `CollectionViewPageViewModel` had a `FilterText` property
   that nothing read, and the XAML declared its `CollectionViewSource` instances as static resources —
   so the page could not have filtered even with the `IList` fix. The rebuilt page owns its
   `CollectionViewSource` in the ViewModel, which is also the only place the ordering rule can be
   honoured: **the `Filter` event must be attached before `Source` is assigned**, because
   `OnSourceChanged` captures it once. Sort descriptions go on *after* `Source`, for the opposite
   reason — see follow-up 8.
3. **Live sort and grouping controls added.** The source page hard-coded one sort and one grouping in
   XAML. The acceptance criterion says *live*, so the page now has a sort-property picker, an
   ascending/descending switch and a grouping checkbox, all driving `SortDescription` /
   `PropertyGroupDescription` in place and relying on their `DescriptionChanged` to refresh. The source
   collection is also displayed beside the view, so it is visible that the view never reorders it.
4. **Sixteen ribbon commands became three.** The source gives every ribbon control its own
   `IRelayCommand` and a one-line handler that assigns the same string. Fourteen of those are now one
   `RelayCommand<string?>` identified by `CommandParameter` — which the ribbon controls all support and
   which the page therefore also demonstrates. The two toggles keep their own commands because they own
   state. No library surface is lost, and a reference app should not teach fourteen copies of one line.
5. **`ToggleThemeCommand` dropped** from `SettingsPageViewModel`: it was never bound in the source's
   view. The `ToggleSwitch` binds `IsDarkTheme` two-way, which is the whole mechanism.
6. **Two `SelectedItem` bindings repaired.** The source binds `SelectedRegion` (a `string?`) to a
   `ComboBox` whose items are `ComboBoxItem`s — the selection is a control, not a string. Both region
   and sort pickers now bind `ItemsSource` to a `IReadOnlyList<string>`, which is also what makes them
   work under compiled bindings.
7. **Whitespace between adjacent `<Run>` elements is an inline.** A newline between two `Run`s renders
   as a real space, so `"Brown, Diana"` came out `"Brown , Diana"` and a group header read
   `"Engineering  (4)"`. Caught by reading the rendered text back out of the visual tree. Adjacent
   `Run`s in the new pages are now on one line, with a comment saying why. *Follow-up: the PHASE02
   pages and `HomePageView` were not audited for this.*
8. **Library follow-up — a description added before `Source` is subscribed twice.**
   `CollectionViewSource.OnSourceChanged` calls `AttachDescriptionChangedHandlers` for everything
   already in `SortDescriptions`, and `OnSortDescriptionsCollectionChanged` also attaches on add — so a
   description present when `Source` is assigned gets two handlers and refreshes the view twice per
   change. Harmless but wasteful. The showcase adds its sort description after `Source`, which is the
   natural order anyway, so this is not a workaround being taught. Worth a `BUG-` item.
9. **Library follow-up — a transient binding warning during a dock drop.** Re-docking a pane logs
   `An error occurred binding 'Content' to '$templatedParent.SelectedPane.PaneContent' at
   'SelectedPane': 'Value is null.'` from `DockTabGroup.axaml`, while a group is momentarily empty
   mid-rebuild. The drop completes correctly and every docking check passes; the template just needs a
   null guard. Worth folding into the same `BUG-` item.
10. **Rail label "Collections".** `CollectionView` is fourteen characters with no space and the rail
    wraps on whitespace, so it broke mid-word as `CollectionVie / w`. Caught on a real desktop
    screenshot. The page heading still reads "CollectionView" and the config key is still
    `collection-view`.
11. **The Settings page's app description was rewritten**, per plan §6 — it now describes this control
    library, with no trace of the port source's encryption-application text.
12. **Small additions to the ported pages**, all in the same spirit as PHASE02's: the charts page's line
    chart gained a seven-label axis to match its seven data points; `ChartsPageViewModel` configures the
    LiveCharts theme in its constructor as well as on the event, because it is a singleton that may
    first be built long after a theme switch; the docking panes carry icons and text that explain what
    each flag does; `SettingsPageViewModel.IsDarkTheme` is seeded by a property initializer so opening
    the page does not announce a switch that never happened.
13. **`d:DesignWidth`/`mc:Ignorable` dropped** and every view root and `DataTemplate` declares
    `x:DataType`, consistent with PHASE01 and PHASE02.
14. **Line endings:** nothing to report. Every file added or modified is LF with a final newline.

## Build/test evidence

- **Build:** `dotnet build --no-incremental` → `Build succeeded. 0 Warning(s), 0 Error(s)`, across all
  four target framework builds. No `AVLN*` on a full rebuild.
- **Tests:** `dotnet test` → **281 passed, 0 failed** (273 before; the 8 new ones are
  `CollectionViewBindingTests`). The new tests fail against the pre-fix `CollectionView`, which is what
  makes them a regression test rather than a description.
- **Clean-slate rule:** zero occurrences of `Carbon` anywhere under `samples/`.
- **Interaction, headless with real input events.** As in PHASE02 the compositor blocks synthetic
  input, so the interactive criteria were driven through Avalonia's headless platform with Skia
  rendering, against the **real** `MainWindow`, the **real** `AddEnigmaServices()` /
  `AddPagesAndViewModels()` registrations and the real page Views. A throwaway harness in the session
  scratchpad (not committed) reported **170/170 checks passing**:
  - all eleven rail items resolve their expected View, attach their expected ViewModel and lay out;
  - `NavigateToAsync` reaches the Settings page and selects its footer item, and reaches the unlisted
    page and *clears* the selection;
  - with unsaved text, leaving opens the guard dialog and the page does **not** change; "Keep editing"
    cancels the navigation and snaps the rail back; "Discard changes" lets it through;
  - ribbon: clicking a tab header switches the tab and reaches `SelectedTabIndex`; clicking the
    drop-down opens it and clicking away light-dismisses it; a menu item reports through its
    `CommandParameter`; a `RibbonButton` records its action; the Bold toggle checks, reaches the
    ViewModel, gains `:checked`, and its command sees the new state;
  - docking: the model produced 4 groups, 3 splits and 7 panes with the right `CanClose`/`CanMove`;
    clicking a tab selects its pane and swaps the content; **dragging** Doc 2's tab onto the bottom
    group's right edge created a fifth group holding it alone, and dragging it back onto the document
    group's centre rejoined those tabs and removed the emptied group;
  - collections: filtering by department and by name narrows the view case-insensitively and never
    touches the source; the direction switch and the property picker re-sort it; grouping produces
    three groups that keep the view's sort and compose with the filter; adding to and removing from the
    source refresh it; Reset restores everything. The rendered rows and group headers were read back
    out of the visual tree, which is how the empty-`ListBox` defect and the stray-space defect were
    both found;
  - charts and theme: flipping the Settings switch changes `Application.ActualThemeVariant`, the window
    background, the text on the settings page, and the headings on Home, Ribbon and Collections; the
    charts rebuild their series; the captured frame of the charts page differs pixel-for-pixel between
    variants.
- **Rendered frames.** 22 Skia frames captured from the real visual tree — every one of the eleven
  pages in both variants — and reviewed by eye: the ribbon's three tabs with icons and its four Home
  groups; the docking layout with the Solution and Properties panes correctly missing their close
  buttons; the collections page showing "10 of 10 people shown" over the view in `LastName` order with
  the source in insertion order beneath; the settings cards with their chevrons; the charts in the
  light variant with LiveCharts' own axes and labels following.
- **Real desktop, real window** (Wayland/KDE): the app was launched with
  `Showcase__InitialPage=<key>` and screenshotted; the Ribbon and Docking pages were confirmed by eye
  in a real window, which is where the rail's mid-word wrap was spotted. The remaining pages could not
  be captured that way — `spectacle -a` races with window focus and no window-raise tool
  (`kdotool`/`xdotool`/`wmctrl`) is available on this session, so the app stayed behind the terminal.
  The Skia frames above cover those pages instead, from the same visual tree the real window draws.
- **The one criterion not verified by machine:** nothing new. The native file/folder picker noted in
  PHASE02 is still the only interaction that needs a human click, and it belongs to PHASE02's page.
