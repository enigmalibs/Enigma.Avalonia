using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Controls;

/// <summary>
/// Covers what paints the dialog card: <c>EnigmaSurfaceHighBrush</c> by default, the darker
/// <c>EnigmaDialogSecondaryBackgroundBrush</c> under the <c>secondary</c> class, and the control's own
/// <see cref="TemplatedControl.Background"/> when a consumer sets one.
/// </summary>
public sealed class ContentDialogBackgroundTests
{
    /// <summary>Builds an open dialog — a closed one is never measured, so never templated.</summary>
    /// <returns>The dialog.</returns>
    private static ContentDialog OpenDialog() => new() { IsOpen = true, Title = "Branches", CloseButtonText = "Close" };

    /// <summary>Finds the card border of a dialog's own template.</summary>
    /// <param name="dialog">The realised dialog.</param>
    /// <returns>The <c>PART_Card</c> border.</returns>
    private static Border Card(ContentDialog dialog) => dialog.GetVisualDescendants()
        .OfType<Border>()
        .Single(child => ReferenceEquals(child.TemplatedParent, dialog) && child.Name == "PART_Card");

    /// <summary>Resolves a theme brush against the active variant.</summary>
    /// <param name="key">The brush key.</param>
    /// <returns>The shared brush instance the theme defines for the key.</returns>
    private static IBrush ThemeBrush(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, null, out var value), $"'{key}' does not resolve.");
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    /// <summary>The default look is unchanged: the card sits on the raised dialog surface.</summary>
    [AvaloniaFact]
    public void ByDefault_TheCardPaintsTheRaisedDialogSurface()
    {
        var dialog = OpenDialog();
        var window = ControlCatalog.Realise(dialog);

        Assert.Same(ThemeBrush("EnigmaSurfaceHighBrush"), Card(dialog).Background);
        window.Close();
    }

    [AvaloniaFact]
    public void TheSecondaryClass_PaintsTheCardWithTheSecondaryBrush()
    {
        var dialog = OpenDialog();
        dialog.Classes.Add("secondary");
        var window = ControlCatalog.Realise(dialog);

        Assert.Same(ThemeBrush("EnigmaDialogSecondaryBackgroundBrush"), Card(dialog).Background);
        window.Close();
    }

    /// <summary>In the Dark variant — the test application's pinned variant — the secondary card is darker.</summary>
    [AvaloniaFact]
    public void TheSecondaryBackground_IsDarkerThanTheDefaultInTheDarkVariant()
    {
        var secondary = Assert.IsAssignableFrom<ISolidColorBrush>(ThemeBrush("EnigmaDialogSecondaryBackgroundBrush")).Color;
        var standard = Assert.IsAssignableFrom<ISolidColorBrush>(ThemeBrush("EnigmaSurfaceHighBrush")).Color;

        Assert.True(
            secondary.R + secondary.G + secondary.B < standard.R + standard.G + standard.B,
            $"{secondary} is not darker than {standard}.");
    }

    /// <summary>The class is a live style: toggling it off at runtime restores the default card.</summary>
    [AvaloniaFact]
    public void RemovingTheSecondaryClass_RestoresTheDefaultCard()
    {
        var dialog = OpenDialog();
        dialog.Classes.Add("secondary");
        var window = ControlCatalog.Realise(dialog);

        dialog.Classes.Remove("secondary");
        Dispatcher.UIThread.RunJobs();

        Assert.Same(ThemeBrush("EnigmaSurfaceHighBrush"), Card(dialog).Background);
        window.Close();
    }

    /// <summary>The card paints the control's <see cref="TemplatedControl.Background"/>, so a consumer's own brush applies.</summary>
    [AvaloniaFact]
    public void AnExplicitBackground_PaintsTheCard()
    {
        var brush = new SolidColorBrush(Colors.DarkSlateGray);
        var dialog = OpenDialog();
        dialog.Background = brush;
        var window = ControlCatalog.Realise(dialog);

        Assert.Same(brush, Card(dialog).Background);
        window.Close();
    }

    /// <summary>
    /// The 1.0 workaround keeps working: redefining <c>EnigmaSurfaceHighBrush</c> in a dialog's own
    /// resources still re-paints that dialog's card, as Enigma.GitClient's tool-dialog host relies on.
    /// </summary>
    [AvaloniaFact]
    public void RedefiningTheSurfaceBrushInTheDialogsResources_StillRepaintsTheCard()
    {
        var brush = new SolidColorBrush(Colors.DarkSlateGray);
        var dialog = OpenDialog();
        dialog.Resources["EnigmaSurfaceHighBrush"] = brush;
        var window = ControlCatalog.Realise(dialog);

        Assert.Same(brush, Card(dialog).Background);
        window.Close();
    }
}
