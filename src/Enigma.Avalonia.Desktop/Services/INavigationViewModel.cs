using System.Threading.Tasks;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines async lifecycle methods for navigation with cancellation support.
/// Implement it on a page's ViewModel: <see cref="INavigationService"/> inspects the page's
/// <c>DataContext</c> and never the page <c>Control</c> itself, so a Control implementing this
/// interface receives no callbacks. The interface is optional — a page whose <c>DataContext</c> does
/// not implement it navigates with no callbacks at all.
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
