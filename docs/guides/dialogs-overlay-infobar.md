# Dialogs, overlay and info bar

`Enigma.Avalonia.Desktop` ships three overlay surfaces — a modal `ContentDialog`, a blocking
`Overlay`, and a top-anchored `InfoBar` — and one service per surface. The controls are ordinary
templated controls you can drive by hand, but the intended use is the service: a ViewModel takes
`IContentDialogService`, `IOverlayService` or `IInfoBarService` from the container, calls `ShowAsync`,
and awaits the answer without ever holding a reference to a `Window`.

A service never *creates* its surface. It drives exactly one host control that you place in the
window's XAML and hand over with `RegisterHost` at startup. **This wiring is the whole contract for
this family.** Each host must be a *last* sibling of the window's root `Panel`, so it is drawn over
everything else, and each `RegisterHost` must run before the window is shown — a service whose host
is still unregistered throws `InvalidOperationException` from every `Show*` call.

`ContentDialog` and `InfoBar` complete their `ShowAsync` when the surface is *dismissed*, not when it
appears. That is the point: a decision reads as an awaited return value instead of a callback, and a
long-running operation can keep mutating live content while the surface stays up.

## Surfaces

| Surface | Host control | Service | Opens with | Closes with |
|---------|--------------|---------|------------|-------------|
| Modal dialog | `ContentDialog` | `IContentDialogService` | `ShowMessageAsync`, `ShowAsync` | one of its three buttons, `Escape`, a click on the scrim, or `HideAsync` |
| Blocking overlay | `Overlay` | `IOverlayService` | `ShowAsync(Control)` | `HideAsync` only — the user cannot dismiss it |
| Inline notification | `InfoBar` | `IInfoBarService` | `ShowAsync(Action<InfoBar>?)` | its close button, or `HideAsync` |

Every service member, in full:

| Service | Member | Returns |
|---------|--------|---------|
| `IContentDialogService` | `RegisterHost(ContentDialog dialog)` | `void` |
| | `ShowMessageAsync(string title, string message, string closeButtonText = "OK")` | `Task<DialogResult>` |
| | `ShowAsync(Action<ContentDialog> configure)` | `Task<DialogResult>` |
| | `HideAsync()` | `Task` — resolves the pending dialog with `DialogResult.None` |
| `IOverlayService` | `RegisterHost(Overlay presenter)` | `void` |
| | `ShowAsync(Control control)` | `Task` — already completed when it returns |
| | `HideAsync()` | `Task` — already completed; also clears `Overlay.Content` |
| `IInfoBarService` | `RegisterHost(InfoBar infoBar)` | `void` |
| | `ShowAsync(Action<InfoBar>? configure = null)` | `Task` — completes when the bar is dismissed |
| | `HideAsync()` | `Task` |

`Show*` is strict about the host, `HideAsync` is forgiving. With no host registered every `Show*`
throws `InvalidOperationException` — "ContentDialog host has not been registered. Call RegisterHost
first.", "No Overlay registered. Call RegisterHost first.", "InfoBar host has not been registered.
Call RegisterHost first." — while all three `HideAsync` methods are silent no-ops, because there is
nothing to close. `IOverlayService.ShowAsync(null!)` throws `ArgumentNullException` with `ParamName`
`"control"`, but only once a host exists: the host check runs first. Calling `RegisterHost` twice
replaces the host rather than stacking one on another.

## Key types

| Type | Namespace | Role |
|------|-----------|------|
| `ContentDialog` | `Enigma.Avalonia.Desktop.Controls.ContentDialog` | Modal dialog host. A `ContentControl`. |
| `DefaultButton` | `Enigma.Avalonia.Desktop.Controls.ContentDialog` | Enum: `None`, `Primary`, `Secondary`, `Close`. |
| `DialogResult` | `Enigma.Avalonia.Desktop.Controls.ContentDialog` | Enum: `None`, `Primary`, `Secondary`, `Close`. |
| `Overlay` | `Enigma.Avalonia.Desktop.Controls` | Full-window scrim hosting arbitrary content. A `ContentControl`. |
| `InfoBar` | `Enigma.Avalonia.Desktop.Controls.InfoBar` | Inline notification host. A `ContentControl`. |
| `InfoBarSeverity` | `Enigma.Avalonia.Desktop.Controls.InfoBar` | Enum: `Info`, `Success`, `Warning`, `Error`. Selects the glyph and the background/border pair. |
| `IContentDialogService` / `ContentDialogService` | `Enigma.Avalonia.Desktop.Services` | Dialog service and its implementation. Singleton. |
| `IOverlayService` / `OverlayService` | `Enigma.Avalonia.Desktop.Services` | Overlay service and its implementation. Singleton. |
| `IInfoBarService` / `InfoBarService` | `Enigma.Avalonia.Desktop.Services` | Info bar service and its implementation. Singleton. |

