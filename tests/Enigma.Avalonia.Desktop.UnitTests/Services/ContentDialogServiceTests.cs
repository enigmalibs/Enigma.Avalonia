using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Services;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Services;

/// <summary>
/// Covers the content dialog service's host contract: the guard before registration, the reset it
/// performs between dialogs, and how a shown dialog resolves.
/// </summary>
public sealed class ContentDialogServiceTests
{
    [AvaloniaFact]
    public async Task ShowAsync_BeforeRegisterHost_Throws()
    {
        ContentDialogService service = new();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowAsync(_ => { }));

        Assert.Equal(
            "ContentDialog host has not been registered. Call RegisterHost first.",
            exception.Message);
    }

    [AvaloniaFact]
    public async Task ShowMessageAsync_BeforeRegisterHost_Throws()
    {
        ContentDialogService service = new();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ShowMessageAsync("Title", "Message"));
    }

    /// <summary>
    /// Hiding is forgiving where showing is strict: with no host there is nothing to close, so the
    /// call is a no-op rather than a throw.
    /// </summary>
    [AvaloniaFact]
    public async Task HideAsync_BeforeRegisterHost_DoesNothing()
    {
        ContentDialogService service = new();

        await service.HideAsync();
    }

    [AvaloniaFact]
    public async Task ShowAsync_AfterRegisterHost_OpensTheHostAndStaysPending()
    {
        ContentDialog host = new();
        ContentDialogService service = new();
        service.RegisterHost(host);

        Task<DialogResult> pending = service.ShowAsync(dialog => dialog.Title = "Confirm");

        Assert.False(pending.IsCompleted);
        Assert.True(host.IsOpen);
        Assert.Equal("Confirm", host.Title);

        await service.HideAsync();
        await pending;
    }

    [AvaloniaFact]
    public async Task ShowMessageAsync_PopulatesTheTitleContentAndCloseButton()
    {
        ContentDialog host = new();
        ContentDialogService service = new();
        service.RegisterHost(host);

        Task<DialogResult> pending = service.ShowMessageAsync("Title", "Body text", "Dismiss");

        Assert.Equal("Title", host.Title);
        Assert.Equal("Dismiss", host.CloseButtonText);
        TextBlock content = Assert.IsType<TextBlock>(host.Content);
        Assert.Equal("Body text", content.Text);

        await service.HideAsync();
        await pending;
    }

    [AvaloniaFact]
    public async Task ShowMessageAsync_DefaultsTheCloseButtonToOk()
    {
        ContentDialog host = new();
        ContentDialogService service = new();
        service.RegisterHost(host);

        Task<DialogResult> pending = service.ShowMessageAsync("Title", "Body text");

        Assert.Equal("OK", host.CloseButtonText);

        await service.HideAsync();
        await pending;
    }

    /// <summary>
    /// Every dialog starts from a clean host: what one call configured must not leak into the next.
    /// </summary>
    [AvaloniaFact]
    public async Task ShowAsync_ResetsTheHostBetweenDialogs()
    {
        ContentDialog host = new();
        ContentDialogService service = new();
        service.RegisterHost(host);

        Task<DialogResult> first = service.ShowAsync(dialog =>
        {
            dialog.Title = "First";
            dialog.PrimaryButtonText = "Yes";
            dialog.SecondaryButtonText = "No";
            dialog.IsPrimaryButtonEnabled = false;
            dialog.DefaultButton = DefaultButton.Primary;
        });
        await service.HideAsync();
        await first;

        Task<DialogResult> second = service.ShowAsync(dialog => dialog.Title = "Second");

        Assert.Equal("Second", host.Title);
        Assert.Null(host.PrimaryButtonText);
        Assert.Null(host.SecondaryButtonText);
        Assert.Null(host.CloseButtonText);
        Assert.Null(host.Content);
        Assert.True(host.IsPrimaryButtonEnabled);
        Assert.Equal(DefaultButton.None, host.DefaultButton);

        await service.HideAsync();
        await second;
    }

    /// <summary>Hiding resolves the pending dialog with <see cref="DialogResult.None"/>.</summary>
    [AvaloniaFact]
    public async Task HideAsync_ClosesTheHostAndResolvesWithNone()
    {
        ContentDialog host = new();
        ContentDialogService service = new();
        service.RegisterHost(host);
        Task<DialogResult> pending = service.ShowAsync(dialog => dialog.Title = "Confirm");

        await service.HideAsync();

        DialogResult result = await pending;
        Assert.Equal(DialogResult.None, result);
        Assert.False(host.IsOpen);
    }

    /// <summary>Registering a second host replaces the first — the service tracks one host, not a stack.</summary>
    [AvaloniaFact]
    public async Task RegisterHost_CalledTwice_TheSecondHostWins()
    {
        ContentDialog first = new();
        ContentDialog second = new();
        ContentDialogService service = new();
        service.RegisterHost(first);
        service.RegisterHost(second);

        Task<DialogResult> pending = service.ShowAsync(dialog => dialog.Title = "Confirm");

        Assert.True(second.IsOpen);
        Assert.False(first.IsOpen);
        Assert.Equal("Confirm", second.Title);
        Assert.Null(first.Title);

        await service.HideAsync();
        await pending;
    }
}
