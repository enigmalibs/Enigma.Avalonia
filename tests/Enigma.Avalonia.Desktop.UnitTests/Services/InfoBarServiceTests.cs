using System;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Enigma.Avalonia.Desktop.Services;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Covers the info bar service's host contract, the reset it performs between messages, the task a
/// shown info bar hands back, and the timed <c>ShowAsync(TimeSpan, …)</c> extension.
/// </summary>
public sealed class InfoBarServiceTests
{
    /// <summary>A display duration short enough to elapse quickly inside a test.</summary>
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(50);

    /// <summary>How long a "does not close" test waits — several times <see cref="Short"/>.</summary>
    private static readonly TimeSpan PastTheDeadline = TimeSpan.FromMilliseconds(400);

    [AvaloniaFact]
    public async Task ShowAsync_BeforeRegisterHost_Throws()
    {
        InfoBarService service = new();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ShowAsync());

        Assert.Equal(
            "InfoBar host has not been registered. Call RegisterHost first.",
            exception.Message);
    }

    [AvaloniaFact]
    public async Task HideAsync_BeforeRegisterHost_DoesNothing()
    {
        InfoBarService service = new();

        await service.HideAsync();
    }

    [AvaloniaFact]
    public async Task ShowAsync_AppliesTheConfigurationAndOpensTheHost()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);

        Task pending = service.ShowAsync(bar =>
        {
            bar.Title = "Saved";
            bar.Message = "Your changes are stored.";
            bar.Severity = InfoBarSeverity.Success;
        });

        Assert.False(pending.IsCompleted);
        Assert.True(host.IsOpen);
        Assert.Equal("Saved", host.Title);
        Assert.Equal("Your changes are stored.", host.Message);
        Assert.Equal(InfoBarSeverity.Success, host.Severity);

        await service.HideAsync();
        await pending;
    }

    /// <summary>Showing without a configuration action is legal and opens a bare info bar.</summary>
    [AvaloniaFact]
    public async Task ShowAsync_WithoutAConfigurationAction_OpensTheHostAtItsDefaults()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);

        Task pending = service.ShowAsync();

        Assert.True(host.IsOpen);
        Assert.Null(host.Title);
        Assert.Null(host.Message);
        Assert.Equal(InfoBarSeverity.Info, host.Severity);

        await service.HideAsync();
        await pending;
    }

    /// <summary>
    /// Every message starts from a clean host, severity included — a previous <c>Error</c> must not
    /// tint the next informational message.
    /// </summary>
    [AvaloniaFact]
    public async Task ShowAsync_ResetsTheHostBetweenMessages()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);
        Task first = service.ShowAsync(bar =>
        {
            bar.Title = "Failed";
            bar.Message = "Something went wrong.";
            bar.Severity = InfoBarSeverity.Error;
        });
        await service.HideAsync();
        await first;

        Task second = service.ShowAsync();

        Assert.Null(host.Title);
        Assert.Null(host.Message);
        Assert.Equal(InfoBarSeverity.Info, host.Severity);

        await service.HideAsync();
        await second;
    }

    [AvaloniaFact]
    public async Task HideAsync_ClosesTheHostAndCompletesThePendingTask()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);
        Task pending = service.ShowAsync(bar => bar.Title = "Saved");

        await service.HideAsync();

        await pending;
        Assert.False(host.IsOpen);
    }

    [AvaloniaFact]
    public async Task ShowAsync_WithADuration_OpensATimedBarThatClosesItself()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);

        Task pending = service.ShowAsync(Short, bar => bar.Title = "Saved");

        Assert.True(host.IsOpen);
        Assert.Equal("Saved", host.Title);
        Assert.Equal(Short, host.DisplayDuration);

        await TaskAssert.CompletesAsync(pending);
        Assert.False(host.IsOpen);
    }

    /// <summary>The duration argument is applied after the configuration action, so it wins.</summary>
    [AvaloniaFact]
    public async Task ShowAsync_WithADuration_TheArgumentWinsOverTheConfigurationAction()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);

        Task pending = service.ShowAsync(Short, bar => bar.DisplayDuration = TimeSpan.FromMinutes(1));

        Assert.Equal(Short, host.DisplayDuration);

        await TaskAssert.CompletesAsync(pending);
    }

    /// <summary>
    /// An invalid duration is refused before the host is touched: no reset, no configuration, no opening.
    /// </summary>
    /// <param name="milliseconds">The invalid duration, in milliseconds.</param>
    [AvaloniaTheory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(2147483648d)]
    public async Task ShowAsync_WithAnInvalidDuration_ThrowsAndLeavesTheHostAlone(double milliseconds)
    {
        InfoBar host = new() { Title = "Previous" };
        InfoBarService service = new();
        service.RegisterHost(host);
        var configured = false;

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ShowAsync(TimeSpan.FromMilliseconds(milliseconds), _ => configured = true));

        Assert.Equal("displayDuration", exception.ParamName);
        Assert.False(configured);
        Assert.False(host.IsOpen);
        Assert.Equal("Previous", host.Title);
    }

    /// <summary>A timed message does not make the next one timed — the duration is part of the reset.</summary>
    [AvaloniaFact]
    public async Task ShowAsync_ResetsTheDisplayDurationBetweenMessages()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);
        Task first = service.ShowAsync(Short);
        await TaskAssert.CompletesAsync(first);

        Task second = service.ShowAsync(bar => bar.Title = "Untimed");

        Assert.Null(host.DisplayDuration);

        await service.HideAsync();
        await second;
    }

    /// <summary>
    /// An untimed message shown over a timed one that is still up inherits none of its countdown: the
    /// reset clears the duration, which stops the countdown before the new message opens.
    /// </summary>
    [AvaloniaFact]
    public async Task AnUntimedMessageShownOverATimedOne_StaysOpen()
    {
        InfoBar host = new();
        InfoBarService service = new();
        service.RegisterHost(host);
        Task timed = service.ShowAsync(Short, bar => bar.Title = "Timed");

        Task untimed = service.ShowAsync(bar => bar.Title = "Untimed");
        await Task.Delay(PastTheDeadline, TestContext.Current.CancellationToken);

        Assert.True(host.IsOpen);
        Assert.Equal("Untimed", host.Title);
        Assert.False(untimed.IsCompleted);

        await service.HideAsync();
        await Task.WhenAll(timed, untimed);
    }

    /// <summary>Registering a second host replaces the first — the service tracks one host, not a stack.</summary>
    [AvaloniaFact]
    public async Task RegisterHost_CalledTwice_TheSecondHostWins()
    {
        InfoBar first = new();
        InfoBar second = new();
        InfoBarService service = new();
        service.RegisterHost(first);
        service.RegisterHost(second);

        Task pending = service.ShowAsync(bar => bar.Title = "Saved");

        Assert.True(second.IsOpen);
        Assert.False(first.IsOpen);
        Assert.Equal("Saved", second.Title);
        Assert.Null(first.Title);

        await service.HideAsync();
        await pending;
    }
}
