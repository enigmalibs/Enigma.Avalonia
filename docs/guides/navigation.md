# Navigation

Enigma.Avalonia.Desktop turns a window into a multi-page shell with two pieces that meet in the
middle: `NavigationView`, a templated rail that draws a list of `NavigationItem` entries and reports
which one is selected, and `INavigationService`, the singleton that owns those entries, builds the
page for the selected one and publishes it as `CurrentPage`. Bind a `ContentControl` to `CurrentPage`
and the shell is complete.

Pages are never constructed by the library itself. `INavigationService.PageFactory` is a
`Func<NavigationItem, Control>` mapping an entry to a ready-to-display `Control` with its
`DataContext` already set. The default factory calls `Activator.CreateInstance` on the entry's
`PageType` and `PageViewModelType`; replacing it is how you resolve both halves from a DI container.

Navigation is serialized by a single `SemaphoreSlim` taken with a **zero** timeout, so a navigation
raised while another is still awaiting a lifecycle callback is *dropped*, not queued behind it — and
nothing here ever throws at the caller. A page factory that blows up, or a lifecycle callback that
throws, is reported on the `NavigationFailed` event instead.

## Operations

| Operation | Member | Notes |
|-----------|--------|-------|
| Show the page for a rail entry | `INavigationService.SelectedItem` | Fire-and-forget — the setter starts the navigation and returns. Not awaitable. |
| Show any `Control` | `INavigationService.NavigateToAsync` | Awaitable. Takes a page whose `DataContext` you set, plus an optional parameter. |
| Read the page on screen | `INavigationService.CurrentPage` | Read-only through the interface. Raises `PropertyChanged`; bind a `ContentControl` to it. |
| Populate the rail | `INavigationService.Items` / `FooterItems` | `ObservableCollection<NavigationItem>`. Footer entries are pinned to the far end of the rail. |
| Build a page from an entry | `INavigationService.PageFactory` | Assign your own `Func<NavigationItem, Control>` to resolve pages from a container. |
| Run code on arrival, veto a departure | `INavigationViewModel` | Implemented by the page's `DataContext`. Only the `DataContext` is inspected, never the `Control`. |
| Observe failures | `INavigationService.NavigationFailed` | Carries the exception and the `Phase` string it came from. |

The two controls carry presentation state only — no routing, no page construction:

| Property | Type | Default | Role |
|----------|------|---------|------|
| `NavigationView.Items` | `IReadOnlyList<NavigationItem>?` | `null` | Main entry list. Bind it to `INavigationService.Items`. |
| `NavigationView.FooterItems` | `IReadOnlyList<NavigationItem>?` | `null` | Entries pinned to the bottom, or to the right when horizontal. |
| `NavigationView.SelectedItem` | `NavigationItem?` | `null` | Selection across *both* lists — picking in one clears the other. Binds two-way by default. |
| `NavigationView.Logo` | `object?` | `null` | Free-form content before the entries; collapsed while `null`. |
| `NavigationView.Orientation` | `NavigationOrientation` | `Vertical` | `Vertical` for a side rail, `Horizontal` for a top or bottom bar. |
| `NavigationView.PaneSize` | `double` | `90` | Cross-axis extent: the rail's `Width` when vertical, its `MaxHeight` when horizontal. |
| `NavigationItem.Header` | `string?` | `null` | The label under the icon. Wraps on whitespace. |
| `NavigationItem.IconData` | `Geometry?` | `null` | Path geometry drawn at 24×24 through a `PathIcon`. |
| `NavigationItem.PageType` | `Type` | `null` | The page's View type. Also what `NavigateToAsync` matches a `Control` against. |
| `NavigationItem.PageViewModelType` | `Type` | `null` | The page's ViewModel type, for the factory to resolve. |
| `NavigationItem.LabelMaxWidth` | `double` | `72` | Caps the label width in horizontal orientation only. |

## Key types