`ContentDialog` carries the whole dialog surface as styled properties:

| Property | Default | Effect |
|----------|---------|--------|
| `Title` | `null` | Heading row; hidden when null or empty. |
| `Content` | `null` | Card body — a `string`, a `Control`, or anything `ContentTemplate` renders. Scrolls vertically inside the card. |
| `IconData` | `null` | 48×48 `Geometry` left of the content; the slot is hidden when null. |
| `IconBrush` | `EnigmaForegroundBrush` | Fill for `IconData`. |
| `PrimaryButtonText`, `SecondaryButtonText`, `CloseButtonText` | `null` | Each button is visible only while its text is non-empty. The primary button carries the `accent` class. |
| `IsPrimaryButtonEnabled`, `IsSecondaryButtonEnabled`, `IsCloseButtonEnabled` | `true` | Per-button enablement. |
| `PrimaryButtonCommand`, `SecondaryButtonCommand`, `CloseButtonCommand` | `null` | Executed with a `null` parameter on click, *before* the dialog closes. |
| `DefaultButton` | `DefaultButton.None` | Declares intent only — see Notes. |
| `DialogResult` | `DialogResult.None` | Set to the closing button just before `Closed` fires. |
| `IsOpen` | `false` | Drives visibility; `ShowAsync` sets it. |
| `OverlayBrush` | `#4D000000` | The scrim behind the card. Clicking it closes with `DialogResult.None`. |
| `DialogWidth`, `DialogHeight` | `double.NaN` | Explicit card size; `NaN` auto-sizes within the bounds below. |
| `DialogMinWidth`, `DialogMaxWidth` | `320`, `600` | Width bounds. Raise the max for wide content. |
| `DialogMinHeight`, `DialogMaxHeight` | `0`, `double.PositiveInfinity` | Height bounds. Set the max to make tall content scroll inside the card. |

`InfoBar` exposes `Title`, `Message`, `Severity` (default `InfoBarSeverity.Info`) and `IsOpen`.
`Overlay` exposes only `IsOpen` and `OverlayBrush` (same `#4D000000` default) plus the inherited
`Content`. Every brush key these controls resolve is documented in [theming](theming.md).

The services are thin wrappers over the controls' own members, which are equally usable on a
page-local host that nothing registers:

| Control | Member | Behaviour |
|---------|--------|-----------|
| `ContentDialog` | `Task<DialogResult> ShowAsync()` | Sets `IsOpen`; completes when the dialog closes. |
| | `Task HideAsync()` | Closes with `DialogResult.None`; already completed if the dialog was not open. |
| | `event EventHandler<DialogResult>? Closed` | Raised after `DialogResult` and `IsOpen` are updated. |
| `InfoBar` | `Task ShowAsync()` | Sets `IsOpen`; completes when the bar is dismissed. |
| | `void Close()` | Clears `IsOpen` and raises `Closed`. |
| | `Task CloseAsync()` | Calls `Close()` and returns an already-completed task. |
| | `event EventHandler? Closed` | Raised by the close button and by `Close()`. |
| `Overlay` | — | No methods at all: assign `Content`, then set `IsOpen`. That is exactly what `OverlayService` does. |

## Usage

### Placing and registering the three hosts

The hosts go in the window's root `Panel`, after everything else: a `Panel` stacks its children in
declaration order, so being last is what puts them on top. `InfoBar` is declared after the other two
so a notification stays readable over an open dialog. The six `Dialog*` size properties belong here
too rather than in a `configure` action — the service does not reset them between dialogs.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MyApp.ViewModels"
        xmlns:controls="using:Enigma.Avalonia.Desktop.Controls"
        xmlns:contentDialog="using:Enigma.Avalonia.Desktop.Controls.ContentDialog"
        xmlns:infoBar="using:Enigma.Avalonia.Desktop.Controls.InfoBar"
        x:Class="MyApp.Views.MainWindow"
        x:DataType="vm:MainWindowViewModel"
        Title="{Binding Title}"
        Width="1280"
        Height="800"
        Background="{DynamicResource EnigmaBackgroundBrush}">

  <!-- The root is a Panel, not the application's own layout container: the three hosts have to be
       the LAST children of the window's outermost container so they overlay everything it draws. -->
  <Panel>
    <DockPanel>
      <ContentControl Content="{Binding CurrentPage}" />
    </DockPanel>

    <contentDialog:ContentDialog x:Name="HostDialog"
                                 DialogMinWidth="360"
                                 DialogMaxWidth="900"
                                 DialogMaxHeight="520"
                                 OverlayBrush="{DynamicResource EnigmaOverlayBrush}" />
    <controls:Overlay x:Name="HostOverlay"
                      OverlayBrush="{DynamicResource EnigmaOverlayBrush}" />
    <infoBar:InfoBar x:Name="HostInfoBar" />
  </Panel>

