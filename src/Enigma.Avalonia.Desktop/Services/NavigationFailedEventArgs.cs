using System;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Provides data for the <see cref="INavigationService.NavigationFailed"/> event.
/// </summary>
public class NavigationFailedEventArgs(Exception exception, string phase) : EventArgs
{
    /// <summary>
    /// Gets the exception that caused the navigation failure.
    /// </summary>
    public Exception Exception { get; } = exception;

    /// <summary>
    /// Gets the navigation phase where the failure occurred (e.g., "PageFactory", "OnAppearingAsync", "OnDisappearingAsync").
    /// </summary>
    public string Phase { get; } = phase;
}
