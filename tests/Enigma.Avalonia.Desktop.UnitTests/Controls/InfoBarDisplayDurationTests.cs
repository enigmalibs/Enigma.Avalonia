using System;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Controls;

/// <summary>
/// Covers <see cref="InfoBar.DisplayDuration"/>: an info bar never closes on its own by default, closes
/// itself once a duration elapses, and a countdown never outlives the opening it belongs to.
/// </summary>
/// <remarks>
/// The countdown is a real dispatcher timer, so these tests wait in real time. Durations are short, and
/// every "still open" check waits well past the deadline it is testing against.
/// </remarks>
public sealed class InfoBarDisplayDurationTests
{
    /// <summary>A duration short enough to elapse quickly inside a test.</summary>
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(50);

    /// <summary>A duration no test waits for.</summary>
    private static readonly TimeSpan Long = TimeSpan.FromMinutes(1);

    /// <summary>How long a "does not close" test waits — several times <see cref="Short"/>.</summary>
    private static readonly TimeSpan PastTheDeadline = TimeSpan.FromMilliseconds(400);

    [AvaloniaFact]
    public void DisplayDuration_DefaultsToNull()
    {
        Assert.Null(new InfoBar().DisplayDuration);
    }

    /// <summary>Today's behaviour is the default: without a duration the bar waits to be dismissed.</summary>
    [AvaloniaFact]
    public async Task WithoutADuration_TheInfoBarStaysOpen()
    {
        InfoBar bar = new();

        Task pending = bar.ShowAsync();
        await Task.Delay(PastTheDeadline, TestContext.Current.CancellationToken);

        Assert.True(bar.IsOpen);
        Assert.False(pending.IsCompleted);

        bar.Close();
        await pending;
    }

    [AvaloniaFact]
    public async Task WithADuration_TheInfoBarClosesItselfAndCompletesShowAsync()
    {
        InfoBar bar = new() { DisplayDuration = Short };
        var closedCount = 0;
        bar.Closed += (_, _) => closedCount++;

        Task pending = bar.ShowAsync();
        Assert.True(bar.IsOpen);

        await TaskAssert.CompletesAsync(pending);
        Assert.False(bar.IsOpen);
        Assert.Equal(1, closedCount);
    }

    /// <summary>Setting <see cref="InfoBar.IsOpen"/> directly starts the countdown too, not only <see cref="InfoBar.ShowAsync"/>.</summary>
    [AvaloniaFact]
    public async Task OpeningThroughIsOpen_StartsTheCountdown()
    {
        InfoBar bar = new() { DisplayDuration = Short };
        TaskCompletionSource closed = new();
        bar.Closed += (_, _) => closed.TrySetResult();

        bar.IsOpen = true;

        await TaskAssert.CompletesAsync(closed.Task);
        Assert.False(bar.IsOpen);
    }

    /// <summary>
    /// A bar dismissed before its deadline raises <see cref="InfoBar.Closed"/> once, not a second time when
    /// the abandoned countdown would have fired — the countdown is cancelled, not left running.
    /// </summary>
    [AvaloniaFact]
    public async Task ClosingEarly_CancelsTheCountdown()
    {
        InfoBar bar = new() { DisplayDuration = Short };
        var closedCount = 0;
        bar.Closed += (_, _) => closedCount++;
        Task pending = bar.ShowAsync();

        bar.Close();
        await pending;
        await Task.Delay(PastTheDeadline, TestContext.Current.CancellationToken);

        Assert.Equal(1, closedCount);
        Assert.False(bar.IsOpen);
    }

    [AvaloniaFact]
    public async Task ShorteningTheDurationWhileOpen_RestartsTheCountdownWithTheNewValue()
    {
        InfoBar bar = new() { DisplayDuration = Long };
        Task pending = bar.ShowAsync();

        bar.DisplayDuration = Short;

        await TaskAssert.CompletesAsync(pending);
        Assert.False(bar.IsOpen);
    }

    [AvaloniaFact]
    public async Task ClearingTheDurationWhileOpen_StopsTheCountdown()
    {
        InfoBar bar = new() { DisplayDuration = Short };
        Task pending = bar.ShowAsync();

        bar.DisplayDuration = null;
        await Task.Delay(PastTheDeadline, TestContext.Current.CancellationToken);

        Assert.True(bar.IsOpen);
        Assert.False(pending.IsCompleted);

        bar.Close();
        await pending;
    }

    /// <summary>
    /// A second <see cref="InfoBar.ShowAsync"/> on an open bar restarts the countdown, so the new message
    /// gets its whole duration rather than whatever was left of the previous one's.
    /// </summary>
    [AvaloniaFact]
    public async Task ShowAsyncOnAnOpenInfoBar_RestartsTheCountdown()
    {
        var duration = TimeSpan.FromMilliseconds(1000);
        var wait = TimeSpan.FromMilliseconds(600);
        InfoBar bar = new() { DisplayDuration = duration };
        Task first = bar.ShowAsync();
        await Task.Delay(wait, TestContext.Current.CancellationToken);
        Assert.True(bar.IsOpen);

        Task second = bar.ShowAsync();
        await Task.Delay(wait, TestContext.Current.CancellationToken);

        // 1.2 s after the first opening, past the first deadline but not the restarted one.
        Assert.True(bar.IsOpen);

        await TaskAssert.CompletesAsync(second);
        await first;
        Assert.False(bar.IsOpen);
    }

    /// <summary>Only <see langword="null"/> and strictly positive values up to the timer's ceiling are accepted.</summary>
    /// <param name="milliseconds">The candidate duration, in milliseconds.</param>
    [AvaloniaTheory]
    [InlineData(1d)]
    [InlineData(5000d)]
    [InlineData(2147483647d)]
    public void DisplayDuration_AcceptsPositiveValuesUpToInt32MaxValueMilliseconds(double milliseconds)
    {
        var duration = TimeSpan.FromMilliseconds(milliseconds);

        InfoBar bar = new() { DisplayDuration = duration };

        Assert.Equal(duration, bar.DisplayDuration);
    }

    [AvaloniaFact]
    public void DisplayDuration_AcceptsNull()
    {
        InfoBar bar = new() { DisplayDuration = Short };

        bar.DisplayDuration = null;

        Assert.Null(bar.DisplayDuration);
    }

    /// <summary>An invalid duration is refused where it is set, not later when a countdown would start.</summary>
    /// <param name="milliseconds">The candidate duration, in milliseconds.</param>
    [AvaloniaTheory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(2147483648d)]
    public void DisplayDuration_RejectsZeroNegativeAndOverlongValues(double milliseconds)
    {
        InfoBar bar = new();

        Assert.ThrowsAny<ArgumentException>(() => bar.DisplayDuration = TimeSpan.FromMilliseconds(milliseconds));
        Assert.Null(bar.DisplayDuration);
    }
}
