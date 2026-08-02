using CommunityToolkit.Mvvm.ComponentModel;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>Backs the base-controls page.</summary>
/// <remarks>
/// Deliberately empty. The page shows stock Avalonia controls with no state of their own — what it
/// demonstrates is that merging the library's theme restyles them, so there is nothing for a
/// ViewModel to hold. It exists because a page is always a View plus a ViewModel resolved from the
/// container, and because the View's <c>x:DataType</c> needs a type to compile bindings against.
/// </remarks>
public class BaseControlsPageViewModel : ObservableObject
{
}
