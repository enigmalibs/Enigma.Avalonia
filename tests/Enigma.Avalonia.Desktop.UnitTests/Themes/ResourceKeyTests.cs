using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Enigma.Avalonia.Desktop.Controls;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Themes;

/// <summary>
/// The rename guard. Every key a control template names must resolve out of the merged
/// <c>Fluent.axaml</c>, in both theme variants — the single most likely fallout of the theme-key
/// rename is a template left pointing at a key the dictionary no longer defines, which Avalonia
/// reports by silently rendering nothing rather than by throwing.
/// </summary>
public sealed class ResourceKeyTests
{
    /// <summary>Gets the distinct keys the control templates reference, one per theory case.</summary>
    public static TheoryData<string> ReferencedKeys
    {
        get
        {
            TheoryData<string> data = [];
            foreach (var key in ThemeSource.KeysReferencedByTemplates) data.Add(key);
            return data;
        }
    }

    /// <summary>Gets the two theme variants by name, one per theory case.</summary>
    public static TheoryData<string> VariantNames => ["Dark", "Light"];

    private static ThemeVariant Variant(string name) => name == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;

    /// <summary>
    /// Every key a control template references resolves, under both variants.
    /// </summary>
    /// <param name="key">The referenced resource key.</param>
    [AvaloniaTheory]
    [MemberData(nameof(ReferencedKeys))]
    public void EveryKeyAControlTemplateReferences_ResolvesInBothVariants(string key)
    {
        var application = Application.Current!;

        Assert.True(application.TryFindResource(key, ThemeVariant.Dark, out var dark), $"'{key}' does not resolve in the Dark variant.");
        Assert.True(application.TryFindResource(key, ThemeVariant.Light, out var light), $"'{key}' does not resolve in the Light variant.");
        Assert.NotNull(dark);
        Assert.NotNull(light);
    }

    /// <summary>
    /// The aggregate form of the test above: it names every unresolved key at once, so a rename that
    /// breaks twenty keys reports twenty, not the first one alphabetically.
    /// </summary>
    [AvaloniaFact]
    public void NoControlTemplate_ReferencesAKeyTheDictionaryDoesNotDefine()
    {
        var application = Application.Current!;

        List<string> missing = [];
        foreach (var file in ThemeSource.TemplateFiles)
        {
            foreach (var key in ThemeSource.ReferencedKeys(file))
            {
                if (!application.TryFindResource(key, ThemeVariant.Dark, out _)
                    || !application.TryFindResource(key, ThemeVariant.Light, out _))
                {
                    missing.Add($"{file} → {key}");
                }
            }
        }

        Assert.Empty(missing);
    }

    /// <summary>
    /// Proves the guard above can actually fail. Without this, a lookup that silently returned
    /// <see langword="true"/> for everything would make the whole class vacuous.
    /// </summary>
    [AvaloniaFact]
    public void TheKeyLookup_ReportsAKeyThatIsNotDefined()
    {
        var application = Application.Current!;

        Assert.False(application.TryFindResource("EnigmaKeyThatDoesNotExistBrush", ThemeVariant.Dark, out _));
        Assert.False(application.TryFindResource("EnigmaKeyThatDoesNotExistBrush", ThemeVariant.Light, out _));
    }

    /// <summary>Every key a template references resolves to a brush — templates only reference brushes.</summary>
    [AvaloniaTheory]
    [MemberData(nameof(ReferencedKeys))]
    public void EveryKeyAControlTemplateReferences_ResolvesToABrush(string key)
    {
        Application.Current!.TryFindResource(key, ThemeVariant.Dark, out var value);

        Assert.IsAssignableFrom<IBrush>(value);
    }

