using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Enigma.Avalonia.Desktop.Controls.InfoBar;

/// <summary>
/// An inline notification control that displays a title, message, and severity indicator.
/// Use <see cref="ShowAsync"/> to display it and await its dismissal.
/// </summary>
public class InfoBar : ContentControl
{
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

    /// <summary>Raised when the info bar is dismissed via its close button or <see cref="Close"/>.</summary>
    public event EventHandler? Closed;

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
        IsOpen = true;

        return tcs.Task;
    }
}
