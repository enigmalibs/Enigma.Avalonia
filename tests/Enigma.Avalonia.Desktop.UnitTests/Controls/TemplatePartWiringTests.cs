using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Enigma.Avalonia.Desktop.Controls.ContentDialog;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Controls;

/// <summary>
/// The other half of the part contract. <see cref="ControlTemplateTests"/> asserts the template
/// still declares each <c>PART_*</c>; these assert the code-behind still finds them — a control that
/// looks up a part by a name the template no longer uses wires nothing, throws nothing, and simply
/// stops responding.
/// </summary>
public sealed class TemplatePartWiringTests
{
    /// <summary>Raises a click on a named button of a control's own template.</summary>
    /// <param name="control">The templated control.</param>
    /// <param name="part">The button's part name.</param>
    private static void ClickPart(Control control, string part)
    {
        var button = control.GetVisualDescendants().OfType<Button>()
            .Single(child => ReferenceEquals(child.TemplatedParent, control) && child.Name == part);
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    /// <summary><c>PART_CloseButton</c> is wired to <see cref="InfoBar.Close"/>.</summary>
    [AvaloniaFact]
    public void TheInfoBarCloseButton_ClosesTheInfoBar()
    {
        InfoBar bar = new() { IsOpen = true, Title = "Saved", Message = "All good" };
        var window = ControlCatalog.Realise(bar);
        var closed = false;
        bar.Closed += (_, _) => closed = true;

        ClickPart(bar, "PART_CloseButton");

        Assert.False(bar.IsOpen);
        Assert.True(closed);
        window.Close();
    }

    /// <summary>Each of the dialog's three buttons is wired to the result it advertises.</summary>
    /// <param name="part">The button part to click.</param>
    /// <param name="expected">The result that click must produce.</param>
    [AvaloniaTheory]
    [InlineData("PART_PrimaryButton", DialogResult.Primary)]
    [InlineData("PART_SecondaryButton", DialogResult.Secondary)]
    [InlineData("PART_CloseButton", DialogResult.Close)]
    public async Task EachContentDialogButton_ClosesTheDialogWithItsResult(string part, DialogResult expected)
    {
        ContentDialog dialog = new()
        {
            Title = "Delete?",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Keep",
            CloseButtonText = "Cancel",
        };
        var window = ControlCatalog.Realise(dialog);
        var pending = dialog.ShowAsync();
        Dispatcher.UIThread.RunJobs();

        ClickPart(dialog, part);

        Assert.Equal(expected, await pending);
        Assert.False(dialog.IsOpen);
        window.Close();
    }
}