| Type | Namespace | Role |
|------|-----------|------|
| `NavigationView` | `Enigma.Avalonia.Desktop.Controls.Navigation` | The rail control. A `TemplatedControl`, not an `ItemsControl`. |
| `NavigationItem` | `Enigma.Avalonia.Desktop.Controls.Navigation` | One entry. Also a `TemplatedControl` — an entry is a control, not a data object. |
| `NavigationOrientation` | `Enigma.Avalonia.Desktop.Controls.Navigation` | `Vertical` or `Horizontal`. |
| `INavigationService` | `Enigma.Avalonia.Desktop.Services` | The navigation surface. Registered as a singleton; inject it. |
| `NavigationService` | `Enigma.Avalonia.Desktop.Services` | The implementation. An `ObservableObject`, so `CurrentPage` and `SelectedItem` are bindable. |
| `INavigationViewModel` | `Enigma.Avalonia.Desktop.Services` | Optional page lifecycle: `OnAppearingAsync` and `OnDisappearingAsync`. |
| `NavigationFailedEventArgs` | `Enigma.Avalonia.Desktop.Services` | The `Exception`, plus the `Phase` it was thrown in. |

Because `NavigationItem` is a control rather than a data object it carries no payload of its own:
there is no tag, no route string and no parameter. An entry identifies a page by `PageType`, and
per-navigation data can only be supplied through `NavigateToAsync`.

## Usage

### Binding the rail to the service

Bind `Items`, `FooterItems` and `SelectedItem` to the service, and a `ContentControl` to
`CurrentPage`. `SelectedItemProperty` registers `BindingMode.TwoWay` as its default, so no explicit
`Mode` is needed for the rail to drive the service.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:nav="using:Enigma.Avalonia.Desktop.Controls.Navigation"
        xmlns:vm="using:MyApp.ViewModels"
        x:Class="MyApp.Views.MainWindow"
        x:DataType="vm:MainWindowViewModel"
        Title="My App">

  <DockPanel>
    <nav:NavigationView DockPanel.Dock="Left"
                        Items="{Binding Navigation.Items}"
                        FooterItems="{Binding Navigation.FooterItems}"
                        SelectedItem="{Binding Navigation.SelectedItem}"
                        Orientation="Vertical"
                        PaneSize="90">
      <nav:NavigationView.Logo>
        <PathIcon Data="M12,2L2,7L12,12L22,7L12,2Z"
                  Width="24"
                  Height="24"
                  Foreground="{DynamicResource EnigmaAccentBrush}" />
      </nav:NavigationView.Logo>
    </nav:NavigationView>

    <ContentControl Content="{Binding Navigation.CurrentPage}" />
  </DockPanel>

</Window>
```

The window's ViewModel owns the page catalogue — the rail is the shell, not a page. Under the default
`PageFactory` both `PageType` and `PageViewModelType` must be concrete types with a public
parameterless constructor.

```csharp
using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Enigma.Avalonia.Desktop.Controls.Navigation;
using Enigma.Avalonia.Desktop.Services;
using MyApp.Views;

namespace MyApp.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(INavigationService navigation)
    {
        Navigation = navigation;

        Navigation.Items.Add(CreateItem(
            "Home", "M12,3L2,12H5V20H10V14H14V20H19V12H22L12,3Z",
            typeof(HomePageView), typeof(HomePageViewModel)));
        Navigation.Items.Add(CreateItem(
            "Reports", "M4,6H20V8H4V6M4,11H20V13H4V11M4,16H14V18H4V16Z",
            typeof(ReportsPageView), typeof(ReportsPageViewModel)));
        Navigation.FooterItems.Add(CreateItem(
            "Settings", "M12,2L22,12L12,22L2,12L12,2Z",
            typeof(SettingsPageView), typeof(SettingsPageViewModel)));

        // Assigning SelectedItem runs the factory and highlights the entry in one step.
        Navigation.SelectedItem = Navigation.Items[0];
    }

    public INavigationService Navigation { get; }

    private static NavigationItem CreateItem(string header, string icon, Type page, Type viewModel) =>
        new()
        {
            Header = header,
            IconData = Geometry.Parse(icon),
            PageType = page,
            PageViewModelType = viewModel,
        };
}
```

### Resolving pages from the container

Replace `PageFactory` to build pages through DI. `PageType` and `PageViewModelType` then act as
container keys rather than `Activator` arguments, so both halves may have constructor dependencies.

```csharp
using System;
using Avalonia.Controls;
using Enigma.Avalonia.Desktop.Controls.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