</Window>
```

`x:Name` gives each host an assembly-internal field on the generated partial class, so the
composition root can reach it. Register all three there, before assigning `desktop.MainWindow`.

```csharp
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Enigma.Avalonia.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using MyApp.ViewModels;
using MyApp.Views;

namespace MyApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // The README quick start registers all six services; these three need a host.
            var collection = new ServiceCollection();
            collection.AddSingleton<IContentDialogService, ContentDialogService>();
            collection.AddSingleton<IOverlayService, OverlayService>();
            collection.AddSingleton<IInfoBarService, InfoBarService>();
            collection.AddSingleton<MainWindowViewModel>();
            collection.AddSingleton<MainWindow>();

            IServiceProvider services = collection.BuildServiceProvider();
            var mainWindow = services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = services.GetRequiredService<MainWindowViewModel>();

            // Before the window is handed over: a service whose host is still unregistered throws
            // the moment a page asks it for a dialog, an overlay or a notification.
            services.GetRequiredService<IContentDialogService>().RegisterHost(mainWindow.HostDialog);
            services.GetRequiredService<IOverlayService>().RegisterHost(mainWindow.HostOverlay);
            services.GetRequiredService<IInfoBarService>().RegisterHost(mainWindow.HostInfoBar);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

### Showing dialogs

`ShowMessageAsync` is the one-liner: it sets `Title`, wraps `message` in a `TextBlock` with
`TextWrapping.Wrap` as the `Content`, and shows a single close button.
`ShowAsync(Action<ContentDialog>)` hands you the host to configure in place, after resetting it so
nothing a previous dialog set leaks in. Each button reports its own `DialogResult`; `Escape` and a
scrim click both report `DialogResult.None`.

```csharp
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp.ViewModels;

public class ProjectViewModel : ObservableObject
{
    private const string WarningGlyph = "M1 21h22L12 2 1 21Zm12-3h-2v-2h2v2Zm0-4h-2v-4h2v4Z";

    private readonly IContentDialogService _dialogService;

    public ProjectViewModel(IContentDialogService dialogService)
    {
        _dialogService = dialogService;
        AboutCommand = new AsyncRelayCommand(OnAboutAsync);
        DeleteCommand = new AsyncRelayCommand(OnDeleteAsync);
    }

    public IAsyncRelayCommand AboutCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    public string? Status { get; set => SetProperty(ref field, value); }

    private async Task OnAboutAsync()
    {
        DialogResult result = await _dialogService.ShowMessageAsync(
            "About", "Build 1.0.0. Everything is fine.", closeButtonText: "Close");

        Status = $"About closed with {result}.";
    }

    private async Task OnDeleteAsync()
    {
        // A live control, kept in a local so its state can be read back after the dialog closes.
        var alsoBackups = new CheckBox { Content = "Also delete the backups" };

        DialogResult result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Delete project";
            dialog.IconData = Geometry.Parse(WarningGlyph);

            // Safe to bind on a shared host: the service calls ClearValue on IconBrush every time.
            dialog[!ContentDialog.IconBrushProperty] = new DynamicResourceExtension("EnigmaWarningBrush");

            dialog.Content = new StackPanel
            {
                Spacing = 8,
                Children = { new TextBlock { Text = "This cannot be undone." }, alsoBackups },
            };

            dialog.PrimaryButtonText = "Delete";
            dialog.SecondaryButtonText = "Archive instead";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = DefaultButton.Close;
        });

        Status = result switch
        {
            DialogResult.Primary => $"Deleted (backups: {alsoBackups.IsChecked == true}).",
            DialogResult.Secondary => "Archived.",
            DialogResult.Close => "Cancelled.",
            _ => "Dismissed.",
        };
    }
}
```

### An overlay around a long operation, and an info bar for the outcome

`IOverlayService.ShowAsync` takes any `Control` — a panel built in code, or your own templated
control — and returns immediately, because the user has no way to dismiss an overlay. The control
stays live while the overlay holds it, so the operation reports progress by mutating it in place and
calls `HideAsync` from a `finally` so a fault cannot leave the UI blocked. `IInfoBarService.ShowAsync`
is the opposite: it completes only once the bar is dismissed, so keep the `Task` and await it where
it suits you.

