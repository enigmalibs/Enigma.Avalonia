using System.Threading.Tasks;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines async lifecycle methods for navigation with cancellation support.
/// Can be implemented by either page views (Controls) or their ViewModels.
/// Takes precedence over INavigationLifecycle when both are implemented.
/// </summary>
public interface INavigationViewModel
{
    /// <summary>
    /// Called when the page is about to disappear from view.
    /// Return false to cancel the navigation.
    /// This is invoked BEFORE the CurrentPage property changes.
    /// </summary>
    /// <returns>True to allow navigation, false to cancel it</returns>
    Task<bool> OnDisappearingAsync();

    /// <summary>
    /// Called when the page has appeared and is now visible.
    /// This is invoked AFTER the CurrentPage property changes.
    /// </summary>
    Task OnAppearingAsync(object? parameter = null);
}
