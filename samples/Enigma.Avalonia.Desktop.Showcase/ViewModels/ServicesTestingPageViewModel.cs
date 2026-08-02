using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Enigma.Avalonia.Desktop.Services;
using Enigma.Avalonia.Desktop.Showcase.Controls;
using Enigma.Icons.Avalonia;
using Enigma.Icons.Phosphor;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the services page: the content dialog, the overlay and the info bar.</summary>
/// <remarks>
/// <para>
/// All three services follow the same shape — the window registered a host control at startup, and
/// from here a ViewModel just calls <c>ShowAsync</c> and awaits it. The await is the useful part: it
/// completes when the user dismisses the surface, so a decision reads as a return value rather than a
/// callback.
/// </para>
/// <para>
/// Dialog content is built in C# here because that is what <c>IContentDialogService.ShowAsync</c>
/// takes — an <c>Action&lt;ContentDialog&gt;</c> configuring the host in place, so anything assignable
/// to <c>Content</c> works, from a string to a panel to a consumer's own templated control (see
/// <see cref="ProgressOverlayCard"/>, which the overlay commands use). Every brush comes from the
/// theme as a dynamic resource, never a literal colour, so content built in code follows a runtime
/// theme switch exactly like content declared in XAML.
/// </para>
/// </remarks>
public class ServicesTestingPageViewModel : ObservableObject
{
    /// <summary>The content dialog service.</summary>
    private readonly IContentDialogService _dialogService;

    /// <summary>The overlay service.</summary>
    private readonly IOverlayService _overlayService;

    /// <summary>The info bar service.</summary>
    private readonly IInfoBarService _infoBarService;

    /// <summary>Initializes a new instance of the <see cref="ServicesTestingPageViewModel"/> class.</summary>
    /// <param name="dialogService">The content dialog service.</param>
    /// <param name="overlayService">The overlay service.</param>
    /// <param name="infoBarService">The info bar service.</param>
    public ServicesTestingPageViewModel(
        IContentDialogService dialogService,
        IOverlayService overlayService,
        IInfoBarService infoBarService)
    {
        _dialogService = dialogService;
        _overlayService = overlayService;
        _infoBarService = infoBarService;

        ShowSimpleDialogCommand = new AsyncRelayCommand(OnShowSimpleDialogAsync);
        ShowComplexDialogCommand = new AsyncRelayCommand(OnShowComplexDialogAsync);
        ShowPasswordDialogCommand = new AsyncRelayCommand(OnShowPasswordDialogAsync);
        ShowWideDialogCommand = new AsyncRelayCommand(OnShowWideDialogAsync);
        ShowTallDialogCommand = new AsyncRelayCommand(OnShowTallDialogAsync);

        RunSimpleTaskCommand = new AsyncRelayCommand(OnRunSimpleTaskAsync, CanRunTask);
        RunComplexTaskCommand = new AsyncRelayCommand(OnRunComplexTaskAsync, CanRunTask);

        ShowInfoCommand = new AsyncRelayCommand(OnShowInfoAsync);
        ShowSuccessCommand = new AsyncRelayCommand(OnShowSuccessAsync);
        ShowWarningCommand = new AsyncRelayCommand(OnShowWarningAsync);
        ShowErrorCommand = new AsyncRelayCommand(OnShowErrorAsync);
        CloseInfoBarCommand = new AsyncRelayCommand(OnCloseInfoBarAsync);
    }

