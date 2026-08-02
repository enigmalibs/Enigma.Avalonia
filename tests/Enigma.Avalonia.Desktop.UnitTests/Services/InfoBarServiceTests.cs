using System;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Enigma.Avalonia.Desktop.Services;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Covers the info bar service's host contract, the reset it performs between messages, and the
/// task a shown info bar hands back.
/// </summary>
public sealed class InfoBarServiceTests
{
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
