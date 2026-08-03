using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Enigma.Avalonia.Desktop.UnitTests.Themes;
using Xunit;

namespace Enigma.Avalonia.Desktop.UnitTests.Controls;

/// <summary>
/// One headless smoke test per templated control: instantiate it, lay it out, and prove its
/// <c>ControlTheme</c> was found and its template applied. A control whose theme key stopped
/// matching renders as an empty box rather than throwing, so "nothing threw" is not enough on its
/// own — every case also asserts the template produced a visual tree.
/// </summary>
public sealed class ControlTemplateTests
{
    /// <summary>Gets the templated controls that ship a theme, one per theory case.</summary>
    public static TheoryData<string> TemplatedControls => Names(ControlCatalog.All);

    /// <summary>Gets the concrete editor subclasses, one per theory case.</summary>
    public static TheoryData<string> ConcreteEditors => Names(ControlCatalog.Editors);

    private static TheoryData<string> Names(IReadOnlyList<ControlCase> cases)
    {
        TheoryData<string> data = [];
        foreach (var entry in cases) data.Add(entry.Name);
        return data;
    }

    /// <summary>Every templated control applies its template when realised in a headless window.</summary>
    /// <param name="name">The control's type name.</param>
    [AvaloniaTheory]
    [MemberData(nameof(TemplatedControls))]
    public void EveryTemplatedControl_AppliesItsTemplate(string name)
    {
        var control = ControlCatalog.Case(name).Create();

        var window = ControlCatalog.Realise(control);

        Assert.NotEmpty(control.GetVisualChildren());
        window.Close();
    }

    /// <summary>Every templated control exposes the template parts its theme documents.</summary>
    /// <param name="name">The control's type name.</param>
    [AvaloniaTheory]
    [MemberData(nameof(TemplatedControls))]
    public void EveryTemplatedControl_ExposesItsTemplateParts(string name)
    {
        var entry = ControlCatalog.Case(name);
        var control = entry.Create();

        var window = ControlCatalog.Realise(control);

        var found = ControlCatalog.TemplateParts(control);
        Assert.Equal(entry.Parts.Order(StringComparer.Ordinal), found);
        window.Close();
    }

    /// <summary>
    /// A concrete editor has no theme of its own — it resolves the <c>BaseEditor</c> or
    /// <c>MultiLineTextEditor</c> theme through <c>StyleKeyOverride</c>. Drop that override and the
    /// editor loses its template silently, which is exactly what this asserts against.
    /// </summary>
    /// <param name="name">The editor's type name.</param>
    [AvaloniaTheory]
    [MemberData(nameof(ConcreteEditors))]
    public void EveryConcreteEditor_ResolvesTheSharedEditorTemplate(string name)
    {
        var entry = ControlCatalog.Case(name);
        var control = entry.Create();

        var window = ControlCatalog.Realise(control);

        Assert.NotEmpty(control.GetVisualChildren());
        Assert.Equal(entry.Parts.Order(StringComparer.Ordinal), ControlCatalog.TemplateParts(control));
        window.Close();
    }

    /// <summary>
    /// The catalogue covers every control the theme templates. Without this, adding a control family
    /// and forgetting the test would look like full coverage.
    /// </summary>
    [AvaloniaFact]
    public void EveryShippedControlTheme_HasACatalogueEntry()
    {
        var templated = ThemeSource.ControlThemeTargets().Order(StringComparer.Ordinal);
        var catalogued = ControlCatalog.All.Select(entry => entry.Name).Order(StringComparer.Ordinal);

        Assert.Equal(templated, catalogued);
    }

    /// <summary>
    /// Realising every control in one window at once catches interference the isolated cases cannot —
    /// a duplicate resource key, or a template that only resolves when it is the sole child.
    /// </summary>
    [AvaloniaFact]
    public void EveryTemplatedControl_AppliesItsTemplateWhenAllAreHostedTogether()
    {
        var controls = ControlCatalog.All.Concat(ControlCatalog.Editors)
            .Select(entry => entry.Create())
            .ToArray();
        StackPanel panel = new();
        foreach (var control in controls) panel.Children.Add(control);

        var window = ControlCatalog.Realise(panel);

        Assert.All(controls, control => Assert.NotEmpty(control.GetVisualChildren()));
        window.Close();
    }
}
