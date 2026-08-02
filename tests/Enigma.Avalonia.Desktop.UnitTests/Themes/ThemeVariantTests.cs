using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Enigma.Avalonia.Desktop.Controls;
using Enigma.Avalonia.Desktop.UnitTests.Controls;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Themes;

/// <summary>
/// Runtime theme switching, which is the claim the whole brush layer exists to support:
/// <c>Brushes.axaml</c> is deliberately not theme-scoped, so one brush instance per key tracks the
/// active variant through <c>DynamicResource</c>. If the rename had broken that indirection, every
/// brush would freeze at whichever variant loaded first — and nothing would throw.
/// </summary>
/// <remarks>
/// The headless session shares one <see cref="Application"/> across the assembly, so every test here
/// restores <see cref="Application.RequestedThemeVariant"/> in a <c>finally</c>.
/// </remarks>
public sealed class ThemeVariantTests
{
    /// <summary>Applies a variant and pumps the dispatcher so the resource re-resolution lands.</summary>
    /// <param name="variant">The variant to activate.</param>
    private static void Activate(ThemeVariant variant)
    {
        Application.Current!.RequestedThemeVariant = variant;
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>Reads a brush's current colour, as resolved against the active variant.</summary>
    /// <param name="key">The brush key.</param>
    /// <returns>The brush's colour.</returns>
    private static Color Sample(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, null, out var value), $"'{key}' does not resolve.");
        return Assert.IsAssignableFrom<ISolidColorBrush>(value).Color;
    }

    /// <summary>
    /// Every brush's colour follows the variant switch exactly as <c>Colors.axaml</c> declares —
    /// brushes whose colour differs between variants change, brushes whose colour is deliberately
    /// identical do not. Deriving the expectation from the source is what keeps this honest: a brush
    /// that silently stopped tracking is a failure, and so is one that changes when it should not.
    /// </summary>
    [AvaloniaFact]
    public void EveryBrushColour_TracksTheVariantExactlyAsColorsAxamlDeclares()
    {
        var darkValues = ThemeSource.ColorValues("Dark");
        var lightValues = ThemeSource.ColorValues("Light");

        try
        {
            Activate(ThemeVariant.Dark);
            var darkSamples = ThemeSource.BrushColorBindings.ToDictionary(pair => pair.Key, pair => Sample(pair.Key), StringComparer.Ordinal);
            Activate(ThemeVariant.Light);
            var lightSamples = ThemeSource.BrushColorBindings.ToDictionary(pair => pair.Key, pair => Sample(pair.Key), StringComparer.Ordinal);

            List<string> wrong = [];
            foreach (var (brushKey, colorKey) in ThemeSource.BrushColorBindings)
            {
                var declaredChange = !string.Equals(darkValues[colorKey], lightValues[colorKey], StringComparison.OrdinalIgnoreCase);
                var observedChange = darkSamples[brushKey] != lightSamples[brushKey];
                if (declaredChange != observedChange)
                {
                    wrong.Add($"{brushKey} → {colorKey}: declared change={declaredChange}, observed={observedChange}");
                }
            }

            Assert.Empty(wrong);
            Assert.Contains(ThemeSource.BrushColorBindings.Keys, key => darkSamples[key] != lightSamples[key]);
        }
        finally
        {
            Activate(ThemeVariant.Dark);
        }
    }

    /// <summary>
    /// The documented exception: the accent is the same blue in both variants, by design. Any future
    /// "every key differs per variant" assertion has to exempt it.
    /// </summary>
    [AvaloniaFact]
    public void TheAccentColour_IsIdenticalInBothVariantsByDesign()
    {
        Assert.Equal(
            ThemeSource.ColorValues("Dark")["EnigmaAccentColor"],
            ThemeSource.ColorValues("Light")["EnigmaAccentColor"],
            StringComparer.OrdinalIgnoreCase);

        try
        {
            Activate(ThemeVariant.Dark);
            var dark = Sample("EnigmaAccentBrush");
            Activate(ThemeVariant.Light);

            Assert.Equal(dark, Sample("EnigmaAccentBrush"));
        }
        finally
        {
            Activate(ThemeVariant.Dark);
        }
    }

    /// <summary>
    /// The overlay scrim keeps its premultiplied alpha across the switch — it is the one colour whose
    /// alpha channel carries meaning, and a naive re-theming would flatten it to opaque.
    /// </summary>
    [AvaloniaFact]
    public void TheOverlayScrim_KeepsItsAlphaInBothVariants()
    {
        try
        {
            Activate(ThemeVariant.Dark);
            var dark = Sample("EnigmaOverlayBrush");
            Activate(ThemeVariant.Light);
            var light = Sample("EnigmaOverlayBrush");

            Assert.Equal(0x80, dark.A);
            Assert.Equal(0x40, light.A);
        }
        finally
        {
            Activate(ThemeVariant.Dark);
        }
    }

    /// <summary>
    /// Switching the variant with every control loaded neither throws nor detaches a template — the
    /// specific failure a broken <c>DynamicResource</c> chain would produce.
    /// </summary>
    [AvaloniaFact]
    public void SwitchingTheVariant_WithEveryControlLoaded_KeepsThemTemplated()
    {
        var controls = ControlCatalog.All.Concat(ControlCatalog.Editors)
            .Select(entry => entry.Create())
            .ToArray();
        StackPanel panel = new();
        foreach (var control in controls) panel.Children.Add(control);
        var window = ControlCatalog.Realise(panel);

        try
        {
            Activate(ThemeVariant.Light);
            Activate(ThemeVariant.Dark);
            Activate(ThemeVariant.Light);

            Assert.All(controls, control => Assert.NotEmpty(control.GetVisualChildren()));
        }
        finally
        {
            Activate(ThemeVariant.Dark);
            window.Close();
        }
    }

    /// <summary>
    /// The end-to-end proof: a brush a live control template resolved actually repaints on a switch.
    /// The brush object is the same instance either way — that is the design — so the assertion is on
    /// its colour, and on the instance being shared with the theme's own key.
    /// </summary>
    [AvaloniaFact]
    public void ALiveControlTemplate_RepaintsWhenTheVariantChanges()
    {
        SettingsCard card = new() { Header = "Theme", Description = "Pick one" };
        var window = ControlCatalog.Realise(card);
        var root = (Border)card.GetVisualDescendants().OfType<Control>()
            .Single(child => ReferenceEquals(child.TemplatedParent, card) && child.Name == "PART_Root");

        try
        {
            Activate(ThemeVariant.Dark);
            var background = Assert.IsAssignableFrom<ISolidColorBrush>(root.Background);
            var dark = background.Color;

            Activate(ThemeVariant.Light);

            Assert.Same(background, root.Background);
            Assert.NotEqual(dark, background.Color);
            Assert.Equal(Sample("EnigmaSurfaceBrush"), background.Color);
        }
        finally
        {
            Activate(ThemeVariant.Dark);
            window.Close();
        }
    }

    /// <summary>
    /// The FluentTheme override keys track the palette too, so the framework's own <c>TextBox</c> and
    /// <c>ComboBox</c> stay in the same skin as this library's controls.
    /// </summary>
    /// <param name="key">A FluentTheme brush key this library overrides.</param>
    [AvaloniaTheory]
    [InlineData("TextControlBackground")]
    [InlineData("ComboBoxBackground")]
    public void TheFluentThemeOverrides_TrackThePalette(string key)
    {
        var colorKey = ThemeSource.BrushColorBindings[key];
        Assert.Equal("EnigmaInputBackgroundColor", colorKey);

        try
        {
            Activate(ThemeVariant.Dark);
            var dark = Sample(key);
            Activate(ThemeVariant.Light);
            var light = Sample(key);

            Assert.NotEqual(dark, light);
            Assert.Equal(Color.Parse(ThemeSource.ColorValues("Dark")[colorKey]), dark);
            Assert.Equal(Color.Parse(ThemeSource.ColorValues("Light")[colorKey]), light);
        }
        finally
        {
            Activate(ThemeVariant.Dark);
        }
    }
}
