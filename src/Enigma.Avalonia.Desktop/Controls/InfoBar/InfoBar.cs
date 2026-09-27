using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Enigma.Avalonia.Desktop.Controls.InfoBar;

/// <summary>
/// An inline notification control that displays a title, message, and severity indicator.
/// Use <see cref="ShowAsync"/> to display it and await its dismissal.
/// </summary>
/// <remarks>
/// An info bar stays open until it is dismissed. Set <see cref="DisplayDuration"/> to have it close
/// itself once that period has elapsed.
/// </remarks>
public class InfoBar : ContentControl
{
    /// <summary>The pending auto-close countdown, disposed to cancel it; <see langword="null"/> when none runs.</summary>
    private IDisposable? _displayCountdown;

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<InfoBar, string?>(nameof(Title));

    /// <summary>Defines the <see cref="Message"/> property.</summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<InfoBar, string?>(nameof(Message));

    /// <summary>Defines the <see cref="Severity"/> property.</summary>
    public static readonly StyledProperty<InfoBarSeverity> SeverityProperty =
        AvaloniaProperty.Register<InfoBar, InfoBarSeverity>(nameof(Severity), InfoBarSeverity.Info);

    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<InfoBar, bool>(nameof(IsOpen), false);

    /// <summary>Defines the <see cref="DisplayDuration"/> property.</summary>
    public static readonly StyledProperty<TimeSpan?> DisplayDurationProperty =
        AvaloniaProperty.Register<InfoBar, TimeSpan?>(nameof(DisplayDuration), validate: IsValidDisplayDuration);

    /// <summary>Gets or sets the title text displayed at the top of the info bar.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the descriptive message displayed in the info bar.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Gets or sets the severity level that controls the info bar's visual styling.</summary>
    public InfoBarSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the info bar is currently visible.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets how long the info bar stays open before it closes itself. Defaults to
    /// <see langword="null"/>: the info bar stays open until it is dismissed.
    /// </summary>
    /// <remarks>
    /// The countdown starts when the info bar opens, restarts when this value changes or
    /// <see cref="ShowAsync"/> is called on an open info bar, and is cancelled when the info bar
    /// closes by any other means. When it elapses the info bar closes exactly as <see cref="Close"/>
    /// would: <see cref="IsOpen"/> becomes <see langword="false"/> and <see cref="Closed"/> is raised.
    /// A value must be <see langword="null"/>, or greater than <see cref="TimeSpan.Zero"/> and at most
    /// <see cref="int.MaxValue"/> milliseconds; any other value throws <see cref="ArgumentException"/>.
    /// </remarks>
    public TimeSpan? DisplayDuration
    {
        get => GetValue(DisplayDurationProperty);
        set => SetValue(DisplayDurationProperty, value);
    }

    /// <summary>
    /// Raised when the info bar is dismissed via its close button or <see cref="Close"/>, or closes itself
    /// once its <see cref="DisplayDuration"/> has elapsed.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>Determines whether a value is a valid <see cref="DisplayDuration"/>.</summary>
    /// <param name="value">The candidate value.</param>
    /// <returns>
    /// <see langword="true"/> for <see langword="null"/>, or for a value greater than
    /// <see cref="TimeSpan.Zero"/> and at most <see cref="int.MaxValue"/> milliseconds — the longest
    /// interval a <see cref="DispatcherTimer"/> accepts.
    /// </returns>
    internal static bool IsValidDisplayDuration(TimeSpan? value) =>
        value is not { } duration || (duration > TimeSpan.Zero && duration.TotalMilliseconds <= int.MaxValue);

    /// <summary>Restarts or stops the auto-close countdown when <see cref="IsOpen"/> or <see cref="DisplayDuration"/> changes.</summary>
    /// <param name="change">The property change data.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty || change.Property == DisplayDurationProperty)
            RestartDisplayCountdown();
    }

    /// <summary>Finds <c>PART_CloseButton</c> and wires the click handler.</summary>
    /// <param name="e">The template applied event data.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (closeButton != null)
            closeButton.Click += OnCloseButtonClick;
    }

    /// <summary>Handles the close button click by calling <see cref="Close"/>.</summary>
    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>Hides the info bar and raises the <see cref="Closed"/> event.</summary>
    public void Close()
    {
        IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Closes the info bar and returns a completed task.</summary>
    /// <returns>A completed task.</returns>
    public Task CloseAsync()
    {
        Close();
        return Task.CompletedTask;
    }

    /// <summary>Makes the info bar visible and returns a task that completes when it is dismissed.</summary>
    /// <returns>A task that completes when the info bar is closed.</returns>
    public Task ShowAsync()
    {
        var tcs = new TaskCompletionSource();
        EventHandler? handler = null;

        handler = (s, e) =>
        {
            Closed -= handler;
            tcs.SetResult();
        };

        Closed += handler;

        if (IsOpen)
            RestartDisplayCountdown(); // Already open: the new message gets its full duration.
        else
            IsOpen = true;             // Opening starts the countdown through OnPropertyChanged.

        return tcs.Task;
    }

    /// <summary>
    /// Cancels any running countdown, then starts a new one if the info bar is open and has a
    /// <see cref="DisplayDuration"/>.
    /// </summary>
    private void RestartDisplayCountdown()
    {
        _displayCountdown?.Dispose();
        _displayCountdown = null;

        if (IsOpen && DisplayDuration is { } duration)
            _displayCountdown = DispatcherTimer.RunOnce(OnDisplayDurationElapsed, duration);
    }

    /// <summary>Closes the info bar once its <see cref="DisplayDuration"/> has elapsed.</summary>
    private void OnDisplayDurationElapsed()
    {
        _displayCountdown = null;
        Close();
    }
}