```csharp
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Enigma.Avalonia.Desktop.Services;

namespace MyApp.ViewModels;

public class ImportViewModel : ObservableObject
{
    private readonly IOverlayService _overlayService;
    private readonly IInfoBarService _infoBarService;

    public ImportViewModel(IOverlayService overlayService, IInfoBarService infoBarService)
    {
        _overlayService = overlayService;
        _infoBarService = infoBarService;
        ImportCommand = new AsyncRelayCommand(OnImportAsync);

        // Dismissing the bar from code rather than from its close button.
        DismissCommand = new AsyncRelayCommand(() => _infoBarService.HideAsync());
    }

    public IAsyncRelayCommand ImportCommand { get; }

    public IAsyncRelayCommand DismissCommand { get; }

    private async Task OnImportAsync()
    {
        var message = new TextBlock { Text = "Connecting…", TextWrapping = TextWrapping.Wrap };
        var progress = new ProgressBar { Minimum = 0, Maximum = 100, IsIndeterminate = true };
        var card = new StackPanel { Width = 360, Spacing = 12, Children = { message, progress } };

        try
        {
            await _overlayService.ShowAsync(card);

            for (var step = 1; step <= 4; step++)
            {
                progress.IsIndeterminate = false;
                progress.Value = step * 25;
                message.Text = $"Importing record set {step} of 4…";
                await Task.Delay(500);
            }
        }
        finally
        {
            await _overlayService.HideAsync();
        }

        // Deliberately not awaited: the command returns while the bar waits to be dismissed.
        Task dismissed = _infoBarService.ShowAsync(bar =>
        {
            bar.Title = "Import complete";
            bar.Message = "4 record sets were imported.";
            bar.Severity = InfoBarSeverity.Success;
        });

        // Give the user five seconds to read it, then take it down if it is still up.
        await Task.WhenAny(dismissed, Task.Delay(5000));
        await _infoBarService.HideAsync();
    }
}
```

## Notes

- `RegisterHost` is not optional and is not lazy. Register at startup, before the window is shown; a
  missing host surfaces as an `InvalidOperationException` from the first `Show*` call, at whatever
  arbitrary moment a page happens to raise a dialog.
- Registering a host a second time replaces the first. Each service holds one host, not a stack, so a
  second window cannot share the same singleton service and expect both to work.
- `DefaultButton` currently records intent only: the bundled theme has no selector for it and the
  control performs no focus assignment, so the primary button carries the accent styling regardless
  and there is no Enter-to-commit. Set it to express intent, but do not rely on it for visuals.
- `Escape` closes the dialog only while focus is inside it — the handler is on the control and
  `KeyDown` bubbles, and neither the control nor the theme moves focus into the card when it opens.
  Give a control in your `Content` initial focus if the shortcut has to work immediately.
- A click on the scrim closes with `DialogResult.None`. There is no must-choose mode; treat
  `DialogResult.None` as a cancel.
- `ContentDialogService.ShowAsync` resets the title, content, all three button texts, all three
  button commands, all three enabled flags, `DefaultButton` and `IconData`, and calls `ClearValue` on
  `IconBrush`. It does **not** reset the six `Dialog*` size properties, `OverlayBrush` or
  `DialogResult` — put those on the host once, in XAML.
- `InfoBarService.ShowAsync` resets `Title`, `Message` and `Severity` only. `InfoBar` is a
  `ContentControl`, but its template renders no `ContentPresenter`: setting `Content` has no visual
  effect, so use `Title` and `Message`.
- There is no auto-dismiss timer on `InfoBar`; it stays up until its close button, `Close()`,
  `CloseAsync()` or the service's `HideAsync()` closes it. The bar is `Top`-aligned and stretches the
  width of its container regardless of where it sits in the panel.
- `Overlay` cannot be dismissed by the user — no `Escape` handling, no scrim click. Always pair
  `ShowAsync` with a `HideAsync` in a `finally`. Its content is centred, not stretched.
- `IOverlayService.HideAsync` clears `Overlay.Content`, so the overlay holds no reference to the
  control afterwards. Keep your own reference if you intend to show it again.
- Both scrims default to `#4D000000`, and it is the brush that makes the surface hit-testable.
  Setting `OverlayBrush` to `null` lets pointer input fall through to the application beneath; use a
  fully transparent brush instead if you want an invisible but still modal surface.