    /// <summary>No template still points at a pre-rename key.</summary>
    [AvaloniaFact]
    public void NoControlTemplate_ReferencesACarbonPrefixedKey()
    {
        var stale = ThemeSource.TemplateFiles
            .SelectMany(file => ThemeSource.ReferencedKeys(file).Select(key => $"{file} → {key}"))
            .Where(entry => entry.Contains("→ Carbon", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(stale);
    }

    /// <summary>No dictionary still defines a pre-rename key.</summary>
    [AvaloniaFact]
    public void NoThemeDictionary_DefinesACarbonPrefixedKey()
    {
        List<string> stale = [];
        stale.AddRange(ThemeSource.BrushKeys.Where(key => key.StartsWith("Carbon", StringComparison.Ordinal)));
        stale.AddRange(ThemeSource.ColorKeys("Dark").Where(key => key.StartsWith("Carbon", StringComparison.Ordinal)));
        stale.AddRange(ThemeSource.ColorKeys("Light").Where(key => key.StartsWith("Carbon", StringComparison.Ordinal)));

        Assert.Empty(stale);
    }

    /// <summary>
    /// Both variants define the same colour keys. A key present in one and not the other resolves to
    /// nothing after a theme switch, and the brush bound to it silently stops updating.
    /// </summary>
    [AvaloniaFact]
    public void BothThemeVariants_DefineTheSameColorKeys()
    {
        var dark = ThemeSource.ColorKeys("Dark");
        var light = ThemeSource.ColorKeys("Light");

        Assert.Equal(dark.Order(StringComparer.Ordinal), light.Order(StringComparer.Ordinal));
        Assert.Equal(dark.Count, dark.Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>Every colour a brush binds to is defined in both variants, and resolves in both.</summary>
    /// <param name="variantName">The variant to resolve against.</param>
    [AvaloniaTheory]
    [MemberData(nameof(VariantNames))]
    public void EveryColorABrushBindsTo_ResolvesInThatVariant(string variantName)
    {
        var application = Application.Current!;
        var variant = Variant(variantName);

        var unresolved = ThemeSource.ReferencedKeys("Brushes.axaml")
            .Where(key => !application.TryFindResource(key, variant, out _))
            .ToArray();

        Assert.Empty(unresolved);
    }

    /// <summary>
    /// <c>Fluent.axaml</c> merges every control-template dictionary the library ships. A template
    /// file that exists but is never merged compiles, ships, and applies to nothing.
    /// </summary>
    [AvaloniaFact]
    public void FluentAxaml_MergesEveryControlTemplateTheLibraryShips()
    {
        var shipped = ThemeSource.AllFiles
            .Where(path => path.StartsWith("Controls/", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);

        Assert.Equal(shipped, ThemeSource.TemplateFiles.Order(StringComparer.Ordinal));
    }

    /// <summary>The two foundation dictionaries are merged first, and in the order the templates need.</summary>
    [AvaloniaFact]
    public void FluentAxaml_MergesColorsThenBrushesBeforeAnyTemplate()
    {
        var merged = ThemeSource.MergedFiles;

        Assert.Equal("Colors.axaml", merged[0]);
        Assert.Equal("Brushes.axaml", merged[1]);
        Assert.All(merged.Skip(2), path => Assert.StartsWith("Controls/", path, StringComparison.Ordinal));
    }

    /// <summary>Each template file declares exactly one <c>ControlTheme</c>, keyed on its own control.</summary>
    [AvaloniaFact]
    public void EveryControlTemplateFile_DeclaresExactlyOneControlTheme()
    {
        var targets = ThemeSource.ControlThemeTargets();

        Assert.Equal(ThemeSource.TemplateFiles.Count, targets.Count);
        Assert.Equal(targets.Distinct(StringComparer.Ordinal).Count(), targets.Count);
    }

    /// <summary>
    /// Every <c>ControlTheme</c> is keyed by <c>{x:Type}</c> on a type this library actually ships,
    /// and every shipped templated control has one. A theme keyed on a type that no longer exists is
    /// dead weight; a control with no theme renders blank.
    /// </summary>
    [AvaloniaFact]
    public void EveryControlThemeTarget_IsAShippedControl()
    {
        var shipped = typeof(Overlay).Assembly
            .GetExportedTypes()
            .Where(type => typeof(Control).IsAssignableFrom(type))
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(ThemeSource.ControlThemeTargets(), target => Assert.Contains(target, shipped));
    }
}
