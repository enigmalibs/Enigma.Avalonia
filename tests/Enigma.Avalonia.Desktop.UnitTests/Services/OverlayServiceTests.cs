using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Enigma.Avalonia.Desktop.Controls;
using Enigma.Avalonia.Desktop.Services;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Covers the overlay service's host contract, its two argument guards, and the content clearing it
/// performs on hide.
/// </summary>
public sealed class OverlayServiceTests
{
    [AvaloniaFact]
    public async Task ShowAsync_BeforeRegisterHost_Throws()
    {
        OverlayService service = new();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowAsync(new TextBlock()));

        Assert.Equal("No Overlay registered. Call RegisterHost first.", exception.Message);
    }

    [AvaloniaFact]
    public async Task HideAsync_BeforeRegisterHost_DoesNothing()
    {
        OverlayService service = new();

        await service.HideAsync();
    }

    /// <summary>
    /// The host check runs before the argument check, so a null control on an unregistered service
    /// still reports the missing host — the more actionable of the two failures.
    /// </summary>
    [AvaloniaFact]
    public async Task ShowAsync_WithNullControlAndNoHost_ReportsTheMissingHostFirst()
    {
        OverlayService service = new();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ShowAsync(null!));
    }

    [AvaloniaFact]
    public async Task ShowAsync_WithNullControl_ThrowsArgumentNullException()
    {
        OverlayService service = new();
        service.RegisterHost(new Overlay());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => service.ShowAsync(null!));

        Assert.Equal("control", exception.ParamName);
    }

    [AvaloniaFact]
    public async Task ShowAsync_SetsTheContentAndOpensTheOverlay()
    {
        Overlay host = new();
        OverlayService service = new();
        service.RegisterHost(host);
        TextBlock content = new() { Text = "Loading…" };

        await service.ShowAsync(content);

        Assert.True(host.IsOpen);
        Assert.Same(content, host.Content);
    }

    /// <summary>Hiding clears the content as well as closing, so the overlay holds no reference to it.</summary>
    [AvaloniaFact]
    public async Task HideAsync_ClosesTheOverlayAndClearsItsContent()
    {
        Overlay host = new();
        OverlayService service = new();
        service.RegisterHost(host);
        await service.ShowAsync(new TextBlock());

        await service.HideAsync();

        Assert.False(host.IsOpen);
        Assert.Null(host.Content);
    }

    [AvaloniaFact]
    public async Task ShowAsync_ReplacesThePreviousContent()
    {
        Overlay host = new();
        OverlayService service = new();
        service.RegisterHost(host);
        await service.ShowAsync(new TextBlock { Text = "First" });

        TextBlock second = new() { Text = "Second" };
        await service.ShowAsync(second);

        Assert.Same(second, host.Content);
        Assert.True(host.IsOpen);
    }

    /// <summary>Registering a second host replaces the first — the service tracks one host, not a stack.</summary>
    [AvaloniaFact]
    public async Task RegisterHost_CalledTwice_TheSecondHostWins()
    {
        Overlay first = new();
        Overlay second = new();
        OverlayService service = new();
        service.RegisterHost(first);
        service.RegisterHost(second);

        await service.ShowAsync(new TextBlock());

        Assert.True(second.IsOpen);
        Assert.False(first.IsOpen);
        Assert.Null(first.Content);
    }
}
