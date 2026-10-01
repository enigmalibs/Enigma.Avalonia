using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Enigma.Avalonia.Desktop.Controls.InfoBar;
using Enigma.Avalonia.Desktop.UnitTests.Themes;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Controls;

/// <summary>
/// Covers what paints an info bar: each severity's background/border brush pair on the card, the
/// <c>EnigmaInfoBarMessageForegroundBrush</c> on the message line, and the palette rules behind the
/// Dark variant's pastel fills — readable text on every fill, and every fill raised above the surface
/// scale it used to sink into.
/// </summary>
public sealed class InfoBarPaletteTests
{
    /// <summary>WCAG 2's AA minimum contrast ratio for body text.</summary>
    private const double AaContrast = 4.5;

    /// <summary>The message every realised bar shows, so its line can be found by its text.</summary>
    private const string MessageText = "All good";

    /// <summary>Gets each severity with the background and border brush keys its card paints.</summary>
    public static TheoryData<InfoBarSeverity, string, string> SeverityBrushes => new()
    {
        { InfoBarSeverity.Info, "EnigmaInfoBackgroundBrush", "EnigmaInfoBorderBrush" },
        { InfoBarSeverity.Success, "EnigmaSuccessBackgroundBrush", "EnigmaSuccessBorderBrush" },
        { InfoBarSeverity.Warning, "EnigmaWarningBackgroundBrush", "EnigmaWarningBorderBrush" },
        { InfoBarSeverity.Error, "EnigmaErrorBackgroundBrush", "EnigmaErrorBorderBrush" },
    };

    /// <summary>Gets the four severity fill colour keys.</summary>
    public static TheoryData<string> FillKeys =>
    [
        "EnigmaInfoBackgroundColor",
        "EnigmaSuccessBackgroundColor",
        "EnigmaWarningBackgroundColor",
        "EnigmaErrorBackgroundColor",
    ];

    /// <summary>Builds an open bar — a closed one is never measured, so never templated.</summary>
    /// <param name="severity">The severity to show.</param>
    /// <returns>The bar.</returns>
    private static InfoBar OpenBar(InfoBarSeverity severity) =>
        new() { IsOpen = true, Title = "Saved", Message = MessageText, Severity = severity };

    /// <summary>Finds the card border of a bar's own template.</summary>
    /// <param name="bar">The realised bar.</param>
    /// <returns>The <c>PART_Card</c> border.</returns>
    private static Border Card(InfoBar bar) => bar.GetVisualDescendants()
        .OfType<Border>()
        .Single(child => ReferenceEquals(child.TemplatedParent, bar) && child.Name == "PART_Card");

    /// <summary>Resolves a theme brush against the active variant.</summary>
    /// <param name="key">The brush key.</param>
    /// <returns>The shared brush instance the theme defines for the key.</returns>
    private static IBrush ThemeBrush(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, null, out var value), $"'{key}' does not resolve.");
        return Assert.IsAssignableFrom<IBrush>(value);
    }

    /// <summary>Reads a literal colour one variant of <c>Colors.axaml</c> declares.</summary>
    /// <param name="variant">The variant's <c>x:Key</c> — <c>Dark</c> or <c>Light</c>.</param>
    /// <param name="key">The colour key.</param>
    /// <returns>The parsed colour.</returns>
    private static Color Declared(string variant, string key) => Color.Parse(ThemeSource.ColorValues(variant)[key]);

    /// <summary>Computes a colour's WCAG 2 relative luminance.</summary>
    /// <param name="color">An opaque colour.</param>
    /// <returns>The luminance, from 0 (black) to 1 (white).</returns>
    private static double Luminance(Color color)
    {
        static double Linear(byte channel)
        {
            var value = channel / 255.0;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Linear(color.R)) + (0.7152 * Linear(color.G)) + (0.0722 * Linear(color.B));
    }

    /// <summary>Computes the WCAG 2 contrast ratio between two opaque colours.</summary>
    /// <param name="first">One colour.</param>
    /// <param name="second">The other colour.</param>
    /// <returns>The ratio, from 1 to 21.</returns>
    private static double Contrast(Color first, Color second)
    {
        var a = Luminance(first);
        var b = Luminance(second);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    /// <param name="severity">The severity under test.</param>
    /// <param name="backgroundKey">The brush the card must fill with.</param>
    /// <param name="borderKey">The brush the card must outline with.</param>
    [AvaloniaTheory]
    [MemberData(nameof(SeverityBrushes))]
    public void TheCard_PaintsTheSeveritysBrushPair(InfoBarSeverity severity, string backgroundKey, string borderKey)
    {
        var bar = OpenBar(severity);
        var window = ControlCatalog.Realise(bar);

        Assert.Same(ThemeBrush(backgroundKey), Card(bar).Background);
        Assert.Same(ThemeBrush(borderKey), Card(bar).BorderBrush);
        window.Close();
    }

    [AvaloniaFact]
    public void TheMessageLine_PaintsTheInfoBarMessageBrush()
    {
        var bar = OpenBar(InfoBarSeverity.Info);
        var window = ControlCatalog.Realise(bar);

        var message = bar.GetVisualDescendants()
            .OfType<TextBlock>()
            .Single(child => ReferenceEquals(child.TemplatedParent, bar) && child.Text == MessageText);

        Assert.Same(ThemeBrush("EnigmaInfoBarMessageForegroundBrush"), message.Foreground);
        window.Close();
    }

    /// <summary>
    /// The pastel fills stay dark enough for the Dark variant's light text: the primary tone — which
    /// consumer content on these shared brushes uses too — and the info bar's message tone.
    /// </summary>
    /// <param name="fillKey">A severity fill colour key.</param>
    [AvaloniaTheory]
    [MemberData(nameof(FillKeys))]
    public void EveryDarkFill_KeepsAaContrastForPrimaryAndMessageText(string fillKey)
    {
        var fill = Declared("Dark", fillKey);

        var primary = Contrast(fill, Declared("Dark", "EnigmaForegroundColor"));
        var message = Contrast(fill, Declared("Dark", "EnigmaInfoBarMessageForegroundColor"));

        Assert.True(primary >= AaContrast, $"{fillKey}: primary text contrast {primary:F2} is below {AaContrast}.");
        Assert.True(message >= AaContrast, $"{fillKey}: message text contrast {message:F2} is below {AaContrast}.");
    }

    /// <summary>
    /// The pastel fills are raised above the surface scale: each is lighter than the raised surface,
    /// so a card no longer sinks into the panels around it.
    /// </summary>
    /// <param name="fillKey">A severity fill colour key.</param>
    [AvaloniaTheory]
    [MemberData(nameof(FillKeys))]
    public void EveryDarkFill_IsLighterThanTheRaisedSurface(string fillKey)
    {
        var fill = Declared("Dark", fillKey);
        var surface = Declared("Dark", "EnigmaSurfaceHighColor");

        Assert.True(Luminance(fill) > Luminance(surface), $"{fillKey} {fill} is not lighter than {surface}.");
    }

    /// <summary>The Light look is unchanged: its message line keeps the secondary text tone.</summary>
    [AvaloniaFact]
    public void TheLightMessageColour_IsTheSecondaryTextTone()
    {
        Assert.Equal(
            Declared("Light", "EnigmaForegroundSecondaryColor"),
            Declared("Light", "EnigmaInfoBarMessageForegroundColor"));
    }
}
