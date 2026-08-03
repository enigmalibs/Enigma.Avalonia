using CommunityToolkit.Mvvm.ComponentModel;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the page that exists only as a navigation target.</summary>
/// <remarks>
/// Deliberately empty, and deliberately absent from the rail. The navigation page it backs is reached
/// only through <c>INavigationService.NavigateToAsync</c> from the Navigation page, which is how the
/// showcase demonstrates that navigating to a Control the rail does not know about clears the
/// selection instead of leaving a stale item highlighted.
/// </remarks>
public class DummyPageViewModel : ObservableObject
{
}
