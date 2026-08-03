using System;
using System.Threading.Tasks;
using Enigma.Avalonia.Desktop.Controls.InfoBar;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines a service for displaying and managing info bars in the application.
/// InfoBar instances must be registered via <see cref="RegisterHost"/> before showing info bars.
/// </summary>
public interface IInfoBarService
{
    /// <summary>
    /// Registers the host InfoBar that will be used to display all info bars.
    /// This must be called once during application initialization before showing any info bars.
    /// </summary>
    /// <param name="infoBar">The InfoBar to register as the host.</param>
    void RegisterHost(InfoBar infoBar);

    /// <summary>
    /// Shows an info bar with optional configuration.
    /// The info bar is reset to default state before the configuration action is applied.
    /// </summary>
    /// <param name="configure">An optional action that configures the InfoBar properties before showing it.</param>
    /// <returns>A task that represents the asynchronous show operation.</returns>
    Task ShowAsync(Action<InfoBar>? configure = null);

    /// <summary>
    /// Hides the currently displayed info bar, if any.
    /// </summary>
    /// <returns>A task that represents the asynchronous hide operation.</returns>
    Task HideAsync();
}
