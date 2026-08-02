using System;
using System.Threading.Tasks;
using Enigma.Avalonia.Desktop.Controls.InfoBar;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Implementation of the info bar service for displaying notification messages in the application.
/// Manages a single InfoBar host and resets it between uses.
/// </summary>
public class InfoBarService : IInfoBarService
{
    /// <summary>
    /// The registered InfoBar used to display all info bars.
    /// </summary>
    private InfoBar? _host;

    /// <summary>
    /// Registers the host InfoBar that will be used to display all info bars.
    /// This must be called once during application initialization before showing any info bars.
    /// </summary>
    /// <param name="infoBar">The InfoBar to register as the host.</param>
    public void RegisterHost(InfoBar infoBar)
    {
        _host = infoBar;
    }

    /// <summary>
    /// Shows an info bar with optional configuration.
    /// The info bar is reset to default state before the configuration action is applied.
    /// </summary>
    /// <param name="configure">An optional action that configures the InfoBar properties before showing it.</param>
    /// <returns>A task that represents the asynchronous show operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no InfoBar host has been registered.</exception>
    public async Task ShowAsync(Action<InfoBar>? configure = null)
    {
        if (_host is null)
            throw new InvalidOperationException("InfoBar host has not been registered. Call RegisterHost first.");

        ResetInfoBar(_host);
        configure?.Invoke(_host);
        await _host.ShowAsync();
    }

    /// <summary>
    /// Hides the currently displayed info bar, if any, by calling its CloseAsync method.
    /// </summary>
    /// <returns>A task that represents the asynchronous hide operation.</returns>
    public async Task HideAsync()
    {
        if (_host is not null)
            await _host.CloseAsync();
    }

    /// <summary>
    /// Resets all properties of the specified InfoBar to their default values.
    /// This ensures each info bar starts with a clean state.
    /// </summary>
    /// <param name="infoBar">The InfoBar to reset.</param>
    private static void ResetInfoBar(InfoBar infoBar)
    {
        infoBar.Title = null;
        infoBar.Message = null;
        infoBar.Severity = InfoBarSeverity.Info;
    }
}
