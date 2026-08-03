using Enigma.Icons.Phosphor;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>The content object a docked pane displays.</summary>
/// <remarks>
/// <c>DockPaneModel.Content</c> is typed <c>object?</c>, so a pane holds whatever a consumer puts in
/// it and the <c>DataTemplate</c> on the page decides how it looks — the docking controls never see
/// this type. Giving each pane a distinct icon and title is what makes a drag visible while the pane
/// moves between groups.
/// </remarks>
/// <param name="Icon">The Phosphor glyph shown beside the pane's title.</param>
/// <param name="Title">The pane's heading.</param>
/// <param name="Description">A sentence describing what the pane stands for.</param>
public sealed record PaneContent(PhosphorIcon Icon, string Title, string Description);