public sealed class ContainerPageFactory(IServiceProvider services)
{
    public Control Create(NavigationItem item)
    {
        if (services.GetRequiredService(item.PageType) is not Control page)
            throw new InvalidOperationException($"Page type {item.PageType} is not a Control.");

        page.DataContext = services.GetRequiredService(item.PageViewModelType);
        return page;
    }
}
```

Assign it before the first navigation, otherwise the default factory runs instead:
`Navigation.PageFactory = new ContainerPageFactory(services).Create;`. Registering Views transient and
ViewModels singleton is a useful default — returning to a page then builds a fresh visual tree but
re-attaches the ViewModel it had before, so page state survives leaving it.

### Navigating from code

`NavigateToAsync` takes any `Control`, listed on the rail or not, and is the only way to pass a
parameter. Once the page is set the service looks for a `NavigationItem` whose `PageType` matches the
`Control`'s runtime type — searching `Items` first, then `FooterItems` — and selects it. When nothing
matches, the rail's selection is cleared rather than left stale.

```csharp
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Views;

namespace MyApp.ViewModels;

public class ReportsPageViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private readonly INavigationService _navigation;

    public ReportsPageViewModel(IServiceProvider services, INavigationService navigation)
    {
        _services = services;
        _navigation = navigation;
        OpenSelectedCommand = new AsyncRelayCommand(OpenSelectedAsync);
    }

    public IAsyncRelayCommand OpenSelectedCommand { get; }

    public int SelectedReportId { get; set => SetProperty(ref field, value); }

    private Task OpenSelectedAsync()
    {
        var page = _services.GetRequiredService<ReportDetailPageView>();
        page.DataContext = _services.GetRequiredService<ReportDetailPageViewModel>();

        // The parameter reaches the target's OnAppearingAsync — and only through this call.
        return _navigation.NavigateToAsync(page, SelectedReportId);
    }
}
```

### Page lifecycle, and refusing to leave

A page's `DataContext` may implement `INavigationViewModel`. `OnDisappearingAsync` is awaited
**before** `CurrentPage` changes and returning `false` cancels the navigation; `OnAppearingAsync` runs
**after** the page is on screen. The interface is optional — a page whose `DataContext` ignores it
navigates with no callbacks at all. Because the guard is awaited it can just as well prompt: show a
dialog and return the user's answer.

```csharp
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp.ViewModels;

public class ReportDetailPageViewModel : ObservableObject, INavigationViewModel
{
    public int ReportId { get; private set => SetProperty(ref field, value); }

    public bool HasUnsavedChanges { get; set => SetProperty(ref field, value); }

    // parameter is whatever NavigateToAsync was given, and null when the rail drove the navigation.
    public Task OnAppearingAsync(object? parameter = null)
    {
        ReportId = parameter is int id ? id : 0;
        HasUnsavedChanges = false;
        return Task.CompletedTask;
    }

    // false cancels the navigation; a rail-driven one also rolls its selection back.
    public Task<bool> OnDisappearingAsync() => Task.FromResult(!HasUnsavedChanges);
}
```

### Reacting to failures

Nothing on `INavigationService` throws. Subscribe to `NavigationFailed` and branch on `Phase`, which
is one of four literals. The service is a singleton, so a handler attached at startup lives as long
as the app; detach it if you attach from something shorter-lived.

```csharp
using System;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp;

