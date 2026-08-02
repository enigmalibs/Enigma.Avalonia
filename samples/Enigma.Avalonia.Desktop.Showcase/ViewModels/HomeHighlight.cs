using Enigma.Icons.Phosphor;

namespace Enigma.Avalonia.Desktop.Showcase.ViewModels;

/// <summary>One entry in the home page's list of what the library provides.</summary>
/// <param name="Icon">The Phosphor glyph shown beside the entry.</param>
/// <param name="Title">The entry's heading.</param>
/// <param name="Description">A sentence expanding on the heading.</param>
public sealed record HomeHighlight(PhosphorIcon Icon, string Title, string Description);
