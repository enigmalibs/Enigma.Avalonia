using System.Threading.Tasks;
using Avalonia.Controls;
using Enigma.Avalonia.Desktop.Controls;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines a service for displaying overlay content that appears on top of the main application UI.
/// Overlay instances must be registered via <see cref="RegisterHost"/> before showing overlays.
/// </summary>
public interface IOverlayService
{
    /// <summary>
    /// Registers the host Overlay control that will be used to display all overlay content.
    /// This must be called once during application initialization before showing any overlays.
    /// </summary>
    /// <param name="presenter">The Overlay to register as the host.</param>
    void RegisterHost(Overlay presenter);

    /// <summary>
    /// Shows the overlay with the specified control as content.
    /// The overlay becomes visible and displays the provided control.
    /// </summary>
    /// <param name="control">The control to display in the overlay.</param>
    /// <returns>A task that represents the asynchronous show operation.</returns>
    Task ShowAsync(Control control);

    /// <summary>
    /// Hides the currently displayed overlay and clears its content.
    /// </summary>
    /// <returns>A task that represents the asynchronous hide operation.</returns>
    Task HideAsync();
}