public static class NavigationDiagnostics
{
    public static void Attach(INavigationService navigation) =>
        navigation.NavigationFailed += (_, e) => Console.Error.WriteLine(e.Phase switch
        {
            // The factory threw; CurrentPage was cleared to null.
            "PageFactory" => $"The page could not be built: {e.Exception}",
            // The page is on screen; only its initialisation failed.
            "OnAppearingAsync" => $"The page failed to initialise: {e.Exception}",
            // A broken guard counts as consent, so the navigation went ahead.
            "OnDisappearingAsync" => $"A page guard threw: {e.Exception}",
            // The rail-driven navigation itself faulted.
            "TryNavigateToItemAsync" => $"The item navigation faulted: {e.Exception}",
            _ => $"Navigation failed in phase '{e.Phase}': {e.Exception}",
        });
}
```

### A horizontal bar with entries declared in XAML

For a shell whose pages are fixed at compile time the entries can live in XAML. `Items` and
`FooterItems` are typed `IReadOnlyList<NavigationItem>`, which is not instantiable, so the list must
be spelled out — an implicit child collection does not compile. `FooterItems` takes the same shape.
Keep `SelectedItem` bound to the service so picking an entry still runs the page factory;
`ShellViewModel` here is nothing but the injected `INavigationService` exposed as `Navigation`.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:gen="using:System.Collections.Generic"
             xmlns:nav="using:Enigma.Avalonia.Desktop.Controls.Navigation"
             xmlns:views="using:MyApp.Views"
             xmlns:vm="using:MyApp.ViewModels"
             x:Class="MyApp.Views.TopBarShell"
             x:DataType="vm:ShellViewModel">

  <DockPanel>
    <nav:NavigationView DockPanel.Dock="Top"
                        Orientation="Horizontal"
                        PaneSize="72"
                        SelectedItem="{Binding Navigation.SelectedItem}">
      <nav:NavigationView.Items>
        <gen:List x:TypeArguments="nav:NavigationItem">
          <nav:NavigationItem Header="Home"
                              IconData="M12,3L2,12H5V20H10V14H14V20H19V12H22L12,3Z"
                              PageType="{x:Type views:HomePageView}"
                              PageViewModelType="{x:Type vm:HomePageViewModel}"
                              LabelMaxWidth="96" />
          <nav:NavigationItem Header="Reports"
                              IconData="M4,6H20V8H4V6M4,11H20V13H4V11M4,16H14V18H4V16Z"
                              PageType="{x:Type views:ReportsPageView}"
                              PageViewModelType="{x:Type vm:ReportsPageViewModel}"
                              LabelMaxWidth="96" />
        </gen:List>
      </nav:NavigationView.Items>
    </nav:NavigationView>

    <ContentControl Content="{Binding Navigation.CurrentPage}" />
  </DockPanel>

</UserControl>
```

Entries declared this way are unknown to the service, so `NavigateToAsync` finds no match for them and
clears the selection instead of highlighting the page it just opened. Put the entries in
`INavigationService.Items` whenever both entry points are used.

## Notes

- Setting `SelectedItem` is fire-and-forget: the setter returns immediately and the navigation runs
  unawaited. `NavigateToAsync` is the awaitable path, and the only one that takes a parameter.
- The navigation lock uses a zero timeout, so a second navigation raised while one is still awaiting a
  lifecycle callback is dropped outright. `NavigateToAsync` returns a completed task in that case; it
  does not report that it did nothing. Assigning `SelectedItem` mid-flight is worse — the property
  change sticks and the rail highlights the new entry, but the page never changes. Gate rapid
  re-entry with `CanExecute` rather than relying on the service.
- The two paths differ on cancellation. When `OnDisappearingAsync` returns `false` for a rail-driven
  navigation the previous `SelectedItem` is restored, so the rail never highlights a page that was not
  shown; `NavigateToAsync` has no selection to roll back and returns with `CurrentPage` unchanged. A
  *throwing* `OnDisappearingAsync` is reported and then treated as consent — a broken guard must not
  trap the user on a page, so only an explicit `false` cancels.
- `PageFactory` failures clear `CurrentPage` to `null`, so the content area empties rather than
  keeping a page the rail no longer points at. The default factory throws `ArgumentNullException`
  when an entry's `PageType` was never assigned. Setting `SelectedItem` to `null` clears `CurrentPage`
  without invoking the factory at all.
- `NavigationFailed` for the `PageFactory`, `OnAppearingAsync` and `OnDisappearingAsync` phases is
  raised on the UI thread. The `TryNavigateToItemAsync` phase comes from a task continuation and may
  not be; marshal through `Dispatcher.UIThread` before touching UI from that branch.
- The template exposes `PART_Border`, `PART_Logo`, `PART_ItemsListBox` and `PART_FooterListBox`, and
  both the rail and every entry it owns carry a `:vertical` or `:horizontal` pseudo-class. Entries sit
  in plain `ListBox`es, so `ListBoxItem` selectors reach them; see [theming](theming.md) for the brush
  keys. Those pseudo-classes are stamped on when the template is applied and whenever `Orientation`
  changes — not when the collections change, so populate `Items` and `FooterItems` before the rail is
  shown.
- `NavigationItem` is a control, so an instance belongs to exactly one rail: never share one between
  two `NavigationView`s, or move one between `Items` and `FooterItems` at runtime.
