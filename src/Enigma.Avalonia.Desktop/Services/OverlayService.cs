using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Enigma.Avalonia.Desktop.Controls;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Implementation of the overlay service for displaying overlay content on top of the main application UI.
/// Manages a single Overlay host and controls its visibility and content.
/// </summary>
public class OverlayService : IOverlayService
{
    /// <summary>
    /// The registered Overlay control used to display overlay content.
    /// </summary>
    private Overlay? _host;

    /// <summary>
    /// Registers the host Overlay control that will be used to display all overlay content.
    /// This must be called once during application initialization before showing any overlays.
    /// </summary>
    /// <param name="presenter">The Overlay to register as the host.</param>
    public void RegisterHost(Overlay presenter)
    {
        _host = presenter;
    }

    /// <summary>
    /// Shows the overlay with the specified control as content.
    /// Sets the control as the overlay's content and makes the overlay visible.
    /// </summary>
    /// <param name="control">The control to display in the overlay.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no Overlay host has been registered.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the control parameter is null.</exception>
    public Task ShowAsync(Control control)
    {
        if (_host is null)
            throw new InvalidOperationException("No Overlay registered. Call RegisterHost first.");

        if (control is null)
            throw new ArgumentNullException(nameof(control));

        _host.Content = control;
        _host.IsOpen = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Hides the currently displayed overlay and clears its content to prevent memory leaks.
    /// If no host is registered, this method does nothing.
    /// </summary>
    /// <returns>A completed task.</returns>
    public Task HideAsync()
    {
        if (_host is not null)
        {
            _host.IsOpen = false;
            _host.Content = null;  // Clear content to prevent memory leaks
        }
        return Task.CompletedTask;
    }
}