    /// <summary>Gets or sets what the last dialog returned.</summary>
    public string? LastDialogResult { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets what the last overlay-hosted task reported.</summary>
    public string? LastOverlayResult { get; set => SetProperty(ref field, value); }

    /// <summary>Gets or sets what the last info bar reported.</summary>
    public string? LastInfoBarResult { get; set => SetProperty(ref field, value); }

    /// <summary>Gets a value indicating whether an overlay-hosted task is running.</summary>
    /// <remarks>
    /// The overlay already blocks input, so this exists for the commands' <c>CanExecute</c>: both task
    /// buttons disable while either task runs, which is what the visual state should show.
    /// </remarks>
    public bool IsBusy
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value)) return;
            RunSimpleTaskCommand.NotifyCanExecuteChanged();
            RunComplexTaskCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>Gets the command showing a message dialog with a single button.</summary>
    public AsyncRelayCommand ShowSimpleDialogCommand { get; }

    /// <summary>Gets the command showing a three-button dialog with an icon and composed content.</summary>
    public AsyncRelayCommand ShowComplexDialogCommand { get; }

    /// <summary>Gets the command showing a dialog that collects input.</summary>
    public AsyncRelayCommand ShowPasswordDialogCommand { get; }

    /// <summary>Gets the command showing a dialog wider than the default cap.</summary>
    public AsyncRelayCommand ShowWideDialogCommand { get; }

    /// <summary>Gets the command showing a dialog whose content scrolls inside the card.</summary>
    public AsyncRelayCommand ShowTallDialogCommand { get; }

    /// <summary>Gets the command running a task behind a progress overlay.</summary>
    public AsyncRelayCommand RunSimpleTaskCommand { get; }

    /// <summary>Gets the command running a multi-step task behind a progress overlay.</summary>
    public AsyncRelayCommand RunComplexTaskCommand { get; }

    /// <summary>Gets the command showing an informational info bar.</summary>
    public AsyncRelayCommand ShowInfoCommand { get; }

    /// <summary>Gets the command showing a success info bar.</summary>
    public AsyncRelayCommand ShowSuccessCommand { get; }

    /// <summary>Gets the command showing a warning info bar.</summary>
    public AsyncRelayCommand ShowWarningCommand { get; }

    /// <summary>Gets the command showing an error info bar.</summary>
    public AsyncRelayCommand ShowErrorCommand { get; }

    /// <summary>Gets the command dismissing the current info bar from code.</summary>
    public AsyncRelayCommand CloseInfoBarCommand { get; }

    /// <summary>Determines whether an overlay-hosted task may start.</summary>
    /// <returns><see langword="true"/> when no task is running.</returns>
    private bool CanRunTask() => !IsBusy;

    /// <summary>Shows a message dialog: a string for content and one button.</summary>
    private async Task OnShowSimpleDialogAsync()
    {
        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Information";
            dialog.Content = "This is a simple message dialog. Click OK to close it.";
            dialog.IconData = Glyph(PhosphorIcon.Info);
            dialog.PrimaryButtonText = "OK";
        });

        LastDialogResult = $"Simple dialog result: {result}";
    }

    /// <summary>Shows a three-button dialog and reports which button closed it.</summary>
    private async Task OnShowComplexDialogAsync()
    {
        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Confirm Action";
            dialog.IconData = Glyph(PhosphorIcon.Warning);

            // The one dialog that overrides the icon's colour. Safe to bind on a shared host: the
            // service's reset calls ClearValue on this property before every dialog.
            dialog[!ContentDialog.IconBrushProperty] = new DynamicResourceExtension("EnigmaWarningBrush");
            dialog.Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    ThemedText(
                        "Are you sure you want to perform this action?",
                        "EnigmaForegroundBrush"),
                    ThemedText(
                        "Each button returns its own DialogResult, so the caller reads the decision "
                        + "from the awaited value.",
                        "EnigmaForegroundSecondaryBrush"),
                },
            };
            dialog.PrimaryButtonText = "Confirm";
            dialog.SecondaryButtonText = "Maybe Later";
            dialog.CloseButtonText = "Cancel";

            // The only dialog here that nominates a default: Confirm gets the accent treatment and
            // the initial focus, so Enter commits the affirmative action.
            dialog.DefaultButton = DefaultButton.Primary;
        });

        LastDialogResult = $"Complex dialog result: {result}";
    }

    /// <summary>Shows a dialog that collects input, and reads the input back after it closes.</summary>
    private async Task OnShowPasswordDialogAsync()
    {
        // Held in a local so the result can be read after the dialog closes: the content is a live
        // control, not a snapshot handed to the service.
        var passwordBox = new TextBox
        {
            PasswordChar = '•',
            PlaceholderText = "Enter your password",
            Width = 300,
        };

        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Password Required";
            dialog.IconData = Glyph(PhosphorIcon.Lock);
            dialog.Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    ThemedText("Please enter your password:", "EnigmaForegroundBrush"),
                    passwordBox,
                },
            };
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = DefaultButton.Primary;
        });

        LastDialogResult = result == DialogResult.Primary
            ? $"Password entered: {passwordBox.Text}"
            : "Password dialog cancelled.";
    }

    /// <summary>Shows a dialog whose content needs more width than the 600px default cap.</summary>
    private async Task OnShowWideDialogAsync()
    {
        var columns = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };

        for (var i = 1; i <= 4; i++)
        {
            var column = new Border
            {
                Width = 190,
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = $"Column {i}", FontWeight = FontWeight.SemiBold },
                        ThemedText(
                            "DialogMaxWidth is 900 here, so wide content is no longer clamped at the "
                            + "default 600px.",
                            "EnigmaForegroundSecondaryBrush"),
                    },
                },
            };

            // A step below the card's own EnigmaSurfaceHighBrush, so the columns read as panels
            // inside it in both theme variants.
            column[!Border.BackgroundProperty] = new DynamicResourceExtension("EnigmaSurfaceBrush");
            column[!Border.BorderBrushProperty] = new DynamicResourceExtension("EnigmaBorderBrush");

            columns.Children.Add(column);
        }

        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Wide Dialog";
            dialog.DialogMaxWidth = 900;
            dialog.Content = columns;
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
        });

        LastDialogResult = $"Wide dialog result: {result}";
    }

    /// <summary>Shows a dialog taller than its cap, to demonstrate scrolling inside the card.</summary>
    private async Task OnShowTallDialogAsync()
    {
        var lines = new StackPanel { Spacing = 6 };

        for (var i = 1; i <= 30; i++)
        {
            lines.Children.Add(ThemedText(
                $"Line {i} — tall content scrolls within the card while the title and buttons stay fixed.",
                "EnigmaForegroundBrush"));
        }

        var result = await _dialogService.ShowAsync(dialog =>
        {
            dialog.Title = "Tall Dialog";
            dialog.DialogMaxHeight = 400;
            dialog.Content = lines;
            dialog.PrimaryButtonText = "OK";
            dialog.CloseButtonText = "Cancel";
        });

        LastDialogResult = $"Tall dialog result: {result}";
    }

    /// <summary>Runs a task behind the overlay, switching the card between indeterminate and measured progress.</summary>
    private async Task OnRunSimpleTaskAsync()
    {
        IsBusy = true;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // The card is a showcase control, not a library one: IOverlayService takes any Control, so
            // a consumer's own progress UI drops straight in.
            var card = new ProgressOverlayCard
            {
                Title = "Processing",
                IsIndeterminate = true,
                Message = "Initializing...",
            };

            await _overlayService.ShowAsync(card);
            await Task.Delay(2000);

            // The card is live while the overlay holds it — mutating its properties is the whole
            // point, and no second ShowAsync is needed.
            card.IsIndeterminate = false;
            card.Progress = 0;
            card.Message = "Step 1 of 4...";
            await Task.Delay(1000);

            card.Progress = 25;
            card.Message = "Step 2 of 4...";
            await Task.Delay(1000);

            card.Progress = 50;
            card.IsIndeterminate = true;
            card.Message = "Analyzing results...";
            await Task.Delay(2000);

            card.IsIndeterminate = false;
            card.Progress = 75;
            card.Message = "Step 3 of 4...";
            await Task.Delay(1000);

            card.Progress = 100;
            card.Message = "Step 4 of 4...";
            await Task.Delay(500);

            await _overlayService.HideAsync();
            stopwatch.Stop();
            LastOverlayResult = $"Task completed in {stopwatch.Elapsed.TotalSeconds:F1} seconds";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Runs a multi-step task behind the overlay, replacing the card's content as steps complete.</summary>
    private async Task OnRunComplexTaskAsync()
    {
        IsBusy = true;
        var stopwatch = Stopwatch.StartNew();

        string[] steps =
        [
            "Validating input data",
            "Connecting to service",
            "Processing records",
            "Generating report",
            "Finalizing",
        ];

        try
        {
            var card = new ProgressOverlayCard
            {
                Title = "Complex Operation",
                IsIndeterminate = true,
                Message = "Starting...",
                Content = BuildStepList(steps, -1),
            };

            await _overlayService.ShowAsync(card);

            for (var i = 0; i < steps.Length; i++)
            {
                card.IsIndeterminate = true;
                card.Message = steps[i] + "...";
                card.Content = BuildStepList(steps, i - 1);
                await Task.Delay(1500);

                card.IsIndeterminate = false;
                card.Progress = (double)(i + 1) / steps.Length * 100;
                card.Content = BuildStepList(steps, i);
                await Task.Delay(500);
            }

            card.Message = "Done!";
            await Task.Delay(500);

            await _overlayService.HideAsync();
            stopwatch.Stop();
            LastOverlayResult = $"Task completed in {stopwatch.Elapsed.TotalSeconds:F1} seconds";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Shows an informational info bar and waits for it to close.</summary>
    private async Task OnShowInfoAsync()
    {
        await ShowInfoBarAsync(
            "Information",
            "This is an informational message.",
            InfoBarSeverity.Info);
    }

    /// <summary>Shows a success info bar and waits for it to close.</summary>
    private async Task OnShowSuccessAsync()
    {
        await ShowInfoBarAsync(
            "Success",
            "The operation completed successfully.",
            InfoBarSeverity.Success);
    }

    /// <summary>Shows a warning info bar and waits for it to close.</summary>
    private async Task OnShowWarningAsync()
    {
        await ShowInfoBarAsync(
            "Warning",
            "Something might need your attention.",
            InfoBarSeverity.Warning);
    }

    /// <summary>Shows an error info bar and waits for it to close.</summary>
    private async Task OnShowErrorAsync()
    {
        await ShowInfoBarAsync(
            "Error",
            "An error has occurred during the operation.",
            InfoBarSeverity.Error);
    }

    /// <summary>Dismisses the current info bar from code rather than from its close button.</summary>
    private async Task OnCloseInfoBarAsync() => await _infoBarService.HideAsync();

    /// <summary>Shows an info bar of one severity and records that it closed.</summary>
    /// <param name="title">The bar's title.</param>
    /// <param name="message">The bar's message.</param>
    /// <param name="severity">The severity that selects the bar's icon and colours.</param>
    private async Task ShowInfoBarAsync(string title, string message, InfoBarSeverity severity)
    {
        await _infoBarService.ShowAsync(bar =>
        {
            bar.Title = title;
            bar.Message = message;
            bar.Severity = severity;
        });

        LastInfoBarResult = $"{severity} closed";
    }

    /// <summary>Builds the overlay card's step list, ticking off everything completed so far.</summary>
    /// <param name="steps">The step labels.</param>
    /// <param name="completedUpTo">The index of the last completed step; -1 when none are.</param>
    /// <returns>A panel with one line per step.</returns>
    private static StackPanel BuildStepList(string[] steps, int completedUpTo)
    {
        var panel = new StackPanel { Spacing = 4 };

        for (var i = 0; i < steps.Length; i++)
        {
            var isDone = i <= completedUpTo;

            panel.Children.Add(ThemedText(
                (isDone ? "✓ " : "  ") + steps[i],
                isDone ? "EnigmaSuccessBrush" : "EnigmaForegroundSecondaryBrush"));
        }

        return panel;
    }

    /// <summary>Creates a wrapping text block whose foreground follows a theme brush.</summary>
    /// <param name="text">The text to show.</param>
    /// <param name="brushKey">The theme resource key supplying the foreground.</param>
    /// <returns>The text block, bound to the brush as a dynamic resource.</returns>
    private static TextBlock ThemedText(string text, string brushKey)
    {
        var block = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };

        // A dynamic resource, not a fixed brush: content built in code follows a runtime theme switch
        // exactly like content declared in XAML.
        block[!TextBlock.ForegroundProperty] = new DynamicResourceExtension(brushKey);
        return block;
    }

    /// <summary>Resolves a Phosphor glyph to the geometry a control's icon property takes.</summary>
    /// <param name="icon">The glyph to resolve.</param>
    /// <returns>The glyph's outline as a <see cref="Geometry"/>.</returns>
    private static Geometry Glyph(PhosphorIcon icon) =>
        PhosphorIconSet.Instance.GetGlyph(icon, PhosphorWeight.Regular).ToGeometry();
}
