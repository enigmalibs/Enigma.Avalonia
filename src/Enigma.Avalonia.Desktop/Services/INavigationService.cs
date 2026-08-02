using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Enigma.Avalonia.Desktop.Controls.Navigation;

namespace Enigma.Avalonia.Desktop.Services;

/// <summary>
/// Defines a service for managing navigation within the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the currently displayed page Control.
    /// </summary>
    Control? CurrentPage { get; }

    /// <summary>
    /// Navigates to the specified page Control.
    /// </summary>
    /// <param name="page">The page Control to navigate to (with DataContext already set).</param>
    /// <param name="parameter">Optional parameter to pass to the page's <see cref="INavigationViewModel.OnAppearingAsync"/>.</param>
    Task NavigateToAsync(Control page, object? parameter = null);

    /// <summary>
    /// Gets the collection of main navigation items displayed in the primary navigation area.
    /// </summary>
    ObservableCollection<NavigationItem> Items { get; }

    /// <summary>
    /// Gets the collection of footer navigation items displayed at the bottom of the navigation area.
    /// </summary>
    ObservableCollection<NavigationItem> FooterItems { get; }

    /// <summary>
    /// Gets or sets the currently selected navigation item.
    /// Setting this property triggers navigation to the corresponding page.
    /// </summary>
    NavigationItem? SelectedItem { get; set; }

    /// <summary>
    /// Gets or sets the factory function that creates page Control instances from navigation items.
    /// Takes a <see cref="NavigationItem"/> and returns the corresponding page Control with its DataContext set.
    /// </summary>
    Func<NavigationItem, Control> PageFactory { get; set; }

    /// <summary>
    /// Raised when an error occurs during navigation. Subscribers can use this
    /// to log or display navigation failures.
    /// </summary>
    event EventHandler<NavigationFailedEventArgs>? NavigationFailed;
}
