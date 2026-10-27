using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Xunit;

namespace Enigma.Avalonia.Mobile.UnitTests.Themes;

/// <summary>
/// The package's whole promise: a mobile app that merges this library resolves exactly the colours
/// and brushes a desktop app resolves from <c>Enigma.Avalonia.Desktop</c> — the same keys, the same
/// values in both variants, and brushes that follow a variant switch. Every expectation comes from
/// the canonical source (<see cref="PaletteSource"/>), so a palette edit needs no test edit, and a
/// linked file swapped for a drifting copy fails here.
/// </summary>
/// <remarks>
/// The headless session shares one <see cref="Application"/> across the assembly, so every test that
/// switches the variant restores it in a <c>finally</c>.
/// </remarks>
public sealed class PaletteTests
{
    /// <summary>Gets the two theme variants by name, one per theory case.</summary>
    public static TheoryData<string> VariantNames => ["Dark", "Light"];

    private static ThemeVariant Variant(string name) => name == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;

    /// <summary>Applies a variant and pumps the dispatcher so the resource re-resolution lands.</summary>
    /// <param name="variant">The variant to activate.</param>
    private static void Activate(ThemeVariant variant)
    {
        Application.Current!.RequestedThemeVariant = variant;
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>Resolves a brush key against the application, as a view would.</summary>
    /// <param name="key">The brush key.</param>
    /// <returns>The brush.</returns>
    private static ISolidColorBrush Brush(string key)
    {
        Assert.True(Application.Current!.TryFindResource(key, null, out var value), $"'{key}' does not resolve.");
        return Assert.IsAssignableFrom<ISolidColorBrush>(value);
    }

    /// <summary>Loads one of the mobile assembly's palette dictionaries on its own.</summary>
    /// <param name="file">The file under <c>Themes/</c>.</param>
    /// <returns>The loaded dictionary.</returns>
    private static IResourceDictionary Load(string file)
    {
        Uri uri = new($"avares://Enigma.Avalonia.Mobile/Themes/{file}");
        return new ResourceInclude(uri) { Source = uri }.Loaded;
    }

    /// <summary>
    /// Every colour key resolves, in the variant asked for, to exactly the literal the canonical
    /// <c>Colors.axaml</c> declares. Failures are aggregated so a drift reports every key at once.
    /// </summary>
    /// <param name="variantName">The variant to resolve against.</param>
    [AvaloniaTheory]
    [MemberData(nameof(VariantNames))]
    public void EveryColour_ResolvesToTheCanonicalValue(string variantName)
    {
        var expected = PaletteSource.ColorValues(variantName);
        var application = Application.Current!;

        List<string> wrong = [];
        foreach (var (key, literal) in expected)
        {
            if (!application.TryFindResource(key, Variant(variantName), out var value) || value is not Color colour)
            {
                wrong.Add($"{key}: does not resolve to a Color");
            }
            else if (colour != Color.Parse(literal))
            {
                wrong.Add($"{key}: {colour} instead of {literal}");
            }
        }

        Assert.NotEmpty(expected);
        Assert.Empty(wrong);
    }

    /// <summary>
    /// Every brush key — the <c>Enigma*</c> brushes and the FluentTheme <c>TextBox</c> / <c>ComboBox</c>
    /// overrides alike — resolves to a solid brush painting the canonical value of the colour key it
    /// binds, in both variants. For the override keys this also proves the library's brush wins over
    /// FluentTheme's own.
    /// </summary>
    /// <param name="variantName">The variant to activate.</param>
    [AvaloniaTheory]
    [MemberData(nameof(VariantNames))]
    public void EveryBrush_PaintsTheCanonicalColourOfItsKey(string variantName)
    {
        var values = PaletteSource.ColorValues(variantName);

        try
        {
            Activate(Variant(variantName));

            List<string> wrong = [];
            foreach (var (brushKey, colorKey) in PaletteSource.BrushColorBindings)
            {
                if (!Application.Current!.TryFindResource(brushKey, null, out var value) || value is not ISolidColorBrush brush)
                {
                    wrong.Add($"{brushKey}: does not resolve to a solid brush");
                }
                else if (brush.Color != Color.Parse(values[colorKey]))
                {
                    wrong.Add($"{brushKey} → {colorKey}: {brush.Color} instead of {values[colorKey]}");
                }
            }

            Assert.Equal(PaletteSource.BrushKeys.Count, PaletteSource.BrushColorBindings.Count);
            Assert.Empty(wrong);
        }
        finally
        {
            Activate(ThemeVariant.Dark);
        }
    }

    /// <summary>
    /// The mobile <c>Colors.axaml</c> defines exactly the canonical colour keys in each variant —
    /// nothing missing, nothing extra — and nothing outside its two theme dictionaries.
    /// </summary>
    /// <param name="variantName">The variant to inspect.</param>
    [AvaloniaTheory]
    [MemberData(nameof(VariantNames))]
    public void TheMobileColours_DefineExactlyTheCanonicalKeys(string variantName)
    {
        var colours = Load("Colors.axaml");

        var variant = Assert.IsAssignableFrom<IResourceDictionary>(colours.ThemeDictionaries[Variant(variantName)]);

        Assert.Empty(colours.Keys);
        Assert.Equal(2, colours.ThemeDictionaries.Count);
        Assert.Equal(
            PaletteSource.ColorValues(variantName).Keys.Order(StringComparer.Ordinal),
            variant.Keys.Cast<string>().Order(StringComparer.Ordinal));
    }

    /// <summary>The mobile <c>Brushes.axaml</c> defines exactly the canonical brush keys.</summary>
    [AvaloniaFact]
    public void TheMobileBrushes_DefineExactlyTheCanonicalKeys()
    {
        var brushes = Load("Brushes.axaml");

        Assert.Empty(brushes.ThemeDictionaries);
        Assert.Equal(
            PaletteSource.BrushKeys.Order(StringComparer.Ordinal),
            brushes.Keys.Cast<string>().Order(StringComparer.Ordinal));
    }

    /// <summary>
    /// The brush layer's contract holds on mobile too: one brush instance per key, re-coloured in place
    /// when the variant changes. A brush that froze at its first variant would pass a lookup and still
    /// render the wrong theme.
    /// </summary>
    [AvaloniaFact]
    public void ABrushInstance_IsRecolouredInPlaceWhenTheVariantChanges()
    {
        try
        {
            Activate(ThemeVariant.Dark);
            var surface = Brush("EnigmaSurfaceBrush");
            Assert.Equal(Color.Parse(PaletteSource.ColorValues("Dark")["EnigmaSurfaceColor"]), surface.Color);

            Activate(ThemeVariant.Light);

            Assert.Same(surface, Brush("EnigmaSurfaceBrush"));
            Assert.Equal(Color.Parse(PaletteSource.ColorValues("Light")["EnigmaSurfaceColor"]), surface.Color);
        }
        finally
        {
            Activate(ThemeVariant.Dark);
        }
    }

    /// <summary>
    /// Proves the lookups above can fail. Without it, a lookup that returned <see langword="true"/> for
    /// everything would make this class vacuous.
    /// </summary>
    [AvaloniaFact]
    public void TheKeyLookup_ReportsAKeyThatIsNotDefined()
    {
        var application = Application.Current!;

        Assert.False(application.TryFindResource("EnigmaKeyThatDoesNotExistBrush", ThemeVariant.Dark, out _));
        Assert.False(application.TryFindResource("EnigmaKeyThatDoesNotExistBrush", ThemeVariant.Light, out _));
    }
}
